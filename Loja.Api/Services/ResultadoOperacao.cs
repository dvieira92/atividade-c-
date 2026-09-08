namespace Loja.Api.Services;

public enum FalhaOperacao
{
    Nenhuma,
    EntradaInvalida,
    NaoEncontrado,
    Conflito
}

public sealed class ResultadoOperacao<T>
{
    private ResultadoOperacao(T? valor, FalhaOperacao falha, string? mensagem)
    {
        Valor = valor;
        Falha = falha;
        Mensagem = mensagem;
    }

    public T? Valor { get; }
    public FalhaOperacao Falha { get; }
    public string? Mensagem { get; }
    public bool Sucesso => Falha == FalhaOperacao.Nenhuma;

    public static ResultadoOperacao<T> Ok(T valor) =>
        new(valor, FalhaOperacao.Nenhuma, null);

    public static ResultadoOperacao<T> EntradaInvalida(string mensagem) =>
        new(default, FalhaOperacao.EntradaInvalida, mensagem);

    public static ResultadoOperacao<T> NaoEncontrado(string mensagem) =>
        new(default, FalhaOperacao.NaoEncontrado, mensagem);

    public static ResultadoOperacao<T> Conflito(string mensagem) =>
        new(default, FalhaOperacao.Conflito, mensagem);
}
