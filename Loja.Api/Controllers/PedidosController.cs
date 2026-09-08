using Loja.Api.Dtos;
using Loja.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Loja.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PedidosController(IPedidoService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PedidoResponse>> Criar(CriarPedidoRequest request)
    {
        var resultado = await service.CriarAsync(request);

        if (!resultado.Sucesso)
        {
            return Falha(resultado);
        }

        return CreatedAtAction(
            nameof(Obter),
            new { id = resultado.Valor!.Id },
            resultado.Valor);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PedidoResponse>>> Listar()
    {
        return Ok(await service.ListarAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PedidoResponse>> Obter(int id)
    {
        var pedido = await service.ObterAsync(id);

        return pedido is null ? NotFound() : Ok(pedido);
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<PedidoResponse>> Atualizar(
        int id,
        AtualizarPedidoRequest request)
    {
        var resultado = await service.AtualizarAsync(id, request);

        return resultado.Sucesso ? Ok(resultado.Valor) : Falha(resultado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desativar(int id)
    {
        var resultado = await service.DesativarAsync(id);

        return resultado.Sucesso ? NoContent() : Falha(resultado);
    }

    [HttpPost("{id:int}/fechar")]
    public async Task<ActionResult<PedidoResponse>> Fechar(int id)
    {
        var resultado = await service.FecharAsync(id);

        return resultado.Sucesso ? Ok(resultado.Valor) : Falha(resultado);
    }

    [HttpPost("importacao")]
    public async Task<ActionResult<ImportacaoPedidosResponse>> Importar(
        List<CriarPedidoRequest> requests)
    {
        var resultado = await service.ImportarAsync(requests);

        return resultado.Sucesso ? Ok(resultado.Valor) : Falha(resultado);
    }

    [HttpGet("resumo")]
    public async Task<ActionResult<IReadOnlyList<ResumoStatusResponse>>> ObterResumo()
    {
        return Ok(await service.ObterResumoAsync());
    }

    private ActionResult Falha<T>(ResultadoOperacao<T> resultado) => resultado.Falha switch
    {
        FalhaOperacao.EntradaInvalida => BadRequest(new { erro = resultado.Mensagem }),
        FalhaOperacao.NaoEncontrado => NotFound(new { erro = resultado.Mensagem }),
        FalhaOperacao.Conflito => Conflict(new { erro = resultado.Mensagem }),
        _ => StatusCode(StatusCodes.Status500InternalServerError)
    };
}
