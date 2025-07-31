using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// Represents the serialized data for a clip on the Tween track
[Serializable]
[DisplayName("Partial Tween Clip")]
public class PartialVETweenClip : 
        PlayableAsset,
        IFXExperienceClip
{
    [Header("The keys to get start and end at runtime.")]
    public string startLocationKey;
    public string endLocationKey;

    [Header("The key for the visual element name to query for.")]
    public string visualElementKey;

    [Header("The percentage of the tween to move")]
    public float percentage;

    [Header("Reverse the direction that the tween is cut off")]
    public bool reversed;

    public ExposedReference<FXExperience> experienceReference;

    [Tooltip("Only keys in the [0,1] range will be used")]
    public AnimationCurve curve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

    // Creates the playable that represents the instance of this clip.
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        // create a new TweenBehaviour
        ScriptPlayable<PartialVETweenBehaviour> playable = ScriptPlayable<PartialVETweenBehaviour>.Create(graph);
        PartialVETweenBehaviour tween = playable.GetBehaviour();

        tween.fXExperience = experienceReference.Resolve(graph.GetResolver());
        tween.endLocationKey = endLocationKey;
        tween.startLocationKey = startLocationKey;
        tween.percentage = percentage;
        tween.reversedPartial = reversed;
        tween.visualElementKey = visualElementKey;

        // set the behaviour's data
        tween.curve = curve;

        return playable;
    }

    public ExposedReference<FXExperience> GetExposedReference()
    {
        return experienceReference;
    }
}