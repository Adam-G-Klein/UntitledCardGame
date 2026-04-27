using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class ShopTutorialDisplay : MonoBehaviour
{
    public enum ShopTutorialToShow
    {
        FullFeatureShopTutorial,
        None
    }
    private ShopDataSO shopData;
    private EncounterConstantsSO encounterConstants;
    [SerializeField]
    private float fullyUpgradeShopTime = 3f;

    [SerializeField]
    private float postShopUpgradeDelay = 1f;
    [SerializeField]
    private float delayPerFreeMoneyGiven = 0.1f;
    [SerializeField]
    private float postFreeMoneyDelay = 1f;
    [SerializeField]
    private float delayPerFreeRerollGiven = 0.5f;
    [SerializeField]
    private float postFreeRerollDelay = 1f;
    [SerializeField]
    private float delayPerFreeCardRemovalGiven = 1f;


    public void Setup(ShopDataSO shopDataSO, EncounterConstantsSO encounterConstants, ShopTutorialToShow shopTutorialToShow = ShopTutorialToShow.FullFeatureShopTutorial)
    {
        this.encounterConstants = encounterConstants;
        this.shopData = shopDataSO;
        StartCoroutine(RunTutorialWrapper(shopTutorialToShow));
    }

    private IEnumerator RunTutorialWrapper(ShopTutorialToShow shopTutorialToShow)
    {
        switch (shopTutorialToShow)
        {
            case ShopTutorialToShow.FullFeatureShopTutorial:
                yield return FullFeatureShopTutorialCoroutine();
                break;
            default:
                break;
        }
        CinematicIntroComplete();
    }
    // Called here, so we can wait on the first dialogue to finish before starting the cinematic
    public IEnumerator FullFeatureShopTutorialCoroutine()
    {
        // UI stays disabled from the call in ShopManager, it's waiting on cinematicIntroComplete = true;
        int prevGold = ShopManager.Instance.gameState.playerData.GetValue().gold;
        ShopManager.Instance.gameState.playerData.initialize(prevGold);
        yield return DemoDirector.Instance.InvokeDemoStepCoroutine(DemoStepName.FullFeatureShopTutorialStep1);
        yield return GiveFreeMoney();
        yield return new WaitForSeconds(postFreeMoneyDelay);
        yield return DemoDirector.Instance.InvokeDemoStepCoroutine(DemoStepName.FullFeatureShopTutorialStep2);
        yield return FullyUpgradeShopCoroutine();
        yield return new WaitForSeconds(postShopUpgradeDelay);
        yield return DemoDirector.Instance.InvokeDemoStepCoroutine(DemoStepName.FullFeatureShopTutorialStep3);
        yield return GiveFreeRerolls();
        yield return new WaitForSeconds(postFreeRerollDelay);
        yield return GiveFreeCardRemovals();
        yield return DemoDirector.Instance.InvokeDemoStepCoroutine(DemoStepName.FullFeatureShopTutorialStep4);
    }

    public IEnumerator GiveFreeRerolls()
    {
        int rerollsGiven = 0;
        while (rerollsGiven < shopData.freeRerolls)
        {
            ShopManager.Instance.AddFreeReroll();
            yield return new WaitForSeconds(delayPerFreeRerollGiven);
            rerollsGiven += 1;
        }
    }

    public IEnumerator GiveFreeMoney()
    {
        int moneyGiven = 0;
        while (moneyGiven < shopData.freeMoney)
        {
            ShopManager.Instance.AddFreeMoney();
            yield return new WaitForSeconds(delayPerFreeMoneyGiven);
            moneyGiven += 1;
        }

    }

    public IEnumerator GiveFreeCardRemovals()
    {
        int removalsGiven = 0;
        while (removalsGiven < shopData.freeCardRemovals)
        {
            ShopManager.Instance.AddFreeCardRemoval();
            yield return new WaitForSeconds(delayPerFreeCardRemovalGiven);
            removalsGiven += 1;
        }
    }

    private IEnumerator FullyUpgradeShopCoroutine()
    {
        int totalIncrementsNeeded = 0;
        foreach(ShopLevel shopLevel in encounterConstants.shopLevels)
        {
            totalIncrementsNeeded += shopLevel.shopLevelIncrementsToUnlock;
        }
        totalIncrementsNeeded -= ShopManager.Instance.gameState.playerData.GetValue().shopLevelIncrementsEarned;
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
