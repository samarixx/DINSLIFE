using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Sistema de mercado do DIN'SLIFE.
///
/// - Chega um cliente aleatório (nome, aparência e pedido sorteados).
/// - No computador o jogador vê a tabela de preços (também sorteados) e o pedido.
/// - Ele precisa calcular o TOTAL da compra e depois o TROCO.
///
/// Toda a interface é criada por código: basta colocar este script em um
/// GameObject vazio da cena MERCADO. Nada mais é obrigatório no Inspector.
/// </summary>
public class MercadoManager : MonoBehaviour
{
    public static MercadoManager Instance { get; private set; }

    // Outros scripts consultam isso para ignorar cliques no mundo.
    public static bool PcAberto { get; private set; }

    [System.Serializable]
    public class ProdutoMercado
    {
        public string nome;
        public float precoMinimo = 2f;   // em reais
        public float precoMaximo = 10f;  // em reais
        [HideInInspector] public int precoAtual; // em centavos (sorteado em runtime)
    }

    private class ItemPedido
    {
        public ProdutoMercado produto;
        public int quantidade;
    }

    private class Cliente
    {
        public string nome;
        public List<ItemPedido> itens = new List<ItemPedido>();
        public int total;  // centavos
        public int pago;   // centavos
        public int troco => pago - total;
    }

    private enum Etapa { Total, Troco }

    [Header("Produtos (se vazio, usa uma lista padrão)")]
    public List<ProdutoMercado> produtos = new List<ProdutoMercado>();

    [Header("Cliente na cena (opcional)")]
    [Tooltip("SpriteRenderer do cliente (ex.: Pessoa2). Ele é ligado/desligado a cada cliente.")]
    [SerializeField] private SpriteRenderer clienteVisual;
    [Tooltip("Aparências possíveis. Uma é sorteada para cada cliente.")]
    [SerializeField] private List<Sprite> spritesClientes = new List<Sprite>();
    [SerializeField] private List<string> nomesClientes = new List<string>
    {
        "Maria", "João", "Ana", "Carlos", "Beatriz", "Pedro", "Luiza", "Rafael", "Joana", "Marcos"
    };

    [Header("Regras")]
    [SerializeField, Min(1)] private int minItens = 2;
    [SerializeField, Min(1)] private int maxItens = 4;
    [SerializeField, Min(1)] private int maxQuantidade = 5;
    [Tooltip("A cada quantos clientes os preços são sorteados de novo.")]
    [SerializeField, Min(1)] private int clientesPorRodada = 5;
    [Tooltip("Dinheiro ganho (em reais) a cada cliente atendido corretamente.")]
    [SerializeField] private float recompensa = 5f;

    // Estado do jogo
    private Cliente clienteAtual;
    private Etapa etapa;
    private string entrada = "";
    private bool aguardando;
    private int clientesAtendidos;
    private int acertos;
    private int erros;
    private int dinheiro; // centavos

    private static readonly int[] notas = { 500, 1000, 2000, 5000, 10000, 20000 };

    // UI
    private GameObject painelPc, balao;
    private Button botaoAbrir;
    private TMP_Text txtHud, txtBalao, txtPrecos, txtPedido, txtEtapa, txtDisplay, txtFeedback;

    private static readonly Color corAcerto = new Color(0.5f, 1f, 0.5f);
    private static readonly Color corErro = new Color(1f, 0.45f, 0.45f);
    private static readonly Color corAviso = new Color(1f, 0.92f, 0.4f);

    // ------------------------------------------------------------------ ciclo de vida

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (produtos.Count == 0) PreencherProdutosPadrao();
        if (nomesClientes.Count == 0) nomesClientes.Add("Cliente");
        if (maxItens < minItens) maxItens = minItens;

