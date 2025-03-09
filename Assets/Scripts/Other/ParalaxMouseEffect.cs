using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseParalaxEffect : MonoBehaviour
{
    [Tooltip("Start from thurthest to the nearest object.")]
    [SerializeField] private GameObject[] ParalaxObjects;
    [SerializeField] private float MouseSpeedX = 1f, MouseSpeedY = .2f;
    [SerializeField] private Camera cam;

    //Paralax effect will be applied as an ofset to the original positions
    private Vector3[] OriginalPositions;

    // Start is called before the first frame update
    void Start()
    {
        //Cursor.lockState = CursorLockMode.Confined;

        OriginalPositions = new Vector3[ParalaxObjects.Length];
        for (int i = 0; i < ParalaxObjects.Length; i++)
        {
            OriginalPositions[i] = ParalaxObjects[i].transform.position;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float x, y;
        x = (Input.mousePosition.x - (Screen.width / 2)) * MouseSpeedX / Screen.width;
        y = (Input.mousePosition.y - (Screen.height / 2)) * MouseSpeedY / Screen.height;
        //For each object in ParalaxObjects calculate and applly an offset based on cursor position
        for (int i = 1; i < ParalaxObjects.Length+1; i++)
        {
            ParalaxObjects[i-1].transform.position = OriginalPositions[i-1] + (new Vector3(x, y, 0f) * i * ((i-1) - (ParalaxObjects.Length/2)));
        }
    }
}