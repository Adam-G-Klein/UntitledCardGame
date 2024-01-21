using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class DoorInScene : MonoBehaviour
{

    void OnCollisionEnter2D(Collision2D col) {
        SceneManager.LoadScene("SoftLocked");
    }
}
