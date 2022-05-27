using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayHealth: MonoBehaviour
{
    public float lineWidth = 0.2f;
    public Gradient backgroundColor;
    public Gradient healthBarColor;
    public Material lineMaterial;
    public Vector2 startPoint;
    public Vector2 endPoint;

    private LineRenderer backgroundLine;
    private LineRenderer healthBarLine;
    private GameObject background;
    private GameObject healthBar;
    private TargetableEntity entity;
    private Vector2 transformedStartPoint;
    private Vector2 transformedEndPoint;

    // Start is called before the first frame update
    void Start()
    {
        background = transform.Find("background").gameObject;
        healthBar = transform.Find("healthBar").gameObject;
        setupBackground();
        setupHealthBar();
    }

    // Update is called once per frame
    void Update()
    {
        float percent = (float)entity.getHealth() / (float)entity.getMaxHealth();
        Vector2 direction;

        transformedStartPoint = transform.TransformPoint(startPoint);
        transformedEndPoint = transform.TransformPoint(endPoint);

        backgroundLine.SetPosition(0, transformedStartPoint);
        backgroundLine.SetPosition(1, transformedEndPoint);
        healthBarLine.SetPosition(0, transformedStartPoint);
        direction = transformedEndPoint - transformedStartPoint;
        healthBarLine.SetPosition(1, transformedStartPoint + (direction * percent));
    }

    void setupBackground()
    {
        backgroundLine = background.AddComponent<LineRenderer>();
        backgroundLine.positionCount = 2;
        backgroundLine.SetPosition(0, transformedStartPoint);
        backgroundLine.SetPosition(1, transformedEndPoint);
        backgroundLine.startWidth = lineWidth;
        backgroundLine.endWidth = lineWidth;
        backgroundLine.material = lineMaterial;
        backgroundLine.colorGradient = backgroundColor;
        backgroundLine.sortingOrder = 1;

    }

    void setupHealthBar()
    {
        healthBarLine = healthBar.AddComponent<LineRenderer>();
        healthBarLine.positionCount = 2;
        healthBarLine.SetPosition(0, transformedStartPoint);
        healthBarLine.SetPosition(1, transformedEndPoint);
        healthBarLine.startWidth = lineWidth;
        healthBarLine.endWidth = lineWidth;
        healthBarLine.material = lineMaterial;
        healthBarLine.colorGradient = healthBarColor;
        healthBarLine.sortingOrder = 2;
    }

    public void setEntity(TargetableEntity entity)
    {
        this.entity = entity;
    }
}