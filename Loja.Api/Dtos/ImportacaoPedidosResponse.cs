namespace Loja.Api.Dtos;

public sealed class ImportacaoPedidosResponse
{
    public int QuantidadeCriada { get; set; }
    public List<int> Ids { get; set; } = [];
}
