using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.Domain.Entities
{
    public class MercadoPagoNotification
    {
        public long Id { get; set; }
        public string Status { get; set; }
        public string ExternalReference { get; set; }
        public string PreferenceId { get; set; }
        public List<Payment> Payments { get; set; }
        public List<object> Shipments { get; set; } // En el ejemplo no se muestra información sobre "shipments"
        public List<object> Payouts { get; set; } // En el ejemplo no se muestra información sobre "payouts"
        public Collector Collector { get; set; }
        public string Marketplace { get; set; }
        public string NotificationUrl { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastUpdated { get; set; }
        public string SponsorId { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RefundedAmount { get; set; }
        public Payer Payer { get; set; }
        public List<Item> Items { get; set; }
        public bool Cancelled { get; set; }
        public string AdditionalInfo { get; set; }
        public bool IsTest { get; set; }
        public string OrderStatus { get; set; }
    }

    public class Payment
    {
        public long Id { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public string CurrencyId { get; set; }
        public string Status { get; set; }
        public string StatusDetail { get; set; }
        public string OperationType { get; set; }
        public DateTime DateApproved { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }
        public decimal AmountRefunded { get; set; }
    }

    public class Collector
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
    }

    public class Payer
    {
        public long Id { get; set; }
        public string Email { get; set; }
    }

    public class Item
    {
        public string Id { get; set; }
        public string CategoryId { get; set; }
        public string CurrencyId { get; set; }
        public string Description { get; set; }
        public string PictureUrl { get; set; }
        public string Title { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }


}
