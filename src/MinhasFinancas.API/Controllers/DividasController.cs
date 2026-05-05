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
    public async Task<IActionResult> ExportarPdf([FromQuery] string nomeDevedor, [FromQuery] int mes, [FromQuery] int ano, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var todas = await _mediator.Send(new ListarDividasQuery(usuarioId.Value), ct);
        var dividasBruto = todas
            .Where(d => d.NomeDevedor.Trim().Equals(nomeDevedor.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Filtra parcelas pelo mês/ano selecionado e mantém apenas dívidas com parcelas no período
        var dividas = dividasBruto
            .Select(d => d with
            {
                Parcelas = d.Parcelas
                    .Where(p => p.DataVencimento.Year == ano && p.DataVencimento.Month == mes)
                    .ToList()
            })
            .Where(d => d.Parcelas.Any())
            .ToList();

        if (!dividas.Any())
            return NotFound(new { message = "Nenhuma dívida encontrada para este período." });

        var periodo = new System.Globalization.CultureInfo("pt-BR")
            .DateTimeFormat.GetMonthName(mes) + $"/{ano}";

        var pdf = GerarPdf(nomeDevedor, dividas, periodo);
        var nomeArquivo = $"dividas_{nomeDevedor.Replace(" ", "_")}_{mes:D2}_{ano}.pdf";
        return File(pdf, "application/pdf", nomeArquivo);
    }

    private static byte[] GerarPdf(string nomeDevedor, List<DividaDto> dividas, string periodo)
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        var culture  = new System.Globalization.CultureInfo("pt-BR");
        var geradoEm = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        const string Vermelho  = "#C62828";
        const string VermClaro = "#FFEBEE";
        const string VermFnd   = "#FFEBEE";
        const string Verde     = "#2E7D32";
        const string VerdeFnd  = "#E8F5E9";
        const string Cinza     = "#ECEFF1";
        const string CinzaTxt  = "#546E7A";

        var linhas    = dividas
            .SelectMany(d => d.Parcelas.Select(p => new { d, p }))
            .OrderBy(x => x.p.DataVencimento).ThenBy(x => x.d.NomeDevedor).ThenBy(x => x.d.Descricao)
            .ToList();

        var totalMes  = linhas.Sum(x => x.p.Valor);
        var totalPago = linhas.Where(x => x.p.Paga).Sum(x => x.p.Valor);
        var emAberto  = totalMes - totalPago;

        return QuestPDF.Fluent.Document.Create(container =>
        {
            QuestPDF.Fluent.PageExtensions.Page(container, page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginLeft(45, QuestPDF.Infrastructure.Unit.Point);
                page.MarginRight(45, QuestPDF.Infrastructure.Unit.Point);
                page.MarginTop(30, QuestPDF.Infrastructure.Unit.Point);
                page.MarginBottom(30, QuestPDF.Infrastructure.Unit.Point);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(h =>
                {
                    h.Item().Background(Vermelho).Padding(12).Column(col =>
                    {
                        col.Item().Text($"Minhas Finanças — Contas a Receber  •  {periodo}")
                            .FontColor("#FFCDD2").FontSize(9).SemiBold();
                        col.Item().Text($"{nomeDevedor}  —  {linhas.Count} parcela(s)  •  Total: {totalMes.ToString("C2", culture)}  •  Em aberto: {emAberto.ToString("C2", culture)}")
                            .FontColor("#FFFFFF").FontSize(13).Bold();
                    });

                    h.Item().Background(Cinza).PaddingHorizontal(12).PaddingVertical(3)
                        .Text($"Gerado em: {geradoEm}").FontSize(7).FontColor(CinzaTxt);

                    h.Item().Background(VermClaro).PaddingVertical(8).PaddingHorizontal(30).Row(row =>
                    {
                        void Card(string valor, string label, string cor) =>
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignCenter().Text(valor).Bold().FontSize(13).FontColor(cor);
                                c.Item().AlignCenter().Text(label).FontSize(7).FontColor(CinzaTxt);
                            });

                        Card(dividas.Count.ToString(),          "Dívidas",      Vermelho);
                        Card(totalMes.ToString("C2", culture),  "Total do Mês", Vermelho);
                        Card(totalPago.ToString("C2", culture), "Já Recebido",  Verde);
                        Card(emAberto.ToString("C2", culture),  "Em Aberto",    emAberto > 0 ? Vermelho : Verde);
                    });

                    h.Item().PaddingBottom(10);
                });

                page.Content().Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(2);  // Vencimento
                            cols.RelativeColumn(3);  // Devedor
                            cols.RelativeColumn(4);  // Descrição
                            cols.RelativeColumn(1);  // Parc.
                            cols.RelativeColumn(2);  // Valor
                            cols.RelativeColumn(2);  // Status
                        });

                        static void TH(IContainer c, string t) =>
                            c.Background("#C62828").PaddingVertical(5).PaddingHorizontal(6)
                             .AlignCenter().Text(t).FontColor("#FFFFFF").Bold().FontSize(8);

                        static void THL(IContainer c, string t) =>
                            c.Background("#C62828").PaddingVertical(5).PaddingHorizontal(6)
                             .Text(t).FontColor("#FFFFFF").Bold().FontSize(8);

                        table.Header(h =>
                        {
                            h.Cell().Element(c => TH(c, "Vencimento"));
                            h.Cell().Element(c => THL(c, "Devedor"));
                            h.Cell().Element(c => THL(c, "Descrição"));
                            h.Cell().Element(c => TH(c, "Parc."));
                            h.Cell().Element(c => TH(c, "Valor"));
                            h.Cell().Element(c => TH(c, "Status"));
                        });

                        var idx = 0;
                        foreach (var x in linhas)
                        {
                            idx++;
                            string bg, tc;
                            if (x.p.Paga)         { bg = VerdeFnd; tc = Verde; }
                            else if (x.p.Vencida) { bg = VermFnd;  tc = Vermelho; }
                            else                  { bg = idx % 2 == 0 ? "#FFF5F5" : "#FFFFFF"; tc = "#212121"; }

                            string status = x.p.Paga ? "Recebido" : x.p.Vencida ? "Vencida" : "Pendente";

                            void TD(IContainer c, string t) =>
                                c.Background(bg).BorderBottom(1).BorderColor("#E0E0E0")
                                 .PaddingVertical(5).PaddingHorizontal(6)
                                 .Text(t).FontColor(tc).FontSize(8);

                            void TDC(IContainer c, string t) =>
                                c.Background(bg).BorderBottom(1).BorderColor("#E0E0E0")
                                 .PaddingVertical(5).PaddingHorizontal(6)
                                 .AlignCenter().Text(t).FontColor(tc).FontSize(8);

                            table.Cell().Element(c => TDC(c, x.p.DataVencimento.ToString("dd/MM/yyyy")));
                            table.Cell().Element(c => TD(c, x.d.NomeDevedor));
                            table.Cell().Element(c => TD(c, x.d.Descricao));
                            table.Cell().Element(c => TDC(c, $"{x.p.Numero}/{x.d.QuantidadeParcelas}"));
                            table.Cell().Background(bg).BorderBottom(1).BorderColor("#E0E0E0")
                                .PaddingVertical(5).PaddingHorizontal(6).AlignCenter()
                                .Text(x.p.Valor.ToString("C2", culture)).Bold().FontColor(tc).FontSize(8);
                            table.Cell().Element(c => TDC(c, status));
                        }

                        table.Footer(f =>
                        {
                            f.Cell().ColumnSpan(4).Background(VermClaro).PaddingVertical(6).PaddingHorizontal(6)
                                .Text("TOTAL").Bold().FontSize(9).FontColor(Vermelho);
                            f.Cell().Background(VermClaro).PaddingVertical(6).PaddingHorizontal(6)
                                .AlignCenter().Text(totalMes.ToString("C2", culture))
                                .Bold().FontSize(9).FontColor(Vermelho);
                            f.Cell().Background(VermClaro);
                        });
                    });
                });

                page.Footer().BorderTop(1).BorderColor("#CFD8DC").PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text($"Minhas Finanças — Contas a Receber  •  {periodo}")
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
                request.DataCompra,
                request.DataPrimeiraParcela);

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
            var command = new AtualizarDividaCommand(id, usuarioId.Value, request.NomeDevedor, request.Descricao, request.ValorTotal, request.QuantidadeParcelas, request.DataCompra, request.DataPrimeiraParcela);
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

public record AtualizarDividaRequest(
    string NomeDevedor,
    string Descricao,
    decimal ValorTotal,
    int QuantidadeParcelas,
    DateOnly DataCompra,
    DateOnly DataPrimeiraParcela);

public record CriarDividaRequest(
    string NomeDevedor,
    string Descricao,
    decimal ValorTotal,
    int QuantidadeParcelas,
    DateOnly DataCompra,
    DateOnly DataPrimeiraParcela);

public record MarcarParcelaDividaRequest(bool Paga, DateOnly? DataPagamento = null);
