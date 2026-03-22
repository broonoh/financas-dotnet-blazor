using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinhasFinancas.Application.Commands.Despesas;
using MinhasFinancas.Domain.Enums;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using MinhasFinancas.Domain.Interfaces;
using System.Security.Claims;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MinhasFinancas.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DespesasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IDespesaRepository _despesaRepo;

    public DespesasController(IMediator mediator, IDespesaRepository despesaRepo)
    {
        _mediator = mediator;
        _despesaRepo = despesaRepo;
    }

    [HttpGet("fixas")]
    [ProducesResponseType(typeof(IEnumerable<DespesaFixaDto>), 200)]
    public async Task<IActionResult> ListarFixas(CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var resultado = await _mediator.Send(new ListarDespesasFixasQuery(usuarioId.Value), ct);
        return Ok(resultado);
    }

    [HttpGet("extras")]
    [ProducesResponseType(typeof(IEnumerable<DespesaExtraDto>), 200)]
    public async Task<IActionResult> ListarExtras(CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var resultado = await _mediator.Send(new ListarDespesasExtrasQuery(usuarioId.Value), ct);
        return Ok(resultado);
    }

    [HttpPost("fixas")]
    [ProducesResponseType(typeof(DespesaFixaDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CriarFixa([FromBody] CriarDespesaFixaRequest request, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            var command = new CriarDespesaFixaCommand(
                usuarioId.Value,
                request.Descricao,
                request.ValorTotal,
                request.QuantidadeParcelas,
                request.DataCompra,
                request.DataPrimeiraParcela,
                request.Categoria,
                request.FormaPagamento);

            var resultado = await _mediator.Send(command, ct);
            return CreatedAtAction(nameof(CriarFixa), new { id = resultado.Id }, resultado);
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

    [HttpPost("extras")]
    [ProducesResponseType(typeof(DespesaExtraDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CriarExtra([FromBody] CriarDespesaExtraRequest request, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            var command = new CriarDespesaExtraCommand(
                usuarioId.Value,
                request.Descricao,
                request.Valor,
                request.DataDespesa,
                request.Categoria,
                request.FormaPagamento);

            var resultado = await _mediator.Send(command, ct);
            return CreatedAtAction(nameof(CriarExtra), new { id = resultado.Id }, resultado);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
    }

    [HttpPut("fixas/{id:guid}")]
    [ProducesResponseType(typeof(DespesaFixaDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AtualizarFixa(Guid id, [FromBody] AtualizarDespesaFixaRequest request, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            var command = new AtualizarDespesaFixaCommand(id, usuarioId.Value, request.Descricao, request.Categoria, request.FormaPagamento);
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

    [HttpPut("extras/{id:guid}")]
    [ProducesResponseType(typeof(DespesaExtraDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AtualizarExtra(Guid id, [FromBody] AtualizarDespesaExtraRequest request, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            var command = new AtualizarDespesaExtraCommand(id, usuarioId.Value, request.Descricao, request.Valor, request.DataDespesa, request.Categoria, request.FormaPagamento);
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

    [HttpPatch("parcelas/{parcelaId:guid}/pagar")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarcarParcela(Guid parcelaId, [FromBody] MarcarParcelaRequest request, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            await _mediator.Send(new MarcarParcelaPagaCommand(parcelaId, usuarioId.Value, request.Paga, request.DataPagamento), ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("fixas/{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ExcluirFixa(Guid id, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            await _mediator.Send(new ExcluirDespesaCommand(id, usuarioId.Value, Fixa: true), ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("extras/{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ExcluirExtra(Guid id, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            await _mediator.Send(new ExcluirDespesaCommand(id, usuarioId.Value, Fixa: false), ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("fixas/export/pdf")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ExportarFixasPdf(CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var despesas = (await _despesaRepo.ListarFixasComParcelasAsync(usuarioId.Value, ct)).ToList();
        if (!despesas.Any())
            return NotFound(new { message = "Nenhuma despesa fixa encontrada." });

        var pdf = GerarPdfFixas(despesas);
        return File(pdf, "application/pdf", "despesas_fixas.pdf");
    }

    [HttpGet("extras/export/pdf")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ExportarExtrasPdf(CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var despesas = (await _despesaRepo.ListarExtrasPorUsuarioAsync(usuarioId.Value, ct)).ToList();
        if (!despesas.Any())
            return NotFound(new { message = "Nenhuma despesa extra encontrada." });

        var pdf = GerarPdfExtras(despesas);
        return File(pdf, "application/pdf", "despesas_extras.pdf");
    }

    private static byte[] GerarPdfFixas(List<MinhasFinancas.Domain.Entities.DespesaFixa> despesas)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var culture = new System.Globalization.CultureInfo("pt-BR");

        const string Roxo       = "#5C35CC";
        const string RoxoClaro  = "#EDE7F6";
        const string Cinza      = "#ECEFF1";
        const string CinzaTexto = "#546E7A";
        const string Verde      = "#2E7D32";
        const string Vermelho   = "#C62828";

        var totalGeral  = despesas.Sum(d => d.ValorTotal);
        var totalPagas  = despesas.Sum(d => d.Parcelas.Count(p => p.Paga));
        var totalParc   = despesas.Sum(d => d.Parcelas.Count);
        var geradoEm    = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        return Document.Create(container =>
        {
            PageExtensions.Page(container, page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginLeft(40, QuestPDF.Infrastructure.Unit.Point);
                page.MarginRight(40, QuestPDF.Infrastructure.Unit.Point);
                page.MarginTop(25, QuestPDF.Infrastructure.Unit.Point);
                page.MarginBottom(25, QuestPDF.Infrastructure.Unit.Point);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(h =>
                {
                    h.Item().Background(Roxo).Padding(12).Column(col =>
                    {
                        col.Item().Text("Minhas Finanças — Relatório de Despesas Fixas")
                            .FontColor("#D1C4E9").FontSize(9).SemiBold();
                        col.Item().Text($"Total: {totalGeral.ToString("C2", culture)}  •  {despesas.Count} despesa(s)  •  {totalPagas}/{totalParc} parcelas pagas")
                            .FontColor("#FFFFFF").FontSize(13).Bold();
                    });

                    h.Item().Background(Cinza).PaddingHorizontal(12).PaddingVertical(3)
                        .Text($"Gerado em: {geradoEm}").FontSize(7).FontColor(CinzaTexto);

                    h.Item().Background(RoxoClaro).PaddingVertical(8).PaddingHorizontal(30).Row(row =>
                    {
                        void Card(string valor, string label, string cor) =>
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignCenter().Text(valor).Bold().FontSize(13).FontColor(cor);
                                c.Item().AlignCenter().Text(label).FontSize(7).FontColor(CinzaTexto);
                            });

                        Card(despesas.Count.ToString(),                  "Despesas",        Roxo);
                        Card(totalGeral.ToString("C2", culture),         "Valor Total",     Roxo);
                        Card($"{totalPagas}/{totalParc}",                "Parcelas Pagas",  totalPagas == totalParc ? Verde : Roxo);
                        Card(despesas.Sum(d => d.Parcelas.Count(p => !p.Paga)).ToString(), "Em Aberto", Vermelho);
                    });

                    h.Item().PaddingBottom(8);
                });

                page.Content().Column(col =>
                {
                    foreach (var d in despesas)
                    {
                        var parcelasTotal = d.Parcelas.Count;
                        var parcelasPagas = d.Parcelas.Count(p => p.Paga);
                        var pct = parcelasTotal > 0 ? (decimal)parcelasPagas / parcelasTotal * 100m : 0;

                        col.Item().PaddingBottom(10).Border(1).BorderColor("#CFD8DC").Column(inner =>
                        {
                            inner.Item().Background(Cinza).PaddingHorizontal(10).PaddingTop(6).PaddingBottom(2)
                                .Row(r =>
                                {
                                    r.RelativeItem().Text(d.Descricao).Bold().FontSize(11);
                                    r.RelativeItem().Column(c =>
                                        c.Item().AlignRight().Text(d.ValorTotal.ToString("C2", culture))
                                            .Bold().FontSize(11).FontColor(Roxo));
                                });

                            inner.Item().Background(Cinza).PaddingHorizontal(10).PaddingBottom(5)
                                .Row(r =>
                                {
                                    r.RelativeItem().Text(
                                        $"Compra: {d.DataCompra:dd/MM/yyyy}   •   " +
                                        $"1ª Parcela: {d.DataPrimeiraParcela:dd/MM/yyyy}   •   " +
                                        $"{d.QuantidadeParcelas} parcela(s)   •   " +
                                        $"Categoria: {d.Categoria}   •   " +
                                        $"Quitado: {pct:F0}%")
                                        .FontSize(8).FontColor(CinzaTexto);
                                    r.RelativeItem().Column(c =>
                                        c.Item().AlignRight()
                                            .Text($"{parcelasPagas}/{parcelasTotal} pagas")
                                            .FontSize(8).FontColor(parcelasPagas == parcelasTotal ? Verde : Vermelho).SemiBold());
                                });

                            inner.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(1);  // #
                                    cols.RelativeColumn(3);  // Vencimento
                                    cols.RelativeColumn(3);  // Valor
                                    cols.RelativeColumn(2);  // Status
                                });

                                static void TH(IContainer c, string t) =>
                                    c.Background("#5C35CC").PaddingVertical(4).PaddingHorizontal(6)
                                     .AlignCenter().Text(t).FontColor("#FFFFFF").Bold().FontSize(8);

                                table.Header(h =>
                                {
                                    h.Cell().Element(c => TH(c, "#"));
                                    h.Cell().Element(c => TH(c, "Vencimento"));
                                    h.Cell().Element(c => TH(c, "Valor"));
                                    h.Cell().Element(c => TH(c, "Status"));
                                });

                                var idx = 0;
                                foreach (var p in d.Parcelas.OrderBy(x => x.Numero))
                                {
                                    idx++;
                                    string bg, tc;
                                    if (p.Paga)          { bg = "#E8F5E9"; tc = Verde; }
                                    else if (p.DataVencimento < DateOnly.FromDateTime(DateTime.UtcNow))
                                                          { bg = "#FFEBEE"; tc = Vermelho; }
                                    else                  { bg = idx % 2 == 0 ? "#F5F5F5" : "#FFFFFF"; tc = "#212121"; }

                                    void TD(IContainer c, string t) =>
                                        c.Background(bg).BorderBottom(1).BorderColor("#E0E0E0")
                                         .PaddingVertical(4).PaddingHorizontal(6)
                                         .AlignCenter().Text(t).FontColor(tc).FontSize(8);

                                    table.Cell().Element(c => TD(c, p.Numero.ToString()));
                                    table.Cell().Element(c => TD(c, p.DataVencimento.ToString("dd/MM/yyyy")));
                                    table.Cell().Element(c => TD(c, p.Valor.ToString("C2", culture)));
                                    table.Cell().Element(c => TD(c, p.Paga ? "✓ Paga" : "Pendente"));
                                }
                            });
                        });
                    }
                });

                page.Footer().BorderTop(1).BorderColor("#CFD8DC").PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text("Minhas Finanças — Relatório de Despesas Fixas").FontSize(7).FontColor("#9E9E9E");
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

    private static byte[] GerarPdfExtras(List<MinhasFinancas.Domain.Entities.DespesaExtra> despesas)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var culture = new System.Globalization.CultureInfo("pt-BR");

        const string Laranja     = "#E65100";
        const string LaranjaClaro = "#FFF3E0";
        const string Cinza       = "#ECEFF1";
        const string CinzaTexto  = "#546E7A";

        var totalGeral = despesas.Sum(d => d.ValorTotal);
        var geradoEm   = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        return Document.Create(container =>
        {
            PageExtensions.Page(container, page =>
            {
                page.Size(PageSizes.A4);
                page.MarginLeft(40, QuestPDF.Infrastructure.Unit.Point);
                page.MarginRight(40, QuestPDF.Infrastructure.Unit.Point);
                page.MarginTop(25, QuestPDF.Infrastructure.Unit.Point);
                page.MarginBottom(25, QuestPDF.Infrastructure.Unit.Point);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(h =>
                {
                    h.Item().Background(Laranja).Padding(12).Column(col =>
                    {
                        col.Item().Text("Minhas Finanças — Relatório de Despesas Extras")
                            .FontColor("#FFE0B2").FontSize(9).SemiBold();
                        col.Item().Text($"Total: {totalGeral.ToString("C2", culture)}  •  {despesas.Count} despesa(s)")
                            .FontColor("#FFFFFF").FontSize(13).Bold();
                    });

                    h.Item().Background(Cinza).PaddingHorizontal(12).PaddingVertical(3)
                        .Text($"Gerado em: {geradoEm}").FontSize(7).FontColor(CinzaTexto);

                    h.Item().Background(LaranjaClaro).PaddingVertical(8).PaddingHorizontal(30).Row(row =>
                    {
                        void Card(string valor, string label) =>
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignCenter().Text(valor).Bold().FontSize(13).FontColor(Laranja);
                                c.Item().AlignCenter().Text(label).FontSize(7).FontColor(CinzaTexto);
                            });

                        Card(despesas.Count.ToString(),          "Despesas");
                        Card(totalGeral.ToString("C2", culture), "Valor Total");
                        Card(despesas.Select(d => d.Categoria).Distinct().Count().ToString(), "Categorias");
                    });

                    h.Item().PaddingBottom(8);
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(4);  // Descrição
                        cols.RelativeColumn(2);  // Data
                        cols.RelativeColumn(2);  // Valor
                        cols.RelativeColumn(2);  // Categoria
                        cols.RelativeColumn(2);  // Forma Pagamento
                    });

                    static void TH(IContainer c, string t) =>
                        c.Background("#E65100").PaddingVertical(5).PaddingHorizontal(6)
                         .AlignCenter().Text(t).FontColor("#FFFFFF").Bold().FontSize(8);

                    table.Header(h =>
                    {
                        h.Cell().Element(c => TH(c, "Descrição"));
                        h.Cell().Element(c => TH(c, "Data"));
                        h.Cell().Element(c => TH(c, "Valor"));
                        h.Cell().Element(c => TH(c, "Categoria"));
                        h.Cell().Element(c => TH(c, "Pagamento"));
                    });

                    var idx = 0;
                    foreach (var d in despesas.OrderByDescending(x => x.DataDespesa))
                    {
                        idx++;
                        var bg = idx % 2 == 0 ? "#F5F5F5" : "#FFFFFF";

                        void TD(IContainer c, string t, bool bold = false) =>
                            c.Background(bg).BorderBottom(1).BorderColor("#E0E0E0")
                             .PaddingVertical(5).PaddingHorizontal(6)
                             .AlignCenter().Text(t).FontColor("#212121")
                             .FontSize(8);

                        table.Cell().Background(bg).BorderBottom(1).BorderColor("#E0E0E0")
                            .PaddingVertical(5).PaddingHorizontal(6)
                            .Text(d.Descricao).FontSize(8);
                        table.Cell().Element(c => TD(c, d.DataDespesa.ToString("dd/MM/yyyy")));
                        table.Cell().Background(bg).BorderBottom(1).BorderColor("#E0E0E0")
                            .PaddingVertical(5).PaddingHorizontal(6)
                            .AlignCenter().Text(d.ValorTotal.ToString("C2", culture))
                            .FontSize(8).Bold().FontColor(Laranja);
                        table.Cell().Element(c => TD(c, d.Categoria));
                        table.Cell().Element(c => TD(c, d.FormaPagamento.ToString()));
                    }

                    // Linha de total
                    table.Footer(f =>
                    {
                        f.Cell().ColumnSpan(2).Background("#FFF3E0").PaddingVertical(6).PaddingHorizontal(6)
                            .Text("TOTAL").Bold().FontSize(9).FontColor(Laranja);
                        f.Cell().Background("#FFF3E0").PaddingVertical(6).PaddingHorizontal(6)
                            .AlignCenter().Text(totalGeral.ToString("C2", culture))
                            .Bold().FontSize(9).FontColor(Laranja);
                        f.Cell().ColumnSpan(2).Background("#FFF3E0");
                    });
                });

                page.Footer().BorderTop(1).BorderColor("#CFD8DC").PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text("Minhas Finanças — Relatório de Despesas Extras").FontSize(7).FontColor("#9E9E9E");
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

    private Guid? ObterUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

// Request DTOs (separados do domínio)
public record AtualizarDespesaFixaRequest(
    string Descricao,
    string Categoria,
    FormaPagamentoDespesaFixa FormaPagamento);

public record AtualizarDespesaExtraRequest(
    string Descricao,
    decimal Valor,
    DateOnly DataDespesa,
    string Categoria,
    FormaPagamentoDespesaExtra FormaPagamento);

public record CriarDespesaFixaRequest(
    string Descricao,
    decimal ValorTotal,
    int QuantidadeParcelas,
    DateOnly DataCompra,
    DateOnly DataPrimeiraParcela,
    string Categoria,
    FormaPagamentoDespesaFixa FormaPagamento);

public record CriarDespesaExtraRequest(
    string Descricao,
    decimal Valor,
    DateOnly DataDespesa,
    string Categoria,
    FormaPagamentoDespesaExtra FormaPagamento);

public record MarcarParcelaRequest(bool Paga, DateOnly? DataPagamento = null);
