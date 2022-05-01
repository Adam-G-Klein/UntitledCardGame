using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    private static RoomManager instance;
    private RoomBuilder roomBuilder;

    private Room activeRoom = null;

    void Awake()
    {
        // If the instance reference has not been set yet, 
        if (instance == null)
        {
            // Set this instance as the instance reference.
            instance = this;
            roomBuilder = new RoomBuilder();
        }
        else if(instance != this)
        {
            // If the instance reference has already been set, and this is not the
            // the instance reference, destroy this game object.
            Destroy(gameObject);
        }

        // Do not destroy this object when we load a new scene
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start() {}

    // Update is called once per frame
    void Update() {}

    public void loadRoom(Room room) {
        activeRoom = room;
        SceneManager.LoadScene(room.getRoomSceneString());
        SceneManager.sceneLoaded += OnRoomLoad;
    }

    private void OnRoomLoad(Scene scene, LoadSceneMode mode)
    {
        roomBuilder.buildRoom(activeRoom);
    }
}
