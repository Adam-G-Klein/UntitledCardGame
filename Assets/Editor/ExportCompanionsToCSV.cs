using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System;

public class ExportCompanionsToCSV
{
    [MenuItem("Tools/Export Companion Abilities to CSV")]
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
        string path = EditorUtility.SaveFilePanel("Save CSV", "", "exported-companions.csv", "csv");
        if (string.IsNullOrEmpty(path)) return;


        // Find all ScriptableObject assets of type CardTypeSO
        string[] guids = AssetDatabase.FindAssets("t:CompanionTypeSO");
        List<CompanionTypeSO> companionTypes = new List<CompanionTypeSO>();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            CompanionTypeSO obj = AssetDatabase.LoadAssetAtPath<CompanionTypeSO>(assetPath);
            if (!assetPath.StartsWith(relativeFolderPath)) {
                continue;
            }
            if (obj != null)
                companionTypes.Add(obj);
        }
        foreach (CompanionTypeSO cardPool in companionTypes) {
            Debug.Log("Found card pool " + cardPool.name);
        }

        // Build CSV content
        StringBuilder csv = new StringBuilder();

        // Collect all companion content
        List<string> companionContent = new List<string>();
        foreach (CompanionTypeSO companion in companionTypes)
        {
            String csvLine = createCSVLine(companion);
            companionContent.Add(csvLine);
        }

        // Sort the companion content by CompanionName and CompanionLevel
        companionContent = companionContent
            .OrderBy(line => line.Split(',')[0]) // Sort by PackName
            .ThenBy(line => line.Split(',')[2], new RarityComparer()) // Sort by Rarity (common, uncommon, rare)
            .ThenBy(line => line.Split(',')[1]) // Sort by CompanionName
            .ThenBy(line => line.Split(',')[3]) // Then by CompanionLevel
            .ToList();

        // Add the header line and sorted content to the CSV
        csv.Clear();
        csv.AppendLine("CompanionName,Rarity,CompanionLevel,Ability,PackName");
        foreach (string line in companionContent)
        {
            csv.AppendLine(line);
        }

        // Write the CSV file
        File.WriteAllText(path, csv.ToString());

        Debug.Log($"Exported {companionTypes.Count} Companions to {path}");
        EditorUtility.RevealInFinder(path);
    }

    private static string createCSVLine(CompanionTypeSO companion) {
        string[] tempName = companion.name.Split("_");
        string companionName = tempName[^1].Replace(",", "");
        string rarity = companion.rarity.ToString();
        string companionLevel = companion.level.ToString();
        string ability = companion.tooltip.lines[0].description.Replace(",", "");
        string pack = companion.pack.packCardPoolSO != null ? companion.pack.packCardPoolSO.name.Substring(3, companion.pack.packCardPoolSO.name.Length - 11) : "None";
        return $"{pack},{companionName},{rarity},{companionLevel},{ability}";
    }

    // Add a custom comparer for rarity
    private class RarityComparer : IComparer<string>
    {
        private static readonly Dictionary<string, int> rarityOrder = new Dictionary<string, int>
        {
            { "common", 0 },
            { "uncommon", 1 },
            { "rare", 2 }
        };

        public int Compare(string x, string y)
        {
            return rarityOrder[x.ToLower()].CompareTo(rarityOrder[y.ToLower()]);
        }
    }
}
