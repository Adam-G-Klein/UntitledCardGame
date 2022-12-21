using UnityEngine;

[CreateAssetMenu(
    fileName ="Card",
    menuName = "Constants/GameplayConstants")]
public class GameplayConstants : ScriptableObject {
    public bool DEVELOPMENT_MODE = true;
    public int START_TURN_DRAW_PER_COMPANION = 1;
    public int START_TURN_MANA = 1;
}