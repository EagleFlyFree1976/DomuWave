using DomuWave.Services.Dto.PaymentNotice;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DomuWave.Services.Pdf;

/// <summary>
/// Documento QuestPDF per lettere/comunicazioni (raccomandate).
/// Genera una pagina per ogni <see cref="PaymentNoticeData"/> passato,
/// usando CustomSubject e CustomBody come contenuto della lettera.
/// </summary>
public class CommunicationLetterDocument : IDocument
{
    private readonly IReadOnlyList<PaymentNoticeData> _letters;

    private static readonly string AccentColor = "#4f46e5";
    private static readonly string LightBg     = "#f5f5f5";
    private static readonly string BorderColor = "#e5e7eb";
    private static readonly string MutedColor  = "#6b7280";

    public CommunicationLetterDocument(IReadOnlyList<PaymentNoticeData> letters)
        => _letters = letters;

    public DocumentMetadata GetMetadata()
    {
        var meta = DocumentMetadata.Default;
        meta.Title  = "Comunicazioni Condominiali";
        meta.Author = "DomuWave";
        return meta;
    }

    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        foreach (var letter in _letters)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2.5f, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontSize(10).FontFamily("Helvetica"));

                page.Content().Column(col =>
                {
                    // ── Header condominio ──────────────────────────────────────
                    col.Item().BorderBottom(1).BorderColor(BorderColor).PaddingBottom(8).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(letter.CondominiumName).Bold().FontSize(13).FontColor(AccentColor);
                            if (!string.IsNullOrWhiteSpace(letter.CondominiumAddress))
                                c.Item().Text(letter.CondominiumAddress).FontSize(8).FontColor(MutedColor);
                        });
                        row.ConstantItem(160).Column(c =>
                        {
                            if (!string.IsNullOrWhiteSpace(letter.AdministratorName))
                                c.Item().Text($"Amministratore: {letter.AdministratorName}").FontSize(8).FontColor(MutedColor);
                            if (!string.IsNullOrWhiteSpace(letter.AdministratorPhone))
                                c.Item().Text($"Tel: {letter.AdministratorPhone}").FontSize(8).FontColor(MutedColor);
                            if (!string.IsNullOrWhiteSpace(letter.AdministratorEmail))
                                c.Item().Text(letter.AdministratorEmail).FontSize(8).FontColor(MutedColor);
                        });
                    });

                    col.Item().Height(16);

                    // ── Destinatario ──────────────────────────────────────────
                    col.Item().PaddingLeft(10).Column(c =>
                    {
                        c.Item().Text("Spett.le").FontSize(9).FontColor(MutedColor);
                        c.Item().Text(letter.OwnerFullName).Bold().FontSize(11);
                        if (!string.IsNullOrWhiteSpace(letter.UnitInternalNumber))
                            c.Item().Text($"Unità {letter.UnitInternalNumber}"
                                          + (string.IsNullOrWhiteSpace(letter.UnitDisplayName) ? "" : $" — {letter.UnitDisplayName}"))
                                   .FontSize(9).FontColor(MutedColor);
                    });

                    col.Item().Height(20);

                    // ── Data ──────────────────────────────────────────────────
                    col.Item().AlignRight()
                        .Text($"Lettera del {DateTime.Now:dd/MM/yyyy}").FontSize(9).FontColor(MutedColor);

                    col.Item().Height(8);

                    // ── Oggetto ───────────────────────────────────────────────
                    if (!string.IsNullOrWhiteSpace(letter.CustomSubject))
                    {
                        col.Item().Background(LightBg).Padding(8).Text(t =>
                        {
                            t.Span("Oggetto: ").Bold();
                            t.Span(letter.CustomSubject);
                        });
                        col.Item().Height(12);
                    }

                    // ── Corpo ─────────────────────────────────────────────────
                    col.Item().Text(letter.CustomBody ?? string.Empty).FontSize(10);

                    col.Item().Height(24);

                    // ── Firma ─────────────────────────────────────────────────
                    col.Item().AlignRight().Column(c =>
                    {
                        c.Item().Text("Cordiali saluti,").FontSize(9).FontColor(MutedColor);
                        if (!string.IsNullOrWhiteSpace(letter.AdministratorName))
                            c.Item().Text(letter.AdministratorName).Bold().FontSize(10);
                        c.Item().Text("Amministratore di Condominio").FontSize(8).FontColor(MutedColor);
                    });
                });

                page.Footer().AlignCenter()
                    .Text(t =>
                    {
                        t.Span("DomuWave — ").FontColor(MutedColor).FontSize(8);
                        t.Span($"Stampato il {DateTime.Now:dd/MM/yyyy}").FontColor(MutedColor).FontSize(8);
                        t.Span("   |   pag. ").FontColor(MutedColor).FontSize(8);
                        t.CurrentPageNumber().FontColor(MutedColor).FontSize(8);
                        t.Span("/").FontColor(MutedColor).FontSize(8);
                        t.TotalPages().FontColor(MutedColor).FontSize(8);
                    });
            });
        }
    }
}
