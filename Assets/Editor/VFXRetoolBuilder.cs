using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/*
    Builds the retooled card VFX ("Card Effects V2") out of the hand-made base effects' own layers, so the
    new effects share their structure: dark backdrop, one hero icon that flips from black ink to colour on
    the impact frame, a short flash (glow, ring, spikes), a few paired-colour flecks, then theme dressing.

    Each layer is copied from a named child of an existing effect (see Templates) and re-themed through the
    particle system's own start colour and timing, never by editing a shared material. Icons and themed
    sprites use CM_ copies of the source material with a placeholder texture from
    Assets/VFX V3/Card Effects V2/Placeholders; drawing over a placeholder PNG updates every effect using it.

    Menu: VFX Retool / Build all.
*/
public static class VFXRetoolBuilder
{
    public const string OutDir = "Assets/Prefabs/VFX/Card Effects V2";
    public const string MatDir = "Assets/VFX V3/Card Effects V2";
    public const string PlaceholderDir = "Assets/VFX V3/Card Effects V2/Placeholders";
    const string Luke = "Assets/Prefabs/VFX/VFX Prefabs - For Luke/";

    // key -> (source effect, child layer)
    static readonly Dictionary<string, (string prefab, string child)> Templates = new Dictionary<string, (string, string)>
    {
        { "Backdrop",   ("Allied Big Hit", "Background Big") },
        { "Glow",       ("Allied Small Hit", "Impact Burst (2)") },
        { "Flash",      ("Allied Big Hit", "Impact Burst (1)") },
        { "Core",       ("Allied Big Hit", "Impact Burst (3)") },
        { "Ring",       ("Allied Big Hit", "Impact Outer Circle") },
        { "Spikes",     ("Allied Big Hit", "Thing Burst") },
        { "SpikesBig",  ("Allied Small Hit", "Thing Burst 2") },
        { "Streaks",    ("Allied Small Hit", "Black Ink Streaks (1)") },
        { "DotsA",      ("Allied Small Hit", "Blue Dots") },
        { "DotsB",      ("Allied Small Hit", "White Dots") },
        { "Icon",       ("Allied Big Hit", "Sword") },
        { "Sheen",      ("Allied Big Hit", "Rainbow Overlay") },
        { "Embers",     ("Card Effect - Pureknife Attack", "Embers (1)") },
        { "Leaves",     ("Card Effect - Doombloom Damage", "Embers (2)") },
        { "Petals",     ("Card Effect - Doombloom Damage", "Embers (3)") },
        { "MagicStar",  ("Card Effect - Doombloom Damage", "Main Burst") },
        { "Anticipate", ("Card Effect - Doombloom Damage", "Anticipation Burst") },
        { "InkSplat",   ("Card Effect - Chunkmungus Impact", "Main Burst") },
        { "InkSub",     ("Card Effect - Chunkmungus Impact", "Sub Burst") },
        { "InkBlops",   ("Card Effect - Chunkmungus Impact", "Ink Blops") },
        { "BlackSplots",("Card Effect - Finishing Touch", "Ink Splots") },
        { "MoneyTrails",("Shop and Money/Generic Money Burst", "Trails (1)") },
        { "MoneyStreaks",("Shop and Money/Generic Money Burst", "Coloured Ink Streaks") },
        { "SlashDark",  ("Card Effect - Finishing Touch", "Dark") },
        { "SlashLight", ("Card Effect - Finishing Touch", "Enemy Background Black (1)") },
        { "StarIcons",  ("Card Effect - Finishing Touch", "Taretting Sparkle") },
        { "Arrows",     ("Allied Buff", "Ink Arrows") },
        { "Arrows2",    ("Allied Buff", "Ink Arrows (1)") },
        { "BuffWave",   ("Allied Buff", "Ink Splots Aura (3)") },
        { "BuffStreaks",("Allied Buff", "Black Ink Streaks (1)") },
        { "Shield",     ("Allied Block", "Shield (1)") },
        { "ShieldStars",("Allied Block", "Star Plots (1)") },
        { "ShieldRise", ("Allied Block", "Black Ink Streaks (2)") },
        { "PulseIn",    ("Allied Charge Up", "Circle Pulse In") },
        { "PulseOut",   ("Allied Charge Up", "Outer Circle Pulse") },
        { "Plus",       ("Heal", "Small Plus") },
        // the status kit (Luke's Status prefabs)
        { "GradPaint",  ("Status Apply Effect Bleed", "Outer Grad Paint") },
        { "Bubbles",    ("Status Effect Poision", "Black Ink Streaks (5)") },
        { "BubblesIn",  ("Status Apply Poison", "Black Ink Streaks (5)") },
        // the enemy kit (Luke's Enemy (Red) claw hit): only enemy effects use these
        { "EGlow",      ("Enemy (Red)/Enemy Claw Big Attack", "Enemy Background Big (1)") },
        { "ESmoke",     ("Enemy (Red)/Enemy Claw Big Attack", "Smoke (1)") },
        { "EStreaks",   ("Enemy (Red)/Enemy Claw Big Attack", "Black Ink Streaks") },
        { "EStreaksRed",("Enemy (Red)/Enemy Claw Big Attack", "Coloured Ink Streaks") },
    };

    static readonly Dictionary<string, GameObject> templateCache = new Dictionary<string, GameObject>();

