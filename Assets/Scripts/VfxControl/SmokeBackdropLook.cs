using System;
using System.Collections;
using UnityEngine;

/*
    VFX: the smoke behind an enemy (Combat Screen Highlight) takes a look chosen for that enemy's design, matched on its
    sprite's name. An enemy always looks the same from fight to fight, and enemies drawn from the same art match (all
    three Brutuses, all the weasels). It swaps the texture the smoke's breakup and drift come from, through a property
    block so the shared material is untouched, and how fast it drifts. Designs with no look keep the material's own.
    Purely visual. The looks are set on WorldspaceEnemy's Combat Screen Highlight (VFX Retool / Wire smoke looks).
*/
public class SmokeBackdropLook : MonoBehaviour
{
    [Serializable]
    public class Look
    {
        public string name;
        [Tooltip("Any of these in the enemy's sprite name picks this look (not case sensitive).")]
        public string[] spriteKeywords = new string[0];
        [Tooltip("The texture the smoke breaks up and drifts through (the material's _FadeTexture and _LERP_Mask_Texture).")]
        public Texture2D breakup;
        [Tooltip("How the smoke drifts (the material's _Fade_Tex_Speed): x sideways, negative y rises.")]
        public Vector2 drift = new Vector2(-0.01f, -0.075f);
        [Tooltip("How much of the smoke survives the breakup (the material's _Cutoff_Value, 0.28 by default): higher thins a bright texture out.")]
        [Range(0f, 1f)] public float cutoff = 0.28f;
    }

    [SerializeField] private Look[] looks = new Look[0];

    private static readonly int FadeTexture = Shader.PropertyToID("_FadeTexture");
    private static readonly int LerpMask = Shader.PropertyToID("_LERP_Mask_Texture");
    private static readonly int FadeSpeed = Shader.PropertyToID("_Fade_Tex_Speed");
    private static readonly int Cutoff = Shader.PropertyToID("_Cutoff_Value");
    // the shader's "World Texture" toggle: the breakup is sampled by world position too, so two enemies with the same
    // look, standing in different places, don't show the same smoke
    private static readonly int WorldTexture = Shader.PropertyToID("_World_Texture_Smoothstep");

    private IEnumerator Start()
    {
        // the enemy is set up by whoever spawned it; give it a few frames
        EnemyInstance enemy = null;
        for (int tries = 0; tries < 30; tries++)
        {
            enemy = GetComponentInParent<EnemyInstance>();
            if (enemy != null && enemy.enemy != null && enemy.enemy.enemyType != null) break;
            yield return null;
        }
        if (enemy == null || enemy.enemy == null || enemy.enemy.enemyType == null || enemy.enemy.enemyType.sprite == null) yield break;
        Look look = Pick(enemy.enemy.enemyType.sprite.name);
        if (look != null) Apply(look);
    }

    private Look Pick(string spriteName)
    {
        foreach (Look look in looks)
            foreach (string keyword in look.spriteKeywords)
                if (!string.IsNullOrEmpty(keyword) && spriteName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) return look;
        return null;
    }

    private void Apply(Look look)
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        foreach (ParticleSystemRenderer r in GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            if (r.sharedMaterial == null || !r.sharedMaterial.HasProperty(FadeTexture)) continue;
            r.GetPropertyBlock(block);
            if (look.breakup != null)
            {
                block.SetTexture(FadeTexture, look.breakup);
                block.SetTexture(LerpMask, look.breakup);
            }
            block.SetVector(FadeSpeed, look.drift);
            block.SetFloat(Cutoff, look.cutoff);
            block.SetFloat(WorldTexture, 1f);
            r.SetPropertyBlock(block);
        }
    }
}
