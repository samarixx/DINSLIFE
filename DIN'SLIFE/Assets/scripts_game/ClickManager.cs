using UnityEngine;
using UnityEngine.InputSystem; // Linha obrigatória para usar o novo Input System

public class ClickManager : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
       
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            DetectClick();
        }
    }

    void DetectClick()
    {
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        
        
        Vector2 rayPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        
     
        RaycastHit2D hit = Physics2D.Raycast(rayPosition, Vector2.zero);

      
        if (hit.collider != null)
        {
            IClickable clickable = hit.collider.GetComponent<IClickable>();
            if (clickable != null)
            {
                clickable.OnClick();
            }
        }
    }
}