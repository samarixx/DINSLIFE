using UnityEngine;
using UnityEngine.InputSystem; // o projeto usa o novo Input System
using UnityEngine.SceneManagement;

/// <summary>
/// Celular do jogo como CENA.
/// - O ícone no canto inferior direito abre a cena do celular (TELEFONE).
/// - O botão de fechar (ou ESC) dentro do celular volta para a cena do quarto (QUARTO).
///
/// Use o mesmo script nos dois lugares:
///   * no ícone do celular (cena QUARTO etc.): OnClick -> CelularUI.AbrirCelular
///   * no botão "X" da cena TELEFONE:          OnClick -> CelularUI.FecharCelular
/// </summary>
public class CelularUI : MonoBehaviour
{
    private static string nomeCenaAtualDoCelular = "TELEFONE";

    // true enquanto a cena do celular está ativa (usado por ClickManager e PlayerMovement2D)
    public static bool Aberto => SceneManager.GetActiveScene().name == nomeCenaAtualDoCelular;

    [Header("Cenas (nomes exatos, como aparecem em Assets/Scenes)")]
    [SerializeField] private string nomeCenaCelular = "TELEFONE";
    [SerializeField] private string nomeCenaQuarto = "QUARTO";

    [Header("Ícone do celular (só na cena onde ele fica no canto)")]
    [Tooltip("O botão/imagem do ícone. Deixe vazio no script que fica na cena TELEFONE.")]
    [SerializeField] private GameObject iconeCelular;
    [SerializeField] private Vector2 tamanhoIcone = new Vector2(100f, 100f);
    [SerializeField] private Vector2 margem = new Vector2(30f, 30f);

    private void Awake()
    {
        nomeCenaAtualDoCelular = nomeCenaCelular;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += AoCarregarCena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AoCarregarCena;
    }

    private void Start()
    {
        if (iconeCelular != null)
        {
            PosicionarIconeNoCantoInferiorDireito();
            iconeCelular.SetActive(SceneManager.GetActiveScene().name != nomeCenaCelular);
        }
    }

    private void Update()
    {
        // ESC dentro do celular volta para o quarto
        if (Aberto && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            FecharCelular();
        }
    }

    // Ligue no OnClick() do ícone
    public void AbrirCelular()
    {
        SceneManager.LoadScene(nomeCenaCelular);
    }

    // Ligue no OnClick() do botão "X" da cena do celular
    public void FecharCelular()
    {
        SceneManager.LoadScene(nomeCenaQuarto);
    }

    // Se o ícone estiver num Canvas persistente (PersistentUI), ele some dentro do celular
    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        if (iconeCelular != null)
            iconeCelular.SetActive(cena.name != nomeCenaCelular);
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
}