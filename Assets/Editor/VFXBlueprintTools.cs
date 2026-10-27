using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/*
    Post-processing tools for the generated card-effect blueprints in
    Assets/Prefabs/VFX/VFX Prefabs - For Luke/Card Effect Blueprints.

    The blueprints ship as compositions of nested prefab instances so they stay
    linked to the source effects. Once you want to hand-tune one, run these:

      1. Unpack   - breaks every nested instance so edits no longer touch the
                    source effects (and the source effects no longer push
                    changes into the blueprint).
      2. Prune    - deletes particle systems that can never contribute:
                    emission off / zero rate with no bursts, no renderer,
                    renderer disabled, or no material.
      3. Palette  - tints each remaining system to its theme family via the
                    particle system's own start colour, weighted by what the
                    layer is (glow, paint, shadow, solid sprite, soft halo).
                    This does NOT touch any shared material, so nothing else
                    in the game changes.

    Run them in order, or use "Run all". Everything is logged.
*/
public static class VFXBlueprintTools
{
    const string BlueprintDir = "Assets/Prefabs/VFX/VFX Prefabs - For Luke/Card Effect Blueprints";
    const string BossDir = "Assets/Prefabs/VFX/VFX Prefabs - For Luke/Boss Variations"; // palettes only
    const string ReviveDir = "Assets/Prefabs/VFX/VFX Prefabs - For Luke/Revive";     // palettes only

    struct Palette
    {
        public string Name;
        public Color Highlight;
        public Color Core;
        public Color Shadow;
        public Color[] Confetti; // optional: discrete multi-colour lights (fireworks)
        public Palette(string n, string highlight, string core, string shadow, params string[] confetti)
        {
            Name = n;
            Highlight = Hex(highlight); Core = Hex(core); Shadow = Hex(shadow);
            Confetti = confetti.Select(Hex).ToArray();
        }
    }

    static Color Hex(string s)
    {
        Color c;
        ColorUtility.TryParseHtmlString(s, out c);
        return c;
    }

    /*
        Each palette is a HIGHLIGHT, a CORE identity colour and a complementary SHADOW.

        The shadow is deliberately NOT just a darker version of the core - a dark
        orange next to an orange is invisible. Shifting the shadow across the wheel
        (orange against purple, green against violet) is what keeps these colourful
        instead of muddy.

        The highlight is NOT white. Red plus white is pink, so blood highlights lean
        orange-red, fire leans yellow, nature leans lime.

        How much of each a particle gets depends on what that layer IS (see Role):
        light layers are mostly core with a few shadow grains, dark layers are mostly
        shadow. Splitting everything 50/50 made the shadow win - additive purple on
        an orange fire reads as magenta, and Forest's Might came out violet.
    */
    //                                              highlight   core identity  complementary shadow
    static readonly Palette Blood     = new Palette("Blood",      "#FF4B2B", "#C8141E", "#4A1240"); // crimson / wine-violet
    static readonly Palette Nature    = new Palette("Nature",     "#B8E05A", "#4FAE33", "#5B2E8F"); // green / purple
    static readonly Palette Fungal    = new Palette("Fungal",     "#B5D94A", "#8B3FD1", "#16306E"); // violet with sickly-lime glints / deep blue
    static readonly Palette Fire      = new Palette("Fire",       "#FFC23D", "#FF6A1F", "#6B2E8F"); // orange / purple
    static readonly Palette Firework  = new Palette("Firework",   "#FFD35A", "#FF6A1F", "#5B2A8A",  // fire, plus confetti lights
                                                    "#FFD35A", "#FF6A1F", "#FF3D8B", "#3DD6C8");
    static readonly Palette GreekFire = new Palette("Greek fire", "#C8FF6A", "#3BE85A", "#124C6B"); // toxic green / deep teal
    static readonly Palette Gambling  = new Palette("Gambling",   "#FFE38A", "#D8B33C", "#14472B"); // gold / felt green
    static readonly Palette Martial   = new Palette("Martial",    "#FFD27A", "#8FB0E8", "#2A2246"); // steel with brass glints / indigo
    static readonly Palette Ink       = new Palette("Ink",        "#B9B4D6", "#6A6A78", "#241B3A"); // grey / violet-black
    static readonly Palette Mineral   = new Palette("Mineral",    "#FFF1C9", "#D2C4A4", "#4A3B5C"); // sand / violet-brown

