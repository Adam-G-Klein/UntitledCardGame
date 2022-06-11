using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyInputForTesting : MonoBehaviour
{
    private RoomManager roomManager;
    // Start is called before the first frame update
    void Start()
    {
        roomManager = GameObject.FindGameObjectWithTag("Managers").GetComponent<RoomManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.L)){
            print("loading active room!");
            roomManager.loadActiveRoom();
        }
    }

    public void leaveEncounter() {
        print("loading active room!");
        roomManager.loadActiveRoom();
    }
}
