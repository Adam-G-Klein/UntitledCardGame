using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.UIElements;

public class MusicController : GenericSingleton<MusicController>
{
    [SerializeField] FMODUnity.EventReference fmodMixer;
    [SerializeField][ParamRef] public string combatState;
    public FMODUnity.EventReference meothraMusic;
    private FMOD.Studio.EventInstance mixerInstance;
    private FMOD.Studio.EventInstance instance;
    public List<LocationTrack> locationTracks;
    private FMODUnity.EventReference currentReference;
    public float currentMusicVolume = 0.5f;
    public float currentSFXVolume = 0.5f;

    [System.Serializable]
    public class LocationTrack
    {
        public FMODUnity.EventReference eventReference;
        public Location location;
    }

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
        if (locationTracks == null) {
            Debug.LogWarning("No location tracks set in MusicController2");
            return;
        }
        
        if (location == Location.COMBAT)
        {
            SetCombatState("Combat");
        }

        foreach (LocationTrack locationTrack in locationTracks) {
            if (location == locationTrack.location) {
                instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                instance.release();
                instance = FMODUnity.RuntimeManager.CreateInstance(locationTrack.eventReference);
                instance.start();
                instance.setVolume(currentMusicVolume);
                currentReference = locationTrack.eventReference;
                // Debug.Log(locationTrack.eventReference);
                // Debug.Log(locationTrack.location);
            }
        }
    }

     public void SetVolume(float volume, VolumeType volumeType) {
        switch(volumeType) {
            case VolumeType.SFX:
                currentSFXVolume = volume;
            break;

            case VolumeType.MUSIC:
                currentMusicVolume = volume;
                instance.setVolume(volume);
            break;
        }
    }

    public void PlaySFX(string sfx)
    {
        // want the boss attack to feel weightier, so increase volume
        // and hard code to avoid affecting other SFX volume
        if (sfx == "event:/SFX/BossFight/SFX_MeothraAttack") { RuntimeManager.PlayOneShot(sfx, 0.75f); }
        else { RuntimeManager.PlayOneShot(sfx, currentSFXVolume); }
        
    }

    public void PlayStartSFX()
    {
        RuntimeManager.PlayOneShot("event:/SFX/SFX_StartRun", currentSFXVolume);
    }

    public void RegisterButtonClickSFX(UIDocument uiDoc) {
        uiDoc.rootVisualElement.Query<VisualElement>(className: "selected-sfx").ToList()
            .ForEach(x => {
                if (x is Toggle toggle)
                    toggle.RegisterValueChangedCallback((evt) => PlaySFX("event:/SFX/SFX_ButtonClick"));
                else
                    x.RegisterOnSelected(() => PlaySFX("event:/SFX/SFX_ButtonClick"));
            });
    }

    // Start is called before the first frame update
    public void PlayMainMenuMusic()
    {
        // mixerInstance = FMODUnity.RuntimeManager.CreateInstance(fmodMixer);
        // mixerInstance.start();
        // mixerInstance.setParameterByName("Master_Volume", 0.1f);
        // wait for the settings to be loaded
        PlayMusicLocation(Location.MAIN_MENU);
    }

    public void SetCombatState(string combatstate)
    {
        switch (combatstate)
        {
            case "Combat":
                RuntimeManager.StudioSystem.setParameterByName(combatState, 0);
                break;

            case "Victory":
                RuntimeManager.StudioSystem.setParameterByName(combatState, 1);
                PlaySFX("event:/MX/MX_CombatStingers");
                break;

            case "Defeat":
                RuntimeManager.StudioSystem.setParameterByName(combatState, 2);
                PlaySFX("event:/MX/MX_CombatStingers");
                break;

            default:
                break;
        }
    }

    public void PlayBossMusic() 
    {
        instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        instance.release();
        // instance = FMODUnity.RuntimeManager.CreateInstance(meothraMusic);
        instance = FMODUnity.RuntimeManager.CreateInstance("{12fa3fda-9f96-4f70-a7a1-24e2273bc2bb}");
        instance.start();
        instance.setVolume(currentMusicVolume);
        // currentReference = meothraMusic;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //TO DO: add GC functionality!
}

public enum VolumeType {
    MUSIC,
    SFX
}