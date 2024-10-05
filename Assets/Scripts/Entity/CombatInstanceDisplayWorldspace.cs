
using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(SpriteRenderer))]
public class CombatInstanceDisplayWorldspace : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    // maintaining a bit of backwards compatibility with the old combat screen 
    private BoxCollider2D boxCollider2D;
    [SerializeField]
    [Header("Added because I threw in the towel on getting the size perfect")]
    private int arbitraryInteriorPaddingPixels = 15;

    [SerializeField]
    [Header("Added because I threw in the towel on getting the positioning perfect")]
    private int arbitraryBottomPlacementPixels = 5;

    void Start(){
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    void Update() {
        // debug print the mouse position on the screen
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Debug.Log("Mouse position world: " + mousePos + " screen: " + Input.mousePosition);
    }

    public void Setup(CombatInstance combatInstance, WorldPositionVisualElement wpve) {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
        this.boxCollider2D = GetComponent<BoxCollider2D>();
        spriteRenderer.sprite = combatInstance.GetSprite();
        boxCollider2D.offset = new Vector2(0, 0);
        SetSize(wpve);
        AlignBottom(wpve);
    }

    private void AlignBottom(WorldPositionVisualElement wpve) {
        VisualElement box = wpve.ve;
        float veBottom = Screen.height - box.worldBound.yMax + arbitraryBottomPlacementPixels;
        print(transform.parent.name + " veBottom: " + veBottom);
        float spriteScreenHeightPixels = (spriteRenderer.sprite.rect.height / 2) * transform.localScale.y;
        print(transform.parent.name + " spriteScreenHeightPixels: " + spriteScreenHeightPixels);
        float spriteScreenYPos = Camera.main.ScreenToWorldPoint(new Vector3(0, veBottom + spriteScreenHeightPixels, 0)).y; // minus because screen space y=0 is at the top
        print(transform.parent.name + " spriteScreenYPos: " + spriteScreenYPos);    
        transform.position = new Vector3(transform.position.x, 
            spriteScreenYPos,
            transform.position.z);
    }

    private void SetSize(WorldPositionVisualElement wpve) {
        VisualElement box = wpve.ve;
        // scale the sprite to be the same screenspace width as the box 
        // then align the bottom with the bottom of the ve box
        // 1 get the pixels per unit of the sprite
        // 2 get the width of the box in pixels
        // 3 get the width of the sprite in pixels
        // 4 get the ratio of the box width to the sprite width
        // 5 scale the transform by that ratio

        //print(transform.parent.name + " sizing based on element " + wpve.ve.name);

        // 1
        float spritePixelsPerUnit = spriteRenderer.sprite.pixelsPerUnit;
        //print(transform.parent.name + " spritePixelsPerUnit: " + spritePixelsPerUnit);
        // 2
        float boxWidthInPixels = box.worldBound.width - arbitraryInteriorPaddingPixels;
        //print(transform.parent.name + " boxWidthInPixels: " + boxWidthInPixels);
        // 3
        float spriteWidthInPixels = spriteRenderer.sprite.rect.width;
        //print(transform.parent.name + " spriteWidthInPixels: " + spriteWidthInPixels);
        // 4
        float scaleRatio = boxWidthInPixels / spriteWidthInPixels;
        //print(transform.parent.name + " scale ratio: " + scaleRatio);
        // 5
        transform.localScale = new Vector3(scaleRatio, scaleRatio, 1);
    }
}