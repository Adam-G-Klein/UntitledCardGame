using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DualChildLayoutController : UIBehaviour, ILayoutSelfController
{
    public RectTransform child1;
    public RectTransform child2;

    public void Update() {
        UpdateRectTransform();
    }

    //This handles horizontal aspects of the layout (derived from ILayoutController)
    public virtual void SetLayoutHorizontal()
    {
        //Move and Rotate the RectTransform appropriately
        UpdateRectTransform();
    }

    //This handles vertical aspects of the layout
    public virtual void SetLayoutVertical()
    {
        //Move and Rotate the RectTransform appropriately
        UpdateRectTransform();
    }

    void UpdateRectTransform()
    {
        //Fetch the RectTransform from the GameObject
        RectTransform rectTransform = GetComponent<RectTransform>();

        // rectTransform.sizeDelta = new Vector2(rectTransform.rect.width, Mathf.Max(child1.rect.height, child2.rect.height);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(child1.rect.height, child2.rect.height));
    }
}
