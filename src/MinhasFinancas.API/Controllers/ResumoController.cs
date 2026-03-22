using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MinhasFinancas.API.Controllers;

[ApiController]
[Route("api/resumo")]
[Authorize]
public class ResumoController : ControllerBase
{
    private readonly IMediator _mediator;

    public ResumoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("mensal")]
    public async Task<IActionResult> ObterResumoMensal(
        [FromQuery] int? ano,
        [FromQuery] int? mes,
        CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var hoje = DateTime.UtcNow;
        var resultado = await _mediator.Send(
            new ObterResumoMensalQuery(usuarioId.Value, ano ?? hoje.Year, mes ?? hoje.Month), ct);
        return Ok(resultado);
    }

    [HttpGet("mensal/export/pdf")]
    public async Task<IActionResult> ExportarPdf(
        [FromQuery] int? ano,
        [FromQuery] int? mes,
        CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var hoje = DateTime.UtcNow;
        var anoVal = ano ?? hoje.Year;
        var mesVal = mes ?? hoje.Month;

        var resumo = await _mediator.Send(
            new ObterResumoMensalQuery(usuarioId.Value, anoVal, mesVal), ct);

        var culture = new System.Globalization.CultureInfo("pt-BR");
        var nomeMes = culture.DateTimeFormat.GetMonthName(mesVal);
        var nomeArquivo = $"resumo_{nomeMes.ToLower()}_{anoVal}.pdf";

        var pdf = GerarPdf(resumo, anoVal, mesVal, culture);
        return File(pdf, "application/pdf", nomeArquivo);
    }

    private static byte[] GerarPdf(ResumoMensalDto resumo, int ano, int mes,
        System.Globalization.CultureInfo culture)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        const string Roxo        = "#6A1B9A";
        const string RoxoClaro   = "#F3E5F5";
        const string Vermelho    = "#C62828";
        const string VermelhoClaro = "#FFEBEE";
        const string Laranja     = "#E65100";
        const string LaranjaClaro = "#FFF3E0";
        const string Azul        = "#1565C0";
        const string AzulClaro   = "#E3F2FD";
        const string Cinza       = "#ECEFF1";
        const string CinzaTexto  = "#546E7A";
        const string Verde       = "#2E7D32";
        const string Branco      = "#FFFFFF";

        var nomeMes    = culture.DateTimeFormat.GetMonthName(mes);
        var geradoEm   = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        var totalGeral = resumo.TotalDespesasFixas + resumo.TotalDespesasExtras;

        return Document.Create(container =>
        {
            PageExtensions.Page(container, page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginLeft(40, QuestPDF.Infrastructure.Unit.Point);
                page.MarginRight(40, QuestPDF.Infrastructure.Unit.Point);
                page.MarginTop(28, QuestPDF.Infrastructure.Unit.Point);
                page.MarginBottom(28, QuestPDF.Infrastructure.Unit.Point);
                page.DefaultTextStyle(x => x.FontSize(9));

                // ── HEADER ──────────────────────────────────────────
                page.Header().Column(h =>
                {
                    h.Item().Background(Roxo).Padding(12).Column(col =>
                    {
                        col.Item().Text("Minhas Finanças — Relatório Mensal")
                            .FontColor("#CE93D8").FontSize(9).SemiBold();
                        col.Item().Text($"{nomeMes} / {ano}")
                            .FontColor(Branco).FontSize(20).Bold();
                    });

                    h.Item().Background(Cinza).PaddingHorizontal(12).PaddingVertical(3)
                        .Text($"Gerado em: {geradoEm}")
                        .FontSize(7).FontColor(CinzaTexto);

                    // Cards de resumo
                    h.Item().Background(RoxoClaro).PaddingVertical(8).PaddingHorizontal(30).Row(row =>
                    {
                        void Card(string valor, string label, string cor)
                            => row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignCenter().Text(valor).Bold().FontSize(13).FontColor(cor);
                                c.Item().AlignCenter().Text(label).FontSize(7).FontColor(CinzaTexto);
                            });

                        Card(resumo.TotalDespesasFixas.ToString("C2", culture),  "Despesas Fixas",    Vermelho);
                        Card(resumo.TotalDespesasExtras.ToString("C2", culture), "Despesas Extras",   Laranja);
                        Card(resumo.TotalContasAReceber.ToString("C2", culture), "Contas a Receber",  Azul);
                        Card(totalGeral.ToString("C2", culture),                 "Total Saídas",      Roxo);
                    });

                    h.Item().PaddingBottom(8);
                });

                // ── CONTENT ─────────────────────────────────────────
                page.Content().Column(col =>
                {
                    // ── Seção 1: Despesas Fixas ──────────────────────
                    col.Item().PaddingBottom(14).Column(sec =>
                    {
                        sec.Item().Background(VermelhoClaro).PaddingHorizontal(10).PaddingVertical(6)
                            .Row(r =>
                            {
                                r.RelativeItem().Text("Despesas Fixas — Parcelas do Mês")
                                    .Bold().FontSize(10).FontColor(Vermelho);
                                r.AutoItem().Text($"{resumo.DespesasFixas.Count} parcela(s)   •   " +
                                    $"{resumo.TotalDespesasFixas.ToString("C2", culture)}")
                                    .FontSize(9).FontColor(Vermelho).SemiBold();
                            });

                        if (!resumo.DespesasFixas.Any())
                        {
                            sec.Item().Background(Cinza).Padding(8)
                                .Text("Nenhuma parcela neste mês.").FontColor(CinzaTexto).Italic();
                        }
                        else
                        {
                            sec.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(4); // Descrição
                                    cols.RelativeColumn(3); // Categoria
                                    cols.RelativeColumn(2); // Parcela
                                    cols.RelativeColumn(2); // Vencimento
                                    cols.RelativeColumn(2); // Valor
                                    cols.RelativeColumn(2); // Status
                                });

                                static void TH(IContainer c, string t) =>
                                    c.Background("#C62828").PaddingVertical(4).PaddingHorizontal(6)
                                     .AlignCenter().Text(t).FontColor(Branco).Bold().FontSize(8);

                                table.Header(h =>
                                {
                                    h.Cell().Element(c => TH(c, "Descrição"));
                                    h.Cell().Element(c => TH(c, "Categoria"));
                                    h.Cell().Element(c => TH(c, "Parcela"));
                                    h.Cell().Element(c => TH(c, "Vencimento"));
                                    h.Cell().Element(c => TH(c, "Valor"));
                                    h.Cell().Element(c => TH(c, "Status"));
                                });

                                var idx = 0;
                                foreach (var item in resumo.DespesasFixas)
                                {
                                    idx++;
                                    var bg = idx % 2 == 0 ? "#FFF8F8" : Branco;
                                    void TD(IContainer c, string t, string? cor = null) =>
                                        c.Background(bg).BorderBottom(1).BorderColor("#EEE")
                                         .PaddingVertical(4).PaddingHorizontal(6)
                                         .AlignCenter().Text(t).FontColor(cor ?? "#212121").FontSize(8);

                                    var statusTxt = item.Paga ? "Paga" : (item.DataVencimento < DateOnly.FromDateTime(DateTime.Today) ? "Vencida" : "Pendente");
                                    var statusCor = item.Paga ? Verde : (item.DataVencimento < DateOnly.FromDateTime(DateTime.Today) ? Vermelho : CinzaTexto);

                                    table.Cell().Background(bg).BorderBottom(1).BorderColor("#EEE")
                                        .PaddingVertical(4).PaddingHorizontal(6)
                                        .Text(item.Descricao).FontSize(8).FontColor("#212121");
                                    table.Cell().Element(c => TD(c, item.Categoria));
                                    table.Cell().Element(c => TD(c, $"{item.NumeroParcela}/{item.TotalParcelas}"));
                                    table.Cell().Element(c => TD(c, item.DataVencimento.ToString("dd/MM/yyyy")));
                                    table.Cell().Element(c => TD(c, item.Valor.ToString("C2", culture), Vermelho));
                                    table.Cell().Element(c => TD(c, statusTxt, statusCor));
                                }

                                table.Footer(f =>
                                {
                                    f.Cell().ColumnSpan(4).Background(VermelhoClaro)
                                        .PaddingVertical(5).PaddingHorizontal(6)
                                        .Text("TOTAL").Bold().FontSize(8).FontColor(Vermelho);
                                    f.Cell().Background(VermelhoClaro).PaddingVertical(5).PaddingHorizontal(6)
                                        .AlignCenter().Text(resumo.TotalDespesasFixas.ToString("C2", culture))
                                        .Bold().FontSize(8).FontColor(Vermelho);
                                    f.Cell().Background(VermelhoClaro);
                                });
                            });
                        }
                    });

                    // ── Seção 2: Despesas Extras ─────────────────────
                    col.Item().PaddingBottom(14).Column(sec =>
                    {
                        sec.Item().Background(LaranjaClaro).PaddingHorizontal(10).PaddingVertical(6)
                            .Row(r =>
                            {
                                r.RelativeItem().Text("Despesas Extras")
                                    .Bold().FontSize(10).FontColor(Laranja);
                                r.AutoItem().Text($"{resumo.DespesasExtras.Count} lançamento(s)   •   " +
                                    $"{resumo.TotalDespesasExtras.ToString("C2", culture)}")
                                    .FontSize(9).FontColor(Laranja).SemiBold();
                            });

                        if (!resumo.DespesasExtras.Any())
                        {
                            sec.Item().Background(Cinza).Padding(8)
                                .Text("Nenhuma despesa extra neste mês.").FontColor(CinzaTexto).Italic();
                        }
                        else
                        {
                            sec.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(5); // Descrição
                                    cols.RelativeColumn(3); // Categoria
                                    cols.RelativeColumn(2); // Data
                                    cols.RelativeColumn(2); // Valor
                                });

                                static void TH(IContainer c, string t) =>
                                    c.Background("#E65100").PaddingVertical(4).PaddingHorizontal(6)
                                     .AlignCenter().Text(t).FontColor(Branco).Bold().FontSize(8);

                                table.Header(h =>
                                {
                                    h.Cell().Element(c => TH(c, "Descrição"));
                                    h.Cell().Element(c => TH(c, "Categoria"));
                                    h.Cell().Element(c => TH(c, "Data"));
                                    h.Cell().Element(c => TH(c, "Valor"));
                                });

                                var idx = 0;
                                foreach (var item in resumo.DespesasExtras)
                                {
                                    idx++;
                                    var bg = idx % 2 == 0 ? "#FFF8F2" : Branco;
                                    void TD(IContainer c, string t, string? cor = null) =>
                                        c.Background(bg).BorderBottom(1).BorderColor("#EEE")
                                         .PaddingVertical(4).PaddingHorizontal(6)
                                         .AlignCenter().Text(t).FontColor(cor ?? "#212121").FontSize(8);

                                    table.Cell().Background(bg).BorderBottom(1).BorderColor("#EEE")
                                        .PaddingVertical(4).PaddingHorizontal(6)
                                        .Text(item.Descricao).FontSize(8).FontColor("#212121");
                                    table.Cell().Element(c => TD(c, item.Categoria));
                                    table.Cell().Element(c => TD(c, item.DataDespesa.ToString("dd/MM/yyyy")));
                                    table.Cell().Element(c => TD(c, item.Valor.ToString("C2", culture), Laranja));
                                }

                                table.Footer(f =>
                                {
                                    f.Cell().ColumnSpan(3).Background(LaranjaClaro)
                                        .PaddingVertical(5).PaddingHorizontal(6)
                                        .Text("TOTAL").Bold().FontSize(8).FontColor(Laranja);
                                    f.Cell().Background(LaranjaClaro).PaddingVertical(5).PaddingHorizontal(6)
                                        .AlignCenter().Text(resumo.TotalDespesasExtras.ToString("C2", culture))
                                        .Bold().FontSize(8).FontColor(Laranja);
                                });
                            });
                        }
                    });

                    // ── Seção 3: Contas a Receber ────────────────────
                    col.Item().Column(sec =>
                    {
                        sec.Item().Background(AzulClaro).PaddingHorizontal(10).PaddingVertical(6)
                            .Row(r =>
                            {
                                r.RelativeItem().Text("Contas a Receber")
                                    .Bold().FontSize(10).FontColor(Azul);
                                r.AutoItem().Text($"{resumo.ContasAReceber.Count} devedor(es)   •   " +
                                    $"{resumo.TotalContasAReceber.ToString("C2", culture)}")
                                    .FontSize(9).FontColor(Azul).SemiBold();
                            });

                        if (!resumo.ContasAReceber.Any())
                        {
                            sec.Item().Background(Cinza).Padding(8)
                                .Text("Nenhuma conta a receber neste mês.").FontColor(CinzaTexto).Italic();
                        }
                        else
                        {
                            foreach (var devedor in resumo.ContasAReceber)
                            {
                                sec.Item().PaddingTop(6).PaddingBottom(2)
                                    .Background(AzulClaro).PaddingHorizontal(10).PaddingVertical(4)
                                    .Row(r =>
                                    {
                                        r.RelativeItem().Text(devedor.NomeDevedor)
                                            .Bold().FontSize(9).FontColor(Azul);
                                        r.AutoItem().Text(devedor.Total.ToString("C2", culture))
                                            .SemiBold().FontSize(9).FontColor(Azul);
                                    });

                                sec.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(cols =>
                                    {
                                        cols.RelativeColumn(4); // Descrição
                                        cols.RelativeColumn(2); // Parcela
                                        cols.RelativeColumn(2); // Vencimento
                                        cols.RelativeColumn(2); // Valor
                                        cols.RelativeColumn(2); // Status
                                    });

                                    static void TH(IContainer c, string t) =>
                                        c.Background("#1565C0").PaddingVertical(4).PaddingHorizontal(6)
                                         .AlignCenter().Text(t).FontColor(Branco).Bold().FontSize(8);

                                    table.Header(h =>
                                    {
                                        h.Cell().Element(c => TH(c, "Descrição"));
                                        h.Cell().Element(c => TH(c, "Parcela"));
                                        h.Cell().Element(c => TH(c, "Vencimento"));
                                        h.Cell().Element(c => TH(c, "Valor"));
                                        h.Cell().Element(c => TH(c, "Status"));
                                    });

                                    var idx = 0;
                                    foreach (var p in devedor.Parcelas)
                                    {
                                        idx++;
                                        var bg = idx % 2 == 0 ? "#EBF3FD" : Branco;
                                        void TD(IContainer c, string t, string? cor = null) =>
                                            c.Background(bg).BorderBottom(1).BorderColor("#EEE")
                                             .PaddingVertical(4).PaddingHorizontal(6)
                                             .AlignCenter().Text(t).FontColor(cor ?? "#212121").FontSize(8);

                                        var statusTxt = p.Paga ? "Recebida" : (p.DataVencimento < DateOnly.FromDateTime(DateTime.Today) ? "Vencida" : "Pendente");
                                        var statusCor = p.Paga ? Verde : (p.DataVencimento < DateOnly.FromDateTime(DateTime.Today) ? Vermelho : CinzaTexto);

                                        table.Cell().Background(bg).BorderBottom(1).BorderColor("#EEE")
                                            .PaddingVertical(4).PaddingHorizontal(6)
                                            .Text(p.Descricao).FontSize(8).FontColor("#212121");
                                        table.Cell().Element(c => TD(c, $"{p.NumeroParcela}/{p.TotalParcelas}"));
                                        table.Cell().Element(c => TD(c, p.DataVencimento.ToString("dd/MM/yyyy")));
                                        table.Cell().Element(c => TD(c, p.Valor.ToString("C2", culture), Azul));
                                        table.Cell().Element(c => TD(c, statusTxt, statusCor));
                                    }
                                });
                            }
                        }
                    });
                });

                // ── FOOTER ──────────────────────────────────────────
                page.Footer().AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Minhas Finanças   •   ").FontColor(CinzaTexto).FontSize(7);
                        x.Span("Página ").FontColor(CinzaTexto).FontSize(7);
                        x.CurrentPageNumber().FontColor(CinzaTexto).FontSize(7);
                        x.Span(" de ").FontColor(CinzaTexto).FontSize(7);
                        x.TotalPages().FontColor(CinzaTexto).FontSize(7);
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
