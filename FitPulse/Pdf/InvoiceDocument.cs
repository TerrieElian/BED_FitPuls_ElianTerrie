namespace FitPulse.Pdf;

public class InvoiceDocument : IDocument
{
    private readonly Payment _payment;
    private readonly TrainingSession _session;
    private readonly Member _member;

    public InvoiceDocument(Payment payment, TrainingSession session, Member member)
    {
        _payment = payment;
        _session = session;
        _member = member;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(11));

            page.Header().Text("FitPulse - Factuur").FontSize(20).Bold();

            page.Content().Column(column =>
            {
                column.Spacing(10);

                column.Item().Text($"Factuurnummer: {_payment.BankTransactionRef}");
                column.Item().Text($"Datum: {_payment.PaidAt:dd/MM/yyyy}");
                column.Item().Text($"Klant: {(_member.FullName != string.Empty ? _member.FullName : _member.Email)}");

                column.Item().PaddingTop(10).Text("Sessiedetails").Bold();
                column.Item().Text($"Toestel: {_session.Device.SerialNumber} ({_session.Device.DeviceType})");
                column.Item().Text($"Duur: {_session.DurationMinutes} minuten");
                column.Item().Text($"Calorieën: {_session.CaloriesBurned} kcal");

                column.Item().PaddingTop(10).Text("Bedrag").Bold();
                column.Item().Text($"Subtotaal (excl. BTW): €{_payment.AmountExclVat:F2}");
                column.Item().Text($"BTW (21%): €{_payment.VatAmount:F2}");
                column.Item().Text($"Totaal (incl. BTW): €{_payment.Amount:F2}").Bold();
            });

            page.Footer().AlignCenter().Text("FitPulse - Bedankt voor je training!");
        });
    }
}