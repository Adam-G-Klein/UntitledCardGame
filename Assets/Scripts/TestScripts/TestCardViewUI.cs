using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCardViewUI : MonoBehaviour
{
    public GameObject cardViewUIPrefab;
    public List<Card> cards;
    public int selections;
    public string promptText;

    private GameObject cardViewUI;

    // Start is called before the first frame update
    void Start()
    {
        cardViewUI = GameObject.Instantiate(
                        cardViewUIPrefab,
                        new Vector3(Screen.width / 2, Screen.height / 2, 0),
                        Quaternion.identity);
        cardViewUI.GetComponent<CardViewUI>().Setup(cards, selections, promptText, selections);
    }
}
