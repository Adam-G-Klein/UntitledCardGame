using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/*
    Points the live cards at the retooled effects in Assets/Prefabs/VFX/Card Effects V2.
    Each rule swaps one effect reference on one card:
      - by old prefab: every step on the card that used `oldPrefab` now uses `newPrefab`
      - AOE: only the damage step that hits ALL enemies (CombatEffectStep whose targets come from a
        GetTargets with enemy targets + TargetAllValidTargets) gets the area effect.
    A rule applies to every copy of the card: the release cards (CardsV2, StartingDeckCards) and the demo
    map's copies (DBFCards). AssetRules does the same for other assets by path (powers).
    InstantiatePrefab steps that are switched have their scale reset to 1 (the new effects are authored
    at game scale).

    These are one-off migrations that have been applied to the card data; they have no menu entries, so they
    can't be rerun over later card edits by accident. Each returns a log of every change when called from code.
*/
public static class VFXRetoolWiring
{
    const string V2 = "Assets/Prefabs/VFX/Card Effects V2/Card Effect V2 - ";
    const string Luke = "Assets/Prefabs/VFX/VFX Prefabs - For Luke/";
    const string BP = Luke + "Card Effect Blueprints/Card Effect - ";
    const string AOE = "<all enemies>";

    // card asset name -> (old prefab path or AOE, new effect name)
    static readonly (string card, string old, string nu)[] Rules =
    {
        // area attacks
        ("PB_Flamethrower", BP + "Flamethrower.prefab", "Flamethrower"),
        ("PB_Detonate", BP + "Detonate.prefab", "Detonate"),
        ("PB_Boomflask", BP + "Boomflask.prefab", "Boomflask"),
        ("PB_Gunslinger", AOE, "Gunslinger"),
        ("PB_EbbAndFlow", AOE, "Ebb and Flow"),
        ("PB_LashOut", AOE, "Lash Out"),
        ("PB_Slobber", AOE, "Slobber"),
        // first-pass card effects
        ("PB_Pyrotechnics", BP + "Pyrotechnics.prefab", "Pyrotechnics"),
        ("PB_GreekFire", BP + "Greek Fire.prefab", "Greek Fire Impact"),
        ("PB_GreekFire", Luke + "Allied Small Magic Hit.prefab", "Greek Fire Impact"),   // demo copy
        ("PB_BloodiedBlade", BP + "Bloodied Blade.prefab", "Bloodied Blade"),
        ("PB_Ritual", BP + "Ritual.prefab", "Ritual"),
        ("PB_CollectiveRage", BP + "Collective Rage.prefab", "Collective Rage"),
        ("PB_SerratedWarclaw", BP + "Serrated Waraxe.prefab", "Serrated Waraxe"),
        ("PB_SerratedWarclaw", BP + "Serrated Waraxe Bite.prefab", "Serrated Waraxe Bite"),
        ("PB_StrengthInNumbers", BP + "Strength in Numbers.prefab", "Strength in Numbers"),
        ("RP_ForestsMight", BP + "Forest's Might.prefab", "Forest's Might"),
        ("PB_Bezoar", BP + "Bezoar.prefab", "Bezoar"),
        ("PB_Brainstorm", BP + "Brainstorm.prefab", "Brainstorm"),
        ("PB_Groupthink", BP + "Groupthink.prefab", "Groupthink"),
        ("PB_Disappear", BP + "Disappear.prefab", "Disappear"),
        ("PB_Hyperfocus", BP + "Hyperfocused Shot.prefab", "Hyperfocused Shot"),
        ("PB_Hyperfocus", BP + "Hyperfocused Shot Bonus.prefab", "Hyperfocused Shot Bonus"),
        ("PB_Followthrough", BP + "Followthrough.prefab", "Followthrough"),
        ("PB_Followthrough", BP + "Followthrough Strength.prefab", "Followthrough Strength"),
        // frequently played
        // the starter Ratslaps keep the basic slash; the slap goes to pack cards that earn it
        ("PB_WristFlick", Luke + "Allied Big Hit.prefab", "Wrist Flick"),
        ("PB_ComboPiece", Luke + "Allied Small Hit.prefab", "Combo Piece"),
        ("PB_Frenzy", Luke + "Allied Small Hit.prefab", "Frenzy"),
        ("RP_Start_DoubleSlash", Luke + "Allied Big Hit.prefab", "Double Slash"),
        ("RP_Start_TwoSidedBlade", Luke + "Allied Big Hit.prefab", "Two-sided Blade"),
        ("RP_Start_Thwack", Luke + "Allied Big Hit.prefab", "Thwack"),
        ("RP_Start_PewPew", Luke + "Allied Big Magic Hit.prefab", "Pew Pew"),
        ("RP_Start_HonedBlade", Luke + "Allied Small Hit.prefab", "Honed Blade"),
        ("RP_Start_TacticalJab", Luke + "Allied Small Hit.prefab", "Tactical Jab"),
        ("PB_Mince", Luke + "Allied Rapid Hit.prefab", "Mince"),
        ("PB_Clobber", Luke + "Allied Big Hit.prefab", "Clobber"),
        ("PB_VengefulSlash", Luke + "Allied Big Hit.prefab", "Vengeful Slash"),
        ("PB_BladeVolley", Luke + "Allied Rapid Hit.prefab", "Blade Volley"),
        // signature and big cards that were on generic effects
        ("PB_AceOfSpades", Luke + "Allied Rapid Hit.prefab", "Ace of Spades"),
        ("PB_WorldEater", Luke + "Allied Big Magic Hit.prefab", "World Eater"),
        ("PB_GolemsMight", Luke + "Allied Big Hit.prefab", "Golem's Might"),
        ("PB_OverwhelmingAPM", Luke + "Allied Rapid Hit.prefab", "Overwhelming APM"),
        ("PB_MonteCarlo", Luke + "Filler Fill x Draw x Create x Scry x Gain.prefab", "Monte Carlo"),
        ("PB_Monsoon", Luke + "Allied Block.prefab", "Monsoon"),
        ("RP_ContingencyPlan", Luke + "Allied Block.prefab", "Contingency Plan"),
        ("RP_Aegis", Luke + "Allied Block.prefab", "Aegis"),
        ("PB_Aegis", Luke + "Allied Block.prefab", "Aegis"),   // demo copy
        ("PB_Powernap", Luke + "Strength Buff.prefab", "Powernap"),
        ("PB_LessonInPain", Luke + "Strength Buff.prefab", "Lesson in Pain"),
    };

    // asset path -> (old prefab path, new effect name)
    static readonly (string asset, string old, string nu)[] AssetRules =
    {
        ("Assets/ScriptableObjects/Powers/Retaliation.asset", BP + "Retaliation Counter.prefab", "Retaliation Counter"),
    };

