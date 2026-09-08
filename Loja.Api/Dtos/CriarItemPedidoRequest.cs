namespace Loja.Api.Dtos;

public sealed class CriarItemPedidoRequest
{
    public string Produto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
}
