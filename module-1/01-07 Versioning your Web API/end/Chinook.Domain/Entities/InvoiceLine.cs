using Chinook.Domain.ApiModels;
using Chinook.Domain.Converters;

namespace Chinook.Domain.Entities;

public sealed class InvoiceLine : IConvertModel<InvoiceLineApiModel>
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public int TrackId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }


    public Invoice? Invoice { get; set; }

    public Track? Track { get; set; }

    public InvoiceLineApiModel Convert() =>
        new()
        {
            Id = Id,
            InvoiceId = InvoiceId,
            TrackId = TrackId,
            UnitPrice = UnitPrice,
            Quantity = Quantity
        };
}