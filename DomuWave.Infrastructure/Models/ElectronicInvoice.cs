using System;

namespace DomuWave.Services.Models
{
    /// <summary>
    /// Fattura elettronica passiva scaricata dal Cassetto Fiscale / SdI tramite provider
    /// accreditato. Conserva l'XML grezzo (FatturaPA 1.2.1) e i dati estratti per la
    /// riconciliazione con fornitori e spese del condominio.
    /// </summary>
    public class ElectronicInvoice : TenantEntity<int>
    {
        public virtual Condominium Condominium { get; set; }

        /// <summary>Fornitore individuato per partita IVA (match automatico). Null se non riconosciuto.</summary>
        public virtual Supplier Supplier { get; set; }

        /// <summary>Spesa generata da questa fattura. Null finché non collegata.</summary>
        public virtual Expense Expense { get; set; }

        /// <summary>Stato della fattura: 0=New, 1=Linked, 2=Ignored (ElectronicInvoiceStatusLookup).</summary>
        public virtual int StatusId { get; set; }

        /// <summary>Identificativo univoco SdI: chiave di deduplica in fase di download.</summary>
        public virtual string SdiIdentifier { get; set; }

        public virtual string InvoiceNumber { get; set; }
        public virtual DateTime InvoiceDate { get; set; }
        public virtual string SupplierVat { get; set; }
        public virtual string SupplierTaxCode { get; set; }
        public virtual string SupplierName { get; set; }
        public virtual decimal TotalAmount { get; set; }

        /// <summary>File XML grezzo della fattura (FatturaPA 1.2.1).</summary>
        public virtual string XmlContent { get; set; }

        public override int GetHashCode() => Id.GetHashCode();
    }
}
