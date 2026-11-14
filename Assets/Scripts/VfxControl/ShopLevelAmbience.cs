using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
    The shop's ambience follows its upgrade level: a fresh shop is dim and quiet, and each upgrade brings the lights
    up, until the last level has every effect running (and some extras of its own). On an upgrade it eases up to the
    new level over a couple of seconds, so the shop visibly lights up as the upgrade lands.

    How far along the shop is (0 at the first level, 1 at the last) drives:
      background - the shop's backdrop images, tinted from `dimTint` up to white;
      layers     - each ambient particle layer, faded in between its own startAt and fullAt (how many particles
                   it emits and how opaque they are), never below its own minimum. A layer at 0 is fully off: its
                   renderer hidden and any particles it already had cleared (those already alive when the shop
                   opens, or from prewarming, would otherwise keep showing).
    It reads the level from the game state each frame (no shop code calls it). Purely visual. On the shop's
    ShopAmbientVFX object in PlaceholderShopEncounter.
*/
public class ShopLevelAmbience : MonoBehaviour
{
    [Serializable]
    public class Layer
    {
        public ParticleSystem system;
        [Tooltip("How far along the shop (0 first level, 1 last) this layer starts to come in, and where it's full.")]
        [Range(0f, 1f)] public float startAt = 0f;
        [Range(0f, 1f)] public float fullAt = 1f;
        [Tooltip("Its strength before startAt (0: off).")]
        [Range(0f, 1f)] public float minimum = 0f;

        [NonSerialized] public float baseRate;
        [NonSerialized] public ParticleSystem.MinMaxGradient baseColour;
        [NonSerialized] public ParticleSystemRenderer renderer;
        [NonSerialized] public bool rendererWasOn;
    }

    [SerializeField] private GameStateVariableSO gameState;
    [SerializeField] private EncounterConstantsSO encounterConstants;
    [Tooltip("The shop's backdrop images, tinted down when the shop is dim.")]
    [SerializeField] private List<Graphic> background = new List<Graphic>();
    [SerializeField] private Color dimTint = new Color(0.34f, 0.31f, 0.4f, 1f);
    [SerializeField] private List<Layer> layers = new List<Layer>();
    [Tooltip("Seconds to ease up to a new level.")]
    [SerializeField] private float riseTime = 2f;

    private float shown = -1f;

    private void Start()
    {
        foreach (Layer layer in layers)
        {
            if (layer.system == null) continue;
            layer.baseRate = layer.system.emission.rateOverTimeMultiplier;
            layer.baseColour = layer.system.main.startColor;
            layer.renderer = layer.system.GetComponent<ParticleSystemRenderer>();
            layer.rendererWasOn = layer.renderer != null && layer.renderer.enabled;
        }
        shown = Progress();   // a shop opens at its level, not easing up to it
        Apply(shown);
    }

    private void Update()
    {
        float target = Progress();
        if (Mathf.Approximately(shown, target)) return;
        shown = Mathf.MoveTowards(shown, target, Time.deltaTime / riseTime);
        Apply(shown);
    }

    // 0 at the first shop level, 1 at the last
    private float Progress()
    {
        if (gameState == null || encounterConstants == null || encounterConstants.shopLevels.Count < 2) return 1f;
        return Mathf.Clamp01((float)gameState.playerData.GetValue().shopLevel / (encounterConstants.shopLevels.Count - 1));
    }

    private void Apply(float progress)
    {
        Color tint = Color.Lerp(dimTint, Color.white, progress);
        foreach (Graphic image in background) if (image != null) image.color = tint;
        foreach (Layer layer in layers)
        {
            if (layer.system == null) continue;
            float strength = Mathf.Max(layer.minimum, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(layer.startAt, layer.fullAt, progress)));
            bool on = strength > 0.01f;
            if (!on && layer.system.particleCount > 0) layer.system.Clear(false);
            if (layer.renderer != null) layer.renderer.enabled = on && layer.rendererWasOn;
            if (on && !layer.system.isPlaying) layer.system.Play(false);
            var emission = layer.system.emission;
            emission.enabled = on;
            emission.rateOverTimeMultiplier = layer.baseRate * strength;
            var main = layer.system.main;
            main.startColor = Faded(layer.baseColour, strength);
        }
    }

    private static ParticleSystem.MinMaxGradient Faded(ParticleSystem.MinMaxGradient colour, float strength)
    {
        switch (colour.mode)
        {
            case ParticleSystemGradientMode.Color:
                return new ParticleSystem.MinMaxGradient(Alpha(colour.color, strength));
            case ParticleSystemGradientMode.TwoColors:
                return new ParticleSystem.MinMaxGradient(Alpha(colour.colorMin, strength), Alpha(colour.colorMax, strength));
            default:
                return colour;   // gradients keep their own alpha; the emission rate still fades them
        }
    }

    private static Color Alpha(Color c, float strength) { c.a *= strength; return c; }
}
