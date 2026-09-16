using UnityEngine;
using UnityEngine.SceneManagement;

public class Carregar_Cena : MonoBehaviour
{
    public string nomeDaCena;

    public void carregar()
    {
        SceneManager.LoadScene(nomeDaCena);
    }
}
