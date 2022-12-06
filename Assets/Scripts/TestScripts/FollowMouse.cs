
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{

    void Update(){
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10;
        transform.position = Camera.main.ScreenToWorldPoint(mousePos);
    }


}