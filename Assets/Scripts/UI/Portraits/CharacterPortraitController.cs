using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPortraitController : MonoBehaviour
{
    [SerializeField] private Transform parentTransform;
    [SerializeField] private GameObject portraitParentGO;

    public void Awake() {
    }
    
    public void SetupCharacterPortraits(List<CompanionInstance> companions) {
        foreach (CompanionInstance companion in companions) {
            GameObject parentPortrait = Instantiate(
                portraitParentGO,
                Vector3.zero,
                Quaternion.identity,
                parentTransform);
            CharacterPortrait portrait = parentPortrait.GetComponent<CharacterPortrait>();
            portrait.Setup(companion);
        }
    }
}
