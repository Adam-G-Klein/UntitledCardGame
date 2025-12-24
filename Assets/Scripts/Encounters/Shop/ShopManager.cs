using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopManager : GenericSingleton<ShopManager>, IEncounterBuilder
{
    public bool IS_DEVELOPMENT_MODE = false;

    [Header("Variables")]
    public GameStateVariableSO gameState;

    [Header("Shop")]
    public ShopViewController shopViewController;
    public EncounterConstantsSO encounterConstants;
    public VoidGameEvent shopRefreshEvent;
    public GameObject companionViewUIPrefab;

    [Header("VFX")]
    public GameObject moneyGainedPrefab;
    public GameObject moneySpentPrefab;
    public GameObject shopRerollPrefab;
    public GameObject shopUpgradePrefab;
    public GameObject healPrefab;
    public GameObject sparklePrefab;
    public GameObject bigSparklePrefab;

    private ShopEncounter shopEncounter;
    private ShopLevel shopLevel;
    private bool buyingCard = false;
    private bool removingCard = false;
    private CardInShopWithPrice currentCardBuyRequest;
    private ShopItemView currentCardBuyRequestItemView;
    private CompanionCombinationManager companionCombinationManager;
    [SerializeField]
    public UIDocumentGameObjectPlacer placer { get; set; }
    private CompanionInShopWithPrice companionInShop;
    private Companion newCompanion;
    public GameObject tooltipPrefab;
    private int timesRerolledThisShop = 0;
    private bool healedCompanions;
    private int availableBenchSlots;
    public int AvailableBenchSlots {
        get { return availableBenchSlots; }
    }
    void Awake() {
        gameObject.GetComponent<UIDocument>().enabled = true;
        shopViewController.Init(this);
        // This ends up calling BuildShopEncounter below
        gameState.activeEncounter.GetValue().BuildWithEncounterBuilder(this);
    }

    void Start()
    {
        companionCombinationManager = GetComponent<CompanionCombinationManager>();
        availableBenchSlots = ProgressManager.Instance.IsFeatureEnabled(AscensionType.WORSE_BENCH) ? (int)ProgressManager.Instance.GetAscensionSO(AscensionType.WORSE_BENCH).
            ascensionModificationValues.GetValueOrDefault("numSlots", 3f) : 5;
        healedCompanions = false;
    }

    public void BuildShopEncounter(ShopEncounter shopEncounter) {
        this.shopEncounter = shopEncounter;
        this.shopLevel = shopEncounter.shopData.GetShopLevel(gameState.playerData.GetValue().shopLevel);
        List<Companion> allCompanions = new();
        allCompanions.AddRange(gameState.companions.activeCompanions);
        allCompanions.AddRange(gameState.companions.benchedCompanions);
        shopEncounter.Build(this, allCompanions, encounterConstants, this.shopLevel);
        shopViewController.SetMoney(gameState.playerData.GetValue().gold);
        shopViewController.SetShopUpgradePrice(shopLevel.upgradeIncrementCost);
        shopViewController.SetShopRerollPrice(shopEncounter.shopData.rerollShopPrice);
        shopViewController.SetShopCardRemovalPrice(shopEncounter.shopData.cardRemovalPrice);

        CheckDisableUpgradeButtonV2();

        shopViewController.SetupUpgradeIncrements(shopEncounter.shopData.shopLevels.Count - 1 <= shopLevel.level);
        /* uncomment to re-enable shop dialogue
        DialogueManager.Instance.SetDialogueLocation(
            gameState.dialogueLocations.GetDialogueLocation(gameState));
        DialogueManager.Instance.StartAnyDialogueSequence();
        */
    }

    public void SetupUnitManagement() {
        StartCoroutine(DelayedSetupUnitManagement());
    }

    private IEnumerator DelayedSetupUnitManagement()
    {
        yield return new WaitForEndOfFrame();
        shopViewController.RebuildUnitManagement(gameState.companions);

        // Heal the companions on the bench if it's the first time this shop.
        if (!healedCompanions)
        {
            yield return new WaitForEndOfFrame();
            HealCompanionsOnBench();
            healedCompanions = true;
        }
    }

    private void HealCompanionsOnBench()
    {
        int scale = shopEncounter.shopData.benchHealingAmount;
        if (ProgressManager.Instance.IsFeatureEnabled(AscensionType.WORSE_BENCH))
        {
            scale -= (int)ProgressManager.Instance.GetAscensionSO(AscensionType.WORSE_BENCH).
                ascensionModificationValues.GetValueOrDefault("healingReduction", 0f);
        }
        scale = Math.Max(scale, 0);
        for (int i = 0; i < gameState.companions.benchedCompanions.Count; i++)
        {
            Companion rat = gameState.companions.benchedCompanions[i];
            int updatedHealth = Mathf.Min(rat.combatStats.getCurrentHealth() + scale, rat.combatStats.maxHealth);
            rat.combatStats.setCurrentHealth(updatedHealth);
            InstantiateShopVFX(healPrefab, shopViewController.GetBenchSlotVE(i), 1.1f);
        }
        shopViewController.RefreshCompanionViews();
    }

    void Update() {
        // I want to make it very clear this is frowned upon and is only for testing
        if(IS_DEVELOPMENT_MODE && Input.GetKeyDown(KeyCode.R)) {
            rerollShop();
        }

        if (IS_DEVELOPMENT_MODE && Input.GetKeyDown(KeyCode.G)) {
            gameState.playerData.GetValue().gold += 1;
        }
    }

    public void ProcessCardBuyRequestV2(ShopItemView shopItemView, CardInShopWithPrice cardInShop) {
        if (DialogueManager.Instance.dialogueInProgress) {
            return;
        }
        if (gameState.playerData.GetValue().gold >= cardInShop.price) {
            this.buyingCard = true;
            this.currentCardBuyRequest = cardInShop;
            this.currentCardBuyRequestItemView = shopItemView;
            shopViewController.CardBuyingSetup(shopItemView, cardInShop);
        } else {
            shopViewController.NotEnoughMoney();
        }
    }

    public bool IsApplicableCompanion(CardInShopWithPrice cardInShop, Companion companion) {
        // return cardInShop.cardPool == companion.companionType.cardPool ||
        //     cardInShop.cardPool == companion.companionType.pack.packCardPoolSO ||
        //     cardInShop.cardPool == shopEncounter.shopData.neutralCardPool;
        // (James): experimental change in which we do not restrict which companion gets which card.
        return true;
    }

    public void ProcessCompanionBuyRequest(ShopItemView shopItemView, CompanionInShopWithPrice companionInShop) {
        Debug.Log("Processing companion buy request");
        if (DialogueManager.Instance.dialogueInProgress) {
            return;
        }

        if (gameState.playerData.GetValue().gold >= companionInShop.price)
        {
            // Create a new instance of the companion and then attempt companion upgrades before adding
            // them to your team;
            this.companionInShop = companionInShop;
            newCompanion = new Companion(companionInShop.companionType);
            newCompanion.combatStats.currentHealth -= companionInShop.sustainedDamage;
            // companionToAdd is the final companion to add to your team :)
            Companion companionToAdd = newCompanion;

            UpgradeInfo upgradeInfo = companionCombinationManager.PurchaseWouldCauseUpgrade(newCompanion);
            if (upgradeInfo != null)
            {
                shopViewController.isBuyingDisabled = true;
                shopViewController.canDragCompanions = false;
                gameState.playerData.GetValue().gold -= companionInShop.price;
                shopViewController.SetMoney(gameState.playerData.GetValue().gold);
                InstantiateShopVFX(moneySpentPrefab, shopItemView.shopItemElement, 1.5f);
                MusicController.Instance.PlaySFX("event:/SFX/SFX_CompanionBuy");

                // first we need to find the companionManagementSlotView that all of them are animating towards
                CompanionManagementSlotView slotView = shopViewController.GetCompanionManagementSlotView(upgradeInfo.resultingSlotViewIndex, upgradeInfo.onBench);
                Companion targetCompanion = slotView.companionManagementView.companion;
                int animationsToDo = upgradeInfo.companionsInvolved.Count - 2; // (one for the shop purchase that's taken care of right away and one for the resulting companion view that doesn't move)
                float delay = 0;
                shopViewController.AnimateNewCompanionToSlot(companionInShop, slotView, true, delay, () => { shopViewController.CompanionScaleBumpAnimation(slotView.companionManagementView.container, 1, 1.2f, .25f); });
                delay += .3f;

                for (int i = 0; i < upgradeInfo.companionsInvolved.Count; i++)
                {
                    Companion companion = upgradeInfo.companionsInvolved[i];

                    // don't animate the target companion
                    if ((companion == targetCompanion) || (companion == newCompanion)) continue;

                    CompanionManagementSlotView startingSlotView = shopViewController.GetCompanionManagementSlotView(companion);
                    animationsToDo -= 1;
                    bool isLastAnimation = animationsToDo == 0;
                    shopViewController.AnimateExistingCompanionToSlot(startingSlotView, slotView, true, delay, () => { AnimateExistingCompanionToSlotOnComplete(startingSlotView, slotView.companionManagementView.container, isLastAnimation, slotView, upgradeInfo); });
                    delay += .3f;

                    // the rest are animated from other companionManagementView (this does need to be handled slightly differently)
                }
                return;
            }
            if (gameState.companions.activeCompanions.Count + shopViewController.GetBlockedCompanionSlots() == 5 && gameState.companions.benchedCompanions.Count == availableBenchSlots)
            {
                StartCoroutine(shopViewController.ShowGenericNotification("You have reached the maximum number of companions.", 2));
                return;
            }
            shopViewController.isBuyingDisabled = true;
            shopViewController.canDragCompanions = false;
            gameState.playerData.GetValue().gold -= companionInShop.price;
            shopViewController.SetMoney(gameState.playerData.GetValue().gold);
            gameState.AddCompanionToTeam(companionToAdd);
            CompanionManagementSlotView companionManagementSlotView = shopViewController.FindNextAvailableSlot();
            shopViewController.AnimateNewCompanionToSlot(companionInShop, companionManagementSlotView, false, 0, () => { CompanionBoughtAnimationOnComplete(companionToAdd, companionManagementSlotView); });
            InstantiateShopVFX(moneySpentPrefab, shopItemView.shopItemElement, 1.5f);
            MusicController.Instance.PlaySFX("event:/SFX/SFX_CompanionBuy");

            // Also record an analytics event for buying the companion.
            var eventData = new CompanionBuyAnalyticsEvent
            {
                CompanionName = companionInShop.companionType.name,
                PackName = companionInShop.companionType.pack.name,
                Rarity = companionInShop.rarity.ToString()
            };
            AnalyticsManager.Instance.RecordEvent(eventData);
        }
        else
        {
            Debug.Log("Not enuff munny");
            shopViewController.NotEnoughMoney();
        }
    }
    private void AnimateExistingCompanionToSlotOnComplete(CompanionManagementSlotView companionManagementSlotView, VisualElement visualElement, bool isLastAnimation, CompanionManagementSlotView remainingSlotView, UpgradeInfo upgradeInfo)
    {
        // maybe this should happen before the animation is over
        gameState.RemoveCompanionsFromTeam(new List<Companion> { companionManagementSlotView.companionManagementView.companion });

        companionManagementSlotView.Reset();
        shopViewController.CompanionScaleBumpAnimation(visualElement, 1, 1.2f, .25f);

        if (isLastAnimation)
        {
            shopViewController.CompanionUpgradeAnimation(remainingSlotView, upgradeInfo);
        }
    }


    private void CompanionBoughtAnimationOnComplete(Companion companion, CompanionManagementSlotView companionManagementSlotView)
    {
        shopViewController.SetupCompanionManagementView(companion, companionManagementSlotView);
        shopViewController.CompanionScaleBumpAnimation(companionManagementSlotView.companionManagementView.container, 1, 1.2f, .25f);
        shopViewController.isBuyingDisabled = false;
        shopViewController.canDragCompanions = true;
        //Sparkle(companionManagementSlotView.root);
    }

    private void InstantiateShopVFX(GameObject prefab, VisualElement ve, float scale)
    {
        GameObject instance = Instantiate(prefab, UIDocumentGameObjectPlacer.GetWorldPositionFromElement(ve), Quaternion.identity);
        ScaleGameObjectAndChildren(instance, scale);
    }

    public void Sparkle(VisualElement ve) {
        InstantiateShopVFX(sparklePrefab, ve, 1.0f);
    }

    public void UpgradeSparkle(VisualElement ve)
    {
        InstantiateShopVFX(bigSparklePrefab, ve, 1.0f);
    }
    private void ScaleGameObjectAndChildren(GameObject obj, float scale)
    {
        obj.transform.localScale *= scale;
        foreach (Transform child in obj.transform)
        {
            child.localScale *= scale;
        }
    }

    public void CancelUpgradePurchase() {
        companionInShop = null;
        newCompanion = null;
    }

    public void ConfirmUpgradePurchase() {
        gameState.playerData.GetValue().gold -= companionInShop.price;
        shopViewController.SetMoney(gameState.playerData.GetValue().gold);

        int preferredActiveSlotIdx = -1;
        Companion companionToAdd = null;
        Companion level2Dude = companionCombinationManager.AttemptCompanionUpgrade(newCompanion);
        if (level2Dude != null)
        {
            // Find an active slot to place the companion so it keeps the same spot in your team.
            preferredActiveSlotIdx = gameState.companions.activeCompanions.FindIndex(c => c.companionType == newCompanion.companionType); ;

            companionToAdd = level2Dude;
            // Then attempt the level 3 upgrade :)
            int existingL2CompanionActiveSlot = gameState.companions.activeCompanions.FindIndex(c => c.companionType == level2Dude.companionType);
            Companion level3Dude = companionCombinationManager.AttemptCompanionUpgrade(level2Dude);
            if (level3Dude != null)
            {
                if (preferredActiveSlotIdx == -1)
                {
                    preferredActiveSlotIdx = existingL2CompanionActiveSlot;
                }
                else
                {
                    preferredActiveSlotIdx = Math.Min(preferredActiveSlotIdx, existingL2CompanionActiveSlot);
                }

                companionToAdd = level3Dude;
            }
            Debug.Log("Preferred active slot index for the companion: " + preferredActiveSlotIdx);
            gameState.AddCompanionToTeam(companionToAdd, preferredActiveSlotIdx);
            shopViewController.RemoveCompanionFromShopView(companionInShop);
            shopViewController.RebuildUnitManagement(gameState.companions);
            return;
        }
    }

    public void ProcessCompanionClicked(CompanionManagementView companionView) {
        // The player selected a companion, so the transaction is complete
        // (assuming there is a transaction) and we're gonna add the card
        // to the companion's deck and lets forcefully close the companion
        // view UI
        Companion companion = companionView.companion;
        if (this.buyingCard)
        {
            if (!IsApplicableCompanion(currentCardBuyRequest, companion)) return;
            Card newCard = new Card(currentCardBuyRequest.cardType, companion.companionType, currentCardBuyRequest.rarity);
            companion.deck.cards.Add(newCard);
            companion.trackingStats.RecordCardBuy(newCard);
            gameState.playerData.GetValue().gold -= currentCardBuyRequest.price;
            shopViewController.SetMoney(gameState.playerData.GetValue().gold);
            // shopViewController.RemoveCardFromShopView(currentCardBuyRequestItemView.cardInShop);
            shopViewController.AnimateCardToCompanion(currentCardBuyRequestItemView.cardInShop, companionView);
            this.buyingCard = false;
            shopViewController.StopBuyingCard();
            InstantiateShopVFX(moneySpentPrefab, currentCardBuyRequestItemView.shopItemElement, 1.5f);

            // Fire off a cardBought Analytics event.
            var cardBuy = new CardBuyAnalyticsEvent
            {
                CardName = newCard.cardType.name,
                PackName = currentCardBuyRequest.packSO?.name ?? "",
                CompanionName = companion.companionType.name,
                CardCategory = newCard.cardType.cardCategory.ToString(),
                Rarity = newCard.shopRarity.ToString(),
            };
            AnalyticsManager.Instance.RecordEvent(cardBuy);
        }

        if (removingCard) {
            shopViewController.ShopDeckViewForCardRemoval(companion);
        }
    }

    public void ProcessCardRemoval() {
        gameState.playerData.GetValue().gold -= shopEncounter.shopData.cardRemovalPrice;
        shopViewController.SetMoney(gameState.playerData.GetValue().gold);
        removingCard = false;
    }

    public void ProcessCardBuyCanceled() {
        this.buyingCard = false;
    }

    public void ProcessCardRemovalCancelled() {
        removingCard = false;
    }

    public void SellCompanion(Companion companion, VisualElement ve) {
        if (gameState.companions.activeCompanions.Count + gameState.companions.benchedCompanions.Count == 1) {
            shopViewController.ShowCantSellLastCompanion();
            return;
        }
        int sellValue = CalculateCompanionSellPrice(companion).Total();
        gameState.playerData.GetValue().gold += sellValue;
        shopViewController.SetMoney(gameState.playerData.GetValue().gold);
        gameState.companions.activeCompanions.Remove(companion);
        gameState.companions.benchedCompanions.Remove(companion);
        shopViewController.RebuildUnitManagement(gameState.companions);
        InstantiateShopVFX(moneyGainedPrefab, ve, 1.1f);
    }

    public CompanionSellValue CalculateCompanionSellPrice(Companion companion) {
        return new CompanionSellValue(shopEncounter.shopData, companion.trackingStats, companion.companionType.level);
    }

    // public int CalculateCompanionSellPriceWithBreakdown(Companion companion) {

    // }

    public void ProcessUpgradeShopClick() {
        if(DialogueManager.Instance.dialogueInProgress) {
            return;
        }
        PlayerData playerData = gameState.playerData.GetValue();
        if (playerData.gold >= shopLevel.upgradeIncrementCost) {
            // Clean up hoverables for old shop items

            playerData.gold -= shopLevel.upgradeIncrementCost;
            EarnUpgradeIncrement();

            shopViewController.SetMoney(gameState.playerData.GetValue().gold);
        } else {
            shopViewController.NotEnoughMoney();
        }
    }

    // EarnUpgradeIncrement increments the shop upgrade.
    // It returns true if the upgrade advanced a shop level, and false otherwise.
    public void EarnUpgradeIncrement()
    {
        bool didUpgrade = gameState.EarnUpgradeIncrement();
        PlayerData playerData = gameState.playerData.GetValue();
        if (didUpgrade) {
            shopLevel = shopEncounter.shopData.GetShopLevel(playerData.shopLevel);

            // Record an event for the analytics service.
            var eventData = new ShopUpgradedAnalyticsEvent
            {
                ShopLevel = shopLevel.level
            };
            AnalyticsManager.Instance.RecordEvent(eventData);

            shopViewController.SetShopUpgradePrice(shopLevel.upgradeIncrementCost);
            InstantiateShopVFX(shopUpgradePrefab, shopViewController.GetUpgradeShopButton(), 1f);
            MusicController.Instance.PlaySFX("event:/MX/MX_Shop_Upgrade_Stinger");
            CheckDisableUpgradeButtonV2();
            shopViewController.SetupUpgradeIncrements(shopEncounter.shopData.shopLevels.Count - 1 <= shopLevel.level);
            shopViewController.RebuildUnitManagement(gameState.companions);
        } else {
            shopViewController.ActivateUpgradeIncrement(playerData.shopLevelIncrementsEarned - 1 /* -1 because we just earned an increment */);
        }
        shopViewController.SetMoney(playerData.gold);
    }

    private void CheckDisableUpgradeButtonV2() {
        if (shopEncounter.shopData.shopLevels.Count - 1 <= shopLevel.level) {
            shopViewController.DisableUpgradeButton();
        }
    }

    public void ProcessRerollShopClick() {
        if(DialogueManager.Instance.dialogueInProgress) {
            return;
        }
        int price = GetRerollCost();
        if (gameState.playerData.GetValue().gold >= price) {
            MusicController.Instance.PlaySFX("event:/SFX/SFX_Reroll");
            gameState.playerData.GetValue().gold -= price;
            shopViewController.SetMoney(gameState.playerData.GetValue().gold);
            // shopViewController.Clear();
            InstantiateShopVFX(shopRerollPrefab, shopViewController.GetRerollShopButton(), 1.25f);
            timesRerolledThisShop++;
            price = GetRerollCost();
            shopViewController.SetShopRerollPrice(price);
            rerollShop();
        }
        else
        {
            shopViewController.NotEnoughMoney();
        }
    }

    private int GetRerollCost()
    {
        // Default to increasing the reroll cost by 1 each time.
        var baseIncrease = 0;
        var incIncrease = 1;

        if (ProgressManager.Instance.IsFeatureEnabled(AscensionType.COSTLY_REROLLS))
        {
            baseIncrease = (int) ProgressManager.Instance.GetAscensionSO(AscensionType.COSTLY_REROLLS).
                ascensionModificationValues.GetValueOrDefault("baseCostIncrease", 0f);
            incIncrease = (int) ProgressManager.Instance.GetAscensionSO(AscensionType.COSTLY_REROLLS).
                ascensionModificationValues.GetValueOrDefault("incrementalCostIncrease", 1f);
        }

        return shopEncounter.shopData.rerollShopPrice + baseIncrease + incIncrease * timesRerolledThisShop;
    }

    private void rerollShop()
    {
        shopRefreshEvent.Raise(null);
        List<Companion> allCompanions = new();
        allCompanions.AddRange(gameState.companions.activeCompanions);
        allCompanions.AddRange(gameState.companions.benchedCompanions);
        shopEncounter.Reroll(allCompanions, this.shopLevel);
    }

    public void ProcessCardRemovalClick() {
        removingCard = true;
        // wanna keep this right next to the bools the manager tracks in case we wanna unite state someday
        // "Dalinar... you must UNITE THEM"
        // NonMouseInputManager.Instance.SetUIState(UIState.REMOVING_CARD);
        if (gameState.playerData.GetValue().gold >= shopEncounter.shopData.cardRemovalPrice) {
            shopViewController.StartCardRemoval();
        } else {
            shopViewController.NotEnoughMoney();
        }
    }

    public void saveShopEncounter(ShopEncounter shopEncounter) {
        this.shopEncounter = shopEncounter;
    }

    public void exitShop() {
        if (gameState.companions.activeCompanions.Count == 0) {
            StartCoroutine(shopViewController.ShowGenericNotification("You need at least one active companion!"));
            return;
        }
        gameState.activeEncounter.GetValue().isCompleted = true;
        Debug.Log("gameState active encounter id: " + gameState.activeEncounter.GetValue().id);
        Debug.Log("gameState active encounter complete: " + gameState.activeEncounter.GetValue().isCompleted);
        gameState.LoadNextLocation();
    }

    public ShopEncounter getShopEncounter() {
        return this.shopEncounter;
    }

    public ShopLevel GetShopLevel() {
        return shopLevel;
    }

    public TooltipViewModel GetShopUpgradeTooltip() {
        TooltipViewModel tooltipViewModel = new()
        {
            lines = new List<TooltipLine>()
        };
        List<ShopLevel> shopLevels = shopEncounter.shopData.shopLevels;
        for (int i = 0; i < shopLevels.Count; i++) {
            ShopLevel level = shopLevels[i];
            if (level.upgradeTooltip == null || level.upgradeTooltip.lines == null || level.upgradeTooltip.lines.Count == 0) {
                continue;
            }
            TooltipLine tooltipLine = new(level.upgradeTooltip.lines[0].title, level.upgradeTooltip.lines[0].description);
            if (i < gameState.playerData.GetValue().shopLevel) {
                tooltipLine.title = "Level " + (i + 2) + " Benefits Earned:";
            } else if (i == gameState.playerData.GetValue().shopLevel) {
                tooltipLine.title = "Upgrade To Level " + (i + 2);
            } else {
                tooltipLine.title = "Later Upgrade To Level " + (i + 2);
            }
            tooltipViewModel.lines.Add(tooltipLine);
        }
        return tooltipViewModel;
    }

    // This exists to satisfy the IEncounterBuilder interface.
    // The IEncounterBuilder interface exists to avoid type casting at runtime
    public void BuildEnemyEncounter(EnemyEncounter encounter, UIDocumentGameObjectPlacer placer) {
        Debug.LogError("The shop encounter scene was loaded but the active encounter is an enemy encounter!");
        return;
    }

    // This function is called under the assumption that the companion in question
    // is NOT being swapped with another companion
    public void MoveCompanionToActiveAtIndex(Companion companion, int index) {
        if (gameState.companions.activeCompanions.Contains(companion)) {
            gameState.companions.activeCompanions.Remove(companion);
        }
        gameState.companions.activeCompanions.Add(companion);
    }

    // This function is called under the assumption that the companion in question
    // is NOT being swapped with another companion
    public void MoveCompanionToBenchAtIndex(Companion companion, int index) {
        if (gameState.companions.benchedCompanions.Contains(companion)) {
            gameState.companions.benchedCompanions.Remove(companion);
        }
        gameState.companions.benchedCompanions.Add(companion);
    }

    public void SetCompanionOrdering(List<Companion> activeCompanions, List<Companion> benchCompanions) {
        gameState.companions.activeCompanions = activeCompanions;
        gameState.companions.benchedCompanions = benchCompanions;
    }

    public bool CanMoveCompanionToNewOpenSlot(Companion companion) {
        if (gameState.companions.activeCompanions.Count == 1 && gameState.companions.activeCompanions.Contains(companion)) {
            return false;
        }
        return true;
    }

    public void ShopItemHovered() {
        // MusicController.Instance.PlaySFX("event:/SFX/SFX_UIHover");
    }

    public void SetRemovingCard(bool val) {
        removingCard = val;
    }

    public void SetAutoUpgrade(bool v)
    {
        gameState.autoUpgrade = v;
    }
}
