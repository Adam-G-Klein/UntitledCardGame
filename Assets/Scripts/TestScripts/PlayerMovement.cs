using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{

    public Animator animator;
    public Vector2 speed = new Vector2(50, 50);
    public bool movementEnabled = true;
    private RoomManager  roomManager;

    // Start is called before the first frame update
    void Start()
    {

        RoomManager roomManager = GameObject.FindGameObjectWithTag("Managers").GetComponent<RoomManager>();
        Debug.Log("activeRoom at playerStart: " + roomManager.getActiveRoom().getSceneString());
        
    }

    // Update is called once per frame
    void Update()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        if (movementEnabled)
        {
            animator.SetFloat("inputX", inputX);
            animator.SetFloat("inputY", inputY);

            Vector3 movement = new Vector3(speed.x * inputX, speed.y * inputY, 0);

            movement *= Time.deltaTime;

            transform.Translate(movement);
        }
    }
}
