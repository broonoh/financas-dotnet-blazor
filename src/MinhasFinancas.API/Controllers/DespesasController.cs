using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinhasFinancas.Application.Commands.Despesas;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using System.Security.Claims;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.Extensions.Hosting;

namespace MinhasFinancas.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DespesasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _env;

    public DespesasController(IMediator mediator, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _env = env;
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
                request.ValorParcela,
                request.QuantidadeParcelas,
                request.DataCompra,
                request.DataPrimeiraParcela,
                request.Categoria,
                request.FormaPagamento,
                request.CredorId);

            var resultado = await _mediator.Send(command, ct);
            return CreatedAtAction(nameof(CriarFixa), new { id = resultado.Id }, resultado);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
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
                request.FormaPagamento,
                request.PagaEm,
                request.CredorId);

            var resultado = await _mediator.Send(command, ct);
            return CreatedAtAction(nameof(CriarExtra), new { id = resultado.Id }, resultado);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
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
            var command = new AtualizarDespesaFixaCommand(id, usuarioId.Value, request.Descricao, request.ValorParcela, request.QuantidadeParcelas, request.DataCompra, request.DataPrimeiraParcela, request.Categoria, request.FormaPagamento, request.CredorId);
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
            var command = new AtualizarDespesaExtraCommand(id, usuarioId.Value, request.Descricao, request.Valor, request.DataDespesa, request.Categoria, request.FormaPagamento, request.PagaEm, request.CredorId);
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

    [HttpPatch("fixas/parcelas/pagar-mes")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> MarcarTodasFixasDoMesPagas([FromQuery] int mes, [FromQuery] int ano, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var quantidade = await _mediator.Send(new MarcarTodasParcelasFixasPagasDoMesCommand(usuarioId.Value, ano, mes), ct);
        return Ok(new { quantidade });
    }

    [HttpPatch("extras/pagar-mes")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> MarcarTodasExtrasDoMesPagas([FromQuery] int mes, [FromQuery] int ano, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var quantidade = await _mediator.Send(new MarcarTodasDespesasExtrasPagasDoMesCommand(usuarioId.Value, ano, mes), ct);
        return Ok(new { quantidade });
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

    [HttpPatch("extras/{id:guid}/pagar")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarcarExtraPaga(Guid id, [FromBody] MarcarDespesaExtraRequest request, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            await _mediator.Send(new MarcarDespesaExtraPagaCommand(id, usuarioId.Value, request.Paga), ct);
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
    public async Task<IActionResult> ExportarFixasPdf([FromQuery] int mes, [FromQuery] int ano, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var todas = (await _mediator.Send(new ListarDespesasFixasQuery(usuarioId.Value), ct)).ToList();
        var despesas = (mes > 0 && ano > 0)
            ? todas
                .Select(d => d with { Parcelas = d.Parcelas.Where(p => p.DataVencimento.Year == ano && p.DataVencimento.Month == mes).ToList() })
                .Where(d => d.Parcelas.Any())
                .ToList()
            : todas;

        if (!despesas.Any())
            return NotFound(new { message = "Nenhuma despesa fixa encontrada para este período." });

        try
        {
            var pdf = GerarPdfFixas(despesas, mes, ano);
            return File(pdf, "application/pdf", "despesas_fixas.pdf");
        }
        catch (Exception ex) when (_env.IsDevelopment())
        {
            return StatusCode(500, new { message = ex.Message, type = ex.GetType().Name, stack = ex.StackTrace });
        }
    }

    [HttpGet("extras/export/pdf")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ExportarExtrasPdf([FromQuery] int mes, [FromQuery] int ano, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var todas = (await _mediator.Send(new ListarDespesasExtrasQuery(usuarioId.Value), ct)).ToList();
        var despesas = (mes > 0 && ano > 0)
            ? todas.Where(d => { var dt = d.PagaEm ?? d.DataDespesa; return dt.Year == ano && dt.Month == mes; }).ToList()
            : todas;

        if (!despesas.Any())
            return NotFound(new { message = "Nenhuma despesa extra encontrada para este período." });

        var pdf = GerarPdfExtras(despesas, mes, ano);
        return File(pdf, "application/pdf", "despesas_extras.pdf");
    }

    private static byte[] GerarPdfFixas(List<DespesaFixaDto> despesas, int mes, int ano)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var culture = new System.Globalization.CultureInfo("pt-BR");

        const string Roxo      = "#5C35CC";
        const string RoxoClaro = "#EDE7F6";
        const string Cinza     = "#ECEFF1";
        const string CinzaTxt  = "#546E7A";
        const string Verde     = "#2E7D32";
        const string VerdeFnd  = "#E8F5E9";
        const string Vermelho  = "#C62828";
        const string VermFnd   = "#FFEBEE";

        var periodo  = culture.DateTimeFormat.GetMonthName(mes) + $"/{ano}";
        var geradoEm = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        var linhas = despesas
            .SelectMany(d => d.Parcelas.Select(p => new { d, p }))
            .OrderBy(x => x.p.DataVencimento).ThenBy(x => x.d.Descricao)
            .ToList();

        var totalMes  = linhas.Sum(x => x.p.Valor);
        var totalPago = linhas.Where(x => x.p.Paga).Sum(x => x.p.Valor);
        var emAberto  = totalMes - totalPago;

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
                        col.Item().Text($"Minhas Finanças — Despesas Fixas  •  {periodo}")
                            .FontColor("#D1C4E9").FontSize(9).SemiBold();
                        col.Item().Text($"{linhas.Count} parcela(s)  •  Total: {totalMes.ToString("C2", culture)}  •  Em aberto: {emAberto.ToString("C2", culture)}")
                            .FontColor("#FFFFFF").FontSize(13).Bold();
                    });

                    h.Item().Background(Cinza).PaddingHorizontal(12).PaddingVertical(3)
                        .Text($"Gerado em: {geradoEm}").FontSize(7).FontColor(CinzaTxt);

                    h.Item().Background(RoxoClaro).PaddingVertical(8).PaddingHorizontal(30).Row(row =>
                    {
                        void Card(string valor, string label, string cor) =>
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignCenter().Text(valor).Bold().FontSize(13).FontColor(cor);
                                c.Item().AlignCenter().Text(label).FontSize(7).FontColor(CinzaTxt);
                            });

                        Card(despesas.Count.ToString(),           "Despesas",     Roxo);
                        Card(totalMes.ToString("C2", culture),    "Total do Mês", Roxo);
                        Card(totalPago.ToString("C2", culture),   "Já Pago",      Verde);
                        Card(emAberto.ToString("C2", culture),    "Em Aberto",    emAberto > 0 ? Vermelho : Verde);
                    });

                    h.Item().PaddingBottom(8);
                });

                page.Content().Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(2);  // Vencimento
                            cols.RelativeColumn(2);  // Dt. Compra
                            cols.RelativeColumn(3);  // Descrição
                            cols.RelativeColumn(1);  // Parc.
                            cols.RelativeColumn(2);  // Categoria
                            cols.RelativeColumn(2);  // Forma Pgto
                            cols.RelativeColumn(2);  // Credor
                            cols.RelativeColumn(2);  // Valor
                            cols.RelativeColumn(2);  // Status
                        });

                        static void TH(IContainer c, string t) =>
                            c.Background("#5C35CC").PaddingVertical(5).PaddingHorizontal(6)
                             .AlignCenter().Text(t).FontColor("#FFFFFF").Bold().FontSize(8);

                        static void THL(IContainer c, string t) =>
                            c.Background("#5C35CC").PaddingVertical(5).PaddingHorizontal(6)
                             .Text(t).FontColor("#FFFFFF").Bold().FontSize(8);

                        table.Header(h =>
                        {
                            h.Cell().Element(c => TH(c, "Vencimento"));
                            h.Cell().Element(c => TH(c, "Dt. Compra"));
                            h.Cell().Element(c => THL(c, "Descrição"));
                            h.Cell().Element(c => TH(c, "Parc."));
                            h.Cell().Element(c => THL(c, "Categoria"));
                            h.Cell().Element(c => THL(c, "Forma Pgto"));
                            h.Cell().Element(c => THL(c, "Credor"));
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
                            else                  { bg = idx % 2 == 0 ? "#F3EEF9" : "#FFFFFF"; tc = "#212121"; }

                            string status = x.p.Paga ? "Pago" : x.p.Vencida ? "Vencida" : "Pendente";

                            void TD(IContainer c, string t) =>
                                c.Background(bg).BorderBottom(1).BorderColor("#E0E0E0")
                                 .PaddingVertical(5).PaddingHorizontal(6)
                                 .Text(t).FontColor(tc).FontSize(8);

                            void TDC(IContainer c, string t) =>
                                c.Background(bg).BorderBottom(1).BorderColor("#E0E0E0")
                                 .PaddingVertical(5).PaddingHorizontal(6)
                                 .AlignCenter().Text(t).FontColor(tc).FontSize(8);

                            table.Cell().Element(c => TDC(c, x.p.DataVencimento.ToString("dd/MM/yyyy")));
                            table.Cell().Element(c => TDC(c, x.d.DataCompra.ToString("dd/MM/yyyy")));
                            table.Cell().Element(c => TD(c, x.d.Descricao));
                            table.Cell().Element(c => TDC(c, $"{x.p.Numero}/{x.d.QuantidadeParcelas}"));
                            table.Cell().Element(c => TD(c, x.d.Categoria));
                            table.Cell().Element(c => TD(c, x.d.FormaPagamento));
                            table.Cell().Element(c => TD(c, x.d.NomeCredor ?? "-"));
                            table.Cell().Background(bg).BorderBottom(1).BorderColor("#E0E0E0")
                                .PaddingVertical(5).PaddingHorizontal(6).AlignCenter()
                                .Text(x.p.Valor.ToString("C2", culture)).Bold().FontColor(tc).FontSize(8);
                            table.Cell().Element(c => TDC(c, status));
                        }

                        table.Footer(f =>
                        {
                            f.Cell().ColumnSpan(7).Background(RoxoClaro).PaddingVertical(6).PaddingHorizontal(6)
                                .Text("TOTAL").Bold().FontSize(9).FontColor(Roxo);
                            f.Cell().Background(RoxoClaro).PaddingVertical(6).PaddingHorizontal(6)
                                .AlignCenter().Text(totalMes.ToString("C2", culture))
                                .Bold().FontSize(9).FontColor(Roxo);
                            f.Cell().Background(RoxoClaro);
                        });
                    });
                });

                page.Footer().BorderTop(1).BorderColor("#CFD8DC").PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text($"Minhas Finanças — Despesas Fixas  •  {periodo}").FontSize(7).FontColor("#9E9E9E");
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

    private static byte[] GerarPdfExtras(List<DespesaExtraDto> despesas, int mes, int ano)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var culture = new System.Globalization.CultureInfo("pt-BR");

        const string Laranja     = "#E65100";
        const string LaranjaFnd  = "#FFF3E0";
        const string Cinza       = "#ECEFF1";
        const string CinzaTxt    = "#546E7A";
        const string Verde       = "#2E7D32";
        const string VerdeFnd    = "#E8F5E9";
        const string Vermelho    = "#C62828";

        var periodo  = culture.DateTimeFormat.GetMonthName(mes) + $"/{ano}";
        var geradoEm = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        var linhas    = despesas.OrderBy(d => d.PagaEm ?? d.DataDespesa).ThenBy(d => d.Descricao).ToList();
        var totalMes  = linhas.Sum(d => d.Valor);
        var totalPago = linhas.Where(d => d.Paga).Sum(d => d.Valor);
        var emAberto  = totalMes - totalPago;

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
                    h.Item().Background(Laranja).Padding(12).Column(col =>
                    {
                        col.Item().Text($"Minhas Finanças — Despesas Extras  •  {periodo}")
                            .FontColor("#FFE0B2").FontSize(9).SemiBold();
                        col.Item().Text($"{linhas.Count} despesa(s)  •  Total: {totalMes.ToString("C2", culture)}  •  Em aberto: {emAberto.ToString("C2", culture)}")
                            .FontColor("#FFFFFF").FontSize(13).Bold();
                    });

                    h.Item().Background(Cinza).PaddingHorizontal(12).PaddingVertical(3)
                        .Text($"Gerado em: {geradoEm}").FontSize(7).FontColor(CinzaTxt);

                    h.Item().Background(LaranjaFnd).PaddingVertical(8).PaddingHorizontal(30).Row(row =>
                    {
                        void Card(string valor, string label, string cor) =>
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignCenter().Text(valor).Bold().FontSize(13).FontColor(cor);
                                c.Item().AlignCenter().Text(label).FontSize(7).FontColor(CinzaTxt);
                            });

                        Card(linhas.Count.ToString(),           "Despesas",     Laranja);
                        Card(totalMes.ToString("C2", culture),  "Total do Mês", Laranja);
                        Card(totalPago.ToString("C2", culture), "Já Pago",      Verde);
                        Card(emAberto.ToString("C2", culture),  "Em Aberto",    emAberto > 0 ? Vermelho : Verde);
                    });

                    h.Item().PaddingBottom(8);
                });

                page.Content().Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(2);  // Vencimento
                            cols.RelativeColumn(2);  // Dt. Compra
                            cols.RelativeColumn(4);  // Descrição
                            cols.RelativeColumn(2);  // Categoria
                            cols.RelativeColumn(2);  // Forma Pgto
                            cols.RelativeColumn(2);  // Credor
                            cols.RelativeColumn(2);  // Valor
                            cols.RelativeColumn(2);  // Status
                        });

                        static void TH(IContainer c, string t) =>
                            c.Background("#E65100").PaddingVertical(5).PaddingHorizontal(6)
                             .AlignCenter().Text(t).FontColor("#FFFFFF").Bold().FontSize(8);

                        static void THL(IContainer c, string t) =>
                            c.Background("#E65100").PaddingVertical(5).PaddingHorizontal(6)
                             .Text(t).FontColor("#FFFFFF").Bold().FontSize(8);

                        table.Header(h =>
                        {
                            h.Cell().Element(c => TH(c, "Vencimento"));
                            h.Cell().Element(c => TH(c, "Dt. Compra"));
                            h.Cell().Element(c => THL(c, "Descrição"));
                            h.Cell().Element(c => THL(c, "Categoria"));
                            h.Cell().Element(c => THL(c, "Forma Pgto"));
                            h.Cell().Element(c => THL(c, "Credor"));
                            h.Cell().Element(c => TH(c, "Valor"));
                            h.Cell().Element(c => TH(c, "Status"));
                        });

                        var idx = 0;
                        foreach (var d in linhas)
                        {
                            idx++;
                            string bg, tc;
                            if (d.Paga) { bg = VerdeFnd; tc = Verde; }
                            else        { bg = idx % 2 == 0 ? "#FFF8F0" : "#FFFFFF"; tc = "#212121"; }

                            string status  = d.Paga ? "Pago" : "Pendente";
                            string dataRef = (d.PagaEm ?? d.DataDespesa).ToString("dd/MM/yyyy");

                            void TD(IContainer c, string t) =>
                                c.Background(bg).BorderBottom(1).BorderColor("#E0E0E0")
                                 .PaddingVertical(5).PaddingHorizontal(6)
                                 .Text(t).FontColor(tc).FontSize(8);

                            void TDC(IContainer c, string t) =>
                                c.Background(bg).BorderBottom(1).BorderColor("#E0E0E0")
                                 .PaddingVertical(5).PaddingHorizontal(6)
                                 .AlignCenter().Text(t).FontColor(tc).FontSize(8);

                            table.Cell().Element(c => TDC(c, dataRef));
                            table.Cell().Element(c => TDC(c, d.DataDespesa.ToString("dd/MM/yyyy")));
                            table.Cell().Element(c => TD(c, d.Descricao));
                            table.Cell().Element(c => TD(c, d.Categoria));
                            table.Cell().Element(c => TD(c, d.FormaPagamento));
                            table.Cell().Element(c => TD(c, d.NomeCredor ?? "-"));
                            table.Cell().Background(bg).BorderBottom(1).BorderColor("#E0E0E0")
                                .PaddingVertical(5).PaddingHorizontal(6).AlignCenter()
                                .Text(d.Valor.ToString("C2", culture)).Bold().FontColor(tc).FontSize(8);
                            table.Cell().Element(c => TDC(c, status));
                        }

                        table.Footer(f =>
                        {
                            f.Cell().ColumnSpan(6).Background(LaranjaFnd).PaddingVertical(6).PaddingHorizontal(6)
                                .Text("TOTAL").Bold().FontSize(9).FontColor(Laranja);
                            f.Cell().Background(LaranjaFnd).PaddingVertical(6).PaddingHorizontal(6)
                                .AlignCenter().Text(totalMes.ToString("C2", culture))
                                .Bold().FontSize(9).FontColor(Laranja);
                            f.Cell().Background(LaranjaFnd);
                        });
                    });
                });

                page.Footer().BorderTop(1).BorderColor("#CFD8DC").PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text($"Minhas Finanças — Despesas Extras  •  {periodo}").FontSize(7).FontColor("#9E9E9E");
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
    decimal ValorParcela,
    int QuantidadeParcelas,
    DateOnly DataCompra,
    DateOnly DataPrimeiraParcela,
    string Categoria,
    string FormaPagamento,
    Guid? CredorId = null);

public record AtualizarDespesaExtraRequest(
    string Descricao,
    decimal Valor,
    DateOnly DataDespesa,
    string Categoria,
    string FormaPagamento,
    DateOnly? PagaEm = null,
    Guid? CredorId = null);

public record CriarDespesaFixaRequest(
    string Descricao,
    decimal ValorParcela,
    int QuantidadeParcelas,
    DateOnly DataCompra,
    DateOnly DataPrimeiraParcela,
    string Categoria,
    string FormaPagamento,
    Guid? CredorId = null);

public record CriarDespesaExtraRequest(
    string Descricao,
    decimal Valor,
    DateOnly DataDespesa,
    string Categoria,
    string FormaPagamento,
    DateOnly? PagaEm = null,
    Guid? CredorId = null);

public record MarcarParcelaRequest(bool Paga, DateOnly? DataPagamento = null);
public record MarcarDespesaExtraRequest(bool Paga);
