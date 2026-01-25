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
    public static Vector2Int CARD_REFERENCE_RESOLUTION = new(543, 832);
    public Vector2 CARD_SIZE = new Vector2(1f, 1.75f);
    public Vector2 HITBOX_SIZE = new Vector2(1f, 2.4f); // this helps if the cards are along the bottom of the screen to prevent constant hovering/unhovering

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
        boxCollider.size = HITBOX_SIZE;
        // pCard.UpdateCardText(); //we gonna run this shit like a millino times
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
        if (cardView == null) return;

        // string description = card.cardType.GetDescriptionWithUpdatedValues(document.intMap);
        // cardView.UpdateCardText(description);
        cardView.UpdateManaCost();

        if (card.cardType.HasIconDescription())
        {
            List<DescriptionToken> iconTokens = card.cardType.GetIconDescriptionTokensWithStylizedValues(document.intMap);
            cardView.UpdateCardIconDescription(iconTokens);
        }
        else
        {
            string modText = card.cardType.GetDescriptionWithUpdatedValues(document.intMap);
            cardView.UpdateCardText(modText);
        }

        cardView.SetHighlight(document.boolMap.ContainsKey("highlightCard") && document.boolMap["highlightCard"]);
    }

    void OnExitPlaymode()
    {
        //doc.panelSettings.targetTexture = defaultTexture;
    }

    public Texture GetTexture() {
        return doc.panelSettings.targetTexture;
    }
}