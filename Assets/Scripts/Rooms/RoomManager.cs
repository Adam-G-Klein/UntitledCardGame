using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : Manager 
{
    private Room activeRoom = null;

    // Start is called before the first frame update
    void Start() {}

    // Update is called once per frame
    void Update() {}

    public void loadRoom(Room room) {
        LoadRoomArgs args = new LoadRoomArgs(room, room.buildRoom);
        
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
        args.callback();
    }
}

class LoadRoomArgs 
{
    public Room room;
    public Action callback;

    public LoadRoomArgs(Room room, Action callback)
    {
        this.room = room;
        this.callback = callback;
    }
}
