using Loja.Api.Dtos;

namespace Loja.Api.Services;

public interface IPedidoService
{
    Task<ResultadoOperacao<PedidoResponse>> CriarAsync(CriarPedidoRequest request);

    Task<IReadOnlyList<PedidoResponse>> ListarAsync();

    Task<PedidoResponse?> ObterAsync(int id);

    Task<ResultadoOperacao<PedidoResponse>> AtualizarAsync(int id, AtualizarPedidoRequest request);

    Task<ResultadoOperacao<bool>> DesativarAsync(int id);

    Task<ResultadoOperacao<PedidoResponse>> FecharAsync(int id);

    Task<ResultadoOperacao<ImportacaoPedidosResponse>> ImportarAsync(IReadOnlyList<CriarPedidoRequest> requests);

    Task<IReadOnlyList<ResumoStatusResponse>> ObterResumoAsync();
}
