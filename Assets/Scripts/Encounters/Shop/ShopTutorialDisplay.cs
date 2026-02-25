using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class ShopTutorialDisplay : MonoBehaviour
{

    private ShopDataSO shopData;
    [SerializeField]
    private float fullyUpgradeShopTime = 3f;

    [SerializeField]
    private float postShopUpgradeDelay = 1f;
    [SerializeField]
    private int freeRerolls = 6;
    [SerializeField]
    private float delayPerFreeRerollGiven = 0.5f;
    [SerializeField]
    private int freeCardRemovals = 2;
    [SerializeField]
    private float delayPerFreeCardRemovalGiven = 1f;


    public void Setup(ShopDataSO shopDataSO)
    {
        shopData = shopDataSO;
        StartCoroutine(ShopTutorialCoroutine());
    }

    // Called here, so we can wait on the first dialogue to finish before starting the cinematic
    public IEnumerator ShopTutorialCoroutine()
    {
        // UI stays disabled from the call in ShopManager, it's waiting on cinematicIntroComplete = true;
        yield return DemoDirector.Instance.InvokeDemoStepCorouutine(DemoStepName.SecondShopTutorialStep1);
        yield return FullyUpgradeShopCoroutine();
        yield return new WaitForSeconds(postShopUpgradeDelay);
        yield return PlayShopDialogue2();
        yield return GiveFreeRerolls();
        yield return GiveFreeCardRemovals();
        yield return PlayShopDialogue3();
        CinematicIntroComplete();
    }

    public IEnumerator PlayShopDialogue2()
    {
        yield return DemoDirector.Instance.InvokeDemoStepCorouutine(DemoStepName.SecondShopTutorialStep2);
    }

    public IEnumerator PlayShopDialogue3()
    {
        yield return DemoDirector.Instance.InvokeDemoStepCorouutine(DemoStepName.SecondShopTutorialStep3);
    }

    public IEnumerator GiveFreeRerolls()
    {
        int rerollsGiven = 0;
        while (rerollsGiven < freeRerolls)
        {
            ShopManager.Instance.AddFreeReroll();
            yield return new WaitForSeconds(delayPerFreeRerollGiven);
            rerollsGiven += 1;
        }
    }

    public IEnumerator GiveFreeCardRemovals()
    {
        int removalsGiven = 0;
        while (removalsGiven < freeCardRemovals)
        {
            ShopManager.Instance.AddFreeCardRemoval();
            yield return new WaitForSeconds(delayPerFreeCardRemovalGiven);
            removalsGiven += 1;
        }
    }

    private IEnumerator FullyUpgradeShopCoroutine()
    {
        int totalIncrementsNeeded = 0; 
        foreach(ShopLevel shopLevel in shopData.shopLevels)
        {
            totalIncrementsNeeded += shopLevel.shopLevelIncrementsToUnlock;
        }
        float timePerIncrement = fullyUpgradeShopTime / totalIncrementsNeeded;
        int incrementsApplied = 0;
        while(incrementsApplied < totalIncrementsNeeded)
        {
            ShopManager.Instance.UpgradeShopForFree();
            yield return new WaitForSeconds(timePerIncrement);
            incrementsApplied += 1;
        }
    }

    public void CinematicIntroComplete()
    {
        ShopManager.Instance.CinematicIntroComplete();
    }
}