    // cards put back on an earlier effect (by full prefab path): every copy of the card swaps `from` for `to`
    public static readonly (string card, string from, string to)[] Swaps =
    {
        ("PB_Trailblaze", V2 + "Trailblaze.prefab", BP + "Trailblaze.prefab"),
        ("PB_Trailblaze", V2 + "Trailblaze Draw.prefab", BP + "Trailblaze Draw.prefab"),
        ("PB_FocusShield", V2 + "Focus Shield.prefab", BP + "Focus Shield.prefab"),
        ("PB_EldritchTuber", V2 + "Eldritch Tuber.prefab", BP + "Eldritch Tuber.prefab"),
        ("RP_Start_RecklessBlow", V2 + "Reckless Blow.prefab", Luke + "Card Effect - Reckless Blow.prefab"),
        // relic-granting cards have no effect of their own; Retaliation's spiked shield moves to Phalanx's block
        ("PB_Retaliation", V2 + "Retaliation.prefab", ""),
        ("RP_Phalanx", Luke + "Allied Block.prefab", V2 + "Retaliation.prefab"),
    };

    // one-off migration, already applied; no menu entry so it can't be rerun over later card edits ("VFX Retool/Apply swaps")
    public static string ApplySwaps((string card, string from, string to)[] swaps = null)
    {
        var log = new StringBuilder();
        var cardPaths = AssetDatabase.FindAssets("t:CardType", new[] { "Assets/ScriptableObjects/RatPack" }).Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.Contains("/CardsV2/") || p.Contains("/StartingDeckCards/") || p.Contains("/DBFCards/")).ToList();
        foreach (var swap in swaps ?? Swaps)
        {
            GameObject from = AssetDatabase.LoadAssetAtPath<GameObject>(swap.from), to = swap.to == "" ? null : AssetDatabase.LoadAssetAtPath<GameObject>(swap.to);
            if (to == null && swap.to != "") { log.AppendLine("MISSING " + swap.to); continue; }   // "" clears the reference
            foreach (string path in cardPaths.Where(p => Path.GetFileNameWithoutExtension(p) == swap.card))
            {
                var card = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                var so = new SerializedObject(card); int n = 0;
                foreach (var (fp, field, parent) in RefFields(so))
                    if (from != null && PrefabRoot(fp.objectReferenceValue) == from) { fp.objectReferenceValue = to; n++; }
                if (n > 0) { so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(card); AssetDatabase.SaveAssetIfDirty(card); }
                log.AppendLine(swap.card + (path.Contains("/DBFCards/") ? " (demo)" : "") + ": " + n + " -> " + (swap.to == "" ? "(none)" : Path.GetFileNameWithoutExtension(swap.to)));
            }
        }
        return log.ToString();
    }

    // one-off migration, already applied; no menu entry so it can't be rerun over later card edits ("VFX Retool/Wire cards")
    public static string Wire()
    {
        var log = new StringBuilder();
        var cardPaths = AssetDatabase.FindAssets("t:CardType", new[] { "Assets/ScriptableObjects/RatPack" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.Contains("/CardsV2/") || p.Contains("/StartingDeckCards/") || p.Contains("/DBFCards/")).ToList();
        var targets = new List<(string path, string label, List<(string old, string nu)> rules)>();
        foreach (var group in Rules.GroupBy(r => r.card))
        {
            var paths = cardPaths.Where(p => Path.GetFileNameWithoutExtension(p) == group.Key).ToList();
            if (paths.Count == 0) { log.AppendLine("MISSING card " + group.Key); continue; }
            foreach (string path in paths)
                targets.Add((path, group.Key + (path.Contains("/DBFCards/") ? " (demo copy)" : ""), group.Select(r => (r.old, r.nu)).ToList()));
        }
        foreach (var group in AssetRules.GroupBy(r => r.asset))
            targets.Add((group.Key, Path.GetFileNameWithoutExtension(group.Key) + " (power)", group.Select(r => (r.old, r.nu)).ToList()));

        foreach (var target in targets)
        {
            var card = AssetDatabase.LoadAssetAtPath<ScriptableObject>(target.path);
            if (card == null) { log.AppendLine("MISSING asset " + target.path); continue; }
            var so = new SerializedObject(card);
            int changed = 0;
            HashSet<string> aoeKeys = AllEnemyKeys(so);
            foreach (var rule in target.rules)
            {
                GameObject nu = AssetDatabase.LoadAssetAtPath<GameObject>(V2 + rule.nu + ".prefab");
                if (nu == null) { log.AppendLine("MISSING effect " + rule.nu); continue; }
                GameObject old = rule.old == AOE ? null : AssetDatabase.LoadAssetAtPath<GameObject>(rule.old);
                if (rule.old != AOE && old == null) { log.AppendLine("MISSING old " + rule.old); continue; }
                foreach (var (fp, field, parent) in RefFields(so))
                {
                    bool match;
                    if (rule.old == AOE)
                    {
                        SerializedProperty key = so.FindProperty(parent + ".inputKey");
                        match = field == "vfxPrefab" && key != null && aoeKeys.Contains(key.stringValue);
                    }
                    else match = PrefabRoot(fp.objectReferenceValue) == old;
                    if (!match) continue;
                    fp.objectReferenceValue = nu;
                    SerializedProperty scale = so.FindProperty(parent + ".scale");
                    if (field == "prefabToInstantiate" && scale != null) scale.floatValue = 1f;
                    changed++;
                    log.AppendLine(target.label + ": " + field + " -> " + rule.nu);
                }
            }
            if (changed > 0)
            {
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(card);
                AssetDatabase.SaveAssetIfDirty(card);
            }
            else log.AppendLine(target.label + ": NOTHING MATCHED");
        }
        return log.ToString();
    }

    // asset name -> (workflow list holding its play/ability steps, projectile)
    static readonly (string asset, string workflows, bool cannon)[] GunShots =
    {
        ("PB_Gunslinger", "effectWorkflows", false),
        ("PB_Peashooter", "effectWorkflows", false),
        ("PB_Peacemaker", "effectWorkflows", false),
        ("PB_FireFromTheHip", "effectWorkflows", false),
        ("RP_Start_PewPew", "effectWorkflows", false),
        ("PB_Hyperfocus", "effectWorkflows", false),
        ("CreatorCard_SniperRifle", "effectWorkflows", false),
        ("PB_SteamCannon", "effectWorkflows", true),
        ("PB_Howitzer", "effectWorkflows", true),
        ("RP_Holdem", "abilitiesV2", false),
        ("RP_L2_Holdem", "abilitiesV2", false),
        ("RP_L3_Holdem", "abilitiesV2", false),
        ("SD_Holdem", "abilitiesV2", false),
    };

    // Gun cards and abilities fire their shot before the hit: the shooter (the rat whose card it is, or the
    // rat whose ability it is) is found first, then each damage step is preceded by the straight-line shot
    // FX Experience and a wait for it to arrive, so the hit effect plays as the projectile lands.
    // Every copy of each asset is wired; workflows that already fire a shot are left alone.
    // one-off migration, already applied; no menu entry so it can't be rerun over later card edits ("VFX Retool/Wire gun shots")
    public static string WireGunShots()
    {
        var log = new StringBuilder();
        GameObject bullet = AssetDatabase.LoadAssetAtPath<GameObject>(VFXRetoolBuilder.BulletFxPath);
        GameObject cannon = AssetDatabase.LoadAssetAtPath<GameObject>(VFXRetoolBuilder.CannonFxPath);
        if (bullet == null || cannon == null) return "MISSING shot FX Experiences (run VFXRetoolBuilder.BuildGunShots)";
        var shots = new HashSet<GameObject> { bullet, cannon };
        var all = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ScriptableObjects" }).Select(AssetDatabase.GUIDToAssetPath).ToList();
        foreach (var rule in GunShots)
        {
            var paths = all.Where(p => Path.GetFileNameWithoutExtension(p) == rule.asset).ToList();
            if (paths.Count == 0) { log.AppendLine("MISSING " + rule.asset); continue; }
            GameObject fx = rule.cannon ? cannon : bullet;
            float flight = rule.cannon ? VFXRetoolBuilder.CannonFlight : VFXRetoolBuilder.BulletFlight;
            foreach (string path in paths)
            {
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                var so = new SerializedObject(asset);
                var shooterRule = asset is CardType ? GetTargets.SpecialTargetRule.TargetEntityThatDeltCard : GetTargets.SpecialTargetRule.TargetSelf;
                var lists = new List<string>();
                SerializedProperty it = so.GetIterator();
                while (it.Next(true)) if (it.name == "effectSteps" && it.isArray && it.propertyPath.StartsWith(rule.workflows)) lists.Add(it.propertyPath);
                int wired = 0;
                foreach (string listPath in lists)
                {
                    SerializedProperty steps = so.FindProperty(listPath);
                    var damage = new List<(int index, string key)>();
                    bool already = false;
                    for (int i = 0; i < steps.arraySize; i++)
                    {
                        SerializedProperty step = steps.GetArrayElementAtIndex(i);
                        SerializedProperty fxRef = step.FindPropertyRelative("fXExperiencePrefab");
                        if (fxRef != null && IsShot(fxRef.objectReferenceValue)) already = true;
                        if (!(step.managedReferenceValue is CombatEffectStep)) continue;
                        SerializedProperty effect = step.FindPropertyRelative("combatEffect");
                        if (effect == null || !effect.enumNames[effect.enumValueIndex].Contains("Damage")) continue;
                        damage.Add((i, step.FindPropertyRelative("inputKey").stringValue));
                    }
                    if (already || damage.Count == 0) { wired += PointShotsAtTheirHits(steps); continue; }
                    // from the last damage step backwards so earlier indices stay valid
                    for (int d = damage.Count - 1; d >= 0; d--)
                    {
                        int at = damage[d].index;
                        steps.InsertArrayElementAtIndex(at);
                        SerializedProperty fxStep = steps.GetArrayElementAtIndex(at);
                        fxStep.managedReferenceValue = new FXEffectStep();
                        fxStep.FindPropertyRelative("waitForEffect").boolValue = false;
                        fxStep.FindPropertyRelative("fXExperiencePrefab").objectReferenceValue = fx;
                        SerializedProperty map = fxStep.FindPropertyRelative("positionMappingList");
                        map.arraySize = 2;
                        map.GetArrayElementAtIndex(0).FindPropertyRelative("fromWorkflow").stringValue = "shooter";
                        map.GetArrayElementAtIndex(0).FindPropertyRelative("toFxExperience").stringValue = "self";
                        map.GetArrayElementAtIndex(1).FindPropertyRelative("fromWorkflow").stringValue = damage[d].key;
                        map.GetArrayElementAtIndex(1).FindPropertyRelative("toFxExperience").stringValue = "target";
                        steps.InsertArrayElementAtIndex(at + 1);
                        SerializedProperty wait = steps.GetArrayElementAtIndex(at + 1);
                        wait.managedReferenceValue = new WaitForSecondsEffect();
                        wait.FindPropertyRelative("seconds").floatValue = flight;
                    }
                    steps.InsertArrayElementAtIndex(0);
                    SerializedProperty shooter = steps.GetArrayElementAtIndex(0);
                    shooter.managedReferenceValue = new GetTargets();
                    shooter.FindPropertyRelative("outputKey").stringValue = "shooter";
                    shooter.FindPropertyRelative("specialTargetRule").enumValueIndex = (int)shooterRule;
                    wired += damage.Count;
                    PointShotsAtTheirHits(steps);
                }
                if (wired > 0)
                {
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(asset);
                    AssetDatabase.SaveAssetIfDirty(asset);
                }
                log.AppendLine(path.Replace("Assets/ScriptableObjects/", "") + ": " + (wired > 0 ? wired + " change(s) " + (rule.cannon ? "(cannon)" : "(bullet)") : "already wired / nothing to wire"));
            }
        }
        return log.ToString();
    }

    static bool IsShot(Object o) => o != null && o.name.StartsWith("FXE_Shot_");

    // Like the dagger throws, a shot carries the card's hit and plays it as the projectile lands: each shot
    // step followed (after its wait) by a damage step with a hit effect is pointed at the shot variant built
    // around that hit, and the damage step's own hit is cleared so it doesn't play twice.
    static int PointShotsAtTheirHits(SerializedProperty steps)
    {
        int changed = 0;
        for (int i = 0; i + 2 < steps.arraySize; i++)
        {
            SerializedProperty fxRef = steps.GetArrayElementAtIndex(i).FindPropertyRelative("fXExperiencePrefab");
            if (fxRef == null || !IsShot(fxRef.objectReferenceValue)) continue;
            SerializedProperty hit = steps.GetArrayElementAtIndex(i + 2);
            if (!(hit.managedReferenceValue is CombatEffectStep)) continue;
            SerializedProperty vfx = hit.FindPropertyRelative("vfxPrefab");
            GameObject impact = vfx != null ? vfx.objectReferenceValue as GameObject : null;
            if (impact == null) continue;   // already carried by the shot
            bool cannon = fxRef.objectReferenceValue.name.Contains("Cannon");
            fxRef.objectReferenceValue = VFXRetoolBuilder.ShotVariant(cannon, impact);
            vfx.objectReferenceValue = null;
            changed++;
        }
        return changed;
    }

    // Every automatic effect (a unit's passive, a relic/power) shows the trigger cue on its unit: the existing
    // ghost pop plus a badge naming the source. Abilities that already play the cue keep their placement; the
    // rest get it just before their first real effect, i.e. after their checks, so it only shows when the
    // ability actually does something. Relics name themselves (title and icon); units fall back to their own
    // passive's name at run time.
    // one-off migration, already applied; no menu entry so it can't be rerun over later card edits ("VFX Retool/Wire ability trigger cues")
    public static string WireAbilityTriggers()
    {
        var log = new StringBuilder();
        int added = 0, labelled = 0, skipped = 0;
        var sources = new List<(string filter, string list)> { ("t:CompanionTypeSO", "abilitiesV2"), ("t:EnemyTypeSO", "abilities"), ("t:PowerSO", "abilities") };
        foreach (var source in sources)
        {
            foreach (string path in AssetDatabase.FindAssets(source.filter, new[] { "Assets/ScriptableObjects" }).Select(AssetDatabase.GUIDToAssetPath))
            {
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                PowerSO power = asset as PowerSO;
                var so = new SerializedObject(asset);
                var lists = new List<string>();
                SerializedProperty it = so.GetIterator();
                while (it.Next(true)) if (it.name == "effectSteps" && it.isArray && it.propertyPath.StartsWith(source.list + ".")) lists.Add(it.propertyPath);
                bool changed = false;
                foreach (string listPath in lists)
                {
                    SerializedProperty steps = so.FindProperty(listPath);
                    int existing = -1, firstEffect = -1;
                    for (int i = 0; i < steps.arraySize; i++)
                    {
                        object step = steps.GetArrayElementAtIndex(i).managedReferenceValue;
                        if (step == null) continue;
                        if (step is EntityAbilityTriggeredVFX) { existing = i; break; }
                        if (firstEffect < 0 && !IsCheckStep(step)) firstEffect = i;
                    }
                    int at = existing;
                    if (at < 0)
                    {
                        if (firstEffect < 0) { skipped++; continue; }   // nothing it does is visible
                        steps.InsertArrayElementAtIndex(firstEffect);
                        steps.GetArrayElementAtIndex(firstEffect).managedReferenceValue = new EntityAbilityTriggeredVFX();
                        at = firstEffect;
                        added++;
                        changed = true;
                    }
                    if (power != null)
                    {
                        SerializedProperty cue = steps.GetArrayElementAtIndex(at);
                        SerializedProperty label = cue.FindPropertyRelative("label"), icon = cue.FindPropertyRelative("icon");
                        if (label.stringValue != power.title || icon.objectReferenceValue != power.displaySprite)
                        {
                            label.stringValue = power.title;
                            icon.objectReferenceValue = power.displaySprite;
                            labelled++;
                            changed = true;
                        }
                    }
                }
                if (changed)
                {
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(asset);
                    AssetDatabase.SaveAssetIfDirty(asset);
                    log.AppendLine("  " + path.Replace("Assets/ScriptableObjects/", ""));
                }
            }
        }
        return "cues added " + added + ", relic labels set " + labelled + ", abilities with no visible effect skipped " + skipped + "\n" + log;
    }

    // Cards that fire from the hand (discard, draw, exhaust, end-of-turn workflows) flash first, and cards a step
    // changes (cost, damage, block) shimmer right after the change. Runs over every copy; idempotent.
    // one-off migration, already applied; no menu entry so it can't be rerun over later card edits ("VFX Retool/Wire card cues")
    public static string WireCardCues()
    {
        GameObject trig = AssetDatabase.LoadAssetAtPath<GameObject>(V2 + "Card Triggered.prefab");
        GameObject changed = AssetDatabase.LoadAssetAtPath<GameObject>(V2 + "Card Changed.prefab");
        if (trig == null || changed == null) return "MISSING cue prefabs (run VFXRetoolBuilder.BasicCues)";
        int flashes = 0, shimmers = 0, cards = 0;
        string[] triggerFlows = { "onDiscardEffectWorkflow", "onDrawEffectWorkflow", "onExhaustEffectWorkflow", "inPlayerHandEndOfTurnWorkflow" };
        foreach (string path in AssetDatabase.FindAssets("t:CardType", new[] { "Assets/ScriptableObjects/RatPack" }).Select(AssetDatabase.GUIDToAssetPath)
                     .Where(p => p.Contains("/CardsV2/") || p.Contains("/StartingDeckCards/") || p.Contains("/DBFCards/")))
        {
            var card = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            var so = new SerializedObject(card); bool changedAsset = false;
            foreach (string flow in triggerFlows)
            {
                SerializedProperty steps = so.FindProperty(flow + ".effectSteps");
                if (steps == null || steps.arraySize == 0) continue;
                bool has = false; int first = -1;
                for (int i = 0; i < steps.arraySize; i++)
                {
                    object st = steps.GetArrayElementAtIndex(i).managedReferenceValue;
                    if (st is CardTriggeredVFX) { has = true; break; }
                    if (first < 0 && st != null && !IsCheckStep(st)) first = i;
                }
                if (has || first < 0) continue;
                steps.InsertArrayElementAtIndex(first);
                var cue = steps.GetArrayElementAtIndex(first);
                cue.managedReferenceValue = new CardTriggeredVFX();
                cue.FindPropertyRelative("vfxPrefab").objectReferenceValue = trig;
                flashes++; changedAsset = true;
            }
            var lists = new List<string>(); var it = so.GetIterator();
            while (it.Next(true)) if (it.name == "effectSteps" && it.isArray) lists.Add(it.propertyPath);
            foreach (string listPath in lists)
            {
                SerializedProperty steps = so.FindProperty(listPath);
                for (int i = steps.arraySize - 1; i >= 0; i--)
                {
                    SerializedProperty st = steps.GetArrayElementAtIndex(i);
                    if (!(st.managedReferenceValue is CardModificationEffect)) continue;
                    if (i + 1 < steps.arraySize && steps.GetArrayElementAtIndex(i + 1).managedReferenceValue is CardsChangedVFX) continue;
                    string key = st.FindPropertyRelative("inputKey").stringValue;
                    steps.InsertArrayElementAtIndex(i + 1);
                    var cue = steps.GetArrayElementAtIndex(i + 1);
                    cue.managedReferenceValue = new CardsChangedVFX();
                    cue.FindPropertyRelative("inputKey").stringValue = key;
                    cue.FindPropertyRelative("vfxPrefab").objectReferenceValue = changed;
                    shimmers++; changedAsset = true;
                }
            }
            if (changedAsset) { so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(card); AssetDatabase.SaveAssetIfDirty(card); cards++; }
        }
        return "trigger flashes " + flashes + ", change shimmers " + shimmers + ", cards changed " + cards;
    }

    // Enemy moves that hit every rat get one sweep across the row (bosses: Meothra's tail swipe and rake). The move's
    // own hit is kept: the sweep is built around it and plays it on each rat. Re-running rebuilds every sweep around
    // the hit it already carries, so recipe changes reach all of them.
    // one-off migration, already applied; no menu entry so it can't be rerun over later card edits ("VFX Retool/Wire enemy sweeps")
    public static string WireEnemySweeps()
    {
        var log = new StringBuilder();
        foreach (string path in AssetDatabase.FindAssets("t:EnemyTypeSO", new[] { "Assets/ScriptableObjects" }).Select(AssetDatabase.GUIDToAssetPath))
        {
            var enemy = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            var so = new SerializedObject(enemy); int n = 0;
            bool isBoss = enemy.name.Contains("Meothra") || enemy.name.Contains("Meowthra");
            var behaviours = new List<string>(); var it = so.GetIterator();
            while (it.Next(true))
                if (it.name == "enemyTargetMethod" && it.propertyType == SerializedPropertyType.Enum && it.enumNames[it.enumValueIndex] == "AllCompanions")
                    behaviours.Add(it.propertyPath.Substring(0, it.propertyPath.LastIndexOf('.')));
            foreach (string b in behaviours)
            {
                SerializedProperty steps = so.FindProperty(b + ".effectSteps");
                if (steps == null) continue;
                for (int i = 0; i < steps.arraySize; i++)
                {
                    SerializedProperty st = steps.GetArrayElementAtIndex(i);
                    if (!(st.managedReferenceValue is CombatEffectStep)) continue;
                    SerializedProperty effect = st.FindPropertyRelative("combatEffect");
                    if (effect == null || effect.enumNames[effect.enumValueIndex] != "Damage") continue;
                    SerializedProperty vfx = st.FindPropertyRelative("vfxPrefab");
                    GameObject current = vfx.objectReferenceValue as GameObject;
                    bool sweep = current != null && current.name.Contains("Row Sweep");
                    GameObject own = sweep ? VFXRetoolBuilder.CarriedHit(current) : current;
                    GameObject want = VFXRetoolBuilder.EnemySweepVariant(isBoss, own);
                    if (want == null) { log.AppendLine("  MISSING sweep for " + enemy.name); continue; }
                    log.AppendLine("  " + enemy.name + ": " + (own != null ? own.name : "(no hit: small claw)") + " -> " + want.name);
                    if (vfx.objectReferenceValue == want) continue;
                    vfx.objectReferenceValue = want; n++;
                }
            }
            if (n > 0) { so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(enemy); AssetDatabase.SaveAssetIfDirty(enemy); }
        }
        return log.ToString();
    }

    // steps that only look things up or decide whether to go on; the cue goes after these
    static bool IsCheckStep(object step)
    {
        string n = step.GetType().Name;
        return n.StartsWith("Get") || n.StartsWith("EndWorkflow") || n.Contains("Count") || n.Contains("Comparison")
            || n.Contains("Cache") || n.Contains("Math") || n.Contains("Filter") || n == "WaitForSecondsEffect";
    }

    const string GreekFireThrow = "Assets/Prefabs/VFX Experience/Card Variants/FXE_GreekFire_Throw.prefab";

    // Greek Fire's exhaust damage gets the pot throw first: after it picks the random enemy, find the rat whose
    // card it was ("thrower") and play the throw from there to the enemy, waiting for it to land before the
    // damage and impact. Applied to every copy of the card; skipped where the throw is already wired.
    // one-off migration, already applied; no menu entry so it can't be rerun over later card edits ("VFX Retool/Wire Greek Fire throw")
    public static string WireGreekFireThrow()
    {
        var log = new StringBuilder();
        GameObject throwFx = AssetDatabase.LoadAssetAtPath<GameObject>(GreekFireThrow);
        if (throwFx == null) return "MISSING " + GreekFireThrow;
        foreach (string path in AssetDatabase.FindAssets("PB_GreekFire t:CardType", new[] { "Assets/ScriptableObjects/RatPack" }).Select(AssetDatabase.GUIDToAssetPath))
        {
            if (Path.GetFileNameWithoutExtension(path) != "PB_GreekFire") continue;
            var card = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            var so = new SerializedObject(card);
            SerializedProperty steps = so.FindProperty("onExhaustEffectWorkflow.effectSteps");
            if (steps == null || steps.arraySize == 0) { log.AppendLine(path + ": no exhaust workflow"); continue; }
            bool wired = false; int enemyStep = -1;
            for (int i = 0; i < steps.arraySize; i++)
            {
                SerializedProperty step = steps.GetArrayElementAtIndex(i);
                SerializedProperty fx = step.FindPropertyRelative("fXExperiencePrefab");
                if (fx != null && fx.objectReferenceValue == throwFx) wired = true;
                SerializedProperty outKey = step.FindPropertyRelative("outputKey");
                if (enemyStep < 0 && step.managedReferenceValue is GetTargets && outKey != null && outKey.stringValue == "enemy") enemyStep = i;
            }
            if (wired) { log.AppendLine(path + ": already wired"); continue; }
            if (enemyStep < 0) { log.AppendLine(path + ": no enemy GetTargets step"); continue; }

            steps.InsertArrayElementAtIndex(enemyStep + 1);
            SerializedProperty thrower = steps.GetArrayElementAtIndex(enemyStep + 1);
            thrower.managedReferenceValue = new GetTargets();
            thrower.FindPropertyRelative("outputKey").stringValue = "thrower";
            thrower.FindPropertyRelative("specialTargetRule").enumValueIndex = (int)GetTargets.SpecialTargetRule.TargetEntityThatDeltCard;

            steps.InsertArrayElementAtIndex(enemyStep + 2);
            SerializedProperty fxStep = steps.GetArrayElementAtIndex(enemyStep + 2);
            fxStep.managedReferenceValue = new FXEffectStep();
            fxStep.FindPropertyRelative("waitForEffect").boolValue = true;
            fxStep.FindPropertyRelative("fXExperiencePrefab").objectReferenceValue = throwFx;
            SerializedProperty mappings = fxStep.FindPropertyRelative("positionMappingList");
            mappings.arraySize = 2;
            mappings.GetArrayElementAtIndex(0).FindPropertyRelative("fromWorkflow").stringValue = "thrower";
            mappings.GetArrayElementAtIndex(0).FindPropertyRelative("toFxExperience").stringValue = "self";
            mappings.GetArrayElementAtIndex(1).FindPropertyRelative("fromWorkflow").stringValue = "enemy";
            mappings.GetArrayElementAtIndex(1).FindPropertyRelative("toFxExperience").stringValue = "target";

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(card);
            AssetDatabase.SaveAssetIfDirty(card);
            log.AppendLine(path + ": throw wired after step " + enemyStep);
        }
        return log.ToString();
    }

    // HP loss that ignores block (the bleed icon) gets the status looks: a rat losing HP bleeds, Everything Burns
    // burns the foes, Snake Oil and Stinky Cheese poison the rat that eats them. Passive triggers (powers,
    // companion abilities, discard/exhaust triggers) get the lighter status version, played cards the apply version.
    // Only steps showing nothing, an enemy claw, the basic big hit or the old Bloodied Blade drop are switched, so
    // signature effects (Ritual's sigil) stay.
    const string StatusApplyBleed = Luke + "Status Apply Effect Bleed.prefab", StatusBleed = Luke + "Status Effect Bleed.prefab";
    static readonly Dictionary<string, string> StatusLooks = new Dictionary<string, string>
    {
        { "EverythingBurns", Luke + "Status Apply Effect Burn.prefab" },
        { "Bloodfire", Luke + "Status Effect Burn.prefab" },
        { "CreatorCard_GhostPepper", Luke + "Status Apply Effect Burn.prefab" },
        { "Card_SnakeOil", Luke + "Status Apply Poison.prefab" },
        { "Card_StinkyCheese", Luke + "Status Effect Poision.prefab" },
        { "RP_Thistlethorn", StatusBleed },
        { "RP_Mab", StatusBleed }, { "RP_L2_Mab", StatusBleed }, { "RP_L3_Mab", StatusBleed },
        { "RP_Spaulder", StatusBleed }, { "RP_L2_Spaulder", StatusBleed }, { "RP_L3_Spaulder", StatusBleed },
        { "DiscardHealth_CultistsCircle", StatusBleed }, { "HealthExhaust_Bloodletting", StatusBleed },
    };
    static readonly string[] ReplaceableLooks =
    {
        Luke + "Enemy (Red)/Enemy Claw Small Attack.prefab", Luke + "Enemy (Red)/Enemy Claw Rapid Attack.prefab",
        Luke + "Enemy (Red)/Enemy Claw Big Attack.prefab", Luke + "Allied Big Hit.prefab", V2 + "Bloodied Blade Bleed.prefab",
        StatusApplyBleed, StatusBleed,   // so a step can move between status looks when the table above changes
    };
    const int FixedDamageThatIgnoresBlock = 16;

    // one-off migration, already applied; no menu entry so it can't be rerun over later card edits ("VFX Retool/Wire status looks")
    public static string WireStatusLooks()
    {
        var log = new StringBuilder();
        var replaceable = new HashSet<GameObject>(ReplaceableLooks.Select(AssetDatabase.LoadAssetAtPath<GameObject>).Where(g => g != null));
        foreach (string path in AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ScriptableObjects" }).Select(AssetDatabase.GUIDToAssetPath).Distinct())
        {
            if (!path.EndsWith(".asset")) continue;
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null) continue;
            var so = new SerializedObject(asset);
            int changed = 0;
            SerializedProperty it = so.GetIterator();
            var fields = new List<SerializedProperty>();
            while (it.Next(true))
                if (it.propertyType == SerializedPropertyType.ObjectReference && it.name == "vfxPrefab") fields.Add(it.Copy());
            foreach (SerializedProperty fp in fields)
            {
                int dot = fp.propertyPath.LastIndexOf('.');
                if (dot < 0) continue;   // a top-level field, not an effect step
                string parent = fp.propertyPath.Substring(0, dot);
                SerializedProperty effect = so.FindProperty(parent + ".combatEffect");
                if (effect == null || effect.intValue != FixedDamageThatIgnoresBlock) continue;
                GameObject current = PrefabRoot(fp.objectReferenceValue);
                if (current != null && !replaceable.Contains(current)) continue;
                string name = Path.GetFileNameWithoutExtension(path);
                string look = StatusLooks.TryGetValue(name, out string l) ? l : StatusApplyBleed;
                GameObject nu = AssetDatabase.LoadAssetAtPath<GameObject>(look);
                if (nu == null) { log.AppendLine("MISSING " + look); continue; }
                if (current == nu) continue;
                fp.objectReferenceValue = nu; changed++;
                log.AppendLine(name + (path.Contains("/DBFCards/") ? " (demo)" : "") + ": " + (current == null ? "(none)" : current.name) + " -> " + nu.name);
            }
            if (changed == 0) continue;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssetIfDirty(asset);
        }
        return log.ToString();
    }

    // workflows that only grant Temporary Strength show its own buff (the Strength Buff with an hourglass) instead
    // of the permanent Strength Buff; a workflow that also grants permanent Strength keeps the Strength Buff
    const int StatusStrength = 0, StatusTemporaryStrength = 6, CombatEffectStrength = 1;

    // one-off migration, already applied; no menu entry so it can't be rerun over later card edits ("VFX Retool/Wire temporary strength")
    public static string WireTemporaryStrength()
    {
        var log = new StringBuilder();
        GameObject strength = AssetDatabase.LoadAssetAtPath<GameObject>(Luke + "Strength Buff.prefab");
        GameObject temporary = AssetDatabase.LoadAssetAtPath<GameObject>(V2 + "Temporary Strength.prefab");
        if (strength == null || temporary == null) return "MISSING Strength Buff or Temporary Strength";
        foreach (string path in AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ScriptableObjects" }).Select(AssetDatabase.GUIDToAssetPath).Distinct())
        {
            if (!path.EndsWith(".asset")) continue;
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null) continue;
            var so = new SerializedObject(asset);
            var temp = new HashSet<string>(); var permanent = new HashSet<string>();
            SerializedProperty it = so.GetIterator();
            while (it.Next(true))
            {
                if (it.name == "statusEffect" && it.propertyType == SerializedPropertyType.Enum && IsStep(so, it, "ApplyStatus"))
                {
                    if (it.intValue == StatusTemporaryStrength) temp.Add(Workflow(it.propertyPath));
                    if (it.intValue == StatusStrength) permanent.Add(Workflow(it.propertyPath));
                }
                if (it.name == "combatEffect" && it.propertyType == SerializedPropertyType.Enum && it.intValue == CombatEffectStrength) permanent.Add(Workflow(it.propertyPath));
            }
            temp.ExceptWith(permanent);
            if (temp.Count == 0) continue;
            int n = 0;
            foreach (var (fp, field, parent) in RefFields(so))
                if (temp.Contains(Workflow(fp.propertyPath)) && PrefabRoot(fp.objectReferenceValue) == strength) { fp.objectReferenceValue = temporary; n++; }
            if (n == 0) continue;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssetIfDirty(asset);
            log.AppendLine(Path.GetFileNameWithoutExtension(path) + (path.Contains("/DBFCards/") ? " (demo)" : "") + ": " + n + " -> Temporary Strength");
        }
        return log.ToString();
    }

    // InstantiatePrefab multiplies the effect's root and every child by its scale (so x1.5 draws it 2.25x as big):
    // the same buff came out at different sizes on different cards (x1, x1.2, x1.5, up to x5). Every non-area
    // effect spawned this way goes back to x1, the size CombatEffectStep spawns it at
    // one-off migration, already applied; no menu entry so it can't be rerun over later card edits ("VFX Retool/Normalise effect scales")
    public static string NormaliseEffectScales()
    {
        var log = new StringBuilder();
        var counts = new Dictionary<string, int>();
        int assets = 0;
        foreach (string path in AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ScriptableObjects" }).Select(AssetDatabase.GUIDToAssetPath).Distinct())
        {
            if (!path.EndsWith(".asset")) continue;
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null) continue;
            var so = new SerializedObject(asset);
            int n = 0;
            foreach (var (fp, field, parent) in RefFields(so))
            {
                if (field != "prefabToInstantiate") continue;
                GameObject prefab = PrefabRoot(fp.objectReferenceValue);
                if (prefab == null || prefab.GetComponentInChildren<AreaEffect>(true) != null) continue;
                SerializedProperty scale = so.FindProperty(parent + ".scale");
                if (scale == null || Mathf.Approximately(scale.floatValue, 1f)) continue;
                string key = prefab.name + " x" + scale.floatValue.ToString("0.##");
                counts[key] = (counts.TryGetValue(key, out int c) ? c : 0) + 1;
                scale.floatValue = 1f; n++;
            }
            if (n == 0) continue;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssetIfDirty(asset);
            assets++;
        }
        foreach (var kv in counts.OrderByDescending(kv => kv.Value)) log.AppendLine(kv.Value + "  " + kv.Key + " -> x1");
        log.AppendLine(assets + " assets changed");
        return log.ToString();
    }

    // live cards whose status had no effect on the rat: an InstantiatePrefab is added right after the ApplyStatus,
    // on the same target. (asset, status, target key, effect)
    static readonly (string asset, int status, string key, string prefab)[] MissingStatusCues =
    {
        ("PB_IronStomach", 2, "rat", Luke + "Allied Block.prefab"),          // +3 block when the rat took damage
        ("PB_Ironseed_V2", 2, "left", Luke + "Allied Block.prefab"),         // heal (has its effect) + 6 block
        ("HealthAttack_RatYoga", 0, "self", Luke + "Strength Buff.prefab"),  // heal (has its effect) + 2 strength
    };
    // effects played on the wrong rat: (asset, prefab, from key, to key)
    static readonly (string asset, string prefab, string from, string to)[] RetargetCues =
    {
        ("Rejuvenation", Luke + "Strength Buff.prefab", "healedCompanion", "self"),   // the strength goes to self
    };

    // one-off migration, already applied; no menu entry so it can't be rerun over later card edits ("VFX Retool/Add missing status cues")
    public static string AddMissingStatusCues()
    {
        var log = new StringBuilder();
        foreach (var cue in MissingStatusCues)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(cue.prefab);
            foreach (string path in AssetDatabase.FindAssets(cue.asset + " t:ScriptableObject", new[] { "Assets/ScriptableObjects" }).Select(AssetDatabase.GUIDToAssetPath).Where(p => Path.GetFileNameWithoutExtension(p) == cue.asset))
            {
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                var so = new SerializedObject(asset);
                // the ApplyStatus step, and whether its list already shows this effect on that target
                SerializedProperty list = null; int index = -1; bool already = false;
                SerializedProperty it = so.GetIterator();
                while (it.Next(true))
                {
                    if (it.propertyType != SerializedPropertyType.ManagedReference || it.managedReferenceValue == null) continue;
                    SerializedProperty key = it.FindPropertyRelative("inputKey");
                    if (key == null || key.stringValue != cue.key) continue;
                    if (it.managedReferenceValue is InstantiatePrefab && PrefabRoot(it.FindPropertyRelative("prefabToInstantiate").objectReferenceValue) == prefab) already = true;
                    SerializedProperty status = it.FindPropertyRelative("statusEffect");
                    if (it.managedReferenceValue is ApplyStatus && status != null && status.intValue == cue.status)
                    {
                        string p = it.propertyPath; int i = p.LastIndexOf(".Array.data[");
                        list = so.FindProperty(p.Substring(0, i));
                        index = int.Parse(p.Substring(i + 12, p.Length - i - 13));
                    }
                }
                if (already) { log.AppendLine(cue.asset + ": already shows " + prefab.name); continue; }
                if (list == null) { log.AppendLine(cue.asset + ": NO MATCHING ApplyStatus"); continue; }
                list.InsertArrayElementAtIndex(index + 1);
                SerializedProperty step = list.GetArrayElementAtIndex(index + 1);
                step.managedReferenceValue = new InstantiatePrefab();
                step.FindPropertyRelative("inputKey").stringValue = cue.key;
                step.FindPropertyRelative("prefabToInstantiate").objectReferenceValue = prefab;
                step.FindPropertyRelative("scale").floatValue = 1f;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssetIfDirty(asset);
                log.AppendLine(cue.asset + (path.Contains("/DBFCards/") ? " (demo)" : "") + ": + " + prefab.name + " on " + cue.key);
            }
        }
        foreach (var cue in RetargetCues)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(cue.prefab);
            foreach (string path in AssetDatabase.FindAssets(cue.asset + " t:ScriptableObject", new[] { "Assets/ScriptableObjects" }).Select(AssetDatabase.GUIDToAssetPath).Where(p => Path.GetFileNameWithoutExtension(p) == cue.asset))
            {
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                var so = new SerializedObject(asset); int n = 0;
                foreach (var (fp, field, parent) in RefFields(so))
                {
                    SerializedProperty key = so.FindProperty(parent + ".inputKey");
                    if (PrefabRoot(fp.objectReferenceValue) != prefab || key == null || key.stringValue != cue.from) continue;
                    key.stringValue = cue.to; n++;
                }
                if (n == 0) continue;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssetIfDirty(asset);
                log.AppendLine(cue.asset + ": " + prefab.name + " now plays on " + cue.to + " (was " + cue.from + ")");
            }
        }
        return log.ToString();
    }

    // the catch-all filler effect (draw / create / scry / gain) becomes the draw effect wherever it marks a draw:
    // in a step list that draws cards and doesn't also create them
    // one-off migration, already applied; no menu entry so it can't be rerun over later card edits ("VFX Retool/Wire draw effect")
    public static string WireDrawEffect()
    {
        var log = new StringBuilder();
        GameObject filler = AssetDatabase.LoadAssetAtPath<GameObject>(Luke + "Filler Fill x Draw x Create x Scry x Gain.prefab");
        GameObject draw = AssetDatabase.LoadAssetAtPath<GameObject>(V2 + "Draw.prefab");
        if (filler == null || draw == null) return "MISSING filler or draw effect";
        foreach (string path in AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ScriptableObjects" }).Select(AssetDatabase.GUIDToAssetPath).Distinct())
        {
            if (!path.EndsWith(".asset")) continue;
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null) continue;
            var so = new SerializedObject(asset);
            var draws = new HashSet<string>(); var creates = new HashSet<string>();
            SerializedProperty it = so.GetIterator();
            while (it.Next(true))
            {
                if (it.propertyType != SerializedPropertyType.ManagedReference || it.managedReferenceValue == null) continue;
                object step = it.managedReferenceValue;
                if (step is DrawCards) draws.Add(Workflow(it.propertyPath + ".x"));
                if (step is AddCardsToHand || step is AddCardsToDeck) creates.Add(Workflow(it.propertyPath + ".x"));
            }
            draws.ExceptWith(creates);
            if (draws.Count == 0) continue;
            int n = 0;
            foreach (var (fp, field, parent) in RefFields(so))
                if (draws.Contains(Workflow(fp.propertyPath)) && PrefabRoot(fp.objectReferenceValue) == filler) { fp.objectReferenceValue = draw; n++; }
            if (n == 0) continue;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssetIfDirty(asset);
            log.AppendLine(Path.GetFileNameWithoutExtension(path) + (path.Contains("/DBFCards/") ? " (demo)" : "") + ": " + n + " -> Draw");
        }
        return log.ToString();
    }

    // Exhausting a card in hand already plays the card's own exhaust (PlayableCard.CardExhaustVFX: the card
    // dissolves), so the old "Exhaust X Consume" burst some cards also spawn on that card doubled it. Those steps
    // go (an exhaust from the deck, like Cauterize, keeps its burst: there's no card in hand to dissolve).
    // The exhaust dissolve itself goes through the watercolour texture instead of the shared generic noise.
    const int CardInHandExhaust = 1;
    const string ExhaustExperience = "Assets/Prefabs/VFX/CardExhaustVFX.prefab";
    const string ExhaustFadeTexture = "Assets/VFX V3/TN_Watercolour.jpg";

    // one-off migration, already applied; no menu entry so it can't be rerun over later card edits ("VFX Retool/Wire exhaust")
    public static string WireExhaust()
    {
        var log = new StringBuilder();
        GameObject old = AssetDatabase.LoadAssetAtPath<GameObject>(Luke + "Exhaust X Consume.prefab");
        foreach (string path in AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ScriptableObjects" }).Select(AssetDatabase.GUIDToAssetPath).Distinct())
        {
            if (!path.EndsWith(".asset")) continue;
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null) continue;
            var so = new SerializedObject(asset);
            var exhausted = new HashSet<string>();
            var doubled = new List<string>();
            SerializedProperty it = so.GetIterator();
            while (it.Next(true))
            {
                if (it.propertyType != SerializedPropertyType.ManagedReference || it.managedReferenceValue == null) continue;
                SerializedProperty key = it.FindPropertyRelative("inputKey");
                if (key == null) continue;
                if (it.managedReferenceValue is CardInHandEffect && it.FindPropertyRelative("effect").intValue == CardInHandExhaust) exhausted.Add(key.stringValue);
                if (it.managedReferenceValue is InstantiatePrefab && PrefabRoot(it.FindPropertyRelative("prefabToInstantiate").objectReferenceValue) == old) doubled.Add(it.propertyPath + "|" + key.stringValue);
            }
            // delete from the back so earlier indices stay valid
            int n = 0;
            foreach (string entry in doubled.Where(d => exhausted.Contains(d.Split('|')[1])).OrderByDescending(d => StepIndex(d.Split('|')[0])))
            {
                string stepPath = entry.Split('|')[0];
                int i = stepPath.LastIndexOf(".Array.data[");
                SerializedProperty list = so.FindProperty(stepPath.Substring(0, i));
                list.DeleteArrayElementAtIndex(StepIndex(stepPath));
                n++;
            }
            if (n == 0) continue;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssetIfDirty(asset);
            log.AppendLine(Path.GetFileNameWithoutExtension(path) + (path.Contains("/DBFCards/") ? " (demo)" : "") + ": removed " + n + " old exhaust burst");
        }

        GameObject experience = PrefabUtility.LoadPrefabContents(ExhaustExperience);
        try
        {
            ExhaustDissolveTexture swap = experience.GetComponent<ExhaustDissolveTexture>();
            if (swap == null) swap = experience.AddComponent<ExhaustDissolveTexture>();
            var swapSo = new SerializedObject(swap);
            swapSo.FindProperty("fadeTexture").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Texture>(ExhaustFadeTexture);
            swapSo.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(experience, ExhaustExperience);
            log.AppendLine("CardExhaustVFX: dissolves through " + Path.GetFileName(ExhaustFadeTexture));
        }
        finally { PrefabUtility.UnloadPrefabContents(experience); }
        return log.ToString();
    }

    static int StepIndex(string stepPath)
    {
        int i = stepPath.LastIndexOf(".Array.data[");
        return int.Parse(stepPath.Substring(i + 12, stepPath.Length - i - 13));
    }

    // the effect list a step field sits in (its path up to the step's index)
    static string Workflow(string fieldPath)
    {
        int i = fieldPath.LastIndexOf(".Array.data[");
        return i < 0 ? "" : fieldPath.Substring(0, i);
    }

    // is this field on an effect step of the given class (not, say, a status filter or counter)?
    static bool IsStep(SerializedObject so, SerializedProperty field, string stepClass)
    {
        int dot = field.propertyPath.LastIndexOf('.');
        if (dot < 0) return false;
        SerializedProperty step = so.FindProperty(field.propertyPath.Substring(0, dot));
        return step != null && step.propertyType == SerializedPropertyType.ManagedReference
            && step.managedReferenceFullTypename.EndsWith(" " + stepClass);
    }

    // every vfxPrefab / prefabToInstantiate reference in the card's effect steps (managed references)
    static List<(SerializedProperty prop, string field, string parent)> RefFields(SerializedObject so)
    {
        var list = new List<(SerializedProperty, string, string)>();
        SerializedProperty it = so.GetIterator();
        while (it.Next(true))
        {
            if (it.propertyType != SerializedPropertyType.ObjectReference) continue;
            if (it.name != "vfxPrefab" && it.name != "prefabToInstantiate") continue;
            if (it.objectReferenceValue == null) continue;
            string path = it.propertyPath; int dot = path.LastIndexOf('.');
            list.Add((so.FindProperty(path), it.name, dot > 0 ? path.Substring(0, dot) : ""));
        }
        return list;
    }

    static GameObject PrefabRoot(Object o)
    {
        GameObject g = o as GameObject;
        return g == null ? null : g.transform.root.gameObject;
    }

    // output keys of GetTargets steps that target every enemy
    static HashSet<string> AllEnemyKeys(SerializedObject so)
    {
        var keys = new HashSet<string>();
        SerializedProperty it = so.GetIterator();
        while (it.Next(true))
        {
            if (it.name != "specialTargetRule") continue;
            string path = it.propertyPath; string parent = path.Substring(0, path.LastIndexOf('.'));
            SerializedProperty valid = so.FindProperty(parent + ".validTargets");
            SerializedProperty outKey = so.FindProperty(parent + ".outputKey");
            if (valid == null || outKey == null || !valid.isArray) continue;
            bool enemies = false;
            for (int v = 0; v < valid.arraySize; v++) if (valid.GetArrayElementAtIndex(v).intValue == 2) enemies = true;
            if (enemies && it.enumValueIndex == 1) keys.Add(outKey.stringValue);
        }
        return keys;
    }
}
