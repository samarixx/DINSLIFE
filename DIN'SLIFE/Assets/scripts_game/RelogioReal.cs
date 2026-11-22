using System;
using System.Globalization;
using TMPro;
using UnityEngine;

public class RelogioReal : MonoBehaviour
{
    public TMP_Text textoHora;
    public TMP_Text textoData;

    private static readonly CultureInfo Brasil = new CultureInfo("pt-BR");
    private DateTime ultimoMinuto;

    private void OnEnable()
    {
        ultimoMinuto = DateTime.MinValue;
        AtualizarRelogio();
    }

    private void Update()
    {
        AtualizarRelogio();
    }

    private void AtualizarRelogio()
    {
        DateTime agora = DateTime.Now;
        DateTime minutoAtual = agora.Date.AddHours(agora.Hour).AddMinutes(agora.Minute);

        if (minutoAtual == ultimoMinuto) return;
        ultimoMinuto = minutoAtual;

        if (textoHora != null)
            textoHora.text = agora.ToString("HH:mm", Brasil);

        if (textoData != null)
            textoData.text = agora.ToString("dd/MM/yyyy", Brasil);
    }
}