using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseTimescale : MonoBehaviour
{
    public float timeScale = 1f;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = timeScale;
    }
}
