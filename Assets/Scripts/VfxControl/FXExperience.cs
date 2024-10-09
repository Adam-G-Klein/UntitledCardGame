using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[RequireComponent(typeof(PlayableDirector))]
public class FXExperience : MonoBehaviour
{
    public PlayableDirector playableDirector;
    public List<Location> locations = new List<Location>();

    public delegate void VoidDelegate();

    // GO HOME THE SHOWS OVER
    public VoidDelegate onExperienceOver;

    [ContextMenu("Start Experience")]
    public void StartExperience() {
        if (playableDirector == null) {
            playableDirector = GetComponent<PlayableDirector>();
        }
        playableDirector.stopped += OnPlayableDirectorStopped;
        playableDirector.Play();
    }

    public void AddLocationToKey(string key, Vector3 location) {
        locations.Add(new Location(key, location));
    }

    public void BindGameObjectsToTracks(Dictionary<String, GameObject> bindingsMap) {
        if (playableDirector == null) {
            playableDirector = GetComponent<PlayableDirector>();
        }

        TimelineAsset timeline = (TimelineAsset) playableDirector.playableAsset;
        foreach (var track in timeline.GetOutputTracks()) {
            if (bindingsMap.ContainsKey(track.name)) {
                playableDirector.SetGenericBinding(track, bindingsMap[track.name].transform);
            }
        }
    }

    public Vector3 GetLocationFromKey(string key) {
        foreach (Location location in locations) {
            if (location.key == key) {
                return location.location;
            }
        }
        return Vector3.zero;
    }

    public bool ContainsLocationForKey(string key) {
        foreach (Location location in locations) {
            if (location.key == key) {
                return true;
            }
        }
        return false;
    }

    private void OnPlayableDirectorStopped(PlayableDirector director) {
        if (onExperienceOver != null) {
            onExperienceOver();
        }
        Destroy(this.gameObject);
    }

    [Serializable]
    public class Location {
        public string key;
        public Vector3 location;

        public Location(string key, Vector3 location) {
            this.key = key;
            this.location = location;
        }
    }
}
