using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Won't execute in prefab edit mode, 
// apparently there's some weird logic there
// we'd have to account for
[ExecuteInEditMode]
public class CardDisplay : MonoBehaviour
{
    public CardInfo cardInfo;

    public TMP_Text CardName;
    public TMP_Text CardDesc;
    public Image Artwork;
    private Camera mainCamera;
    [SerializeField]
    private float hoverScale = 30f;
    [SerializeField]
    private float nonHoverScale = 20f;
    [SerializeField]
    private float hoverYDiff = 185f;

    // Start is called before the first frame update
    void Update()
    {
        runInEditMode = true;
        CardName.text = cardInfo.Name;
        CardDesc.text = cardInfo.Description;
        Artwork.sprite = cardInfo.Artwork;
    }

    void Awake()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    void OnMouseEnter()
    {
        print("Mouse entered, y before: " +  transform.localPosition.y);
        transform.localScale = new Vector3(hoverScale, hoverScale, 1);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + hoverYDiff, transform.localPosition.z);
        print("Mouse entered, y after: " +  transform.localPosition.y);
    }
    void OnMouseExit()
    {
        transform.localScale = new Vector3(nonHoverScale, nonHoverScale, 1);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - hoverYDiff, transform.localPosition.z);
    }

}