    static readonly Palette Revive    = new Palette("Revive",     "#FFF3C4", "#FFC85A", "#6A3F9A"); // dawn gold / the violet of death's door

    // thrown-weapon variants, one per card so each throw is recognisable
    static readonly Palette Contract  = new Palette("Contract",   "#FF6B6B", "#B3122E", "#1E1030"); // contract-wax crimson / ink black-violet
    static readonly Palette Holy      = new Palette("Holy",       "#FFFBEA", "#F2D27A", "#2E6F8F"); // pure pale gold / cool teal
    static readonly Palette Rust      = new Palette("Rust",       "#F08A4B", "#9C2A1A", "#3A2018"); // rusted blood / dried umber
    static readonly Palette Moss      = new Palette("Moss",       "#E8C35A", "#6E9F3A", "#3B2A4F"); // moss green with amber / violet

    // boss identities - every variant of a boss shares its palette
    static readonly Palette Brutus        = new Palette("Brutus",            "#FFB36B", "#D9622B", "#2E3F5C"); // rust / steel navy
    static readonly Palette EssenceFeeder = new Palette("Essence Feeder",    "#E6FBFF", "#6FE0FF", "#3B1C6E"); // spectral cyan / deep violet
    static readonly Palette PoohPowerful  = new Palette("Pooh the Powerful", "#FFE08A", "#F2A93B", "#7A1530"); // royal gold / wine
    static readonly Palette Polyphemus    = new Palette("Polyphemus",        "#FFD36B", "#FF9A2E", "#0F4A47"); // amber eye / sea teal
    static readonly Palette Grimmclaw     = new Palette("Baron Grimmclaw",   "#FF5A6E", "#C2183B", "#2A0F45"); // blood crimson / night violet

    enum Role { Light, Bloom, Paint, Dark, Sprite, Halo }

    // Solid, recognisable sprites. Tinting these with a dark shadow turns them into black silhouettes.
    static readonly HashSet<string> SpriteMaterials = new HashSet<string> {
        "M_SwordGold", "M_SwordBlue", "M_DaggerVFX", "M_ShuffleCard", "M_CardShuffle", "M_ShieldVFX",
        "M_IconDeath", "M_MouseIconImpact", "M_Scry", "M_Precast", "CM_Groupthink", "CM_Bezoar", "M_ChunkBombVFX" };
    // Layers that exist to be the shadow: smoke, black ink, rotating dark rune discs.
    static readonly HashSet<string> DarkMaterials = new HashSet<string> {
        "M_Smoke", "M_DarkBackgroundWave", "M_BackgroundTurn 1", "M_BackgroundTurn", "M_TippedParticle" };

    static Role RoleOf(ParticleSystem ps)
    {
        ParticleSystemRenderer r = ps.GetComponent<ParticleSystemRenderer>();
        Material m = r != null ? r.sharedMaterial : null;
        if (m == null) return Role.Paint;
        if (SpriteMaterials.Contains(m.name)) return Role.Sprite;
        if (DarkMaterials.Contains(m.name) || ps.gameObject.name.StartsWith("Black")) return Role.Dark;
        bool additive = m.HasProperty("_DstBlend") && Mathf.Approximately(m.GetFloat("_DstBlend"), 1f);
        // big additive flashes stack into white and wash out the theme colour
        if (additive && (m.name == "M_BasicAddGlow" || m.name == "M_RainbowOverlay") && ps.main.startSize.constantMax >= 2.5f) return Role.Bloom;
        if (additive) return Role.Light;
        if (m.name == "M_BasicBackground" && ps.main.startSize.constantMax >= 2.5f) return Role.Halo;
        return Role.Paint;
    }

