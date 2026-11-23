using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Telas da loja Papaya dentro do celular: a vitrine (uma peça por página, com a imagem,
/// preço de tabela, desconto e botão de comprar) e o inventário (grade com os itens comprados).
/// É criada pelo EconomiaTelefone; não precisa ser colocada em nenhum objeto.
/// </summary>
public class LojaPapayaUI
{
    private const int ItensPorPaginaInventario = 12; // grade de 3 colunas x 4 linhas

    private static readonly Color corFundo = new Color(.09f, .12f, .18f);
    private static readonly Color corBotao = new Color(.15f, .35f, .49f);
    private static readonly Color corSlot = new Color(.16f, .2f, .28f);
    private static readonly Color corOk = new Color(.5f, 1f, .5f);
    private static readonly Color corErro = new Color(1f, .5f, .5f);

    private readonly List<ItemLoja> catalogo;
    private readonly float fator;
    private readonly Action aoMudar;

    private readonly RectTransform telaLoja;
    private readonly RectTransform telaInventario;

    private int pagina;
    private int paginaInventario;

    // vitrine
    private TMP_Text txtLivre, txtNome, txtAntigo, txtPreco, txtMsg, txtPagina, txtBadge, rotuloComprar;
    private Image imgItem;
    private GameObject badge;
    private Button botaoComprar;

    // inventário
    private TMP_Text txtTituloInventario, txtPaginaInventario;
    private readonly Image[] slotImagem = new Image[ItensPorPaginaInventario];
    private readonly TMP_Text[] slotNome = new TMP_Text[ItensPorPaginaInventario];

    public LojaPapayaUI(RectTransform pai, List<ItemLoja> catalogo, float fatorPreco, Action aoMudar)
    {
        this.catalogo = catalogo;
        this.fator = fatorPreco;
        this.aoMudar = aoMudar;

        telaLoja = Painel(pai, "Loja Papaya");
        telaInventario = Painel(pai, "Inventario do Din");
        ConstruirVitrine();
        ConstruirInventario();
        Fechar();
    }

    // ------------------------------------------------------------------ abrir / fechar

    public void Abrir()
    {
        telaInventario.gameObject.SetActive(false);
        telaLoja.gameObject.SetActive(true);
        txtMsg.text = "";
        DesenharVitrine();
    }

    public void Fechar()
    {
        telaLoja.gameObject.SetActive(false);
        telaInventario.gameObject.SetActive(false);
    }

    private void AbrirInventario()
    {
        telaLoja.gameObject.SetActive(false);
        telaInventario.gameObject.SetActive(true);
        paginaInventario = 0;
        DesenharInventario();
    }

    /// <summary>Redesenha a tela que estiver aberta (saldo e compras mudaram).</summary>
    public void Atualizar()
    {
        if (telaLoja.gameObject.activeSelf) DesenharVitrine();
        if (telaInventario.gameObject.activeSelf) DesenharInventario();
    }

    // ------------------------------------------------------------------ vitrine

    private void ConstruirVitrine()
    {
        Botao(telaLoja, new Vector2(.04f, .92f), new Vector2(.34f, .99f), "Voltar", Fechar);
        Botao(telaLoja, new Vector2(.38f, .92f), new Vector2(.96f, .99f), "Inventário", AbrirInventario);

        txtLivre = Texto(telaLoja, new Vector2(.04f, .855f), new Vector2(.96f, .915f), "", 11, TextAlignmentOptions.Center, Color.white);

        RectTransform areaImagem = Area(telaLoja, new Vector2(.2f, .55f), new Vector2(.8f, .85f));
        imgItem = areaImagem.gameObject.AddComponent<Image>();
        imgItem.preserveAspect = true;
        imgItem.raycastTarget = false;

        RectTransform areaBadge = Area(telaLoja, new Vector2(.68f, .79f), new Vector2(.96f, .87f));
        areaBadge.gameObject.AddComponent<Image>().color = new Color(.8f, .15f, .15f);
        badge = areaBadge.gameObject;
        txtBadge = Texto(areaBadge, Vector2.zero, Vector2.one, "", 12, TextAlignmentOptions.Center, Color.white);

        txtNome = Texto(telaLoja, new Vector2(.04f, .47f), new Vector2(.96f, .55f), "", 15, TextAlignmentOptions.Center, Color.white);
        txtAntigo = Texto(telaLoja, new Vector2(.04f, .41f), new Vector2(.96f, .47f), "", 12, TextAlignmentOptions.Center, new Color(.7f, .7f, .7f));
        txtPreco = Texto(telaLoja, new Vector2(.04f, .33f), new Vector2(.96f, .41f), "", 20, TextAlignmentOptions.Center, corOk);
        txtMsg = Texto(telaLoja, new Vector2(.04f, .245f), new Vector2(.96f, .33f), "", 11, TextAlignmentOptions.Center, Color.white);

        botaoComprar = Botao(telaLoja, new Vector2(.06f, .15f), new Vector2(.94f, .245f), "Comprar", Comprar);
        rotuloComprar = botaoComprar.GetComponentInChildren<TMP_Text>(true);

        Botao(telaLoja, new Vector2(.06f, .03f), new Vector2(.26f, .13f), "<", () => Folhear(-1));
        txtPagina = Texto(telaLoja, new Vector2(.28f, .03f), new Vector2(.72f, .13f), "", 13, TextAlignmentOptions.Center, Color.white);
        Botao(telaLoja, new Vector2(.74f, .03f), new Vector2(.94f, .13f), ">", () => Folhear(1));
    }

    private void Folhear(int passo)
    {
        if (catalogo.Count == 0) return;
        pagina = (pagina + passo + catalogo.Count) % catalogo.Count;
        txtMsg.text = "";
        DesenharVitrine();
    }

    private void DesenharVitrine()
    {
        txtLivre.text = "Livre para peças: " + BancoDin.Reais(BancoDin.Livre);

        if (catalogo.Count == 0)
        {
            txtNome.text = "Loja sem produtos";
            txtAntigo.text = txtPreco.text = txtPagina.text = "";
            imgItem.enabled = false;
            badge.SetActive(false);
            botaoComprar.interactable = false;
            return;
        }

        pagina = Mathf.Clamp(pagina, 0, catalogo.Count - 1);
        ItemLoja item = catalogo[pagina];

        imgItem.sprite = item.sprite;
        imgItem.enabled = item.sprite != null;
        txtNome.text = item.nome;

        bool temDesconto = item.descontoPercentual > 0;
        txtAntigo.text = temDesconto ? "<s>" + BancoDin.Reais(item.PrecoOriginalCentavos(fator)) + "</s>" : "";
        badge.SetActive(temDesconto);
        txtBadge.text = "-" + item.descontoPercentual + "%";
        txtPreco.text = BancoDin.Reais(item.PrecoFinalCentavos(fator));
        txtPagina.text = (pagina + 1) + "/" + catalogo.Count;

        bool jaTem = InventarioDin.Tem(item.Chave);
        rotuloComprar.text = jaTem ? "No inventário" : "Comprar";
        botaoComprar.interactable = !jaTem;
    }

    private void Comprar()
    {
        if (catalogo.Count == 0) return;
        ItemLoja item = catalogo[pagina];

        if (InventarioDin.Tem(item.Chave))
        {
            txtMsg.text = "Esse item já está no inventário.";
            txtMsg.color = corErro;
            return;
        }

        BancoDin.Peca? libera = null;
        if (item.libera == LiberaPeca.PlacaMae) libera = BancoDin.Peca.PlacaMae;
        else if (item.libera == LiberaPeca.Memoria) libera = BancoDin.Peca.Memoria;

        bool ok = BancoDin.ComprarItem(item.nome, item.PrecoFinalCentavos(fator), libera, out string mensagem);
        if (ok) InventarioDin.Adicionar(item.Chave);

        txtMsg.text = mensagem;
        txtMsg.color = ok ? corOk : corErro;
        DesenharVitrine();
        aoMudar?.Invoke();
    }

    // ------------------------------------------------------------------ inventário

    private void ConstruirInventario()
    {
        txtTituloInventario = Texto(telaInventario, new Vector2(.04f, .91f), new Vector2(.96f, .99f), "", 15, TextAlignmentOptions.Center, Color.white);

        const float colunas = 3f, linhas = 4f;
        const float esquerda = .04f, largura = .92f, topo = .86f, altura = .69f;
        for (int i = 0; i < ItensPorPaginaInventario; i++)
        {
            int col = i % 3, lin = i / 3;
            float x0 = esquerda + col * largura / colunas;
            float x1 = x0 + largura / colunas;
            float yTopo = topo - lin * altura / linhas;
            float yBase = yTopo - altura / linhas;

            RectTransform slot = Area(telaInventario, new Vector2(x0, yBase), new Vector2(x1, yTopo));
            slot.offsetMin = new Vector2(2, 2);
            slot.offsetMax = new Vector2(-2, -2);
            slot.gameObject.AddComponent<Image>().color = corSlot;

            RectTransform rtImg = Area(slot, new Vector2(.08f, .34f), new Vector2(.92f, .96f));
            slotImagem[i] = rtImg.gameObject.AddComponent<Image>();
            slotImagem[i].preserveAspect = true;
            slotImagem[i].raycastTarget = false;

            slotNome[i] = Texto(slot, new Vector2(0f, 0f), new Vector2(1f, .34f), "", 8, TextAlignmentOptions.Center, Color.white);
        }

        Botao(telaInventario, new Vector2(.06f, .09f), new Vector2(.26f, .16f), "<", () => FolhearInventario(-1));
        txtPaginaInventario = Texto(telaInventario, new Vector2(.28f, .09f), new Vector2(.72f, .16f), "", 12, TextAlignmentOptions.Center, Color.white);
        Botao(telaInventario, new Vector2(.74f, .09f), new Vector2(.94f, .16f), ">", () => FolhearInventario(1));
        Botao(telaInventario, new Vector2(.06f, .01f), new Vector2(.94f, .08f), "Voltar à loja", Abrir);
    }

    private List<ItemLoja> ItensComprados()
    {
        List<ItemLoja> lista = new List<ItemLoja>();
        foreach (string id in InventarioDin.Itens)
        {
            ItemLoja item = catalogo.Find(x => x.Chave == id);
            if (item != null) lista.Add(item);
        }
        return lista;
    }

    private void FolhearInventario(int passo)
    {
        int paginas = Mathf.Max(1, Mathf.CeilToInt(ItensComprados().Count / (float)ItensPorPaginaInventario));
        paginaInventario = (paginaInventario + passo + paginas) % paginas;
        DesenharInventario();
    }

    private void DesenharInventario()
    {
        List<ItemLoja> comprados = ItensComprados();
        int paginas = Mathf.Max(1, Mathf.CeilToInt(comprados.Count / (float)ItensPorPaginaInventario));
        paginaInventario = Mathf.Clamp(paginaInventario, 0, paginas - 1);

        txtTituloInventario.text = "<b>Inventário</b> (" + comprados.Count + "/" + catalogo.Count + ")";
        txtPaginaInventario.text = (paginaInventario + 1) + "/" + paginas;

        for (int i = 0; i < ItensPorPaginaInventario; i++)
        {
            int indice = paginaInventario * ItensPorPaginaInventario + i;
            if (indice < comprados.Count)
            {
                ItemLoja item = comprados[indice];
                slotImagem[i].sprite = item.sprite;
                slotImagem[i].enabled = item.sprite != null;
                slotNome[i].text = item.nome;
            }
            else
            {
                slotImagem[i].sprite = null;
                slotImagem[i].enabled = false;
                slotNome[i].text = "";
            }
        }
    }

    // ------------------------------------------------------------------ construção da interface

    private static RectTransform Area(Transform pai, Vector2 min, Vector2 max)
    {
        RectTransform rt = (RectTransform)new GameObject("Area", typeof(RectTransform)).transform;
        rt.SetParent(pai, false);
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    private static RectTransform Painel(Transform pai, string nome)
    {
        RectTransform rt = Area(pai, Vector2.zero, Vector2.one);
        rt.gameObject.name = nome;
        rt.gameObject.AddComponent<Image>().color = corFundo;
        return rt;
    }

    private static TMP_Text Texto(Transform pai, Vector2 min, Vector2 max, string conteudo,
        float tamanhoMaximo, TextAlignmentOptions alinhamento, Color cor)
    {
        RectTransform rt = Area(pai, min, max);
        TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = conteudo;
        t.enableAutoSizing = true;
        t.fontSizeMin = 5;
        t.fontSizeMax = tamanhoMaximo;
        t.color = cor;
        t.alignment = alinhamento;
        t.raycastTarget = false;
        return t;
    }

    private static Button Botao(Transform pai, Vector2 min, Vector2 max, string rotulo, UnityAction acao)
    {
        RectTransform rt = Area(pai, min, max);
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = corBotao;
        Button b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        b.onClick.AddListener(acao);
        Texto(rt, new Vector2(.03f, 0f), new Vector2(.97f, 1f), rotulo, 14, TextAlignmentOptions.Center, Color.white);
        return b;
    }
}
