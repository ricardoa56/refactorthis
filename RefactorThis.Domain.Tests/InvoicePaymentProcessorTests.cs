using System;
using System.Collections.Generic;
using NUnit.Framework;
using RefactorThis.Persistence;

namespace RefactorThis.Domain.Tests
{
	[TestFixture]
	public class InvoicePaymentProcessorTests
	{
		[Test]
		public void ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReference( )
		{
            InvoiceRepository repo = new InvoiceRepository( );
            InvoiceService paymentProcessor = new InvoiceService( repo );
            Payment payment = new Payment( );
            string response;
            try
			{
                response = paymentProcessor.ProcessPayment( payment );
			}
			catch ( InvalidOperationException e )
			{
                response = e.Message;
			}

			Assert.AreEqual( "There is no invoice matching this payment", response);
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded( )
		{
            InvoiceRepository repo = new InvoiceRepository( );
            Invoice invoice = new Invoice( repo )
			{
				Amount = 0,
				AmountPaid = 0,
				Payments = null
			};
			repo.Add( invoice );
            InvoiceService paymentProcessor = new InvoiceService( repo );
            Payment payment = new Payment( );

			string result = paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "no payment needed", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid( )
		{
            InvoiceRepository repo = new InvoiceRepository( );
            Invoice invoice = new Invoice( repo )
			{
				Amount = 10,
				AmountPaid = 10,
				Payments = new List<Payment>
				{
					new Payment(10)
				}
			};
			repo.Add( invoice );
            InvoiceService paymentProcessor = new InvoiceService( repo );
            Payment payment = new Payment( );

			string result = paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "invoice was already fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue( )
		{
            InvoiceRepository repo = new InvoiceRepository( );
            Invoice invoice = new Invoice( repo )
			{
				Amount = 10,
				AmountPaid = 5,
				Payments = new List<Payment>
				{
					new Payment(5)
				}
			};
			repo.Add( invoice );
            InvoiceService paymentProcessor = new InvoiceService( repo );
            Payment payment = new Payment(6);

			string result = paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "the payment is greater than the partial amount remaining", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount( )
		{
            InvoiceRepository repo = new InvoiceRepository( );
            Invoice invoice = new Invoice( repo )
			{
				Amount = 5,
				AmountPaid = 0,
				Payments = new List<Payment>( )
			};
			repo.Add( invoice );
            InvoiceService paymentProcessor = new InvoiceService( repo );
            Payment payment = new Payment(6);

			string result = paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "the payment is greater than the invoice amount", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue( )
		{
            InvoiceRepository repo = new InvoiceRepository( );
            Invoice invoice = new Invoice( repo )
			{
				Amount = 10,
				AmountPaid = 5,
				Payments = new List<Payment>
				{
					new Payment(5)
				}
			};
			repo.Add( invoice );
            InvoiceService paymentProcessor = new InvoiceService( repo );
            Payment payment = new Payment(5);

			var result = paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "final partial payment received, invoice is now fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount( )
		{
			var repo = new InvoiceRepository( );
            Invoice invoice = new Invoice( repo )
			{
				Amount = 10,
				AmountPaid = 0,
				Payments = new List<Payment>( ) { new Payment(10) }
			};
			repo.Add( invoice );
            InvoiceService paymentProcessor = new InvoiceService( repo );
            Payment payment = new Payment(10);

			var result = paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "invoice was already fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue( )
		{
            InvoiceRepository repo = new InvoiceRepository( );
            Invoice invoice = new Invoice( repo )
			{
				Amount = 10,
				AmountPaid = 5,
				Payments = new List<Payment>
				{
					new Payment(5)
				}
			};
			repo.Add( invoice );
            InvoiceService paymentProcessor = new InvoiceService( repo );
            Payment payment = new Payment(1);

			string result = paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "another partial payment received, still not fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount( )
		{
            InvoiceRepository repo = new InvoiceRepository( );
            Invoice invoice = new Invoice( repo )
			{
				Amount = 10,
				AmountPaid = 0,
				Payments = new List<Payment>( )
			};
			repo.Add( invoice );
            InvoiceService paymentProcessor = new InvoiceService( repo );
            Payment payment = new Payment(1);

			string result = paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "invoice is now partially paid", result );
		}
	}
}