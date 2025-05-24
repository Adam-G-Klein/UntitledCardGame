using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class DefeatView : MonoBehaviour
{
    private UIDocument doc;
    private UIDocumentScreenspace docRenderer;

    private Material mat;
    private CanvasGroup canvasGroup;

    [SerializeField]
    private float fadeTime = .1f;
    private Button button;


    void OnEnable()
    {
        doc = GetComponent<UIDocument>();
        docRenderer = GetComponent<UIDocumentScreenspace>();
        // Set initial alpha to 0
        mat = GetComponent<RawImage>().material;
        mat.SetFloat("_alpha", 0);

        // Ensure the CanvasGroup alpha is set to 0 and blocks raycasts
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0;

        button = doc.rootVisualElement.Q<Button>();
        button.RegisterOnSelected(() => {
            button.SetEnabled(false);
            SceneManager.LoadScene("EndOfRunProgressScene");
        });
        FocusManager.Instance.RegisterFocusableTarget(button.AsFocusable());
    }

    public void Setup(List<Enemy> enemies) {
        VisualElement container = doc.rootVisualElement.Q(name: "enemiesThatDefeatedYou");
        for (int i = 0; i < enemies.Count; i++) {
            Enemy enemy = enemies[i];
            VisualElement enemyContainer = new VisualElement();
            enemyContainer.AddToClassList("victory-companion-container");
            EnemyTypeSO enemyTypeSO = enemies[i].enemyType;
            EntityView entityView = new EntityView(enemies[i], 0, true);
            //entityView.entityContainer.AddToClassList("compendium-item-container");
            VisualElement portraitContainer = entityView.entityContainer.Q(className: "entity-portrait");
            portraitContainer.style.backgroundImage = new StyleBackground(enemyTypeSO.sprite);
            enemyContainer.Add(entityView.entityContainer);
            
            container.Add(enemyContainer);
        }
    }

    public void Show() {
        // Ensure the initial alpha is set to 0 before starting the fade-in
        canvasGroup.blocksRaycasts = true;
        mat.SetFloat("_alpha", 0);
        canvasGroup.alpha = 0;
        LeanTween.value(gameObject, 0, 1, fadeTime)
            .setOnUpdate((float val) => {
                mat.SetFloat("_alpha", val);
                canvasGroup.alpha = val;
            });
    }


}
