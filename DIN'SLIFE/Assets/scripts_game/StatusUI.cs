using UnityEngine;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour
{
    public Slider barraFome;
    public Slider barraEnergia;
    public Slider barraHigiene;
    public Slider barraDiversao;

    private void Start()
    {
        Slider[] barras = {
            barraFome, barraEnergia,
            barraHigiene, barraDiversao
        };

        foreach (Slider barra in barras)
        {
            barra.minValue = 0;
            barra.maxValue = 100;
            barra.interactable = false;
        }
    }

    private void Update()
    {
        if (PlayerNeeds.Instance == null)
            return;

        PlayerNeeds stats = PlayerNeeds.Instance;

        barraFome.value = stats.fome;
        barraEnergia.value = stats.energia;
        barraHigiene.value = stats.higiene;
        barraDiversao.value = stats.diversao;
    }
}