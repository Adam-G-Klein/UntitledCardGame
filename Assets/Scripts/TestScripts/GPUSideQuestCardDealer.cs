using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GPUSideQuestCardDealer : MonoBehaviour
{

    [SerializeField]
    private GameObject quadPrefab;

    [SerializeField]
    private RenderTexture renderTexture;

    // Start is called before the first frame update
    void Start()
    {
        GameObject quad = Instantiate(quadPrefab);
        quad.GetComponent<MeshRenderer>().material.mainTexture = renderTexture;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
