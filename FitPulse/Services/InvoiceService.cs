namespace FitPulse.Services;

public interface IInvoiceService
{
    byte[] GenerateInvoicePdf(Payment payment, TrainingSession session, Member member);
}

public class InvoiceService : IInvoiceService
{
    public byte[] GenerateInvoicePdf(Payment payment, TrainingSession session, Member member)
    {
        var document = new InvoiceDocument(payment, session, member);
        return document.GeneratePdf();
    }
}