using TMPro;
using UnityEngine;

public class AcoesQuarto : MonoBehaviour
{
    [Header("Barras")]
    [SerializeField, Min(0)] private float higienePorBanho = 40f;
    [SerializeField, Min(0)] private float diversaoPorSegundo = 5f;

    [Header("Interface do quarto")]
    [SerializeField] private TMP_Text aviso;
    [SerializeField] private TMP_Text textoBotaoPC;

    private bool pcLigado;

    private void OnEnable()
    {
        pcLigado = false;
        AtualizarBotaoPC();
        MostrarAviso("");
    }

    private void Update()
    {
        if (pcLigado && PlayerNeeds.Instance != null)
        {
            PlayerNeeds.Instance.Divertir(diversaoPorSegundo * Time.deltaTime);
        }
    }

    public void TomarBanho()
    {
        PlayerNeeds din = PlayerNeeds.Instance;
        if (din == null) return;

        din.TomarBanho(higienePorBanho);
        MostrarAviso("Din tomou banho!");
    }

    public void LigarOuDesligarPC()
    {
        if (pcLigado)
        {
            pcLigado = false;
            AtualizarBotaoPC();
            MostrarAviso("Din desligou o PC.");
            return;
        }
    
        string[] pecasObrigatorias =
        {
            "mouse",
            "teclado",
            "estabilizador",
            "cooler",
            "gabinete",
            "ssd",
            "placa-basica",
            "ram-fury-16",
            "ryzen-5900x",
            "rtx-5067"
        };
    
        int pecasCompradas = 0;
    
        foreach (string id in pecasObrigatorias)
        {
            if (InventarioDin.Tem(id))
                pecasCompradas++;
        }
    
        bool temMonitor = InventarioDin.Tem("monitor-usado") ||
                          InventarioDin.Tem("monitor-bom");
    
        if (temMonitor)
            pecasCompradas++;
    
        int totalNecessario = pecasObrigatorias.Length + 1;
    
        if (pecasCompradas < totalNecessario)
        {
            string aviso = $"Din tem {pecasCompradas} de {totalNecessario} peças.";
            if (!temMonitor)
                aviso += " Falta um monitor. Pode ser o usado ou o novo.";
    
            MostrarAviso(aviso);
            return;
        }
    
        pcLigado = true;
        AtualizarBotaoPC();
        MostrarAviso("Din ligou o PC dos sonhos!");
    }

    private void AtualizarBotaoPC()
    {
        if (textoBotaoPC != null)
            textoBotaoPC.text = pcLigado ? "Desligar PC" : "Ligar PC";
    }

    private void MostrarAviso(string mensagem)
    {
        if (aviso != null)
            aviso.text = mensagem;
    }
}