using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasShaker : MonoBehaviour
{

    public float shakeAmount = 3f;
    private Canvas canvasToShake;
    private float shakeTime = 0.0f;
    private Quaternion initialPosition;
    private bool isScreenShaking = false;
    void Start()
    {

    }

    void Update()
    {
        if (shakeTime > 0.0f)
        {
            canvasToShake.transform.rotation = initialPosition;
            canvasToShake.transform.Rotate(new Vector3(0, 0, 1), shakeAmount * Random.insideUnitSphere.z);
            shakeTime -= Time.deltaTime;
        }
        else if (isScreenShaking)
        {
            isScreenShaking = false;
            shakeTime = 0.0f;
            canvasToShake.transform.rotation = initialPosition;

        }
    }

    public void ScreenShakeForTime(float time, Canvas canvas)
    {
        canvasToShake = canvas;
        initialPosition = canvas.transform.rotation;
        shakeTime = time;
        isScreenShaking = true;
    }
}