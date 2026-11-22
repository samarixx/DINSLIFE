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

        bool temPlacaMae = BancoDin.Comprada(BancoDin.Peca.PlacaMae);
        bool temMemoria = BancoDin.Comprada(BancoDin.Peca.Memoria);

        if (!temPlacaMae || !temMemoria)
        {
            string falta;
            if (!temPlacaMae && !temMemoria)
                falta = "a placa-mãe e a memória RAM";
            else if (!temPlacaMae)
                falta = "a placa-mãe";
            else
                falta = "a memória RAM";

            MostrarAviso("Para ligar o PC, compre " + falta + " no Papaya.");
            return;
        }

        pcLigado = true;
        AtualizarBotaoPC();
        MostrarAviso("Din está jogando no PC!");
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