using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController2 : GenericSingleton<MusicController2>
{
    
    private FMOD.Studio.EventInstance instance;
    public List<LocationTrack> locationTracks;
    private FMODUnity.EventReference currentReference;
    //public float currentVolume = 0.5f;

    [System.Serializable]
    public class LocationTrack
    {
        public FMODUnity.EventReference eventReference;
        public Location location;
    } 
    //public FMODUnity.EventReference exampleReference;

    void Awake()
    {
        // A rare use case for this, we want our music to give nary a FLINCH at a scene change
        // GenericSingleton handles deduping across scenes
        DontDestroyOnLoad(this.gameObject);
    }

    public void PrepareForGoingBackToMainMenu() {
        instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        instance.release();
        Destroy(this.gameObject);
    }
    
    public void PlayMusicLocation(Location location)
    {
        if(locationTracks == null)
        {
            Debug.LogWarning("No location tracks set in MusicController2");
            return;
        }
        foreach (LocationTrack locationTrack in locationTracks)
        {
            
            if (location == locationTrack.location) {
                instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                instance.release();
                instance = FMODUnity.RuntimeManager.CreateInstance(locationTrack.eventReference);
                //instance.setVolume(currentVolume);
                instance.start();
                currentReference = locationTrack.eventReference;
                Debug.Log(locationTrack.eventReference);
                Debug.Log(locationTrack.location);
            }
        }
    }

    /*public void SetVolume(float volume)
    {
        currentVolume = volume;
        instance.setVolume(volume);
    }*/

    /*public void PlaySFX(string sfx)
    {
        FMODUnity.RuntimeManager.PlayOneShot();
    }*/

    public void PlayStartSFX()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/SFX_StartRun");
    }
    
    // Start is called before the first frame update
    void Start()
    {
        PlayMusicLocation(Location.MAIN_MENU);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //TO DO: add GC functionality!
}
