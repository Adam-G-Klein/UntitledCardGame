using UnityEngine;

[CreateAssetMenu(
    fileName ="RoomConstants",
    menuName = "Constants/Room Constants")]
public class RoomConstants : ScriptableObject {
    public GameObject doorPrefab;
    public GameObject shopPrefab;
    public GameObject encounterPrefab;

    public Vector3 SHOPKEEP_POSITION = new Vector3(0.47f, 2.14f, 1f);
    public Vector3 FIRST_DOOR_LOCATION = new Vector3(-9.39f,-2.38f,-9.96f);
    public Vector3 SECOND_DOOR_LOCATION = new Vector3(8.92f,-2.61f,-9.96f);
    public Vector3 ENCOUNTER_LOCATION = new Vector3(4.26f,-2.28f,-0.08f);
}