using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Good idle vals: wave freq 65, speed -0.05
public class OutlineControl : MonoBehaviour
{
    public float outlineWidth = 0.1f;
    private SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("OutlineControl: " + outlineWidth);
        spriteRenderer.material.SetFloat("_OutlineThickness", outlineWidth);
        
    }
}