    static ParticleSystem.MinMaxGradient Pick(params (float t, Color c)[] keys)
    {
        Gradient g = new Gradient();
        g.SetKeys(keys.Select(k => new GradientColorKey(k.c, k.t)).ToArray(),
                  new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        // RandomColor samples one point on the gradient per particle, so the key
        // spacing is the probability of each colour.
        return new ParticleSystem.MinMaxGradient(g) { mode = ParticleSystemGradientMode.RandomColor };
    }

    static ParticleSystem.MinMaxGradient ColourFor(Role role, Palette p)
    {
        switch (role)
        {
            case Role.Light:
                if (p.Confetti.Length > 0)
                {
                    Gradient g = new Gradient { mode = GradientMode.Fixed };
                    g.SetKeys(p.Confetti.Select((c, i) => new GradientColorKey(c, (i + 1f) / p.Confetti.Length)).ToArray(),
                              new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
                    return new ParticleSystem.MinMaxGradient(g) { mode = ParticleSystemGradientMode.RandomColor };
                }
                // ~30% highlight, ~55% core, ~15% lifted shadow
                return Pick((0f, p.Highlight), (0.3f, p.Core), (0.85f, p.Core), (1f, Color.Lerp(p.Shadow, p.Core, 0.35f)));
            case Role.Bloom:
                Color core = p.Core; core.a = 0.6f;
                Color warm = Color.Lerp(p.Core, p.Highlight, 0.4f); warm.a = 0.6f;
                return new ParticleSystem.MinMaxGradient(core, warm);
            case Role.Dark:
                return Pick((0f, p.Shadow), (0.65f, p.Shadow), (1f, p.Core));
            case Role.Sprite:
                return new ParticleSystem.MinMaxGradient(Color.Lerp(Color.white, p.Highlight, 0.35f), Color.Lerp(Color.white, p.Core, 0.55f));
            case Role.Halo:
                Color a = p.Core; a.a = 0.55f;
                Color b = Color.Lerp(p.Core, p.Shadow, 0.4f); b.a = 0.55f;
                return new ParticleSystem.MinMaxGradient(a, b);
            default: // Paint
                return Pick((0f, p.Core), (0.55f, p.Core), (1f, p.Shadow));
        }
    }

    static Dictionary<string, Palette> PaletteMap()
    {
        Dictionary<string, Palette> m = new Dictionary<string, Palette>();
        // blood magic - warriors, shamans, healers
        m["Card Effect - Collective Rage"] = Blood;
        m["Card Effect - Retaliation"] = Blood;
        m["Card Effect - Bloodied Blade"] = Blood;
        m["Card Effect - Ritual"] = Blood;
        m["Card Effect - Serrated Waraxe"] = Blood;
        // nature and the uncanny
        m["Card Effect - Forest's Might"] = Nature;
        m["Card Effect - Eldritch Tuber"] = Fungal;
        // fire and volatility
        m["Card Effect - Detonate"] = Fire;
        m["Card Effect - Flamethrower"] = Fire;
        m["Card Effect - Pyrotechnics"] = Firework;
        m["Card Effect - Boomflask"] = Fire;
        m["Card Effect - Trailblaze"] = Fire;
        m["Card Effect - Greek Fire"] = GreekFire;
        // gamblers and card sharps
        m["Card Effect - Groupthink"] = Gambling;
        m["Card Effect - Brainstorm"] = Gambling;
        m["Card Effect - Focus Shield"] = Gambling;
        // martial
        m["Card Effect - Hyperfocused Shot"] = Martial;
        m["Card Effect - Followthrough"] = Martial;
        m["Card Effect - Strength in Numbers"] = Martial;
        // one-offs
        m["Card Effect - Disappear"] = Ink;
        m["Card Effect - Bezoar"] = Mineral;
        // per-step parts split out of the blueprints
        m["Card Effect - Bloodied Blade Bleed"] = Blood;
        m["Card Effect - Serrated Waraxe Bite"] = Blood;
        m["Card Effect - Retaliation Counter"] = Blood;
        m["Card Effect - Trailblaze Draw"] = Fire;
        m["Card Effect - Hyperfocused Shot Bonus"] = Martial;
        m["Card Effect - Followthrough Strength"] = Martial;
        // revive
        m["Companion Revive"] = Revive;
        // thrown-weapon variants (Assets/Prefabs/VFX Experience/Card Variants)
        m["FXE_Dagger_Dagger"] = Martial;
        m["FXE_Dagger_LethalContract"] = Contract;
        m["FXE_Dagger_Pureknife"] = Holy;
        m["FXE_Dagger_JaggedKnife"] = Rust;
        m["FXE_Chunk_Chunkmungus"] = Moss;
        // boss variations
        m["Brutus - Enemy Buff"] = Brutus;
        m["Brutus - Enemy Claw Big Attack"] = Brutus;
        m["Essence Feeder - Strength Buff"] = EssenceFeeder;
        m["Pooh the Powerful - Enemy Buff"] = PoohPowerful;
        m["Polyphemus - Enemy Buff"] = Polyphemus;
        m["Baron Grimmclaw - Enemy Buff"] = Grimmclaw;
        m["Baron Grimmclaw - Enemy Claw Small Attack"] = Grimmclaw;
        return m;
    }

    static string[] BlueprintPaths()
    {
        return PrefabsIn(BlueprintDir);
    }

    static string[] PrefabsIn(string dir)
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { dir });
        return guids.Select(g => AssetDatabase.GUIDToAssetPath(g)).OrderBy(p => p).ToArray();
    }

