namespace Loja.Api.Dtos;

public sealed class CriarPedidoRequest
{
    public string Cliente { get; set; } = string.Empty;
    public List<CriarItemPedidoRequest> Itens { get; set; } = [];
}
