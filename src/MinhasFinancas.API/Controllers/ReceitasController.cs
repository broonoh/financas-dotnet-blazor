using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinhasFinancas.Application.Commands.Receitas;
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
public class ReceitasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IReceitaRepository _receitaRepo;

    public ReceitasController(IMediator mediator, IReceitaRepository receitaRepo)
    {
        _mediator = mediator;
        _receitaRepo = receitaRepo;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReceitaDto>), 200)]
    public async Task<IActionResult> Listar([FromQuery] int? ano, [FromQuery] int? mes, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var resultado = await _mediator.Send(new ListarReceitasQuery(usuarioId.Value, ano, mes), ct);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReceitaDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Criar([FromBody] CriarReceitaRequest request, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            var command = new CriarReceitaCommand(
                usuarioId.Value,
                request.Descricao,
                request.Valor,
                request.DataRecebimento,
                request.Categoria,
                request.Recorrente);

            var resultado = await _mediator.Send(command, ct);
            return CreatedAtAction(nameof(Criar), new { id = resultado.Id }, resultado);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ReceitaDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarReceitaRequest request, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            var command = new AtualizarReceitaCommand(id, usuarioId.Value, request.Descricao, request.Valor, request.DataRecebimento, request.Categoria);
            var resultado = await _mediator.Send(command, ct);
            return Ok(resultado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
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
            await _mediator.Send(new ExcluirReceitaCommand(id, usuarioId.Value), ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("export/pdf")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ExportarPdf(CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var receitas = (await _receitaRepo.ListarPorUsuarioAsync(usuarioId.Value, ct)).ToList();
        if (!receitas.Any())
            return NotFound(new { message = "Nenhuma receita encontrada." });

        var pdf = GerarPdfReceitas(receitas);
        return File(pdf, "application/pdf", "receitas.pdf");
    }

    private static byte[] GerarPdfReceitas(List<MinhasFinancas.Domain.Entities.Receita> receitas)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var culture = new System.Globalization.CultureInfo("pt-BR");

        const string Verde      = "#2E7D32";
        const string VerdeClaro = "#E8F5E9";
        const string Cinza      = "#ECEFF1";
        const string CinzaTexto = "#546E7A";

        var totalGeral   = receitas.Sum(r => r.Valor);
        var recorrentes  = receitas.Count(r => r.Recorrente);
        var geradoEm     = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

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
                    h.Item().Background(Verde).Padding(12).Column(col =>
                    {
                        col.Item().Text("Minhas Finanças — Relatório de Receitas")
                            .FontColor("#C8E6C9").FontSize(9).SemiBold();
                        col.Item().Text($"Total: {totalGeral.ToString("C2", culture)}  •  {receitas.Count} receita(s)")
                            .FontColor("#FFFFFF").FontSize(13).Bold();
                    });

                    h.Item().Background(Cinza).PaddingHorizontal(12).PaddingVertical(3)
                        .Text($"Gerado em: {geradoEm}").FontSize(7).FontColor(CinzaTexto);

                    h.Item().Background(VerdeClaro).PaddingVertical(8).PaddingHorizontal(30).Row(row =>
                    {
                        void Card(string valor, string label) =>
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignCenter().Text(valor).Bold().FontSize(13).FontColor(Verde);
                                c.Item().AlignCenter().Text(label).FontSize(7).FontColor(CinzaTexto);
                            });

                        Card(receitas.Count.ToString(),                                   "Total");
                        Card(totalGeral.ToString("C2", culture),                          "Valor Total");
                        Card(recorrentes.ToString(),                                       "Recorrentes");
                        Card(receitas.Select(r => r.Categoria).Distinct().Count().ToString(), "Categorias");
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
                        cols.RelativeColumn(1);  // Recorrente
                    });

                    static void TH(IContainer c, string t) =>
                        c.Background("#2E7D32").PaddingVertical(5).PaddingHorizontal(6)
                         .AlignCenter().Text(t).FontColor("#FFFFFF").Bold().FontSize(8);

                    table.Header(h =>
                    {
                        h.Cell().Element(c => TH(c, "Descrição"));
                        h.Cell().Element(c => TH(c, "Data"));
                        h.Cell().Element(c => TH(c, "Valor"));
                        h.Cell().Element(c => TH(c, "Categoria"));
                        h.Cell().Element(c => TH(c, "Recorrente"));
                    });

                    var idx = 0;
                    foreach (var r in receitas.OrderByDescending(x => x.DataRecebimento))
                    {
                        idx++;
                        var bg = idx % 2 == 0 ? "#F5F5F5" : "#FFFFFF";

                        void TD(IContainer c, string t) =>
                            c.Background(bg).BorderBottom(1).BorderColor("#E0E0E0")
                             .PaddingVertical(5).PaddingHorizontal(6)
                             .AlignCenter().Text(t).FontColor("#212121").FontSize(8);

                        table.Cell().Background(bg).BorderBottom(1).BorderColor("#E0E0E0")
                            .PaddingVertical(5).PaddingHorizontal(6)
                            .Text(r.Descricao).FontSize(8);
                        table.Cell().Element(c => TD(c, r.DataRecebimento.ToString("dd/MM/yyyy")));
                        table.Cell().Background(bg).BorderBottom(1).BorderColor("#E0E0E0")
                            .PaddingVertical(5).PaddingHorizontal(6)
                            .AlignCenter().Text(r.Valor.ToString("C2", culture))
                            .FontSize(8).Bold().FontColor(Verde);
                        table.Cell().Element(c => TD(c, r.Categoria));
                        table.Cell().Element(c => TD(c, r.Recorrente ? "Sim" : "Nao"));
                    }

                    table.Footer(f =>
                    {
                        f.Cell().ColumnSpan(2).Background(VerdeClaro).PaddingVertical(6).PaddingHorizontal(6)
                            .Text("TOTAL").Bold().FontSize(9).FontColor(Verde);
                        f.Cell().Background(VerdeClaro).PaddingVertical(6).PaddingHorizontal(6)
                            .AlignCenter().Text(totalGeral.ToString("C2", culture))
                            .Bold().FontSize(9).FontColor(Verde);
                        f.Cell().ColumnSpan(2).Background(VerdeClaro);
                    });
                });

                page.Footer().BorderTop(1).BorderColor("#CFD8DC").PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text("Minhas Finanças — Relatório de Receitas").FontSize(7).FontColor("#9E9E9E");
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

public record AtualizarReceitaRequest(
    string Descricao,
    decimal Valor,
    DateOnly DataRecebimento,
    string Categoria);

public record CriarReceitaRequest(
    string Descricao,
    decimal Valor,
    DateOnly DataRecebimento,
    string Categoria,
    bool Recorrente = false);
