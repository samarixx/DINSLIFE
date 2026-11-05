using UnityEngine;

public class PlayerNeeds : MonoBehaviour
{
    public static PlayerNeeds Instance { get; private set; }

    [Header("Necessidades")]
    [Range(0, 100)] public float fome = 100;
    [Range(0, 100)] public float energia = 100;
    [Range(0, 100)] public float higiene = 100;
    [Range(0, 100)] public float diversao = 100;

    [Header("Redução por segundo")]
    public float taxaFome = 0.2f;
    public float taxaEnergia = 0.1f;
    public float taxaHigiene = 0.05f;
    public float taxaDiversao = 0.1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        fome = Mathf.Clamp(fome - taxaFome * Time.deltaTime, 0, 100);
        energia = Mathf.Clamp(energia - taxaEnergia * Time.deltaTime, 0, 100);
        higiene = Mathf.Clamp(higiene - taxaHigiene * Time.deltaTime, 0, 100);
        diversao = Mathf.Clamp(diversao - taxaDiversao * Time.deltaTime, 0, 100);
    }

    public void Comer(float valor)
    {
        fome = Mathf.Clamp(fome + valor, 0, 100);
    }

    public void Dormir(float valor)
    {
        energia = Mathf.Clamp(energia + valor, 0, 100);
    }

    public void TomarBanho(float valor)
    {
        higiene = Mathf.Clamp(higiene + valor, 0, 100);
    }

    public void Divertir(float valor)
    {
        diversao = Mathf.Clamp(diversao + valor, 0, 100);
    }
}