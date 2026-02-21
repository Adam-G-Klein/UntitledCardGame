using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private MeothraAnimationController meothraAnimationController;

    void Update()
    {
        if (intentUIDoc != null && 
        intentUIDoc.rootVisualElement != null &&
        intentUIDoc.rootVisualElement.visible) {
            intentTransform.position = intentAnchor.transform.position + anchorOffset;
        }
    }

    public void Setup()
    {
        if (enemyInstance == null) enemyInstance = GetComponent<EnemyInstance>();

        enemyInstance.onIntentDeclared = UpdateIntent();
        enemyInstance.onIntentEnacted += HideIntent;
        intentTransform = intentUIDoc.transform;

        meothraAnimationController = GetComponentInChildren<MeothraAnimationController>();

        UIDocumentUtils.SetAllPickingMode(intentUIDoc.rootVisualElement, PickingMode.Ignore);
        intentImage = intentUIDoc.rootVisualElement.Q<VisualElement>("intent-image");
        intentLabel = intentUIDoc.rootVisualElement.Q<Label>("intent-value");
        intentUIDoc.rootVisualElement.visible = false;
    }

    private IEnumerable UpdateIntent() {
        List<Vector3> targets = enemyInstance.currentIntent.targets.Select(t => t.transform.position).ToList();
        if(targets.Count == 1)
        {
            yield return meothraAnimationController.DisplaySingleTarget(
                targets[0]
            );
        } else
        {
            // don't yield here because it loops
            StartCoroutine(meothraAnimationController.DisplayMultipleTargets(targets));
        }
        intentUIDoc.rootVisualElement.visible = true;
        intentImage.style.backgroundImage = new StyleBackground(
                enemyIntentsSO.GetIntentImage(enemyInstance.currentIntent.intentType));
        if (enemyInstance.currentIntent.GetDisplayValue() != 0) {
            intentLabel.style.display = DisplayStyle.Flex;
            intentLabel.text = enemyInstance.currentIntent.GetDisplayValue().ToString();
        } else {
            intentLabel.style.display = DisplayStyle.None;
        }
        yield return null;
    }

    public void HideIntent()
    {
        intentUIDoc.rootVisualElement.visible = false;
    }

    public void ShowIntent() {
        intentUIDoc.rootVisualElement.visible = true;
    }
}
