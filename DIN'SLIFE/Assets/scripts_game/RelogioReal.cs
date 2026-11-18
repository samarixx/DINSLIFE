
using UnityEngine;
using TMPro;
using System;

public class RelogioReal : MonoBehaviour
{
    [Header("Texto do relógio")]
    public TMP_Text textoHora;

    void Update()
    {
        if (textoHora != null)
        {
            textoHora.text = DateTime.Now.ToString("HH:mm");
        }
    }
}
