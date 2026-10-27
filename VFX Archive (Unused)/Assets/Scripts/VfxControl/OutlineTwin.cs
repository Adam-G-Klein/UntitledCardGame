using UnityEngine;

// The outline layer of a painted icon: a twin of the icon's particle system drawn behind it in one fixed colour.
// Both get the same fresh random seed on every spawn, so each copy of the effect still varies but the outline
// never drifts off its icon (spawn rotation and flips already treat every layer alike).
[RequireComponent(typeof(ParticleSystem))]
public class OutlineTwin : MonoBehaviour
{
    [SerializeField] private ParticleSystem icon;

    private void Awake()
    {
        if (icon == null) return;
        ParticleSystem twin = GetComponent<ParticleSystem>();
        uint seed = (uint)Random.Range(1, int.MaxValue);
        foreach (ParticleSystem ps in new[] { icon, twin })
        {
            bool play = ps.isPlaying || ps.main.playOnAwake;
            ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.useAutoRandomSeed = false;
            ps.randomSeed = seed;
            if (play) ps.Play(false);
        }
    }
}
