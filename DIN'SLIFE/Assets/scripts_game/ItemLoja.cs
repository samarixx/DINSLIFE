using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

/// <summary>Qual peça do PC do quarto uma compra libera (o botão "Ligar PC" em AcoesQuarto).</summary>
public enum LiberaPeca { Nenhuma, PlacaMae, Memoria }

/// <summary>
/// Um produto da loja Papaya (peça de computador).
/// Os preços ficam em reais no Inspector; o desconto é em porcentagem.
/// </summary>
[Serializable]
public class ItemLoja
{
    public string id;
    public string nome;
    public Sprite sprite;
    [Tooltip("Preço de tabela, em reais.")]
    public float precoOriginal = 100f;
    [Range(0, 90)]
    public int descontoPercentual = 0;
    [Tooltip("Peça do PC do quarto que esta compra libera. Para peças comuns, deixe em Nenhuma.")]
    public LiberaPeca libera = LiberaPeca.Nenhuma;

    public string Chave => string.IsNullOrEmpty(id) ? nome : id;

    public int PrecoOriginalCentavos(float fator)
    {
        return Mathf.Max(1, Mathf.RoundToInt(precoOriginal * 100f * fator));
    }

    public int PrecoFinalCentavos(float fator)
    {
        int original = PrecoOriginalCentavos(fator);
        return Mathf.Max(1, Mathf.RoundToInt(original * (100 - descontoPercentual) / 100f));
    }

    // ------------------------------------------------------------------ catálogo padrão

    private class Def
    {
        public string palavra;   // trecho do nome do arquivo da imagem (sem acento, minúsculo)
        public string id;
        public string nome;
        public float preco;      // reais
        public int desconto;     // %
        public LiberaPeca libera;
    }

    // Preços aproximados de mercado no Brasil, com desconto à vista/promoção.
    private static readonly Def[] defs =
    {
        new Def { palavra = "mouse",         id = "mouse",        nome = "Mouse óptico",                    preco = 59.90f,   desconto = 15 },
        new Def { palavra = "teclado",       id = "teclado",      nome = "Teclado mecânico",                preco = 189.90f,  desconto = 20 },
        new Def { palavra = "estabilizador", id = "estabilizador",nome = "Estabilizador 1000VA",            preco = 219.90f,  desconto = 10 },
        new Def { palavra = "cooler",        id = "cooler",       nome = "Cooler duplo para processador",   preco = 229.90f,  desconto = 15 },
        new Def { palavra = "monitor ruim",  id = "monitor-usado",nome = "Monitor antigo (usado)",          preco = 199.90f,  desconto = 10 },
        new Def { palavra = "gabinete",      id = "gabinete",     nome = "Gabinete gamer ATX",              preco = 349.90f,  desconto = 20 },
        new Def { palavra = "monitor bom",   id = "monitor-bom",  nome = "Monitor 24\" Full HD 144Hz",      preco = 899.90f,  desconto = 15 },
        new Def { palavra = "ssd",           id = "ssd",          nome = "SSD NVMe WD Black SN850X 1TB",    preco = 899.90f,  desconto = 20 },
        new Def { palavra = "placa de vid",  id = "placa-basica", nome = "Placa de vídeo básica",           preco = 1199.90f, desconto = 15, libera = LiberaPeca.PlacaMae },
        new Def { palavra = "fury",          id = "ram-fury-16",  nome = "Memória RAM Kingston Fury 16GB",  preco = 1449.90f, desconto = 24, libera = LiberaPeca.Memoria },
        new Def { palavra = "ryzen",         id = "ryzen-5900x",  nome = "Processador Ryzen 9 5900X",       preco = 2699.00f, desconto = 15 },
        new Def { palavra = "rtx",           id = "rtx-5067",     nome = "Placa de vídeo RTX 5067",         preco = 4299.90f, desconto = 15 },
    };

    /// <summary>
    /// Monta o catálogo com as imagens da pasta Assets/media/pc (só funciona no Editor).
    /// Imagens de "colocar em cima da mesa" não entram: elas são para o quarto, não para a loja.
    /// </summary>
    public static List<ItemLoja> CatalogoPadrao()
    {
        List<KeyValuePair<string, Sprite>> sprites = SpritesDaPasta();
        List<ItemLoja> lista = new List<ItemLoja>();
        HashSet<int> usados = new HashSet<int>();

        foreach (Def d in defs)
        {
            int achado = -1;
            for (int i = 0; i < sprites.Count; i++)
            {
                if (usados.Contains(i)) continue;
                if (sprites[i].Key.Contains(d.palavra)) { achado = i; break; }
            }

            // sem imagem: fora da loja (exceto as peças que liberam o PC, para o jogo não travar)
            if (achado < 0 && sprites.Count > 0 && d.libera == LiberaPeca.Nenhuma) continue;
            if (achado >= 0) usados.Add(achado);

            lista.Add(new ItemLoja
            {
                id = d.id,
                nome = d.nome,
                sprite = achado >= 0 ? sprites[achado].Value : null,
                precoOriginal = d.preco,
                descontoPercentual = d.desconto,
                libera = d.libera
            });
        }
        return lista;
    }

    private static List<KeyValuePair<string, Sprite>> SpritesDaPasta()
    {
        List<KeyValuePair<string, Sprite>> lista = new List<KeyValuePair<string, Sprite>>();
#if UNITY_EDITOR
        const string pasta = "Assets/media/pc";
        if (!UnityEditor.AssetDatabase.IsValidFolder(pasta)) return lista;

        foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:Sprite", new[] { pasta }))
        {
            string caminho = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            string nome = Normalizar(System.IO.Path.GetFileNameWithoutExtension(caminho));
            if (nome.Contains("em cima") || nome.Contains("colocar") || nome.Contains("mesa")) continue;

            foreach (UnityEngine.Object o in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(caminho))
            {
                if (o is Sprite sp) { lista.Add(new KeyValuePair<string, Sprite>(nome, sp)); break; }
            }
        }
#endif
        return lista;
    }

    private static string Normalizar(string texto)
    {
        string d = texto.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder();
        foreach (char c in d)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().ToLowerInvariant();
    }
}
