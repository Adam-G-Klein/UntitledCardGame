using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TargettingArrow : MonoBehaviour
{
    public Transform Root;
    public Transform FollowMouse;

    public LineRenderer linerenderer;

    private FollowMouse followMouse;
    public float vertexCount = 12;
    public float Zpos = 2;
    public float bendY = 2;
    public bool frozen = false;

    private Transform target = null;
    private Vector2 targetVector2 = Vector2.zero;
    private Vector3 BendToPoint1;
    private Vector3 BendToPoint2;
    public float selfTargetBulge = 1;
    
    void Awake()
    {
        linerenderer = GetComponent<LineRenderer>();
        followMouse = GetComponentInChildren<FollowMouse>();
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        DrawCurve();
        // null checks for the Update frame where this arrow is being Destroyed
        if(frozen && FollowMouse != null) {
            // Not following the mouse anymore, but still want to
            // follow the target that's been set if it moves
            if (target != null) FollowMouse.position = target.position;
            else FollowMouse.position = targetVector2;
        }  
    }
    
    private void DrawCurve(){
        // a point 1/3 of the way from Root to FollowMouse
        BendToPoint1 = Vector3.Lerp(Root.transform.position, FollowMouse.transform.position, 0.333f);
        BendToPoint2 = Vector3.Lerp(Root.transform.position, FollowMouse.transform.position, 0.666f);
        // Add a bulge if self-targeting
        BendToPoint1 = new Vector3(BendToPoint1.x + (Root.transform.position == FollowMouse.transform.position ? 1 : 0), BendToPoint1.y + bendY, Zpos);
        BendToPoint2 = new Vector3(BendToPoint2.x, BendToPoint2.y + bendY, Zpos);
        // a point 2/3 of the way from Root to FollowMouse
        var pointList = new List<Vector3>();
        Vector3 currentBendPoint = BendToPoint1;
        List<Vector3> lerpPoints = new List<Vector3>(){
                    Root.position,
                    BendToPoint1,
                    BendToPoint2,
                    FollowMouse.position
                };;

        for(float ratio = 0;ratio<=1;ratio+= 1/vertexCount)
        {
            var tangent1 = Vector3.Lerp(lerpPoints[0], lerpPoints[1], ratio);
            var tangent2 = Vector3.Lerp(lerpPoints[1], lerpPoints[2], ratio);
            var tangent3 = Vector3.Lerp(lerpPoints[2], lerpPoints[3], ratio);

            var lerp1 = Vector3.Lerp(tangent1, tangent2, ratio);
            var lerp2 = Vector3.Lerp(tangent2, tangent3, ratio);
            var curve = Vector3.Lerp(lerp1, lerp2, ratio);
            pointList.Add(curve);
        }

        linerenderer.positionCount = pointList.Count;
        linerenderer.SetPositions(pointList.ToArray());
        linerenderer.enabled = true;
    }

    public void setColor(Color color){
        linerenderer.endColor = color;
    }

    // All that's necessary 
    public void setAllChildrenPosition(Vector3 position){
        foreach(Transform child in transform){
            child.position = position;
        }
    }

    public void freeze(Transform target){
        frozen = true;
        followMouse.followMouse = false;
        this.target = target;
    }

    public void freeze(Vector2 position){
        frozen = true;
        followMouse.followMouse = false;
        this.targetVector2 = position;
    }
}