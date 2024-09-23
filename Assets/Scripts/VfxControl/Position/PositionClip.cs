using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// Represents the serialized data for a clip on the Tween track
[Serializable]
[DisplayName("Position Clip")]
public class PositionClip : 
        PlayableAsset,
        ITimelineClipAsset,
        IPropertyPreview,
        IFXExperienceClip
{
    [Header("The key to get location from.")]
    public string locationKey;

    public ExposedReference<FXExperience> experienceReference;

    // Implementation of ITimelineClipAsset. This specifies the capabilities of this timeline clip inside the editor.
    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending; }
    }

    // Creates the playable that represents the instance of this clip.
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        // create a new TweenBehaviour
        ScriptPlayable<PositionBehaviour> playable = ScriptPlayable<PositionBehaviour>.Create(graph);
        PositionBehaviour position = playable.GetBehaviour();

        position.fXExperience = experienceReference.Resolve(graph.GetResolver());
        position.locationKey = locationKey;

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