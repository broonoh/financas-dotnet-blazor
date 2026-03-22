using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinhasFinancas.Application.Commands.Dividas;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using System.Security.Claims;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MinhasFinancas.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DividasController : ControllerBase
{
    private readonly IMediator _mediator;

    public DividasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DividaDto>), 200)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var resultado = await _mediator.Send(new ListarDividasQuery(usuarioId.Value), ct);
        return Ok(resultado);
    }

    [HttpGet("export/pdf")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ExportarPdf([FromQuery] string nomeDevedor, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var todas = await _mediator.Send(new ListarDividasQuery(usuarioId.Value), ct);
        var dividas = todas
            .Where(d => d.NomeDevedor.Trim().Equals(nomeDevedor.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!dividas.Any())
            return NotFound(new { message = "Nenhuma dívida encontrada para esta pessoa." });

        var pdf = GerarPdf(nomeDevedor, dividas);
        var nomeArquivo = $"dividas_{nomeDevedor.Replace(" ", "_")}.pdf";
        return File(pdf, "application/pdf", nomeArquivo);
    }

    private static byte[] GerarPdf(string nomeDevedor, List<DividaDto> dividas)
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        var culture       = new System.Globalization.CultureInfo("pt-BR");
        var totalGeral    = dividas.Sum(d => d.ValorTotal);
        var saldoGeral    = dividas.Sum(d => d.SaldoRestante);
        var pagas         = dividas.Sum(d => d.Parcelas.Count(p => p.Paga));
        var totalParcelas = dividas.Sum(d => d.Parcelas.Count());
        var geradoEm      = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        // Cores
        const string Azul    = "#1565C0";
        const string AzulClaro = "#E3F2FD";
        const string Cinza   = "#ECEFF1";
        const string CinzaTexto = "#546E7A";
        const string Verde   = "#2E7D32";
        const string Vermelho = "#C62828";

        return QuestPDF.Fluent.Document.Create(container =>
        {
            QuestPDF.Fluent.PageExtensions.Page(container, page =>
            {
                page.Size(PageSizes.A4.Landscape());
                // Margens simétricas generosas — todo conteúdo fica dentro
                page.MarginLeft(45, QuestPDF.Infrastructure.Unit.Point);
                page.MarginRight(45, QuestPDF.Infrastructure.Unit.Point);
                page.MarginTop(30, QuestPDF.Infrastructure.Unit.Point);
                page.MarginBottom(30, QuestPDF.Infrastructure.Unit.Point);
                page.DefaultTextStyle(x => x.FontSize(9));

                // ── HEADER ──────────────────────────────────────────────
                // "Gerado em" é uma linha SEPARADA abaixo do banner azul
                // evitando completamente o problema de AlignRight dentro de Row
                page.Header().Column(h =>
                {
                    // Banner azul — somente título, sem colunas à direita
                    h.Item().Background(Azul).Padding(12).Column(col =>
                    {
                        col.Item().Text("Minhas Finanças — Relatório de Contas a Receber")
                            .FontColor("#B3C9F4").FontSize(9).SemiBold();
                        col.Item().Text(nomeDevedor)
                            .FontColor("#FFFFFF").FontSize(18).Bold();
                    });

                    // "Gerado em" em linha própria, fora do banner
                    h.Item().Background(Cinza).PaddingHorizontal(12).PaddingVertical(3)
                        .Text($"Gerado em: {geradoEm}")
                        .FontSize(7).FontColor(CinzaTexto);

                    // Barra de resumo — 4 cards com RelativeItem idênticos
                    h.Item().Background(AzulClaro).PaddingVertical(8).PaddingHorizontal(30).Row(row =>
                    {
                        void Card(string valor, string label, string cor)
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignCenter().Text(valor)
                                    .Bold().FontSize(14).FontColor(cor);
                                c.Item().AlignCenter().Text(label)
                                    .FontSize(7).FontColor(CinzaTexto);
                            });
                        }

                        Card(dividas.Count.ToString(),                      "Dívidas",         Azul);
                        Card(totalGeral.ToString("C2", culture),            "Valor Total",     Azul);
                        Card(saldoGeral.ToString("C2", culture),            "Saldo Restante",  saldoGeral > 0 ? Vermelho : Verde);
                        Card($"{pagas}/{totalParcelas}",                    "Parcelas Pagas",  Azul);
                    });

                    h.Item().PaddingBottom(10);
                });

                // ── CONTENT ─────────────────────────────────────────────
                page.Content().Column(col =>
                {
                    foreach (var divida in dividas)
                    {
                        var pct = divida.ValorTotal > 0
                            ? (divida.ValorTotal - divida.SaldoRestante) / divida.ValorTotal * 100m
                            : 100m;
                        var statusCor = divida.SaldoRestante == 0 ? Verde : Vermelho;

                        col.Item().PaddingBottom(10).Border(1).BorderColor("#CFD8DC").Column(inner =>
                        {
                            // ── Título da dívida ────────────────────────
                            // Linha 1: descrição (esquerda) e valor total (direita)
                            inner.Item().Background(Cinza).PaddingHorizontal(10).PaddingTop(6).PaddingBottom(2)
                                .Row(r =>
                            {
                                r.RelativeItem().Text(divida.Descricao).Bold().FontSize(12);
                                // Texto direito: colocamos no mesmo RelativeItem mas alinhado à direita
                                // usando Column com AlignRight no texto — sem AlignRight no container
                                r.RelativeItem().Column(c =>
                                {
                                    c.Item().AlignRight()
                                        .Text(divida.ValorTotal.ToString("C2", culture))
                                        .Bold().FontSize(12).FontColor(Azul);
                                });
                            });

                            // Linha 2: metadados e saldo
                            inner.Item().Background(Cinza).PaddingHorizontal(10).PaddingBottom(5)
                                .Row(r =>
                            {
                                r.RelativeItem().Text(
                                    $"Compra: {divida.DataCompra:dd/MM/yyyy}   •   " +
                                    $"{divida.QuantidadeParcelas} parcela(s)   •   " +
                                    $"Quitado: {pct:F0}%")
                                    .FontSize(8).FontColor(CinzaTexto);
                                r.RelativeItem().Column(c =>
                                {
                                    c.Item().AlignRight()
                                        .Text($"Saldo: {divida.SaldoRestante.ToString("C2", culture)}")
                                        .FontSize(8).FontColor(statusCor).SemiBold();
                                });
                            });

                            // ── Tabela de parcelas ───────────────────────
                            // Sem PaddingHorizontal extra — tabela usa toda a largura do card
                            inner.Item().PaddingTop(0).Table(table =>
                            {
                                // Proporções fixas que somam exatamente 100% da largura disponível
                                // Colunas: # | Vencimento | Valor | Paga | Pago em
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(1);   // #
                                    cols.RelativeColumn(3);   // Vencimento
                                    cols.RelativeColumn(3);   // Valor
                                    cols.RelativeColumn(2);   // Paga
                                    cols.RelativeColumn(3);   // Pago em
                                });

                                // Cabeçalho
                                static void TH(IContainer c, string t) =>
                                    c.Background(Azul).PaddingVertical(5).PaddingHorizontal(6)
                                     .AlignCenter().Text(t)
                                     .FontColor("#FFFFFF").Bold().FontSize(8);

                                table.Header(h =>
                                {
                                    h.Cell().Element(c => TH(c, "#"));
                                    h.Cell().Element(c => TH(c, "Vencimento"));
                                    h.Cell().Element(c => TH(c, "Valor"));
                                    h.Cell().Element(c => TH(c, "Paga"));
                                    h.Cell().Element(c => TH(c, "Pago em"));
                                });

                                // Dados — TODOS centralizados para ficarem sob o cabeçalho
                                var rowIdx = 0;
                                foreach (var p in divida.Parcelas.OrderBy(x => x.Numero))
                                {
                                    rowIdx++;
                                    string bg, tc;
                                    if (p.Paga)
                                    { bg = "#E8F5E9"; tc = Verde; }
                                    else if (p.Vencida)
                                    { bg = "#FFEBEE"; tc = Vermelho; }
                                    else
                                    { bg = rowIdx % 2 == 0 ? "#F5F5F5" : "#FFFFFF"; tc = "#212121"; }

                                    // Célula padrão — centralizada
                                    void TD(IContainer c, string t) =>
                                        c.Background(bg)
                                         .BorderBottom(1).BorderColor("#E0E0E0")
                                         .PaddingVertical(5).PaddingHorizontal(6)
                                         .AlignCenter().Text(t)
                                         .FontColor(tc).FontSize(8);

                                    table.Cell().Element(c => TD(c, p.Numero.ToString()));
                                    table.Cell().Element(c => TD(c, p.DataVencimento.ToString("dd/MM/yyyy")));
                                    table.Cell().Element(c => TD(c, p.Valor.ToString("C2", culture)));
                                    table.Cell().Element(c => TD(c, p.Paga ? "✓ Sim" : "Não"));
                                    table.Cell().Element(c => TD(c, p.DataPagamento?.ToString("dd/MM/yyyy") ?? "—"));
                                }
                            });
                        });
                    }
                });

                // ── FOOTER ──────────────────────────────────────────────
                page.Footer().BorderTop(1).BorderColor("#CFD8DC").PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text("Minhas Finanças — Relatório de Dívidas")
                        .FontSize(7).FontColor("#9E9E9E");
                    r.RelativeItem().AlignRight().Text(x =>
                    {
                        x.Span("Página ").FontSize(7).FontColor("#9E9E9E");
                        x.CurrentPageNumber().FontSize(7).FontColor("#9E9E9E");
                        x.Span(" de ").FontSize(7).FontColor("#9E9E9E");
                        x.TotalPages().FontSize(7).FontColor("#9E9E9E");
                    });
                });
            });
        }).GeneratePdf();
    }

    [HttpPost]
    [ProducesResponseType(typeof(DividaDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Criar([FromBody] CriarDividaRequest request, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            var command = new CriarDividaCommand(
                usuarioId.Value,
                request.NomeDevedor,
                request.Descricao,
                request.ValorTotal,
                request.QuantidadeParcelas,
                request.DataCompra);

            var resultado = await _mediator.Send(command, ct);
            return CreatedAtAction(nameof(Criar), new { id = resultado.Id }, resultado);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DividaDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarDividaRequest request, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            var command = new AtualizarDividaCommand(id, usuarioId.Value, request.NomeDevedor, request.Descricao);
            var resultado = await _mediator.Send(command, ct);
            return Ok(resultado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            await _mediator.Send(new ExcluirDividaCommand(id, usuarioId.Value), ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("parcelas/{parcelaId:guid}/pagar")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarcarParcela(Guid parcelaId, [FromBody] MarcarParcelaDividaRequest request, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            await _mediator.Send(new MarcarParcelaDividaPagaCommand(parcelaId, usuarioId.Value, request.Paga, request.DataPagamento), ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private Guid? ObterUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

public record AtualizarDividaRequest(string NomeDevedor, string Descricao);

public record CriarDividaRequest(
    string NomeDevedor,
    string Descricao,
    decimal ValorTotal,
    int QuantidadeParcelas,
    DateOnly DataCompra);

public record MarcarParcelaDividaRequest(bool Paga, DateOnly? DataPagamento = null);
