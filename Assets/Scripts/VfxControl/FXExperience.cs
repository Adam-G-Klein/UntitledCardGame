using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

[RequireComponent(typeof(PlayableDirector))]
public class FXExperience : MonoBehaviour
{
    public PlayableDirector playableDirector;
    public List<Location> locations = new List<Location>();
    public List<VEMapping> veMappings = new List<VEMapping>();
    private bool earlyStopped = false;

    public delegate void VoidDelegate();

    // GO HOME THE SHOWS OVER
    public VoidDelegate onExperienceOver;
    private bool playedOnExperienceOverEarly = false;

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

    public void AddVisualElementToKey(string key, VisualElement visualElement) {
        veMappings.Add(new VEMapping(key, visualElement));
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

    public VisualElement GetVisualElementFromKey(string key) {
        foreach (VEMapping mapping in veMappings) {
            if (mapping.key == key) {
                return mapping.ve;
            }
        }
        return null;
    }

    public bool ContainsLocationForKey(string key) {
        foreach (Location location in locations) {
            if (location.key == key) {
                return true;
            }
        }
        return false;
    }

    public bool ContainsVisualElementForKey(string key) {
        foreach (VEMapping mapping in veMappings) {
            if (mapping.key == key) {
                return true;
            }
        }
        return false;
    }

    public void UpdateLocationKey(string key, Vector3 newLocation) {
        foreach (Location location in locations) {
            if (location.key == key) {
                location.location = newLocation;
                return;
            }
        }
        Debug.LogWarning($"FXExperience: Key '{key}' not found to update location.");
    }

    public void PlayOnExperienceOverEarly() {
        Debug.Log("playing on experience over early!");
        if (onExperienceOver != null && !earlyStopped) {
            onExperienceOver();
        }
        playedOnExperienceOverEarly = true;
    }

    private void OnPlayableDirectorStopped(PlayableDirector director) {
        if (onExperienceOver != null && !earlyStopped && !playedOnExperienceOverEarly) {
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

    [Serializable]
    public class VEMapping {
        public string key;
        public VisualElement ve;

        public VEMapping(string key, VisualElement ve) {
            this.key = key;
            this.ve = ve;
        }
    }
}