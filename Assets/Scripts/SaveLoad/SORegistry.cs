using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/ScriptableObject Registry")]
public class SORegistry : ScriptableObject
{
    [SerializeField] private List<IdentifiableSO> registeredAssets;

    private Dictionary<string, IdentifiableSO> guidToAsset;

    public void Initialize()
    {
        guidToAsset = new Dictionary<string, IdentifiableSO>();
        foreach (var asset in registeredAssets)
        {
            if (asset != null && !string.IsNullOrEmpty(asset.GUID))
                guidToAsset[asset.GUID] = asset;
        }
    }

    public T GetAsset<T>(string guid) where T : IdentifiableSO
    {
        if (guidToAsset == null) Initialize();
        if (guidToAsset.TryGetValue(guid, out var asset))
            return asset as T;

        Debug.LogWarning($"No asset found for GUID {guid}");
        return null;
    }

    public void RegisterAssets(List<IdentifiableSO> assets) {
        registeredAssets = assets;
    }
}