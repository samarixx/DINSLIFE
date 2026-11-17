using UnityEngine;

/// <summary>
/// Coloque este script no objeto do computador na cena MERCADO.
/// O objeto precisa ter um Collider2D (o ClickManager detecta o clique nele).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ComputadorMercado : MonoBehaviour, IClickable
{
    public void OnClick()
    {
        if (MercadoManager.Instance != null)
            MercadoManager.Instance.AbrirComputador();
    }
}
