using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// A track that allows the user to do simple transform movements.
// It demonstrates how to define a custom track mixer in order to support blending of clips.
[TrackColor(1.0f, 0.0f, 0.0f)]
[TrackClipType(typeof(PartialVETweenClip))]
public class PartialVETweenTrack : TrackAsset
{
    // Creates a runtime instance of the track, represented by a PlayableBehaviour.
    // The runtime instance performs mixing on the clips.
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<PartialVETweenMixerBehaviour>.Create(graph, inputCount);
    }
}