using UnityEngine;

/*
    Gives each spawned copy of an effect one random rotation, so repeated hits on the same
    target (multi-hit cards, several attacks in a row) don't stack exactly on top of each other.

    Particles are camera-facing billboards, so rotating the transform alone only turns emission
    shapes, velocities and stretched particles; the sprites themselves turn through each system's
    start rotation. The same angle is applied to both so the effect keeps its composition.
*/
public class RandomSpawnRotation : MonoBehaviour
{
    [Tooltip("Maximum rotation either way, in degrees.")]
    [SerializeField] private float maxAngle = 25f;

    private void Awake()
    {
        Apply(this.gameObject, Random.Range(-maxAngle, maxAngle));
    }

    public static void Apply(GameObject root, float angle)
    {
        root.transform.Rotate(0f, 0f, angle, Space.Self);
        // A positive transform Z rotation turns counter-clockwise on screen, while a positive
        // particle start rotation turns clockwise, so the particle offset is negated.
        float radians = -angle * Mathf.Deg2Rad;
        foreach (ParticleSystem ps in root.GetComponentsInChildren<ParticleSystem>(true))
        {
            ParticleSystem.MainModule main = ps.main;
            if (main.startRotation3D) continue;
            ParticleSystem.MinMaxCurve rotation = main.startRotation;
            switch (rotation.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    rotation.constant += radians;
                    break;
                case ParticleSystemCurveMode.TwoConstants:
                    rotation.constantMin += radians;
                    rotation.constantMax += radians;
                    break;
                default:
                    continue; // curve-driven rotation is left as authored
            }
            main.startRotation = rotation;
            // motion set in world space doesn't turn with the transform: turn it by the same angle, or a moving
            // part (a sword sweeping across) starts from the turned spot but still travels the original way
            ParticleSystem.VelocityOverLifetimeModule v = ps.velocityOverLifetime;
            if (v.enabled && v.space == ParticleSystemSimulationSpace.World && v.x.mode == v.y.mode)
            {
                float c = Mathf.Cos(angle * Mathf.Deg2Rad), s = Mathf.Sin(angle * Mathf.Deg2Rad);
                float x = v.xMultiplier, y = v.yMultiplier;
                v.xMultiplier = x * c - y * s;
                v.yMultiplier = x * s + y * c;
            }
        }
    }
}
