using UnityEngine;
using UnityEngine.UI;

public class OpcoesDeAudioDin : MonoBehaviour
{
    [SerializeField] private Slider volume;
    [SerializeField] private Toggle musica;

    private void OnEnable()
    {
        if (volume != null)
            volume.onValueChanged.AddListener(MudarVolume);

        if (musica != null)
            musica.onValueChanged.AddListener(MudarMusica);

        AtualizarPainel();
    }

    private void Start()
    {
        AtualizarPainel();
    }

    private void AtualizarPainel()
    {
        if (TrilhaSonora.Instance == null)
            return;

        if (volume != null)
            volume.SetValueWithoutNotify(TrilhaSonora.VolumeMenu);

        if (musica != null)
            musica.SetIsOnWithoutNotify(TrilhaSonora.MusicaLigada);
    }

    private void MudarVolume(float valor)
    {
        if (TrilhaSonora.Instance != null)
            TrilhaSonora.Instance.AjustarVolumeMenu(valor);
    }

    private void MudarMusica(bool ligada)
    {
        if (TrilhaSonora.Instance != null)
            TrilhaSonora.Instance.AjustarMusicaAmbientes(ligada);
    }

    private void OnDisable()
    {
        if (volume != null)
            volume.onValueChanged.RemoveListener(MudarVolume);

        if (musica != null)
            musica.onValueChanged.RemoveListener(MudarMusica);

        PlayerPrefs.Save();
    }
}