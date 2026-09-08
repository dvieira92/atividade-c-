using Loja.Api.Models;

namespace Loja.Api.Dtos;

public sealed class ResumoStatusResponse
{
    public StatusPedido Status { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorTotal { get; set; }
}
