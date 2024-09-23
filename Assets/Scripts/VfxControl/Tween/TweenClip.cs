using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// Represents the serialized data for a clip on the Tween track
[Serializable]
[DisplayName("Tween Clip")]
public class TweenClip : 
        PlayableAsset,
        ITimelineClipAsset,
        IPropertyPreview,
        IFXExperienceClip
{
    [Header("These values are only used when modifying the clip in the editor.")]
    public Vector3 startLocation;
    public Vector3 endLocation;

    [Header("The keys to get start and end at runtime.")]
    public string startLocationKey;
    public string endLocationKey;

    [Header("Should rotate the trasnform to point towards the end point")]
    public bool shouldRotate = true;

    public ExposedReference<FXExperience> experienceReference;

    [Tooltip("Only keys in the [0,1] range will be used")]
    public AnimationCurve curve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

    // Implementation of ITimelineClipAsset. This specifies the capabilities of this timeline clip inside the editor.
    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending; }
    }

    // Creates the playable that represents the instance of this clip.
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        // create a new TweenBehaviour
        ScriptPlayable<TweenBehaviour> playable = ScriptPlayable<TweenBehaviour>.Create(graph);
        TweenBehaviour tween = playable.GetBehaviour();

        tween.fXExperience = experienceReference.Resolve(graph.GetResolver());
        tween.editorStartLocation = startLocation;
        tween.editorEndLocation = endLocation;
        tween.endLocationKey = endLocationKey;
        tween.startLocationKey = startLocationKey;
        tween.shouldRotate = shouldRotate;

        // set the behaviour's data
        tween.curve = curve;

        return playable;
    }

    // Defines which properties are changed by this playable. Those properties will be reverted in editmode
    // when Timeline's preview is turned off.
    public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        const string kLocalPosition = "m_LocalPosition";
        const string kLocalRotation = "m_LocalRotation";

        driver.AddFromName<Transform>(kLocalPosition + ".x");
        driver.AddFromName<Transform>(kLocalPosition + ".y");
        driver.AddFromName<Transform>(kLocalPosition + ".z");
        driver.AddFromName<Transform>(kLocalRotation + ".x");
        driver.AddFromName<Transform>(kLocalRotation + ".y");
        driver.AddFromName<Transform>(kLocalRotation + ".z");
        driver.AddFromName<Transform>(kLocalRotation + ".w");
    }

    public ExposedReference<FXExperience> GetExposedReference()
    {
        return experienceReference;
    }
}