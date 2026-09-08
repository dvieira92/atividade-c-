using Loja.Api.Data;
using Loja.Api.Dtos;
using Loja.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Loja.Api.Services;

public sealed class PedidoService(AppDbContext db) : IPedidoService
{
    public async Task<ResultadoOperacao<PedidoResponse>> CriarAsync(CriarPedidoRequest request)
    {
        var erro = Validar(request);

        if (erro is not null)
        {
            return ResultadoOperacao<PedidoResponse>.EntradaInvalida(erro);
        }

        var pedido = MapearParaEntidade(request);

        await db.Pedidos.AddAsync(pedido);
        await db.SaveChangesAsync();

        return ResultadoOperacao<PedidoResponse>.Ok(MapearParaResposta(pedido));
    }

    public async Task<IReadOnlyList<PedidoResponse>> ListarAsync()
    {
        var pedidos = await db.Pedidos
            .AsNoTracking()
            .Include(pedido => pedido.Itens)
            .Where(pedido => pedido.Ativo)
            .OrderBy(pedido => pedido.Id)
            .ToListAsync();

        return pedidos.Select(MapearParaResposta).ToList();
    }

    public async Task<PedidoResponse?> ObterAsync(int id)
    {
        var pedido = await db.Pedidos
            .AsNoTracking()
            .Include(item => item.Itens)
            .FirstOrDefaultAsync(item => item.Id == id && item.Ativo);

        return pedido is null ? null : MapearParaResposta(pedido);
    }

    public async Task<ResultadoOperacao<PedidoResponse>> AtualizarAsync(
        int id,
        AtualizarPedidoRequest request)
    {
        var pedido = await db.Pedidos
            .Include(item => item.Itens)
            .FirstOrDefaultAsync(item => item.Id == id && item.Ativo);

        if (pedido is null)
        {
            return ResultadoOperacao<PedidoResponse>.NaoEncontrado(
                $"Pedido {id} não encontrado.");
        }

        if (pedido.Status != StatusPedido.Aberto)
        {
            return ResultadoOperacao<PedidoResponse>.Conflito(
                "Somente um pedido aberto pode ser atualizado.");
        }

        if (request.Cliente is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Cliente))
            {
                return ResultadoOperacao<PedidoResponse>.EntradaInvalida(
                    "O cliente não pode ser vazio.");
            }

