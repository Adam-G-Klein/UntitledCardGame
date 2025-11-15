using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Splines;

public class MeothraAnimationController: MonoBehaviour
{

    [SerializeField] private Transform lhTarg;
    [SerializeField] private Transform lPole;
    [SerializeField] private Transform rhTarg;
    [SerializeField] private Transform rPole;

    [SerializeField] private float strikeTime;
    [SerializeField] private GameObject strikeSpline;
    [SerializeField] private GameObject strikeVFX;
    [SerializeField] private float currentStrikeTime;
    [SerializeField] private Vector3 currentStrikePosition;

    // TODO: rotate strike spline based on how negative or positive the x position is
    public IEnumerator StrikeAnimation(Vector3 strikePosition)
    {
        GameObject splineParent = Instantiate(strikeSpline, strikePosition, Quaternion.identity);
        SplineContainer splineContainer = splineParent.GetComponentInChildren<SplineContainer>();
        Spline spline = splineContainer.Spline;
        LeanTween.value(0f, 1f, strikeTime).setOnUpdate((float val) =>
        {
            Vector3 localPosition = spline.EvaluatePosition(val);
            // convert local position to world position
            Vector3 worldPosition = splineContainer.transform.TransformPoint(localPosition);
            lhTarg.position = worldPosition; 
            currentStrikeTime = val;
            currentStrikePosition = worldPosition;
            
        });
        yield return new WaitForSeconds(strikeTime / 2);    

        GameObject gameObject = GameObject.Instantiate(
                strikeVFX,
                strikePosition,
                Quaternion.identity);
        yield return new WaitForSeconds(strikeTime / 2);
    }
}