    // one-off migration, already applied; no menu entry so it can't be rerun over later card edits ("VFX Blueprints/1. Unpack all (break links to source effects)")
    public static void UnpackAll()
    {
        string[] paths = BlueprintPaths();
        int totalUnpacked = 0;
        foreach (string path in paths)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            int n = 0;
            // repeatedly unpack the outermost instances until none remain
            for (int guard = 0; guard < 16; guard++)
            {
                List<GameObject> instances = new List<GameObject>();
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                {
                    GameObject go = t.gameObject;
                    if (go == root) continue;
                    if (!PrefabUtility.IsPartOfPrefabInstance(go)) continue;
                    GameObject outer = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
                    if (outer != null && !instances.Contains(outer)) instances.Add(outer);
                }
                if (instances.Count == 0) break;
                foreach (GameObject inst in instances)
                {
                    PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    n++;
                }
            }
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            totalUnpacked += n;
            Debug.Log("[Blueprints] Unpacked " + n + " nested instance(s) in " + System.IO.Path.GetFileNameWithoutExtension(path));
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[Blueprints] UNPACK COMPLETE - " + totalUnpacked + " instances across " + paths.Length + " blueprints. Edits no longer affect source effects.");
    }

    static bool IsDead(ParticleSystem ps, out string reason)
    {
        reason = "";
        // activeInHierarchy, not activeSelf: a live-looking system under a switched-off
        // parent (e.g. "Allied Big Hit/Old") never renders either.
        if (!ps.gameObject.activeInHierarchy) { reason = "gameobject inactive"; return true; }

        ParticleSystemRenderer r = ps.GetComponent<ParticleSystemRenderer>();
        if (r == null) { reason = "no renderer"; return true; }
        if (!r.enabled) { reason = "renderer disabled"; return true; }
        if (r.sharedMaterial == null) { reason = "no material"; return true; }

        ParticleSystem.EmissionModule em = ps.emission;
        if (!em.enabled) { reason = "emission disabled"; return true; }

        float rate = em.rateOverTime.constantMax;
        float dist = em.rateOverDistance.constantMax;
        int bursts = em.burstCount;
        if (rate <= 0f && dist <= 0f && bursts == 0) { reason = "never emits"; return true; }

        return false;
    }

    // one-off migration, already applied; no menu entry so it can't be rerun over later card edits ("VFX Blueprints/2. Prune particle systems that never emit")
    public static void PruneAll()
    {
        if (!EditorUtility.DisplayDialog("Prune blueprint particle systems",
                "This deletes particle systems that never emit from every blueprint. " +
                "Any hand-made systems that are switched off will be removed too.\n\nContinue?",
                "Prune", "Cancel")) return;
        string[] paths = BlueprintPaths();
        int totalRemoved = 0, totalKept = 0;
        foreach (string path in paths)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);

            ParticleSystem[] all = root.GetComponentsInChildren<ParticleSystem>(true);
            HashSet<GameObject> live = new HashSet<GameObject>();
            List<GameObject> dead = new List<GameObject>();
            List<string> reasons = new List<string>();

