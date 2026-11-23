using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Trilha sonora do jogo: uma música diferente para cada cena.
///
/// - Coloque em um GameObject vazio na primeira cena (MENU). Ele continua tocando entre as cenas.
/// - Na lista "Trilhas", cada item tem o nome da cena e a música dela.
/// - Ao trocar de cena, a música atual some aos poucos (fade) e a nova entra.
/// - Se a cena nova usa a MESMA música, ela não reinicia.
/// - Ao voltar para uma cena (ex.: fechar o celular e voltar ao quarto), a música
///   continua de onde parou.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class TrilhaSonora : MonoBehaviour
{
    public static TrilhaSonora Instance { get; private set; }

    [System.Serializable]
    public class TrilhaDaCena
    {
        [Tooltip("Nome exato da cena, como aparece em Assets/Scenes (ex.: QUARTO).")]
        public string nomeDaCena;
        public AudioClip musica;
        [Range(0f, 1f)] public float volume = 0.6f;
    }

    [Header("Uma trilha para cada cena")]
    [SerializeField] private List<TrilhaDaCena> trilhas = new List<TrilhaDaCena>();

    [Header("Ajustes")]
    [SerializeField, Range(0f, 1f)] private float volumeGeral = 1f;
    [Tooltip("Segundos para uma música sumir e a outra entrar. 0 = troca seca.")]
    [SerializeField, Min(0f)] private float duracaoDoFade = 1f;
    [Tooltip("Cena sem música na lista (ou com o campo Musica vazio) continua com a música que já estava tocando. Desligado = silêncio.")]
    [SerializeField] private bool cenaSemMusicaMantemAnterior = true;
    [Tooltip("Ao voltar para uma música que já tocou, continua de onde parou.")]
    [SerializeField] private bool retomarDeOndeParou = true;

    private AudioSource fonte;
    private Coroutine troca;
    private AudioClip clipAtual;
    private float volumeDaCena;
    private readonly Dictionary<AudioClip, float> posicoes = new Dictionary<AudioClip, float>();

    // Ao adicionar o componente, já cria uma linha para cada cena do jogo.
    private void Reset()
    {
        trilhas = new List<TrilhaDaCena>
        {
            new TrilhaDaCena { nomeDaCena = "MENU" },
            new TrilhaDaCena { nomeDaCena = "QUARTO" },
            new TrilhaDaCena { nomeDaCena = "MAPA" },
            new TrilhaDaCena { nomeDaCena = "MERCADO" },
            new TrilhaDaCena { nomeDaCena = "TELEFONE" },
        };
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // já existe uma trilha tocando (vinda de outra cena): esta cópia some
            enabled = false;
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        fonte = GetComponent<AudioSource>();
        fonte.loop = true;
        fonte.playOnAwake = false;
        fonte.spatialBlend = 0f; // som 2D
        fonte.volume = 0f;
    }

    private void OnEnable()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded += AoCarregarCena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AoCarregarCena;
    }

    private void Start()
    {
        if (Instance != this) return;
        TocarParaCena(SceneManager.GetActiveScene().name); // a cena em que o jogo começou
    }

    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        if (modo == LoadSceneMode.Single)
            TocarParaCena(cena.name);
    }

    // ------------------------------------------------------------------ troca de música

    private void TocarParaCena(string nomeDaCena)
    {
        TrilhaDaCena trilha = trilhas.Find(t =>
            t != null && string.Equals(t.nomeDaCena, nomeDaCena, System.StringComparison.OrdinalIgnoreCase));

        bool temMusica = trilha != null && trilha.musica != null;
        if (!temMusica && cenaSemMusicaMantemAnterior) return; // segue tocando a música atual

        AudioClip novo = temMusica ? trilha.musica : null; // sem música e sem "manter": silêncio
        float volume = temMusica ? trilha.volume : 0f;

        if (troca != null) StopCoroutine(troca);
        troca = StartCoroutine(Trocar(novo, volume));
    }

    private IEnumerator Trocar(AudioClip novo, float volume)
    {
        volumeDaCena = volume;
        float alvo = volume * volumeGeral;

        // mesma música da cena anterior: não reinicia, só ajusta o volume
        if (novo != null && novo == clipAtual && fonte.isPlaying)
        {
            yield return Fade(fonte.volume, alvo);
            troca = null;
            yield break;
        }

        // a música atual sai
        if (clipAtual != null && fonte.isPlaying)
        {
            yield return Fade(fonte.volume, 0f);
            posicoes[clipAtual] = fonte.time;
            fonte.Stop();
        }

        clipAtual = novo;
        if (novo == null)
        {
            troca = null;
            yield break;
        }

        // a nova música entra
        fonte.clip = novo;
        fonte.volume = 0f;
        fonte.time = retomarDeOndeParou && posicoes.TryGetValue(novo, out float t) && t < novo.length ? t : 0f;
        fonte.Play();
        yield return Fade(0f, alvo);
        troca = null;
    }

    private IEnumerator Fade(float de, float para)
    {
        if (duracaoDoFade <= 0f)
        {
            fonte.volume = para;
            yield break;
        }

        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duracaoDoFade)
        {
            fonte.volume = Mathf.Lerp(de, para, t);
            yield return null;
        }
        fonte.volume = para;
    }

    // ------------------------------------------------------------------ para um menu de opções

    /// <summary>Volume de todas as trilhas (0 a 1). Pode ser ligado a um Slider.</summary>
    public void DefinirVolumeGeral(float valor)
    {
        volumeGeral = Mathf.Clamp01(valor);
        if (troca == null && fonte.isPlaying)
            fonte.volume = volumeDaCena * volumeGeral;
    }

    /// <summary>Liga ou desliga o som da trilha.</summary>
    public void Mutar(bool mudo)
    {
        fonte.mute = mudo;
    }
}
