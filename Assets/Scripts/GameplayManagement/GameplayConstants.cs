using UnityEngine;

[CreateAssetMenu(
    fileName ="GameplayConstants",
    menuName = "Constants/Gameplay Constants")]
public class GameplayConstants : ScriptableObject {
    public bool DEVELOPMENT_MODE = true;
    public int START_TURN_DRAW_PER_COMPANION = 1;
    public int START_TURN_MANA = 1;
}