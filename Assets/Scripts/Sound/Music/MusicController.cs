
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : GenericSingleton<MusicController>
{
    private AudioSource audioSource;
    [Header("Workaround for a serialized map")]
    [SerializeField]
    private List<Location> locationOrder = new List<Location>();
    [SerializeField]
    private List<AudioClip> musicClipsOrderedByLocation = new List<AudioClip>();
    [SerializeField]
    private List<float> clipStartTimes = new List<float>();


    void Awake()
    {
        // A rare use case for this, we want our music to give nary a FLINCH at a scene change
        // GenericSingleton handles deduping across scenes
        DontDestroyOnLoad(this.gameObject);
        int i = 0;
        while(i <= (int)Location.BOSSFIGHT) {
            Location location = (Location) i;
            locationOrder.Add(location);
            i += 1;
        }
    }

    void Start() {
        audioSource = GetComponent<AudioSource>();
        if(musicClipsOrderedByLocation.Count > 0) {
            PlayMusicForLocation(locationOrder[1]);
        } else {
            Debug.LogError("No music clips found");
        }
    }

    public void PlayMusicForLocation(Location location)
    {
        if(Instance == null || audioSource == null) {
            Debug.LogWarning("No music controller found");
            return;
        }
        if(audioSource.clip == musicClipsOrderedByLocation[(int)location]) {
            return;
        }
        audioSource.clip = musicClipsOrderedByLocation[(int)location];
        audioSource.time = clipStartTimes[(int)location];
        audioSource.Play();

    }

}