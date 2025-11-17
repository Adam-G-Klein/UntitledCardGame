using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Splines;
using FMODUnity;

public class MeothraAnimationController: MonoBehaviour
{

    [SerializeField] private Transform lhTarg;
    [SerializeField] private Transform lPole;
    [SerializeField] private Transform rhTarg;
    [SerializeField] private Transform rPole;

    [SerializeField] private float strikeTime;
    [SerializeField] private GameObject lhstrikeSpline;
    [SerializeField] private GameObject lpolestrikeSpline;
    [SerializeField] private GameObject rhstrikeSpline;
    [SerializeField] private GameObject rpolestrikeSpline;

    [SerializeField] private GameObject headstrikeSpline;

    [SerializeField] private GameObject strikeVFX;
    /* // for curve debugging
    [SerializeField] private float currentStrikeTime;
    [SerializeField] private Vector3 currentStrikePosition;
    */
    [SerializeField] private Vector3 intentTargettingPointerOffset;
    [SerializeField] float tweenBackToPointingTime = 1f;
    [SerializeField] private Transform handIntentLocation;
    [SerializeField] private float strikePrepTime = 1f;
   private Animator animator;



    public void Setup()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }
    // TODO: rotate strike spline based on how negative or positive the x position is
    // TODO: rotate and move root motion and right hand
    public IEnumerator StrikeAnimation(Vector3 strikePosition)
    {
        
        GameObject leftHand = Instantiate(lhstrikeSpline, strikePosition, Quaternion.identity);
        SplineContainer splineContainer = leftHand.GetComponentInChildren<SplineContainer>();
        Spline spline = splineContainer.Spline;

        // move left hand to start of strike position
        int ltid = LeanTween.move(lhTarg.gameObject, 
            splineContainer.transform.TransformPoint(spline.EvaluatePosition(0)), 
            strikePrepTime)
            .setEaseInOutQuint()
            .id;
        // move right hand to strike position
        yield return new WaitUntil(() => !LeanTween.isTweening(ltid));

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
        Destroy(leftHand); // clean up clean up everybody do your share
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

    public void PlayHurtAnimation()
    {
        animator.Play("Hurt");
    }

    public void PlayIdleAnimation()
    {
        animator.Play("Idle");
    }
}
