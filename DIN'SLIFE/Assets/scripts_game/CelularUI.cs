using UnityEngine;
using UnityEngine.InputSystem; // o projeto usa o novo Input System

/// <summary>
/// Controla o celular do jogo: um ícone fixo no canto da tela que, ao ser clicado,
/// abre o painel do celular. Clicar no botão de fechar (ou apertar ESC) fecha.
///
/// Coloque este script no GameObject "Canvas" (ou em qualquer objeto sempre ativo).
/// </summary>
public class CelularUI : MonoBehaviour
{
    // Outros scripts (ClickManager, PlayerMovement2D) podem consultar isso
    // para ignorar cliques/movimento enquanto o celular estiver aberto.
    public static bool Aberto { get; private set; }

    [Header("Referências (arraste no Inspector)")]
    [SerializeField] private GameObject iconeCelular;   // botão pequeno no canto da tela
    [SerializeField] private GameObject painelCelular;  // painel/tela do celular (começa desligado)

    [Header("Posição do ícone (canto inferior direito)")]
    [SerializeField] private Vector2 tamanhoIcone = new Vector2(100f, 100f);
    [SerializeField] private Vector2 margem = new Vector2(30f, 30f);

    private void Start()
    {
        PosicionarIconeNoCantoInferiorDireito();
        Aberto = false;
        painelCelular.SetActive(false);
        iconeCelular.SetActive(true);
    }

    private void PosicionarIconeNoCantoInferiorDireito()
    {
        RectTransform rt = iconeCelular.GetComponent<RectTransform>();
        if (rt == null) return;

        rt.anchorMin = new Vector2(1f, 0f); // canto inferior direito
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.sizeDelta = tamanhoIcone;
        rt.anchoredPosition = new Vector2(-margem.x, margem.y);
    }

    private void Update()
    {
        // ESC fecha o celular
        if (Aberto && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            FecharCelular();
        }
    }

    // Ligue no OnClick() do botão do ícone
    public void AbrirCelular()
    {
        Aberto = true;
        painelCelular.SetActive(true);
        iconeCelular.SetActive(false);
    }

    // Ligue no OnClick() do botão "X" dentro do painel
    public void FecharCelular()
    {
        Aberto = false;
        painelCelular.SetActive(false);
        iconeCelular.SetActive(true);
    }

    private void OnDisable()
    {
        Aberto = false; // evita ficar "preso" como aberto ao trocar de cena
    }
}