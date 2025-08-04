using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


public class ScaleClip : PlayableAsset
{
    public float startingScale;
    public float endingScale;
    public AnimationCurve curve;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        ScriptPlayable<ScaleBehaviour> playable = ScriptPlayable<ScaleBehaviour>.Create(graph);
        ScaleBehaviour scale = playable.GetBehaviour();

        scale.startingScale = startingScale;
        scale.endingScale = endingScale;
        return playable;
    }
}
