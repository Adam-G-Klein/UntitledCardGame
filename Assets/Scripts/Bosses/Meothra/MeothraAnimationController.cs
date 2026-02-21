using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Splines;
using FMODUnity;
using UnityEngine.InputSystem;

public class MeothraAnimationController: MonoBehaviour
{
    [Header("References to the rig's IK handles")]

    [SerializeField] private Transform lhTarg;
    [SerializeField] private Transform lPole;
    [SerializeField] private Transform rhTarg;
    [SerializeField] private Transform rPole;
    [SerializeField] private Transform headTarg;
    [Header("References to the positions of the handles 'at rest'")]

    [SerializeField] private Transform lhRest;
    [SerializeField] private Transform lPoleRest;
    [SerializeField] private Transform rhRest;
    [SerializeField] private Transform rPoleRest;
    [SerializeField] private Transform headRest;

    [Header("reference to movement splines")]

    [SerializeField] private GameObject lhstrikeSpline;
    [SerializeField] private GameObject lpolestrikeSpline;
    [SerializeField] private GameObject rhstrikeSpline;
    [SerializeField] private GameObject rpolestrikeSpline;

    [SerializeField] private GameObject headstrikeSpline;

    [SerializeField] private float strikeTime;
    [SerializeField] private float multiTargetSweepTime = 1f;
    [SerializeField] private GameObject strikeVFX;
    [Header("Full Team Strike Spline Settings")]
    [SerializeField] private float fullTeamSplineScaleX = 1.5f;
    [SerializeField] private float fullTeamSplineScaleY = 0.3f;
    [SerializeField] private float fullTeamSplineRotation = 0f;
    /* // for curve debugging
    [SerializeField] private float currentStrikeTime;
    [SerializeField] private Vector3 currentStrikePosition;
    */
    [SerializeField] private Vector3 intentTargettingPointerOffset;
    [SerializeField] float tweenBackToPointingTime = 1f;
    [SerializeField] private Transform handIntentLocation;
    [SerializeField] private float strikePrepTime = 1f;
    [SerializeField] private float backToIdleTime = 1f;
    private Animator animator;

    private bool _isSweeping = false;
    private int _sweepTweenId = -1;

    private Dictionary<GameObject, Transform> splineToHandleMap = new Dictionary<GameObject, Transform> ();
    private Dictionary<Transform, Transform> handleToRestPositionMap = new Dictionary<Transform, Transform>();
    // 13 x units from left side of screen to right side
    // 45 units of z rotation

    [Header("Rotation of splines by X offset so that strike angles look better")]
    [SerializeField] private Transform primeStrikeLocation;
    [SerializeField] private float degSplineRotationPerXOffset = 45f / 13f;

    [Header("Camera Movement Controls")]
    private Cinemachine.CinemachineVirtualCamera virtualCamera;
    [SerializeField]private float maxOrthoSize = 5.40f;
    [SerializeField]private float normalOrthoSize = 5.0f;
    [SerializeField]private float minOrthoSize = 4.80f;
    

    public void Setup()
    {
        if (animator == null) animator = GetComponent<Animator>();
        // special case for left hand since we want it to go right over the target
        splineToHandleMap.Add(lpolestrikeSpline, lPole);
        splineToHandleMap.Add(rhstrikeSpline, rhTarg);
        splineToHandleMap.Add(rpolestrikeSpline, rPole);
        splineToHandleMap.Add(headstrikeSpline, headTarg);

        handleToRestPositionMap.Add(lhTarg, lhRest);
        handleToRestPositionMap.Add(lPole, lPoleRest);
        handleToRestPositionMap.Add(rhTarg, rhRest);
        handleToRestPositionMap.Add(rPole, rPoleRest);
        handleToRestPositionMap.Add(headTarg, headRest);

        // janky but works for now
        virtualCamera = ScreenShakeManager.Instance.GetComponent<Cinemachine.CinemachineVirtualCamera>();
        if(virtualCamera == null)
        {
            Debug.LogError("MeothraAnimationController: Setup: could not find CinemachineVirtualCamera on ScreenShakeManager!");
        }

    }
    private void StopSweep()
    {
        _isSweeping = false;
        if (_sweepTweenId != -1)
        {
            LeanTween.cancel(_sweepTweenId);
            _sweepTweenId = -1;
        }
    }

