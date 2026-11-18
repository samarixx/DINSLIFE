
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PingPongCelular : MonoBehaviour,
    IPointerDownHandler, IDragHandler
{
    [Header("Objetos do jogo")]
    public RectTransform barraJogador;
    public RectTransform barraCPU;
    public RectTransform bola;

    [Header("Placar")]
    public TMP_Text placarJogador;
    public TMP_Text placarCPU;

    [Header("Velocidades")]
    public float velocidadeBola = 300f;
    public float velocidadeJogador = 400f;
    public float velocidadeCPU = 220f;

    [Header("Distancia das laterais")]
    public float margem = 15f;

    private RectTransform area;
    private Vector2 direcaoBola;

    private int pontosJogador = 0;
    private int pontosCPU = 0;

    private bool jogoPronto = false;

    void Start()
    {
        area = GetComponent<RectTransform>();

        if (area == null ||
            barraJogador == null ||
            barraCPU == null ||
            bola == null)
        {
            Debug.LogError(
                "PING PONG: Verifique os objetos no Inspector!",
                this
            );
            enabled = false;
            return;
        }

        if (area.rect.width <= 0 ||
            area.rect.height <= 0)
        {
            Debug.LogError(
                "PING PONG: O Panel precisa ter tamanho!",
                this
            );
            enabled = false;
            return;
        }

        // Posiciona as barras automaticamente
        float metadeLargura = area.rect.width / 2f;

        barraJogador.anchoredPosition = new Vector2(
            -metadeLargura + margem +
            barraJogador.rect.width / 2f,
            0f
        );

        barraCPU.anchoredPosition = new Vector2(
            metadeLargura - margem -
            barraCPU.rect.width / 2f,
            0f
        );

        bola.anchoredPosition = Vector2.zero;

        AtualizarPlacar();
        ReiniciarBola();

        jogoPronto = true;

        Debug.Log("Ping Pong iniciado!");
    }

    void Update()
    {
        if (!jogoPronto)
            return;

        // Movimento independente do Time.timeScale
        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);

        ControlarJogador(dt);
        ControlarCPU(dt);
        MoverBola(dt);
    }

    // CONTROLE DO JOGADOR
    void ControlarJogador(float dt)
    {
        float movimento = 0f;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed ||
                Keyboard.current.upArrowKey.isPressed)
                movimento = 1f;

            if (Keyboard.current.sKey.isPressed ||
                Keyboard.current.downArrowKey.isPressed)
                movimento = -1f;
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.UpArrow))
            movimento = 1f;

        if (Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.DownArrow))
            movimento = -1f;
