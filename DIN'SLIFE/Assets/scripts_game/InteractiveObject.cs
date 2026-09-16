using UnityEngine;

public class InteractiveObject : MonoBehaviour, IClickable
{
    [SerializeField] private Texture2D hoverCursor;

    private void OnMouseEnter()
    {
        if (hoverCursor != null)
            Cursor.SetCursor(hoverCursor, Vector2.zero, CursorMode.Auto);
    }

    private void OnMouseExit()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void OnClick()
    {
        Debug.Log("Objeto clicado!");
    }
}