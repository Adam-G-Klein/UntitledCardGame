using UnityEngine;
using UnityEditor;
using System.Linq;

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
            
            // Set packFrom for common cards
            foreach (CardType card in cardPool.commonCards)
            {
                if (card != null)
                {
                    card.packFrom = pack;
                    EditorUtility.SetDirty(card);
                    Debug.Log($"Set {card.Name} (Common) packFrom to {pack.packName}");
                }
            }
            
            // Set packFrom for uncommon cards
            foreach (CardType card in cardPool.uncommonCards)
            {
                if (card != null)
                {
                    card.packFrom = pack;
                    EditorUtility.SetDirty(card);
                    Debug.Log($"Set {card.Name} (Uncommon) packFrom to {pack.packName}");
                }
            }
            
            // Set packFrom for rare cards
            foreach (CardType card in cardPool.rareCards)
            {
                if (card != null)
                {
                    card.packFrom = pack;
                    EditorUtility.SetDirty(card);
                    Debug.Log($"Set {card.Name} (Rare) packFrom to {pack.packName}");
                }
            }
        }
        
        // Save all changes
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Finished setting pack references!");
    }
}