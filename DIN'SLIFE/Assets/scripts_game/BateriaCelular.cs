
using UnityEngine;

public class BateriaCelular : MonoBehaviour
{
    public static BateriaCelular instancia;

    [Header("Configurações da bateria")]
    public float bateriaAtual = 100f;
    public float tempoParaDiminuir = 5f;
    public float consumo = 1f;

    private float contador = 0f;

    void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (bateriaAtual <= 0)
            return;

        contador += Time.unscaledDeltaTime;

        while (contador >= tempoParaDiminuir)
        {
            contador -= tempoParaDiminuir;

            bateriaAtual -= consumo;
            bateriaAtual = Mathf.Clamp(
                bateriaAtual, 0f, 100f
            );

            if (bateriaAtual <= 0)
                break;
        }
    }
}
