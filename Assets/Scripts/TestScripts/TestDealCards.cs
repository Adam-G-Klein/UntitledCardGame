
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDealCards : MonoBehaviour
{
    public GameObject cardPrefab;
    public int count = 5;
    public GameObject startLoc;
    public Vector3 step;
    public Vector3 currOffset = Vector3.zero;

    void Start() {
        StartCoroutine(DealCards());
    }

    private IEnumerator DealCards()
    {
        yield return new WaitForSeconds(1);
        for (int i = 0; i < count; i++)
        {
            GameObject card = Instantiate(cardPrefab, startLoc.transform.position + currOffset, Quaternion.identity);
            currOffset += step;
        }
    }
}