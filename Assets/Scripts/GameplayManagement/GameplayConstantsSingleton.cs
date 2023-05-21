using UnityEngine;

public class GameplayConstantsSingleton : MonoBehaviour {
    public GameplayConstantsSO gameplayConstants;

    public static GameplayConstantsSingleton Instance { get; private set; }
    private void Awake() 
    { 
        // If there is an instance, and it's not me, delete myself.
        
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 
    }
}