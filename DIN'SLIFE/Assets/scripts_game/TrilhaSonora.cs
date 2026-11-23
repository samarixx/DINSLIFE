using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class TrilhaSonora : MonoBehaviour
{
    public static TrilhaSonora Instance { get; private set; }

    [Serializable]
    public class TrilhaDaCena
    {
        public string nomeDaCena;
        public AudioClip musica;
        [Range(0f, 1f)] public float volume = 0.6f;
    }

    [Header("Uma trilha para cada cena")]
    [SerializeField] private List<TrilhaDaCena> trilhas = new List<TrilhaDaCena>();

    [Header("Ajustes")]
    [SerializeField, Range(0f, 1f)] private float volumeGeral = 1f;
    [SerializeField, Min(0f)] private float duracaoDoFade = 1f;
    [SerializeField] private bool cenaSemMusicaMantemAnterior = true;
    [SerializeField] private bool retomarDeOndeParou = true;

    private const string ChaveVolume = "Din.VolumeMenu";
    private const string ChaveMusica = "Din.MusicaAmbientes";

    public static float VolumeMenu { get; private set; }
    public static bool MusicaLigada { get; private set; }

    private readonly Dictionary<AudioClip, float> posicoes =
        new Dictionary<AudioClip, float>();

    private AudioSource fonte;
    private Coroutine troca;
    private AudioClip clipAtual;
    private bool tocandoMenu;
    private float nivelAtual;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        VolumeMenu = PlayerPrefs.GetFloat(ChaveVolume, 1f);
        MusicaLigada = PlayerPrefs.GetInt(ChaveMusica, 1) == 1;

        fonte = GetComponent<AudioSource>();
        fonte.playOnAwake = false;
        fonte.loop = true;
        fonte.spatialBlend = 0f;
        fonte.volume = 0f;
    }

    private void OnEnable()
    {
        if (Instance == this)
            SceneManager.sceneLoaded += AoCarregarCena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AoCarregarCena;
    }

    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        if (modo == LoadSceneMode.Single)
            TocarParaCena(cena.name);
    }

    private void TocarParaCena(string nomeDaCena)
    {
        bool menu = string.Equals(nomeDaCena, "MENU",
            StringComparison.OrdinalIgnoreCase);

        TrilhaDaCena trilha = trilhas.Find(item =>
            item != null && string.Equals(item.nomeDaCena, nomeDaCena,
                StringComparison.OrdinalIgnoreCase));

        bool temMusica = trilha != null && trilha.musica != null;

        // Com Música desligada, entrar num ambiente interrompe a trilha.
        if (!menu && !MusicaLigada)
        {
            CancelarTroca();
            PararEGuardarPosicao();
            return;
        }

        if (!temMusica && cenaSemMusicaMantemAnterior)
            return;

        CancelarTroca();
        AudioClip proximo = temMusica ? trilha.musica : null;
        float volume = temMusica ? trilha.volume : 0f;
        troca = StartCoroutine(Trocar(proximo, volume, menu));
    }

    private void CancelarTroca()
    {
        if (troca != null)
            StopCoroutine(troca);

        troca = null;
    }

    private IEnumerator Trocar(AudioClip proximo, float volume, bool menu)
    {
        // QUARTO e TELEFONE usam "celular". Continua do mesmo ponto.
        if (proximo != null && proximo == clipAtual && fonte.isPlaying)
        {
            tocandoMenu = menu;
            yield return Fade(volume);
            yield break;
        }

        if (fonte.isPlaying)
        {
            yield return Fade(0f);
            PararEGuardarPosicao();
        }

        if (proximo == null)
            yield break;

        clipAtual = proximo;
        tocandoMenu = menu;
        fonte.clip = proximo;
        fonte.time = 0f;

        if (retomarDeOndeParou &&
            posicoes.TryGetValue(proximo, out float tempo) &&
            tempo < proximo.length)
        {
            fonte.time = tempo;
        }

        nivelAtual = 0f;
        AplicarVolume();
        fonte.Play();
        yield return Fade(volume);
    }

    private IEnumerator Fade(float destino)
    {
        float inicio = nivelAtual;
        float tempo = 0f;

        while (tempo < duracaoDoFade)
        {
            nivelAtual = Mathf.Lerp(inicio, destino, tempo / duracaoDoFade);
            AplicarVolume();
            yield return null;
            tempo += Time.unscaledDeltaTime;
        }

        nivelAtual = destino;
        AplicarVolume();
    }

    private void PararEGuardarPosicao()
    {
        if (clipAtual != null && fonte.isPlaying)
            posicoes[clipAtual] = fonte.time;

        fonte.Stop();
        clipAtual = null;
        nivelAtual = 0f;
        AplicarVolume();
    }

    private void AplicarVolume()
    {
        float ajusteDoMenu = tocandoMenu ? VolumeMenu : 1f;
        fonte.volume = nivelAtual * volumeGeral * ajusteDoMenu;
    }

    public void AjustarVolumeMenu(float valor)
    {
        VolumeMenu = Mathf.Clamp01(valor);
        PlayerPrefs.SetFloat(ChaveVolume, VolumeMenu);
        AplicarVolume();
    }

    public void AjustarMusicaAmbientes(bool ligada)
    {
        MusicaLigada = ligada;
        PlayerPrefs.SetInt(ChaveMusica, ligada ? 1 : 0);
        PlayerPrefs.Save();

        string cena = SceneManager.GetActiveScene().name;
        if (!string.Equals(cena, "MENU", StringComparison.OrdinalIgnoreCase))
            TocarParaCena(cena);
    }

    public void DefinirVolumeGeral(float valor)
    {
        volumeGeral = Mathf.Clamp01(valor);
        AplicarVolume();
    }

    public void Mutar(bool mudo)
    {
        fonte.mute = mudo;
    }
}