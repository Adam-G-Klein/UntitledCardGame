using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// Good idle vals: wave freq 65, speed -0.05
public class TargettableEntityOutlineControl : MonoBehaviour
{
    public float idleOutlineWidth = 10f;
    public float idleWaveFrequency = 65f;
    public float idleSpeed = -0.05f;
    public Color testColor;
    private Image entityImage;
    // Start is called before the first frame update
    void Start()
    {
        entityImage = GetComponent<Image>();
        entityImage.material = new Material(entityImage.material);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("OutlineControl: " + idleOutlineWidth);
        entityImage.material.SetFloat("_OutlineThickness", idleOutlineWidth);
        entityImage.material.SetFloat("_WaveFrequency", idleWaveFrequency);
        entityImage.material.SetFloat("_WaveSpeed", idleSpeed);
        entityImage.material.SetColor("_testColor", testColor); 
        //entityImage.material.SetFloat("_OutlineThickness", idleOutlineWidth);
        
    }
}
