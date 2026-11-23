using UnityEngine;

public class SairDoJogo : MonoBehaviour
{
    public void Sair()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}