    // TODO: rotate strike spline based on how negative or positive the x position is
    // TODO: rotate and move root motion and right hand
    public IEnumerator StrikeAnimation(Vector3 strikePosition)
    {
        StopSweep();

        float strikePositionDelta = strikePosition.x - primeStrikeLocation.position.x;
        float splineRotationAmount = degSplineRotationPerXOffset * strikePositionDelta;
        Dictionary<GameObject, Transform> instantiateSplineToHandleMap = new Dictionary<GameObject, Transform> ();
        // move each object to the start of its spline
        foreach(KeyValuePair<GameObject, Transform> entry in splineToHandleMap)
        {
            GameObject splineObj = Instantiate(entry.Key, entry.Value.position, Quaternion.identity);
            splineObj.transform.Rotate(Vector3.forward, splineRotationAmount);
            instantiateSplineToHandleMap.Add(splineObj, entry.Value);
            StartCoroutine(MoveGameObjectToStartOfSpline(splineObj, entry.Value, strikePrepTime));
        }
        // move right hand to strike position
        GameObject leftHandSpline = Instantiate(lhstrikeSpline, strikePosition, Quaternion.identity);
        leftHandSpline.transform.Rotate(Vector3.forward, splineRotationAmount);
        StartCoroutine(ZoomCamera(maxOrthoSize, strikePrepTime));
        yield return MoveGameObjectToStartOfSpline(leftHandSpline, lhTarg.transform, strikePrepTime);

        animator.Play("Strike");
        foreach(KeyValuePair<GameObject, Transform> entry in instantiateSplineToHandleMap)
        {
            StartCoroutine(MoveGameObjectOnSpline(entry.Key, entry.Value, strikeTime));
        }
        // don't yield so we can still control the timing of the strike vfx
        StartCoroutine(MoveGameObjectOnSpline(leftHandSpline, lhTarg.transform, strikeTime));
        StartCoroutine(ZoomCamera(minOrthoSize, strikeTime));
        yield return new WaitForSeconds(strikeTime / 2);

        GameObject gameObject = GameObject.Instantiate(
                strikeVFX,
                strikePosition,
                Quaternion.identity);
        MusicController.Instance.PlaySFX("event:/SFX/BossFight/SFX_MeothraAttack");
        yield return new WaitForSeconds(strikeTime / 2);
        animator.Play("Idle");
        StartCoroutine(BackToIdlePositions());
        StartCoroutine(ZoomCamera(normalOrthoSize, backToIdleTime));
        //Destroy(leftHandSpline); // clean up clean up everybody do your share
    
    }

    private IEnumerator ZoomCamera(float toValue, float zoomTime)
    {
        // use leantween to zoom out the camera
        float tween = LeanTween.value(virtualCamera.m_Lens.OrthographicSize, toValue, zoomTime).setOnUpdate((float val) =>
        {
            virtualCamera.m_Lens.OrthographicSize = val;
        }).setEaseInOutQuint().id;
        yield return null;
    }

    public IEnumerator MoveGameObjectToStartOfSpline(GameObject spline, Transform toBeMoved, float time)
    {
        SplineContainer splineContainer = spline.GetComponentInChildren<SplineContainer>();
        Spline splineComp = splineContainer.Spline;
        Vector3 startPosition = splineComp.EvaluatePosition(0);
        Vector3 worldStartPosition = splineContainer.transform.TransformPoint(startPosition);
        int tween = LeanTween.move(toBeMoved.gameObject, worldStartPosition, time)
            .setEaseInOutQuint()
            .id;
        yield return new WaitUntil(() => !LeanTween.isTweening(tween));
    }

    public IEnumerator MoveGameObjectOnSpline(GameObject spline, Transform toBeMoved, float time) {
        SplineContainer splineContainer = spline.GetComponentInChildren<SplineContainer>();
        Spline splineComp = splineContainer.Spline;
        LeanTween.value(0f, 1f, time).setOnUpdate((float val) =>
        {
            Vector3 localPosition = splineComp.EvaluatePosition(val);
            // convert local position to world position
            Vector3 worldPosition = splineContainer.transform.TransformPoint(localPosition);
            toBeMoved.position = worldPosition;
        }).setEaseInOutQuint();
        yield return new WaitForSeconds(time);
    }

    private IEnumerator BackToIdlePositions() {
        foreach (var kvp in handleToRestPositionMap) {
            Transform handle = kvp.Key;
            Transform restPos = kvp.Value;
            LeanTween.move(handle.gameObject, restPos.position, backToIdleTime).setEaseInOutQuint();
        }
        yield return null;
    }

