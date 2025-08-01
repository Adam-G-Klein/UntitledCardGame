using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(
    fileName = "New GameplayConstants",
    menuName = "Constants/Gameplay Constants")]
public class GameplayConstantsSO : ScriptableObject
{
    public bool DEVELOPMENT_MODE = true;
    public int START_TURN_MANA = 0;
    public int COMPANIONS_FOR_LEVELTWO_COMBINATION = 3;
    public int COMPANIONS_FOR_LEVELTHREE_COMBINATION = 2;

    // not being enforced right now, just for spawn locations
    public int MAX_MINIONS_PER_COMPANION = 4;

    public int MAX_HAND_SIZE = 10;
    public VisualTreeAsset cardTemplate;
}