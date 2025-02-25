using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Playables;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(UIDocument))]
public class UIDocumentCard : MonoBehaviour
{

    private Card card;
    private UIDocument doc;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    [SerializeField]
    private Texture2D texture { get; set; }
    public static Vector2Int CARD_REFERENCE_RESOLUTION = new(400, 700);
    public Vector2 CARD_SIZE = new Vector2(1f, 1.75f);

    [Header("Only for dev, one of the worst possible things you could do for GPU performance")]
    public bool renderTextureConstantly = false;
    private bool renderTextureCoroutineIsRunning = false;
    private PlayableCard pCard = null;
    private CardView cardView = null;

    void Start()
    {
        pCard = GetComponent<PlayableCard>();
        if (pCard != null)
        {
            card = pCard.card;
            // without this we won't have a UIDocument in the render texture i guess? weird
            Invoke("LateStart", 0.01f);
            return;
        }
        CardInShop cCard = GetComponent<CardInShop>();
        if (cCard != null)
        {
            card = cCard.cardDisplay.card;
            Invoke("LateStart", 0.01f);
            return;
        }
        Debug.LogError("UIDocumentCard: No card in playableCard component");
    }

    void LateStart()
    {

        boxCollider = GetComponent<BoxCollider2D>();
        doc = GetComponent<UIDocument>();
        doc.panelSettings = CardPanelSettingsPooler.Instance.GetPanelSettings();
        if(doc.panelSettings == null) {
            Debug.LogError("UIDocumentCard: No panel settings available");
            return;
        }
        // TODO: take in card rather than cardtype
        cardView = new CardView(card, pCard.deckFrom.GetCompanionTypeSO(), false);
        doc.rootVisualElement.Add(cardView.cardContainer);
        UIDocumentUtils.SetAllPickingMode(doc.rootVisualElement, PickingMode.Ignore);
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.size = CARD_SIZE;
        boxCollider.size = CARD_SIZE;
        pCard.UpdateCardText(); //we gonna run this shit like a millino times
        spriteRenderer.material.SetTexture("_SecondTex", doc.panelSettings.targetTexture);
    }

    void Update()
    {
    }


    public void Cleanup(Action postCleanupCallback)
    {
        CardPanelSettingsPooler.Instance.ReturnPanelSettings(doc.panelSettings);
        UIDocumentGameObjectPlacer.Instance.removeMapping(gameObject);
        Debug.Log("UIDocumentCard: OnExitScreen");
        postCleanupCallback.Invoke();
    }

    public void UpdateCardText(EffectDocument document)
    {
        /*cardView.UpdateManaCost();
        if (cardView == null) return;
        if (!document.intMap.ContainsKey("rpl_damage")) return;
        List<int> newValues = new();
        newValues.Add(document.intMap["rpl_damage"]);
        if (document.intMap.ContainsKey("rpl_mult") && card.cardType.values.Count() == 2) {
            newValues.Add(document.intMap["rpl_mult"]);
        }
        object[] styledValues = newValues
            .Select((currentValue, index) => {
                string styledValue;
                // Compare with default value at same index
                if (currentValue > card.cardType.values[index])
                {
                    styledValue = $"<color=green><b>{currentValue}</b></color>";
                }
                else if (currentValue < card.cardType.values[index])
                {
                    styledValue = $"<color=red><b>{currentValue}</b></color>";
                }
                else
                {
                    styledValue = $"<b>{currentValue}</b>";  // or whatever your default color is
                }
                return styledValue;
            })
            .Cast<object>()
            .ToArray();
        //object[] valueArray = card.values.Cast<object>().ToArray(); 
        cardView.UpdateCardText(string.Format(card.cardType.Description, styledValues));
        runCoroutine();*/
        cardView.UpdateManaCost();
        if (cardView == null) return;

        List<object> styledValues = new();
        string description = card.cardType.Description;

        // Loop through each default value and check if it exists in document.intMap
        foreach (var defaultValue in card.cardType.defaultValues)
        {
            string key = defaultValue.key;
            if (document.intMap.ContainsKey(key))
            {
                int currentValue = document.intMap[key];
                string styledValue;

                if (currentValue > defaultValue.value)
                {
                    styledValue = $"<color=green><b>{currentValue}</b></color>";
                }
                else if (currentValue < defaultValue.value)
                {
                    styledValue = $"<color=red><b>{currentValue}</b></color>";
                }
                else
                {
                    styledValue = $"<b>{currentValue}</b>";
                }

                description = description.Replace($"{{{defaultValue.key}}}", styledValue);

            }
            else
            {
                // If the value isn't in the map, use the default value unstylized
                description = description.Replace($"{{{defaultValue.key}}}", $"<b>{defaultValue.value}</b>");
            }
        }

        cardView.UpdateCardText(description);

        cardView.SetHighlight(document.boolMap["highlightCard"]);

    }

    void OnExitPlaymode()
    {
        //doc.panelSettings.targetTexture = defaultTexture;
    }
}