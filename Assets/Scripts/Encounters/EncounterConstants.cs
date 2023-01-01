using UnityEngine;

[CreateAssetMenu(
    fileName ="EncounterConstants",
    menuName = "Constants/Encounter Constants")]
public class EncounterConstants : ScriptableObject {
    public GameObject companionPrefab;
    public GameObject enemyPrefab;
    public GameObject playerPrefab;
}