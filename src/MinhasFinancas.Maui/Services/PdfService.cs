using SkiaSharp;

namespace MinhasFinancas.Maui.Services;

/// <summary>
/// Gera relatórios em PDF simples (tabela) localmente no dispositivo, usando
/// SkiaSharp (que, ao contrário do QuestPDF, publica binários nativos corretos
/// para Android), e abre o menu de compartilhar/salvar do Android.
/// </summary>
public class PdfService
{
    private const float PageWidth = 842f;  // A4 paisagem, em pontos
    private const float PageHeight = 595f;
    private const float Margin = 28f;
    private const float RowHeight = 22f;
    private const float HeaderHeight = 66f;
    private const float ColHeaderHeight = 24f;

    public async Task GerarESalvarAsync(
        string titulo,
        string periodo,
        string corHex,
        string[] colunas,
        IReadOnlyList<string[]> linhas,
        string nomeArquivo,
        string[]? linhaTotal = null)
    {
        var cor = SKColor.Parse(corHex);
        var caminho = Path.Combine(FileSystem.CacheDirectory, nomeArquivo);

        using (var stream = new SKFileWStream(caminho))
        using (var document = SKDocument.CreatePdf(stream))
        {
            using var paintBranco = TextPaint(SKColors.White, 11);
            using var paintTitulo = TextPaint(SKColors.White, 17, bold: true);
            using var paintCabecalhoColuna = TextPaint(SKColors.White, 9, bold: true);
            using var paintCelula = TextPaint(new SKColor(30, 41, 59), 9);
            using var paintVazio = TextPaint(new SKColor(100, 116, 139), 10);
            using var paintGeradoEm = TextPaint(new SKColor(84, 110, 122), 7);

            using var paintTotal = TextPaint(cor, 10, bold: true);
            using var fillHeader = new SKPaint { Color = cor, IsAntialias = true, Style = SKPaintStyle.Fill };
            using var fillZebra = new SKPaint { Color = new SKColor(245, 247, 250), IsAntialias = true, Style = SKPaintStyle.Fill };
            using var fillTotal = new SKPaint { Color = cor.WithAlpha(30), IsAntialias = true, Style = SKPaintStyle.Fill };
            using var strokeLinha = new SKPaint { Color = new SKColor(226, 232, 240), StrokeWidth = 1, IsAntialias = true };

            var colWidth = (PageWidth - Margin * 2) / colunas.Length;
            var geradoEm = $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}";

            SKCanvas? canvas = null;
            float y = 0;

            void DesenharCabecalho()
            {
                canvas!.DrawRect(0, 0, PageWidth, HeaderHeight, fillHeader);
                canvas.DrawText($"Minhas Finanças — {titulo}", Margin, 22, paintBranco);
                canvas.DrawText(periodo, Margin, 46, paintTitulo);
                canvas.DrawText(geradoEm, PageWidth - Margin - 140, HeaderHeight - 8, paintGeradoEm);

                y = HeaderHeight + 14;
                canvas.DrawRect(Margin, y, PageWidth - Margin * 2, ColHeaderHeight, fillHeader);
                for (var c = 0; c < colunas.Length; c++)
                    canvas.DrawText(colunas[c], Margin + c * colWidth + 6, y + 16, paintCabecalhoColuna);
                y += ColHeaderHeight;
            }

            void NovaPagina()
            {
                if (canvas != null) document.EndPage();
                canvas = document.BeginPage(PageWidth, PageHeight);
                DesenharCabecalho();
            }

            NovaPagina();

            var idx = 0;
            foreach (var linha in linhas)
            {
                if (y + RowHeight > PageHeight - Margin) NovaPagina();

                if (idx % 2 == 1)
                    canvas!.DrawRect(Margin, y, PageWidth - Margin * 2, RowHeight, fillZebra);

                for (var c = 0; c < linha.Length && c < colunas.Length; c++)
                    canvas!.DrawText(Truncar(linha[c] ?? string.Empty, colWidth), Margin + c * colWidth + 6, y + 15, paintCelula);

                canvas!.DrawLine(Margin, y + RowHeight, PageWidth - Margin, y + RowHeight, strokeLinha);
                y += RowHeight;
                idx++;
            }

            if (linhas.Count == 0)
                canvas!.DrawText("Nenhum registro encontrado para este período.", Margin, y + 20, paintVazio);
            else if (linhaTotal is not null)
            {
                if (y + RowHeight > PageHeight - Margin) NovaPagina();

                canvas!.DrawRect(Margin, y, PageWidth - Margin * 2, RowHeight, fillTotal);
                for (var c = 0; c < linhaTotal.Length && c < colunas.Length; c++)
                    canvas.DrawText(Truncar(linhaTotal[c] ?? string.Empty, colWidth), Margin + c * colWidth + 6, y + 15, paintTotal);
                y += RowHeight;
            }

            document.EndPage();
            document.Close();
        }

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = titulo,
            File = new ShareFile(caminho)
        });
    }

    private static string Truncar(string texto, float larguraColuna)
    {
        var maxChars = Math.Max(4, (int)(larguraColuna / 4.6));
        return texto.Length > maxChars ? texto[..(maxChars - 1)] + "…" : texto;
    }

    private static SKPaint TextPaint(SKColor color, float size, bool bold = false) => new()
    {
        Color = color,
        TextSize = size,
        IsAntialias = true,
        Typeface = SKTypeface.FromFamilyName(null, bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
            SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
    };
}
