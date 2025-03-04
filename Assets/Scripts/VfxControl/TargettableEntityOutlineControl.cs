using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum OutlineState {
    Idle,
    Targettable,
    Hovered,
    Selected
}

[System.Serializable]
public class OutlineVals {
    public float outlineWidth;
    public float waveFrequency;
    public float speed;
    public Color color;

    public OutlineVals(float outlineWidth, float waveFrequency, float speed, Color color) {
        this.outlineWidth = outlineWidth;
        this.waveFrequency = waveFrequency;
        this.speed = speed;
        this.color = color;
    }
}
// Good idle vals: wave freq 65, speed -0.05
public class TargettableEntityOutlineControl : MonoBehaviour
{
    public OutlineState outlineState;
    public OutlineVals currentOutlineVals;
    public Color testColor;
    private Material material;

    private static Dictionary<OutlineState, OutlineVals> outlineValsMap = new Dictionary<OutlineState, OutlineVals>() {
        // can use the global colors scriptable object for more granularity here
        {OutlineState.Idle, new OutlineVals(10f, 65f, -0.05f, Color.gray)},
        {OutlineState.Targettable, new OutlineVals(20f, 30f, 0.1f, Color.yellow)},
        {OutlineState.Hovered, new OutlineVals(30f, 65f, 0.2f, Color.black)},
        {OutlineState.Selected, new OutlineVals(20f, 65f, -0.05f, Color.white)}
    };


    // Start is called before the first frame update
    void Start()
    {
        Image entityImage = GetComponent<Image>();
        SpriteRenderer entitySpriteRenderer = GetComponent<SpriteRenderer>();
        if (entityImage) {
            material = new Material(entityImage.material);
            entityImage.material = material;
        } else if (entitySpriteRenderer) {
            material = new Material(entitySpriteRenderer.material);
            entitySpriteRenderer.material = material;
        } else {
            Debug.LogError("TargettableEntityOutlineControl: No image or sprite renderer found on entity");
        }
        // makes a copy of the material so that the original material is not modified
        setOutlineState(OutlineState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        //entityImage.material.SetFloat("_OutlineThickness", idleOutlineWidth);
    }


    public void setOutlineState(OutlineState newOutlineState) {
        this.outlineState = newOutlineState;
        currentOutlineVals = outlineValsMap[newOutlineState];
        material.SetFloat("_OutlineThickness", currentOutlineVals.outlineWidth);
        material.SetFloat("_WaveFrequency", currentOutlineVals.waveFrequency);
        material.SetFloat("_WaveSpeed", currentOutlineVals.speed);
        material.SetColor("_OutlineColor", currentOutlineVals.color);
    }
}
