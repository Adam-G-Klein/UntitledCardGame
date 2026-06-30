using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public class SetPackReferencesTool : EditorWindow
{
    [MenuItem("Tools/Set Pack References")]
    public static void ShowWindow()
    {
        GetWindow<SetPackReferencesTool>("Set Pack References");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Set Pack References"))
        {
            SetPackReferences();
        }
    }

    void SetPackReferences()
    {
        // Find all PackSO assets
        string[] packGuids = AssetDatabase.FindAssets("t:PackSO");
        Debug.LogError(packGuids.Count());

        foreach (string packGuid in packGuids)
        {
            string packPath = AssetDatabase.GUIDToAssetPath(packGuid);
            PackSO pack = AssetDatabase.LoadAssetAtPath<PackSO>(packPath);

            if (pack.packCardPoolSO == null)
            {
                Debug.LogWarning($"Pack {pack.packName} has no packCardPoolSO assigned");
                continue;
            }

            CardPoolSO cardPool = pack.packCardPoolSO;

            SetPackFromForCardPool(cardPool, pack);

            // Set packFrom for companion cards
            List<CompanionTypeSO> allCompanions = new List<CompanionTypeSO>();
            allCompanions.AddRange(pack.companionPoolSO.commonCompanions);
            allCompanions.AddRange(pack.companionPoolSO.uncommonCompanions);
            allCompanions.AddRange(pack.companionPoolSO.rareCompanions);
            foreach (CompanionTypeSO companion in allCompanions)
            {
                if (companion != null)
                {
                    companion.pack = pack;
                    EditorUtility.SetDirty(companion);
                    Debug.Log($"Set {companion.name} packFrom to {pack.packName}");
                }
                SetPackFromForCardPool(companion.cardPool, pack);
                SetPackFromForStarterDeck(companion.startingDeck, pack);
            }
        }

        // Save all changes
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Finished setting pack references!");
    }

    private void SetPackFromForStarterDeck(StartingDeck starterDeck, PackSO pack)
    {
        foreach (CardType card in starterDeck.cards)
        {
            if (card != null)
            {
                card.packFrom = pack;
                EditorUtility.SetDirty(card);
                Debug.Log($"Set {card.Name} (Starter Deck) packFrom to {pack.packName}");
            }
        }
    }

    private void SetPackFromForCardPool(CardPoolSO cardPool, PackSO pack)
    {
        foreach (CardType card in cardPool.commonCards)
        {
            if (card != null)
            {
                card.packFrom = pack;
                EditorUtility.SetDirty(card);
                Debug.Log($"Set {card.Name} (Common) packFrom to {pack.packName}");
            }
        }

        foreach (CardType card in cardPool.uncommonCards)
        {
            if (card != null)
            {
                card.packFrom = pack;
                EditorUtility.SetDirty(card);
                Debug.Log($"Set {card.Name} (Uncommon) packFrom to {pack.packName}");
            }
        }

        foreach (CardType card in cardPool.rareCards)
        {
            if (card != null)
            {
                card.packFrom = pack;
                EditorUtility.SetDirty(card);
                Debug.Log($"Set {card.Name} (Rare) packFrom to {pack.packName}");
            }
        }
        foreach (CardType card in cardPool.unlockableCommonCards)
        {
            if (card != null)
            {
                card.packFrom = pack;
                EditorUtility.SetDirty(card);
                Debug.Log($"Set {card.Name} (Unlockable Common) packFrom to {pack.packName}");
            }
        }
        foreach (CardType card in cardPool.unlockableUncommonCards)
        {
            if (card != null)
            {
                card.packFrom = pack;
                EditorUtility.SetDirty(card);
                Debug.Log($"Set {card.Name} (Unlockable Uncommon) packFrom to {pack.packName}");
            }
        }
        foreach (CardType card in cardPool.unlockableRareCards)
        {
            if (card != null)
            {
                card.packFrom = pack;
                EditorUtility.SetDirty(card);
                Debug.Log($"Set {card.Name} (Unlockable Rare) packFrom to {pack.packName}");
            }
        }
    }
}