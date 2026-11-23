using System;
using UnityEngine;
using UnityEngine.UI;

public class ControleDeMusica : MonoBehaviour
{
    public enum Funcao
    {
        PainelDeOpcoes,
        TrilhaDoMenu,
        TrilhaDoAmbiente
    }

    public Funcao funcao;

    [Header("Preencha no painel de opções")]
    public Slider barraVolume;
    public Toggle opcaoMusica;

    [Header("Preencha no objeto que toca a trilha")]
    public AudioSource fonteDaTrilha;

    private const string ChaveVolume = "VolumeDoMenu";
    private const string ChaveMusica = "MusicaQuartoEMercado";

    private static event Action<float> VolumeAlterado;
    private static event Action<bool> MusicaAlterada;

    private void OnEnable()
    {
        float volume = PlayerPrefs.GetFloat(ChaveVolume, 0.5f);
        bool musicaLigada = PlayerPrefs.GetInt(ChaveMusica, 1) == 1;

        switch (funcao)
        {
            case Funcao.PainelDeOpcoes:
                if (barraVolume != null)
                {
                    barraVolume.SetValueWithoutNotify(volume);
                    barraVolume.onValueChanged.AddListener(MudarVolume);
                }

                if (opcaoMusica != null)
                {
                    opcaoMusica.SetIsOnWithoutNotify(musicaLigada);
                    opcaoMusica.onValueChanged.AddListener(MudarMusica);
                }
                break;

            case Funcao.TrilhaDoMenu:
                AplicarVolume(volume);
                ControlarReproducao(true);
                VolumeAlterado += AplicarVolume;
                break;

            case Funcao.TrilhaDoAmbiente:
                ControlarReproducao(musicaLigada);
                MusicaAlterada += ControlarReproducao;
                break;
        }
    }

    private void OnDisable()
    {
        VolumeAlterado -= AplicarVolume;
        MusicaAlterada -= ControlarReproducao;

        if (funcao == Funcao.PainelDeOpcoes)
        {
            if (barraVolume != null)
                barraVolume.onValueChanged.RemoveListener(MudarVolume);

            if (opcaoMusica != null)
                opcaoMusica.onValueChanged.RemoveListener(MudarMusica);

            PlayerPrefs.Save();
        }
    }

    private void MudarVolume(float volume)
    {
        PlayerPrefs.SetFloat(ChaveVolume, volume);
        VolumeAlterado?.Invoke(volume);
    }

    private void MudarMusica(bool ligada)
    {
        PlayerPrefs.SetInt(ChaveMusica, ligada ? 1 : 0);
        PlayerPrefs.Save();
        MusicaAlterada?.Invoke(ligada);
    }

    private void AplicarVolume(float volume)
    {
        if (fonteDaTrilha != null)
            fonteDaTrilha.volume = volume;
    }

    private void ControlarReproducao(bool ligada)
    {
        if (fonteDaTrilha == null)
            return;

        if (!ligada)
            fonteDaTrilha.Stop();
        else if (!fonteDaTrilha.isPlaying)
            fonteDaTrilha.Play();
    }
}