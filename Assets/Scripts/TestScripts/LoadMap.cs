using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMap : MonoBehaviour
{
    public MapVariableSO mapReference;
    public RoomVariableSO roomReference;

    public void loadMap() {
        if (mapReference.GetValue() == null) {
            Debug.LogError("Map Reference Value is null!");
            return;
        }
        Map map = mapReference.GetValue();
        if (map.rooms.Count == 0) {
            Debug.LogError("Map has no rooms!");
            return;
        }
        roomReference.SetValue(map.rooms[0]);
        SceneManager.LoadScene("PlaceholderRoom");
    }
}