            pedido.Cliente = request.Cliente.Trim();
        }

        await db.SaveChangesAsync();

        return ResultadoOperacao<PedidoResponse>.Ok(MapearParaResposta(pedido));
    }

    public async Task<ResultadoOperacao<bool>> DesativarAsync(int id)
    {
        var pedido = await db.Pedidos
            .FirstOrDefaultAsync(item => item.Id == id && item.Ativo);

        if (pedido is null)
        {
            return ResultadoOperacao<bool>.NaoEncontrado($"Pedido {id} não encontrado.");
        }

        pedido.Ativo = false;
        await db.SaveChangesAsync();

        return ResultadoOperacao<bool>.Ok(true);
    }

    public async Task<ResultadoOperacao<PedidoResponse>> FecharAsync(int id)
    {
        var pedido = await db.Pedidos
            .Include(item => item.Itens)
            .FirstOrDefaultAsync(item => item.Id == id && item.Ativo);

        if (pedido is null)
        {
            return ResultadoOperacao<PedidoResponse>.NaoEncontrado(
                $"Pedido {id} não encontrado.");
        }

        if (pedido.Status == StatusPedido.Fechado)
        {
            return ResultadoOperacao<PedidoResponse>.Conflito("O pedido já está fechado.");
        }

        if (pedido.Status == StatusPedido.Cancelado)
        {
            return ResultadoOperacao<PedidoResponse>.Conflito(
                "Um pedido cancelado não pode ser fechado.");
        }

        if (pedido.Itens.Count == 0)
        {
            return ResultadoOperacao<PedidoResponse>.Conflito(
                "Um pedido sem itens não pode ser fechado.");
        }

        pedido.Total = pedido.Itens.Sum(
            item => item.Quantidade * item.ValorUnitario);

        pedido.Status = StatusPedido.Fechado;

        await db.SaveChangesAsync();

        return ResultadoOperacao<PedidoResponse>.Ok(MapearParaResposta(pedido));
    }

    public async Task<ResultadoOperacao<ImportacaoPedidosResponse>> ImportarAsync(
        IReadOnlyList<CriarPedidoRequest> requests)
    {
        if (requests.Count == 0)
        {
            return ResultadoOperacao<ImportacaoPedidosResponse>.EntradaInvalida(
                "A importação precisa de pelo menos um pedido.");
        }

        for (var posicao = 0; posicao < requests.Count; posicao++)
        {
            var erro = Validar(requests[posicao]);

            if (erro is not null)
            {
                return ResultadoOperacao<ImportacaoPedidosResponse>.EntradaInvalida(
                    $"Pedido na posição {posicao} da lista: {erro}");
            }
        }

        var pedidos = requests.Select(MapearParaEntidade).ToList();

        await using var transacao = await db.Database.BeginTransactionAsync();

        try
        {
            await db.Pedidos.AddRangeAsync(pedidos);
            await db.SaveChangesAsync();
            await transacao.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await transacao.RollbackAsync();

            return ResultadoOperacao<ImportacaoPedidosResponse>.Conflito(
                "Nenhum pedido foi importado porque o banco de dados recusou o lote.");
        }

        var resposta = new ImportacaoPedidosResponse
        {
            QuantidadeCriada = pedidos.Count,
            Ids = pedidos.Select(pedido => pedido.Id).ToList()
        };

        return ResultadoOperacao<ImportacaoPedidosResponse>.Ok(resposta);
    }

    public async Task<IReadOnlyList<ResumoStatusResponse>> ObterResumoAsync()
    {
        return await db.Pedidos
            .AsNoTracking()
            .Where(pedido => pedido.Ativo)
            .GroupBy(pedido => pedido.Status)
            .OrderBy(grupo => grupo.Key)
            .Select(grupo => new ResumoStatusResponse
            {
                Status = grupo.Key,
                Quantidade = grupo.Count(),
                ValorTotal = grupo.Sum(pedido => pedido.Total)
            })
            .ToListAsync();
    }

    private static string? Validar(CriarPedidoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Cliente))
        {
            return "O cliente é obrigatório.";
        }

        if (request.Itens.Count == 0)
        {
            return "O pedido precisa de pelo menos um item.";
        }

        foreach (var item in request.Itens)
        {
            if (string.IsNullOrWhiteSpace(item.Produto))
            {
                return "O produto do item é obrigatório.";
            }

            if (item.Quantidade <= 0)
            {
                return "A quantidade do item deve ser maior que zero.";
            }

            if (item.ValorUnitario <= 0)
            {
                return "O valor unitário do item deve ser maior que zero.";
            }
        }

        return null;
    }

    private static Pedido MapearParaEntidade(CriarPedidoRequest request) => new()
    {
        Cliente = request.Cliente.Trim(),
        Itens = request.Itens
            .Select(item => new ItemPedido
            {
                Produto = item.Produto.Trim(),
                Quantidade = item.Quantidade,
                ValorUnitario = item.ValorUnitario
            })
            .ToList()
    };

    private static PedidoResponse MapearParaResposta(Pedido pedido) => new()
    {
        Id = pedido.Id,
        Cliente = pedido.Cliente,
        Status = pedido.Status,
        Total = pedido.Total,
        CriadoEm = pedido.CriadoEm,
        Itens = pedido.Itens
            .Select(item => new ItemPedidoResponse
            {
                Id = item.Id,
                Produto = item.Produto,
                Quantidade = item.Quantidade,
                ValorUnitario = item.ValorUnitario
            })
            .ToList()
    };
}