        ConstruirUI();
    }

    private void Start()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput += AoDigitar;

        if (clienteVisual != null) clienteVisual.enabled = false;
        SortearPrecos();
        ProximoCliente();
    }

    private void Update()
    {
        if (PcAberto && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            FecharComputador();
    }

    private void OnDestroy()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= AoDigitar;
        if (Instance == this) PcAberto = false;
    }

    // ------------------------------------------------------------------ produtos e clientes

    private void PreencherProdutosPadrao()
    {
        produtos.Add(new ProdutoMercado { nome = "Pão",       precoMinimo = 1f, precoMaximo = 3f });
        produtos.Add(new ProdutoMercado { nome = "Queijo",    precoMinimo = 4f, precoMaximo = 9f });
        produtos.Add(new ProdutoMercado { nome = "Macarrão",  precoMinimo = 3f, precoMaximo = 7f });
        produtos.Add(new ProdutoMercado { nome = "Feijão",    precoMinimo = 5f, precoMaximo = 10f });
        produtos.Add(new ProdutoMercado { nome = "Chocolate", precoMinimo = 3f, precoMaximo = 8f });
        produtos.Add(new ProdutoMercado { nome = "Leite",     precoMinimo = 4f, precoMaximo = 7f });
    }

    private void SortearPrecos()
    {
        foreach (ProdutoMercado p in produtos)
        {
            // preços de 0,50 em 0,50 para o cálculo mental ficar possível
            int passos = Mathf.RoundToInt(Random.Range(p.precoMinimo, p.precoMaximo) * 2f);
            p.precoAtual = Mathf.Max(1, passos) * 50;
        }
        AtualizarTextosPc();
    }

    private void ProximoCliente()
    {
        Cliente c = new Cliente
        {
            nome = nomesClientes[Random.Range(0, nomesClientes.Count)]
        };

        // embaralha os produtos e pega alguns
        List<ProdutoMercado> pool = new List<ProdutoMercado>(produtos);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        int n = Mathf.Clamp(Random.Range(minItens, maxItens + 1), 1, pool.Count);
        for (int i = 0; i < n; i++)
        {
            ItemPedido item = new ItemPedido
            {
                produto = pool[i],
                quantidade = Random.Range(1, maxQuantidade + 1)
            };
            c.itens.Add(item);
            c.total += item.produto.precoAtual * item.quantidade;
        }

        // paga com a menor nota que cobre a compra (sempre sobra troco)
        c.pago = notas[notas.Length - 1];
        foreach (int nota in notas)
        {
            if (nota > c.total) { c.pago = nota; break; }
        }

        clienteAtual = c;
        etapa = Etapa.Total;
        entrada = "";

        if (clienteVisual != null)
        {
            if (spritesClientes.Count > 0)
                clienteVisual.sprite = spritesClientes[Random.Range(0, spritesClientes.Count)];
            clienteVisual.enabled = true;
        }

        txtBalao.text = $"<b>{c.nome}:</b> Olá! Quero levar:\n{TextoPedido(c)}\nPode calcular no computador?";
        balao.SetActive(true);
        botaoAbrir.gameObject.SetActive(true);
        AtualizarHud();
        AtualizarTextosPc();
    }

    // ------------------------------------------------------------------ computador

    public void AbrirComputador()
    {
        if (clienteAtual == null || PcAberto) return;
        PcAberto = true;
        painelPc.SetActive(true);
        balao.SetActive(false);
        botaoAbrir.gameObject.SetActive(false);
        Feedback("", Color.white);
        AtualizarTextosPc();
    }

    public void FecharComputador()
    {
        if (aguardando) return;
        PcAberto = false;
        painelPc.SetActive(false);
        if (clienteAtual != null)
        {
            balao.SetActive(true);
            botaoAbrir.gameObject.SetActive(true);
        }
    }

    // ------------------------------------------------------------------ entrada do jogador

    private void AoDigitar(char c)
    {
        if (!PcAberto || aguardando) return;

        if (char.IsDigit(c)) Digitar(c.ToString());
        else if (c == ',' || c == '.') Digitar(",");
        else if (c == '\b') Apagar();
        else if (c == '\r' || c == '\n') Confirmar();
    }

    private void Digitar(string s)
    {
        if (aguardando) return;
        if (entrada.Length >= 8) return;

        if (s == ",")
        {
            if (entrada.Contains(",")) return;
            if (entrada.Length == 0) entrada = "0";
        }
        else
        {
            int v = entrada.IndexOf(',');
            if (v >= 0 && entrada.Length - v > 2) return; // no máximo 2 casas decimais
        }

        entrada += s;
        txtDisplay.text = "R$ " + entrada;
    }

    private void Apagar()
    {
        if (aguardando || entrada.Length == 0) return;
        entrada = entrada.Substring(0, entrada.Length - 1);
        txtDisplay.text = "R$ " + entrada;
    }

    private void Confirmar()
    {
        if (aguardando || clienteAtual == null) return;

        if (!TentarLer(entrada, out int valor))
        {
            Feedback("Digite um valor primeiro.", corAviso);
            return;
        }

        int esperado = etapa == Etapa.Total ? clienteAtual.total : clienteAtual.troco;

        if (valor == esperado)
        {
            if (etapa == Etapa.Total)
            {
                etapa = Etapa.Troco;
                entrada = "";
                Feedback("Total correto! Agora calcule o troco.", corAcerto);
                AtualizarTextosPc();
            }
            else
            {
                acertos++;
                int ganho = Mathf.RoundToInt(recompensa * 100f);
                dinheiro += ganho;
                StartCoroutine(Encerrar($"Perfeito! Você ganhou {Reais(ganho)}.", corAcerto));
            }
        }
        else
        {
            erros++;
            string o = etapa == Etapa.Total ? "total" : "troco";
            StartCoroutine(Encerrar($"Errado! O {o} era {Reais(esperado)}. O cliente foi embora.", corErro));
        }
    }

    private IEnumerator Encerrar(string mensagem, Color cor)
    {
        aguardando = true;
        Feedback(mensagem, cor);
        AtualizarHud();
        yield return new WaitForSeconds(2.2f);

        aguardando = false;
        clientesAtendidos++;
        FecharComputador();

        clienteAtual = null;
        balao.SetActive(false);
        botaoAbrir.gameObject.SetActive(false);
        if (clienteVisual != null) clienteVisual.enabled = false;

        yield return new WaitForSeconds(1.5f);

        // a cada rodada de clientes, os preços mudam
        if (clientesAtendidos % clientesPorRodada == 0) SortearPrecos();
        ProximoCliente();
    }

    private static bool TentarLer(string texto, out int centavos)
    {
        centavos = 0;
        if (string.IsNullOrEmpty(texto)) return false;
        if (!decimal.TryParse(texto.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out decimal v))
            return false;
        centavos = (int)decimal.Round(v * 100m);
        return true;
    }

    // ------------------------------------------------------------------ textos

    private static string Reais(int centavos)
    {
        return $"R$ {centavos / 100},{centavos % 100:00}";
    }

    private static string TextoPedido(Cliente c)
    {
        string s = "";
        foreach (ItemPedido i in c.itens)
            s += $"{i.quantidade}x {i.produto.nome}\n";
        return s.TrimEnd('\n');
    }

    private void AtualizarHud()
    {
        txtHud.text = $"Dinheiro: {Reais(dinheiro)}    Acertos: {acertos}    Erros: {erros}";
    }

    private void Feedback(string msg, Color cor)
    {
        txtFeedback.text = msg;
        txtFeedback.color = cor;
    }

    private void AtualizarTextosPc()
    {
        if (txtPrecos == null) return;

        string precos = "";
        foreach (ProdutoMercado p in produtos)
            precos += $"{p.nome}: {Reais(p.precoAtual)}\n";
        txtPrecos.text = precos.TrimEnd('\n');

        if (clienteAtual == null)
        {
            txtPedido.text = "Nenhum cliente no momento.";
            txtEtapa.text = "";
            txtDisplay.text = "R$ ";
            return;
        }

        txtPedido.text = $"<b>{clienteAtual.nome}</b> quer:\n\n{TextoPedido(clienteAtual)}";
        txtEtapa.text = etapa == Etapa.Total
            ? "Qual é o <b>TOTAL</b> da compra?"
            : $"O cliente pagou com <b>{Reais(clienteAtual.pago)}</b>.\nQual é o <b>TROCO</b>?";
        txtDisplay.text = "R$ " + entrada;
    }

    // ------------------------------------------------------------------ construção da UI

    private void ConstruirUI()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        GameObject goCanvas = new GameObject("MercadoCanvas");
        goCanvas.transform.SetParent(transform, false);
        Canvas canvas = goCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler cs = goCanvas.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight = 0.5f;
        goCanvas.AddComponent<GraphicRaycaster>();
        Transform raiz = goCanvas.transform;

        // HUD
        txtHud = Texto("Hud", raiz, new Vector2(0.01f, 0.93f), new Vector2(0.6f, 0.99f),
            "", 34, TextAlignmentOptions.Left, Color.white);

        // Balão do cliente
        RectTransform rtBalao = Retangulo("Balao", raiz, new Vector2(0.28f, 0.76f), new Vector2(0.72f, 0.96f));
        Fundo(rtBalao, new Color(1f, 1f, 1f, 0.95f));
        balao = rtBalao.gameObject;
        txtBalao = Texto("TextoBalao", rtBalao, new Vector2(0.03f, 0.05f), new Vector2(0.97f, 0.95f),
            "", 30, TextAlignmentOptions.Left, new Color(0.1f, 0.1f, 0.1f));
        balao.SetActive(false);

        // Botão para abrir o computador
        botaoAbrir = Botao("BotaoComputador", raiz, new Vector2(0.01f, 0.02f), new Vector2(0.2f, 0.1f),
            "Computador", new Color(0.15f, 0.45f, 0.85f), AbrirComputador, 34);
        botaoAbrir.gameObject.SetActive(false);

        // Painel do computador
        RectTransform rtPc = Retangulo("PainelComputador", raiz, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.92f));
        Fundo(rtPc, new Color(0.07f, 0.09f, 0.15f, 0.98f));
        painelPc = rtPc.gameObject;

        Texto("Titulo", rtPc, new Vector2(0.02f, 0.9f), new Vector2(0.98f, 0.99f),
            "COMPUTADOR DO MERCADO", 44, TextAlignmentOptions.Center, Color.white);

        Texto("CabPrecos", rtPc, new Vector2(0.03f, 0.8f), new Vector2(0.33f, 0.89f),
            "TABELA DE PREÇOS", 32, TextAlignmentOptions.Left, corAviso);
        txtPrecos = Texto("Precos", rtPc, new Vector2(0.03f, 0.15f), new Vector2(0.33f, 0.8f),
            "", 34, TextAlignmentOptions.TopLeft, Color.white);

        Texto("CabPedido", rtPc, new Vector2(0.35f, 0.8f), new Vector2(0.65f, 0.89f),
            "PEDIDO", 32, TextAlignmentOptions.Left, corAviso);
        txtPedido = Texto("Pedido", rtPc, new Vector2(0.35f, 0.15f), new Vector2(0.65f, 0.8f),
            "", 34, TextAlignmentOptions.TopLeft, Color.white);

        txtEtapa = Texto("Etapa", rtPc, new Vector2(0.67f, 0.68f), new Vector2(0.97f, 0.89f),
            "", 30, TextAlignmentOptions.Left, Color.white);

        RectTransform rtDisplay = Retangulo("Display", rtPc, new Vector2(0.67f, 0.57f), new Vector2(0.97f, 0.67f));
        Fundo(rtDisplay, new Color(0.02f, 0.03f, 0.06f, 1f));
        txtDisplay = Texto("TextoDisplay", rtDisplay, new Vector2(0.04f, 0f), new Vector2(0.96f, 1f),
            "R$ ", 48, TextAlignmentOptions.Right, Color.white);

        // Teclado numérico (3 colunas x 4 linhas)
        RectTransform rtTeclado = Retangulo("Teclado", rtPc, new Vector2(0.67f, 0.17f), new Vector2(0.97f, 0.56f));
        string[] teclas = { "7", "8", "9", "4", "5", "6", "1", "2", "3", ",", "0", "<" };
        for (int i = 0; i < teclas.Length; i++)
        {
            string t = teclas[i];
            int col = i % 3, lin = i / 3;
            Vector2 min = new Vector2(col / 3f, 1f - (lin + 1) / 4f);
            Vector2 max = new Vector2((col + 1) / 3f, 1f - lin / 4f);
            UnityAction acao = t == "<" ? (UnityAction)Apagar : () => Digitar(t);
            Botao("Tecla" + t, rtTeclado, min, max, t, new Color(0.25f, 0.3f, 0.4f), acao, 42);
        }

        Botao("BotaoOk", rtPc, new Vector2(0.67f, 0.04f), new Vector2(0.82f, 0.15f),
            "OK", new Color(0.15f, 0.6f, 0.25f), Confirmar, 40);
        Botao("BotaoFechar", rtPc, new Vector2(0.83f, 0.04f), new Vector2(0.97f, 0.15f),
            "Fechar", new Color(0.7f, 0.2f, 0.2f), FecharComputador, 34);

        txtFeedback = Texto("Feedback", rtPc, new Vector2(0.03f, 0.03f), new Vector2(0.65f, 0.14f),
            "", 32, TextAlignmentOptions.Left, Color.white);

        painelPc.SetActive(false);
    }

    private static RectTransform Retangulo(string nome, Transform pai, Vector2 min, Vector2 max)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(pai, false);
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    private static Image Fundo(RectTransform rt, Color cor)
    {
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = cor;
        return img;
    }

    private static TMP_Text Texto(string nome, Transform pai, Vector2 min, Vector2 max,
        string conteudo, float tamanho, TextAlignmentOptions alinhamento, Color cor)
    {
        RectTransform rt = Retangulo(nome, pai, min, max);
        TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = conteudo;
        t.fontSize = tamanho;
        t.alignment = alinhamento;
        t.color = cor;
        t.raycastTarget = false;
        return t;
    }

    private static Button Botao(string nome, Transform pai, Vector2 min, Vector2 max,
        string rotulo, Color cor, UnityAction acao, float tamanhoFonte)
    {
        RectTransform rt = Retangulo(nome, pai, min, max);
        rt.offsetMin = new Vector2(3, 3);
        rt.offsetMax = new Vector2(-3, -3);
        Image img = Fundo(rt, cor);
        Button b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        b.onClick.AddListener(acao);
        Texto("Texto", rt, Vector2.zero, Vector2.one, rotulo, tamanhoFonte,
            TextAlignmentOptions.Center, Color.white);
        return b;
    }
}
