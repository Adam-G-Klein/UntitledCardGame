using UnityEngine;
using UnityEngine.Playables;

// The runtime instance of a Tween track. It is responsible for blending and setting
// the final data on the transform binding.
public class TweenMixerBehaviour : PlayableBehaviour
{
    static AnimationCurve s_DefaultCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

    bool m_ShouldInitializeTransform = true;
    Vector3 m_InitialPosition;
    bool shouldRotate = false;
    // VFX (Nov 2026): whether a rotating clip placed the binding last frame (see the rotation below)
    bool m_RotatedLastFrame = false;

    // Performs blend of position and rotation of all clips connected to a track mixer
    // The result is applied to the track binding's (playerData) transform.
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Transform trackBinding = playerData as Transform;

        if (trackBinding == null)
            return;

        // Get the initial position and rotation of the track binding, only when ProcessFrame is first called
        InitializeIfNecessary(trackBinding);

        Vector3 accumPosition = Vector3.zero;
        Quaternion accumRotation = QuaternionUtils.zero;

        float totalPositionWeight = 0.0f;
        shouldRotate = false;

        // Iterate on all mixer's inputs (ie each clip on the track)
        int inputCount = playable.GetInputCount();
        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            if (inputWeight <= 0)
                continue;

            Playable input = playable.GetInput(i);
            float normalizedInputTime = (float)(input.GetTime() / input.GetDuration());

            // get the clip's behaviour and evaluate the progression along the curve
            TweenBehaviour tweenInput = GetTweenBehaviour(input);
            float tweenProgress = GetCurve(tweenInput).Evaluate(normalizedInputTime);

            // calculate the position's progression along the curve according to the input's (clip) weight
            totalPositionWeight += inputWeight;
            Vector3 startLocation = tweenInput.GetStartLocation();
            Vector3 endLocation = tweenInput.GetEndLocation();
            accumPosition += TweenPosition(startLocation, endLocation, tweenProgress, inputWeight);

            if (tweenInput.shouldRotate) {
                shouldRotate = true;
                Vector3 direction = (endLocation - startLocation).normalized;
                // Account for any odd positioning of anything on the screen z axis wise
                direction = new Vector3(direction.x, direction.y, 0);
                if (direction.sqrMagnitude > 1e-8f) trackBinding.right = direction;
            }
        }

        Vector3 newPosition = accumPosition + m_InitialPosition * (1.0f - totalPositionWeight);

        // VFX (Nov 2026): follow the frame-to-frame travel only once the shot is already on its path, and only while a
        // rotating clip plays. On a clip's first frame the binding still sits where the experience was spawned (the world
        // origin when the effect has no root, as with Holdem's passive), so that "travel" pointed from mid-screen to the
        // shooter and turned the bullet sideways or backwards; after the clip it pointed back at the spawn point.
        if (shouldRotate && m_RotatedLastFrame) {
            Vector3 direction = newPosition - trackBinding.position;
            // Account for any odd positioning of anything on the screen z axis wise
            direction = new Vector3(direction.x, direction.y, 0);
            if (direction.sqrMagnitude > 1e-8f) trackBinding.right = direction.normalized;
        }
        m_RotatedLastFrame = shouldRotate;

        // Apply the final position and rotation values in the track binding
        trackBinding.position = newPosition;
        // trackBinding.rotation = accumRotation.Blend(m_InitialRotation, 1.0f - totalRotationWeight);
        trackBinding.rotation.Normalize();
    }

    void InitializeIfNecessary(Transform transform)
    {
        if (m_ShouldInitializeTransform)
        {
            m_InitialPosition = transform.position;
            m_ShouldInitializeTransform = false;
        }
    }

    Vector3 TweenPosition(Vector3 startPosition, Vector3 endPosition, float progress, float weight)
    {
        return Vector3.Lerp(startPosition, endPosition, progress) * weight;
    }

    static TweenBehaviour GetTweenBehaviour(Playable playable)
    {
        ScriptPlayable<TweenBehaviour> tweenInput = (ScriptPlayable<TweenBehaviour>)playable;
        return tweenInput.GetBehaviour();
    }

    static AnimationCurve GetCurve(TweenBehaviour tween)
    {
        if (tween == null || tween.curve == null)
            return s_DefaultCurve;
        return tween.curve;
    }
}