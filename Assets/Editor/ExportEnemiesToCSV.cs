using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System.Linq;
using System;
using System.Collections.Specialized;

class EnemyData {
    int poolIdx;
    string poolName;
    bool isElite;
    int encounterIdx;
    string encounterName;
    string enemyName;
    int maxHP;
    string tooltipNegOne;
    string tooltipZero;
    string tooltipOne;

    public EnemyData(
        int poolIdx,
        EnemyEncounterPoolSO pool,
        int encounterIdx,
        string encounterName,
        EnemyTypeSO enemy,
        string tooltipNegOne = "",
        string tooltipZero = "",
        string tooltipOne = ""
    ) {
        this.poolIdx = poolIdx;
        this.poolName = pool.name;
        this.isElite = pool.isElite;
        this.encounterIdx = encounterIdx;
        this.encounterName = encounterName;
        this.enemyName = enemy.displayName;
        this.maxHP = enemy.maxHealth;
        this.tooltipNegOne = tooltipNegOne;
        this.tooltipZero = tooltipZero;
        this.tooltipOne = tooltipOne;
    }

    public static string GetCSVHeaders() {
        List<string> entries = new()
        {
            "PoolID",
            "PoolName",
            "Elite",
            "EncounterID",
            "EncounterName",
            "EnemyName",
            "MaxHP",
            "Desc",
            "TooltipZero",
            "TooltipOne",
        };
        return string.Join(",", entries);
    }

    public string ToCSVLine() {
        List<string> entries = new()
        {
            Convert.ToString(poolIdx),
            poolName,
            isElite ? "Elite" : "Normal",
            Convert.ToString(encounterIdx),
            encounterName,
            enemyName,
            Convert.ToString(maxHP),
            tooltipNegOne,
            tooltipZero,
            tooltipOne
        };
        for (int i = 0; i < entries.Count; i++) {
            entries[i] = entries[i].Replace(",", "");
        }
        return string.Join(",", entries);
    }
}

public class ExportEnemiesToCSV
{
    [MenuItem("Tools/Export Enemies to CSV")]
    public static void ExportToCSV()
    {
        // Select a folder within the project
        string absoluteFolderPath = EditorUtility.OpenFolderPanel("Select Folder to Search for Enemies (e.g. Ghoul Gauntlet)", Application.dataPath, "");
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
        string path = EditorUtility.SaveFilePanel("Save CSV", "", "exported-enemies.csv", "csv");
        if (string.IsNullOrEmpty(path)) return;


        // Find all ScriptableObject assets of type SimpleEnemyPoolMapGeneratorSO
        string[] mapGenGuids = AssetDatabase.FindAssets("t:SimpleEnemyPoolMapGeneratorSO");
        List<SimpleEnemyPoolMapGeneratorSO> mapGenSOs = new List<SimpleEnemyPoolMapGeneratorSO>();

        foreach (string guid in mapGenGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            SimpleEnemyPoolMapGeneratorSO obj = AssetDatabase.LoadAssetAtPath<SimpleEnemyPoolMapGeneratorSO>(assetPath);
            if (!assetPath.StartsWith(relativeFolderPath)) {
                continue;
            }
            if (obj != null)
                mapGenSOs.Add(obj);
        }
        Assert.AreEqual(mapGenSOs.Count, 1, "There should only be AT MOST 1 map generator in the folder");
        SimpleEnemyPoolMapGeneratorSO map = mapGenSOs[0];

        Debug.Log("Found the map generator " + map.name);

        // Build CSV content
        StringBuilder csv = new StringBuilder();

        // CSV Header
        csv.AppendLine(EnemyData.GetCSVHeaders());

        Dictionary<EnemyEncounterPoolSO, bool> visited = new();
        int poolIdx = 0, encounterIdx = 0;
        foreach (EnemyEncounterPoolSO pool in map.enemyEncounterPools) {
            if (visited.ContainsKey(pool)) {
                continue;
            }

            // Let's create a CSV line for each enemy.
            foreach (EnemyEncounterTypeSO enc in pool.enemyEncounterTypes) {
                foreach (EnemyTypeSO enemy in enc.enemies) {
                    string tooltipNegOne = enemy.tooltip.lines.Find(x => x.relatedBehaviorIndex == -1)?.description ?? "";
                    string tooltipZero = enemy.tooltip.lines.Find(x => x.relatedBehaviorIndex == 0)?.description ?? "";
                    string tooltipOne = enemy.tooltip.lines.Find(x => x.relatedBehaviorIndex == 1)?.description ?? "";

                    EnemyData d = new EnemyData(
                        poolIdx,
                        pool,
                        encounterIdx,
                        enc.name,
                        enemy,
                        tooltipNegOne,
                        tooltipZero,
                        tooltipOne
                    );
                    csv.AppendLine(d.ToCSVLine());
                }
                encounterIdx += 1;
            }

            visited[pool] = true;
            poolIdx += 1;
        }

        // Write CSV file
        File.WriteAllText(path, csv.ToString());

        // Debug.Log($"Exported {cardPools.Count} Cards to {path}");
        EditorUtility.RevealInFinder(path);
    }


}
