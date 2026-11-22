using UnityEngine;

public class DormirDin : MonoBehaviour
{
    public void DormirEAcordar()
    {
        BancoDin.Dormir();
        Debug.Log("Din acordou. " + BancoDin.Data);
    }
}
