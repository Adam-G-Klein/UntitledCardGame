using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class ExportCardsToCSV
{
    [MenuItem("Tools/Export Cards in Card Pools to CSV")]
    public static void ExportToCSV()
    {
        // Select a folder within the project
        string absoluteFolderPath = EditorUtility.OpenFolderPanel("Select Folder to Search for Card Pools", Application.dataPath, "");
        if (string.IsNullOrEmpty(absoluteFolderPath))
            return;

        // Convert absolute path to relative project path
        string projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6); // remove "Assets/"
        if (!absoluteFolderPath.StartsWith(projectPath))
        {
            Debug.LogError("Selected folder is outside the project.");
            return;
        }
        string relativeFolderPath = absoluteFolderPath.Substring(projectPath.Length);
        Debug.Log("Searching in folder " + relativeFolderPath);

        // Choose save path
        string path = EditorUtility.SaveFilePanel("Save CSV", "", "exported-cards.csv", "csv");
        if (string.IsNullOrEmpty(path)) return;


        // Find all ScriptableObject assets of type CardTypeSO
        string[] guids = AssetDatabase.FindAssets("t:CardPoolSO");
        List<CardPoolSO> cardPools = new List<CardPoolSO>();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            CardPoolSO obj = AssetDatabase.LoadAssetAtPath<CardPoolSO>(assetPath);
            if (!assetPath.StartsWith(relativeFolderPath)) {
                continue;
            }
            if (obj != null)
                cardPools.Add(obj);
        }
        foreach (CardPoolSO cardPool in cardPools) {
            Debug.Log("Found card pool " + cardPool.name);
        }

        // Build CSV content
        StringBuilder csv = new StringBuilder();

        // CSV Header
        csv.AppendLine("CardName,PoolName,Text,Rarity,CardCategory,ManaCost");

        // CSV Data Rows
        foreach (CardPoolSO pool in cardPools)
        {
            // string assetPath = AssetDatabase.GetAssetPath(obj);
            // string displayName = obj.name.Replace(",", "");  // Remove commas for safety
            // string value = obj.name.ToString();

            foreach (CardType common in pool.commonCards) {
                csv.AppendLine(createCSVLine(common, pool, Card.CardRarity.COMMON));
            }
            foreach (CardType uncommon in pool.uncommonCards) {
                csv.AppendLine(createCSVLine(uncommon, pool, Card.CardRarity.UNCOMMON));
            }
            foreach (CardType rare in pool.rareCards) {
                csv.AppendLine(createCSVLine(rare, pool, Card.CardRarity.RARE));
            }
        }

        // // Write CSV file
        File.WriteAllText(path, csv.ToString());

        Debug.Log($"Exported {cardPools.Count} Cards to {path}");
        EditorUtility.RevealInFinder(path);
    }

    private static string createCSVLine(CardType card, CardPoolSO pool, Card.CardRarity rarity) {
        // csv.AppendLine("CardName,PoolName,Text,Rarity,CardCategory");
        string cardName = card.name.Replace(",", "");
        string poolName = pool.name.Replace(",", "");
        string text = card.GetDescription().Replace(",", "");
        string rarityStr = rarity.ToString();
        string cardCategory = card.cardCategory.ToString();
        string manaCost = card.Cost.ToString();
        return $"{cardName},{poolName},{text},{rarityStr},{cardCategory},{manaCost}";
    }
}
