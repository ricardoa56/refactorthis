using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public class InvoiceService
    {
        private readonly InvoiceRepository _invoiceRepository;

        public InvoiceService(InvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public string ProcessPayment(Payment payment)
        {
            Invoice inv = _invoiceRepository.GetInvoice(payment.Reference) ?? throw new InvalidOperationException("There is no invoice matching this payment");
            if (NoPaymentIsNeeded(inv)) return "no payment needed";
            if (IsInvoiceInInvalidState(inv)) throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");

            string responseMessage = inv.Payments != null && inv.Payments.Any() ? InstallmentPayment(inv, payment) : FirstTimePayment(inv, payment);
            inv.Save();

            return responseMessage;
        }

        private bool NoPaymentIsNeeded(Invoice invoice)
        {
            return invoice.Amount == 0 && (invoice.Payments == null || !invoice.Payments.Any());
        }

        private bool IsInvoiceInInvalidState(Invoice invoice)
        {
            return invoice.Amount == 0 && invoice.Payments != null && invoice.Payments.Any();
        }

        private bool IsInvoiceWasFullyPaid(Invoice invoice)
        {
            return invoice.Payments != null && invoice.Payments.Any() && invoice.Amount == invoice.Payments.Sum(x => x.Amount);
        }

        private bool IsPaymentGreaterThanPartialAmountRemaining(Invoice invoice, Payment payment)
        {
            return invoice.Payments != null && invoice.Payments.Any() && payment.Amount > (invoice.Amount - invoice.AmountPaid);
        }

        private bool IsPaymentGreaterThanInvoiceAmount(Invoice invoice, Payment payment)
        {
            return payment.Amount > invoice.Amount;
        }

        private string FirstTimePayment(Invoice inv, Payment payment)
        {            
            if (IsPaymentGreaterThanInvoiceAmount(inv, payment)) return "the payment is greater than the invoice amount";
            string invoiceTypeMessage = payment.Amount == inv.Amount ? "invoice is now fully paid" : "invoice is now partially paid";
            string responseMessage;
            switch (inv.Type)
            {
                case InvoiceType.Standard:
                    inv.AmountPaid = payment.Amount;
                    inv.TaxAmount = payment.Amount * 0.14m;
                    inv.Payments.Add(payment);
                    responseMessage = invoiceTypeMessage;
                    break;
                case InvoiceType.Commercial:
                    inv.AmountPaid = payment.Amount;
                    inv.TaxAmount = payment.Amount * 0.14m;
                    inv.Payments.Add(payment);
                    responseMessage = invoiceTypeMessage;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return responseMessage;
        }

        private string InstallmentPayment(Invoice inv, Payment payment)
        {
            if (IsInvoiceWasFullyPaid(inv)) return "invoice was already fully paid";
            if (IsPaymentGreaterThanPartialAmountRemaining(inv, payment)) return "the payment is greater than the partial amount remaining";
            string invoiceTypeMessage = (inv.Amount - inv.AmountPaid) == payment.Amount ? "final partial payment received, invoice is now fully paid" : "another partial payment received, still not fully paid";
            string responseMessage;

            switch (inv.Type)
            {
                case InvoiceType.Standard:
                    inv.AmountPaid += payment.Amount;
                    inv.Payments.Add(payment);
                    responseMessage = invoiceTypeMessage;
                    break;
                case InvoiceType.Commercial:
                    inv.AmountPaid += payment.Amount;
                    inv.TaxAmount += payment.Amount * 0.14m;
                    inv.Payments.Add(payment);
                    responseMessage = invoiceTypeMessage;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return responseMessage;
        }
    }
}