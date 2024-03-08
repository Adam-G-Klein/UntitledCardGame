using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ManaRingController : MonoBehaviour
{

    [SerializeField]
    private List<GameObject> manaOrbs;

    [SerializeField]
    private TextMeshProUGUI manaText;
    private int displayedMana = 0;
    void Update(){
        // copping out here
        var mana = ManaManager.Instance.currentMana;
        manaText.text = mana.ToString();
        updateSprites(mana);
    }

    private void updateSprites(int mana) {
        if(displayedMana == mana) return; // the tiniest performance optimization
        for (int i = 0; i < manaOrbs.Count; i++) {
            if (i < mana) {
                manaOrbs[i].SetActive(true);
            } else {
                manaOrbs[i].SetActive(false);
            }
        }
        displayedMana = mana;
    }

}