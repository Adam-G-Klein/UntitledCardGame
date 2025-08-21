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
    [SerializeField]
    [Header("Set by intent controller, here for manual editing/tweaking during runtime\nThe 1/3rd point of the arrow will pass through the point that is\nLerp(root, followMouse, 0.33) + arrowBend1")]
    private Vector3 arrowBend1;

    [SerializeField]
    [Header("Set by intent controller, here for manual editing/tweaking during runtime\nThe 2/3rd point of the arrow will pass through the point that is\nLerp(root, followMouse, 0.66) + arrowBend2")]
    private Vector3 arrowBend2;

    [SerializeField]
    private Vector3 targetPositionOffset = Vector3.zero;
    public bool frozen = false;

    private Transform target = null;
    private Vector2 targetVector2 = Vector2.zero;
    private Vector3 BendToPoint1;
    private Vector3 BendToPoint2;
    public float selfTargetBulge = 1;
    public bool divideBendByLength = true;

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
        if (frozen && FollowMouse != null)
        {
            // Not following the mouse anymore, but still want to
            // follow the target that's been set if it moves
            if (target != null) FollowMouse.position = target.position;
            else FollowMouse.position = targetVector2;
        }
    }

    private void DrawCurve()
    {
        // TODO to make this and the root offset work the same way
        Vector3 targetPosition = FollowMouse.transform.position + targetPositionOffset;
        // a point 1/3 of the way from Root to Target 
        BendToPoint1 = Vector3.Lerp(Root.transform.position, targetPosition, 0.333f);
        BendToPoint2 = Vector3.Lerp(Root.transform.position, targetPosition, 0.666f);
        // Add the bend in the arrow
        Vector3 normalizedBend1 = getNormalizedBend(arrowBend1);
        Vector3 normalizedBend2 = getNormalizedBend(arrowBend2);

        BendToPoint1 = new Vector3(BendToPoint1.x + normalizedBend1.x, BendToPoint1.y + normalizedBend1.y, Zpos + normalizedBend1.z);
        BendToPoint2 = new Vector3(BendToPoint2.x + normalizedBend2.x, BendToPoint2.y + normalizedBend2.y, Zpos + normalizedBend2.z);

        // Add a bulge if self-targeting
        BendToPoint1 = new Vector3(BendToPoint1.x + (Root.transform.position == targetPosition ? selfTargetBulge : 0), BendToPoint1.y + normalizedBend1.y, Zpos);
        // a point 2/3 of the way from Root to targetPosition
        var pointList = new List<Vector3>();
        Vector3 currentBendPoint = BendToPoint1;
        List<Vector3> lerpPoints = new List<Vector3>(){
                    Root.position,
                    BendToPoint1,
                    BendToPoint2,
                    targetPosition
                }; ;

        for (float ratio = 0; ratio <= 1; ratio += 1 / vertexCount)
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

    public void setColor(Color color)
    {
        linerenderer.endColor = color;
        linerenderer.startColor = color;
    }

    // All that's necessary 
    public void setAllChildrenPosition(Vector3 position)
    {
        foreach (Transform child in transform)
        {
            child.position = position;
        }
    }

    public void freeze(Transform target, Vector3 targetPositionOffset)
    {
        this.targetPositionOffset = targetPositionOffset;
        freeze(target);
    }

    public void freeze(Transform target)
    {
        frozen = true;
        followMouse.followMouse = false;
        this.target = target;
    }

    public void freeze(Vector2 position)
    {
        frozen = true;
        followMouse.followMouse = false;
        this.targetVector2 = position;
    }

    private Vector3 getNormalizedBend(Vector3 bend)
    {
        Vector3 normalized;
        if (divideBendByLength)
        {
            Vector3 targetPosition = followMouse.transform.position + targetPositionOffset;
            float arrowLength = Vector3.Distance(Root.transform.position, targetPosition);
            if (arrowLength != 0)
            {
                normalized = bend / arrowLength;
                return normalized;
            }
            else { return bend; }
        }
        else
        {
            return bend;
        }
    }

    public void SetArrowBends(Vector3 bend1, Vector3 bend2)
    {
        arrowBend1 = bend1;
        arrowBend2 = bend2;
    }
}