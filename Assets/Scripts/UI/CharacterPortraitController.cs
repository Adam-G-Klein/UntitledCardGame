using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPortraitController : MonoBehaviour
{
    [SerializeField] private Transform parentTransform;
    [SerializeField] private GameObject portraitParentGO;
    [SerializeField] private List<CharacterPortrait> characterPortraits;

    private Dictionary<CompanionTypeSO, GameObject> _characterPortraitsMap;

    public void Awake() {
        _characterPortraitsMap = new Dictionary<CompanionTypeSO, GameObject>();
        foreach (CharacterPortrait portrait in characterPortraits) {
            _characterPortraitsMap.Add(portrait.companionType, portrait.characterPortrait);
        }
    }
    
    public void SetupCharacterPortraits(List<Companion> companions) {
        foreach (Companion companion in companions) {
            GameObject parentPortrait = Instantiate(
                portraitParentGO,
                Vector3.zero,
                Quaternion.identity,
                parentTransform);
            GameObject characterPortrait = _characterPortraitsMap[companion.companionType];
            Instantiate(
                characterPortrait,
                Vector3.zero,
                Quaternion.identity,
                parentPortrait.transform);
        }
    }

    [System.Serializable]
    public class CharacterPortrait {
        public CompanionTypeSO companionType;
        public GameObject characterPortrait;
    }
}
