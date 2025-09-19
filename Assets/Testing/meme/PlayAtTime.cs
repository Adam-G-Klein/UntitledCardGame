using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAtTime : MonoBehaviour
{
    public float startTime = 50.0f;
    // Start is called before the first frame update
    void Start()
    {
        AudioSource src = GetComponent<AudioSource>();

        src.time = startTime;
        src.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
