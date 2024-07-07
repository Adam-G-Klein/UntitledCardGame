using UnityEngine;
using UnityEngine.Playables;

// The runtime instance of a Tween track. It is responsible for blending and setting
// the final data on the transform binding.
public class PositionMixerBehaviour : PlayableBehaviour
{

    // Performs blend of position and rotation of all clips connected to a track mixer
    // The result is applied to the track binding's (playerData) transform.

    // Not gonna worry about blending nicely, just want to set position
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Transform trackBinding = playerData as Transform;

        if (trackBinding == null)
            return;

        float maxWeight = -1;
        Vector3 finalPosition = trackBinding.position;

        // Iterate on all mixer's inputs (ie each clip on the track)
        int inputCount = playable.GetInputCount();
        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            if (inputWeight <= 0)
                continue;

            Playable input = playable.GetInput(i);
            PositionBehaviour positionBehaviour = GetPositionBehaviour(input);
            Vector3 retrievedPosition = positionBehaviour.GetPosition(finalPosition);

            if (inputWeight > maxWeight) {
                maxWeight = inputWeight;
                finalPosition = retrievedPosition;
            }
        }

        trackBinding.position = finalPosition;
    }

    static PositionBehaviour GetPositionBehaviour(Playable playable)
    {
        ScriptPlayable<PositionBehaviour> positionInput = (ScriptPlayable<PositionBehaviour>)playable;
        return positionInput.GetBehaviour();
    }
}