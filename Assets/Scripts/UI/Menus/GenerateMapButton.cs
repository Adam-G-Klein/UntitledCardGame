using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GenerateMapButton : MonoBehaviour
{

    private TMP_InputField inputField; 
    private XmlMapManager mapManager;
    private TextMeshProUGUI textOutputField;
    private RoomManager roomManager;
    private string lastInputFieldVal = "";
    private string currentInputFieldVal = "";

    // Start is called before the first frame update
    void Start()
    {
        inputField = GameObject.Find("MapNameInput").GetComponent<TMP_InputField>();
        textOutputField = GameObject.Find("TextOutputField").GetComponent<TextMeshProUGUI>();
        mapManager = GameObject.Find("MapManager").GetComponent<XmlMapManager>();
        roomManager = GameObject.FindGameObjectWithTag("Managers").GetComponent<RoomManager>();


    }

    void Update(){
        currentInputFieldVal = inputField.text;
        if(!currentInputFieldVal.Equals(lastInputFieldVal)){
            textOutputField.text = "";
        }
        lastInputFieldVal = inputField.text;
    }

    public void clicked(){
        Room startRoom;
        try {
            startRoom = mapManager.generateMap(inputField.text);
            roomManager.loadScene(startRoom);
        } catch (Exception e) {
            textOutputField.text = e.Message;
            Debug.LogError(e.Message + "\n" + e.StackTrace);
        }
    }

}
