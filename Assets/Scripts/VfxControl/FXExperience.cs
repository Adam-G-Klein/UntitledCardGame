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
    private bool earlyStopped = false;

    public delegate void VoidDelegate();

    // GO HOME THE SHOWS OVER
    public VoidDelegate onExperienceOver;

    [ContextMenu("Start Experience")]
    public void StartExperience(VoidDelegate onExperienceOver = null) {
        if (playableDirector == null) {
            playableDirector = GetComponent<PlayableDirector>();
        }
        this.onExperienceOver += onExperienceOver;
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
                switch (true) {
                    case true when typeof(TweenTrack).IsAssignableFrom(track.GetType()):
                    case true when typeof(PartialTweenTrack).IsAssignableFrom(track.GetType()):
                        playableDirector.SetGenericBinding(track, bindingsMap[track.name].transform);
                    break;

                    case true when typeof(AnimationTrack).IsAssignableFrom(track.GetType()):
                        playableDirector.SetGenericBinding(track, bindingsMap[track.name].GetComponent<Animator>());
                    break;
                }
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
        if (onExperienceOver != null && !earlyStopped) {
            onExperienceOver();
        }
        playableDirector.stopped -= OnPlayableDirectorStopped;
        onExperienceOver = null;
        StartCoroutine(DestroyAfterFrame());
    }

    private IEnumerator DestroyAfterFrame() {
        yield return null;
        Destroy(gameObject);
    }

    public void EarlyStop() {
        earlyStopped = true;
        if (playableDirector != null) {
            playableDirector.stopped -= OnPlayableDirectorStopped;
        }
        onExperienceOver = null;
        StartCoroutine(DestroyAfterFrame());
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