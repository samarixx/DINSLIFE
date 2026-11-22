using System;
using System.Globalization;
using UnityEngine;

public static class BancoDin
{
    // Todos os valores são guardados em centavos.
    const int Salario = 162100, Alimentacao = 60000;
    const string Chave = "DINSLIFE.economia.v1";
    public const int Refeicao = 2000;
    static readonly CultureInfo Brasil = CultureInfo.GetCultureInfo("pt-BR");

    [Serializable]
    class Conta
    {
        public int dia = 1, mes = 1, saldo = Salario;
        public int aluguel = 75000, agua = 6000, energia = 11000;
        public int reservaComida = Alimentacao;
        public bool placaMae, memoria;
    }

    static Conta conta;
    static Conta Dados
    {
        get
        {
            if (conta == null)
                conta = PlayerPrefs.HasKey(Chave)
                    ? JsonUtility.FromJson<Conta>(PlayerPrefs.GetString(Chave))
                    : new Conta();
            return conta;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void NovaSessao() { conta = null; }

    public static int Saldo => Dados.saldo;
    public static int Livre => Mathf.Max(0, Saldo - Dados.aluguel - Dados.agua - Dados.energia - Dados.reservaComida);
    public static int Reserva => Dados.reservaComida;
    public static string Data => $"Dia {Dados.dia} • Mês {Dados.mes}";
    public static string Reais(int valor) => (valor / 100m).ToString("C2", Brasil);

    static void Salvar()
    {
        PlayerPrefs.SetString(Chave, JsonUtility.ToJson(Dados));
        PlayerPrefs.Save();
    }

    public static void Depositar(int valor)
    {
        if (valor <= 0) return;
        Dados.saldo += valor;
        Salvar();
    }

    public enum Despesa { Aluguel, Agua, Energia }
    public static int Pendente(Despesa tipo)
    {
        if (tipo == Despesa.Aluguel) return Dados.aluguel;
        if (tipo == Despesa.Agua) return Dados.agua;
        return Dados.energia;
    }

    public static string Pagar(Despesa tipo)
    {
        int valor = Pendente(tipo);
        if (valor == 0) return "Essa conta já foi paga.";
        if (Saldo < valor) return "Saldo insuficiente.";
        Dados.saldo -= valor;
        if (tipo == Despesa.Aluguel) Dados.aluguel = 0;
        else if (tipo == Despesa.Agua) Dados.agua = 0;
        else Dados.energia = 0;
        Salvar();
        return "Pagamento de " + Reais(valor) + " confirmado.";
    }

    public static string ComprarComida()
    {
        PlayerNeeds necessidades = PlayerNeeds.Instance;
        if (necessidades == null) return "Entre pelo quarto para carregar o Din.";
        if (necessidades.fome >= 99.99f) return "Din já está alimentado.";
        if (Saldo - Dados.aluguel - Dados.agua - Dados.energia < Refeicao)
            return "O dinheiro restante está reservado para as contas.";
        Dados.saldo -= Refeicao;
        Dados.reservaComida = Mathf.Max(0, Reserva - Refeicao);
        necessidades.Comer(30f);
        Salvar();
        return "Refeição comprada! Din comeu.";
    }

    public enum Peca { PlacaMae, Memoria }
    public static int Preco(Peca p) => p == Peca.PlacaMae ? 35000 : 18000;
    public static bool Comprada(Peca p) => p == Peca.PlacaMae ? Dados.placaMae : Dados.memoria;
    public static string Comprar(Peca p)
    {
        if (Comprada(p)) return "Essa peça já é sua.";
        int preco = Preco(p);
        if (Livre < preco) return "Faltam " + Reais(preco - Livre) + " para comprar.";
        Dados.saldo -= preco;
        if (p == Peca.PlacaMae) Dados.placaMae = true;
        else Dados.memoria = true;
        Salvar();
        return "Peça comprada para o PC do Din!";
    }

    public static void Dormir()
    {
        Dados.dia++;
        if (Dados.dia > 30)
        {
            Dados.dia = 1;
            Dados.mes++;
            Dados.saldo += Salario;
            Dados.aluguel += 75000;
            Dados.agua += 6000;
            Dados.energia += 11000;
            Dados.reservaComida = Alimentacao;
        }
        if (PlayerNeeds.Instance != null) PlayerNeeds.Instance.Dormir(100f);
        Salvar();
    }
}
