using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMovement2D : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float speed = 5f; 

    private Vector2 targetPosition; 
    public bool isMoving = false;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        targetPosition = transform.position; 
    }

    void Update()
    {
        
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            SetNewDestination();
        }

        
        MovePlayer();
    }

    void SetNewDestination()
    {
        
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

       
        targetPosition = new Vector2(worldPoint.x, worldPoint.y);
        isMoving = true;
    }

    void MovePlayer()
    {
        if (isMoving)
        {
            
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            
            if (Vector2.Distance(transform.position, targetPosition) < 0.05f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }
}