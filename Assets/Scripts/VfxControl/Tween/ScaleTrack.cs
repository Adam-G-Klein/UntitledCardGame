using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackBindingType(typeof(Transform))]
[TrackClipType(typeof(ScaleClip))]
public class ScaleTrack : TrackAsset
{
    // Creates a runtime instance of the track, represented by a PlayableBehaviour.
    // The runtime instance performs mixing on the clips.
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<ScaleMixerBehaviour>.Create(graph, inputCount);
    }
}