            // Sub-emitters are spawned by their parent system, so they legitimately sit
            // inactive or at zero emission. They look dead but aren't - deleting one leaves
            // the parent's Sub Emitters module pointing at nothing.
            HashSet<GameObject> subEmitters = new HashSet<GameObject>();
            foreach (ParticleSystem ps in all)
            {
                ParticleSystem.SubEmittersModule sm = ps.subEmitters;
                for (int i = 0; i < sm.subEmittersCount; i++)
                {
                    ParticleSystem sub = sm.GetSubEmitterSystem(i);
                    if (sub != null) subEmitters.Add(sub.gameObject);
                }
            }

            foreach (ParticleSystem ps in all)
            {
                if (subEmitters.Contains(ps.gameObject)) { live.Add(ps.gameObject); continue; }
                string why;
                if (IsDead(ps, out why)) { dead.Add(ps.gameObject); reasons.Add(ps.gameObject.name + " (" + why + ")"); }
                else live.Add(ps.gameObject);
            }

            // never delete something that still has a live system somewhere beneath it
            int removed = 0;
            foreach (GameObject go in dead)
            {
                if (go == null || go == root) continue;
                bool hasLiveChild = false;
                foreach (ParticleSystem child in go.GetComponentsInChildren<ParticleSystem>(true))
                {
                    if (child != null && child.gameObject != go && live.Contains(child.gameObject)) { hasLiveChild = true; break; }
                }
                if (hasLiveChild) continue;
                Object.DestroyImmediate(go);
                removed++;
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            totalRemoved += removed;
            totalKept += live.Count;
            Debug.Log("[Blueprints] " + name + ": removed " + removed + " dead system(s), kept " + live.Count +
                      (reasons.Count > 0 ? ("\n    " + string.Join("\n    ", reasons.Take(12).ToArray())) : ""));
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[Blueprints] PRUNE COMPLETE - removed " + totalRemoved + ", kept " + totalKept +
                  ". Only provably dead systems were removed; anything that still emits was kept for you to judge.");
    }

    // one-off migration, already applied; no menu entry so it can't be rerun over later card edits ("VFX Blueprints/3. Apply theme palettes")
    public static void ApplyPalettes()
    {
        if (!EditorUtility.DisplayDialog("Apply theme palettes",
                "This overwrites the start colour of every particle system in every blueprint, boss variation and the revive effect. " +
                "Any colours you have tuned by hand will be lost.\n\nContinue?",
                "Overwrite colours", "Cancel")) return;
        Dictionary<string, Palette> map = PaletteMap();
        string[] paths = BlueprintPaths().Concat(PrefabsIn(BossDir)).Concat(PrefabsIn(ReviveDir)).ToArray();
        int tinted = 0;
        foreach (string path in paths)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (!map.ContainsKey(name)) { Debug.LogWarning("[Blueprints] no palette for " + name); continue; }
            Palette pal = map[name];

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            int n = ApplyPalette(root, name);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            tinted += n;
            Debug.Log("[Blueprints] " + name + " -> " + pal.Name + " palette on " + n + " system(s)");
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[Blueprints] PALETTES COMPLETE - " + tinted + " systems tinted via start colour. No shared material was modified.");
    }

    // Tint for plain sprites (SpriteRenderers) in a themed effect: a light wash of the palette so the art still reads.
    public static Color SpriteTint(string effectName, Color current)
    {
        Dictionary<string, Palette> map = PaletteMap();
        if (!map.ContainsKey(effectName)) return current;
        Color c = Color.Lerp(Color.white, map[effectName].Core, 0.45f);
        c.a = current.a;
        return c;
    }

    // Recolours every system under root by its role. Returns how many were tinted (0 if the blueprint has no palette).
    public static int ApplyPalette(GameObject root, string blueprintName)
    {
        Dictionary<string, Palette> map = PaletteMap();
        if (!map.ContainsKey(blueprintName)) return 0;
        Palette pal = map[blueprintName];
        int n = 0;
        foreach (ParticleSystem ps in root.GetComponentsInChildren<ParticleSystem>(true))
        {
            ParticleSystem.MainModule main = ps.main;
            main.startColor = ColourFor(RoleOf(ps), pal);
            n++;
        }
        return n;
    }

    // one-off migration, already applied; no menu entry so it can't be rerun over later card edits ("VFX Blueprints/Run all (unpack, prune, palette)")
    public static void RunAll()
    {
        UnpackAll();
        PruneAll();
        ApplyPalettes();
        Debug.Log("[Blueprints] ALL STEPS DONE.");
    }
}
