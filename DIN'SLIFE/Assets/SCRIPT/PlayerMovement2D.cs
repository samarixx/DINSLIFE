using UnityEngine;
using UnityEngine.InputSystem; // Importante para usar o Novo Input System

public class PlayerMovement2D : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float speed = 5f; // Velocidade do personagem

    private Vector2 targetPosition; // Posição para onde o player deve ir
    private bool isMoving = false;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        targetPosition = transform.position; // Inicia a posição de destino na posição atual
    }

    void Update()
    {
        // 1. Detecta o clique do mouse no cenário
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            SetNewDestination();
        }

        // 2. Move o personagem até a posição de destino
        MovePlayer();
    }

    void SetNewDestination()
    {
        // Pega a posição do clique na tela e converte para coordenadas do mundo 2D
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        // Define a nova posição mantendo o eixo Z do Player intacto
        targetPosition = new Vector2(worldPoint.x, worldPoint.y);
        isMoving = true;
    }

    void MovePlayer()
    {
        if (isMoving)
        {
            // Move o Player gradualmente até a posição do clique
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            // Quando chegar bem perto do destino, para de se mover
            if (Vector2.Distance(transform.position, targetPosition) < 0.05f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }
}