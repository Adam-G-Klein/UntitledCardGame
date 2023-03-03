using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TargettableEntityOutlineControl : MonoBehaviour
{
    public float idleOutlineWidth = 10f;
    private Image entityImage;
    // Start is called before the first frame update
    void Start()
    {
        entityImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        //entityImage.material.SetFloat("_OutlineThickness", idleOutlineWidth);
        
    }
}
