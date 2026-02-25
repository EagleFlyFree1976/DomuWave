using System;
using System.Collections.Generic;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class Condominium : TenantEntity<int>
    {
        // Name → from GenericEntity, maps to [Name]
        // Description → from GenericEntity, maps to [Notes]

        public virtual string Code { get; set; }
        public virtual string TaxCode { get; set; }
        public virtual string VatNumber { get; set; }
        public virtual string Email { get; set; }
        public virtual string Phone { get; set; }
        public virtual string Pec { get; set; }
        public virtual int NumberOfUnits { get; set; }
        public virtual int NumberOfStaircases { get; set; }
        public virtual int? NumberOfFloors { get; set; }
        public virtual int? YearOfConstruction { get; set; }
        public virtual decimal TotalMillesimal { get; set; }
        public virtual bool HasElevator { get; set; }
        public virtual int? NumberOfElevators { get; set; }
        public virtual bool HasCentralHeating { get; set; }
        public virtual bool HasConcierge { get; set; }
        public virtual decimal? CommonAreasSqm { get; set; }
        public virtual DateTime? MandateStartDate { get; set; }
        public virtual DateTime? MandateEndDate { get; set; }
        public virtual DateTime? LastAssemblyDate { get; set; }
        public virtual string InstallmentFrequency { get; set; }
        public virtual int InstallmentDueDay { get; set; }
        public virtual bool IsActive { get; set; }

        public virtual CondominiumAddress Address { get; set; }
        public virtual CondominiumCadastralData CadastralData { get; set; }
        public virtual IList<RealEstateUnit> Units { get; set; } = new List<RealEstateUnit>();
        public virtual IList<MillesimalTable> MillesimalTables { get; set; } = new List<MillesimalTable>();
        public virtual IList<ChartOfAccounts> Accounts { get; set; } = new List<ChartOfAccounts>();
        public virtual IList<Budget> Budgets { get; set; } = new List<Budget>();
        public virtual IList<Expense> Expenses { get; set; } = new List<Expense>();
        public virtual IList<CondominiumInstallment> Installments { get; set; } = new List<CondominiumInstallment>();
        public virtual IList<Receipt> Receipts { get; set; } = new List<Receipt>();
        public virtual IList<SupplierContract> SupplierContracts { get; set; } = new List<SupplierContract>();
        public virtual IList<Document> Documents { get; set; } = new List<Document>();
        public virtual IList<Communication> Communications { get; set; } = new List<Communication>();
        public virtual IList<Message> Messages { get; set; } = new List<Message>();
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
