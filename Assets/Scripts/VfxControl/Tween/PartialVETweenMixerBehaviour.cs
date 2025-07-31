using System;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

// The runtime instance of a Tween track. It is responsible for blending and setting
// the final data on the transform binding.
public class PartialVETweenMixerBehaviour : PlayableBehaviour
{
    static AnimationCurve s_DefaultCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

    bool m_ShouldInitializeTransform = true;
    Vector3 m_InitialPosition;
    bool shouldRotate = false;

    // Performs blend of position and rotation of all clips connected to a track mixer
    // The result is applied to the track binding's (playerData) transform.
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Vector3 accumPosition = Vector3.zero;

        float totalPositionWeight = 0.0f;

        VisualElement visualElement = null;

        // Iterate on all mixer's inputs (ie each clip on the track)
        int inputCount = playable.GetInputCount();
        int inputUsedCount = inputCount;
        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            if (inputWeight <= 0) {
                inputUsedCount -= 1;
                continue;
            }

            Playable input = playable.GetInput(i);
            float normalizedInputTime = (float)(input.GetTime() / input.GetDuration());

            // get the clip's behaviour and evaluate the progression along the curve
            PartialVETweenBehaviour tweenInput = GetTweenBehaviour(input);
            float tweenProgress = GetCurve(tweenInput).Evaluate(normalizedInputTime);

            // Get the visual element (will just use the first it finds)
            if (visualElement == null) {
                visualElement = tweenInput.GetVisualElement();
            }

            // calculate the position's progression along the curve according to the input's (clip) weight
            totalPositionWeight += inputWeight;
            Vector3 startLocation = GetStartLocation(tweenInput);
            Vector3 endLocation = GetEndLocation(tweenInput);
            accumPosition += TweenPosition(startLocation, endLocation, tweenProgress, inputWeight);
        }

        if (inputUsedCount == 0) return;

        Vector3 newPosition = accumPosition + m_InitialPosition * (1.0f - totalPositionWeight);

        // Apply the final position and rotation values in the track binding
        // trackBinding.position = newPosition;
        visualElement.transform.position = newPosition;
    }

     /**
            Reversed partial means that we want to reverse the side of the tween
            that we don't go all the way to. Regular patial means we go from normal
            start location, some percent along the tween, and then end. Reversed means that
            we start at 1.0-percentage along the tween, and go to the end location.
        **/

    private Vector3 GetStartLocation(PartialVETweenBehaviour tweenInput) {
        if (tweenInput.reversedPartial) {
            return (tweenInput.GetStartLocation() - tweenInput.GetEndLocation()) * (tweenInput.percentage);
        } else {
            return Vector3.zero;
        }
    }

    private Vector3 GetEndLocation(PartialVETweenBehaviour tweenInput) {
        if (tweenInput.reversedPartial) {
            return Vector3.zero;
        } else {
            return (tweenInput.GetEndLocation() - tweenInput.GetStartLocation()) * (tweenInput.percentage);
        }
    }

    Vector3 TweenPosition(Vector3 startPosition, Vector3 endPosition, float progress, float weight)
    {
        return Vector3.Lerp(startPosition, endPosition, progress) * weight;
    }

    static PartialVETweenBehaviour GetTweenBehaviour(Playable playable)
    {
        ScriptPlayable<PartialVETweenBehaviour> tweenInput = (ScriptPlayable<PartialVETweenBehaviour>)playable;
        return tweenInput.GetBehaviour();
    }

    static AnimationCurve GetCurve(PartialVETweenBehaviour tween)
    {
        if (tween == null || tween.curve == null)
            return s_DefaultCurve;
        return tween.curve;
    }
}