using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.9f, 0.4f, 0.4f)]
[TrackClipType(typeof(DialogueClip))]
[TrackBindingType(typeof(DialogueUIController))]
public class DialogueTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<DialogueMixerBehaviour>.Create(graph, inputCount);
    }
}