    public IEnumerator DisplaySingleTarget(Vector3 targetPosition)
    {
        if(animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Idle")
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


    public IEnumerator DisplayMultipleTargets(List<Vector3> targetPositions)
    {
        if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Idle")
            animator.Play("Idle");

        // Find the furthest left and right positions
        Vector3 leftmost = targetPositions[0];
        Vector3 rightmost = targetPositions[0];
        foreach (Vector3 pos in targetPositions)
        {
            if (pos.x < leftmost.x) leftmost = pos;
            if (pos.x > rightmost.x) rightmost = pos;
        }

        Vector3 leftTarget  = leftmost  + intentTargettingPointerOffset;
        Vector3 rightTarget = handIntentLocation.position;

        // Initial tween: settle right hand to handIntentLocation, left hand to leftmost target
        Vector3 startLPos = lhTarg.position;
        Vector3 startRPos = rhTarg.position;
        Quaternion startRRot = rhTarg.rotation;
        int initTween = LeanTween.value(0f, 1f, tweenBackToPointingTime).setOnUpdate((float val) =>
        {
            lhTarg.position = Vector3.Lerp(startLPos, leftTarget, val);
            rhTarg.position = Vector3.Lerp(startRPos, handIntentLocation.position, val);
            rhTarg.rotation = Quaternion.Lerp(startRRot, handIntentLocation.rotation, val);
        }).setEaseInOutQuint().id;
        yield return new WaitUntil(() => !LeanTween.isTweening(initTween));

        // Sweep left hand back and forth until StrikeAnimation cancels the loop
        _isSweeping = true;
        bool goingRight = true;
        while (_isSweeping)
        {
            Vector3 sweepTarget = goingRight ? rightTarget : leftTarget;
            _sweepTweenId = LeanTween.move(lhTarg.gameObject, sweepTarget, multiTargetSweepTime)
                .setEaseInOutSine().id;
            yield return new WaitUntil(() => !LeanTween.isTweening(_sweepTweenId) || !_isSweeping);
            goingRight = !goingRight;
        }
    }

    public IEnumerator FullTeamStrikeAnimation(List<Vector3> targets)
    {
        StopSweep();

        // Find the middle-most target by sorting on X and taking the centre index
        List<Vector3> sorted = new List<Vector3>(targets);
        sorted.Sort((a, b) => a.x.CompareTo(b.x));
        Vector3 middleTarget = sorted[sorted.Count / 2];

        float splineRotationAmount = fullTeamSplineRotation;

        Dictionary<GameObject, Transform> instantiateSplineToHandleMap = new Dictionary<GameObject, Transform>();
        foreach (KeyValuePair<GameObject, Transform> entry in splineToHandleMap)
        {
            GameObject splineObj = Instantiate(entry.Key, entry.Value.position, Quaternion.identity);
            splineObj.transform.Rotate(Vector3.forward, splineRotationAmount);
            Vector3 s = splineObj.transform.localScale;
            s.x *= fullTeamSplineScaleX;
            s.y *= fullTeamSplineScaleY;
            splineObj.transform.localScale = s;
            instantiateSplineToHandleMap.Add(splineObj, entry.Value);
            StartCoroutine(MoveGameObjectToStartOfSpline(splineObj, entry.Value, strikePrepTime));
        }

        // Left hand spline centred on the middle-most target
        GameObject leftHandSpline = Instantiate(lhstrikeSpline, middleTarget, Quaternion.identity);
        leftHandSpline.transform.Rotate(Vector3.forward, splineRotationAmount);
        Vector3 lhScale = leftHandSpline.transform.localScale;
        lhScale.x *= fullTeamSplineScaleX;
        lhScale.y *= fullTeamSplineScaleY;
        leftHandSpline.transform.localScale = lhScale;

        StartCoroutine(ZoomCamera(maxOrthoSize, strikePrepTime));
        yield return MoveGameObjectToStartOfSpline(leftHandSpline, lhTarg.transform, strikePrepTime);

        animator.Play("Strike");
        foreach (KeyValuePair<GameObject, Transform> entry in instantiateSplineToHandleMap)
        {
            StartCoroutine(MoveGameObjectOnSpline(entry.Key, entry.Value, strikeTime));
        }
        StartCoroutine(MoveGameObjectOnSpline(leftHandSpline, lhTarg.transform, strikeTime));
        StartCoroutine(ZoomCamera(minOrthoSize, strikeTime));
        yield return new WaitForSeconds(strikeTime / 2);

        foreach (Vector3 target in targets)
        {
            GameObject.Instantiate(strikeVFX, target, Quaternion.identity);
        }
        MusicController.Instance.PlaySFX("event:/SFX/BossFight/SFX_MeothraAttack");
        yield return new WaitForSeconds(strikeTime / 2);

        animator.Play("Idle");
        StartCoroutine(BackToIdlePositions());
        StartCoroutine(ZoomCamera(normalOrthoSize, backToIdleTime));
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
