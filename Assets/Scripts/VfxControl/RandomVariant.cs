using UnityEngine;

/*
    VFX: keeps one of this effect's "Variant ..." children at random and switches the others off before anything plays,
    so a hit that's seen over and over (the enemy's basic hit) doesn't look stamped from one die. Purely visual.
*/
[DefaultExecutionOrder(-1000)]
public class RandomVariant : MonoBehaviour
{
    private void Awake()
    {
        int count = 0;
        foreach (Transform child in transform) if (child.name.StartsWith("Variant")) count++;
        if (count == 0) return;
        int pick = Random.Range(0, count), i = 0;
        foreach (Transform child in transform)
        {
            if (!child.name.StartsWith("Variant")) continue;
            child.gameObject.SetActive(i == pick);
            i++;
        }
    }
}
