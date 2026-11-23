using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Inventário do Din: guarda os itens comprados na loja Papaya.
/// Fica salvo no computador (PlayerPrefs), como o resto da economia.
/// Classe estática: não precisa ser colocada em nenhum objeto.
/// </summary>
public static class InventarioDin
{
    const string Chave = "DINSLIFE.inventario.v1";

    [Serializable]
    class Dados
    {
        public List<string> ids = new List<string>();
    }

    static Dados dados;

    static Dados D
    {
        get
        {
            if (dados == null)
            {
                dados = PlayerPrefs.HasKey(Chave)
                    ? JsonUtility.FromJson<Dados>(PlayerPrefs.GetString(Chave))
                    : new Dados();
                if (dados == null) dados = new Dados();
                if (dados.ids == null) dados.ids = new List<string>();
            }
            return dados;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void NovaSessao() { dados = null; }

    /// <summary>Ids dos itens, na ordem em que foram comprados.</summary>
    public static IReadOnlyList<string> Itens => D.ids;
    public static int Quantidade => D.ids.Count;

    public static bool Tem(string id) => D.ids.Contains(id);

    public static bool Adicionar(string id)
    {
        if (string.IsNullOrEmpty(id) || Tem(id)) return false;
        D.ids.Add(id);
        Salvar();
        return true;
    }

    /// <summary>Esvazia o inventário (útil para testes).</summary>
    public static void Zerar()
    {
        dados = new Dados();
        Salvar();
    }

    static void Salvar()
    {
        PlayerPrefs.SetString(Chave, JsonUtility.ToJson(D));
        PlayerPrefs.Save();
    }
}
