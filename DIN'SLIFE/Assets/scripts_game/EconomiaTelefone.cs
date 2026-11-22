using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EconomiaTelefone : MonoBehaviour
{
    [SerializeField] RectTransform inicio, banco, papaya;
    [SerializeField] RectTransform iconeBanco, iconePapaya, iconeComida, carrinho;
    RectTransform telaBanco, telaLoja, telaComida;
    TMP_Text infoBanco, infoLoja, infoComida, msgBanco, msgLoja, msgComida;
    readonly Button[] contas = new Button[3];
    Button placa, memoria;
    readonly string[] nomes = { "Aluguel", "Água", "Energia" };

    void Reset() { LocalizarCampos(); }

    void LocalizarCampos()
    {
        if (!inicio) inicio = transform.Find("TelefoneLigado") as RectTransform;
        if (!banco) banco = transform.Find("INTERFACEdinamao") as RectTransform;
        if (!papaya) papaya = transform.Find("PAPAYA") as RectTransform;
        if (inicio)
        {
            if (!iconeBanco) iconeBanco = inicio.Find("DINNAMAO") as RectTransform;
            if (!iconePapaya) iconePapaya = inicio.Find("Papaya") as RectTransform;
            if (!iconeComida) iconeComida = inicio.Find("YouFood") as RectTransform;
        }
        if (papaya && !carrinho) carrinho = papaya.Find("Image (1)") as RectTransform;
    }

    void Start()
    {
        LocalizarCampos();
        if (!inicio || !banco || !papaya || !iconeBanco || !iconePapaya || !iconeComida || !carrinho)
        {
            Debug.LogError("EconomiaTelefone: confira os sete campos no Canvas do TELEFONE.", this);
            return;
        }
        telaBanco = Painel(banco, "Contas do Din");
        infoBanco = Texto(telaBanco, .62f, .97f);
        for (int i = 0; i < contas.Length; i++)
        {
            var despesa = (BancoDin.Despesa)i;
            float topo = .60f - i * .12f;
            contas[i] = Botao(telaBanco, topo - .10f, topo, "", () =>
            {
                msgBanco.text = BancoDin.Pagar(despesa);
                Atualizar();
            });
        }
        msgBanco = Texto(telaBanco, .12f, .25f);
        Botao(telaBanco, .02f, .12f, "Voltar", () => banco.gameObject.SetActive(false));

        // Mantém a oferta já desenhada no Papaya e abre a compra pelo carrinho.
        Botao(papaya, .90f, .98f, "Voltar ao telefone", () => papaya.gameObject.SetActive(false));
        telaLoja = Painel(papaya, "Carrinho do Papaya");
        infoLoja = Texto(telaLoja, .65f, .94f);
        placa = Botao(telaLoja, .49f, .62f, "", () => Comprar(BancoDin.Peca.PlacaMae));
        memoria = Botao(telaLoja, .32f, .45f, "", () => Comprar(BancoDin.Peca.Memoria));
        msgLoja = Texto(telaLoja, .14f, .30f);
        Botao(telaLoja, .02f, .12f, "Voltar à oferta", () => telaLoja.gameObject.SetActive(false));
        telaLoja.gameObject.SetActive(false);

        telaComida = Painel(inicio, "Refeição do Din");
        infoComida = Texto(telaComida, .59f, .90f);
        Botao(telaComida, .37f, .52f, "Comprar refeição • R$ 20", () =>
        {
            msgComida.text = BancoDin.ComprarComida();
            Atualizar();
        });
        msgComida = Texto(telaComida, .16f, .35f);
        Botao(telaComida, .02f, .12f, "Voltar", () => telaComida.gameObject.SetActive(false));
        telaComida.gameObject.SetActive(false);

        Ligar(iconeBanco, () => { banco.gameObject.SetActive(true); msgBanco.text = ""; Atualizar(); });
        Ligar(iconePapaya, () => { papaya.gameObject.SetActive(true); telaLoja.gameObject.SetActive(false); });
        Ligar(carrinho, () => { telaLoja.gameObject.SetActive(true); msgLoja.text = ""; Atualizar(); });
        Ligar(iconeComida, () => { telaComida.gameObject.SetActive(true); msgComida.text = ""; Atualizar(); });
        Atualizar();
    }

    void Comprar(BancoDin.Peca p)
    {
        msgLoja.text = BancoDin.Comprar(p);
        Atualizar();
    }

    void Atualizar()
    {
        infoBanco.text = $"{BancoDin.Data}\nSaldo: {BancoDin.Reais(BancoDin.Saldo)}\nComida reservada: {BancoDin.Reais(BancoDin.Reserva)}\nLivre para peças: {BancoDin.Reais(BancoDin.Livre)}";
        for (int i = 0; i < contas.Length; i++)
        {
            int valor = BancoDin.Pendente((BancoDin.Despesa)i);
            Rotulo(contas[i], nomes[i] + (valor == 0 ? " • paga" : " • pagar " + BancoDin.Reais(valor)));
            contas[i].interactable = valor > 0;
        }
        infoLoja.text = "Papaya\nLivre para peças: " + BancoDin.Reais(BancoDin.Livre);
        MostrarPeca(placa, "Placa-mãe", BancoDin.Peca.PlacaMae);
        MostrarPeca(memoria, "Memória RAM", BancoDin.Peca.Memoria);
        infoComida.text = "YouFood\nReserva do mês: " + BancoDin.Reais(BancoDin.Reserva);
    }

    static void MostrarPeca(Button b, string nome, BancoDin.Peca p)
    {
        bool comprada = BancoDin.Comprada(p);
        Rotulo(b, nome + " • " + (comprada ? "Comprada" : BancoDin.Reais(BancoDin.Preco(p))));
        b.interactable = !comprada;
    }

    static void Ligar(RectTransform alvo, UnityAction acao)
    {
        var botao = alvo.GetComponent<Button>();
        if (!botao) botao = alvo.gameObject.AddComponent<Button>();
        var imagem = alvo.GetComponent<Graphic>();
        if (imagem) { imagem.raycastTarget = true; botao.targetGraphic = imagem; }
        botao.onClick.AddListener(acao);
    }

    static RectTransform Area(Transform pai, string nome, float margem, float baixo, float topo)
    {
        var rt = (RectTransform)new GameObject(nome, typeof(RectTransform)).transform;
        rt.SetParent(pai, false);
        rt.anchorMin = new Vector2(margem, baixo);
        rt.anchorMax = new Vector2(1f - margem, topo);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    static RectTransform Painel(Transform pai, string nome)
    {
        var rt = Area(pai, nome, 0, 0, 1);
        rt.gameObject.AddComponent<Image>().color = new Color(.09f, .12f, .18f);
        return rt;
    }

    static TMP_Text Texto(Transform pai, float baixo, float topo)
    {
        var rt = Area(pai, "Texto", .04f, baixo, topo);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = "";
        t.enableAutoSizing = true;
        t.fontSizeMin = 8;
        t.fontSizeMax = 19;
        t.color = Color.white;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }

    static Button Botao(Transform pai, float baixo, float topo, string nome, UnityAction acao)
    {
        var rt = Area(pai, "Botao", .06f, baixo, topo);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = new Color(.15f, .35f, .49f);
        var b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        Texto(rt, 0, 1).text = nome;
        b.onClick.AddListener(acao);
        return b;
    }

    static void Rotulo(Button b, string texto) { b.GetComponentInChildren<TMP_Text>(true).text = texto; }
}
