# VFX Archive (Unused)

Moved out of `Assets/` on 2026-10-20 because nothing in the project references them, or they only serve content that is not in a live run (live = what the main menu can start: Pooh Bear's Palace of Pain + tutorial, the Steam demo map, the active RatPack packs and companion pool).

Every file kept its `.meta`, so moving one back to the same path under `Assets/` restores it with its original GUID.
Delete this folder once you are happy nothing here is needed.

| Asset (original path) | Why |
|---|---|
| `Assets/Prefabs/VFX/Previews/Allied Small Hit (Pigment Preview).prefab` | Preview of the ink/pigment look; not wired. Revisit later. |
| `Assets/Prefabs/VFX/Previews/Enemy Wind-up (Preview).prefab` | Preview of the enemy wind-up telegraph; not wired (needs the enemy attack timeline). |
| `Assets/VFX V2/Pigment Preview/CM_InkEnso.mat` | Ink detail material; only used by the pigment preview. |
| `Assets/VFX V2/Pigment Preview/CM_InkSplatter.mat` | Ink detail material; only used by the pigment preview. |
| `Assets/VFX V2/Pigment Preview/CM_InkWash.mat` | Ink detail material; only used by the pigment preview. |
| `Assets/VFX V2/Pigment Preview/CTX_InkEnso_2x2.png` | Sumi ensō 2x2 sheet (latest). Revisit with the ink-detail pass. |
| `Assets/VFX V2/Pigment Preview/CTX_InkSplatter_2x2.png` | Sumi splatter 2x2 sheet incl. the symmetric tile (latest). Revisit with the ink-detail pass. |
| `Assets/VFX V2/Pigment Preview/CTX_InkWash_2x2.png` | Superseded sumi wash sheet (replaced by the watercolour wash). |
| `Assets/VFX V2/Pigment Preview/CTX_WatercolourWash_2x2.png` | Wes Anderson watercolour wash 2x2 sheet (latest). Revisit with the ink-detail pass. |
| `Assets/VFX V2/Card Effect Blueprints/CM_Trailblaze.mat` | Blueprint material no longer referenced by the Trailblaze effect. |
| `Assets/VFX V2/New VFX/Screen Fade/CTX_TextInkWeb_Tile.png` | Seamless ink texture for the scene wipe; CM_SceneTransition now uses TN_Watercolour instead. |
| `Assets/VFX V2/New VFX/Words/C Words/CTX_Word_CardsUnlocked.png` | Title word texture that nothing references. |
| `Assets/Prefabs/VFX Experience/FXE_Dagger.prefab` | Base dagger throw; every dagger card now uses its own variant in Card Variants/. |
| `Assets/Prefabs/VFX/VFX Prefabs - For Luke/Boss Variations/Baron Grimmclaw/Baron Grimmclaw - Enemy Buff.prefab` | Baron Grimmclaw is not in any content. |
| `Assets/Prefabs/VFX/VFX Prefabs - For Luke/Boss Variations/Baron Grimmclaw/Baron Grimmclaw - Enemy Claw Small Attack.prefab` | Baron Grimmclaw is not in any content. |
| `Assets/Prefabs/VFX/VFX Prefabs - For Luke/Boss Variations/Essence Feeder/Essence Feeder - Strength Buff.prefab` | Essence Feeder is not in any live encounter pool. |
| `Assets/Prefabs/VFX/VFX Prefabs - For Luke/Boss Variations/Polyphemus/Polyphemus - Enemy Buff.prefab` | Polyphemus is not in any live encounter pool. |
| `Assets/Prefabs/VFX/VFX Prefabs - For Luke/Boss Variations/Pooh the Powerful/Pooh the Powerful - Enemy Buff.prefab` | Pooh the Powerful (Act 2 elite pool) is not referenced by the current map. |
| `Assets/VFX V3/vecteezy_bubble-watercolor-hand-paint_10335936(1).png` | Unreferenced bubble texture (poison set). |
| `Assets/VFX V3/vecteezy_bubble-watercolor-hand-paint_10335936(3).png` | Unreferenced bubble texture (poison set). |
| `Assets/VFX V2/New VFX/Screen Fade/M_InkWipeTransition.mat` | Original screen-fade material; the game uses CM_SceneTransition. |
| `Assets/VFX V2/New VFX/Words/Splotch1(1).png` | Unreferenced. |
| `Assets/VFX V3/Circle_61.png` | Unreferenced. |
| `Assets/VFX V3/IMG_1381(1).png` | Unreferenced. |
| `Assets/VFX V3/IMG_1381(2).png` | Unreferenced. |
| `Assets/VFX V3/IMG_1381(3)(3).png` | Unreferenced. |
| `Assets/VFX V3/IMG_1381(3)(5).png` | Unreferenced. |
| `Assets/VFX V3/IMG_1381(3)(7).png` | Unreferenced. |
| `Assets/VFX V3/IMG_1381(3)(8).png` | Unreferenced. |
| `Assets/VFX V3/IMG_1381(3).png` | Unreferenced. |
| `Assets/VFX V3/IMG_1381(4).png` | Unreferenced. |
| `Assets/VFX V3/IMG_1381(5).png` | Unreferenced. |
| `Assets/VFX V3/IMG_1381(6).png` | Unreferenced. |
| `Assets/VFX V3/New Project(14).png` | Unreferenced. |
| `Assets/VFX V3/New Project(18).png` | Unreferenced. |
| `Assets/VFX V3/New Project(19).png` | Unreferenced. |
| `Assets/VFX V3/TG_Mask_Gradient_WB_Shallow 7.png` | Unreferenced. |
| `Assets/VFX V3/TX_Circle_43 1.png` | Unreferenced. |
| `Assets/VFX V3/TX_Embers.png` | Unreferenced. |
| `Assets/VFX V3/TX_FireIcon.png` | Unreferenced. |
| `Assets/VFX V3/flame_06.png` | Unreferenced. |
| `Assets/VFX V3/Models/SwordSlashStraight.fbx` | Unreferenced. |
| `Assets/VFX V3/Models/Untitled 1.fbx` | Unreferenced. |

Folders removed because they were left empty: `Assets/Prefabs/VFX/Previews`, `Assets/VFX V2/Pigment Preview`, `Assets/Prefabs/VFX/VFX Prefabs - For Luke/Boss Variations/Baron Grimmclaw`, `Assets/Prefabs/VFX/VFX Prefabs - For Luke/Boss Variations/Essence Feeder`, `Assets/Prefabs/VFX/VFX Prefabs - For Luke/Boss Variations/Polyphemus`, `Assets/Prefabs/VFX/VFX Prefabs - For Luke/Boss Variations/Pooh the Powerful`

Restored since: `Allied Big Magic Hit.prefab`, `Allied Small Magic Hit.prefab` (now used by magic-themed cards).
Restored since (2026-10-26): the six Status Effect / Status Apply prefabs for Bleed, Burn and Poison, with M_Bubble, M_StatusPoison and the bubble texture they use. Bleed is now the look for every rat losing HP (the bleed icon); Burn is on Everything Burns, Bloodfire and Ghost Pepper; Poison is on Snake Oil and Stinky Cheese (VFXRetoolWiring.WireStatusLooks).
| `Assets/VFX V2/Shop Premium Envelope/CM_PremiumEnvelopeSheen.mat` | Frost-looking premium envelope sheen; replaced by CM_PremiumEnvelopeShine (gloss streak + ShineSweep). |
| `Assets/Prefabs/VFX/Card Effects V2/Card Effect V2 - Ratslap Discard.prefab` | Starter Ratslaps went back to the basic slash; the slap now lives on Wrist Flick (upgraded). |
| `Assets/Prefabs/VFX/Card Effects V2/Card Effect V2 - Ratslap Brawler.prefab` | Starter Ratslaps went back to the basic slash; the slap now lives on Combo Piece (upgraded). |
| `Assets/Prefabs/VFX/Card Effects V2/Card Effect V2 - Ratslap Blood.prefab` | Starter Ratslaps went back to the basic slash; the slap now lives on Frenzy (upgraded). |
| `Assets/Prefabs/VFX/Card Effects V2/Card Effect V2 - Ratslap Exhaust.prefab` | Starter Ratslaps went back to the basic slash; no Fireshaper attack suits a slap. |
| `Assets/Prefabs/VFX/Card Effects V2/Card Effect V2 - Ratslap Forest.prefab` | Starter Ratslaps went back to the basic slash; no Forestfolk attack suits a slap. |
| `Assets/VFX V3/Card Effects V2/CM_V2_Paw_Exhaust.mat` | Paw material only used by the archived Ratslap Exhaust. |
| `Assets/VFX V3/Card Effects V2/CM_V2_Paw_Forest.mat` | Paw material only used by the archived Ratslap Forest. |
| `Assets/Prefabs/VFX/Card Effects V2/Card Effect V2 - Trailblaze.prefab` | Card went back to its earlier effect (your notes, 2026-10-25). |
| `Assets/Prefabs/VFX/Card Effects V2/Card Effect V2 - Trailblaze Draw.prefab` | Card went back to its earlier effect (your notes, 2026-10-25). |
| `Assets/Prefabs/VFX/Card Effects V2/Card Effect V2 - Focus Shield.prefab` | Card went back to its earlier effect (your notes, 2026-10-25). |
| `Assets/Prefabs/VFX/Card Effects V2/Card Effect V2 - Eldritch Tuber.prefab` | Card went back to its earlier effect (your notes, 2026-10-25). |
| `Assets/Prefabs/VFX/Card Effects V2/Card Effect V2 - Reckless Blow.prefab` | Card went back to its earlier effect (your notes, 2026-10-25). |
| `Assets/VFX V3/Card Effects V2/CM_V2_AceCard.mat` | Merged into CM_V2_Card (one card material; each effect's tint is now on its particle colour). |
| `Assets/VFX V3/Card Effects V2/CM_V2_BloodCard.mat` | Merged into CM_V2_Card (one card material; each effect's tint is now on its particle colour). |
| `Assets/VFX V3/Card Effects V2/CM_V2_FlickedCard.mat` | Merged into CM_V2_Card (one card material; each effect's tint is now on its particle colour). |
| `Assets/VFX V3/Card Effects V2/CM_V2_GoldCard.mat` | Merged into CM_V2_Card (one card material; each effect's tint is now on its particle colour). |
| `Assets/VFX V3/Card Effects V2/CM_V2_PlanCard.mat` | Merged into CM_V2_Card (one card material; each effect's tint is now on its particle colour). |
| `Assets/VFX V3/Card Effects V2/CM_V2_VanishCard.mat` | Merged into CM_V2_Card (one card material; each effect's tint is now on its particle colour). |
| `Assets/VFX V3/Card Effects V2/CM_V2_AegisShield.mat` | Merged into CM_V2_Shield (tint moved to particle colour). |
| `Assets/VFX V3/Card Effects V2/CM_V2_SteelShield.mat` | Merged into CM_V2_Shield (tint moved to particle colour). |
| `Assets/VFX V3/Card Effects V2/CM_V2_WaterShield.mat` | Merged into CM_V2_Shield (tint moved to particle colour). |
| `Assets/VFX V3/Card Effects V2/CM_V2_BloodBlade.mat` | Merged into CM_V2_Blade (tint moved to particle colour). |
| `Assets/VFX V3/Card Effects V2/CM_V2_HonedBlade.mat` | Merged into CM_V2_Blade (tint moved to particle colour). |
| `Assets/VFX V3/Card Effects V2/CM_V2_SteelBlade.mat` | Merged into CM_V2_Blade (tint moved to particle colour). |
| `Assets/VFX V3/Card Effects V2/CM_V2_Paw_Blood.mat` | Merged into CM_V2_Paw (tint moved to particle colour). |
| `Assets/VFX V3/Card Effects V2/CM_V2_Paw_Brawler.mat` | Merged into CM_V2_Paw (tint moved to particle colour). |
| `Assets/VFX V3/Card Effects V2/CM_V2_Paw_Discard.mat` | Merged into CM_V2_Paw (tint moved to particle colour). |
| `Assets/VFX V3/Card Effects V2/CM_V2_StoneFist.mat` | Merged into CM_V2_Fist (tint moved to particle colour). |
| `Assets/VFX V3/Card Effects V2/CM_V2_EmbersArcane.mat` | Merged into CM_V2_Embers (tint moved to particle colour). |
| `Assets/VFX V3/Card Effects V2/CM_V2_EmbersGreen.mat` | Merged into CM_V2_Embers (tint moved to particle colour). |
| `Assets/VFX V3/Card Effects V2/CM_V2_ReviveHalo.mat` | Merged into CM_V2_Sigil (tint moved to particle colour). |
| `Assets/VFX V3/Card Effects V2/CM_V2_ArcaneFlame.mat` | Unreferenced: left over from earlier drafts or archived V2 recipes. |
| `Assets/VFX V3/Card Effects V2/CM_V2_Blaze.mat` | Unreferenced: left over from earlier drafts or archived V2 recipes. |
| `Assets/VFX V3/Card Effects V2/CM_V2_BulletTrail.mat` | Unreferenced: left over from earlier drafts or archived V2 recipes. |
| `Assets/VFX V3/Card Effects V2/CM_V2_Crosshair.mat` | Unreferenced: left over from earlier drafts or archived V2 recipes. |
| `Assets/VFX V3/Card Effects V2/CM_V2_EmberCard.mat` | Unreferenced: left over from earlier drafts or archived V2 recipes. |
| `Assets/VFX V3/Card Effects V2/CM_V2_FlameJet.mat` | Unreferenced: left over from earlier drafts or archived V2 recipes. |
| `Assets/VFX V3/Card Effects V2/CM_V2_FocusShield.mat` | Unreferenced: left over from earlier drafts or archived V2 recipes. |
| `Assets/VFX V3/Card Effects V2/CM_V2_InkSlug.mat` | Unreferenced: left over from earlier drafts or archived V2 recipes. |
| `Assets/VFX V3/Card Effects V2/CM_V2_JetFlame.mat` | Unreferenced: left over from earlier drafts or archived V2 recipes. |
| `Assets/VFX V3/Card Effects V2/CM_V2_Tuber.mat` | Unreferenced: left over from earlier drafts or archived V2 recipes. |
| `Assets/VFX V3/Card Effects V2/CM_V2_SumiHair.mat` | Sumi-e enemy sweep (dry-brush hairline); the sweeps went back to the red slash / violet rake style. |
| `Assets/VFX V3/Card Effects V2/CM_V2_SumiStroke.mat` | Sumi-e enemy sweep (brushstroke); the sweeps went back to the red slash / violet rake style. |
| `Assets/VFX V3/Card Effects V2/CM_V2_SumiSplatter.mat` | Sumi-e enemy sweep (splatter); the sweeps went back to the red slash / violet rake style. |
| `Assets/VFX V3/Card Effects V2/CM_V2_SumiEnso.mat` | Sumi-e enemy sweep (ensō); the sweeps went back to the red slash / violet rake style. |
| `Assets/VFX V3/Card Effects V2/CM_V2_Hanko.mat` | Sumi-e enemy sweep (prey seal); the sweeps went back to the red slash / violet rake style. |
| `Assets/VFX V3/Card Effects V2/CM_V2_PredatorEye.mat` | Sumi-e enemy sweep (predator eyes); the sweeps went back to the red slash / violet rake style. |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_HankoSeal.png` | Placeholder hanko seal for the sumi enemy sweep (not used). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_PredatorEye.png` | Placeholder slit-pupil eye for the sumi enemy sweep (not used). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_BezoarStone.png` (now in Placeholders/Superseded/) | Flat placeholder silhouette, replaced in place by the painted version (sheet 1). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_BloodDrop.png` (now in Placeholders/Superseded/) | Flat placeholder silhouette, replaced in place by the painted version (sheet 1). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_Bomb.png` (now in Placeholders/Superseded/) | Flat placeholder silhouette, replaced in place by the painted version (sheet 1). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_Club.png` (now in Placeholders/Superseded/) | Flat placeholder silhouette, replaced in place by the painted version (sheet 1). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_Crosshair.png` (now in Placeholders/Superseded/) | Flat placeholder silhouette, replaced in place by the painted version (sheet 1). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_DoubleBlade.png` (now in Placeholders/Superseded/) | Flat placeholder silhouette, replaced in place by the painted version (sheet 1). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_Fist.png` (now in Placeholders/Superseded/) | Flat placeholder silhouette, replaced in place by the painted version (sheet 1). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_Flask.png` (now in Placeholders/Superseded/) | Flat placeholder silhouette, replaced in place by the painted version (sheet 1). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_GreekFirePot.png` (now in Placeholders/Superseded/) | Flat placeholder silhouette, replaced in place by the painted version (sheet 2). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_OakLeaf.png` (now in Placeholders/Superseded/) | Flat placeholder silhouette, replaced in place by the painted version (sheet 2). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_Paw.png` (now in Placeholders/Superseded/) | Flat placeholder silhouette, replaced in place by the painted version (sheet 2). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_Spade.png` (now in Placeholders/Superseded/) | Flat placeholder silhouette, replaced in place by the painted version (sheet 2). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_SpikedShield.png` (now in Placeholders/Superseded/) | Flat placeholder silhouette, replaced in place by the painted version (sheet 2). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_Waraxe.png` (now in Placeholders/Superseded/) | Flat placeholder silhouette, replaced in place by the painted version (sheet 2). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_Zzz.png` (now in Placeholders/Superseded/) | Flat placeholder silhouette, replaced in place by the painted version (sheet 2). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_MeothraTail.png` (now in Placeholders/Superseded/) | Generated tail silhouette, replaced in place by the painted tail. |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_BezoarStone.png` (now in Placeholders/Superseded/Painted/) | Painted version before the whimsical pass (3.5 tones + pink outline twin). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_BloodDrop.png` (now in Placeholders/Superseded/Painted/) | Painted version before the whimsical pass (3.5 tones + pink outline twin). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_Bomb.png` (now in Placeholders/Superseded/Painted/) | Painted version before the whimsical pass (3.5 tones + pink outline twin). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_Club.png` (now in Placeholders/Superseded/Painted/) | Painted version before the whimsical pass (3.5 tones + pink outline twin). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_Fist.png` (now in Placeholders/Superseded/Painted/) | Painted version before the whimsical pass (3.5 tones + pink outline twin). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_Flask.png` (now in Placeholders/Superseded/Painted/) | Painted version before the whimsical pass (3.5 tones + pink outline twin). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_GreekFirePot.png` (now in Placeholders/Superseded/Painted/) | Painted version before the whimsical pass (3.5 tones + pink outline twin). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_OakLeaf.png` (now in Placeholders/Superseded/Painted/) | Painted version before the whimsical pass (3.5 tones + pink outline twin). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_Paw.png` (now in Placeholders/Superseded/Painted/) | Painted version before the whimsical pass (3.5 tones + pink outline twin). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_PawClaws.png` (now in Placeholders/Superseded/Painted/) | Painted version before the whimsical pass (3.5 tones + pink outline twin). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_Spade.png` (now in Placeholders/Superseded/Painted/) | Painted version before the whimsical pass (3.5 tones + pink outline twin). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_SpikedShield.png` (now in Placeholders/Superseded/Painted/) | Painted version before the whimsical pass (3.5 tones + pink outline twin). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_Zzz.png` (now in Placeholders/Superseded/Painted/) | Painted version before the whimsical pass (3.5 tones + pink outline twin). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_DoubleBlade.png` (now in Placeholders/Superseded/Painted/) | Painted version before the whimsical pass (3.5 tones + pink outline twin). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_Waraxe.png` (now in Placeholders/Superseded/Painted/) | Painted version before the whimsical pass (3.5 tones + pink outline twin). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_Crosshair.png` (now in Placeholders/Superseded/Painted/) | Painted version before the whimsical pass (3.5 tones + pink outline twin). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_MeothraTail.png` (now in Placeholders/Superseded/Painted/) | Painted version before the whimsical pass (3.5 tones + pink outline twin). |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_*.png` (17 files, now in Placeholders/Superseded/Whimsy/) | Whimsical pass (3.5 tones), scrapped for the hand-tweaked sheets. |
| `Assets/VFX V3/Card Effects V2/Placeholders/CTX_*_Outline.png` (16 files, now in Placeholders/Superseded/Whimsy/) | Pink outline twin textures, scrapped with the whimsical pass. |
| `Assets/Scripts/VfxControl/OutlineTwin.cs` | Kept the pink outline twin layers in sync with their icons; scrapped with the whimsical pass. |
| `Assets/VFX V3/Card Effects V2/CM_V2_Outline_*.mat` (17) | Materials for the pink outline twin layers; scrapped with the whimsical pass. |

| `Assets/Prefabs/VFX/Card Effects V2/Card Effect V2 - Bloodied Blade Bleed.prefab` | Replaced by the restored Status Apply Effect Bleed, which is now the bleed for every card (2026-10-26). |
| `Assets/VFX V3/Card Effects V2/CM_V2_DrawCardGlow.mat` | The drawn-card aura in the draw effect, removed (the glow now goes on the real card in hand: CM_V2_DrawnCardRim, 2026-10-26). |
| `Assets/VFX V2/New VFX/Screen Fade/M_CardFlicker.mat` | Old screen-fade card flicker; nothing references it. It had been deleted outright; restored here from git (2026-10-27). |
| `Assets/VFX V2/New VFX/Screen Fade/CardFlicker.shadergraph` | The shader that became `InkWipeTransition.shadergraph` (the scene-transition wipe), which kept its GUID. This is the old copy for reference only: **don't move it back into Assets** (two assets would share one GUID). |
| `Assets/Editor/VFXRetoolWiring.cs` | The one-off card-wiring migrations (effect slots, scales, status looks, draw, exhaust). All applied; archived so the programmers don't compile a tool that names their classes (a rename would otherwise block play mode). Move it back to `Assets/Editor/` to run a new migration (2026-10-27). |
