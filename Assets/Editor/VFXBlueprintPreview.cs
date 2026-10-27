using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/*
    Renders a contact sheet per effect prefab: the effect simulated to a handful of
    points in time, framed by an orthographic camera the same size as the combat
    camera (ortho size 5). Handy for reviewing a whole folder of effects at once
    without pressing play on each one.

    Menu: VFX Blueprints/Render preview sheets  ->  writes PNGs to Temp/VFXPreviews

    Batch mode (Unity must be closed):
      Unity.exe -batchmode -projectPath <project> -executeMethod VFXBlueprintPreview.RenderBatch
                -previewOut <folder> [-previewDir <Assets/...>] [-previewFilter <name part>] -quit
*/
public static class VFXBlueprintPreview
{
    const string DefaultDir = "Assets/Prefabs/VFX/VFX Prefabs - For Luke/Card Effect Blueprints";
    static readonly float[] Times = { 0.1f, 0.3f, 0.5f, 0.8f, 1.2f, 1.8f };
    const int Cell = 360;
    const int Cols = 3;

    [MenuItem("VFX Blueprints/Render preview sheets")]
    public static void RenderMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Render(DefaultDir, Path.GetFullPath("Temp/VFXPreviews"), null);
        EditorUtility.RevealInFinder(Path.GetFullPath("Temp/VFXPreviews"));
    }

    public static void RenderBatch()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        string Arg(string key, string fallback)
        {
            int i = System.Array.IndexOf(args, key);
            return (i >= 0 && i + 1 < args.Length) ? args[i + 1] : fallback;
        }
        // -previewList <file>: one asset path per line, rendered instead of a folder
        string list = Arg("-previewList", null);
        string[] paths = list != null
            ? File.ReadAllLines(list).Select(l => l.Trim()).Where(l => l.Length > 0).ToArray()
            : PrefabsIn(Arg("-previewDir", DefaultDir));
        Render(paths, Arg("-previewOut", Path.GetFullPath("Temp/VFXPreviews")), Arg("-previewFilter", null));
    }

    static string[] PrefabsIn(string dir)
    {
        return AssetDatabase.FindAssets("t:Prefab", new[] { dir })
            .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).ToArray();
    }

    static void Render(string dir, string outDir, string filter)
    {
        Render(PrefabsIn(dir), outDir, filter);
    }

    static void Render(string[] paths, string outDir, string filter)
    {
        Directory.CreateDirectory(outDir);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Camera cam = new GameObject("Preview Camera").AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.27f, 0.25f, 0.30f, 1f);
        RenderTexture rt = new RenderTexture(Cell, Cell, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;

        int rows = Mathf.CeilToInt(Times.Length / (float)Cols);
        foreach (string path in paths)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            if (!string.IsNullOrEmpty(filter) && name.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) < 0) continue;

            GameObject inst = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            inst.transform.position = Vector3.zero;
            ParticleSystem[] all = inst.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem ps in all)
            {
                ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.useAutoRandomSeed = false;
                ps.randomSeed = 1234;
            }
            // simulate from the top-most systems only; withChildren covers the rest
            List<ParticleSystem> roots = all.Where(ps => ps.transform.parent == null ||
                ps.transform.parent.GetComponentInParent<ParticleSystem>(true) == null).ToList();

            Texture2D sheet = new Texture2D(Cell * Cols, Cell * rows, TextureFormat.RGBA32, false);
            Texture2D frame = new Texture2D(Cell, Cell, TextureFormat.RGBA32, false);
            for (int i = 0; i < Times.Length; i++)
            {
                foreach (ParticleSystem ps in roots) ps.Simulate(Times[i], true, true, true);
                cam.Render();
                RenderTexture.active = rt;
                frame.ReadPixels(new Rect(0, 0, Cell, Cell), 0, 0);
                frame.Apply();
                int cx = (i % Cols) * Cell;
                int cy = (rows - 1 - i / Cols) * Cell;
                sheet.SetPixels(cx, cy, Cell, Cell, frame.GetPixels());
            }
            RenderTexture.active = null;
            sheet.Apply();
            File.WriteAllBytes(Path.Combine(outDir, name + ".png"), sheet.EncodeToPNG());
            Object.DestroyImmediate(sheet);
            Object.DestroyImmediate(frame);
            Object.DestroyImmediate(inst);
            Debug.Log("[Preview] rendered " + name);
        }
        cam.targetTexture = null;
        Object.DestroyImmediate(rt);
        Debug.Log("[Preview] DONE -> " + outDir);
    }
}
