using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class CountryHandler : MonoBehaviour
{
    private SpriteRenderer sprite;
    private Color oldColor;
    private Color hoverColor;

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    void OnMouseEnter()
    {
        // Guardar el color actual en el momento del hover
        oldColor = sprite.color;
        hoverColor = Color.Lerp(oldColor, Color.white, 0.3f);
        sprite.color = hoverColor;
    }

    void OnMouseExit()
    {
        sprite.color = oldColor;
    }
}