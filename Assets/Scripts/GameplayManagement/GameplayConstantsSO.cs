using UnityEngine;

[CreateAssetMenu(
    fileName ="New GameplayConstants",
    menuName = "Constants/Gameplay Constants")]
public class GameplayConstantsSO : ScriptableObject {
    public bool DEVELOPMENT_MODE = true;
    public int START_TURN_MANA = 0;
    // not being enforced right now, just for spawn locations
    public int MAX_MINIONS_PER_COMPANION = 4;
}