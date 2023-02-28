using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
        spriteRenderer.material.SetFloat("OutlineThickness", outlineWidth);
        
    }
}