    static GameObject Template(string key)
    {
        if (templateCache.TryGetValue(key, out GameObject cached) && cached != null) return cached;
        var (prefab, child) = Templates[key];
        GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(Luke + prefab + ".prefab");
        if (root == null) throw new Exception("missing template prefab " + prefab);
        Transform t = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == child);
        if (t == null) throw new Exception("missing template layer " + prefab + "/" + child);
        templateCache[key] = t.gameObject;
        return t.gameObject;
    }

    // ---------- kit ----------

    public class Fx
    {
        public GameObject root;
        public Transform into;
        public Fx(string name, int seconds = 4)
        {
            root = new GameObject(name);
            root.AddComponent<DestroyAfterSeconds>().seconds = seconds;
            into = root.transform;
        }
        // new layers go under this group until the next Group/Root call
        public Transform Group(string name)
        {
            Transform g = new GameObject(name).transform; g.SetParent(root.transform, false); into = g; return g;
        }
        public void Root() { into = root.transform; }
        public Layer Add(string key, string name = null)
        {
            GameObject src = Template(key);
            GameObject go = UnityEngine.Object.Instantiate(src, into);
            go.name = name ?? key;
            go.SetActive(true);
            foreach (ParticleSystemRenderer r in go.GetComponentsInChildren<ParticleSystemRenderer>(true)) r.enabled = r.enabled || r.gameObject == go;
            if (key.StartsWith("Slash"))
            {
                // the slash strokes come from Finishing Touch, where they were pivoted at one end, offset and
                // drifting diagonally: centre them on the target (recipes add their own position or motion after)
                go.GetComponent<ParticleSystemRenderer>().pivot = Vector3.zero;
                go.transform.localPosition = new Vector3(0f, 0f, go.transform.localPosition.z);
                var lim = go.GetComponent<ParticleSystem>().limitVelocityOverLifetime; lim.enabled = false;
                var vel = go.GetComponent<ParticleSystem>().velocityOverLifetime; vel.enabled = false;
            }
            return new Layer(go);
        }
        // names of layers that read as round bursts: these get a full random spin per particle
        static readonly string[] SpinNames = { "Spikes", "Ring", "Glow", "Flash", "Core", "Backdrop", "Dots", "MagicStar", "Star", "Splat", "Splash", "Shockwave", "Pop", "Pin", "Rays", "Sub", "Foam", "Crack", "Muzzle" };

        public void Save(string file) { SaveAt(OutDir + "/" + file + ".prefab"); }

        public void SaveAt(string path)
        {
            // layers sit under plain group objects, so nothing plays them for us: each must start itself
            // (except trail sub-emitters, which their parent layer drives)
            var subTargets = new HashSet<ParticleSystem>();
            foreach (ParticleSystem ps in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                var se = ps.subEmitters;
                for (int i = 0; i < se.subEmittersCount; i++) subTargets.Add(se.GetSubEmitterSystem(i));
            }
            foreach (ParticleSystem ps in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                // sub-emitters share the flag with their parent, so leave them alone; nested prefabs (an enemy's own
                // hit carried inside a sweep) keep their authored settings
                if (PrefabUtility.IsPartOfPrefabInstance(ps)) continue;
                if (!subTargets.Contains(ps)) { var pm = ps.main; pm.playOnAwake = true; }
            }
            // every copy of this effect (e.g. one per enemy) must look a little different
            foreach (ParticleSystem ps in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (PrefabUtility.IsPartOfPrefabInstance(ps)) continue;
                ps.useAutoRandomSeed = true;
                var m = ps.main;
                if (m.startRotation3D || m.startRotation.mode != ParticleSystemCurveMode.Constant) continue;
                var r = ps.GetComponent<ParticleSystemRenderer>();
                if (r != null && r.alignment == ParticleSystemRenderSpace.Local) continue;   // projectiles face their flight
                bool moving = ps.velocityOverLifetime.enabled;
                if (SpinNames.Any(n => ps.name.Contains(n)) && !moving)
                    m.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
                else if (r != null && r.renderMode == ParticleSystemRenderMode.Billboard)
                {
                    // icons and strokes keep their direction but tilt slightly each time
                    float c = m.startRotation.constant;
                    m.startRotation = new ParticleSystem.MinMaxCurve(c - 0.18f, c + 0.18f);
                }
            }
            AddPreviewSystems(root);
            ResolveTintProxies(root);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    public class Layer
    {
        public GameObject go; public ParticleSystem ps;
        public Layer(GameObject g) { go = g; ps = g.GetComponent<ParticleSystem>(); }
        ParticleSystem.MainModule M => ps.main;
        public Layer Col(Color c) { var m = M; m.startColor = c; return this; }
        public Layer Col(Color a, Color b) { var m = M; m.startColor = new ParticleSystem.MinMaxGradient(a, b); return this; }
        public Layer Delay(float add)
        {
            var m = M; var d = m.startDelay;
            m.startDelay = d.mode == ParticleSystemCurveMode.TwoConstants ? new ParticleSystem.MinMaxCurve(d.constantMin + add, d.constantMax + add) : new ParticleSystem.MinMaxCurve(d.constant + add);
            return this;
        }
        public Layer DelayAt(float t) { var m = M; m.startDelay = t; return this; }
        public Layer Life(float a, float b = -1) { var m = M; m.startLifetime = b < 0 ? new ParticleSystem.MinMaxCurve(a) : new ParticleSystem.MinMaxCurve(a, b); return this; }
        public Layer Size(float mul)
        {
            var m = M;
            if (m.startSize3D) { m.startSizeXMultiplier *= mul; m.startSizeYMultiplier *= mul; }
            else m.startSizeMultiplier *= mul;
            return this;
        }
        public Layer Size2(float x, float y) { var m = M; m.startSize3D = true; m.startSizeX = x; m.startSizeY = y; m.startSizeZ = 1f; return this; }
        public Layer Speed(float a, float b) { var m = M; m.startSpeed = new ParticleSystem.MinMaxCurve(a, b); return this; }
        public Layer Rot(float rad) { var m = M; m.startRotation = rad; return this; }
        public Layer Pos(float x, float y) { go.transform.localPosition = new Vector3(x, y, go.transform.localPosition.z); return this; }
        // template layers are often turned to face along a cone; face the camera so shapes spread on screen
        public Layer Flat() { go.transform.localRotation = Quaternion.identity; var s = ps.shape; s.scale = Vector3.one; return this; }
        public Layer Count(int a, int b)
        {
            var e = ps.emission;
            if (e.burstCount > 0) { var bu = e.GetBurst(0); bu.count = new ParticleSystem.MinMaxCurve(a, b); e.SetBurst(0, bu); }
            return this;
        }
        // a single burst (templates often repeat theirs)
        public Layer Once()
        {
            var e = ps.emission;
            if (e.burstCount > 0) { var bu = e.GetBurst(0); bu.cycleCount = 1; e.SetBurst(0, bu); }
            return this;
        }
        public Layer Grav(float g) { var m = M; m.gravityModifier = g; return this; }
        public Layer Sort(int order) { ps.GetComponent<ParticleSystemRenderer>().sortingOrder = order; return this; }
        public Layer Mat(Material mat) { ps.GetComponent<ParticleSystemRenderer>().sharedMaterial = mat; return this; }
        public Layer Still() { var r = ps.rotationOverLifetime; r.enabled = false; var v = ps.velocityOverLifetime; v.enabled = false; return this; }
        public Layer Shape(ParticleSystemShapeType type, float radius, float rotZDeg = 0f, float arc = 360f)
        {
            var s = ps.shape; s.enabled = true; s.shapeType = type; s.radius = radius; s.arc = arc; s.rotation = new Vector3(0f, 0f, rotZDeg);
            return this;
        }
        public Layer Velocity(float x, float y)
        {
            var v = ps.velocityOverLifetime; v.enabled = true; v.space = ParticleSystemSimulationSpace.World;   // template layers are rotated; move in screen space
            v.orbitalX = 0f; v.orbitalY = 0f; v.orbitalZ = 0f; v.radial = 0f; v.speedModifier = 1f; v.orbitalOffsetX = 0f; v.orbitalOffsetY = 0f; v.orbitalOffsetZ = 0f;
            var frc = ps.forceOverLifetime; frc.enabled = false; var nz = ps.noise; nz.enabled = false;
            v.x = new ParticleSystem.MinMaxCurve(x); v.y = new ParticleSystem.MinMaxCurve(y); v.z = new ParticleSystem.MinMaxCurve(0f);
            return this;
        }
        public Layer Spin(float radPerSec) { var r = ps.rotationOverLifetime; r.enabled = true; r.z = radPerSec; return this; }
        public Layer RotRange(float a, float b) { var m = M; m.startRotation = new ParticleSystem.MinMaxCurve(a, b); return this; }
        public Layer NoColLife() { var c = ps.colorOverLifetime; c.enabled = false; return this; }
        public Layer NoSheet() { var t = ps.textureSheetAnimation; t.enabled = false; return this; }
        // moves at (x,y) and eases to a stop by `stopAt` (fraction of lifetime): a thrust, a drop, a slap
        public Layer Lunge(float x, float y, float stopAt = 0.35f)
        {
            var lim = ps.limitVelocityOverLifetime; lim.enabled = false;
            var v = ps.velocityOverLifetime; v.enabled = true; v.space = ParticleSystemSimulationSpace.World;   // template layers are rotated; move in screen space
            v.orbitalX = 0f; v.orbitalY = 0f; v.orbitalZ = 0f; v.radial = 0f; v.speedModifier = 1f; v.orbitalOffsetX = 0f; v.orbitalOffsetY = 0f; v.orbitalOffsetZ = 0f;
            var frc = ps.forceOverLifetime; frc.enabled = false; var nz = ps.noise; nz.enabled = false;
            AnimationCurve k = new AnimationCurve(new Keyframe(0f, 1f, 0f, 0f), new Keyframe(stopAt, 0f, 0f, 0f), new Keyframe(1f, 0f));
            // all three axes must use the same curve mode or Unity ignores the module
            v.x = new ParticleSystem.MinMaxCurve(x, k); v.y = new ParticleSystem.MinMaxCurve(y, k); v.z = new ParticleSystem.MinMaxCurve(0f, k);
            return this;
        }
        public Layer VelocityRange(float x0, float x1, float y0, float y1)
        {
            var lim = ps.limitVelocityOverLifetime; lim.enabled = false;
            var v = ps.velocityOverLifetime; v.enabled = true; v.space = ParticleSystemSimulationSpace.World;   // template layers are rotated; move in screen space
            v.orbitalX = 0f; v.orbitalY = 0f; v.orbitalZ = 0f; v.radial = 0f; v.speedModifier = 1f; v.orbitalOffsetX = 0f; v.orbitalOffsetY = 0f; v.orbitalOffsetZ = 0f;
            var frc = ps.forceOverLifetime; frc.enabled = false; var nz = ps.noise; nz.enabled = false;
            v.x = new ParticleSystem.MinMaxCurve(x0, x1); v.y = new ParticleSystem.MinMaxCurve(y0, y1); v.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            return this;
        }
        // continuous emission for `seconds` at `rate` per second instead of a single burst
        public Layer Stream(float rate, float seconds)
        {
            var e = ps.emission; e.enabled = true; e.SetBursts(new ParticleSystem.Burst[0]); e.rateOverTime = rate;
            var m = M; m.duration = Mathf.Max(0.05f, seconds); m.loop = false;
            return this;
        }
        public Layer Grow(float from, float to)
        {
            var s = ps.sizeOverLifetime; s.enabled = true; s.separateAxes = false;
            s.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, from), new Keyframe(0.6f, to), new Keyframe(1f, to * 0.8f)));
            return this;
        }
        public Layer Box(float sx, float sy)
        {
            var s = ps.shape; s.enabled = true; s.shapeType = ParticleSystemShapeType.Box; s.scale = new Vector3(sx, sy, 0.01f); s.rotation = Vector3.zero;
            return this;
        }
        // travels right at `speed` while weaving up and down (`cycles` sine waves over its life)
        public Layer Weave(float speed, float amplitude, float cycles, float phase)
        {
            var lim = ps.limitVelocityOverLifetime; lim.enabled = false;
            var v = ps.velocityOverLifetime; v.enabled = true; v.space = ParticleSystemSimulationSpace.World;
            v.orbitalX = 0f; v.orbitalY = 0f; v.orbitalZ = 0f; v.radial = 0f; v.speedModifier = 1f;
            var keysY = new List<Keyframe>(); var keysX = new List<Keyframe>(); var keysZ = new List<Keyframe>();
            for (int i = 0; i <= 24; i++)
            {
                float t = i / 24f;
                keysY.Add(new Keyframe(t, Mathf.Cos((t * cycles + phase) * Mathf.PI * 2f)));
                keysX.Add(new Keyframe(t, 1f)); keysZ.Add(new Keyframe(t, 0f));
            }
            // dy/dt of a sine with this amplitude: 2*pi*cycles/life * amplitude
            float life = ps.main.startLifetime.constantMax;
            v.x = new ParticleSystem.MinMaxCurve(speed, new AnimationCurve(keysX.ToArray()));
            v.y = new ParticleSystem.MinMaxCurve(amplitude * Mathf.PI * 2f * cycles / Mathf.Max(life, 0.05f), new AnimationCurve(keysY.ToArray()));
            v.z = new ParticleSystem.MinMaxCurve(0f, new AnimationCurve(keysZ.ToArray()));
            return this;
        }
        // `trail` emits continuously from this layer's particles as they move (a birth sub-emitter)
        public Layer TrailWith(Layer trail)
        {
            trail.go.transform.SetParent(go.transform, true);
            var sub = ps.subEmitters; sub.enabled = true;
            sub.AddSubEmitter(trail.ps, ParticleSystemSubEmitterType.Birth, ParticleSystemSubEmitterProperties.InheritNothing);
            var m = trail.ps.main; m.loop = true; m.duration = 1f; m.startDelay = 0f;
            return this;
        }
        // a painted ribbon trail behind each particle (Trails module, like the precast sword trail)
        public Layer PaintTrail(Material mat, float width, float life, Color colour)
        {
            var tr = ps.trails; tr.enabled = true; tr.mode = ParticleSystemTrailMode.PerParticle; tr.ratio = 1f;
            tr.lifetime = life; tr.minVertexDistance = 0.05f; tr.worldSpace = true; tr.dieWithParticles = false;
            tr.textureMode = ParticleSystemTrailTextureMode.Stretch; tr.sizeAffectsWidth = false; tr.inheritParticleColor = false;
            tr.widthOverTrail = new ParticleSystem.MinMaxCurve(width, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.15f)));
            Gradient g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(colour, 0f), new GradientColorKey(colour, 1f) }, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.6f, 0.5f), new GradientAlphaKey(0f, 1f) });
            tr.colorOverTrail = g;
            ps.GetComponent<ParticleSystemRenderer>().trailMaterial = mat;
            return this;
        }
        // particles circle the centre at `radPerSec`, emitted from a ring of `radius`
        public Layer Orbit(float radius, float radPerSec, float rise = 0f)
        {
            var lim = ps.limitVelocityOverLifetime; lim.enabled = false;
            var sh = ps.shape; sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Circle; sh.radius = radius; sh.radiusThickness = 0f; sh.arc = 360f; sh.rotation = Vector3.zero;
            var m = M; m.startSpeed = 0f; m.simulationSpace = ParticleSystemSimulationSpace.Local;
            var v = ps.velocityOverLifetime; v.enabled = true; v.space = ParticleSystemSimulationSpace.Local;
            v.x = new ParticleSystem.MinMaxCurve(0f); v.y = new ParticleSystem.MinMaxCurve(rise); v.z = new ParticleSystem.MinMaxCurve(0f);
            v.orbitalX = 0f; v.orbitalY = 0f; v.orbitalZ = radPerSec; v.radial = 0f; v.speedModifier = 1f;
            return this;
        }
        public Layer World() { var m = M; m.simulationSpace = ParticleSystemSimulationSpace.World; return this; }
        // one random still frame from an x-by-y sheet
        public Layer Sheet(int x, int y)
        {
            var t = ps.textureSheetAnimation; t.enabled = true; t.mode = ParticleSystemAnimationMode.Grid; t.numTilesX = x; t.numTilesY = y;
            t.animation = ParticleSystemAnimationType.WholeSheet; t.frameOverTime = new ParticleSystem.MinMaxCurve(0f); t.cycleCount = 1;
            t.startFrame = new ParticleSystem.MinMaxCurve(0f, x * y - 0.01f);
            return this;
        }
        // full strength until `holdUntil` (fraction of life), then fades out
        public Layer Fade(float holdUntil)
        {
            var c = ps.colorOverLifetime; c.enabled = true;
            Gradient g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, holdUntil), new GradientAlphaKey(0f, 1f) });
            c.color = g;
            return this;
        }
    }

    // Icon copies that only differed by tint share one material per texture; the tint moves onto the particle colour.
    // The shared copy's colours are neutral (B white, A a little over white for the painted highlights). MasterShader
    // reads particle colour on a gamma curve, so the tint goes on as tint^(1/2.2) to match the old material tint.
    static readonly Dictionary<string, string> SharedMats = new Dictionary<string, string>
    {
        { "CM_V2_AceCard", "CM_V2_Card" }, { "CM_V2_BloodCard", "CM_V2_Card" }, { "CM_V2_EmberCard", "CM_V2_Card" }, { "CM_V2_FlickedCard", "CM_V2_Card" },
        { "CM_V2_GoldCard", "CM_V2_Card" }, { "CM_V2_PlanCard", "CM_V2_Card" }, { "CM_V2_VanishCard", "CM_V2_Card" },
        { "CM_V2_AegisShield", "CM_V2_Shield" }, { "CM_V2_FocusShield", "CM_V2_Shield" }, { "CM_V2_SteelShield", "CM_V2_Shield" }, { "CM_V2_WaterShield", "CM_V2_Shield" },
        { "CM_V2_BloodBlade", "CM_V2_Blade" }, { "CM_V2_HonedBlade", "CM_V2_Blade" }, { "CM_V2_SteelBlade", "CM_V2_Blade" },
        { "CM_V2_Fist", "CM_V2_Fist" }, { "CM_V2_StoneFist", "CM_V2_Fist" },
        { "CM_V2_EmbersArcane", "CM_V2_Embers" }, { "CM_V2_EmbersGreen", "CM_V2_Embers" },
        { "CM_V2_ReviveHalo", "CM_V2_Sigil" }, { "CM_V2_Sigil", "CM_V2_Sigil" },
    };

    // in-build stand-ins for a shared material plus a tint; SaveAt swaps them for the shared asset
    static readonly Dictionary<Material, (Material shared, Color tint)> tintProxies = new Dictionary<Material, (Material, Color)>();

    static Material CopyOf(string sourceName, string newName)
    {
        Directory.CreateDirectory(MatDir);
        string dst = MatDir + "/" + newName + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(dst);
        if (m == null)
        {
            string src = AssetDatabase.FindAssets(sourceName + " t:Material").Select(AssetDatabase.GUIDToAssetPath).First(p => Path.GetFileNameWithoutExtension(p) == sourceName);
            AssetDatabase.CopyAsset(src, dst);
            m = AssetDatabase.LoadAssetAtPath<Material>(dst);
        }
        return m;
    }

    // CM_ copy of a source material with a new main texture and tint (Colour_B for MasterShader icons)
    public static Material Mat(string sourceName, string newName, string texturePath = null, Color? tintB = null, Color? tintA = null)
    {
        string sharedName = newName.StartsWith("CM_V2_Paw_") ? "CM_V2_Paw" : SharedMats.TryGetValue(newName, out string s) ? s : null;
        if (sharedName != null)
        {
            Material shared = CopyOf(sourceName, sharedName);
            if (texturePath != null) shared.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
            shared.SetColor("_Colour_B", Color.white);
            shared.SetColor("_Colour_A", new Color(1.3f, 1.3f, 1.3f, 1f));
            EditorUtility.SetDirty(shared);
            AssetDatabase.SaveAssetIfDirty(shared);
            Color t = tintB ?? Color.white;
            var proxy = new Material(shared) { name = newName + " (tint)" };
            tintProxies[proxy] = (shared, new Color(Mathf.Pow(t.r, 1f / 2.2f), Mathf.Pow(t.g, 1f / 2.2f), Mathf.Pow(t.b, 1f / 2.2f), 1f));
            return proxy;
        }
        Material m = CopyOf(sourceName, newName);
        if (texturePath != null) m.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
        if (tintB.HasValue && m.HasProperty("_Colour_B")) m.SetColor("_Colour_B", tintB.Value);
        // the icon shader lerps A -> B through painted noise; a pale A washes the icon out to white, so keep
        // A close to the identity colour (B) and let only the highlights lift toward the pale tint
        if (tintA.HasValue && m.HasProperty("_Colour_A")) m.SetColor("_Colour_A", tintB.HasValue ? Color.Lerp(tintB.Value, tintA.Value, 0.35f) : tintA.Value);
        // LineRenderShader (painted trails) names its lerp colours _Color_A / _Color_B
        if (tintB.HasValue && m.HasProperty("_Color_B")) m.SetColor("_Color_B", tintB.Value);
        if (tintA.HasValue && m.HasProperty("_Color_A")) m.SetColor("_Color_A", tintA.Value);
        EditorUtility.SetDirty(m);
        AssetDatabase.SaveAssetIfDirty(m);
        return m;
    }

    // an empty particle system on the root and on every group under it, so selecting any of them previews that part
    // of the effect in the editor; nested prefabs (an enemy's own hit) are left as authored
    public static int AddPreviewSystems(GameObject root)
    {
        int added = 0;
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.childCount == 0) continue;
            if (t.gameObject != root && PrefabUtility.IsPartOfPrefabInstance(t)) continue;
            if (AddPreviewRoot(t.gameObject)) added++;
        }
        return added;
    }

    // an empty particle system: it emits and draws nothing. Play On Awake is one flag shared by the whole hierarchy
    // (setting it here sets every system under it), so it stays on like the rest of the effect
    public static bool AddPreviewRoot(GameObject root)
    {
        if (root.GetComponent<ParticleSystem>() != null) return false;
        ParticleSystem ps = root.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.playOnAwake = true; main.loop = false; main.duration = 1f; main.maxParticles = 0;
        var emission = ps.emission; emission.enabled = false;
        var shape = ps.shape; shape.enabled = false;
        var r = root.GetComponent<ParticleSystemRenderer>();
        r.enabled = false; r.sharedMaterial = null;
        return true;
    }

    // swap each tint stand-in for its shared material and carry the tint on the layer's start colour instead
    static void ResolveTintProxies(GameObject root)
    {
        foreach (ParticleSystemRenderer r in root.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            if (r.sharedMaterial == null || !tintProxies.TryGetValue(r.sharedMaterial, out var p)) continue;
            r.sharedMaterial = p.shared;
            var m = r.GetComponent<ParticleSystem>().main;
            m.startColor = Tinted(m.startColor, p.tint);
        }
    }

    static ParticleSystem.MinMaxGradient Tinted(ParticleSystem.MinMaxGradient g, Color t)
    {
        switch (g.mode)
        {
            case ParticleSystemGradientMode.Color: return new ParticleSystem.MinMaxGradient(g.color * t);
            case ParticleSystemGradientMode.TwoColors: return new ParticleSystem.MinMaxGradient(g.colorMin * t, g.colorMax * t);
            case ParticleSystemGradientMode.TwoGradients: return new ParticleSystem.MinMaxGradient(Tinted(g.gradientMin, t), Tinted(g.gradientMax, t));
            default:
                var n = new ParticleSystem.MinMaxGradient(Tinted(g.gradient, t)); n.mode = g.mode;   // Gradient / RandomColor
                return n;
        }
    }

    static Gradient Tinted(Gradient src, Color t)
    {
        GradientColorKey[] keys = src.colorKeys;
        for (int i = 0; i < keys.Length; i++) keys[i].color *= t;
        var g = new Gradient { mode = src.mode };
        g.SetKeys(keys, src.alphaKeys);
        return g;
    }

    public static string P(string name) => PlaceholderDir + "/" + name + ".png";
    static Color C(string hex, float a = 1f) { ColorUtility.TryParseHtmlString(hex, out Color c); c.a = a; return c; }

    // ---------- shared beats ----------

    // dark backdrop + hero icon that flips black -> colour at ~0.3s
    static Layer HeroIcon(Fx fx, Material iconMat, float sizeX, float sizeY, float rotRad = 0f, float x = 0f, float y = 0f)
    {
        return fx.Add("Icon", "Hero Icon").Mat(iconMat).Still().Size2(sizeX, sizeY).Rot(rotRad).Pos(x, y).Sort(5);
    }

    // glow + ring + spikes on the impact frame (t is the impact time relative to the 0.3s template beat)
    static void Impact(Fx fx, Color core, Color hot, float shift = 0f, float scale = 1f, bool big = true)
    {
        fx.Add("Glow").Col(new Color(core.r, core.g, core.b, 0.5f)).Delay(shift - 0.0f).Size(scale);
        fx.Add("Flash").Col(new Color(hot.r, hot.g, hot.b, 0.55f)).Delay(shift - 0.0f).Size(scale * 0.8f);
        fx.Add("Ring").Col(hot).Delay(shift).Size(scale);
        if (big) fx.Add("SpikesBig").Col(core).Delay(shift).Size(scale);
        fx.Add("Spikes").Col(hot).Delay(shift).Size(scale * 0.9f);
    }

    static void Flecks(Fx fx, Color a, Color b, float shift = 0f, int count = 4)
    {
        fx.Add("DotsA").Col(a).Delay(shift).Count(Mathf.Max(1, count - 2), count);
        fx.Add("DotsB").Col(b).Delay(shift).Count(1, Mathf.Max(2, count - 1));
    }

    // ---------- recipes ----------

    static readonly Color FireCore = C("#FF6A1F"), FireHot = C("#FFD27A"), FireDark = C("#2A0B05", 1f);

    const string BlueprintTex = "Assets/VFX V2/Card Effect Blueprints/Textures/";
    const string CardTex = "Assets/VFX V2/Textures/T_CardHand.png";
    const string FireIcon = "Assets/VFX V3/New Project(15).png";   // the painted fire from the burn status (hero icon only)

    public static void Boomflask()
    {
        // a thrown flask: wobbles, shatters into glass, then the fire takes
        var fx = new Fx("Card Effect V2 - Boomflask");
        fx.Add("Backdrop").Col(FireDark);
        Material flask = Mat("M_SwordGold", "CM_V2_Flask", P("CTX_Flask"), C("#5FC8E8"), C("#E8FBFF"));
        HeroIcon(fx, flask, 1.7f, 1.7f, 0.25f).Spin(-1.6f).Life(0.55f);
        Impact(fx, FireCore, FireHot, 0.12f, 1.1f);
        fx.Add("Streaks", "Glass Shards").Col(C("#E8FBFF"), C("#9FE3FF")).Delay(0.12f).Count(6, 9).Size(0.8f).Speed(4f, 9f);
        fx.Add("Embers", "Flame Tongues").Col(FireCore, FireHot).Delay(0.16f).Count(12, 16).Size(2.6f).Velocity(0f, 1.6f);
        Flecks(fx, C("#9FE3FF"), FireHot, 0.12f, 5);
        fx.Save("Card Effect V2 - Boomflask");
    }

    public static void Detonate()
    {
        // per enemy: a lit fuse sparks, then a big flash with a double shockwave and rising smoke
        var fx = new Fx("Card Effect V2 - Detonate", 5);
        fx.root.AddComponent<SweepDelayByPosition>();
        fx.Add("Backdrop").Col(C("#1A0A08")).Size(1.25f).Life(1.6f);
        Material bomb = Mat("M_SwordGold", "CM_V2_Bomb", P("CTX_Bomb"), C("#3A2A30"), C("#7A6A70"));
        HeroIcon(fx, bomb, 1.8f, 1.8f).Life(0.5f);
        fx.Add("StarIcons", "Fuse Spark").Col(C("#FFE38A")).DelayAt(0.05f).Life(0.3f, 0.4f).Count(2, 3).Pos(0.45f, 0.75f).Speed(0.5f, 1.5f);
        Impact(fx, FireCore, C("#FFF4D6"), 0.2f, 1.6f);
        fx.Add("Ring", "Shockwave 2").Col(FireCore).Delay(0.32f).Size(2.2f);
        fx.Add("Embers", "Fireball").Col(FireCore, FireHot).Delay(0.22f).Count(20, 26).Size(3.4f).Speed(1f, 3.5f);
        fx.Add("Backdrop", "Smoke").Col(C("#3A3436", 0.8f)).Delay(0.55f).Size(0.7f).Velocity(0f, 0.8f);
        Flecks(fx, FireCore, C("#FFF4D6"), 0.2f, 7);
        fx.Save("Card Effect V2 - Detonate");
    }

    public static void Pyrotechnics()
    {
        // a shell streaks up, then three crisp coloured star bursts
        var fx = new Fx("Card Effect V2 - Pyrotechnics", 5);
        fx.Add("Backdrop").Col(C("#120818")).Size(1.2f).Life(1.8f);
        fx.Add("Streaks", "Shell").Col(FireHot).DelayAt(0f).Count(1, 1).Life(0.3f).Speed(9f, 10f).Pos(0f, -1.6f).Shape(ParticleSystemShapeType.Cone, 0.01f, 0f, 1f);
        (float x, float y, float t, Color c)[] bursts = { (-0.55f, 0.45f, 0.0f, C("#FFD35A")), (0.6f, 0.65f, 0.15f, C("#FF5C8A")), (0.05f, -0.15f, 0.3f, C("#3DD6C8")) };
        foreach (var b in bursts)
        {
            fx.Add("MagicStar", "Star Core").Col(b.c).Delay(b.t - 0.1f).Pos(b.x, b.y).Size(0.8f);
            fx.Add("Spikes", "Star Rays").Col(b.c).Delay(b.t).Pos(b.x, b.y).Size(0.75f);
            fx.Add("StarIcons", "Star Sparks").Col(b.c, Color.white).Delay(b.t - 0.45f).Pos(b.x, b.y).Count(4, 6).Speed(1.5f, 3.5f);
        }
        fx.Add("Flash").Col(new Color(1f, 0.9f, 0.7f, 0.5f)).Delay(0f).Size(0.9f);
        fx.Save("Card Effect V2 - Pyrotechnics");
    }

    public static void Trailblaze()
    {
        // a diagonal slash of fire that leaves embers burning along its line
        var fx = new Fx("Card Effect V2 - Trailblaze");
        fx.Add("Backdrop").Col(FireDark);
        fx.Add("SlashDark", "Slash Shadow").Col(C("#1A0603"));
        fx.Add("SlashLight", "Slash Fire").Col(FireHot);
        Impact(fx, FireCore, FireHot, -0.05f, 0.9f, false);
        fx.Add("Embers", "Trail Embers").Col(FireCore, FireHot).Delay(0.05f).Count(14, 18).Size(1.8f).Shape(ParticleSystemShapeType.SingleSidedEdge, 1.6f, 30f).Velocity(0f, 1.2f);
        Flecks(fx, FireCore, FireHot, -0.05f, 5);
        fx.Save("Card Effect V2 - Trailblaze");
    }

    public static void TrailblazeDraw()
    {
        // on the caster: an ember-edged card rises (a card drawn)
        var fx = new Fx("Card Effect V2 - Trailblaze Draw");
        fx.Add("Backdrop").Col(FireDark).Size(0.8f);
        Material card = Mat("M_SwordGold", "CM_V2_EmberCard", CardTex, C("#FFB347"), C("#FFF1C2"));
        HeroIcon(fx, card, 1.3f, 1.3f).Velocity(0f, 1.1f);
        fx.Add("Arrows", "Rise").Col(FireCore);
        fx.Add("Embers", "Card Embers").Col(FireCore, FireHot).Delay(0.1f).Count(8, 10).Size(1.2f).Velocity(0f, 1.6f);
        Flecks(fx, FireCore, FireHot, 0f, 3);
        fx.Save("Card Effect V2 - Trailblaze Draw");
    }

    public static void GreekFireImpact()
    {
        // where the pot lands: a sticky green-gold fire that clings and lingers
        var fx = new Fx("Card Effect V2 - Greek Fire Impact", 5);
        fx.Add("Backdrop").Col(C("#061A14")).Size(1.1f).Life(1.8f);
        // hero: the painted fire icon, green; the living flames around it are the ember sheet
        Material blaze = Mat("M_SwordGold", "CM_V2_GreekBlaze", FireIcon, C("#6BFF7A"), C("#E8FF9A"));
        HeroIcon(fx, blaze, 2.2f, 2.2f, 0f, 0f, 0.3f).Life(1.4f);
        Material greenEmbers = Mat("M_Embers", "CM_V2_EmbersGreen", null, C("#2FBF4E"), C("#E8FF9A"));
        fx.Add("Embers", "Clinging Blaze").Mat(greenEmbers).Col(C("#2FBF4E"), C("#B8F05A")).DelayAt(0.28f).Stream(18f, 1.1f).Life(0.5f, 0.8f)
            .Size(3.2f).Speed(0f, 0f).VelocityRange(-0.3f, 0.3f, 1.2f, 2.2f).Box(1.4f, 0.3f).Pos(0f, -0.2f).Sort(6);
        Impact(fx, C("#3BE85A"), C("#E8FF9A"), 0f, 1f);
        fx.Add("InkSplat", "Sticky Splash").Col(C("#2FBF4E")).Delay(-0.02f).Size(0.8f);
        fx.Add("InkBlops", "Clinging Drops").Col(C("#C8FF6A"), C("#3BE85A")).Count(8, 12);
        fx.Add("Embers", "Green Flames").Mat(greenEmbers).Col(C("#3BE85A"), C("#C8FF6A")).Delay(0.1f).Count(12, 16).Size(2.2f).Life(0.6f, 1.4f).Velocity(0f, 1.3f);
        Flecks(fx, C("#3BE85A"), C("#E8FF9A"), 0f, 5);
        fx.Save("Card Effect V2 - Greek Fire Impact");
    }

    public static void GreekFirePot()
    {
        // the thrown pot: spins in flight with a short green fire trail (played by the FX Experience)
        var fx = new Fx("Card Effect V2 - Greek Fire Pot", 3);
        Material pot = Mat("M_SwordGold", "CM_V2_GreekFirePot", P("CTX_GreekFirePot"), C("#C0703A"), C("#F2C79A"));
        // in full colour for the whole flight (no ink-to-colour flip: it has no impact frame of its own)
        fx.Add("Icon", "Pot").Mat(pot).Still().NoColLife().Col(Color.white).Size2(1.3f, 1.3f).DelayAt(0f).Life(1.2f).Spin(-9f).Sort(7);
        fx.Add("Glow", "Pot Glow").Col(new Color(0.3f, 0.9f, 0.35f, 0.55f)).Still().NoColLife().DelayAt(0f).Life(1.2f).Size(0.9f).Sort(5);
        Material greenEmbers = Mat("M_Embers", "CM_V2_EmbersGreen", null, C("#2FBF4E"), C("#E8FF9A"));
        fx.Add("Embers", "Trail Flames").Mat(greenEmbers).Col(C("#3BE85A"), C("#C8FF6A")).DelayAt(0f).Life(0.3f, 0.55f).Size(1.7f).Sort(6);
        var trail = fx.root.transform.Find("Trail Flames").GetComponent<ParticleSystem>();
        var e = trail.emission; e.SetBursts(new ParticleSystem.Burst[0]); e.rateOverTime = 75f;
        var m = trail.main; m.duration = 1.2f; m.simulationSpace = ParticleSystemSimulationSpace.World;
        fx.Save("Card Effect V2 - Greek Fire Pot");
    }

    const string GreekFireThrowPath = "Assets/Prefabs/VFX Experience/Card Variants/FXE_GreekFire_Throw.prefab";
    const float GreekFireFlight = 0.55f;

    // The FX Experience for Greek Fire: the pot is lobbed from the rat whose card it was ("self") to the enemy
    // ("target"), like the dagger throws. The tween carries it in a straight line; ThrowArc lifts it into an arc.
    // The card's damage and the Greek Fire Impact play when the experience ends.
    public static void GreekFireThrow()
    {
        const string timelinePath = "Assets/Animations/Timelines/FX/Greek Fire/FXE_GreekFire_Throw.playable";
        if (!AssetDatabase.IsValidFolder("Assets/Animations/Timelines/FX/Greek Fire"))
            AssetDatabase.CreateFolder("Assets/Animations/Timelines/FX", "Greek Fire");
        TimelineAsset timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(timelinePath);
        if (timeline == null)
        {
            timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timeline, timelinePath);
        }
        foreach (TrackAsset old in timeline.GetRootTracks().ToList()) timeline.DeleteTrack(old);

        GameObject root = new GameObject("FXE_GreekFire_Throw");
        PlayableDirector director = root.AddComponent<PlayableDirector>();
        director.playOnAwake = false;
        director.extrapolationMode = DirectorWrapMode.None;
        director.playableAsset = timeline;
        FXExperience experience = root.AddComponent<FXExperience>();
        experience.playableDirector = director;

        GameObject flight = new GameObject("Greek Fire Throw");
        flight.transform.SetParent(root.transform, false);
        GameObject arc = new GameObject("Arc");
        arc.transform.SetParent(flight.transform, false);
        var arcSo = new SerializedObject(arc.AddComponent<ThrowArc>());
        arcSo.FindProperty("flightTime").floatValue = GreekFireFlight;
        arcSo.FindProperty("height").floatValue = 1.4f;
        arcSo.ApplyModifiedPropertiesWithoutUndo();
        GameObject pot = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(OutDir + "/Card Effect V2 - Greek Fire Pot.prefab"));
        pot.transform.SetParent(arc.transform, false);
        flight.SetActive(false);   // the activation clip shows it for the flight only

        ActivationTrack active = timeline.CreateTrack<ActivationTrack>(null, "Throw Active");
        TimelineClip activeClip = active.CreateDefaultClip();
        activeClip.start = 0; activeClip.duration = GreekFireFlight;
        director.SetGenericBinding(active, flight);

        TweenTrack path = timeline.CreateTrack<TweenTrack>(null, "Throw Path");
        TimelineClip pathClip = path.CreateClip<TweenClip>();
        pathClip.start = 0; pathClip.duration = GreekFireFlight;
        TweenClip tween = (TweenClip)pathClip.asset;
        tween.startLocationKey = "self";
        tween.endLocationKey = "target";
        tween.shouldRotate = false;
        tween.curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        ExposedReference<FXExperience> reference = tween.experienceReference;
        reference.exposedName = GUID.Generate().ToString();
        tween.experienceReference = reference;
        director.SetReferenceValue(reference.exposedName, experience);
        director.SetGenericBinding(path, flight.transform);
        EditorUtility.SetDirty(tween);
        EditorUtility.SetDirty(timeline);
        AssetDatabase.SaveAssets();

        PrefabUtility.SaveAsPrefabAsset(root, GreekFireThrowPath);
        UnityEngine.Object.DestroyImmediate(root);
    }

    // ---------- gun shots ----------
    // Gun cards and abilities fire a projectile from the shooting rat in a straight line before their hit
    // effect plays, so it reads who shot whom. Two weights: a quick bullet, and a heavier cannon shell.
    public const string BulletFxPath = "Assets/Prefabs/VFX Experience/Card Variants/FXE_Shot_Bullet.prefab";
    public const string CannonFxPath = "Assets/Prefabs/VFX Experience/Card Variants/FXE_Shot_Cannon.prefab";
    public const float BulletFlight = 0.16f;
    public const float CannonFlight = 0.26f;
    static readonly Color Brass = C("#FFC864"), BrassHot = C("#FFF4D6"), Ember = C("#FF8A3A"), Smoke = new Color(0.78f, 0.74f, 0.72f, 0.45f);
    static readonly Color InkBlack = C("#14101A");
    // a painted dab (white, tinted to ink); it fills about a fifth of its texture, hence the large particle sizes
    const string InkBlot = "Assets/VFX V3/TX_IMG_1391.png";

    // the layer moves with its emitter (the tweened projectile) and is drawn facing the way it flies
    // (template layers are tilted to face along a cone, which would draw them edge-on here)
    static Layer Riding(Layer l)
    {
        l.Flat();
        // solid from the first frame: no dissolve-in (custom data) or grow-in, the flight is too short for them
        var cd = l.ps.customData; cd.enabled = false;
        var sz = l.ps.sizeOverLifetime; sz.enabled = false;
        // centred on the projectile and pointing exactly along the flight
        var shape = l.ps.shape; shape.enabled = false;
        var m = l.ps.main; m.simulationSpace = ParticleSystemSimulationSpace.Local;
        m.startRotation = 0f; m.startSpeed = 0f;
        l.ps.GetComponent<ParticleSystemRenderer>().alignment = ParticleSystemRenderSpace.Local;
        return l;
    }

    public static void BuildGunShots()
    {
        // the bullet: a flick of ink stretched along its flight (reads clearly on the red board), a small brass
        // glint at its nose, and a short ink trail
        Material inkSlug = Mat("M_BasicParticlesAlpha", "CM_V2_InkDab", InkBlot, InkBlack, InkBlack);   // plain alpha: solid ink
        Material inkTrail = Mat("M_LineWatercolour", "CM_V2_InkShotTrail", null, InkBlack, C("#2A2230"));
        var bullet = new Fx("Card Effect V2 - Bullet", 2);
        Riding(bullet.Add("Icon", "Ink Slug").Mat(inkSlug).Still().NoColLife().Col(InkBlack).DelayAt(0f).Life(1f).Size2(5.2f, 1.9f).Pos(0f, 0f).Sort(6))
            .PaintTrail(inkTrail, 0.32f, 0.12f, C("#1A1420"));
        Riding(bullet.Add("Glow", "Brass Glint").Col(new Color(Brass.r, Brass.g, Brass.b, 0.75f)).Still().NoColLife().DelayAt(0f).Life(1f).Size2(0.42f, 0.16f).Pos(0.25f, 0f).Sort(7));
        bullet.Save("Card Effect V2 - Bullet");

        // the shot from the barrel: a quick flash and spark, then a wisp of smoke
        var muzzle = new Fx("Card Effect V2 - Muzzle Flash", 1);
        muzzle.Add("Flash").Col(BrassHot).DelayAt(0f).Life(0.12f).Size(0.75f).Sort(6);
        muzzle.Add("Spikes", "Muzzle Spikes").Col(Brass).DelayAt(0f).Life(0.14f).Size(0.55f).Sort(6);
        muzzle.Add("DotsB", "Muzzle Sparks").Col(Brass, BrassHot).DelayAt(0f).Count(5, 7).Life(0.18f, 0.3f).Size(0.6f).Sort(7);
        muzzle.Add("InkBlops", "Muzzle Ink").Col(InkBlack).DelayAt(0f).Count(3, 5).Size(0.45f).Sort(5);
        muzzle.Add("Backdrop", "Muzzle Smoke").Col(Smoke).DelayAt(0.04f).Life(0.5f, 0.7f).Size(0.35f).Velocity(0f, 0.5f).Sort(4);
        muzzle.Save("Card Effect V2 - Muzzle Flash");

        // the cannon shell: a heavier ball of ink, barely stretched, with an ember rim and a smoke trail
        var shell = new Fx("Card Effect V2 - Cannon Shell", 2);
        Riding(shell.Add("Glow", "Shell Ember").Col(new Color(Ember.r, Ember.g, Ember.b, 0.8f)).Still().NoColLife().DelayAt(0f).Life(1f).Size2(1.2f, 0.95f).Pos(0f, 0f).Sort(5))
            .PaintTrail(Mat("M_LineWatercolour", "CM_V2_CannonSmokeTrail", null, new Color(0.85f, 0.82f, 0.8f), new Color(0.55f, 0.52f, 0.5f)), 0.55f, 0.28f, new Color(0.8f, 0.76f, 0.74f));
        Riding(shell.Add("Icon", "Ink Shell").Mat(inkSlug).Still().NoColLife().Col(InkBlack).DelayAt(0f).Life(1f).Size2(6.6f, 5.2f).Pos(0f, 0f).Sort(6));
        shell.Save("Card Effect V2 - Cannon Shell");

        // the cannon blast: a big flash and a billow of smoke and steam
        var blast = new Fx("Card Effect V2 - Cannon Blast", 2);
        blast.Add("Flash").Col(new Color(1f, 0.7f, 0.38f)).DelayAt(0f).Life(0.14f).Size(0.85f).Sort(6);
        blast.Add("SpikesBig", "Blast Spikes").Col(Ember).DelayAt(0f).Life(0.18f).Size(0.8f).Sort(6);
        blast.Add("Backdrop", "Blast Smoke").Col(new Color(0.55f, 0.52f, 0.5f, 0.55f)).DelayAt(0.03f).Count(3, 4).Life(0.7f, 1f).Size(0.55f).VelocityRange(-0.4f, 0.4f, 0.3f, 0.8f).Sort(4);
        blast.Add("Backdrop", "Blast Steam").Col(new Color(0.92f, 0.92f, 0.95f, 0.4f)).DelayAt(0.08f).Count(2, 3).Life(0.6f, 0.9f).Size(0.45f).VelocityRange(-0.2f, 0.2f, 0.7f, 1.1f).Sort(5);
        blast.Save("Card Effect V2 - Cannon Blast");

        // the lock-on mark at the target (placeholder art: drawing over CTX_Crosshair updates every shot)
        var reticle = new Fx("Card Effect V2 - Reticle", 2);
        Material ret = Mat("M_BasicParticlesAlpha", "CM_V2_Reticle", P("CTX_Crosshair"), Color.white, Color.white);
        var rl = reticle.Add("Icon", "Reticle").Mat(ret).Still().NoColLife().Col(new Color(1f, 0.94f, 0.82f, 0.95f)).DelayAt(0f).Life(1f).Size2(1.5f, 1.5f).Spin(1.2f).Sort(8);
        var rcd = rl.ps.customData; rcd.enabled = false;
        reticle.Save("Card Effect V2 - Reticle");

        BuildShotExperience(BulletFxPath, "Card Effect V2 - Bullet", "Card Effect V2 - Muzzle Flash", BulletFlight, 0.7f);
        BuildShotExperience(CannonFxPath, "Card Effect V2 - Cannon Shell", "Card Effect V2 - Cannon Blast", CannonFlight, 1.1f);
    }

    // A straight-line shot: the projectile tweens from "self" to "target" facing the way it flies; the muzzle
    // effect plays at "self" for the whole experience. The card waits `flight` and then plays its own hit.
    // A shot that carries its own hit, like the dagger throws: the impact plays at "target" just before the
    // projectile arrives, and the card's damage step no longer spawns a hit of its own. One per hit effect.
    public static GameObject ShotVariant(bool cannon, GameObject impact)
    {
        string kind = cannon ? "Cannon" : "Bullet";
        string safe = new string(impact.name.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
        string path = "Assets/Prefabs/VFX Experience/Card Variants/FXE_Shot_" + kind + "_" + safe + ".prefab";
        BuildShotExperience(path, cannon ? "Card Effect V2 - Cannon Shell" : "Card Effect V2 - Bullet", cannon ? "Card Effect V2 - Cannon Blast" : "Card Effect V2 - Muzzle Flash",
            cannon ? CannonFlight : BulletFlight, 0f, impact);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    static void BuildShotExperience(string prefabPath, string projectileName, string muzzleName, float flight, float length, GameObject impactPrefab = null)
    {
        const float ImpactLead = 0.02f, ImpactTime = 1.6f;
        if (impactPrefab != null) length = Mathf.Max(length, 0.7f, flight - ImpactLead + ImpactTime);
        string name = Path.GetFileNameWithoutExtension(prefabPath);
        string timelinePath = "Assets/Animations/Timelines/FX/Shots/" + name + ".playable";
        if (!AssetDatabase.IsValidFolder("Assets/Animations/Timelines/FX/Shots")) AssetDatabase.CreateFolder("Assets/Animations/Timelines/FX", "Shots");
        TimelineAsset timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(timelinePath);
        if (timeline == null) { timeline = ScriptableObject.CreateInstance<TimelineAsset>(); AssetDatabase.CreateAsset(timeline, timelinePath); }
        foreach (TrackAsset old in timeline.GetRootTracks().ToList()) timeline.DeleteTrack(old);

        GameObject root = new GameObject(name);
        PlayableDirector director = root.AddComponent<PlayableDirector>();
        director.playOnAwake = false;
        director.extrapolationMode = DirectorWrapMode.None;
        director.playableAsset = timeline;
        FXExperience experience = root.AddComponent<FXExperience>();
        experience.playableDirector = director;

        GameObject projectile = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(OutDir + "/" + projectileName + ".prefab"));
        projectile.transform.SetParent(root.transform, false);
        projectile.SetActive(false);
        GameObject muzzle = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(OutDir + "/" + muzzleName + ".prefab"));
        muzzle.transform.SetParent(root.transform, false);
        muzzle.SetActive(false);

        System.Func<ExposedReference<FXExperience>, ExposedReference<FXExperience>> bind = r =>
        {
            r.exposedName = GUID.Generate().ToString();
            director.SetReferenceValue(r.exposedName, experience);
            return r;
        };

        ActivationTrack shotActive = timeline.CreateTrack<ActivationTrack>(null, "Shot Active");
        TimelineClip a = shotActive.CreateDefaultClip(); a.start = 0; a.duration = flight;
        director.SetGenericBinding(shotActive, projectile);
        TweenTrack path = timeline.CreateTrack<TweenTrack>(null, "Shot Path");
        TimelineClip p = path.CreateClip<TweenClip>(); p.start = 0; p.duration = flight;
        TweenClip tween = (TweenClip)p.asset;
        tween.startLocationKey = "self"; tween.endLocationKey = "target"; tween.shouldRotate = true;
        tween.curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        tween.experienceReference = bind(tween.experienceReference);
        director.SetGenericBinding(path, projectile.transform);
        EditorUtility.SetDirty(tween);

        ActivationTrack muzzleActive = timeline.CreateTrack<ActivationTrack>(null, "Muzzle Active");
        TimelineClip ma = muzzleActive.CreateDefaultClip(); ma.start = 0; ma.duration = length;
        director.SetGenericBinding(muzzleActive, muzzle);
        PositionTrack at = timeline.CreateTrack<PositionTrack>(null, "Muzzle At");
        TimelineClip mp = at.CreateClip<PositionClip>(); mp.start = 0; mp.duration = length;
        PositionClip pos = (PositionClip)mp.asset;
        pos.locationKey = "self";
        pos.experienceReference = bind(pos.experienceReference);
        director.SetGenericBinding(at, muzzle.transform);
        EditorUtility.SetDirty(pos);

        // the reticle marks the target from the shot until just after it lands
        GameObject reticleFx = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(OutDir + "/Card Effect V2 - Reticle.prefab"));
        reticleFx.transform.SetParent(root.transform, false);
        reticleFx.SetActive(false);
        ActivationTrack reticleActive = timeline.CreateTrack<ActivationTrack>(null, "Reticle Active");
        TimelineClip ra = reticleActive.CreateDefaultClip(); ra.start = 0; ra.duration = flight + 0.2f;
        director.SetGenericBinding(reticleActive, reticleFx);
        PositionTrack reticleAt = timeline.CreateTrack<PositionTrack>(null, "Reticle At");
        TimelineClip rp = reticleAt.CreateClip<PositionClip>(); rp.start = 0; rp.duration = ra.duration;
        PositionClip reticlePos = (PositionClip)rp.asset;
        reticlePos.locationKey = "target";
        reticlePos.experienceReference = bind(reticlePos.experienceReference);
        director.SetGenericBinding(reticleAt, reticleFx.transform);
        EditorUtility.SetDirty(reticlePos);

        if (impactPrefab != null)
        {
            GameObject impact = (GameObject)PrefabUtility.InstantiatePrefab(impactPrefab);
            impact.transform.SetParent(root.transform, false);
            // the experience owns its lifetime: no self-destroying parts that the timeline still drives
            foreach (ParticleSystem ps in impact.GetComponentsInChildren<ParticleSystem>(true)) { var m = ps.main; m.stopAction = ParticleSystemStopAction.None; }
            foreach (DestroyAfterSeconds d in impact.GetComponentsInChildren<DestroyAfterSeconds>(true)) UnityEngine.Object.DestroyImmediate(d);
            impact.SetActive(false);
            ActivationTrack hitActive = timeline.CreateTrack<ActivationTrack>(null, "Impact Active");
            TimelineClip ha = hitActive.CreateDefaultClip(); ha.start = flight - ImpactLead; ha.duration = length - ha.start;
            director.SetGenericBinding(hitActive, impact);
            PositionTrack hitAt = timeline.CreateTrack<PositionTrack>(null, "Impact At");
            TimelineClip hp = hitAt.CreateClip<PositionClip>(); hp.start = ha.start; hp.duration = ha.duration;
            PositionClip hitPos = (PositionClip)hp.asset;
            hitPos.locationKey = "target";
            hitPos.experienceReference = bind(hitPos.experienceReference);
            director.SetGenericBinding(hitAt, impact.transform);
            EditorUtility.SetDirty(hitPos);
        }
        EditorUtility.SetDirty(timeline);
        AssetDatabase.SaveAssets();

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        UnityEngine.Object.DestroyImmediate(root);
    }

    // ---------- dagger throws ----------
    const string DaggerTimelinePath = "Assets/Animations/Timelines/FX/FXE_Dagger.playable";
    static readonly string[] DaggerVariants = { "FXE_Dagger_Dagger", "FXE_Dagger_JaggedKnife", "FXE_Dagger_LethalContract", "FXE_Dagger_Pureknife" };
    const float DaggerFlight = 0.24f;      // was 0.4
    const float DaggerImpactAt = 0.22f;    // was 0.31; just before the blade lands so the flash meets it
    const float DaggerArcHeight = 1.0f;
    const float DaggerImpactScale = 1.35f;

    // Snappier dagger throws for every variant: a faster flight on a quick-out, accelerating curve, a shallow
    // arc the blade pitches along (ThrowArc), a world-space trail so the arc reads, and a bigger impact. The
    // cards that throw them wait for the blade before dealing damage, so that wait follows the new flight.
    [MenuItem("VFX Retool/Tune dagger throws")]
    public static string TuneDaggerThrows()
    {
        var log = new System.Text.StringBuilder();
        TimelineAsset timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(DaggerTimelinePath);
        foreach (TrackAsset track in timeline.GetOutputTracks())
        {
            foreach (TimelineClip clip in track.GetClips())
            {
                bool impact = track is ActivationTrack ? track.name.Contains("(1)") : !(track is TweenTrack);
                if (impact) { clip.start = DaggerImpactAt; clip.duration = 1.0; }
                else { clip.start = 0; clip.duration = DaggerFlight; }
                if (clip.asset is TweenClip tween)
                {
                    // leaves the hand quickly, still accelerating into the target
                    tween.curve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0.6f), new Keyframe(1f, 1f, 1.4f, 0f));
                    EditorUtility.SetDirty(tween);
                }
                log.AppendLine("  " + track.name + " " + clip.start.ToString("F2") + " + " + clip.duration.ToString("F2"));
            }
        }
        EditorUtility.SetDirty(timeline);

        foreach (string variant in DaggerVariants)
        {
            string path = "Assets/Prefabs/VFX Experience/Card Variants/" + variant + ".prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            Transform flight = root.transform.Find("Card Effect - Dagger Throw");
            Transform sword = flight != null ? flight.Find("Sword") : null;
            if (flight == null || sword == null) { log.AppendLine(variant + ": no throw/blade found"); PrefabUtility.UnloadPrefabContents(root); continue; }
            // The blade is part of a nested prefab, so it can't be moved under a new parent: the arc goes on the
            // blade itself (it offsets from its parent, the tweened throw root). An empty "Arc" left by an
            // earlier pass means the impact was already enlarged.
            Transform staleArc = flight.Find("Arc");
            ThrowArc throwArc = sword.GetComponent<ThrowArc>();
            bool hadStaleArc = staleArc != null;
            bool firstTime = throwArc == null && !hadStaleArc;
            if (hadStaleArc) UnityEngine.Object.DestroyImmediate(staleArc.gameObject);
            // the throw root was zero-scaled (it draws nothing itself); under a zero scale a child can't be
            // offset at all, which flattens the arc. Its particles scale locally, so 1 changes nothing else.
            flight.localScale = Vector3.one;
            if (throwArc == null) throwArc = sword.gameObject.AddComponent<ThrowArc>();
            var arcSo = new SerializedObject(throwArc);
            arcSo.FindProperty("flightTime").floatValue = DaggerFlight;
            arcSo.FindProperty("height").floatValue = DaggerArcHeight;
            arcSo.FindProperty("tiltWithArc").boolValue = true;
            arcSo.ApplyModifiedPropertiesWithoutUndo();

            // the blade simulates in local space, so its trail only shows the arc if the trail is in world space
            var trails = sword.GetComponent<ParticleSystem>().trails;
            trails.worldSpace = true;

            // bigger impact (applied once, when the arc is first added)
            Transform impactRoot = root.transform.Find("Card Effect - Dagger Impact");
            if (firstTime && impactRoot != null)
            {
                foreach (ParticleSystem ps in impactRoot.GetComponentsInChildren<ParticleSystem>(true))
                {
                    var m = ps.main;
                    if (m.startSize3D) { m.startSizeXMultiplier *= DaggerImpactScale; m.startSizeYMultiplier *= DaggerImpactScale; m.startSizeZMultiplier *= DaggerImpactScale; }
                    else m.startSizeMultiplier *= DaggerImpactScale;
                    var shape = ps.shape;
                    if (shape.enabled) shape.radius *= DaggerImpactScale;
                    m.startSpeedMultiplier *= DaggerImpactScale;
                }
            }
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            log.AppendLine(variant + (firstTime ? ": arc added, impact x" + DaggerImpactScale : ": arc updated") + (hadStaleArc ? " (removed empty Arc)" : ""));
        }

        // the cards wait after starting the throw, then deal damage: land the damage with the blade
        var daggerFx = new HashSet<GameObject>(DaggerVariants.Select(v => AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX Experience/Card Variants/" + v + ".prefab")));
        foreach (string guid in AssetDatabase.FindAssets("t:CardType", new[] { "Assets/ScriptableObjects" }))
        {
            string cardPath = AssetDatabase.GUIDToAssetPath(guid);
            var card = AssetDatabase.LoadAssetAtPath<ScriptableObject>(cardPath);
            var so = new SerializedObject(card);
            bool changed = false;
            SerializedProperty it = so.GetIterator();
            var stepLists = new List<string>();
            while (it.Next(true)) if (it.name == "effectSteps" && it.isArray) stepLists.Add(it.propertyPath);
            foreach (string listPath in stepLists)
            {
                SerializedProperty steps = so.FindProperty(listPath);
                for (int i = 0; i + 1 < steps.arraySize; i++)
                {
                    SerializedProperty fx = steps.GetArrayElementAtIndex(i).FindPropertyRelative("fXExperiencePrefab");
                    if (fx == null || !daggerFx.Contains(fx.objectReferenceValue as GameObject)) continue;
                    SerializedProperty seconds = steps.GetArrayElementAtIndex(i + 1).FindPropertyRelative("seconds");
                    if (seconds == null || Mathf.Approximately(seconds.floatValue, DaggerFlight)) continue;
                    log.AppendLine(Path.GetFileNameWithoutExtension(cardPath) + ": wait " + seconds.floatValue + " -> " + DaggerFlight);
                    seconds.floatValue = DaggerFlight;
                    changed = true;
                }
            }
            if (changed) { so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(card); AssetDatabase.SaveAssetIfDirty(card); }
        }
        AssetDatabase.SaveAssets();
        return log.ToString();
    }

    // ---------- blood ----------
    static readonly Color BloodCore = C("#C8141E"), BloodHot = C("#FF6B5A"), BloodDark = C("#1E0710");

    public static void BloodiedBlade()
    {
        // a crimson blade cut; the blood drips instead of sparking
        var fx = new Fx("Card Effect V2 - Bloodied Blade");
        fx.Add("Backdrop").Col(BloodDark);
        PaintWash(fx, new Color(BloodCore.r, BloodCore.g, BloodCore.b, 0.65f), 0f, 0.9f);
        Material blade = Mat("M_SwordGold", "CM_V2_BloodBlade", "Assets/VFX V3/Card Effects V2/Textures/CTX_SwordSilhouette.png", BloodCore, C("#FFC2B8"));
        fx.Add("Icon", "Hero Icon").Mat(blade).Sort(5);
        Impact(fx, BloodCore, BloodHot, 0f, 1f);
        fx.Add("BlackSplots", "Blood Drips").Col(C("#8E0A14"), C("#C8141E")).Delay(-0.25f).Count(6, 9).Grav(0.9f);
        Flecks(fx, BloodHot, C("#FFE0D8"), 0f, 3);
        fx.Save("Card Effect V2 - Bloodied Blade");
    }

    public static void Ritual()
    {
        // a blood sigil turns; a drop falls in and it flares twice (one per card drawn)
        var fx = new Fx("Card Effect V2 - Ritual", 5);
        fx.Add("Backdrop").Col(BloodDark).Size(1.15f).Life(1.8f);
        Material sigil = Mat("M_SwordGold", "CM_V2_Sigil", BlueprintTex + "CTX_BloodSigil.png", BloodCore, C("#FF9A8A"));
        HeroIcon(fx, sigil, 2.6f, 2.6f).Spin(1.2f).Life(1.4f);
        Material drop = Mat("M_SwordGold", "CM_V2_BloodDrop", P("CTX_BloodDrop"), BloodCore, C("#FF8A7A"));
        fx.Add("Icon", "Falling Drop").Mat(drop).Still().Size2(0.6f, 0.6f).Pos(0f, 1.4f).Velocity(0f, -4.2f).Life(0.33f).DelayAt(0f).Sort(6);
        Impact(fx, BloodCore, BloodHot, 0.02f, 0.9f, false);
        PaintWash(fx, new Color(BloodCore.r, BloodCore.g, BloodCore.b, 0.6f), 0.3f, 1.1f);   // where the drop lands
        fx.Add("Ring", "Second Flare").Col(BloodHot).Delay(0.32f).Size(1.2f);
        fx.Add("Glow", "Second Glow").Col(new Color(BloodCore.r, BloodCore.g, BloodCore.b, 0.5f)).Delay(0.32f);
        Material card = Mat("M_SwordGold", "CM_V2_BloodCard", CardTex, BloodHot, C("#FFE0D8"));
        fx.Add("Icon", "Card 1").Mat(card).Still().Size2(0.7f, 0.7f).Pos(-0.5f, 0.2f).Velocity(-0.2f, 1.4f).DelayAt(0.25f).Life(0.8f).Sort(6);
        fx.Add("Icon", "Card 2").Mat(card).Still().Size2(0.7f, 0.7f).Pos(0.5f, 0.2f).Velocity(0.2f, 1.4f).DelayAt(0.55f).Life(0.8f).Sort(6);
        Flecks(fx, BloodHot, C("#FFE0D8"), 0.02f, 4);
        fx.Save("Card Effect V2 - Ritual");
    }

    public static void CollectiveRage()
    {
        // veins of rage rush in from every side, then one heavy fist blow
        var fx = new Fx("Card Effect V2 - Collective Rage");
        fx.Add("Backdrop").Col(BloodDark).Size(1.15f);
        fx.Add("Streaks", "Veins In").Col(BloodCore, BloodHot).DelayAt(0f).Count(7, 9).Life(0.28f).Speed(-7f, -9f).Shape(ParticleSystemShapeType.Circle, 2.2f);
        Material fist = Mat("M_SwordGold", "CM_V2_Fist", P("CTX_Fist"), BloodCore, C("#FFB8A8"));
        HeroIcon(fx, fist, 1.9f, 1.9f, -0.3f).Velocity(0.6f, -0.4f);
        Impact(fx, BloodCore, BloodHot, 0f, 1.25f);
        PaintWash(fx, new Color(BloodCore.r, BloodCore.g, BloodCore.b, 0.65f), 0f, 1.15f);
        fx.Add("BlackSplots", "Spatter").Col(C("#8E0A14"), BloodCore).Count(5, 8).Grav(0.5f);
        Flecks(fx, BloodHot, C("#FFE0D8"), 0f, 4);
        fx.Save("Card Effect V2 - Collective Rage");
    }

    // the watercolour blood splash from the Bleed status effect (New Project(20)), laid at random angles
    static void BloodWash(Fx fx, float at, int min, int max, float size)
    {
        Material wash = Mat("M_BasicParticlesAlpha", "CM_V2_BloodWash", "Assets/VFX V3/New Project(20).png", Color.white, Color.white);
        var l = fx.Add("Icon", "Blood Splash").Mat(wash).Still().NoColLife().Col(Color.white).DelayAt(at).Count(min, max).Life(0.7f, 0.95f).Size2(size, size).Sort(4);
        var cd = l.ps.customData; cd.enabled = false;
    }

    // the soft watercolour paint wash from the Bleed status, tinted to the effect
    static Layer PaintWash(Fx fx, Color c, float at, float size = 1f)
    {
        return fx.Add("GradPaint", "Paint Wash").Col(c).DelayAt(at).Size(size);
    }

    // the bubbles from the Poison status, tinted to the effect: rising off the target, or drawn in to it
    static Layer Bubbles(Fx fx, Color a, Color b, float at, float seconds, float rate, bool drawnIn = false)
    {
        return fx.Add(drawnIn ? "BubblesIn" : "Bubbles", "Bubbles").Col(a, b).DelayAt(at).Stream(rate, seconds);
    }

    public static void SerratedWaraxe()
    {
        // a jagged axe chops down
        var fx = new Fx("Card Effect V2 - Serrated Waraxe");
        fx.Add("Backdrop").Col(BloodDark);
        Material axe = Mat("M_SwordGold", "CM_V2_Waraxe", P("CTX_Waraxe"), C("#B8C2D6"), C("#FFFFFF"));
        HeroIcon(fx, axe, 2.1f, 2.1f, 0.9f, 0.15f, 0.15f).Spin(-6.5f).Life(0.75f);
        Impact(fx, BloodCore, BloodHot, 0f, 1.05f);
        fx.Add("SlashDark", "Chop Shadow").Col(C("#14060A")).Rot(-1.25f).Delay(0.25f);
        fx.Add("BlackSplots", "Spatter").Col(C("#8E0A14"), BloodCore).Count(4, 7).Grav(0.6f);
        BloodWash(fx, 0.28f, 2, 3, 2.3f);
        Flecks(fx, BloodHot, C("#E6ECF5"), 0f, 3);
        fx.Save("Card Effect V2 - Serrated Waraxe");
    }

    public static void SerratedWaraxeBite()
    {
        // the bonus hit: serrated teeth snap shut on the target
        var fx = new Fx("Card Effect V2 - Serrated Waraxe Bite");
        fx.Add("Backdrop").Col(BloodDark).Size(0.9f);
        Material teeth = Mat("M_SwordGold", "CM_V2_Teeth", BlueprintTex + "CTX_SerratedEdge.png", C("#E6ECF5"), C("#FFFFFF"));
        fx.Add("Icon", "Upper Jaw").Mat(teeth).Still().Size2(1.9f, 1.1f).Rot(Mathf.PI).Pos(0f, 0.75f).Velocity(0f, -2.2f).Life(0.55f).Sort(5);
        fx.Add("Icon", "Lower Jaw").Mat(teeth).Still().Size2(1.9f, 1.1f).Pos(0f, -0.75f).Velocity(0f, 2.2f).Life(0.55f).Sort(5);
        Impact(fx, BloodCore, BloodHot, 0f, 0.8f, false);
        fx.Add("BlackSplots", "Spatter").Col(C("#8E0A14"), BloodCore).Count(4, 6).Grav(0.6f);
        BloodWash(fx, 0.3f, 1, 2, 1.8f);
        PaintWash(fx, new Color(BloodCore.r, BloodCore.g, BloodCore.b, 0.6f), 0.25f, 0.9f);   // as the jaws close
        fx.Save("Card Effect V2 - Serrated Waraxe Bite");
    }

    public static void Retaliation()
    {
        // a spiked shield raises (block), spikes flare outward
        var fx = new Fx("Card Effect V2 - Retaliation");
        Material shield = Mat("M_ShieldVFX", "CM_V2_SpikedShield", P("CTX_SpikedShield"), BloodCore, BloodHot);
        fx.Add("Shield", "Spiked Shield").Mat(shield).Col(C("#FFB0A4"));
        fx.Add("ShieldRise").Col(BloodHot);
        fx.Add("ShieldStars").Col(C("#FFE0D8"));
        fx.Add("Backdrop").Col(BloodDark).DelayAt(0.3f).Life(1.6f);
        fx.Add("Spikes", "Spikes Out").Col(BloodHot).Delay(0.35f).Size(1.2f);
        fx.Add("Ring").Col(BloodCore).Delay(0.35f).Size(1.1f);
        fx.Save("Card Effect V2 - Retaliation");
    }

    public static void RetaliationCounter()
    {
        // the counter: one quick spike jab at the attacker
        var fx = new Fx("Card Effect V2 - Retaliation Counter");
        fx.Add("Backdrop").Col(BloodDark).Size(0.8f);
        fx.Add("Streaks", "Jab").Col(C("#FFE0D8"), BloodHot).DelayAt(0f).Count(3, 4).Life(0.2f).Speed(-9f, -11f).Shape(ParticleSystemShapeType.Circle, 1.6f, 0f, 70f);
        Impact(fx, BloodCore, BloodHot, -0.1f, 0.8f, false);
        Flecks(fx, BloodHot, C("#FFE0D8"), -0.1f, 3);
        fx.Save("Card Effect V2 - Retaliation Counter");
    }

    // ---------- nature / fungal ----------
    static readonly Color LeafCore = C("#4FAE33"), LeafHot = C("#C8FF6A"), Petal = C("#FF8FB1"), LeafDark = C("#0C1F0E");

    public static void ForestsMight()
    {
        // leaves gather inward, a great oak leaf grows and strikes, petals burst
        var fx = new Fx("Card Effect V2 - Forest's Might");
        fx.Add("Backdrop").Col(LeafDark).Size(1.15f);
        fx.Add("Leaves", "Gathering Leaves").Col(LeafHot, LeafCore).DelayAt(0f).Count(12, 16).Life(0.3f).Speed(-5f, -7f).Shape(ParticleSystemShapeType.Circle, 1.9f);
        Material leaf = Mat("M_SwordGold", "CM_V2_OakLeaf", P("CTX_OakLeaf"), LeafCore, C("#E4FFB0"));
        HeroIcon(fx, leaf, 2.2f, 2.2f, 0.35f);
        Impact(fx, LeafCore, LeafHot, 0f, 1.1f);
        fx.Add("Petals").Col(Petal, C("#FFD1E0")).Count(4, 6);
        fx.Add("Leaves", "Leaf Burst").Col(LeafHot, LeafCore).Count(8, 10);
        Flecks(fx, LeafHot, Petal, 0f, 4);
        fx.Save("Card Effect V2 - Forest's Might");
    }

    public static void EldritchTuber()
    {
        // a gnarled violet tuber, tendrils lash up from below, sickly lime impact
        var fx = new Fx("Card Effect V2 - Eldritch Tuber");
        fx.Add("Backdrop").Col(C("#120A24")).Size(1.1f);
        fx.Add("Streaks", "Tendrils").Col(C("#8B3FD1"), C("#B5D94A")).DelayAt(0.05f).Count(4, 6).Life(0.35f).Speed(6f, 9f).Pos(0f, -1.3f).Shape(ParticleSystemShapeType.Cone, 0.6f, 0f, 30f);
        Material tuber = Mat("M_SwordGold", "CM_V2_Tuber", P("CTX_Tuber"), C("#8B3FD1"), C("#D7B8FF"));
        HeroIcon(fx, tuber, 1.9f, 1.9f).Velocity(0f, 0.5f);
        Impact(fx, C("#8B3FD1"), C("#D8FF7A"), 0f, 1f);
        fx.Add("InkBlops", "Spores").Col(C("#B5D94A"), C("#8B3FD1")).Count(6, 9);
        Flecks(fx, C("#B5D94A"), C("#D7B8FF"), 0f, 4);
        fx.Save("Card Effect V2 - Eldritch Tuber");
    }

    public static void Bezoar()
    {
        // block from a stone: the shield is a faceted pebble, dust falls away
        var fx = new Fx("Card Effect V2 - Bezoar");
        Material stone = Mat("M_ShieldVFX", "CM_V2_BezoarStone", P("CTX_BezoarStone"), C("#9C8A68"), C("#F2E6C8"));
        Bubbles(fx, C("#A8C890"), C("#E6D8B8"), 0f, 0.4f, 30f, true);   // the stone draws the poison in
        fx.Add("Shield", "Stone").Mat(stone).Col(C("#E6D8B8"));
        fx.Add("ShieldRise").Col(C("#D2C4A4"));
        fx.Add("ShieldStars").Col(C("#FFF1C9"));
        fx.Add("Backdrop").Col(C("#1E1626")).DelayAt(0.3f).Life(1.6f);
        fx.Add("InkBlops", "Dust").Col(C("#B8A888"), C("#8A7A66")).Delay(0.2f).Count(6, 9);
        fx.Save("Card Effect V2 - Bezoar");
    }

    // ---------- cards / mind ----------
    static readonly Color Gold = C("#D8B33C"), GoldHot = C("#FFE38A"), Felt = C("#0E2418");

    public static void Brainstorm()
    {
        // a fan of cards appears, then three cards stab in one after another
        var fx = new Fx("Card Effect V2 - Brainstorm");
        fx.Add("Backdrop").Col(Felt).Size(1.15f).Life(1.4f);
        Material fan = Mat("M_SwordGold", "CM_V2_CardFan", BlueprintTex + "CTX_CardFan.png", Gold, C("#FFF4CC"));
        fx.Add("Icon", "Card Fan").Mat(fan).Still().Size2(1.6f, 1.6f).Pos(0f, 1.1f).Life(0.5f).Sort(5);
        Material card = Mat("M_SwordGold", "CM_V2_GoldCard", CardTex, Gold, C("#FFF4CC"));
        (float x, float y, float t)[] stabs = { (-1.6f, 0.9f, 0.3f), (1.6f, 0.9f, 0.42f), (0f, 1.8f, 0.54f) };
        foreach (var s in stabs)
        {
            fx.Add("Icon", "Card Stab").Mat(card).Still().Size2(0.75f, 0.75f).Pos(s.x, s.y).Velocity(-s.x * 6f, -s.y * 6f).DelayAt(s.t - 0.16f).Life(0.16f).Sort(6);
            fx.Add("Flash", "Stab Flash").Col(new Color(1f, 0.9f, 0.6f, 0.45f)).DelayAt(s.t).Size(0.5f);
        }
        Impact(fx, Gold, GoldHot, 0.25f, 1f, false);
        Flecks(fx, Gold, Color.white, 0.25f, 4);
        fx.Save("Card Effect V2 - Brainstorm");
    }

    public static void Disappear()
    {
        // a card is swallowed by black smoke
        var fx = new Fx("Card Effect V2 - Disappear");
        Material card = Mat("M_SwordGold", "CM_V2_VanishCard", CardTex, C("#6A6290"), C("#D8D2F0"));
        fx.Add("Icon", "Card").Mat(card).Still().Size2(1.3f, 1.3f).DelayAt(0f).Life(0.7f).Sort(5);
        fx.Add("InkSplat", "Smoke Gulp").Col(C("#120E1A")).Delay(0.05f).Size(0.9f);
        fx.Add("InkSub", "Smoke Wisps").Col(C("#241B3A"), C("#120E1A")).Delay(0.05f);
        fx.Add("Backdrop").Col(C("#0C0812")).DelayAt(0.25f).Life(1.4f);
        fx.Add("DotsB", "Violet Flecks").Col(C("#B9B4D6")).Delay(0.15f).Count(3, 5);
        fx.Save("Card Effect V2 - Disappear");
    }

    // ===================== area attacks (hit every enemy) =====================
    // Area plays once over the enemy row; Hit stays on each enemy and waits for the area to reach it.

    static void Area(Fx fx, AreaEffect.Anchor anchor, float hitDelay, float travelSpeed, bool fromLeft = true, float leftPad = 2.5f, bool companionRow = false)
    {
        Transform area = fx.root.transform.Find("Area"), hit = fx.root.transform.Find("Hit");
        AreaEffect a = fx.root.AddComponent<AreaEffect>();
        var so = new SerializedObject(a);
        so.FindProperty("area").objectReferenceValue = area;
        so.FindProperty("hit").objectReferenceValue = hit;
        so.FindProperty("anchor").enumValueIndex = (int)anchor;
        so.FindProperty("leftPad").floatValue = leftPad;
        so.FindProperty("hitDelay").floatValue = hitDelay;
        so.FindProperty("travelSpeed").floatValue = travelSpeed;
        so.FindProperty("travelFromLeft").boolValue = fromLeft;
        so.FindProperty("companionRow").boolValue = companionRow;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ===================== enemy attacks that hit every rat =====================
    // Enemies keep their own look, apart from the players': Luke's Enemy (Red) kit (claw marks, black smoke, ink
    // streaks and splots; no rings, spikes or sparkle), red and black (Meothra: her indigo and violet), everything
    // coming from the enemy side, right to left, and a predator's rhythm: a still coil, then the strike. Each rat
    // takes the move's own claw hit, carried inside the sweep and played as the strike reaches it.
    static readonly Color EnemyRed = C("#D0141E"), EnemyHot = C("#FF6A5A");
    static readonly Color MeothraNavy = C("#1E1E3A"), MeothraIndigo = C("#3A2A6A"), MeothraViolet = C("#7A2FB0");
    const string EnemyHits = "Assets/Prefabs/VFX/VFX Prefabs - For Luke/Enemy (Red)/";
    const string MeothraTailTex = PlaceholderDir + "/CTX_MeothraTail.png";

    // the sweep built around a move's own hit; a move without one takes the small claw. The small claw (the common
    // case) keeps the plain name, so the sweep's existing references stay as they are.
    public static GameObject EnemySweepVariant(bool boss, GameObject enemyHit)
    {
        GameObject small = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyHits + "Enemy Claw Small Attack.prefab");
        if (enemyHit == null) enemyHit = small;
        string name = (boss ? "Card Effect V2 - Boss Row Sweep" : "Card Effect V2 - Enemy Row Sweep") + (enemyHit == small ? "" : " - " + enemyHit.name);
        if (boss) BossRowSweep(enemyHit, name); else EnemyRowSweep(enemyHit, name);
        return AssetDatabase.LoadAssetAtPath<GameObject>(OutDir + "/" + name + ".prefab");
    }

    // the hit a sweep carries (the nested prefab under Hit), or null
    public static GameObject CarriedHit(GameObject sweep)
    {
        Transform hit = sweep != null ? sweep.transform.Find("Hit") : null;
        if (hit == null) return null;
        foreach (Transform t in hit)
            if (PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject)) return PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject);
        return null;
    }

    static void CarryHit(Fx fx, GameObject enemyHit)
    {
        fx.Group("Hit");
        var own = (GameObject)PrefabUtility.InstantiatePrefab(enemyHit, fx.into);
        own.transform.localPosition = Vector3.zero;
    }

    public static void EnemyRowSweep(GameObject enemyHit, string name)
    {
        // the coil: black smoke draws in at the enemy end of the row around a red glow; then the strike: the red claw
        // slash with two claw lines beside it crosses the row right to left, flinging ink streaks at the rats and
        // leaving smoke and red spatter; each rat takes the enemy's own claw hit as it passes
        var fx = new Fx(name, 5);
        fx.Group("Area");
        fx.Add("Backdrop").Col(C("#1A0508")).DelayAt(0.05f).Size(2.8f).Life(1.0f);
        fx.Add("ESmoke", "Coil Smoke").Flat().Col(new Color(0.05f, 0.02f, 0.02f, 0.75f)).DelayAt(0f).Count(8, 10).Life(0.32f).Speed(-3f, -4f)
            .Shape(ParticleSystemShapeType.Circle, 1.3f).Pos(7.6f, 0.2f).Sort(3);
        fx.Add("EGlow", "Coil Glow").Col(new Color(1f, 0.05f, 0.05f, 0.5f)).DelayAt(0.02f).Life(0.3f).Size(0.7f).Pos(7.6f, 0.2f);
        fx.Add("SlashDark", "Sweep Shadow").Col(C("#0A0204")).DelayAt(0.12f).Life(0.36f).Size2(19f, 0.5f).Rot(0.06f);
        fx.Add("SlashLight", "Sweep").Col(EnemyHot).DelayAt(0.12f).Life(0.3f).Size2(17f, 0.34f).Rot(0.06f).Pos(0f, 0.05f);
        fx.Add("SlashLight", "Claw Line Upper").Col(EnemyRed).DelayAt(0.135f).Life(0.26f).Size2(15f, 0.16f).Rot(0.06f).Pos(0.6f, 0.4f);
        fx.Add("SlashLight", "Claw Line Lower").Col(EnemyRed).DelayAt(0.15f).Life(0.26f).Size2(14f, 0.14f).Rot(0.06f).Pos(-0.4f, -0.32f);
        fx.Add("EStreaks", "Flung Ink").Flat().Col(Color.black).DelayAt(0.14f).Count(10, 14).Box(16f, 0.6f).Speed(0f, 0f).VelocityRange(-14f, -8f, -1f, 1f).Sort(5);
        fx.Add("EStreaksRed", "Flung Red").Flat().Col(EnemyRed).DelayAt(0.15f).Count(8, 10).Box(16f, 0.6f).Speed(0f, 0f).VelocityRange(-13f, -7f, -1f, 1f).Sort(5);
        fx.Add("Backdrop", "Trail Smoke").Col(new Color(0.08f, 0.02f, 0.02f, 0.5f)).DelayAt(0.16f).Count(10, 12).Life(0.7f, 1f).Size(0.3f).Box(17f, 0.5f).Speed(0f, 0f)
            .VelocityRange(-1.2f, -0.4f, 0.1f, 0.5f).Sort(2);
        fx.Add("InkSplat", "Spatter").Col(C("#5A0A10"), EnemyRed).DelayAt(0.16f).Count(8, 11).Box(17f, 0.4f).Speed(0f, 0f).Grav(0.6f).Size(0.4f);
        CarryHit(fx, enemyHit);
        fx.Root();
        Area(fx, AreaEffect.Anchor.RowCentre, 0.14f, 30f, false, 2.5f, companionRow: true);
        fx.Save(name);
    }

    // one copy of Meothra's tail, rotating about its base (the texture's right end) from raised to lying across the row
    static Layer TailSwipe(Fx fx, string name, float at, Color colour, float raised, float sweepTo, float sweepFor, float life)
    {
        var l = PlainIcon(fx, name, MeothraTailTex, "CM_V2_MeothraTail").Col(colour).DelayAt(at).Life(life).Fade(0.55f)
            .Size2(18f, 4.5f).RotRange(raised, raised).Pos(9.6f, -0.65f);
        var rend = l.ps.GetComponent<ParticleSystemRenderer>();
        rend.pivot = new Vector3(-0.5f, 0f, 0f);   // pin the base
        // no dissolve: its noise cutoff reads as a marbled ribbon instead of fur (the tail's own CM_ copy)
        if (rend.sharedMaterial.GetFloat("_Use_Cutoff") != 0f) { rend.sharedMaterial.SetFloat("_Use_Cutoff", 0f); EditorUtility.SetDirty(rend.sharedMaterial); AssetDatabase.SaveAssetIfDirty(rend.sharedMaterial); }
        // angular speed falls linearly to zero at `sweepFor`, so the tip lands with an ease-out
        float k = sweepFor / life, w0 = 2f * (sweepTo - raised) / sweepFor;
        var r = l.ps.rotationOverLifetime; r.enabled = true; r.separateAxes = false;
        r.z = new ParticleSystem.MinMaxCurve(w0, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(k, 0f, 0f, 0f), new Keyframe(1f, 0f)));
        return l;
    }

    public static void BossRowSweep(GameObject enemyHit, string name)
    {
        // Meothra: the screen sinks to her indigo and pipe smoke curls in; her tail rises at the right and sweeps down
        // across the row, kicking up a wave of smoke and flinging violet ink; then four violet claw marks tear across
        // every rat and each takes her claw hit
        var fx = new Fx(name, 5);
        fx.Group("Area");
        fx.Add("Backdrop", "Screen Dim").Col(new Color(0.03f, 0.01f, 0.05f, 0.85f)).DelayAt(0f).Size(5f).Life(1.6f);
        fx.Add("Backdrop", "Pipe Smoke").Col(new Color(0.2f, 0.15f, 0.36f, 0.55f)).DelayAt(0f).Stream(16f, 0.45f).Life(1f, 1.3f).Size(0.32f).Speed(0f, 0f)
            .VelocityRange(-2.2f, -1.2f, 0.2f, 0.7f).Box(1.5f, 2f).Pos(8f, 0.9f).Sort(2);
        // the swipe, with three lagging copies as its motion smear (+ is clockwise: the tip starts raised)
        float raised = 1.3f, down = -0.12f;
        TailSwipe(fx, "Tail Smear 3", 0.155f, new Color(0.16f, 0.12f, 0.3f, 0.1f), raised, down, 0.24f, 0.55f).Sort(6);
        TailSwipe(fx, "Tail Smear 2", 0.13f, new Color(0.16f, 0.12f, 0.3f, 0.18f), raised, down, 0.24f, 0.55f).Sort(7);
        TailSwipe(fx, "Tail Smear 1", 0.105f, new Color(0.15f, 0.11f, 0.28f, 0.3f), raised, down, 0.24f, 0.55f).Sort(8);
        TailSwipe(fx, "Tail", 0.08f, MeothraNavy, raised, down, 0.24f, 0.6f).Sort(9);
        fx.Add("Backdrop", "Smoke Wave").Col(new Color(0.1f, 0.07f, 0.2f, 0.6f)).DelayAt(0.2f).Stream(70f, 0.2f).Life(0.8f, 1.1f).Size(0.3f).Speed(0f, 0f)
            .VelocityRange(-3f, -1f, 0.3f, 1.2f).Box(17f, 0.4f).Pos(0f, -0.9f).Sort(3);
        fx.Add("EStreaks", "Flung Ink").Flat().Col(MeothraIndigo).DelayAt(0.22f).Count(12, 16).Box(16f, 0.8f).Speed(0f, 0f).VelocityRange(-15f, -8f, -0.6f, 1.6f).Sort(5);
        fx.Add("EStreaksRed", "Flung Violet").Flat().Col(MeothraViolet).DelayAt(0.24f).Count(8, 12).Box(16f, 0.8f).Speed(0f, 0f).VelocityRange(-14f, -7f, -0.6f, 1.6f).Sort(5);
        // the rake
        Material rake = Mat("M_BasicParticlesAlpha", "CM_V2_BossRake", "Assets/VFX V3/Card Effects V2/Textures/CTX_ClawRake.png", Color.white, Color.white);
        float[] ys = { -0.95f, -0.32f, 0.32f, 0.95f };
        for (int i = 0; i < ys.Length; i++)
        {
            var l = fx.Add("Icon", "Rake " + (i + 1)).Mat(rake).Still().NoColLife().Col(i % 2 == 0 ? C("#2A0B3A") : MeothraViolet).DelayAt(0.4f + i * 0.04f).Life(0.5f).Size2(20f, 1.1f).Rot(0.1f).Pos(0f, ys[i]).Sort(10);
            var cd = l.ps.customData; cd.enabled = false;
        }
        fx.Add("Flash", "Rake Flash").Col(new Color(0.75f, 0.45f, 1f, 0.6f)).DelayAt(0.42f).Size(2.6f);
        fx.Add("InkSplat", "Ink Spatter").Col(C("#14051F"), C("#4A1A6A")).DelayAt(0.44f).Count(12, 16).Box(18f, 1.8f).Speed(0f, 0f).Grav(0.5f).Size(0.5f);
        CarryHit(fx, enemyHit);
        fx.Root();
        Area(fx, AreaEffect.Anchor.RowCentre, 0.42f, 34f, false, 2.5f, companionRow: true);
        Shake(fx, 0.22f, 0.3f);   // the tail lands
        Shake(fx, 0.42f, 0.5f);   // the rake
        fx.Save(name);
    }

    // ===================== small cues for basic, frequent things =====================

    public static void BasicCues()
    {
        // a card in hand fired on its own (discarded, drawn, exhausted, end of turn)
        var trig = new Fx("Card Effect V2 - Card Triggered", 2);
        trig.Add("Glow").Col(new Color(1f, 0.85f, 0.45f, 0.6f)).DelayAt(0f).Size(0.85f);
        trig.Add("Ring").Col(GoldHot).DelayAt(0f).Size(0.8f);
        trig.Add("Spikes").Col(Gold).DelayAt(0f).Size(0.55f);
        trig.Add("DotsB").Col(GoldHot).DelayAt(0f).Count(3, 5);
        trig.Save("Card Effect V2 - Card Triggered");

        // a card's cost or numbers changed
        var changed = new Fx("Card Effect V2 - Card Changed", 2);
        changed.Add("Glow").Col(new Color(1f, 0.92f, 0.6f, 0.45f)).DelayAt(0f).Life(0.5f).Size(0.7f);
        changed.Add("DotsA", "Sparkles").Col(GoldHot, Color.white).DelayAt(0f).Count(4, 6).Size(0.8f);
        changed.Add("MagicStar", "Twinkle").Col(GoldHot).DelayAt(0.05f).Size(0.4f);
        changed.Save("Card Effect V2 - Card Changed");

        // a card stays in hand at the end of the turn
        var kept = new Fx("Card Effect V2 - Card Retained", 2);
        kept.Add("Glow").Col(new Color(0.5f, 0.85f, 0.9f, 0.5f)).DelayAt(0f).Life(0.8f).Size(0.8f);
        kept.Add("Ring", "Soft Halo").Mat(Mat("M_BasicHollowCircle", "CM_V2_SoftRingThick", "Assets/VFX V3/TX_Circle_40.png")).Col(new Color(0.75f, 0.96f, 1f, 0.6f)).DelayAt(0.05f).Size(0.8f);
        kept.Add("DotsB").Col(C("#BFF4FF")).DelayAt(0.05f).Count(2, 4);
        kept.Save("Card Effect V2 - Card Retained");

        // Elan gained mid-turn, on the emblem
        var mana = new Fx("Card Effect V2 - Mana Gain", 2);
        mana.Add("Flash").Col(new Color(0.6f, 0.8f, 1f, 0.6f)).DelayAt(0f).Size(0.7f);
        // a soft glowing halo (the revive's) rather than the crisp hit ring
        mana.Add("Ring", "Soft Halo").Mat(Mat("M_BasicHollowCircle", "CM_V2_SoftRingGlow", "Assets/VFX V2/Textures/Circle_14.png")).Col(new Color(0.7f, 0.86f, 1f, 0.7f)).DelayAt(0f).Size(1.1f);
        mana.Add("DotsB", "Rising Sparks").Col(C("#BFE0FF"), Color.white).DelayAt(0f).Count(6, 9).VelocityRange(-0.4f, 0.4f, 1.2f, 2f).Size(0.8f);
        mana.Save("Card Effect V2 - Mana Gain");

        // ink trails from where the Elan came from into the emblem (steered by HomingParticles at run time)
        var trails = new Fx("Card Effect V2 - Mana Ink Trails", 2);
        trails.Add("InkSplat", "Source Splash").Col(C("#2A3A6A")).DelayAt(0f).Size(0.35f);
        foreach (var t in new[] { trails.Add("MoneyTrails", "Ink Trails").Col(C("#7FB8FF"), C("#1E2A50")).DelayAt(0f).Count(4, 6).Life(1.2f).World(),
                                  trails.Add("MoneyStreaks", "Ink Streaks").Col(C("#BFE0FF"), C("#3A5A9A")).DelayAt(0f).Count(3, 4).Life(1.2f).World() })
        {
            var tr = t.ps.trails; tr.inheritParticleColor = true;   // the trail takes the particle's Elan blue
            var h = t.go.AddComponent<HomingParticles>();
            var hs = new SerializedObject(h); hs.FindProperty("travelTime").floatValue = 0.5f; hs.ApplyModifiedPropertiesWithoutUndo();
        }
        trails.Save("Card Effect V2 - Mana Ink Trails");
    }

    // the small per-enemy hit, coloured to the effect; its beat is at t=0 (the area adds the wait)
    static void TargetHit(Fx fx, Color core, Color hot, float size = 0.75f)
    {
        fx.Group("Hit");
        fx.Add("Backdrop").Col(new Color(core.r * 0.12f, core.g * 0.12f, core.b * 0.12f, 1f)).DelayAt(0f).Life(0.8f).Size(0.6f);
        fx.Add("Glow").Col(new Color(core.r, core.g, core.b, 0.5f)).DelayAt(0f).Size(size);
        fx.Add("Ring").Col(hot).DelayAt(0f).Size(size);
        fx.Add("Spikes").Col(hot).DelayAt(0f).Size(size);
        fx.Add("DotsA").Col(core).DelayAt(0f).Count(2, 3);
        fx.Add("DotsB").Col(hot).DelayAt(0f).Count(1, 2);
    }

    static readonly string FireSheet = "Assets/VFX V3/IMG_1381(3)(1).png";       // the ember / flame-wisp sheet from Pureknife

    static readonly Color ArcCore = C("#C83CE0"), ArcHot = C("#FFD98A"), ArcPink = C("#FF6FB8"), ArcDark = C("#16061F");

    public static void FlamethrowerArea()
    {
        // arcane fire gathers at the left edge of the screen, then a jet roars all the way across to the right edge
        var fx = new Fx("Card Effect V2 - Flamethrower", 7);
        fx.Group("Area");
        // the arcane fire is the ember tile sheet, recoloured violet-pink
        Material blaze = Mat("M_Embers", "CM_V2_EmbersArcane", null, ArcCore, C("#FFD6F4"));
        fx.Add("Backdrop", "Haze").Col(ArcDark).DelayAt(0f).Life(1.8f).Size(1.3f).Pos(1.2f, 0f);
        fx.Add("Embers", "Gathering Flames").Mat(blaze).Col(ArcCore, ArcPink).DelayAt(0f).Count(26, 32).Life(0.4f).Size(2.2f)
            .Speed(-4.5f, -6f).Shape(ParticleSystemShapeType.Circle, 1.8f).Pos(1.2f, 0f);
        fx.Add("StarIcons", "Gathering Sparks").Col(ArcHot, Color.white).DelayAt(0f).Count(5, 7).Life(0.4f).Speed(-3f, -4f).Shape(ParticleSystemShapeType.Circle, 1.6f).Pos(1.2f, 0f);
        fx.Add("Glow", "Gathering Glow").Col(new Color(0.8f, 0.25f, 1f, 0.6f)).DelayAt(0.05f).Life(0.5f).Size(1.2f).Pos(1.2f, 0f);
        fx.Add("Flash", "Ignite").Col(new Color(1f, 0.85f, 0.95f, 0.75f)).DelayAt(0.38f).Life(0.25f).Size(1.3f).Pos(1.2f, 0f);
        // the jet: arcane ember flames + wisps + streaks streaming right; long enough to cross the whole screen
        fx.Add("Embers", "Jet Flames").Mat(blaze).Col(ArcCore, ArcPink).DelayAt(0.4f).Stream(60f, 0.7f).Life(1.15f, 1.3f)
            .Speed(0f, 0f).Grav(0f).VelocityRange(19f, 21f, -0.6f, 0.6f).Grow(0.5f, 2.2f).RotRange(-1.75f, -1.35f).Size(5f).Pos(1.2f, 0f).World().Sort(6);
        fx.Add("Embers", "Jet Wisps").Mat(blaze).Col(C("#7A2AD0"), ArcPink).DelayAt(0.4f).Stream(90f, 0.7f).Life(1.05f, 1.2f)
            .Speed(0f, 0f).Grav(0f).VelocityRange(20f, 23f, -1.1f, 1.1f).Grow(0.6f, 1.8f).Size(3.4f).Pos(1.2f, 0f).World().Sort(7);
        fx.Add("Streaks", "Gold Streaks").Col(ArcHot, Color.white).DelayAt(0.42f).Stream(40f, 0.65f).Life(0.8f, 0.9f)
            .Speed(0f, 0f).VelocityRange(25f, 28f, -0.4f, 0.4f).Pos(1.2f, 0f).World().Sort(8);
        fx.Add("StarIcons", "Star Embers").Col(ArcHot, Color.white).DelayAt(0.5f).Stream(30f, 0.6f).Life(0.8f, 1.1f)
            .Speed(0f, 0f).VelocityRange(14f, 20f, -0.8f, 1.2f).Pos(1.2f, 0f).World().Sort(8);
        TargetHit(fx, ArcCore, ArcHot, 0.9f);
        fx.Add("Embers", "Catch Fire").Mat(blaze).Col(ArcCore, ArcPink).DelayAt(0f).Count(8, 10).Size(1.8f).Velocity(0f, 1.4f);
        fx.Root();
        Area(fx, AreaEffect.Anchor.ScreenLeft, 0.45f, 20f, true);
        fx.Save("Card Effect V2 - Flamethrower");
    }

    public static void DetonateArea()
    {
        // one bomb over the centre of the row: fuse, a huge flash, a shockwave that rolls over every enemy
        var fx = new Fx("Card Effect V2 - Detonate", 6);
        fx.Group("Area");
        fx.Add("Backdrop").Col(C("#1A0A08")).DelayAt(0f).Size(2.6f).Life(1.8f);
        Material bomb = Mat("M_SwordGold", "CM_V2_Bomb", P("CTX_Bomb"), C("#3A2A30"), C("#7A6A70"));
        fx.Add("Icon", "Bomb").Mat(bomb).Still().Size2(2f, 2f).Life(0.6f).Sort(5);
        fx.Add("StarIcons", "Fuse Spark").Col(C("#FFE38A")).DelayAt(0.05f).Life(0.3f, 0.4f).Count(3, 4).Pos(0.5f, 0.85f).Speed(0.5f, 1.5f);
        fx.Add("Flash", "White Flash").Col(new Color(1f, 0.95f, 0.85f, 0.9f)).DelayAt(0.45f).Life(0.2f).Size(3.2f);
        fx.Add("Glow", "Fire Glow").Col(new Color(1f, 0.45f, 0.1f, 0.6f)).DelayAt(0.45f).Life(0.6f).Size(3f);
        fx.Add("Ring", "Shockwave").Col(C("#FFF4D6")).DelayAt(0.45f).Life(0.45f).Size(4.5f);
        fx.Add("Ring", "Shockwave 2").Col(FireCore).DelayAt(0.55f).Life(0.5f).Size(6f);
        fx.Add("SpikesBig", "Blast Rays").Col(FireCore).DelayAt(0.45f).Size(2.4f);
        fx.Add("Embers", "Fireball").Col(Color.white, C("#FFD27A")).DelayAt(0.45f).Count(14, 18).Life(0.6f, 1f).Size(3.4f).Speed(2f, 7f).Grow(0.5f, 1.4f);
        fx.Add("Embers", "Blast Wisps").Col(FireCore, FireHot).DelayAt(0.45f).Count(30, 40).Size(2f).Speed(4f, 12f);
        fx.Add("Backdrop", "Smoke").Col(C("#3A3436", 0.8f)).DelayAt(0.8f).Size(1.4f).Velocity(0f, 0.8f);
        TargetHit(fx, FireCore, C("#FFF4D6"), 0.9f);
        fx.Root();
        Area(fx, AreaEffect.Anchor.RowCentre, 0.5f, 18f, false);
        fx.Save("Card Effect V2 - Detonate");
    }

    public static void BoomflaskArea()
    {
        // a flask tumbles down onto the middle of the row and shatters; fire spreads outward along the floor
        var fx = new Fx("Card Effect V2 - Boomflask", 6);
        fx.Group("Area");
        Material flask = Mat("M_SwordGold", "CM_V2_Flask", P("CTX_Flask"), C("#5FC8E8"), C("#E8FBFF"));
        fx.Add("Icon", "Flask").Mat(flask).Still().NoColLife().Size2(1.5f, 1.5f).Pos(0f, 2.4f).DelayAt(0f).Life(0.36f).Lunge(0f, -14f, 1f).Spin(-8f).Sort(6);
        fx.Add("Backdrop").Col(C("#1A0A08")).DelayAt(0.2f).Size(2.2f).Life(1.5f);
        fx.Add("Flash").Col(new Color(0.8f, 0.95f, 1f, 0.7f)).DelayAt(0.36f).Life(0.18f).Size(1.6f);
        fx.Add("Streaks", "Glass Shards").Col(C("#E8FBFF"), C("#9FE3FF")).DelayAt(0.36f).Count(10, 14).Size(0.9f).Speed(5f, 11f);
        PaintWash(fx, new Color(0.37f, 0.78f, 0.91f, 0.55f), 0.36f, 1.8f);   // the flask's liquid splashes out
        Bubbles(fx, C("#E8FBFF"), C("#5FC8E8"), 0.36f, 0.5f, 40f);
        fx.Add("Embers", "Spreading Fire").Col(Color.white, C("#FFD27A")).DelayAt(0.38f).Stream(60f, 0.4f).Life(0.6f, 0.9f).Size(2.2f)
            .Speed(0f, 0f).VelocityRange(-12f, 12f, 0.2f, 1.2f).Grow(0.4f, 1.3f).World().Sort(6);
        fx.Add("Ring", "Burst Ring").Col(FireCore).DelayAt(0.38f).Size(3f);
        TargetHit(fx, FireCore, FireHot, 0.8f);
        fx.Add("Embers", "Catch Fire").Col(FireCore, FireHot).DelayAt(0f).Count(6, 8).Size(1.5f).Velocity(0f, 1.3f);
        fx.Root();
        Area(fx, AreaEffect.Anchor.RowCentre, 0.42f, 12f, false);
        fx.Save("Card Effect V2 - Boomflask");
    }

    public static void GunslingerArea()
    {
        // a muzzle flash at the left of the row, then a spray of gold shots rips across every enemy
        var fx = new Fx("Card Effect V2 - Gunslinger", 5);
        fx.Group("Area");
        fx.Add("MagicStar", "Muzzle Flash").Col(GoldHot).DelayAt(0.05f).Size(1.4f);
        fx.Add("Flash").Col(new Color(1f, 0.9f, 0.6f, 0.6f)).DelayAt(0.1f).Life(0.2f).Size(0.9f);
        fx.Add("Streaks", "Shots").Col(GoldHot, Color.white).DelayAt(0.12f).Stream(34f, 0.45f).Life(0.65f).Size(2.2f).Speed(0f, 0f)
            .VelocityRange(30f, 34f, -0.7f, 0.7f).World().Sort(7);
        fx.Add("Glow", "Muzzle Glow").Col(new Color(1f, 0.85f, 0.4f, 0.6f)).DelayAt(0.1f).Life(0.5f).Size(0.9f);
        fx.Add("StarIcons", "Shell Casings").Col(Gold).DelayAt(0.15f).Count(4, 6).Speed(1f, 3f).Grav(0.8f);
        TargetHit(fx, Gold, GoldHot, 0.7f);
        fx.Add("MagicStar", "Bullet Pop").Col(GoldHot).DelayAt(0f).Size(0.7f);
        fx.Root();
        Area(fx, AreaEffect.Anchor.ScreenLeft, 0.15f, 32f, true);
        fx.Save("Card Effect V2 - Gunslinger");
    }

    static readonly Color Water = C("#3FB7D9"), WaterHot = C("#C8F4FF"), Deep = C("#08202E");

    public static void EbbAndFlowArea()
    {
        // a ribbon of water snakes across the screen in a flowing S-curve, foam breaking over each enemy
        var fx = new Fx("Card Effect V2 - Ebb and Flow", 5);
        fx.Group("Area");
        fx.Add("Backdrop").Col(Deep).DelayAt(0f).Size(1.4f).Life(1.4f).Pos(1f, 0f);
        fx.Add("InkSplat", "Rising Water").Col(Water).DelayAt(0f).Size(0.7f).Life(0.6f).Pos(1f, 0f);
        // two heads weave through each other; each leaves a soft ribbon behind it (birth sub-emitters)
        for (int k = 0; k < 2; k++)
        {
            float phase = k * 0.5f;
            var head = fx.Add("Glow", k == 0 ? "Current A" : "Current B").Col(new Color(0.75f, 0.95f, 1f, 0.9f)).DelayAt(0.12f + k * 0.05f)
                .Count(1, 1).Life(1.05f).Size(0.45f).Pos(1f, 0f).Weave(21f, k == 0 ? 0.9f : -0.7f, 2.5f, phase).World().Sort(8);
            var foam = fx.Add("DotsB", "Foam Trail").Col(WaterHot).DelayAt(0f).Stream(30f, 1f).Life(0.5f, 0.9f).Speed(0f, 0.6f).World().Sort(8);
            head.PaintTrail(Mat("M_LineWatercolour", "CM_V2_WaterTrail", null, WaterHot, Water), k == 0 ? 1.7f : 1.2f, 0.6f, k == 0 ? new Color(0.35f, 0.8f, 1f) : new Color(0.2f, 0.55f, 0.9f));
            head.TrailWith(foam);
        }
        TargetHit(fx, Water, WaterHot, 0.75f);
        fx.Add("InkSub", "Splash").Col(WaterHot, Water).DelayAt(0f).Size(0.6f);
        PaintWash(fx, new Color(Water.r, Water.g, Water.b, 0.55f), 0f, 0.8f);
        Bubbles(fx, WaterHot, Water, 0f, 0.5f, 30f);
        fx.Root();
        Area(fx, AreaEffect.Anchor.ScreenLeft, 0.25f, 21f, true);
        fx.Save("Card Effect V2 - Ebb and Flow");
    }

    public static void LashOutArea()
    {
        // a crimson sword tears across the whole screen, a slash trail ripping open behind it
        var fx = new Fx("Card Effect V2 - Lash Out", 5);
        fx.Group("Area");
        fx.Add("Backdrop").Col(BloodDark).DelayAt(0f).Size(1.3f).Life(1f).Pos(1f, 0f);
        Material blade = Mat("M_SwordGold", "CM_V2_BloodBlade", "Assets/VFX V3/Card Effects V2/Textures/CTX_SwordSilhouette.png", BloodCore, C("#FFC2B8"));
        var sword = fx.Add("Icon", "Sword").Mat(blade).Still().NoColLife().Size2(2.4f, 1.8f).Rot(0f).Pos(1f, 0.05f)
            .DelayAt(0.08f).Count(1, 1).Life(0.62f).VelocityRange(34f, 34f, 0f, 0f).World().Sort(9);
        var sparks = fx.Add("DotsA", "Blood Sparks").Col(BloodHot).DelayAt(0f).Stream(25f, 1f).Life(0.3f, 0.6f).Speed(0.5f, 2f).World().Sort(8);
        sword.PaintTrail(Mat("M_LineRender2", "CM_V2_BloodSlashTrail", null, BloodHot, C("#FFC2B8")), 1.5f, 0.34f, new Color(1f, 0.22f, 0.2f));
        sword.TrailWith(sparks);
        TargetHit(fx, BloodCore, BloodHot, 0.75f);
        fx.Add("SlashLight", "Cut").Col(BloodHot).DelayAt(0f).Life(0.2f).Size2(2.4f, 0.16f).Rot(-0.5f);
        fx.Add("BlackSplots", "Spatter").Col(C("#8E0A14"), BloodCore).DelayAt(0f).Count(3, 5).Grav(0.6f);
        fx.Root();
        Area(fx, AreaEffect.Anchor.ScreenLeft, 0.1f, 34f, true);
        fx.Save("Card Effect V2 - Lash Out");
    }

    public static void SlobberArea()
    {
        // a big glob of drool flies up and rains down over the row, splatting on everyone
        var fx = new Fx("Card Effect V2 - Slobber", 5);
        fx.Group("Area");
        fx.Add("InkSplat", "Drool Glob").Col(C("#C9E85A")).DelayAt(0f).Life(0.45f).Size(0.9f).Pos(0f, 2.8f);
        fx.Add("Backdrop").Col(C("#14200A")).DelayAt(0.1f).Size(2.6f).Life(1.2f);
        fx.Add("InkSplat", "Drool Rain").Col(C("#E3F58A"), C("#9CC23A")).DelayAt(0.15f).Count(30, 36).Life(0.55f, 0.75f)
            .Box(19f, 0.5f).Pos(0f, 1.9f).Speed(0f, 0f).Grav(0.9f).Size(0.3f).RotRange(-3.1f, 3.1f);
        fx.Add("InkBlops", "Drool Drops").Col(C("#E3F58A"), C("#9CC23A")).DelayAt(0.15f).Count(20, 26).Life(0.6f, 0.9f).Box(19f, 0.5f).Pos(0f, 1.9f).Speed(0f, 0f).Grav(0.8f).Size(3.2f);
        TargetHit(fx, C("#9CC23A"), C("#F0FFB0"), 0.7f);
        fx.Add("InkSplat", "Splat").Col(C("#B5D94A")).DelayAt(0f).Size(0.55f);
        PaintWash(fx, new Color(0.61f, 0.76f, 0.23f, 0.55f), 0f, 0.8f);
        Bubbles(fx, C("#E3F58A"), C("#9CC23A"), 0.05f, 0.4f, 25f);   // the drool fizzes
        fx.Root();
        Area(fx, AreaEffect.Anchor.RowCentre, 0.5f, 0f);
        fx.Save("Card Effect V2 - Slobber");
    }

    // ===================== frequently played single-target cards =====================

    static void Ratslap(string pack, Color core, Color hot, Color dark)
    {
        // a paw swipes in sideways and smacks
        var fx = new Fx("Card Effect V2 - Ratslap " + pack);
        fx.Add("Backdrop").Col(dark).Size(0.9f);
        Material paw = Mat("M_SwordGold", "CM_V2_Paw_" + pack, P("CTX_Paw"), core, hot);
        fx.Add("Icon", "Paw").Mat(paw).Still().Size2(1.5f, 1.5f).Rot(-0.5f).Pos(-1.1f, 0.15f).Life(0.65f).Lunge(9f, -0.6f, 0.45f).Sort(5);
        fx.Add("SlashLight", "Slap Smear").Col(hot).DelayAt(0.2f).Life(0.18f).Size2(2.6f, 0.2f).Rot(0.05f).Pos(-0.2f, 0.05f);
        Impact(fx, core, hot, -0.02f, 0.85f, false);
        Flecks(fx, core, hot, -0.02f, 3);
        fx.Save("Card Effect V2 - Ratslap " + pack);
    }

    // The slap, upgraded for the pack cards that earn it (the starter Ratslaps keep the basic slash).

    public static void WristFlick()
    {
        // a backhand flick: forehand and backhand smears cross, and two cards flick off the hit (the two discards)
        var fx = new Fx("Card Effect V2 - Wrist Flick");
        fx.Add("Backdrop").Col(Felt).Size(0.95f);
        Material paw = Mat("M_SwordGold", "CM_V2_Paw_Discard", P("CTX_Paw"), Gold, GoldHot);
        fx.Add("Icon", "Paw").Mat(paw).Still().Size2(1.6f, 1.6f).Rot(-0.6f).Pos(-1.1f, 0.25f).Life(0.6f).Lunge(10f, -1.1f, 0.42f).Sort(5);
        fx.Add("SlashLight", "Forehand Smear").Col(GoldHot).DelayAt(0.18f).Life(0.18f).Size2(2.8f, 0.22f).Rot(0.35f).Pos(-0.1f, 0.05f);
        fx.Add("SlashLight", "Backhand Smear").Col(Gold).DelayAt(0.26f).Life(0.18f).Size2(2.4f, 0.18f).Rot(-0.45f).Pos(0.1f, -0.05f);
        Material card = Mat("M_SwordGold", "CM_V2_FlickedCard", CardTex, GoldHot, Gold);
        fx.Add("Icon", "Flicked Cards").Mat(card).Still().NoColLife().Col(Color.white).DelayAt(0.24f).Count(2, 2).Life(0.55f, 0.7f).Size2(0.5f, 0.7f)
            .VelocityRange(-2.6f, 2.6f, 1.6f, 2.8f).Grav(0.9f).Spin(7f).Sort(7);
        Impact(fx, Gold, GoldHot, 0f, 0.95f, false);
        Flecks(fx, Gold, GoldHot, 0f, 4);
        fx.Save("Card Effect V2 - Wrist Flick");
    }

    public static void ComboPiece()
    {
        // a one-two: a jab from one side, then the follow-up slap from the other lands harder with a star
        var fx = new Fx("Card Effect V2 - Combo Piece");
        fx.Add("Backdrop").Col(Indigo).Size(0.95f);
        Material paw = Mat("M_SwordGold", "CM_V2_Paw_Brawler", P("CTX_Paw"), Steel, C("#FFD27A"));
        fx.Add("Icon", "Jab Paw").Mat(paw).Still().Size2(1.3f, 1.3f).Rot(-0.4f).Pos(-1.0f, 0.1f).DelayAt(0f).Life(0.38f).Lunge(8f, -0.3f, 0.4f).Sort(5);
        var second = fx.Add("Icon", "Follow-up Paw").Mat(paw).Still().Size2(1.6f, 1.6f).Rot(0.5f).Pos(1.15f, 0.2f).DelayAt(0.2f).Life(0.5f).Lunge(-10f, -0.8f, 0.42f).Sort(6);
        second.ps.GetComponent<ParticleSystemRenderer>().flip = new Vector3(1f, 0f, 0f);   // the other paw
        fx.Add("SlashLight", "Jab Smear").Col(SteelHot).DelayAt(0.12f).Life(0.14f).Size2(1.8f, 0.16f).Rot(0.1f).Pos(-0.2f, 0f);
        fx.Add("SlashLight", "Slap Smear").Col(C("#FFD27A")).DelayAt(0.36f).Life(0.18f).Size2(2.8f, 0.22f).Rot(-0.2f).Pos(0.2f, 0.05f);
        Impact(fx, Steel, SteelHot, -0.08f, 0.65f, false);
        Impact(fx, Steel, C("#FFD27A"), 0.16f, 1.0f, true);
        fx.Add("MagicStar", "Combo Star").Col(C("#FFD27A")).Delay(0.16f).Size(0.75f).Sort(7);
        Flecks(fx, Steel, C("#FFD27A"), 0.16f, 4);
        fx.Save("Card Effect V2 - Combo Piece");
    }

    public static void Frenzy()
    {
        // a frenzied flurry: the paw rakes in and three quick slaps land at wild angles, scattering blood
        var fx = new Fx("Card Effect V2 - Frenzy");
        fx.Add("Backdrop").Col(BloodDark).Size(1.0f);
        // the clawed paw: its own material (the shared CM_V2_Paw carries the plain paw texture)
        Material paw = Mat("M_SwordGold", "CM_V2_PawClaws", P("CTX_PawClaws"), BloodCore, BloodHot);
        fx.Add("Icon", "Paw").Mat(paw).Still().Size2(1.5f, 1.5f).Rot(-0.5f).Pos(-1.0f, 0.3f).Life(0.55f).Lunge(9f, -1.2f, 0.45f).Sort(5);
        // three distinct slaps: wide apart in angle and a beat apart in time
        fx.Add("SlashLight", "Slap 1").Col(BloodHot).DelayAt(0.14f).Life(0.18f).Size2(3.0f, 0.3f).Rot(0.9f).Pos(-0.2f, 0.15f);
        fx.Add("SlashLight", "Slap 2").Col(C("#FFC2B8")).DelayAt(0.24f).Life(0.18f).Size2(2.8f, 0.28f).Rot(-0.8f).Pos(0.15f, -0.05f);
        fx.Add("SlashLight", "Slap 3").Col(BloodHot).DelayAt(0.34f).Life(0.2f).Size2(3.3f, 0.32f).Rot(0.1f).Pos(0f, 0.02f);
        Impact(fx, BloodCore, BloodHot, 0.02f, 0.9f, false);
        PaintWash(fx, new Color(BloodCore.r, BloodCore.g, BloodCore.b, 0.6f), 0.14f, 1f);   // from the first slap
        fx.Add("BlackSplots", "Blood Spatter").Col(C("#8E0A14"), BloodCore).Delay(0.02f).Count(11, 15).Size(1.4f).Grav(0.9f).Sort(4);
        Flecks(fx, BloodCore, BloodHot, 0.02f, 4);
        fx.Save("Card Effect V2 - Frenzy");
    }

    public static void DoubleSlash()
    {
        // a horizontal cut; the second hit mirrors it back the other way
        var fx = new Fx("Card Effect V2 - Double Slash");
        fx.root.AddComponent<AlternateFlip>();
        fx.Add("Backdrop").Col(Indigo).Size(0.95f);
        fx.Add("SlashDark", "Cut Shadow").Col(C("#0A0A16")).DelayAt(0.05f).Life(0.3f).Size2(4.6f, 0.13f).Rot(0.18f).Pos(0f, 0f);
        fx.Add("SlashLight", "Cut").Col(SteelHot).DelayAt(0.05f).Life(0.26f).Size2(3.8f, 0.12f).Rot(0.18f).Pos(0f, 0.04f);
        Material blade = Mat("M_SwordGold", "CM_V2_SteelBlade", "Assets/VFX V3/Card Effects V2/Textures/CTX_SwordSilhouette.png", Steel, SteelHot);
        fx.Add("Icon", "Blade").Mat(blade).Still().NoColLife().Size2(1.8f, 1.4f).Rot(-0.18f).Pos(-1.6f, -0.3f).DelayAt(0.02f).Life(0.34f).Lunge(11f, 2f, 0.9f).Sort(6);
        Impact(fx, Steel, SteelHot, -0.12f, 0.85f, false);
        Flecks(fx, Steel, Color.white, -0.12f, 3);
        fx.Save("Card Effect V2 - Double Slash");
    }

    public static void TwoSidedBlade()
    {
        // a double-ended blade spins across the target like a thrown saw
        var fx = new Fx("Card Effect V2 - Two-sided Blade");
        fx.Add("Backdrop").Col(Felt).Size(0.95f);
        Material dbl = Mat("M_SwordGold", "CM_V2_DoubleBlade", P("CTX_DoubleBlade"), Gold, GoldHot);
        fx.Add("Icon", "Spinning Blade").Mat(dbl).Still().Size2(1.9f, 1.9f).Pos(-1.4f, 0.1f).Life(0.75f).Lunge(9f, 0f, 0.5f).Spin(-16f).Sort(5);
        fx.Add("Ring", "Spin Trail").Col(GoldHot).DelayAt(0.15f).Life(0.3f).Size(1.1f);
        Impact(fx, Gold, GoldHot, 0f, 0.95f);
        Flecks(fx, Gold, Color.white, 0f, 4);
        fx.Save("Card Effect V2 - Two-sided Blade");
    }

    public static void Thwack()
    {
        // a hammer drops from above and slams, kicking up dust
        var fx = new Fx("Card Effect V2 - Thwack");
        fx.Add("Backdrop").Col(LeafDark).Size(1f);
        Material ham = Mat("M_HammerRed", "CM_V2_Hammer", "Assets/VFX V3/IMG_1381(3)(4).png", C("#B07A4A"), C("#F2D2A8"));
        fx.Add("Icon", "Hammer").Mat(ham).Still().Size2(1.9f, 1.9f).Rot(0.6f).Pos(-0.2f, 1.8f).Life(0.7f).Lunge(0.3f, -11f, 0.45f).Spin(4f).Sort(5);
        Impact(fx, LeafCore, LeafHot, 0.02f, 1.1f);
        fx.Add("Ring", "Slam Wave").Col(C("#E8D8B0")).Delay(0.08f).Size(1.8f);
        fx.Add("InkBlops", "Dust").Col(C("#B8A888"), C("#7A6A56")).DelayAt(0.3f).Count(8, 10).Box(1.6f, 0.2f).Pos(0f, -0.4f).Speed(1f, 3f);
        fx.Add("Leaves").Col(LeafHot, LeafCore).DelayAt(0.3f).Count(4, 6);
        fx.Save("Card Effect V2 - Thwack");
    }

    public static void RecklessBlow()
    {
        // a wild, overcommitted swing: the blade whirls in a big arc and blood flies back at the swinger
        var fx = new Fx("Card Effect V2 - Reckless Blow");
        fx.Add("Backdrop").Col(BloodDark).Size(1.05f);
        Material blade = Mat("M_SwordGold", "CM_V2_BloodBlade", "Assets/VFX V3/Card Effects V2/Textures/CTX_SwordSilhouette.png", BloodCore, C("#FFC2B8"));
        fx.Add("Icon", "Wild Swing").Mat(blade).Size2(3.2f, 2.6f).Pos(0f, 0.1f).Spin(-24f).Life(0.9f).Sort(5);
        Impact(fx, BloodCore, BloodHot, 0f, 1.15f);
        fx.Add("BlackSplots", "Blowback").Col(C("#8E0A14"), BloodCore).Count(6, 9).VelocityRange(-6f, -2f, 0.5f, 2f).Grav(0.8f);
        Flecks(fx, BloodHot, C("#FFE0D8"), 0f, 3);
        fx.Save("Card Effect V2 - Reckless Blow");
    }

    public static void PewPew()
    {
        // a quick gold bolt fired in from the side
        var fx = new Fx("Card Effect V2 - Pew Pew");
        fx.Add("Backdrop").Col(Felt).Size(0.8f);
        fx.Add("MagicStar", "Muzzle").Col(GoldHot).DelayAt(0.02f).Life(0.18f).Size(0.6f).Pos(-2.4f, 0f);
        fx.Add("Icon", "Bolt").Mat(Mat("M_SwordGold", "CM_V2_Bolt", "Assets/VFX V2/Card Effect Blueprints/Textures/CTX_FlameCone.png", Gold, GoldHot)).Still().NoColLife()
            .Size2(1.4f, 0.55f).Rot(Mathf.PI).Pos(-2.2f, 0f).DelayAt(0.05f).Life(0.17f).Lunge(13f, 0f, 1f).Sort(6);
        fx.Add("MagicStar", "Star Pop").Col(GoldHot).DelayAt(0.22f).Size(0.9f);
        Impact(fx, Gold, GoldHot, -0.05f, 0.75f, false);
        Flecks(fx, Gold, Color.white, -0.05f, 3);
        fx.Save("Card Effect V2 - Pew Pew");
    }

    public static void HonedBlade()
    {
        // a glint slides along a freshly honed edge, then it cuts
        var fx = new Fx("Card Effect V2 - Honed Blade");
        fx.Add("Backdrop").Col(FireDark).Size(0.95f);
        Material blade = Mat("M_SwordGold", "CM_V2_HonedBlade", "Assets/VFX V3/Card Effects V2/Textures/CTX_SwordSilhouette.png", C("#E8E2D6"), C("#FFFFFF"));
        fx.Add("Icon", "Blade").Mat(blade).Sort(5);
        fx.Add("StarIcons", "Edge Glint").Col(Color.white).DelayAt(0.02f).Count(1, 1).Life(0.3f).Speed(0f, 0f).Pos(-0.9f, 0.6f).Lunge(6f, -4f, 1f).Size(1.6f);
        fx.Add("Streaks", "Whetstone Sparks").Col(FireCore, FireHot).DelayAt(0.02f).Count(4, 6).Size(0.6f).Speed(2f, 5f);
        Impact(fx, FireCore, FireHot, 0f, 0.95f);
        Flecks(fx, FireHot, Color.white, 0f, 3);
        fx.Save("Card Effect V2 - Honed Blade");
    }

    public static void TacticalJab()
    {
        // a short straight dagger thrust
        var fx = new Fx("Card Effect V2 - Tactical Jab");
        fx.Add("Backdrop").Col(Indigo).Size(0.8f);
        Material dagger = Mat("M_DaggerVFX", "CM_V2_JabDagger", "Assets/VFX V3/Card Effects V2/Textures/CTX_DaggerSilhouette.png");
        fx.Add("Icon", "Dagger").Mat(dagger).Still().NoColLife().Size2(1.6f, 1.6f).Rot(-Mathf.PI / 2f).Pos(-1.2f, 0f).DelayAt(0.05f).Life(0.5f).Lunge(9f, 0f, 0.4f).Sort(5);
        Impact(fx, Steel, SteelHot, -0.05f, 0.7f, false);
        Flecks(fx, Steel, Color.white, -0.05f, 2);
        fx.Save("Card Effect V2 - Tactical Jab");
    }

    public static void Mince()
    {
        // seven quick nicks: each one a tiny cut at a random angle
        var fx = new Fx("Card Effect V2 - Mince", 3);
        fx.Add("SlashLight", "Nick").Col(SteelHot).DelayAt(0f).Life(0.16f).Size2(2.3f, 0.12f).RotRange(-Mathf.PI, Mathf.PI).Pos(0f, 0f).Box(0.6f, 0.6f);
        fx.Add("Glow").Col(new Color(Steel.r, Steel.g, Steel.b, 0.5f)).DelayAt(0.04f).Life(0.18f).Size(0.7f);
        fx.Add("Spikes").Col(SteelHot).DelayAt(0.04f).Size(0.45f);
        fx.Add("DotsB").Col(Color.white).DelayAt(0.05f).Count(1, 2);
        fx.Save("Card Effect V2 - Mince");
    }

    public static void Clobber()
    {
        // a heavy club swings down in an arc and lands with a big shockwave
        var fx = new Fx("Card Effect V2 - Clobber");
        fx.Add("Backdrop").Col(Indigo).Size(1.05f);
        Material club = Mat("M_SwordGold", "CM_V2_Club", P("CTX_Club"), C("#9A7A52"), C("#E8D2A8"));
        fx.Add("Icon", "Club").Mat(club).Still().Size2(2.2f, 2.2f).Rot(1.2f).Pos(-1.2f, 1.2f).Life(0.75f).Lunge(7f, -7f, 0.42f).Spin(-6f).Sort(5);
        Impact(fx, C("#C8A86A"), C("#FFF0C8"), 0.02f, 1.25f);
        fx.Add("Ring", "Shockwave").Col(SteelHot).Delay(0.1f).Size(2.2f);
        fx.Add("InkBlops", "Debris").Col(C("#8A7A66"), C("#4A3E34")).DelayAt(0.3f).Count(6, 9).Speed(2f, 5f).Grav(0.9f);
        fx.Save("Card Effect V2 - Clobber");
    }

    public static void VengefulSlash()
    {
        // a dark crimson cut with a lingering vengeful afterglow
        var fx = new Fx("Card Effect V2 - Vengeful Slash");
        fx.Add("Backdrop").Col(C("#12040A")).Size(1.05f).Life(1.4f);
        fx.Add("SlashDark", "Dark Cut").Col(C("#0A0206")).DelayAt(0.12f).Life(0.34f).Size2(5.2f, 0.18f).Rot(-0.75f);
        fx.Add("SlashLight", "Crimson Cut").Col(BloodHot).DelayAt(0.12f).Life(0.3f).Size2(4.4f, 0.14f).Rot(-0.75f).Pos(0.03f, 0.03f);
        Impact(fx, BloodCore, BloodHot, 0.02f, 1f);
        fx.Add("Glow", "Afterglow").Col(new Color(0.8f, 0.05f, 0.1f, 0.5f)).DelayAt(0.55f).Life(0.6f).Size(1.3f);
        fx.Add("BlackSplots", "Spatter").Col(C("#8E0A14"), BloodCore).Count(4, 7).Grav(0.6f);
        fx.Save("Card Effect V2 - Vengeful Slash");
    }

    public static void BladeVolley()
    {
        // one dagger per hit flies in from the side and sticks (the card spawns this once per card in hand)
        var fx = new Fx("Card Effect V2 - Blade Volley", 3);
        Material dagger = Mat("M_DaggerVFX", "CM_V2_JabDagger", "Assets/VFX V3/Card Effects V2/Textures/CTX_DaggerSilhouette.png");
        fx.Add("Icon", "Dagger").Mat(dagger).Still().NoColLife().Size2(1.4f, 1.4f).Rot(-Mathf.PI / 2f).Pos(-2.4f, 0f).DelayAt(0f).Life(0.6f)
            .Box(0.2f, 1.2f).Lunge(28f, 0f, 0.28f).Sort(5);
        fx.Add("Spikes", "Pin").Col(SteelHot).DelayAt(0.12f).Size(0.45f);
        fx.Add("Glow").Col(new Color(Steel.r, Steel.g, Steel.b, 0.4f)).DelayAt(0.12f).Life(0.15f).Size(0.5f);
        fx.Add("DotsB").Col(Color.white).DelayAt(0.12f).Count(1, 2);
        fx.Save("Card Effect V2 - Blade Volley");
    }

    static readonly Color Steel = C("#8FB0E8"), SteelHot = C("#E6F0FF"), Indigo = C("#100E24");

    public static void Groupthink()
    {
        // on each drawing rat: a gold thought-swirl spins in and pops out a card
        var fx = new Fx("Card Effect V2 - Groupthink");
        fx.Add("Backdrop").Col(Felt).Size(0.85f);
        fx.Add("PulseIn", "Thought Swirl").Col(GoldHot);
        Material card = Mat("M_SwordGold", "CM_V2_GoldCard", CardTex, Gold, C("#FFF4CC"));
        fx.Add("Icon", "Card Pop").Mat(card).Still().Size2(1f, 1f).DelayAt(0.3f).Life(0.8f).Velocity(0f, 1.2f).Sort(5);
        fx.Add("StarIcons", "Idea Sparks").Col(GoldHot, Color.white).Delay(-0.2f).Count(3, 4);
        Flecks(fx, Gold, Color.white, 0f, 3);
        fx.Save("Card Effect V2 - Groupthink");
    }

    public static void FocusShield()
    {
        // block that narrows to a point of focus: a shield with a closing crosshair
        var fx = new Fx("Card Effect V2 - Focus Shield");
        Material shield = Mat("M_ShieldVFX", "CM_V2_FocusShield", "Assets/VFX V3/TX_NAShield.png", Gold, GoldHot);
        fx.Add("Shield", "Shield").Mat(shield).Col(C("#FFE9A8"));
        fx.Add("ShieldRise").Col(GoldHot);
        fx.Add("ShieldStars").Col(Color.white);
        Material cross = Mat("M_SwordGold", "CM_V2_Crosshair", P("CTX_Crosshair"), Gold, GoldHot);
        fx.Add("Icon", "Focus Crosshair").Mat(cross).Still().Size2(2.4f, 2.4f).DelayAt(0.05f).Life(0.5f).Spin(2.5f).Sort(6);
        fx.Add("Backdrop").Col(Felt).DelayAt(0.3f).Life(1.6f);
        fx.Save("Card Effect V2 - Focus Shield");
    }

    public static void HyperfocusedShot()
    {
        // a crosshair locks onto the target, then a piercing steel-blue shot
        var fx = new Fx("Card Effect V2 - Hyperfocused Shot");
        fx.Add("Backdrop").Col(Indigo).Size(1.1f);
        Material cross = Mat("M_SwordGold", "CM_V2_CrosshairBlue", P("CTX_Crosshair"), Steel, SteelHot);
        fx.Add("Icon", "Lock On").Mat(cross).Still().Size2(2.6f, 2.6f).Spin(-3f).Sort(5);
        fx.Add("Streaks", "Shot").Col(SteelHot).DelayAt(0.22f).Count(1, 1).Life(0.12f).Speed(-26f, -28f).Pos(2.8f, 0.5f).Shape(ParticleSystemShapeType.Cone, 0.01f, 80f, 1f);
        Impact(fx, Steel, SteelHot, 0f, 1f);
        Flecks(fx, Steel, Color.white, 0f, 4);
        fx.Save("Card Effect V2 - Hyperfocused Shot");
    }

    public static void HyperfocusedShotBonus()
    {
        // the focused bonus: the crosshair tightens and a second, brighter star strike
        var fx = new Fx("Card Effect V2 - Hyperfocused Shot Bonus");
        fx.Add("Backdrop").Col(Indigo).Size(0.9f);
        fx.Add("PulseIn", "Tighten").Col(SteelHot);
        fx.Add("MagicStar", "Star Strike").Col(SteelHot).Size(1.2f);
        Impact(fx, Steel, Color.white, 0.05f, 0.85f, false);
        Flecks(fx, Steel, Color.white, 0.05f, 3);
        fx.Save("Card Effect V2 - Hyperfocused Shot Bonus");
    }

    public static void Followthrough()
    {
        // a second strike on the heels of the last: the blade whips through with an afterimage
        var fx = new Fx("Card Effect V2 - Followthrough");
        fx.Add("Backdrop").Col(Indigo);
        Material blade = Mat("M_SwordGold", "CM_V2_SteelBlade", "Assets/VFX V3/Card Effects V2/Textures/CTX_SwordSilhouette.png", Steel, SteelHot);
        fx.Add("Icon", "Afterimage").Mat(blade).Col(new Color(1f, 1f, 1f, 0.35f)).Delay(-0.08f).Sort(4);
        fx.Add("Icon", "Hero Icon").Mat(blade).Sort(5);
        Impact(fx, Steel, SteelHot, 0f, 1f);
        Flecks(fx, Steel, Color.white, 0f, 4);
        fx.Save("Card Effect V2 - Followthrough");
    }

    public static void FollowthroughStrength()
    {
        // momentum carried forward: steel chevrons rise with a blade
        var fx = new Fx("Card Effect V2 - Followthrough Strength");
        fx.Add("Arrows").Col(Steel);
        fx.Add("Arrows2").Col(SteelHot);
        fx.Add("BuffWave").Col(Steel);
        fx.Add("BuffStreaks").Col(SteelHot);
        Material blade = Mat("M_SwordGold", "CM_V2_SteelBlade", "Assets/VFX V3/Card Effects V2/Textures/CTX_SwordSilhouette.png", Steel, SteelHot);
        fx.Add("Icon", "Rising Blade").Mat(blade).Still().Size2(1.6f, 1.6f).Rot(Mathf.PI / 2f).Velocity(0f, 1.4f).Life(0.8f).Sort(5);
        fx.Save("Card Effect V2 - Followthrough Strength");
    }

    public static void StrengthInNumbers()
    {
        // every rat: blood-gold chevrons rise and a crossed-blades flare
        var fx = new Fx("Card Effect V2 - Strength in Numbers");
        fx.Add("Arrows").Col(C("#E8B04A"));
        fx.Add("Arrows2").Col(C("#FFE08A"));
        fx.Add("BuffWave").Col(BloodCore);
        fx.Add("BuffStreaks").Col(BloodHot);
        Material twin = Mat("M_SwordGold", "CM_V2_TwinBlades", "Assets/VFX V2/Textures/T_TwinSwordImpact.png", C("#E8B04A"), C("#FFF1C2"));
        fx.Add("Icon", "Crossed Blades").Mat(twin).Still().Size2(1.8f, 1.8f).Rot(0f).Velocity(0f, 0.6f).Sort(5);
        fx.Save("Card Effect V2 - Strength in Numbers");
    }

    // ===================== revive =====================
    static readonly Color Dawn = C("#FFC85A"), DawnHot = C("#FFF3C4"), DawnViolet = C("#8A5FC4"), DawnDark = C("#160E26");

    static readonly Color ReviveGreen = C("#7CE86A"), ReviveMint = C("#C8F7B4");

    public static void CompanionRevive()
    {
        // soft and circular healing: a faint halo turns, two thick soft rings breathe out slowly, plus signs and motes drift,
        // green life with a little dawn gold and violet
        var fx = new Fx("Companion Revive", 5);
        fx.Add("Backdrop").Col(new Color(0.06f, 0.08f, 0.09f, 0.7f)).DelayAt(0f).Life(2.6f).Size(1.35f);
        fx.Add("Glow", "Life Glow").Col(new Color(0.55f, 0.95f, 0.45f, 0.45f)).DelayAt(0.1f).Life(2.3f).Size(2.2f);
        fx.Add("Glow", "Violet Glow").Col(new Color(0.62f, 0.42f, 1f, 0.5f)).DelayAt(0.2f).Life(2.3f).Size(3f).Sort(3);
        fx.Add("Glow", "Dawn Glow").Col(new Color(1f, 0.85f, 0.5f, 0.42f)).DelayAt(0.4f).Life(1.9f).Size(1.3f);
        Material halo = Mat("M_SwordGold", "CM_V2_ReviveHalo", BlueprintTex + "CTX_BloodSigil.png", Dawn, DawnHot);
        fx.Add("Icon", "Halo Sigil").Mat(halo).Still().NoColLife().Col(new Color(1f, 0.86f, 0.45f, 0.4f)).Size2(2.4f, 2.4f).Pos(0f, 0.2f).DelayAt(0.15f).Life(2.2f).Spin(0.22f).Sort(5);
        // thick, soft-edged rings (painterly TX_Circle_40 and the glowing Circle_14), two slow breaths
        Material softRing = Mat("M_BasicHollowCircle", "CM_V2_SoftRingThick", "Assets/VFX V3/TX_Circle_40.png");
        Material glowRing = Mat("M_BasicHollowCircle", "CM_V2_SoftRingGlow", "Assets/VFX V2/Textures/Circle_14.png");
        fx.Add("Ring", "Breath Ring").Mat(softRing).Col(new Color(0.6f, 0.95f, 0.5f, 0.55f)).DelayAt(0.3f).Life(1.9f).Size(1.45f);
        fx.Add("Ring", "Breath Ring").Mat(glowRing).Col(new Color(1f, 0.9f, 0.62f, 0.45f)).DelayAt(1.3f).Life(1.9f).Size(1.6f);
        // the heal signal: a few soft green plus signs drifting up around the ring, once
        fx.Add("Plus", "Plus Signs").Col(Color.white).DelayAt(0.5f).Count(4, 5).Once().Life(1.6f, 2f).Flat().Shape(ParticleSystemShapeType.Circle, 0.95f).Pos(0f, 0f).VelocityRange(-0.06f, 0.06f, 0.35f, 0.55f).Sort(7);
        fx.Add("DotsB", "Orbiting Motes").Col(new Color(1f, 0.85f, 0.4f, 0.7f), new Color(0.72f, 0.5f, 1f, 0.75f)).DelayAt(0.1f).Stream(11f, 1.4f).Life(1.8f, 2.2f).Size(0.9f).Orbit(1.2f, 1f, 0.1f).Sort(6);
        fx.Add("DotsA", "Green Motes").Col(new Color(0.45f, 0.85f, 0.4f, 0.5f), new Color(0.78f, 0.97f, 0.7f, 0.5f)).DelayAt(0.3f).Stream(8f, 1.2f).Life(1.8f, 2.2f).Size(0.8f).Orbit(0.8f, -0.8f, 0.08f).Sort(6);
        fx.Add("Streaks", "Rising Light").Col(new Color(0.85f, 1f, 0.75f, 0.4f)).DelayAt(0.5f).Stream(5f, 1.2f).Life(1.2f, 1.5f).Speed(0f, 0f)
            .VelocityRange(-0.08f, 0.08f, 0.9f, 1.3f).Box(1.4f, 0.2f).Pos(0f, -0.6f).Sort(4);
        fx.SaveAt("Assets/Prefabs/VFX/VFX Prefabs - For Luke/Revive/Companion Revive.prefab");
    }

    // ===================== signature and big cards =====================

    // on an area effect the shake rides the Area group, which only the first of the per-target copies keeps
    static void Shake(Fx fx, float delay, float force)
    {
        Transform area = fx.root.transform.Find("Area");
        var s = (area != null ? area.gameObject : fx.root).AddComponent<ShakeOnSpawn>(); s.delay = delay; s.force = force;
    }

    // a flat-coloured icon (particle colour, no black-to-colour flip or dissolve), visible from its first frame
    static Layer PlainIcon(Fx fx, string name, string tex, string matName)
    {
        Material m = Mat("M_BasicParticlesAlpha", matName, tex, Color.white, Color.white);
        var l = fx.Add("Icon", name).Mat(m).Still().NoColLife().Flat().Rot(0f);
        var cd = l.ps.customData; cd.enabled = false;
        return l;
    }

    static readonly Color Void = C("#7A2FB0"), VoidHot = C("#E0B8FF"), VoidTeal = C("#6AF0C8"), VoidInk = C("#14051F");
    static readonly Color Stone = C("#8A8478"), StoneHot = C("#F2E6C8"), Moss = C("#7FAF5A");
    static readonly Color Apm = C("#3FE0FF"), ApmHot = C("#D8FBFF");

    public static void AceOfSpades()
    {
        // per hit: the ace is dealt in spinning from the upper left, and a giant ink spade stamps the target in gold
        var fx = new Fx("Card Effect V2 - Ace of Spades");
        fx.Add("Backdrop").Col(C("#0A0810")).Size(1.05f);
        Material ace = Mat("M_SwordGold", "CM_V2_AceCard", CardTex, C("#E8E2D6"), Color.white);
        fx.Add("Icon", "Dealt Ace").Mat(ace).Still().NoColLife().Col(Color.white).Size2(0.8f, 1.1f).Pos(-2.2f, 1.4f).DelayAt(0f).Life(0.26f)
            .Lunge(17f, -10.8f, 1f).Spin(-22f).Sort(7);
        fx.Add("InkSplat", "Ink Stamp").Col(C("#14101A")).DelayAt(0.24f).Size(0.85f).Sort(3);
        // a solid ink spade stamped down inside the gold burst
        PlainIcon(fx, "Spade", P("CTX_Spade"), "CM_V2_Spade").Col(C("#14101A")).DelayAt(0.2f).Life(0.6f).Size2(2f, 2f).Grow(1.4f, 1f).Sort(5);
        Impact(fx, Gold, GoldHot, 0f, 1.05f);
        fx.Add("StarIcons", "Gold Glints").Col(GoldHot, Color.white).Count(4, 6).Size(0.9f);
        Flecks(fx, C("#2A2238"), GoldHot, 0f, 4);
        fx.Save("Card Effect V2 - Ace of Spades");
    }

    public static void WorldEater()
    {
        // the screen goes dark and the world is dragged into a maw on the target, then it bites down
        var fx = new Fx("Card Effect V2 - World Eater", 5);
        fx.Add("Backdrop", "Screen Dim").Col(new Color(0.03f, 0.01f, 0.05f, 0.88f)).DelayAt(0f).Size(5f).Life(1.5f);
        fx.Add("InkBlops", "Swallowed Ink").Flat().Col(VoidInk, C("#4A1A6A")).DelayAt(0f).Count(20, 26).Life(0.38f).Speed(-6.5f, -7.5f).Shape(ParticleSystemShapeType.Circle, 2.7f).Size(1.3f).Sort(5);
        fx.Add("StarIcons", "Swallowed Light").Flat().Col(VoidTeal, VoidHot).DelayAt(0.04f).Count(8, 10).Life(0.34f).Speed(-7f, -8f).Shape(ParticleSystemShapeType.Circle, 2.6f).Sort(6);
        fx.Add("PulseIn", "Maw").Col(Void).DelayAt(0f).Size(2.2f);
        fx.Add("InkSplat", "Maw Ink").Col(C("#08020C")).DelayAt(0.3f).Size(1.25f).Sort(4);
        Impact(fx, Void, VoidHot, 0.1f, 1.5f);
        fx.Add("Ring", "Bite Wave").Col(VoidTeal).DelayAt(0.46f).Life(0.5f).Size(3.6f);
        fx.Add("InkSub", "Crumbs").Col(VoidInk, Void).DelayAt(0.4f).Size(1.2f);
        Flecks(fx, VoidTeal, VoidHot, 0.1f, 6);
        Shake(fx, 0.4f, 0.5f);
        fx.Save("Card Effect V2 - World Eater");
    }

    public static void GolemsMight()
    {
        // a stone fist drops from above and slams the target; rubble and moss fly and the ground ripples twice
        var fx = new Fx("Card Effect V2 - Golem's Might", 5);
        fx.Add("Backdrop").Col(C("#14120E")).Size(1.25f).Life(1.4f);
        Material fist = Mat("M_SwordGold", "CM_V2_StoneFist", P("CTX_Fist"), Stone, StoneHot);
        fx.Add("Icon", "Stone Fist").Mat(fist).Still().NoColLife().Col(Color.white).Size2(2.6f, 2.6f).Rot(Mathf.PI).Pos(0f, 3f).DelayAt(0f).Life(0.6f)
            .Lunge(0f, -20f, 0.5f).Sort(6);
        Impact(fx, Stone, StoneHot, 0f, 1.4f);
        fx.Add("Ring", "Slam Wave").Col(StoneHot).Delay(0.05f).Size(2.6f);
        fx.Add("Ring", "Slam Wave 2").Col(Moss).Delay(0.14f).Size(3.8f);
        fx.Add("InkBlops", "Rubble").Col(C("#8A8478"), C("#4A443A")).DelayAt(0.3f).Count(10, 14).Speed(0f, 0f).VelocityRange(-5f, 5f, 2f, 6f).Grav(1.3f).Size(1.2f);
        fx.Add("InkBlops", "Dust").Col(C("#B8AE98"), C("#6A6252")).DelayAt(0.32f).Count(8, 10).Box(3f, 0.2f).Pos(0f, -0.5f).Speed(0f, 0f).VelocityRange(-2f, 2f, 0.2f, 0.8f);
        fx.Add("Leaves", "Moss").Col(Moss, C("#C8E8A0")).DelayAt(0.3f).Count(5, 7);
        Shake(fx, 0.3f, 0.5f);
        fx.Save("Card Effect V2 - Golem's Might");
    }

    public static void OverwhelmingAPM()
    {
        // one per card played this turn, so it is short: a burst of cyan keystroke nicks and clicks all over the target
        var fx = new Fx("Card Effect V2 - Overwhelming APM", 2);
        fx.Add("SlashLight", "Keystrokes").Col(ApmHot, Apm).DelayAt(0f).Stream(24f, 0.2f).Life(0.12f).Size2(1.5f, 0.09f).RotRange(-Mathf.PI, Mathf.PI).Box(1.1f, 1.1f);
        fx.Add("Spikes", "Click Spikes").Col(Apm).DelayAt(0f).Stream(18f, 0.2f).Life(0.15f).Box(1.1f, 1.1f).Size(0.35f);
        fx.Add("Glow").Col(new Color(Apm.r, Apm.g, Apm.b, 0.45f)).DelayAt(0.02f).Life(0.3f).Size(0.8f);
        fx.Add("MagicStar", "Combo Pop").Col(ApmHot).DelayAt(0.2f).Size(0.6f);
        fx.Add("DotsB").Col(Color.white).DelayAt(0.05f).Count(2, 3);
        fx.Save("Card Effect V2 - Overwhelming APM");
    }

    public static void MonteCarlo()
    {
        // on each drawing rat: a gold shuffle swirl, then a hand of cards fans out, tumbles and falls (the reshuffled hand)
        var fx = new Fx("Card Effect V2 - Monte Carlo", 3);
        fx.Add("Backdrop").Col(Felt).Size(0.85f);
        fx.Add("PulseIn", "Shuffle Swirl").Col(GoldHot).Size(0.9f);
        Material card = Mat("M_SwordGold", "CM_V2_GoldCard", CardTex, Gold, C("#FFF4CC"));
        fx.Add("Icon", "Fanned Cards").Mat(card).Still().NoColLife().Col(Color.white).DelayAt(0.25f).Count(5, 5).Life(0.6f, 0.75f).Size2(0.62f, 0.86f)
            .VelocityRange(-3f, 3f, 2f, 3.6f).Grav(1.1f).Spin(9f).Sort(7);
        fx.Add("Ring", "Soft Halo").Mat(Mat("M_BasicHollowCircle", "CM_V2_SoftRingGlow", "Assets/VFX V2/Textures/Circle_14.png")).Col(new Color(1f, 0.88f, 0.55f, 0.6f)).DelayAt(0.25f).Size(1.1f);
        fx.Add("MagicStar", "Lucky Star").Col(GoldHot).DelayAt(0.25f).Size(0.8f);
        fx.Add("StarIcons", "Lucky Sparks").Col(GoldHot, Color.white).DelayAt(0.25f).Count(4, 6);
        Flecks(fx, Gold, Color.white, 0f, 3);
        fx.Save("Card Effect V2 - Monte Carlo");
    }

    public static void Monsoon()
    {
        // a short downpour over the rat, splashing at its feet, then a water shield rises (block from the charms it burned)
        var fx = new Fx("Card Effect V2 - Monsoon", 4);
        fx.Add("Backdrop").Col(Deep).Size(1.1f).Life(1.4f);
        PlainIcon(fx, "Downpour", P("CTX_BloodDrop"), "CM_V2_RainDrop").Col(WaterHot, Water).DelayAt(0f).Stream(40f, 0.6f).Life(0.3f, 0.34f).Size2(0.22f, 0.5f)
            .Box(2.6f, 0.1f).Pos(0f, 2.6f).Speed(0f, 0f).VelocityRange(-1.4f, -1f, -14f, -16f).Sort(6);
        fx.Add("DotsB", "Splash Droplets").Col(WaterHot, Water).DelayAt(0.25f).Stream(22f, 0.5f).Life(0.3f, 0.45f).Box(2.2f, 0.1f).Pos(0f, -0.9f).Speed(0f, 0f).VelocityRange(-1f, 1f, 1f, 2f).Grav(1f);
        Material shield = Mat("M_ShieldVFX", "CM_V2_WaterShield", "Assets/VFX V3/TX_NAShield.png", Water, WaterHot);
        fx.Add("Shield").Mat(shield).Col(WaterHot).Delay(0.25f);
        fx.Add("ShieldRise").Col(WaterHot).Delay(0.25f);
        fx.Add("ShieldStars").Col(Color.white).Delay(0.25f);
        fx.Add("Ring", "Soft Halo").Mat(Mat("M_BasicHollowCircle", "CM_V2_SoftRingThick", "Assets/VFX V3/TX_Circle_40.png")).Col(new Color(0.55f, 0.88f, 1f, 0.6f)).DelayAt(0.4f).Size(1.2f);
        fx.Save("Card Effect V2 - Monsoon");
    }

    public static void ContingencyPlan()
    {
        // on every rat: a card drops in and folds into a steel shield, a soft halo locks it in
        var fx = new Fx("Card Effect V2 - Contingency Plan", 3);
        Material card = Mat("M_SwordGold", "CM_V2_PlanCard", CardTex, Steel, SteelHot);
        fx.Add("Icon", "Folding Card").Mat(card).Still().NoColLife().Col(Color.white).Size2(0.9f, 1.25f).Pos(0f, 0.9f).DelayAt(0f).Life(0.35f).Lunge(0f, -5f, 1f).Grow(1.2f, 0.5f).Sort(7);
        Material shield = Mat("M_ShieldVFX", "CM_V2_SteelShield", "Assets/VFX V3/TX_NAShield.png", Steel, SteelHot);
        fx.Add("Shield").Mat(shield).Col(SteelHot).Delay(0.1f);
        fx.Add("ShieldRise").Col(SteelHot).Delay(0.1f);
        fx.Add("ShieldStars").Col(Color.white).Delay(0.1f);
        fx.Add("Ring", "Soft Halo").Mat(Mat("M_BasicHollowCircle", "CM_V2_SoftRingThick", "Assets/VFX V3/TX_Circle_40.png")).Col(new Color(0.7f, 0.82f, 1f, 0.6f)).DelayAt(0.28f).Size(1.15f);
        fx.Add("Glow").Col(new Color(Steel.r, Steel.g, Steel.b, 0.45f)).DelayAt(0.28f).Life(0.5f).Size(0.9f);
        fx.Save("Card Effect V2 - Contingency Plan");
    }

    public static void Aegis()
    {
        // a radiant gold dome: the shield rises inside two widening soft halos while rays lift and motes circle
        var fx = new Fx("Card Effect V2 - Aegis", 4);
        fx.Add("Glow", "Radiance").Col(new Color(1f, 0.85f, 0.45f, 0.5f)).DelayAt(0f).Life(1f).Size(1.6f);
        Material shield = Mat("M_ShieldVFX", "CM_V2_AegisShield", "Assets/VFX V3/TX_NAShield.png", Gold, GoldHot);
        fx.Add("Shield").Mat(shield).Col(C("#FFE9A8"));
        fx.Add("ShieldRise").Col(GoldHot);
        fx.Add("ShieldStars").Col(Color.white);
        Material glowRing = Mat("M_BasicHollowCircle", "CM_V2_SoftRingGlow", "Assets/VFX V2/Textures/Circle_14.png");
        fx.Add("Ring", "Dome Halo").Mat(glowRing).Col(new Color(1f, 0.88f, 0.55f, 0.65f)).DelayAt(0.1f).Life(0.9f).Size(1.5f);
        fx.Add("Ring", "Dome Halo 2").Mat(glowRing).Col(new Color(1f, 0.95f, 0.75f, 0.45f)).DelayAt(0.35f).Life(0.9f).Size(1.9f);
        fx.Add("Streaks", "Radiant Rays").Col(new Color(1f, 0.92f, 0.65f, 0.6f)).DelayAt(0.1f).Stream(10f, 0.6f).Life(0.7f, 0.9f).Speed(0f, 0f)
            .VelocityRange(-0.1f, 0.1f, 1.2f, 1.8f).Box(1.6f, 0.2f).Pos(0f, -0.6f).Sort(4);
        fx.Add("DotsB", "Orbiting Motes").Col(GoldHot, Color.white).DelayAt(0.1f).Stream(10f, 0.7f).Life(0.9f, 1.1f).Size(0.8f).Orbit(1.1f, 1.4f, 0.1f).Sort(6);
        fx.Save("Card Effect V2 - Aegis");
    }

    public static void DrawCards()
    {
        // a draw: a soft gold lift off the rat, then three cards pop up in a little fan and arc down into the hand
        // below (they rise, slow, and fall past the rat), each a beat after the last, dissolving through the
        // watercolour noise as they go. The glow belongs to the real card arriving in hand (DrawnCardGlow)
        var fx = new Fx("Card Effect V2 - Draw", 3);
        fx.Add("Glow", "Lift Glow").Col(new Color(Gold.r, Gold.g, Gold.b, 0.45f)).DelayAt(0f).Life(0.5f).Size(0.8f);
        fx.Add("Ring", "Lift Ring").Col(GoldHot).DelayAt(0f).Size(0.6f);
        // a plain card (rounded, no pip) in the gold sword's own look: its painted overlay and the white-to-gold
        // gradient toward the bottom (M_SwordGold's colours, untinted)
        Material card = Mat("M_SwordGold", "CM_V2_DrawCard", "Assets/VFX V3/Card Effects V2/Textures/CTX_DrawCard.png");
        Material sword = AssetDatabase.LoadAssetAtPath<Material>("Assets/VFX V2/M_SwordGold.mat");
        card.SetColor("_Colour_A", sword.GetColor("_Colour_A")); card.SetColor("_Colour_B", sword.GetColor("_Colour_B"));
        EditorUtility.SetDirty(card); AssetDatabase.SaveAssetIfDirty(card);
        WatercolourCutoff(card);
        (float vx, float tilt, float at)[] cards = { (-1.5f, 0.5f, 0.04f), (0f, 0f, 0.1f), (1.5f, -0.5f, 0.16f) };
        foreach (var c in cards)
        {
            var drawn = fx.Add("Icon", "Drawn Card").Mat(card).Still().NoColLife().Col(Color.white).Size2(0.72f, 1f).Rot(c.tilt).Shape(ParticleSystemShapeType.Sphere, 0f).Speed(0f, 0f)
                .Pos(0f, 0.25f).Velocity(c.vx, 4f).Grav(1.1f).Spin(-c.tilt * 1.2f).DelayAt(c.at).Life(0.95f).Sort(7);
            WatercolourDissolve(drawn, 0.08f, 0.65f);
            fx.Add("DotsB", "Card Sparkle").Col(GoldHot).DelayAt(c.at).Count(2, 3).Size(0.6f).Pos(0f, 0.25f);
        }
        Flecks(fx, Gold, GoldHot, 0f, 3);
        fx.Save("Card Effect V2 - Draw");
    }

    const string ManaFlourishPath = "Assets/Prefabs/VFX/Mana/Mana Flourish.prefab";

    public static void DrawnCardGlow()
    {
        // a card an effect drew, once it lands in hand: a gold rim of light around it (the card's shape, hollow, so
        // the face stays readable) that pops in, breathes twice and fades, about 3 seconds, with a few motes rising
        // off it at first. It rides the card (CardVFX follow), so hovering and reordering the hand keep it on.
        // Cards in hand are 1 x 1.4; the rim texture is the 256x356 card plus 64px a side
        const float life = 3f;
        var fx = new Fx("Card Effect V2 - Drawn Card Glow", 4);
        // additive, so the rim only ever adds light (the alpha material's overlay mottled its inner edge dark);
        // centred on the card (templates carry their source layer's offset)
        Material rimMat = Mat("M_BasicParticlesAdd", "CM_V2_DrawnCardRim", "Assets/VFX V3/Card Effects V2/Textures/CTX_DrawCardGlow.png", Color.white, Color.white);
        if (rimMat.GetFloat("_Use_Cutoff") != 0f) { rimMat.SetFloat("_Use_Cutoff", 0f); EditorUtility.SetDirty(rimMat); AssetDatabase.SaveAssetIfDirty(rimMat); }
        var rim = fx.Add("Icon", "Gold Rim").Mat(rimMat).Still().NoColLife().Flat().RotRange(0f, 0f).Pos(0f, 0f)   // a range, so SaveAt leaves it square to the card
            .Col(new Color(1f, 0.78f, 0.3f, 1f)).Size2(384f / 256f, 1.4f * 484f / 356f).DelayAt(0f).Life(life).Sort(2);
        var rimData = rim.ps.customData; rimData.enabled = false;
        var col = rim.ps.colorOverLifetime; col.enabled = true;
        var breathe = new Gradient();
        breathe.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.05f), new GradientAlphaKey(0.6f, 0.25f), new GradientAlphaKey(1f, 0.45f),
                    new GradientAlphaKey(0.6f, 0.65f), new GradientAlphaKey(0.85f, 0.8f), new GradientAlphaKey(0f, 1f) });
        col.color = breathe;
        // a little pop as it lands, then settles
        var pop = rim.ps.sizeOverLifetime; pop.enabled = true;
        pop.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1.12f), new Keyframe(0.06f, 1f), new Keyframe(1f, 1f)));
        // scale with the card when the hand hovers or shrinks it
        var rm = rim.ps.main; rm.scalingMode = ParticleSystemScalingMode.Hierarchy; rm.simulationSpace = ParticleSystemSimulationSpace.Local;
        fx.Add("DotsB", "Rising Motes").Col(GoldHot, Color.white).DelayAt(0f).Stream(6f, 1.2f).Life(0.7f, 1.1f).Box(0.9f, 1.3f)
            .Speed(0f, 0f).VelocityRange(-0.15f, 0.15f, 0.5f, 0.9f).Size(0.5f).Pos(0f, 0f).Sort(3);
        fx.Save("Card Effect V2 - Drawn Card Glow");

        // the mana emblem's watcher object (it already shimmers retained cards) plays it on drawn cards
        GameObject glow = AssetDatabase.LoadAssetAtPath<GameObject>(OutDir + "/Card Effect V2 - Drawn Card Glow.prefab");
        GameObject flourish = PrefabUtility.LoadPrefabContents(ManaFlourishPath);
        try
        {
            DrawnCardsVFX watcher = flourish.GetComponent<DrawnCardsVFX>();
            if (watcher == null) watcher = flourish.AddComponent<DrawnCardsVFX>();
            var so = new SerializedObject(watcher);
            so.FindProperty("drawnGlowVFX").objectReferenceValue = glow;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(flourish, ManaFlourishPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(flourish); }
    }

    // the MasterShader dissolve the hero icons use: Custom1.x runs the watercolour fade texture's cutoff, 1 = gone,
    // -1 = solid. Here the icon firms up by `solidAt`, holds, and dissolves away from `holdUntil` to the end
    static void WatercolourDissolve(Layer l, float solidAt, float holdUntil)
    {
        var cd = l.ps.customData; cd.enabled = true;
        cd.SetMode(ParticleSystemCustomData.Custom1, ParticleSystemCustomDataMode.Vector);
        cd.SetVectorComponentCount(ParticleSystemCustomData.Custom1, 4);
        cd.SetVector(ParticleSystemCustomData.Custom1, 0, new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(solidAt, -0.99f), new Keyframe(holdUntil, -0.99f), new Keyframe(1f, 1f))));   // -0.99 like the sword: not the shader's edge value
        var r = l.ps.GetComponent<ParticleSystemRenderer>();
        var streams = new List<ParticleSystemVertexStream>(); r.GetActiveVertexStreams(streams);
        if (!streams.Contains(ParticleSystemVertexStream.Custom1XYZW)) { streams.Add(ParticleSystemVertexStream.Custom1XYZW); r.SetActiveVertexStreams(streams); }
    }

    // dissolve on, through the same watercolour fade texture as the gold sword
    static void WatercolourCutoff(Material m)
    {
        Texture wash = AssetDatabase.LoadAssetAtPath<Material>("Assets/VFX V2/M_SwordGold.mat").GetTexture("_FadeTexture");
        m.SetFloat("_Use_Cutoff", 1f);
        m.SetTexture("_FadeTexture", wash);
        EditorUtility.SetDirty(m); AssetDatabase.SaveAssetIfDirty(m);
    }

    static readonly Color TempRose = C("#FF6A5C"), TempRoseHot = C("#FFA89C");

    public static void TemporaryStrength()
    {
        // Luke's Strength Buff, borrowed for a turn and smaller: just the one sword (no crossing side swords) and the
        // arrows on the same beat, a rose pulse instead of gold, and the hourglass from the Temporary Strength icon
        // turning over beside the sword
        const float shrink = 0.7f;
        var fx = new Fx("Card Effect V2 - Temporary Strength", 3);
        GameObject buff = AssetDatabase.LoadAssetAtPath<GameObject>(Luke + "Strength Buff.prefab");
        foreach (Transform t in buff.transform)
        {
            if (!t.gameObject.activeSelf || t.name == "Sword (2)" || t.name == "Sword (3)") continue;
            GameObject layer = UnityEngine.Object.Instantiate(t.gameObject, fx.root.transform);
            layer.name = t.name;
            layer.transform.localPosition *= shrink;
        }
        // in the hourglass's rose (its painted body is #EE786E), as bright as the gold of the permanent buff
        Material roseSword = Mat("M_SwordGold", "CM_V2_TempStrengthSword", "Assets/VFX V3/TX_NASword.png", TempRose, TempRoseHot);
        Color rose = TempRose;
        foreach (ParticleSystem ps in fx.root.GetComponentsInChildren<ParticleSystem>(true))
        {
            if (ps.name == "Sword") ps.GetComponent<ParticleSystemRenderer>().sharedMaterial = roseSword;
            if (ps.name.StartsWith("Ink Arrows")) { var am = ps.main; am.startColor = TempRoseHot; }
            var m = ps.main;
            if (m.startSize3D) { m.startSizeXMultiplier *= shrink; m.startSizeYMultiplier *= shrink; m.startSizeZMultiplier *= shrink; }
            else m.startSizeMultiplier *= shrink;
            m.startSpeedMultiplier *= shrink;
            var shape = ps.shape; if (shape.enabled) { shape.radius *= shrink; shape.scale *= shrink; }
            // the gold pulse, flash and motes (FFF691) go rose
            if (ps.name == "Circle Pulse" || ps.name == "Enemy Background Big (1)" || ps.name == "Ink Splots (4)")
                m.startColor = new Color(rose.r, rose.g, rose.b, m.startColor.color.a);
        }
        // lands as the sword turns gold
        const float glassLife = 0.95f;
        var glass = PlainIcon(fx, "Hourglass", "Assets/VFX V3/Card Effects V2/Textures/CTX_Hourglass.png", "CM_V2_Hourglass").Col(Color.white)
            .DelayAt(0.42f).Life(glassLife).Size2(0.8f, 0.8f).Pos(0.7f, 0.55f).Fade(0.65f).Sort(6);
        // a painted icon: no dissolve cutoff, or the noise marbles it (as with Meothra's tail)
        Material glassMat = glass.ps.GetComponent<ParticleSystemRenderer>().sharedMaterial;
        if (glassMat.GetFloat("_Use_Cutoff") != 0f) { glassMat.SetFloat("_Use_Cutoff", 0f); EditorUtility.SetDirty(glassMat); AssetDatabase.SaveAssetIfDirty(glassMat); }
        // it turns over once (180 degrees) between 15% and 45% of its life, then holds
        var turn = glass.ps.rotationOverLifetime; turn.enabled = true;
        turn.z = new ParticleSystem.MinMaxCurve(Mathf.PI / (0.5f * 0.3f * glassLife),
            new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.15f, 0f), new Keyframe(0.3f, 1f), new Keyframe(0.45f, 0f), new Keyframe(1f, 0f)));
        fx.Save("Card Effect V2 - Temporary Strength");
    }

    public static void Powernap()
    {
        // a dusky violet doze, a few Z's drift up, then the rat wakes up strong in a warm burst of chevrons
        var fx = new Fx("Card Effect V2 - Powernap", 4);
        fx.Add("Glow", "Dusk Glow").Col(new Color(0.55f, 0.45f, 1f, 0.45f)).DelayAt(0f).Life(1.1f).Size(1.4f);
        PlainIcon(fx, "Zzz", P("CTX_Zzz"), "CM_V2_Zzz").Col(C("#C8BEFF"), C("#F0ECFF")).DelayAt(0f).Stream(5f, 0.6f).Life(0.8f).Size2(0.75f, 0.75f).Pos(0.3f, 0.5f)
            .Speed(0f, 0f).VelocityRange(0.4f, 0.9f, 1.1f, 1.5f).Grow(0.6f, 1.3f).Sort(6);
        Bubbles(fx, C("#C8BEFF"), C("#F0ECFF"), 0.05f, 0.6f, 8f);   // a few sleepy bubbles
        fx.Add("Flash", "Wake Up").Col(new Color(1f, 0.75f, 0.45f, 0.65f)).DelayAt(0.8f).Life(0.25f).Size(1.1f);
        fx.Add("Ring", "Wake Burst").Col(C("#FFB84A")).DelayAt(0.8f).Size(1.2f);
        fx.Add("Arrows").Col(C("#FF7A3A")).Delay(0.6f);
        fx.Add("Arrows2").Col(C("#FFD27A")).Delay(0.6f);
        fx.Add("BuffWave").Col(C("#FF7A3A")).Delay(0.6f);
        fx.Add("BuffStreaks").Col(C("#FFD27A")).Delay(0.6f);
        fx.Save("Card Effect V2 - Powernap");
    }

    public static void LessonInPain()
    {
        // after the rat is hurt: blood washes up and crimson chevrons surge out of the wound
        var fx = new Fx("Card Effect V2 - Lesson in Pain", 4);
        BloodWash(fx, 0f, 3, 4, 1.1f);
        PaintWash(fx, new Color(BloodCore.r, BloodCore.g, BloodCore.b, 0.6f), 0f, 1.1f);
        fx.Add("Glow", "Wound Glow").Col(new Color(0.9f, 0.08f, 0.12f, 0.5f)).DelayAt(0f).Life(0.8f).Size(1.2f);
        fx.Add("Spikes", "Pain Spikes").Col(BloodHot).DelayAt(0.05f).Size(0.9f);
        fx.Add("Arrows").Col(BloodCore);
        fx.Add("Arrows2").Col(BloodHot);
        fx.Add("BuffWave").Col(BloodCore);
        fx.Add("BuffStreaks").Col(BloodHot);
        fx.Add("Ring", "Soft Halo").Mat(Mat("M_BasicHollowCircle", "CM_V2_SoftRingThick", "Assets/VFX V3/TX_Circle_40.png")).Col(new Color(1f, 0.35f, 0.3f, 0.55f)).DelayAt(0.15f).Size(1.2f);
        fx.Save("Card Effect V2 - Lesson in Pain");
    }

    public static void BuildPass3()
    {
        AceOfSpades(); WorldEater(); GolemsMight(); OverwhelmingAPM(); MonteCarlo(); Monsoon(); ContingencyPlan(); Aegis(); Powernap(); LessonInPain(); TemporaryStrength(); DrawCards(); DrawnCardGlow();
        AssetDatabase.Refresh();
    }

    public static void BuildPass2()
    {
        FlamethrowerArea(); DetonateArea(); BoomflaskArea(); GunslingerArea(); EbbAndFlowArea(); LashOutArea(); SlobberArea();
        // the starter Ratslaps keep the basic slash (their per-pack slaps are archived); the slap lives on as
        // upgraded effects for the pack cards that suit it
        WristFlick(); ComboPiece(); Frenzy();
        BasicCues();   // enemy sweeps: VFXRetoolWiring.WireEnemySweeps builds one around each enemy's own hit
        DoubleSlash(); TwoSidedBlade(); Thwack(); /* Reckless Blow uses Finishing Touch 1 (red) now */ PewPew(); HonedBlade(); TacticalJab(); Mince(); Clobber(); VengefulSlash(); BladeVolley();
        AssetDatabase.Refresh();
    }

    public static void BuildAll()
    {
        Groupthink(); HyperfocusedShot(); HyperfocusedShotBonus(); Followthrough(); FollowthroughStrength(); StrengthInNumbers();
        Pyrotechnics(); GreekFireImpact(); GreekFirePot(); GreekFireThrow();
        // Bloodied Blade's bleed is Luke's Status Apply Effect Bleed now (VFXRetoolWiring.WireStatusLooks)
        BloodiedBlade(); Ritual(); CollectiveRage(); SerratedWaraxe(); SerratedWaraxeBite(); Retaliation(); RetaliationCounter();
        ForestsMight(); Bezoar();
        Brainstorm(); Disappear();
        AssetDatabase.Refresh();
        Debug.Log("[VFX Retool] built");
    }

    [MenuItem("VFX Retool/Build all")]
    static void BuildAllMenu() => BuildAll();
}
