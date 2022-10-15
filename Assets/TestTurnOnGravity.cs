using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTurnOnGravity : MonoBehaviour
{
    public IntGameEvent gameEvent;

    void OnCollisionEnter2D(Collision2D col)
    {
        gameEvent.Raise(10);
    }
}
