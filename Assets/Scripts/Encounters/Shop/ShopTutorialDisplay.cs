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
    private float delayPerFreeMoneyGiven = 0.1f;
    [SerializeField]
    private float postFreeMoneyDelay = 1f;
    [SerializeField]
    private float delayPerFreeRerollGiven = 0.5f;
    [SerializeField]
    private float postFreeRerollDelay = 1f;
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
        int prevGold = ShopManager.Instance.gameState.playerData.GetValue().gold;
        ShopManager.Instance.gameState.playerData.initialize(ShopManager.Instance.getShopEncounter().shopData);
        ShopManager.Instance.gameState.playerData.GetValue().gold = prevGold;
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
        CinematicIntroComplete();
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
