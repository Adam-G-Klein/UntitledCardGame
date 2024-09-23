using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[CustomEditor(typeof(FXExperience))]
public class FXExperienceEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        FXExperience experience = (FXExperience) target;
        PlayableDirector director = experience.playableDirector;
        
        EditorGUILayout.Space(10);
        if (GUILayout.Button("Set FXExperience on all clips")) {
            director.Stop();
            TimelineAsset timeline = (TimelineAsset)director.playableAsset;

            foreach (var track in timeline.GetOutputTracks()) {
                foreach (var clip in track.GetClips()) {
                    if (clip.asset is IFXExperienceClip fxClip) {
                        director.SetReferenceValue(fxClip.GetExposedReference().exposedName, experience);
                        EditorUtility.SetDirty(director);
                    }
                }
            }
            AssetDatabase.SaveAssets();
        }
    }
}