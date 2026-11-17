using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.EventSystems;

public class PlayerMovement2D : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float speed = 5f; 

    private Vector2 targetPosition; 
    public bool isMoving = false;
    private Camera mainCamera;
    private Rigidbody2D rb; 

    void Start()
    {
        mainCamera = Camera.main;
        targetPosition = transform.position; 
        
        
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            Debug.LogError("O Player precisa de um Rigidbody2D!");
        }
    }

    void Update()
    {
        // não anda enquanto o computador/celular está aberto ou o mouse está sobre a UI
        if (MercadoManager.PcAberto || CelularUI.Aberto) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

       
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            SetNewDestination();
        }
    }

    
    void FixedUpdate()
    {
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
            
            Vector2 newPos = Vector2.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
            
            
            if (Vector2.Distance(rb.position, targetPosition) < 0.05f)
            {
                isMoving = false;
            }
        }
    }
}