#endif

        Vector2 pos = barraJogador.anchoredPosition;

        pos.y += movimento * velocidadeJogador * dt;

        barraJogador.anchoredPosition =
            LimitarBarra(barraJogador, pos);
    }

    // CONTROLE PELO MOUSE OU TOQUE
    public void OnPointerDown(PointerEventData evento)
    {
        ArrastarBarra(evento);
    }

    public void OnDrag(PointerEventData evento)
    {
        ArrastarBarra(evento);
    }

    void ArrastarBarra(PointerEventData evento)
    {
        if (!jogoPronto)
            return;

        Vector2 posLocal;

        if (RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                area,
                evento.position,
                evento.pressEventCamera,
                out posLocal))
        {
            Vector2 pos = barraJogador.anchoredPosition;

            pos.y = posLocal.y;

            barraJogador.anchoredPosition =
                LimitarBarra(barraJogador, pos);
        }
    }

    // INTELIGENCIA ARTIFICIAL
    void ControlarCPU(float dt)
    {
        Vector2 pos = barraCPU.anchoredPosition;

        pos.y = Mathf.MoveTowards(
            pos.y,
            bola.anchoredPosition.y,
            velocidadeCPU * dt
        );

        barraCPU.anchoredPosition =
            LimitarBarra(barraCPU, pos);
    }

    // IMPEDE AS BARRAS DE SAIREM DO PANEL
    Vector2 LimitarBarra(
        RectTransform barra,
        Vector2 pos)
    {
        float limite =
            area.rect.height / 2f -
            barra.rect.height / 2f;

        limite = Mathf.Max(0f, limite);

        pos.y = Mathf.Clamp(
            pos.y,
            -limite,
            limite
        );

        return pos;
    }

    // MOVIMENTO E COLISAO DA BOLA
    void MoverBola(float dt)
    {
        Vector2 anterior = bola.anchoredPosition;

        Vector2 pos = anterior +
                      direcaoBola * velocidadeBola * dt;

        float raioX = bola.rect.width / 2f;
        float raioY = bola.rect.height / 2f;

        // COLISAO COM PAREDE SUPERIOR E INFERIOR
        float limiteY = area.rect.height / 2f - raioY;

        if (pos.y >= limiteY)
        {
            pos.y = limiteY;
            direcaoBola.y = -Mathf.Abs(direcaoBola.y);
        }
        else if (pos.y <= -limiteY)
        {
            pos.y = -limiteY;
            direcaoBola.y = Mathf.Abs(direcaoBola.y);
        }

        // COLISAO COM A BARRA DO JOGADOR
        float faceJogador =
            barraJogador.anchoredPosition.x +
            barraJogador.rect.width / 2f + raioX;

        if (direcaoBola.x < 0 &&
            anterior.x >= faceJogador &&
            pos.x <= faceJogador)
        {
            float t =
                (faceJogador - anterior.x) /
                (pos.x - anterior.x);

            float yImpacto = Mathf.Lerp(
                anterior.y,
                pos.y,
                t
            );

            float alcance =
                barraJogador.rect.height / 2f + raioY;

            if (Mathf.Abs(
                yImpacto -
                barraJogador.anchoredPosition.y
                ) <= alcance)
            {
                Rebater(barraJogador, yImpacto, 1);

                float restante = dt * (1f - t);

                pos = new Vector2(
                    faceJogador,
                    yImpacto
                ) + direcaoBola * velocidadeBola * restante;
            }
        }

        // COLISAO COM A BARRA DA CPU
        float faceCPU =
            barraCPU.anchoredPosition.x -
            barraCPU.rect.width / 2f - raioX;

        if (direcaoBola.x > 0 &&
            anterior.x <= faceCPU &&
            pos.x >= faceCPU)
        {
            float t =
                (faceCPU - anterior.x) /
                (pos.x - anterior.x);

            float yImpacto = Mathf.Lerp(
                anterior.y,
                pos.y,
                t
            );

            float alcance =
                barraCPU.rect.height / 2f + raioY;

            if (Mathf.Abs(
                yImpacto -
                barraCPU.anchoredPosition.y
                ) <= alcance)
            {
                Rebater(barraCPU, yImpacto, -1);

                float restante = dt * (1f - t);

                pos = new Vector2(
                    faceCPU,
                    yImpacto
                ) + direcaoBola * velocidadeBola * restante;
            }
        }

        // PONTUACAO
        float limiteX = area.rect.width / 2f + raioX;

        if (pos.x > limiteX)
        {
            pontosJogador++;
            AtualizarPlacar();
            ReiniciarBola();
            return;
        }

        if (pos.x < -limiteX)
        {
            pontosCPU++;
            AtualizarPlacar();
            ReiniciarBola();
            return;
        }

        bola.anchoredPosition = pos;
    }

    // DIRECAO APOS BATER NA BARRA
    void Rebater(
        RectTransform barra,
        float alturaImpacto,
        int lado)
    {
        float impacto =
            (alturaImpacto -
            barra.anchoredPosition.y) /
            (barra.rect.height / 2f);

        impacto = Mathf.Clamp(
            impacto,
            -1f,
            1f
        );

        direcaoBola = new Vector2(
            lado,
            impacto * 0.8f
        ).normalized;
    }

    // REINICIA A BOLA APOS UM PONTO
    void ReiniciarBola()
    {
        bola.anchoredPosition = Vector2.zero;

        float lado = Random.value > 0.5f ? 1f : -1f;

        direcaoBola = new Vector2(
            lado,
            Random.Range(-0.5f, 0.5f)
        ).normalized;
    }

    // ATUALIZA O PLACAR
    void AtualizarPlacar()
    {
        if (placarJogador != null)
        {
            placarJogador.text =
                pontosJogador.ToString();
        }

        if (placarCPU != null)
        {
            placarCPU.text =
                pontosCPU.ToString();
        }
    }
}
