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
    /* // for curve debugging
    [SerializeField] private float currentStrikeTime;
    [SerializeField] private Vector3 currentStrikePosition;
    */
    [SerializeField] private Vector3 intentTargettingPointerOffset;
    [SerializeField] float tweenBackToPointingTime = 1f;
    [SerializeField] private Transform handIntentLocation;
   private Animator animator;

    public void Setup()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }
    // TODO: rotate strike spline based on how negative or positive the x position is
    public IEnumerator StrikeAnimation(Vector3 strikePosition)
    {
        GameObject splineParent = Instantiate(strikeSpline, strikePosition, Quaternion.identity);
        SplineContainer splineContainer = splineParent.GetComponentInChildren<SplineContainer>();
        Spline spline = splineContainer.Spline;
        animator.Play("Strike");
        LeanTween.value(0f, 1f, strikeTime).setOnUpdate((float val) =>
        {
            Vector3 localPosition = spline.EvaluatePosition(val);
            // convert local position to world position
            Vector3 worldPosition = splineContainer.transform.TransformPoint(localPosition);
            lhTarg.position = worldPosition; 
            /* // for curve debugging
            currentStrikeTime = val;
            currentStrikePosition = worldPosition;
            */
            
        }).setEaseInOutQuint();
        yield return new WaitForSeconds(strikeTime / 2);    

        GameObject gameObject = GameObject.Instantiate(
                strikeVFX,
                strikePosition,
                Quaternion.identity);
        yield return new WaitForSeconds(strikeTime / 2);
        Destroy(splineParent); // clean up clean up everybody do your share
    }

    public IEnumerator DisplayNextTarget(Vector3 targetPosition)
    {
        animator.Play("Idle");
        Vector3 startLPos = lhTarg.position;
        Vector3 startRPos = rhTarg.position;
        Quaternion startRRot = rhTarg.rotation;
        Vector3 endLPos = targetPosition + intentTargettingPointerOffset;
        Vector3 endRPos = handIntentLocation.position;
        int tween = LeanTween.value(0f, 1f, tweenBackToPointingTime).setOnUpdate((float val) =>
        {
            lhTarg.position = Vector3.Lerp(startLPos, endLPos, val);
            rhTarg.position = Vector3.Lerp(startRPos, endRPos, val);
            rhTarg.rotation = Quaternion.Lerp(startRRot, handIntentLocation.rotation, val);
        }).setEaseInOutQuint().id;
        yield return new WaitForSeconds(tweenBackToPointingTime);

    }
}
