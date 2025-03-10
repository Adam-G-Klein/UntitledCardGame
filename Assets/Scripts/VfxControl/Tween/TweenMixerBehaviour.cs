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
                trackBinding.right = direction;
            }
        }

        Vector3 newPosition = accumPosition + m_InitialPosition * (1.0f - totalPositionWeight);

        if (shouldRotate) {
            Vector3 direction = (newPosition - trackBinding.position).normalized;
            // Account for any odd positioning of anything on the screen z axis wise
            direction = new Vector3(direction.x, direction.y, 0);
            trackBinding.right = direction;
        }

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