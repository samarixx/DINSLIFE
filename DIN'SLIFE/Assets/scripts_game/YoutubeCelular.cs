
using UnityEngine;
using UnityEngine.Video;

public class YoutubeCelular : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    public void Reproduzir()
    {
        videoPlayer.Play();
    }

    public void Pausar()
    {
        videoPlayer.Pause();
    }

    public void Parar()
    {
        videoPlayer.Stop();
    }

    public void TrocarVideo(VideoClip novoVideo)
    {
        videoPlayer.Stop();
        videoPlayer.clip = novoVideo;
        videoPlayer.Play();
    }
}
