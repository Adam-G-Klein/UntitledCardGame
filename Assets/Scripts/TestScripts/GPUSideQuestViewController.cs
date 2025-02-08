using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GPUSideQuestViewController : MonoBehaviour
{

    [SerializeField]
    private UIDocument uIDocument;
    private VisualElement pic;
    [SerializeField]
    private float scaleSpeed;

    // Start is called before the first frame update
    void Start()
    {
        uIDocument = GetComponent<UIDocument>();
        pic = uIDocument.rootVisualElement.Q<VisualElement>("pic");
        if(pic == null)
        {
            Debug.LogError("pic not found");
        }
        else
        {
            Debug.Log("pic found");
        }
    }

    // Update is called once per frame
    void Update()
    {
        float scaleAmount = scaleSpeed * Time.deltaTime;
        pic.transform.scale += new Vector3(scaleAmount % 1, scaleAmount % 1, 0);
    }
}
