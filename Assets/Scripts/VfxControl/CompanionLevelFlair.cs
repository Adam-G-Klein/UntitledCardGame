using System.Collections;
using UnityEngine;

/*
    VFX: 2- and 3-star rats get a little more in the smoke behind them, on top of the silver and gold tint
    CompanionInstance already gives that smoke: silver glints for level two, gold leaf and glints for level three.
    It reads the rat's level itself, so CompanionInstance is unchanged. Purely visual. Set on WorldSpaceCompanion's
    Combat Screen Highlight (VFX Retool / Wire smoke looks); the flair prefabs come from VFX Retool / Build smoke flair.
*/
public class CompanionLevelFlair : MonoBehaviour
{
    [SerializeField] private GameObject levelTwoFlair;
    [SerializeField] private GameObject levelThreeFlair;

    private IEnumerator Start()
    {
        // the rat is set up by whoever spawned it; give it a few frames
        CompanionInstance rat = null;
        for (int tries = 0; tries < 30; tries++)
        {
            rat = GetComponentInParent<CompanionInstance>();
            if (rat != null && rat.companion != null && rat.companion.companionType != null) break;
            yield return null;
        }
        if (rat == null || rat.companion == null || rat.companion.companionType == null) yield break;

        GameObject flair = null;
        switch (rat.companion.companionType.level)
        {
            case CompanionLevel.LevelTwo: flair = levelTwoFlair; break;
            case CompanionLevel.LevelThree: flair = levelThreeFlair; break;
        }
        if (flair == null) yield break;
        GameObject instance = Instantiate(flair, transform);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
    }
}
