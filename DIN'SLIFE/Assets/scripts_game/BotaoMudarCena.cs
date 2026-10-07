using UnityEngine;
using UnityEngine.SceneManagement; 

public class BotaoMudarCena : MonoBehaviour
{
    [SerializeField] private string nomeDaCenaMapa;

    
    public void IrParaMapa()
    {
        SceneManager.LoadScene(nomeDaCenaMapa);
    }
}