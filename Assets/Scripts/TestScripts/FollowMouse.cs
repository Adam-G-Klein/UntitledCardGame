
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{

    public bool followMouse = true;
    public float Zpos = 10;
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
        mousePos.z = Zpos;
        transform.position = Camera.main.ScreenToWorldPoint(mousePos);
        //transform.position = Camera.main.ViewportToWorldPoint(mousePos);
    }


}