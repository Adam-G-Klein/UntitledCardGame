using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/*
    Edit-mode bench for studying and comparing VFX prefabs. Spawns a prefab in a throwaway additive scene
    far from everything else, over the Act 1 board with the combat bloom profile, simulates its particle
    systems to fixed times and saves frames. Optionally renders each particle layer on its own, and writes
    a text breakdown of every layer (material, textures, timing, size, colour, sorting).

    Nothing is saved: the scene is closed without saving and instances are destroyed.
*/
public static class VFXStudyBench
{
    private static readonly Vector3 Origin = new Vector3(5000f, 0f, 0f);
    private const string BoardSprite = "Assets/Sprites/CombatBoards/CombatTable1_ColorCorrection2.png";
    private const string VolumeProfilePath = "Assets/Scenes/Encounters/CombatScene/Global Volume Profile.asset";

    // spawn: offset from screen centre in world units (rats sit near y=-1.34, enemies near y=2).
    // scale: InstantiatePrefab-style scale (root and direct children), 1 for CombatEffectStep spawns.
    public static string Render(string prefabPath, string outDir, float duration, int frames, Vector2 spawn, float scale = 1f,
                                bool layers = true, float orthoSize = 5f, int width = 960, int height = 540, bool dump = true, Vector2[] spawns = null)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) return "missing " + prefabPath;
        Directory.CreateDirectory(outDir);

        Scene active = SceneManager.GetActiveScene();
        Scene bench = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(bench);
        var made = new List<GameObject>();
        RenderTexture rt = null;
        try
        {
            GameObject camGo = new GameObject("BenchCamera"); made.Add(camGo);
            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = orthoSize;
            cam.transform.position = Origin + new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.12f, 0.05f, 0.05f, 1f);
            cam.nearClipPlane = 0.1f; cam.farClipPlane = 100f; cam.allowHDR = true;
            var camData = camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;

            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
            if (profile != null)
            {
                GameObject volGo = new GameObject("BenchVolume"); made.Add(volGo);
                volGo.transform.position = Origin;
                Volume vol = volGo.AddComponent<Volume>(); vol.isGlobal = true; vol.sharedProfile = profile; vol.priority = 1000f;
            }

            Sprite board = AssetDatabase.LoadAllAssetsAtPath(BoardSprite).OfType<Sprite>().FirstOrDefault();
            if (board != null)
            {
                GameObject bg = new GameObject("BenchBoard"); made.Add(bg);
                SpriteRenderer sr = bg.AddComponent<SpriteRenderer>(); sr.sprite = board;
                sr.color = new Color(0.8f, 0.8f, 0.8f, 1f);
                sr.sortingLayerName = "Environment"; sr.sortingOrder = -1000;
                float w = orthoSize * 2f * width / height;
                float s = w / board.bounds.size.x;
                bg.transform.position = Origin + new Vector3(0f, 0f, 5f);
                bg.transform.localScale = new Vector3(s, s, 1f);
            }

            Vector2[] at = spawns != null && spawns.Length > 0 ? spawns : new[] { spawn };
            AreaEffect.PreviewTargets = at.Select(p => Origin + (Vector3)p).ToList();
            AreaEffect.ResetPreview();
            AreaEffect.PreviewScreenLeft = Origin.x - orthoSize * width / (float)height;
            var insts = new List<GameObject>();
            foreach (Vector2 p in at)
            {
                GameObject one = (GameObject)PrefabUtility.InstantiatePrefab(prefab, bench); made.Add(one); insts.Add(one);
                PrefabUtility.UnpackPrefabInstance(one, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                one.transform.position = Origin + (Vector3)p;
                one.transform.rotation = Quaternion.identity;
                if (!Mathf.Approximately(scale, 1f))
                {
                    one.transform.localScale *= scale;
                    foreach (Transform child in one.transform) child.localScale *= scale;
                }
                // edit mode runs no Awake: apply the runtime setup the effect would do when spawned
                foreach (AreaEffect a in one.GetComponentsInChildren<AreaEffect>(true)) a.Setup();
            }
            AreaEffect.PreviewTargets = null; AreaEffect.PreviewScreenLeft = null;
            GameObject inst = insts[0];

            ParticleSystem[] all = insts.SelectMany(g => g.GetComponentsInChildren<ParticleSystem>(true)).ToArray();
            foreach (ParticleSystem ps in all)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.useAutoRandomSeed = false; ps.randomSeed = 1234;
            }
            List<ParticleSystem> roots = all.Where(p => p.transform.parent == null || p.transform.parent.GetComponentInParent<ParticleSystem>() == null).ToList();
            if (duration <= 0f)
            {
                // auto: until the last rendered layer's first particles have lived out (capped)
                float end = 0.3f;
                foreach (ParticleSystem ps in all)
                {
                    var r = ps.GetComponent<ParticleSystemRenderer>();
                    if (r == null || !r.enabled || !ps.gameObject.activeInHierarchy) continue;
                    var m = ps.main; var em = ps.emission;
                    float firstEmit = em.burstCount > 0 ? Enumerable.Range(0, em.burstCount).Min(i => em.GetBurst(i).time) : 0f;
                    end = Mathf.Max(end, m.startDelay.constantMax + firstEmit + Mathf.Min(m.startLifetime.constantMax, 2.5f));
                }
                duration = Mathf.Min(end * 1.05f, 3.5f);
            }

            rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGBHalf) { antiAliasing = 1 };
            cam.targetTexture = rt;
            Texture2D read = new Texture2D(width, height, TextureFormat.RGB24, false);

            float simTime = float.MaxValue;
            System.Action<float, string> shoot = (t, file) =>
            {
                // step forward in small increments (like the game does) so trails and sub-emitters build up
                if (t < simTime) { foreach (ParticleSystem r in roots) r.Simulate(0f, true, true, true); simTime = 0f; }
                while (simTime < t - 0.0001f)
                {
                    float dt = Mathf.Min(1f / 60f, t - simTime);
                    foreach (ParticleSystem r in roots) r.Simulate(dt, true, false, false);
                    simTime += dt;
                }
                cam.Render();
                RenderTexture prev = RenderTexture.active; RenderTexture.active = rt;
                read.ReadPixels(new Rect(0, 0, width, height), 0, 0); read.Apply();
                RenderTexture.active = prev;
                File.WriteAllBytes(file, read.EncodeToPNG());
            };

            float[] times = Enumerable.Range(0, frames).Select(i => duration * (i + 0.5f) / frames).ToArray();
            for (int i = 0; i < frames; i++) shoot(times[i], Path.Combine(outDir, "all_" + i.ToString("D2") + "_" + Mathf.RoundToInt(times[i] * 1000f).ToString("D4") + ".png"));

            var sb = new StringBuilder();
            if (layers)
            {
                var renderers = all.Select(p => p.GetComponent<ParticleSystemRenderer>()).Where(r => r != null && r.enabled && r.gameObject.activeInHierarchy).ToList();
                var state = renderers.ToDictionary(r => r, r => r.enabled);
                int li = 0;
                foreach (ParticleSystemRenderer only in renderers)
                {
                    foreach (var r in renderers) r.enabled = r == only;
                    string safe = new string(only.name.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
                    for (int i = 0; i < frames; i++) shoot(times[i], Path.Combine(outDir, "L" + li.ToString("D2") + "_" + safe + "_" + i.ToString("D2") + ".png"));
                    li++;
                }
                foreach (var kv in state) kv.Key.enabled = kv.Value;
            }
            if (dump) File.WriteAllText(Path.Combine(outDir, "layers.txt"), Describe(inst));
            Object.DestroyImmediate(read);
            return "ok " + duration.ToString("F2") + "s " + frames + " frames" + (layers ? " + layers" : "") + " -> " + outDir;
        }
        finally
        {
            foreach (GameObject g in made) if (g != null) Object.DestroyImmediate(g);
            if (rt != null) { rt.Release(); Object.DestroyImmediate(rt); }
            SceneManager.SetActiveScene(active);
            EditorSceneManager.CloseScene(bench, true);
        }
    }

    public static string Describe(GameObject root)
    {
        var sb = new StringBuilder();
        int li = 0;
        foreach (ParticleSystem ps in root.GetComponentsInChildren<ParticleSystem>(true))
        {
            var r = ps.GetComponent<ParticleSystemRenderer>();
            var m = ps.main; var em = ps.emission;
            bool live = ps.gameObject.activeInHierarchy && r != null && r.enabled;
            sb.Append(live ? "L" + (li++).ToString("D2") : "--").Append(" ").Append(HierarchyPath(ps.transform, root.transform)).Append(live ? "" : "  [not rendered]").Append("\n");
            if (!live) continue;
            Material mat = r.sharedMaterial;
            sb.Append("    mat ").Append(mat ? mat.name : "-").Append(" (").Append(mat ? mat.shader.name : "-").Append(")  mode ").Append(r.renderMode)
              .Append("  sort ").Append(r.sortingLayerName).Append(":").Append(r.sortingOrder).Append("\n");
            if (mat != null)
            {
                sb.Append("    tex");
                foreach (string n in new[] { "_MainTex", "_FadeTexture", "_Fade_Secondary_Tex", "_LERP_Texture", "_Distortion_Tex", "_Mask_Tex", "_OverlayTex" })
                    if (mat.HasProperty(n) && mat.GetTexture(n) != null) sb.Append(" ").Append(n.Trim('_')).Append("=").Append(mat.GetTexture(n).name);
                sb.Append("\n    shade");
                foreach (string n in new[] { "_Color", "_Colour_A", "_Colour_B" })
                    if (mat.HasProperty(n)) sb.Append(" ").Append(n.Trim('_')).Append("=").Append(Hdr(mat.GetColor(n)));
                foreach (string n in new[] { "_Intensity", "_Use_Lerp", "_Use_Cutoff", "_Use_Fade_Tex_Instead", "_Cutoff_Softness", "_Use_Radial", "_Distortion_Strength", "_Use_Overlay_0_No", "_Blend", "_Text_Power" })
                    if (mat.HasProperty(n)) sb.Append(" ").Append(n.Trim('_')).Append("=").Append(mat.GetFloat(n).ToString("G3"));
                sb.Append("\n");
            }
            string bursts = string.Join(" ", Enumerable.Range(0, em.burstCount).Select(i => { var b = em.GetBurst(i); return b.time.ToString("G3") + "s:" + Mc(b.count) + (b.cycleCount > 1 ? "x" + b.cycleCount : ""); }));
            sb.Append("    time delay ").Append(Mc(m.startDelay)).Append(" life ").Append(Mc(m.startLifetime)).Append(" dur ").Append(m.duration.ToString("G3")).Append(m.loop ? " loop" : "")
              .Append("  emit rate ").Append(Mc(em.rateOverTime)).Append(bursts.Length > 0 ? " bursts " + bursts : "").Append(" max ").Append(m.maxParticles).Append("\n");
            sb.Append("    size ").Append(m.startSize3D ? Mc(m.startSizeX) + "x" + Mc(m.startSizeY) : Mc(m.startSize)).Append(" speed ").Append(Mc(m.startSpeed))
              .Append(" rot ").Append(m.startRotation3D ? "3D" : Mc(m.startRotation)).Append(" col ").Append(Mg(m.startColor)).Append(" grav ").Append(Mc(m.gravityModifier))
              .Append(" space ").Append(m.simulationSpace).Append(" pos ").Append(ps.transform.localPosition.ToString("F2")).Append(" scale ").Append(ps.transform.lossyScale.x.ToString("F2")).Append("\n");
            var mods = new List<string>();
            var sh = ps.shape; if (sh.enabled) mods.Add("shape " + sh.shapeType + " r" + sh.radius.ToString("G3") + (sh.arc < 360 ? " arc" + sh.arc : ""));
            if (ps.velocityOverLifetime.enabled) mods.Add("vel");
            if (ps.limitVelocityOverLifetime.enabled) mods.Add("limitVel");
            if (ps.forceOverLifetime.enabled) mods.Add("force");
            if (ps.colorOverLifetime.enabled) mods.Add("colLife" + AlphaKeys(ps.colorOverLifetime.color));
            if (ps.sizeOverLifetime.enabled) mods.Add("sizeLife" + CurveKeys(ps.sizeOverLifetime.separateAxes ? ps.sizeOverLifetime.x : ps.sizeOverLifetime.size));
            if (ps.rotationOverLifetime.enabled) mods.Add("rotLife " + Mc(ps.rotationOverLifetime.z));
            if (ps.noise.enabled) mods.Add("noise " + ps.noise.strength.constant.ToString("G3"));
            if (ps.textureSheetAnimation.enabled) mods.Add("sheet " + ps.textureSheetAnimation.numTilesX + "x" + ps.textureSheetAnimation.numTilesY);
            if (ps.trails.enabled) mods.Add("trails");
            if (ps.subEmitters.enabled) mods.Add("subEmit");
            if (ps.lights.enabled) mods.Add("lights");
            if (ps.customData.enabled) { var c = ps.customData.GetVector(ParticleSystemCustomData.Custom1, 0); mods.Add("dissolve(custom1.x)" + (c.mode == ParticleSystemCurveMode.Curve ? CurveKeys(c) : " " + Mc(c))); }
            sb.Append("    mods ").Append(string.Join(", ", mods)).Append("\n");
        }
        return sb.ToString();
    }

    private static string HierarchyPath(Transform t, Transform root)
    {
        string p = t.name; while (t != root && t.parent != null) { t = t.parent; if (t == root) break; p = t.name + "/" + p; } return p;
    }
    private static string Hdr(Color c) => "(" + c.r.ToString("G2") + "," + c.g.ToString("G2") + "," + c.b.ToString("G2") + "," + c.a.ToString("G2") + ")";
    private static string Mc(ParticleSystem.MinMaxCurve c)
    {
        switch (c.mode)
        {
            case ParticleSystemCurveMode.Constant: return c.constant.ToString("G3");
            case ParticleSystemCurveMode.TwoConstants: return c.constantMin.ToString("G3") + "-" + c.constantMax.ToString("G3");
            default: return "curve*" + c.curveMultiplier.ToString("G3");
        }
    }
    private static string Mg(ParticleSystem.MinMaxGradient g)
    {
        switch (g.mode)
        {
            case ParticleSystemGradientMode.Color: return Hdr(g.color);
            case ParticleSystemGradientMode.TwoColors: return Hdr(g.colorMin) + "/" + Hdr(g.colorMax);
            default: return g.mode.ToString();
        }
    }
    private static string AlphaKeys(ParticleSystem.MinMaxGradient g)
    {
        Gradient gr = g.mode == ParticleSystemGradientMode.TwoGradients ? g.gradientMax : g.gradient;
        if (gr == null) return "";
        return "[a " + string.Join(" ", gr.alphaKeys.Select(k => k.time.ToString("F2") + ":" + k.alpha.ToString("F2"))) + " | c " + string.Join(" ", gr.colorKeys.Select(k => k.time.ToString("F2") + ":" + ColorUtility.ToHtmlStringRGB(k.color))) + "]";
    }
    private static string CurveKeys(ParticleSystem.MinMaxCurve c)
    {
        if (c.mode != ParticleSystemCurveMode.Curve && c.mode != ParticleSystemCurveMode.TwoCurves) return " " + Mc(c);
        AnimationCurve a = c.mode == ParticleSystemCurveMode.TwoCurves ? c.curveMax : c.curve;
        return "[" + string.Join(" ", a.keys.Select(k => k.time.ToString("F2") + ":" + k.value.ToString("F2"))) + " x" + c.curveMultiplier.ToString("G3") + "]";
    }
}
