
using UnityEngine;
using UnityEngine.UI;

public class InterfaceBateria : MonoBehaviour
{
    public Slider sliderBateria;

    void Start()
    {
        sliderBateria.minValue = 0;
        sliderBateria.maxValue = 100;
    }

    void Update()
    {
        if (BateriaCelular.instancia != null)
        {
            sliderBateria.value =
                BateriaCelular.instancia.bateriaAtual;
        }
    }
}
