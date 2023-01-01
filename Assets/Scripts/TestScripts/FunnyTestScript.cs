using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunnyTestScript : MonoBehaviour
{
    public RoomReference roomReference;
    public Room room = new Room();
    public EncounterReference encounterReference;
    public RoomVariableSO roomVariable;
    public List<Room> rooms;
    public List<RoomReference> roomReferences;
    // Start is called before the first frame update
    void Start()
    {
        RoomReference roomReference1 = new RoomReference();
        RoomReference roomReference2 = new RoomReference();
        Debug.Log(roomReference1 == roomReference2);
        roomReferences.Add(roomReference1);
        roomReferences.Add(roomReference2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
