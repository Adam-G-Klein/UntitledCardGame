using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MeothraIntentDisplay: MonoBehaviour
{
    [SerializeField] private EnemyInstance enemyInstance;
    [SerializeField] private UIDocument intentUIDoc;
    [SerializeField] GameObject intentAnchor;
    [SerializeField] Vector3 anchorOffset;
    [SerializeField] EnemyIntentsSO enemyIntentsSO;

    private Transform intentTransform;
    private VisualElement intentImage;
    private Label intentLabel;

    public void Setup()
    {
        if (enemyInstance == null) enemyInstance = GetComponent<EnemyInstance>();

        enemyInstance.onIntentDeclared += UpdateIntent;
        enemyInstance.onIntentEnacted += HideIntent;
        intentTransform = intentUIDoc.transform;

        UIDocumentUtils.SetAllPickingMode(intentUIDoc.rootVisualElement, PickingMode.Ignore);
        intentImage = intentUIDoc.rootVisualElement.Q<VisualElement>("intent-image");
        intentLabel = intentUIDoc.rootVisualElement.Q<Label>("intent-value");
        intentUIDoc.rootVisualElement.visible = false;
    }

    private void UpdateIntent() {
        intentUIDoc.rootVisualElement.visible = true;
        intentImage.style.backgroundImage = new StyleBackground(
                enemyIntentsSO.GetIntentImage(enemyInstance.currentIntent.intentType));
        if (enemyInstance.currentIntent.GetDisplayValue() != 0) {
            intentLabel.style.display = DisplayStyle.Flex;
            intentLabel.text = enemyInstance.currentIntent.GetDisplayValue().ToString();
        } else {
            intentLabel.style.display = DisplayStyle.None;
        }
        intentTransform.position = intentAnchor.transform.position + anchorOffset;
    }

    private void HideIntent() {
        intentUIDoc.rootVisualElement.visible = false;
    }
}
