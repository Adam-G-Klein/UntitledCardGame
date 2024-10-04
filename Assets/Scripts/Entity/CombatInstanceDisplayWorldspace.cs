
using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(SpriteRenderer))]
public class CombatInstanceDisplayWorldspace : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    // maintaining a bit of backwards compatibility with the old combat screen 
    private BoxCollider2D boxCollider2D;
    public float temp_entity_size_mult = 0.3f;


    void Start(){
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    void Update() {
        // debug print the mouse position on the screen
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Debug.Log("Mouse position: " + mousePos);
    }

    public void Setup(CombatInstance combatInstance, WorldPositionVisualElement wpve) {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
        this.boxCollider2D = GetComponent<BoxCollider2D>();
        spriteRenderer.sprite = combatInstance.GetSprite();
        boxCollider2D.offset = new Vector2(0, 0);
        SetSize(wpve);
    }

    private void SetSize(WorldPositionVisualElement wpve) {
        VisualElement box = wpve.ve;
        // set the sprite to be the same screenspace size as the box 
        // 1 get the worldspace position of the left side of the box
        // 2 get the worldspace position of the right side of the box
        // 3 set the x size of the sprite to be the difference between the two
        // 4 get the worldspace position of the top of the box
        // 5 get the worldspace position of the bottom of the box
        // 6 set the y size of the sprite to be the difference between the two

        // 1
        Vector3 left = Camera.main.ScreenToWorldPoint(new Vector3(box.worldBound.xMin, box.worldBound.center.y, 0));
        print(transform.parent.name + " left: " + left);
        // 2
        Vector3 right = Camera.main.ScreenToWorldPoint(new Vector3(box.worldBound.xMax, box.worldBound.center.y, 0));
        print(transform.parent.name + " right: " + right);
        // 3
        float width = right.x - left.x;
        // 4
        Vector3 top = Camera.main.ScreenToWorldPoint(new Vector3(box.worldBound.center.x, box.worldBound.yMax, 0));
        print(transform.parent.name + " top: " + top);
        // 5
        Vector3 bottom = Camera.main.ScreenToWorldPoint(new Vector3(box.worldBound.center.x, box.worldBound.yMin, 0));
        print(transform.parent.name + " bottom: " + bottom);
        // 6
        float height = top.y - bottom.y;
        spriteRenderer.size = new Vector2(width, height) * temp_entity_size_mult;
        print(transform.parent.name + " width: " + width + " height: " + height);
        boxCollider2D.size = new Vector2(width, height) * temp_entity_size_mult;

    }
}