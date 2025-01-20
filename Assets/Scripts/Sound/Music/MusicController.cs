
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

the plan:
- we want to play an alternating track from a set of tracks for each new location
- pre combat splash and post combat are now the only two places that music switches during
    the normal loop
- 



*/
public class MusicController : GenericSingleton<MusicController>
{
    //private AudioSource audioSource;
    [SerializeField]
    private List<string> precombatClips = new List<string>();
    [SerializeField]
    private List<string> postcombatClips = new List<string>();
    [SerializeField]
    //private AudioClip tutorialClip;

    [Header("Only clips in use below are main menu, tutorial, and bossfight")]
    //[SerializeField]
    private List<Location> locationKeys = new List<Location>();
    [SerializeField]
    private List<string> musicClipsOrderedByLocation = new List<string>();

    [SerializeField]
    private List<int> clipStartTimes = new List<int>();
    [SerializeField]
    private float introVolume = 0.15f;


    /*[SerializeField]
    private float otherVolume = 0.3f;
    [SerializeField]
    private float defaultSFXVolume = 0.5f;  
    [SerializeField]
    private float defaultSFXPitch = 1.0f;

    */private FMOD.Studio.EventInstance instance;
    //private FMODUnity.EventReference reference;



    void Awake()
    {
        // A rare use case for this, we want our music to give nary a FLINCH at a scene change
        // GenericSingleton handles deduping across scenes
        DontDestroyOnLoad(this.gameObject);
    }

    void Start() {
        //audioSource = GetComponent<AudioSource>();
        //reference = 
        //if(instance != null)
            //sfxSource = transform.GetChild(0).GetComponent<AudioSource>();  
        PlayMusicForLocation(Location.MAIN_MENU);
    }

    public void PlayMusicForLocation(Location location, bool inTutorial = false)
    {
        //if(instance == null) {
        //    Debug.LogWarning("No music in instance!");
        //    return;
        //}
        if(inTutorial && (location == Location.PRE_COMBAT_SPLASH || location == Location.COMBAT)) {
            //if(audioSource.clip != tutorialClip) {
            //    audioSource.clip = tutorialClip;
            //    audioSource.Play();
            //}
            instance = FMODUnity.RuntimeManager.CreateInstance("event:/MX/MX_Tutorial");
            instance.start();
            return;
        }
        switch(location) {
            case Location.PRE_COMBAT_SPLASH:
                instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                //if(precombatClips.Count == 0) {
                //    return;
                //}
                //audioSource.clip = precombatClips[Random.Range(0, precombatClips.Count)];
                //audioSource.Play();
                instance = FMODUnity.RuntimeManager.CreateInstance("event:/MX/MX_Combat");
                instance.start();
                break;
            case Location.POST_COMBAT:
                instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                //if(postcombatClips.Count == 0) {
                //    return;
                //}
                //audioSource.clip = postcombatClips[Random.Range(0, postcombatClips.Count)];
                //audioSource.Play();
                
                //below will eventually be a victory?
                //instance = FMODUnity.RuntimeManager.CreateInstance("event:/MX/MX_Combat");
                break;
            default:
                PlayMusicForLocationFromList(location);
                break;
        }
    }

    public void PlayMusicForLocationFromList(Location location)
    {
        if(Instance == null) {
            Debug.LogWarning("No music controller found");
            return;
        }
        int indexOfLocation = locationKeys.IndexOf(location);
        /*if(indexOfLocation == -1 || audioSource.clip == musicClipsOrderedByLocation[indexOfLocation]) {
            return;
        }
        if(location == Location.WAKE_UP_ROOM || location == Location.TEAM_SIGNING) {
            audioSource.volume = introVolume;
        } else {
            audioSource.volume = otherVolume;
        }*/
        Debug.Log("playing clip");
        //audioSource.clip = musicClipsOrderedByLocation[indexOfLocation];
        //audioSource.time = clipStartTimes[indexOfLocation];
        //audioSource.Play();
    }

    // spaghetti!
    /*public void PlaySFX(AudioClip clip, float volume = -1, float pitch = 1.0f) {
        if(sfxSource == null) {
            if(transform.childCount == 0) {
                // TODO: completely refactor the sound system. No point in improving this marginally
                // Debug.LogWarning("No sfx source found");
                return;
            }
            sfxSource = transform.GetChild(0).GetComponent<AudioSource>();
            if(sfxSource == null) {
                // Debug.LogWarning("No sfx source found");
                return;
            }
        }
        if(volume == -1) {
            //volume = defaultSFXVolume;
        }
        /*if(!Mathf.Equals(pitch, 1.0f)) {
            sfxSource.pitch = pitch;
        } else {
            sfxSource.pitch = defaultSFXPitch;
        }
        sfxSource.PlayOneShot(clip, volume);*/
}
