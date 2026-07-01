using DomuWave.Services.Dto.PaymentNotice;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DomuWave.Services.Pdf;

/// <summary>
/// Documento QuestPDF per l'avviso di pagamento rate condominiali.
/// Genera una pagina per ogni <see cref="PaymentNoticeData"/> passato.
/// Usato sia per avviso singola unità (1 pagina) sia per batch rata (N pagine).
/// </summary>
public class PaymentNoticeDocument : IDocument
{
    private readonly IReadOnlyList<PaymentNoticeData> _notices;

    private static readonly string AccentColor = "#4f46e5"; // indigo
    private static readonly string LightBg     = "#f5f5f5";
    private static readonly string BorderColor = "#e5e7eb";
    private static readonly string MutedColor  = "#6b7280";
    private static readonly string RedColor    = "#dc2626";
    private static readonly string GreenColor  = "#16a34a";
    private static readonly string InfoBg      = "#eff6ff"; // blue-50
    private static readonly string InfoBorder  = "#bfdbfe"; // blue-200
    private static readonly string InfoColor   = "#1d4ed8"; // blue-700

    public PaymentNoticeDocument(IReadOnlyList<PaymentNoticeData> notices)
        => _notices = notices;

    public DocumentMetadata GetMetadata()
    {
        var meta = DocumentMetadata.Default;
        meta.Title   = "Avviso di Pagamento Rate Condominiali";
        meta.Author  = "DomuWave";
        meta.Subject = "Avviso di pagamento";
        return meta;
    }

    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        foreach (var notice in _notices)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.8f, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontSize(9).FontFamily("Helvetica"));

                page.Header().Element(c => ComposeHeader(c, notice));
                page.Content().PaddingTop(12).Element(c => ComposeContent(c, notice));
                page.Footer().Element(ComposeFooter);
            });
        }
    }

    // ── Header ────────────────────────────────────────────────────────────────

    private void ComposeHeader(IContainer container, PaymentNoticeData notice)
    {
        container.Column(col =>
        {
            col.Item().Background(AccentColor).Padding(10).Row(row =>
            {
                if (notice.LogoContent is { Length: > 0 })
                    row.ConstantItem(70).PaddingRight(10).AlignMiddle()
                        .Height(40).Image(notice.LogoContent).FitArea();

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(notice.CondominiumName)
                        .FontSize(14).Bold().FontColor("#ffffff");
                    c.Item().Text(notice.CondominiumAddress)
                        .FontSize(8).FontColor("#c7d2fe");
                });
                row.ConstantItem(150).AlignRight().Column(c =>
                {
                    c.Item().Text("AVVISO DI PAGAMENTO")
                        .FontSize(11).Bold().FontColor("#ffffff").AlignRight();
                    c.Item().Text($"Esercizio {notice.FiscalYearCode}")
                        .FontSize(8).FontColor("#c7d2fe").AlignRight();
                    c.Item().Text($"Cod. {notice.CondominiumCode}")
                        .FontSize(7).FontColor("#a5b4fc").AlignRight();
                });
            });

            col.Item().Background(LightBg).PaddingHorizontal(10).PaddingVertical(4).Row(row =>
            {
                if (!string.IsNullOrWhiteSpace(notice.CondominiumEmail))
                    row.RelativeItem().Text($"✉ {notice.CondominiumEmail}").FontSize(7.5f).FontColor(MutedColor);
                if (!string.IsNullOrWhiteSpace(notice.CondominiumPhone))
                    row.RelativeItem().Text($"☎ {notice.CondominiumPhone}").FontSize(7.5f).FontColor(MutedColor).AlignRight();
                if (!string.IsNullOrWhiteSpace(notice.CondominiumTaxCode))
                    row.AutoItem().Text($"C.F. {notice.CondominiumTaxCode}").FontSize(7.5f).FontColor(MutedColor).AlignRight();
            });

            col.Item().BorderBottom(1).BorderColor(BorderColor).PaddingBottom(2);
        });
    }

    // ── Content ───────────────────────────────────────────────────────────────

    private void ComposeContent(IContainer container, PaymentNoticeData notice)
    {
        container.Column(col =>
        {
            // Unità + proprietario
            col.Item().PaddingBottom(12).Row(row =>
            {
                row.RelativeItem().Border(1).BorderColor(BorderColor).Padding(10).Column(c =>
                {
                    c.Item().Text("UNITÀ IMMOBILIARE").FontSize(7).Bold()
                        .FontColor(AccentColor).LetterSpacing(0.05f);
                    c.Item().PaddingTop(4).Text(t =>
                    {
                        t.Span($"Interno {notice.UnitInternalNumber}").Bold().FontSize(12);
                        if (!string.IsNullOrWhiteSpace(notice.UnitDisplayName))
                            t.Span($"  –  {notice.UnitDisplayName}").FontSize(9).FontColor(MutedColor);
                    });
                    if (!string.IsNullOrWhiteSpace(notice.UnitStaircase))
                        c.Item().Text($"Scala {notice.UnitStaircase}  ·  Piano {notice.UnitFloor}")
                            .FontSize(8).FontColor(MutedColor);
                });

                row.ConstantItem(10);

                row.RelativeItem().Border(1).BorderColor(BorderColor).Padding(10).Column(c =>
                {
                    c.Item().Text("INTESTATARIO").FontSize(7).Bold()
                        .FontColor(AccentColor).LetterSpacing(0.05f);
                    c.Item().PaddingTop(4).Text(notice.OwnerFullName).Bold().FontSize(11);
                    if (!string.IsNullOrWhiteSpace(notice.OwnerEmail))
                        c.Item().Text(notice.OwnerEmail).FontSize(8).FontColor(MutedColor);
                });
            });

            // Tabella rate
            col.Item().Element(c => ComposeRatesTable(c, notice));

            // Box totale
            col.Item().PaddingTop(14).Element(c => ComposeTotalBox(c, notice));

            // Box pagamento + causale affiancati
            col.Item().PaddingTop(14).Row(row =>
            {
                row.RelativeItem().Element(c => ComposePaymentBox(c, notice));
                row.ConstantItem(12);
                row.RelativeItem().Element(c => ComposeCausaleBox(c, notice));
            });

            // Nota
            col.Item().PaddingTop(12).BorderTop(1).BorderColor(BorderColor).PaddingTop(8)
                .Text("Per effettuare il pagamento utilizzare i dati bancari indicati sopra. " +
                      "Indicare nella causale il testo riportato nel riquadro.")
                .FontSize(7.5f).FontColor(MutedColor).Italic();

            // Tagliandi singola rata
            var pendingRows = notice.Rows.Where(r => r.Balance > 0).ToList();
            if (pendingRows.Count > 1)
                col.Item().PaddingTop(16).Element(c => ComposeInstallmentSlips(c, notice, pendingRows));
        });
    }

    // ── Tabella rate ──────────────────────────────────────────────────────────

    private void ComposeRatesTable(IContainer container, PaymentNoticeData notice)
    {
        container.Column(col =>
        {
            col.Item().PaddingBottom(6).Text("DETTAGLIO RATE")
                .FontSize(7).Bold().FontColor(AccentColor).LetterSpacing(0.05f);

            col.Item().Background(AccentColor).Padding(5).Row(row =>
            {
                row.ConstantItem(35).Text("N° Rata").FontSize(8).Bold().FontColor("#ffffff").AlignCenter();
                row.RelativeItem().Text("Scadenza").FontSize(8).Bold().FontColor("#ffffff").AlignCenter();
                row.ConstantItem(80).Text("Dovuto").FontSize(8).Bold().FontColor("#ffffff").AlignRight();
                row.ConstantItem(80).Text("Pagato").FontSize(8).Bold().FontColor("#ffffff").AlignRight();
                row.ConstantItem(80).Text("Saldo").FontSize(8).Bold().FontColor("#ffffff").AlignRight();
                row.ConstantItem(70).Text("Stato").FontSize(8).Bold().FontColor("#ffffff").AlignCenter();
                row.ConstantItem(110).Text("Cod. Riconciliazione").FontSize(7.5f).Bold().FontColor("#ffffff").AlignCenter();
            });

            foreach (var (row, idx) in notice.Rows.Select((r, i) => (r, i)))
            {
                var bg        = idx % 2 == 0 ? "#ffffff" : LightBg;
                var isPaid    = row.PaymentStatus == "Paid";
                var isOverdue = row.PaymentStatus == "Overdue";

                col.Item().Background(bg).BorderBottom(1).BorderColor(BorderColor)
                    .Padding(5).Row(r =>
                    {
                        r.ConstantItem(35).Text(row.InstallmentNumber.ToString()).FontSize(8).AlignCenter().FontColor(MutedColor);
                        r.RelativeItem().Text(row.DueDate.ToString("dd/MM/yyyy")).FontSize(8).AlignCenter();
                        r.ConstantItem(80).Text(FormatAmount(row.AmountDue)).FontSize(8).AlignRight();
                        r.ConstantItem(80).Text(FormatAmount(row.AmountPaid)).FontSize(8).AlignRight()
                            .FontColor(row.AmountPaid > 0 ? GreenColor : MutedColor);
                        r.ConstantItem(80).Text(FormatAmount(row.Balance)).FontSize(8).AlignRight()
                            .FontColor(row.Balance > 0 ? RedColor : GreenColor);
                        r.ConstantItem(70).Text(StatusLabel(row.PaymentStatus)).FontSize(7.5f).AlignCenter()
                            .FontColor(isPaid ? GreenColor : isOverdue ? RedColor : MutedColor).Bold();
                        r.ConstantItem(110).Text(row.PaymentCode).FontSize(7).AlignCenter()
                            .FontFamily("Courier New").FontColor(AccentColor).Bold();
                    });
            }

            col.Item().Background("#e0e7ff").Padding(5).Row(r =>
            {
                r.ConstantItem(35);
                r.RelativeItem().Text("TOTALE").FontSize(8).Bold().AlignCenter();
                r.ConstantItem(80).Text(FormatAmount(notice.TotalDue)).FontSize(8).Bold().AlignRight();
                r.ConstantItem(80).Text(FormatAmount(notice.TotalPaid)).FontSize(8).Bold().AlignRight()
                    .FontColor(GreenColor);
                r.ConstantItem(80).Text(FormatAmount(notice.TotalBalance)).FontSize(8).Bold().AlignRight()
                    .FontColor(notice.TotalBalance > 0 ? RedColor : GreenColor);
                r.ConstantItem(180);
            });
        });
    }

    // ── Box totale ────────────────────────────────────────────────────────────

    private void ComposeTotalBox(IContainer container, PaymentNoticeData notice)
    {
        var hasPending = notice.TotalBalance > 0;
        var boxBg     = hasPending ? "#fef2f2" : "#f0fdf4";
        var boxBorder = hasPending ? RedColor  : GreenColor;
        var boxLabel  = hasPending ? "IMPORTO RESIDUO DA VERSARE" : "POSIZIONE REGOLARE";
        var boxColor  = hasPending ? RedColor  : GreenColor;

        container.AlignRight().Width(220)
            .Border(2).BorderColor(boxBorder).Background(boxBg).Padding(12).Column(c =>
            {
                c.Item().Text(boxLabel).FontSize(7).Bold().FontColor(boxColor).LetterSpacing(0.05f);
                c.Item().PaddingTop(4).Text(FormatAmount(notice.TotalBalance))
                    .FontSize(22).Bold().FontColor(boxColor);
                c.Item().Text($"Emesso il {notice.GeneratedAt:dd/MM/yyyy}")
                    .FontSize(7).FontColor(MutedColor);
            });
    }

    // ── Box dati pagamento ────────────────────────────────────────────────────

    private void ComposePaymentBox(IContainer container, PaymentNoticeData notice)
    {
        var nextDue = notice.Rows
            .Where(r => r.Balance > 0)
            .OrderBy(r => r.DueDate)
            .Select(r => (DateTime?)r.DueDate)
            .FirstOrDefault();

        container.Border(1).BorderColor(InfoBorder).Background(InfoBg).Padding(10).Column(col =>
        {
            col.Item().PaddingBottom(5).Text("DATI PER IL PAGAMENTO")
                .FontSize(7).Bold().FontColor(InfoColor).LetterSpacing(0.05f);

            LabelValue(col, "Beneficiario",
                !string.IsNullOrWhiteSpace(notice.BankAccountHolder)
                    ? notice.BankAccountHolder
                    : notice.CondominiumName);

            if (!string.IsNullOrWhiteSpace(notice.Iban))
                LabelValue(col, "IBAN", notice.Iban, mono: true);

            if (!string.IsNullOrWhiteSpace(notice.BankName))
                LabelValue(col, "Banca", notice.BankName);

            if (!string.IsNullOrWhiteSpace(notice.OwnerFullName))
                LabelValue(col, "Ordinante", notice.OwnerFullName);

            LabelValue(col, "Data emissione", notice.GeneratedAt.ToString("dd/MM/yyyy"));

            if (nextDue.HasValue)
                LabelValue(col, "Scadenza", nextDue.Value.ToString("dd/MM/yyyy"), highlight: true);

            col.Item().PaddingVertical(6).LineHorizontal(0.5f).LineColor(InfoBorder);

            col.Item().Text("AMMINISTRATORE").FontSize(6.5f).Bold()
                .FontColor(InfoColor).LetterSpacing(0.05f);

            if (!string.IsNullOrWhiteSpace(notice.AdministratorName))
                col.Item().PaddingTop(2).Text(notice.AdministratorName).FontSize(8).Bold();

            if (!string.IsNullOrWhiteSpace(notice.AdministratorPhone))
                col.Item().Text($"☎ {notice.AdministratorPhone}").FontSize(7.5f).FontColor(MutedColor);

            if (!string.IsNullOrWhiteSpace(notice.AdministratorEmail))
                col.Item().Text($"✉ {notice.AdministratorEmail}").FontSize(7.5f).FontColor(MutedColor);
        });
    }

    // ── Box causale ───────────────────────────────────────────────────────────

    private void ComposeCausaleBox(IContainer container, PaymentNoticeData notice)
    {
        var hasPending = notice.TotalBalance > 0;
        var boxBorder  = hasPending ? RedColor  : GreenColor;
        var boxBg      = hasPending ? "#fff7ed" : "#f0fdf4";
        var amtColor   = hasPending ? RedColor  : GreenColor;

        var pendingRows = notice.Rows.Where(r => r.Balance > 0).ToList();
        var rateParts   = pendingRows.Select(r => $"rata {r.InstallmentNumber} ({r.DueDate:MM/yyyy})").ToList();
        var codeParts   = pendingRows.Select(r => r.PaymentCode).Where(c => !string.IsNullOrEmpty(c)).ToList();

        var causale = rateParts.Count > 0
            ? $"Quote condominiali – {notice.CondominiumName}" +
              (!string.IsNullOrWhiteSpace(notice.CondominiumTaxCode) ? $" – C.F. {notice.CondominiumTaxCode}" : "") +
              $" – Interno {notice.UnitInternalNumber} – {string.Join(", ", rateParts)}" +
              $" – Esercizio {notice.FiscalYearCode}" +
              (codeParts.Count > 0 ? $" – COD: {string.Join(" / ", codeParts)}" : "")
            : $"Quote condominiali – {notice.CondominiumName}" +
              $" – Interno {notice.UnitInternalNumber} – Esercizio {notice.FiscalYearCode}";

        container.Border(2).BorderColor(boxBorder).Background(boxBg).Padding(10).Column(col =>
        {
            col.Item().PaddingBottom(5).Text("CAUSALE VERSAMENTO")
                .FontSize(7).Bold().FontColor(amtColor).LetterSpacing(0.05f);

            col.Item().Text(causale).FontSize(8).Italic().FontColor(MutedColor);

            col.Item().PaddingVertical(8).LineHorizontal(0.5f).LineColor(boxBorder);

            col.Item().Text("IMPORTO DA VERSARE").FontSize(7).Bold()
                .FontColor(amtColor).LetterSpacing(0.05f);

            col.Item().PaddingTop(2).Text(FormatAmount(notice.TotalBalance))
                .FontSize(20).Bold().FontColor(amtColor);

            if (!string.IsNullOrWhiteSpace(notice.OwnerFullName))
            {
                col.Item().PaddingTop(6).Text("Intestato a:").FontSize(7).FontColor(MutedColor);
                col.Item().Text(notice.OwnerFullName).FontSize(8).Bold();
            }
        });
    }

    // ── Tagliandi per singola rata ────────────────────────────────────────────

    private void ComposeInstallmentSlips(IContainer container, PaymentNoticeData notice, List<PaymentNoticeRow> rows)
    {
        container.Column(col =>
        {
            // Separatore tratteggiato con etichetta
            col.Item().Row(row =>
            {
                row.RelativeItem().PaddingTop(6).BorderTop(1).BorderColor("#9ca3af");
                row.AutoItem().PaddingHorizontal(8)
                    .Text("✂  TAGLIANDI RATA SINGOLA  ✂")
                    .FontSize(7).FontColor(MutedColor).Italic();
                row.RelativeItem().PaddingTop(6).BorderTop(1).BorderColor("#9ca3af");
            });

            col.Item().PaddingTop(4)
                .Text("Utilizza i tagliandi sottostanti se desideri pagare le rate separatamente.")
                .FontSize(7.5f).FontColor(MutedColor).Italic();

            col.Item().PaddingTop(8).Column(slips =>
            {
                foreach (var (instRow, idx) in rows.Select((r, i) => (r, i)))
                {
                    if (idx > 0)
                        slips.Item().PaddingTop(8).BorderTop(1).BorderColor(BorderColor);

                    slips.Item().PaddingTop(idx > 0 ? 8 : 0).Row(slip =>
                    {
                        // Info unità + rata
                        slip.RelativeItem().Border(1).BorderColor(BorderColor).Padding(8).Column(c =>
                        {
                            c.Item().Text($"RATA N° {instRow.InstallmentNumber}")
                                .FontSize(7).Bold().FontColor(AccentColor).LetterSpacing(0.05f);
                            c.Item().PaddingTop(3).Text(t =>
                            {
                                t.Span($"Interno {notice.UnitInternalNumber}").Bold().FontSize(10);
                                if (!string.IsNullOrWhiteSpace(notice.UnitDisplayName))
                                    t.Span($"  –  {notice.UnitDisplayName}").FontSize(8).FontColor(MutedColor);
                            });
                            c.Item().PaddingTop(2).Text($"Scadenza: {instRow.DueDate:dd/MM/yyyy}")
                                .FontSize(8).FontColor(MutedColor);
                            if (!string.IsNullOrWhiteSpace(notice.OwnerFullName))
                                c.Item().Text(notice.OwnerFullName).FontSize(8).FontColor(MutedColor);
                            c.Item().PaddingTop(4).Text($"Esercizio {notice.FiscalYearCode}")
                                .FontSize(7).FontColor(MutedColor).Italic();
                        });

                        slip.ConstantItem(8);

                        // Box causale + importo rata
                        var causale = $"Quote condominiali – {notice.CondominiumName}" +
                            (!string.IsNullOrWhiteSpace(notice.CondominiumTaxCode) ? $" – C.F. {notice.CondominiumTaxCode}" : "") +
                            $" – Interno {notice.UnitInternalNumber}" +
                            $" – Rata {instRow.InstallmentNumber} ({instRow.DueDate:MM/yyyy})" +
                            $" – Esercizio {notice.FiscalYearCode}" +
                            (!string.IsNullOrEmpty(instRow.PaymentCode) ? $" – COD: {instRow.PaymentCode}" : "");

                        slip.RelativeItem(2).Border(2).BorderColor(RedColor).Background("#fff7ed").Padding(8).Column(c =>
                        {
                            c.Item().Text("CAUSALE").FontSize(6.5f).Bold().FontColor(RedColor).LetterSpacing(0.05f);
                            c.Item().PaddingTop(2).Text(causale).FontSize(7).Italic().FontColor(MutedColor);

                            c.Item().PaddingVertical(6).LineHorizontal(0.5f).LineColor(RedColor);

                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Column(d =>
                                {
                                    d.Item().Text("IMPORTO DA VERSARE").FontSize(6.5f).Bold().FontColor(RedColor).LetterSpacing(0.05f);
                                    d.Item().PaddingTop(2).Text(FormatAmount(instRow.Balance))
                                        .FontSize(18).Bold().FontColor(RedColor);
                                    if (!string.IsNullOrEmpty(instRow.PaymentCode))
                                    {
                                        d.Item().PaddingTop(4).Text("COD. RICONCILIAZIONE").FontSize(6).FontColor(MutedColor).LetterSpacing(0.05f);
                                        d.Item().Text(instRow.PaymentCode).FontSize(9).Bold()
                                            .FontFamily("Courier New").FontColor(AccentColor);
                                    }
                                });
                                r.AutoItem().AlignBottom().Column(d =>
                                {
                                    if (!string.IsNullOrWhiteSpace(notice.Iban))
                                    {
                                        d.Item().Text("IBAN").FontSize(6.5f).FontColor(MutedColor);
                                        d.Item().Text(notice.Iban).FontSize(7).FontFamily("Courier New").FontColor("#1d4ed8");
                                    }
                                });
                            });
                        });
                    });
                }
            });
        });
    }

    // ── Footer ────────────────────────────────────────────────────────────────

    private void ComposeFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(BorderColor).PaddingTop(4)
            .Row(row =>
            {
                row.RelativeItem().Text("Documento generato da DomuWave")
                    .FontSize(7).FontColor(MutedColor);
                row.AutoItem().Text(x =>
                {
                    x.Span("Pagina ").FontSize(7).FontColor(MutedColor);
                    x.CurrentPageNumber().FontSize(7).FontColor(MutedColor);
                    x.Span(" di ").FontSize(7).FontColor(MutedColor);
                    x.TotalPages().FontSize(7).FontColor(MutedColor);
                });
            });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void LabelValue(ColumnDescriptor col, string label, string value,
        bool mono = false, bool highlight = false)
    {
        col.Item().PaddingTop(3).Row(r =>
        {
            r.ConstantItem(90).Text(label + ":").FontSize(7).FontColor(MutedColor);
            var txt = r.RelativeItem().Text(value).FontSize(8);
            if (mono)      txt.FontFamily("Courier New");
            if (highlight) txt.Bold().FontColor(RedColor);
        });
    }

    private static string FormatAmount(decimal v) => $"€ {v:N2}";

    private static string StatusLabel(string status) => status switch
    {
        "Paid"          => "PAGATA",
        "Overdue"       => "SCADUTA",
        "PartiallyPaid" => "PARZ.",
        "ToPay"         => "DA PAGARE",
        _               => status,
    };
}
