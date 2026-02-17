using System;

namespace DomuWave.Services.Models
{
    public class RealEstateUnit
    {
        public virtual int UnitId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual int CondominiumId { get; set; }
        public virtual string Staircase { get; set; }
        public virtual int Floor { get; set; }
        public virtual string InternalNumber { get; set; }
        public virtual string Subordinate { get; set; }
        public virtual string Category { get; set; }
        public virtual decimal? CadastralIncome { get; set; }
        public virtual decimal? AreaSqm { get; set; }
        public virtual decimal? Rooms { get; set; }
        public virtual string UnitType { get; set; }
        public virtual string OccupancyStatus { get; set; }
        public virtual string Notes { get; set; }
        public virtual bool IsActive { get; set; }
        
        
        
        
        
        
        
        

        public virtual Tenant Tenant { get; set; }
        public virtual Condominium Condominium { get; set; }

        public RealEstateUnit()
        {
            UnitType = "Residential";
            OccupancyStatus = "Occupied";
            IsActive = true;
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
