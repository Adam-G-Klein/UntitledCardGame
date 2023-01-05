
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{

    public bool followMouse = true;
    void Awake()
    {
        followMouse = true; 
    }
    void Update(){
        if(followMouse)
            followMousePosition();
    }
     
    private void followMousePosition(){
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10;
        transform.position = Camera.main.ScreenToWorldPoint(mousePos);
    }


}