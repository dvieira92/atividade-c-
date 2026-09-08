# Aula 4 — Primeira API REST com banco de dados

Atividade da aula 4 (FIAP — C# Software Development): API REST de **pedidos** com
ASP.NET Core, Entity Framework Core, SQL Server LocalDB e migrações code-first.

## Como executar

1. Abrir `Loja.sln` no Visual Studio (ou usar a CLI na pasta `Loja.Api`).
2. Conferir a connection string em `Loja.Api/appsettings.json`.
3. Aplicar as migrações:

   ```powershell
   Update-Database
   ```

   Na CLI: `dotnet ef database update`.
4. Executar o projeto e testar as rotas no Insomnia.

## Rotas

| Rota | Responsabilidade | Resposta |
| --- | --- | --- |
| `POST /api/pedidos` | Criar um pedido | `201 Created` |
| `GET /api/pedidos` | Listar pedidos ativos | `200 OK` |
| `GET /api/pedidos/{id}` | Consultar um pedido | `200 OK` / `404 Not Found` |
| `PATCH /api/pedidos/{id}` | Atualizar parcialmente | `200 OK` / `404` / `409` |
| `DELETE /api/pedidos/{id}` | Desativar (exclusão lógica) | `204 No Content` / `404` |
| `POST /api/pedidos/{id}/fechar` | Calcular total e fechar | `200 OK` / `404` / `409` |
| `POST /api/pedidos/importacao` | Importar um lote | `200 OK` / `400` |
| `GET /api/pedidos/resumo` | Agrupar por status | `200 OK` |

O arquivo `exemplos/importacao-pedidos.json` traz um corpo pronto para a importação.

## Organização

```
Loja.Api/
  Models/        Produto, Pedido, ItemPedido e StatusPedido
  Data/          AppDbContext e o relacionamento 1:N
  Dtos/          contratos de entrada e saída
  Services/      IPedidoService, PedidoService e ResultadoOperacao
  Controllers/   PedidosController
  Migrations/    CriarProdutos e AdicionarPedidos
```

O controller cuida apenas do protocolo HTTP; consultas, validações e regras de
negócio ficam no serviço. `ResultadoOperacao<T>` leva a falha do serviço até o
controller, que a traduz em `400`, `404` ou `409`.

## Tickets

| Ticket | Onde foi resolvido |
| --- | --- |
| LAB-001 | `Models/Pedido.cs`, `Models/StatusPedido.cs` |
| LAB-002 | `Models/ItemPedido.cs` |
| LAB-003 | `Data/AppDbContext.cs` |
| LAB-004 | `Migrations/20260908232000_AdicionarPedidos.cs` |
| LAB-005 | `Controllers/PedidosController.cs` |
| LAB-006 | `Dtos/` |
| LAB-007 | `PedidoService.CriarAsync` |
| LAB-008 | `PedidoService.ListarAsync` e `ObterAsync` |
| LAB-009 | `PedidoService.AtualizarAsync` |
| LAB-010 | `PedidoService.DesativarAsync` |
| LAB-011 | `PedidoService.FecharAsync` |
| LAB-012 | `PedidoService.ImportarAsync` |
| LAB-013 | `PedidoService.ObterResumoAsync` |
| EXTRA-014 | transação explícita em `PedidoService.ImportarAsync` |
