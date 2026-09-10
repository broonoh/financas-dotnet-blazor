using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MinhasFinancas.Application.Commands.Despesas;
using MinhasFinancas.Application.Commands.Dividas;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using MinhasFinancas.Maui.Services;

namespace MinhasFinancas.Maui.ViewModels;

public partial class DespesasViewModel(IMediator mediator, UsuarioContexto usuario, PdfService pdfService) : ObservableObject
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");
    private static readonly string[] Meses =
    [
        "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    ];

    private Guid? _idEmEdicaoFixa;
    private Guid? _idEmEdicaoExtra;
    private Guid? _idEmEdicaoDivida;

    public ObservableCollection<DespesaFixaDto> Fixas { get; } = [];
    public ObservableCollection<DespesaExtraDto> Extras { get; } = [];
    public ObservableCollection<DividaDto> Dividas { get; } = [];
    public ObservableCollection<CategoriaDto> Categorias { get; } = [];
    public ObservableCollection<DevedorDto> Devedores { get; } = [];
    public ObservableCollection<CredorDto> Credores { get; } = [];
    public ObservableCollection<FormaPagamentoDto> FormasPagamento { get; } = [];

    [ObservableProperty] private Guid? expandedFixaId;
    [ObservableProperty] private Guid? expandedDividaId;

    [RelayCommand]
    private void ToggleExpandFixa(Guid id) => ExpandedFixaId = ExpandedFixaId == id ? null : id;

    [RelayCommand]
    private void ToggleExpandDivida(Guid id) => ExpandedDividaId = ExpandedDividaId == id ? null : id;

    [ObservableProperty] private bool abaFixa = true;
    [ObservableProperty] private bool abaExtra;
    [ObservableProperty] private bool abaDivida;

    [ObservableProperty] private bool carregando;
    [ObservableProperty] private bool confirmandoPagamentoMassivo;
    [ObservableProperty] private int mesFiltro = DateTime.Now.Month;
    [ObservableProperty] private int anoFiltro = DateTime.Now.Year;

    // --- Formulário Fixa ---
    [ObservableProperty] private string tituloFormFixa = "Nova Despesa Fixa";
    [ObservableProperty] private bool emEdicaoFixa;
    [ObservableProperty] private string descricaoFixa = string.Empty;
    [ObservableProperty] private string valorFixaStr = string.Empty;
    [ObservableProperty] private string parcelasFixaStr = "2";
    [ObservableProperty] private DateTime dataCompraFixa = DateTime.Now;
    [ObservableProperty] private DateTime dataPrimeiraParcelaFixa = DateTime.Now;
    [ObservableProperty] private CategoriaDto? categoriaFixaSelecionada;
    [ObservableProperty] private FormaPagamentoDto? formaPagamentoFixaSelecionada;
    [ObservableProperty] private CredorDto? credorFixaSelecionado;

    // --- Formulário Extra ---
    [ObservableProperty] private string tituloFormExtra = "Nova Despesa Extra";
    [ObservableProperty] private bool emEdicaoExtra;
    [ObservableProperty] private string descricaoExtra = string.Empty;
    [ObservableProperty] private string valorExtraStr = string.Empty;
    [ObservableProperty] private DateTime dataDespesaExtra = DateTime.Now;
    [ObservableProperty] private DateTime? vencimentoExtra;
    [ObservableProperty] private CategoriaDto? categoriaExtraSelecionada;
    [ObservableProperty] private FormaPagamentoDto? formaPagamentoExtraSelecionada;
    [ObservableProperty] private CredorDto? credorExtraSelecionado;

    // --- Formulário Dívida ---
    [ObservableProperty] private string tituloFormDivida = "Nova Dívida";
    [ObservableProperty] private bool emEdicaoDivida;
    [ObservableProperty] private DevedorDto? devedorSelecionado;
    [ObservableProperty] private string descricaoDivida = string.Empty;
    [ObservableProperty] private string valorDividaStr = string.Empty;
    [ObservableProperty] private string parcelasDividaStr = "1";
    [ObservableProperty] private DateTime dataCompraDivida = DateTime.Now;
    [ObservableProperty] private DateTime dataPrimeiraParcelaDivida = DateTime.Now;

    // --- Exportação PDF por devedor (Contas a Receber) ---
    [ObservableProperty] private DevedorDto? devedorExportacaoSelecionado;
    [ObservableProperty] private bool exportandoFixa;
    [ObservableProperty] private bool exportandoExtra;
    [ObservableProperty] private bool exportandoDivida;

    [RelayCommand]
    private void PeriodoAnterior()
    {
        MesFiltro--;
        if (MesFiltro < 1) { MesFiltro = 12; AnoFiltro--; }
    }

    [RelayCommand]
    private void PeriodoSeguinte()
    {
        MesFiltro++;
        if (MesFiltro > 12) { MesFiltro = 1; AnoFiltro++; }
    }

    [RelayCommand]
    private void SelecionarAba(string aba)
    {
        AbaFixa = aba == "fixa";
        AbaExtra = aba == "extra";
        AbaDivida = aba == "divida";
    }

    [RelayCommand]
    public async Task CarregarAsync()
    {
        Carregando = true;
        try
        {
            var categorias = await mediator.Send(new ListarCategoriasDespesaQuery(usuario.UsuarioId));
            Categorias.Clear();
            foreach (var c in categorias) Categorias.Add(c);

            var devedores = await mediator.Send(new ListarDevedoresQuery(usuario.UsuarioId));
            Devedores.Clear();
            foreach (var d in devedores) Devedores.Add(d);
            DevedorExportacaoSelecionado ??= Devedores.FirstOrDefault();

            var credores = await mediator.Send(new ListarCredoresQuery(usuario.UsuarioId));
            Credores.Clear();
            foreach (var c in credores) Credores.Add(c);

            var formasPagamento = await mediator.Send(new ListarFormasPagamentoQuery(usuario.UsuarioId));
            FormasPagamento.Clear();
            foreach (var fp in formasPagamento) FormasPagamento.Add(fp);

            var fixas = await mediator.Send(new ListarDespesasFixasQuery(usuario.UsuarioId));
            Fixas.Clear();
            foreach (var f in fixas) Fixas.Add(f);

            var extras = await mediator.Send(new ListarDespesasExtrasQuery(usuario.UsuarioId));
            Extras.Clear();
            foreach (var e in extras) Extras.Add(e);

            var dividas = await mediator.Send(new ListarDividasQuery(usuario.UsuarioId));
            Dividas.Clear();
            foreach (var d in dividas) Dividas.Add(d);
        }
        finally { Carregando = false; }
    }

    // ================= FIXA =================

    [RelayCommand]
    private async Task SalvarFixaAsync()
    {
        if (string.IsNullOrWhiteSpace(DescricaoFixa) || CategoriaFixaSelecionada is null || FormaPagamentoFixaSelecionada is null
            || !decimal.TryParse(ValorFixaStr.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var valor) || valor <= 0
            || !int.TryParse(ParcelasFixaStr, out var parcelas) || parcelas < 1)
        {
            await Shell.Current.DisplayAlert("Erro", "Preencha todos os campos corretamente.", "OK");
            return;
        }

        try
        {
            if (_idEmEdicaoFixa is Guid id)
                await mediator.Send(new AtualizarDespesaFixaCommand(id, usuario.UsuarioId, DescricaoFixa.Trim(), valor, parcelas,
                    DateOnly.FromDateTime(DataCompraFixa), DateOnly.FromDateTime(DataPrimeiraParcelaFixa), CategoriaFixaSelecionada.Nome, FormaPagamentoFixaSelecionada.Nome,
                    CredorFixaSelecionado?.Id));
            else
                await mediator.Send(new CriarDespesaFixaCommand(usuario.UsuarioId, DescricaoFixa.Trim(), valor, parcelas,
                    DateOnly.FromDateTime(DataCompraFixa), DateOnly.FromDateTime(DataPrimeiraParcelaFixa), CategoriaFixaSelecionada.Nome, FormaPagamentoFixaSelecionada.Nome,
                    CredorFixaSelecionado?.Id));

            LimparFormFixa();
            await CarregarAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException or FluentValidation.ValidationException)
        {
            await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private void EditarFixa(DespesaFixaDto item)
    {
        _idEmEdicaoFixa = item.Id;
        DescricaoFixa = item.Descricao;
        ValorFixaStr = item.ValorTotal.ToString("F2", CultureInfo.InvariantCulture);
        ParcelasFixaStr = item.QuantidadeParcelas.ToString();
        DataCompraFixa = item.DataCompra.ToDateTime(TimeOnly.MinValue);
        DataPrimeiraParcelaFixa = item.DataPrimeiraParcela.ToDateTime(TimeOnly.MinValue);
        CategoriaFixaSelecionada = Categorias.FirstOrDefault(c => c.Nome == item.Categoria);
        FormaPagamentoFixaSelecionada = FormasPagamento.FirstOrDefault(f => f.Nome == item.FormaPagamento);
        CredorFixaSelecionado = item.CredorId is Guid credorId ? Credores.FirstOrDefault(c => c.Id == credorId) : null;
        TituloFormFixa = "Editar Despesa Fixa";
        EmEdicaoFixa = true;
    }

    [RelayCommand]
    private void CancelarEdicaoFixa() => LimparFormFixa();

    [RelayCommand]
    private void LimparCredorFixa() => CredorFixaSelecionado = null;

    [RelayCommand]
    private async Task ExcluirFixaAsync(DespesaFixaDto item)
    {
        var confirmar = await Shell.Current.DisplayAlert("Excluir", $"Excluir \"{item.Descricao}\"?", "Sim", "Não");
        if (!confirmar) return;
        await mediator.Send(new ExcluirDespesaCommand(item.Id, usuario.UsuarioId, Fixa: true));
        await CarregarAsync();
    }

    [RelayCommand]
    private async Task ToggleParcelaFixaAsync(ParcelaDto parcela)
    {
        await mediator.Send(new MarcarParcelaPagaCommand(parcela.Id, usuario.UsuarioId, !parcela.Paga));
        await CarregarAsync();
    }

    private void LimparFormFixa()
    {
        _idEmEdicaoFixa = null;
        DescricaoFixa = string.Empty;
        ValorFixaStr = string.Empty;
        ParcelasFixaStr = "2";
        DataCompraFixa = DateTime.Now;
        DataPrimeiraParcelaFixa = DateTime.Now;
        CategoriaFixaSelecionada = Categorias.FirstOrDefault();
        FormaPagamentoFixaSelecionada = FormasPagamento.FirstOrDefault();
        CredorFixaSelecionado = null;
        TituloFormFixa = "Nova Despesa Fixa";
        EmEdicaoFixa = false;
    }

    // ================= EXTRA =================

    [RelayCommand]
    private async Task SalvarExtraAsync()
    {
        if (string.IsNullOrWhiteSpace(DescricaoExtra) || CategoriaExtraSelecionada is null || FormaPagamentoExtraSelecionada is null
            || !decimal.TryParse(ValorExtraStr.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var valor) || valor <= 0)
        {
            await Shell.Current.DisplayAlert("Erro", "Preencha todos os campos corretamente.", "OK");
            return;
        }

        try
        {
            var pagaEm = VencimentoExtra.HasValue ? DateOnly.FromDateTime(VencimentoExtra.Value) : (DateOnly?)null;

            if (_idEmEdicaoExtra is Guid id)
                await mediator.Send(new AtualizarDespesaExtraCommand(id, usuario.UsuarioId, DescricaoExtra.Trim(), valor,
                    DateOnly.FromDateTime(DataDespesaExtra), CategoriaExtraSelecionada.Nome, FormaPagamentoExtraSelecionada.Nome, pagaEm, CredorExtraSelecionado?.Id));
            else
                await mediator.Send(new CriarDespesaExtraCommand(usuario.UsuarioId, DescricaoExtra.Trim(), valor,
                    DateOnly.FromDateTime(DataDespesaExtra), CategoriaExtraSelecionada.Nome, FormaPagamentoExtraSelecionada.Nome, pagaEm, CredorExtraSelecionado?.Id));

            LimparFormExtra();
            await CarregarAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException or FluentValidation.ValidationException)
        {
            await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private void EditarExtra(DespesaExtraDto item)
    {
        _idEmEdicaoExtra = item.Id;
        DescricaoExtra = item.Descricao;
        ValorExtraStr = item.Valor.ToString("F2", CultureInfo.InvariantCulture);
        DataDespesaExtra = item.DataDespesa.ToDateTime(TimeOnly.MinValue);
        VencimentoExtra = item.PagaEm?.ToDateTime(TimeOnly.MinValue);
        CategoriaExtraSelecionada = Categorias.FirstOrDefault(c => c.Nome == item.Categoria);
        FormaPagamentoExtraSelecionada = FormasPagamento.FirstOrDefault(f => f.Nome == item.FormaPagamento);
        CredorExtraSelecionado = item.CredorId is Guid credorId ? Credores.FirstOrDefault(c => c.Id == credorId) : null;
        TituloFormExtra = "Editar Despesa Extra";
        EmEdicaoExtra = true;
    }

    [RelayCommand]
    private void CancelarEdicaoExtra() => LimparFormExtra();

    [RelayCommand]
    private void LimparVencimentoExtra() => VencimentoExtra = null;

    [RelayCommand]
    private void LimparCredorExtra() => CredorExtraSelecionado = null;

    [RelayCommand]
    private async Task ExcluirExtraAsync(DespesaExtraDto item)
    {
        var confirmar = await Shell.Current.DisplayAlert("Excluir", $"Excluir \"{item.Descricao}\"?", "Sim", "Não");
        if (!confirmar) return;
        await mediator.Send(new ExcluirDespesaCommand(item.Id, usuario.UsuarioId, Fixa: false));
        await CarregarAsync();
    }

    [RelayCommand]
    private async Task ToggleExtraPagaAsync(DespesaExtraDto item)
    {
        await mediator.Send(new MarcarDespesaExtraPagaCommand(item.Id, usuario.UsuarioId, !item.Paga));
        await CarregarAsync();
    }

    private void LimparFormExtra()
    {
        _idEmEdicaoExtra = null;
        DescricaoExtra = string.Empty;
        ValorExtraStr = string.Empty;
        DataDespesaExtra = DateTime.Now;
        VencimentoExtra = null;
        CategoriaExtraSelecionada = Categorias.FirstOrDefault();
        FormaPagamentoExtraSelecionada = FormasPagamento.FirstOrDefault();
        CredorExtraSelecionado = null;
        TituloFormExtra = "Nova Despesa Extra";
        EmEdicaoExtra = false;
    }

    // ================= DÍVIDA (Contas a Receber) =================

    [RelayCommand]
    private async Task SalvarDividaAsync()
    {
        if (DevedorSelecionado is null || string.IsNullOrWhiteSpace(DescricaoDivida)
            || !decimal.TryParse(ValorDividaStr.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var valor) || valor <= 0
            || !int.TryParse(ParcelasDividaStr, out var parcelas) || parcelas < 1)
        {
            await Shell.Current.DisplayAlert("Erro", "Preencha todos os campos corretamente.", "OK");
            return;
        }

        try
        {
            if (_idEmEdicaoDivida is Guid id)
                await mediator.Send(new AtualizarDividaCommand(id, usuario.UsuarioId, DevedorSelecionado.Id, DescricaoDivida.Trim(), valor, parcelas,
                    DateOnly.FromDateTime(DataCompraDivida), DateOnly.FromDateTime(DataPrimeiraParcelaDivida)));
            else
                await mediator.Send(new CriarDividaCommand(usuario.UsuarioId, DevedorSelecionado.Id, DescricaoDivida.Trim(), valor, parcelas,
                    DateOnly.FromDateTime(DataCompraDivida), DateOnly.FromDateTime(DataPrimeiraParcelaDivida)));

            LimparFormDivida();
            await CarregarAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException)
        {
            await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private void EditarDivida(DividaDto item)
    {
        _idEmEdicaoDivida = item.Id;
        DevedorSelecionado = Devedores.FirstOrDefault(d => d.Id == item.DevedorId);
        DescricaoDivida = item.Descricao;
        ValorDividaStr = item.ValorTotal.ToString("F2", CultureInfo.InvariantCulture);
        ParcelasDividaStr = item.QuantidadeParcelas.ToString();
        DataCompraDivida = item.DataCompra.ToDateTime(TimeOnly.MinValue);
        DataPrimeiraParcelaDivida = item.DataPrimeiraParcela.ToDateTime(TimeOnly.MinValue);
        TituloFormDivida = "Editar Dívida";
        EmEdicaoDivida = true;
    }

    [RelayCommand]
    private void CancelarEdicaoDivida() => LimparFormDivida();

    [RelayCommand]
    private async Task ExcluirDividaAsync(DividaDto item)
    {
        var confirmar = await Shell.Current.DisplayAlert("Excluir", $"Excluir a dívida \"{item.Descricao}\"?", "Sim", "Não");
        if (!confirmar) return;
        await mediator.Send(new ExcluirDividaCommand(item.Id, usuario.UsuarioId));
        await CarregarAsync();
    }

    [RelayCommand]
    private async Task ToggleParcelaDividaAsync(ParcelaDividaDto parcela)
    {
        await mediator.Send(new MarcarParcelaDividaPagaCommand(parcela.Id, usuario.UsuarioId, !parcela.Paga));
        await CarregarAsync();
    }

    private void LimparFormDivida()
    {
        _idEmEdicaoDivida = null;
        DevedorSelecionado = Devedores.FirstOrDefault();
        DescricaoDivida = string.Empty;
        ValorDividaStr = string.Empty;
        ParcelasDividaStr = "1";
        DataCompraDivida = DateTime.Now;
        DataPrimeiraParcelaDivida = DateTime.Now;
        TituloFormDivida = "Nova Dívida";
        EmEdicaoDivida = false;
    }

    // ================= EXPORTAR PDF =================

    [RelayCommand]
    private async Task ExportarPdfFixasAsync()
    {
        ExportandoFixa = true;
        try
        {
            var parcelasDoMes = Fixas
                .SelectMany(f => f.Parcelas
                    .Where(p => p.DataVencimento.Year == AnoFiltro && p.DataVencimento.Month == MesFiltro)
                    .Select(p => (Fixa: f, Parcela: p)))
                .ToList();

            var linhas = parcelasDoMes.Select(x => new[]
            {
                x.Fixa.Descricao, x.Fixa.Categoria, $"{x.Parcela.Numero}/{x.Fixa.QuantidadeParcelas}",
                x.Parcela.DataVencimento.ToString("dd/MM/yyyy"), x.Fixa.NomeCredor ?? "-",
                x.Parcela.Valor.ToString("C2", PtBr), x.Parcela.Paga ? "Paga" : "Pendente"
            }).ToList();
            var total = parcelasDoMes.Sum(x => x.Parcela.Valor);

            await pdfService.GerarESalvarAsync("Despesas Fixas", $"{Meses[MesFiltro - 1]}/{AnoFiltro}", "#5C35CC",
                ["Descrição", "Categoria", "Parcela", "Vencimento", "Credor", "Valor", "Status"], linhas,
                $"despesas_fixas_{MesFiltro:D2}_{AnoFiltro}.pdf",
                linhaTotal: ["", "", "", "", "TOTAL A PAGAR", total.ToString("C2", PtBr), ""]);
        }
        catch { await Shell.Current.DisplayAlert("Erro", "Não foi possível gerar o PDF.", "OK"); }
        finally { ExportandoFixa = false; }
    }

    [RelayCommand]
    private async Task ExportarPdfExtrasAsync()
    {
        ExportandoExtra = true;
        try
        {
            var extrasDoMes = Extras
                .Where(e => (e.PagaEm ?? e.DataDespesa).Year == AnoFiltro && (e.PagaEm ?? e.DataDespesa).Month == MesFiltro)
                .ToList();

            var linhas = extrasDoMes.Select(e => new[]
            {
                e.Descricao, e.Categoria, e.DataDespesa.ToString("dd/MM/yyyy"), e.NomeCredor ?? "-",
                e.Valor.ToString("C2", PtBr), e.Paga ? "Paga" : "Pendente"
            }).ToList();
            var total = extrasDoMes.Sum(e => e.Valor);

            await pdfService.GerarESalvarAsync("Despesas Extras", $"{Meses[MesFiltro - 1]}/{AnoFiltro}", "#E65100",
                ["Descrição", "Categoria", "Data", "Credor", "Valor", "Status"], linhas,
                $"despesas_extras_{MesFiltro:D2}_{AnoFiltro}.pdf",
                linhaTotal: ["", "", "", "TOTAL A PAGAR", total.ToString("C2", PtBr), ""]);
        }
        catch { await Shell.Current.DisplayAlert("Erro", "Não foi possível gerar o PDF.", "OK"); }
        finally { ExportandoExtra = false; }
    }

    [RelayCommand]
    private async Task ExportarPdfDividaAsync()
    {
        if (DevedorExportacaoSelecionado is null)
        {
            await Shell.Current.DisplayAlert("Erro", "Selecione um devedor para exportar.", "OK");
            return;
        }

        ExportandoDivida = true;
        try
        {
            var nomeDevedor = DevedorExportacaoSelecionado.Nome;
            var parcelasDoMes = Dividas
                .Where(d => d.NomeDevedor == nomeDevedor)
                .SelectMany(d => d.Parcelas
                    .Where(p => p.DataVencimento.Year == AnoFiltro && p.DataVencimento.Month == MesFiltro)
                    .Select(p => (Divida: d, Parcela: p)))
                .ToList();

            if (parcelasDoMes.Count == 0)
            {
                await Shell.Current.DisplayAlert("Aviso", $"Nenhuma parcela de {nomeDevedor} em {Meses[MesFiltro - 1]}/{AnoFiltro}.", "OK");
                return;
            }

            var linhas = parcelasDoMes.Select(x => new[]
            {
                x.Divida.Descricao, $"{x.Parcela.Numero}/{x.Divida.QuantidadeParcelas}",
                x.Parcela.DataVencimento.ToString("dd/MM/yyyy"), x.Parcela.Valor.ToString("C2", PtBr),
                x.Parcela.Paga ? "Recebida" : (x.Parcela.Vencida ? "Vencida" : "Pendente")
            }).ToList();
            var total = parcelasDoMes.Sum(x => x.Parcela.Valor);
            var totalPago = parcelasDoMes.Where(x => x.Parcela.Paga).Sum(x => x.Parcela.Valor);
            var totalAReceber = total - totalPago;

            await pdfService.GerarESalvarAsync("Contas a Receber", $"{nomeDevedor} — {Meses[MesFiltro - 1]}/{AnoFiltro}", "#C62828",
                ["Descrição", "Parcela", "Vencimento", "Valor", "Status"], linhas,
                $"contas_a_receber_{nomeDevedor}_{MesFiltro:D2}_{AnoFiltro}.pdf",
                linhaTotal: ["", "", "TOTAL A PAGAR", total.ToString("C2", PtBr), $"Em aberto: {totalAReceber.ToString("C2", PtBr)}"]);
        }
        catch { await Shell.Current.DisplayAlert("Erro", "Não foi possível gerar o PDF.", "OK"); }
        finally { ExportandoDivida = false; }
    }

    // ================= CONFIRMAÇÃO DE PAGAMENTO MASSIVO =================

    [RelayCommand]
    private async Task ConfirmarPagamentoMassivoAsync()
    {
        var confirmar = await Shell.Current.DisplayAlert(
            "Confirmação de Pagamento Massivo",
            $"Confirmar todas as parcelas pendentes de {MesFiltro:D2}/{AnoFiltro} como pagas?", "Sim", "Não");
        if (!confirmar) return;

        ConfirmandoPagamentoMassivo = true;
        try
        {
            int quantidade = AbaFixa
                ? await mediator.Send(new MarcarTodasParcelasFixasPagasDoMesCommand(usuario.UsuarioId, AnoFiltro, MesFiltro))
                : AbaExtra
                    ? await mediator.Send(new MarcarTodasDespesasExtrasPagasDoMesCommand(usuario.UsuarioId, AnoFiltro, MesFiltro))
                    : await mediator.Send(new MarcarTodasParcelasDividaPagasDoMesCommand(usuario.UsuarioId, AnoFiltro, MesFiltro));

            await Shell.Current.DisplayAlert("Pronto",
                quantidade > 0 ? $"{quantidade} item(ns) marcado(s) como pago(s)." : "Nada pendente neste período.", "OK");
            await CarregarAsync();
        }
        finally { ConfirmandoPagamentoMassivo = false; }
    }
}
