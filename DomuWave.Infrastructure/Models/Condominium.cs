using System;

namespace DomuWave.Services.Models
{
    public class Condominium
    {
        public virtual int CondominiumId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual string Name { get; set; }
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
        public virtual string Notes { get; set; }
        public virtual bool IsActive { get; set; }
        
        
        
        
        
        
        
        

        public virtual Tenant Tenant { get; set; }

        public Condominium()
        {
            NumberOfUnits = 0;
            NumberOfStaircases = 1;
            TotalMillesimal = 1000.0000M;
            HasElevator = false;
            NumberOfElevators = 0;
            HasCentralHeating = false;
            HasConcierge = false;
            InstallmentFrequency = "Monthly";
            InstallmentDueDay = 10;
            IsActive = true;
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
