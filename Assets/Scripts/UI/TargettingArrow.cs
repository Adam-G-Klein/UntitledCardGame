using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TargettingArrow : MonoBehaviour
{
    public Transform Root;
    public Transform BendToPoint;
    public Transform FollowMouse;
    public LineRenderer linerenderer;

    private FollowMouse followMouse;
    public float vertexCount = 12;
    public float Point2Ypositio = 2;
    public bool frozen = false;

    private Transform target;
    
    void Awake()
    {
        linerenderer = GetComponent<LineRenderer>();
        followMouse = GetComponentInChildren<FollowMouse>();
        frozen = false;
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
        if(frozen && target != null && FollowMouse != null) {
            // Not following the mouse anymore, but still want to
            // follow the target that's been set if it moves
            FollowMouse.position = target.position;
        } 

    }
    
    private void DrawCurve(){
        BendToPoint.transform.position = new Vector3(
            (Root.transform.position.x + FollowMouse.transform.position.x)/2, Point2Ypositio, 
            (Root.transform.position.z + FollowMouse.transform.position.z) / 2);
        var pointList = new List<Vector3>();

        for(float ratio = 0;ratio<=1;ratio+= 1/vertexCount)
        {
            var tangent1 = Vector3.Lerp(Root.position, BendToPoint.position, ratio);
            var tangent2 = Vector3.Lerp(BendToPoint.position, FollowMouse.position, ratio);
            var curve = Vector3.Lerp(tangent1, tangent2, ratio);

            pointList.Add(curve);
        }

        linerenderer.positionCount = pointList.Count;
        linerenderer.SetPositions(pointList.ToArray());
    }

    public void setColor(Color color){
        linerenderer.endColor = color;
    }

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

}