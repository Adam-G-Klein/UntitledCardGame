using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class CanvasScaleAnimator : MonoBehaviour
{

    public float scaleAmount = 20f;
    public Canvas canvasToScale;
    private float scaleTime = 0.0f;
    private float totalTime = 0.0f;
    private float initialScale;
    private float timeToCallBack = 0.0f;
    private bool isScaling;
    private Action<Canvas> cb;
    
    void Start()
    {

    }

    void Update()
    {

        if (scaleTime >= timeToCallBack && cb != null)
        {
            print("done: " + scaleTime);
            cb(canvasToScale);
            cb = null;
        }
        if (scaleTime <= totalTime)
        {
            print(scaleAmount);
            print(scaleTime);
            canvasToScale.scaleFactor = initialScale + (scaleAmount * (totalTime - scaleTime) / totalTime);
            print(canvasToScale.scaleFactor);
            scaleTime += Time.deltaTime;

        }
        else if (isScaling)
        {
            isScaling = false;
            scaleTime = 0.0f;
            totalTime = 0.0f;
            canvasToScale.scaleFactor = initialScale;
        }
    }

    public void scaleForTime(float time, float timeToCallBack, Action<Canvas> cb)
    {
        print(initialScale);
        initialScale = canvasToScale.scaleFactor;
        totalTime = time;
        isScaling = true;
        this.timeToCallBack = timeToCallBack;
        this.cb = cb;
        canvasToScale.scaleFactor = scaleAmount;
    }
}