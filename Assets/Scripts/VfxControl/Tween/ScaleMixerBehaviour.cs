using UnityEngine;
using UnityEngine.Playables;

public class ScaleMixerBehaviour : PlayableBehaviour
{
    private Transform targetTransform;
    private bool shouldInitialize = true;
    public Vector3 initialScale = Vector3.zero;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        targetTransform = playerData as Transform;
        if (targetTransform == null) return;

        int inputCount = playable.GetInputCount();

        float blendedMultiplier = 0f;
        float totalWeight = 0f;

        if (shouldInitialize) {
            shouldInitialize = false;
            initialScale = targetTransform.localScale;
        }

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<ScaleBehaviour> inputPlayable = (ScriptPlayable<ScaleBehaviour>)playable.GetInput(i);
            ScaleBehaviour input = inputPlayable.GetBehaviour();

            float clipMultiplier = Mathf.Lerp(input.startingScale, input.endingScale, (float)(inputPlayable.GetTime() / inputPlayable.GetDuration()));
            blendedMultiplier += clipMultiplier * inputWeight;
            totalWeight += inputWeight;
        }

        if (totalWeight > 0f)
        {
            float finalMultiplier = blendedMultiplier / totalWeight;
            targetTransform.localScale = initialScale * finalMultiplier;
        }
    }
}