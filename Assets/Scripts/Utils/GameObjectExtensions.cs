using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions {
    public static void ScaleSelfAndChildren(this GameObject self, float scale) {
        self.transform.localScale *= scale;
        foreach (Transform child in self.transform) {
            child.localScale *= scale;
        }
    }
}