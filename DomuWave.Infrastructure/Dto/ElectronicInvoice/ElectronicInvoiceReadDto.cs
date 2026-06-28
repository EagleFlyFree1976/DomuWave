using System;
using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.ElectronicInvoice;

/// <summary>
/// Vista di lettura di una fattura elettronica passiva scaricata dallo SdI.
/// </summary>
public class ElectronicInvoiceReadDto : TraceEntityDTO<int>
{
    public int    CondominiumId   { get; set; }
    public string CondominiumName { get; set; }

    public int?   SupplierId      { get; set; }
    public string SupplierName    { get; set; }

    public long?  ExpenseId       { get; set; }

    public int    StatusId        { get; set; }
    public string StatusName      { get; set; }

    public string SdiIdentifier   { get; set; }
    public string InvoiceNumber   { get; set; }
    public DateTime InvoiceDate    { get; set; }
    public string SupplierVat     { get; set; }
    public string SupplierTaxCode { get; set; }
    public decimal TotalAmount    { get; set; }
}
