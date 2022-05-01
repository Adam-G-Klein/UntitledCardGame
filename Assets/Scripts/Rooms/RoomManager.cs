using System;
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
        LoadRoomArgs args = new LoadRoomArgs(room, roomBuilder.buildRoom);
        
        activeRoom = room;
        StartCoroutine(loadRoomCoroutine(args));
    }

    private IEnumerator loadRoomCoroutine(LoadRoomArgs args)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(args.room.getRoomSceneString());

        // Wait for the scene to actually fully load
        while(!asyncLoad.isDone)
        {
            yield return null;
        }
        args.callback(args.room);
    }
}

class LoadRoomArgs 
{
    public Room room;
    public Action<Room> callback;

    public LoadRoomArgs(Room room, Action<Room> callback)
    {
        this.room = room;
        this.callback = callback;
    }
}
