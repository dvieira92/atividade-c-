namespace Loja.Api.Models;

public sealed class ItemPedido
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public string Produto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public Pedido Pedido { get; set; } = null!;
}
