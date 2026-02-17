using System;
namespace DomuWave.Services.Models
{
    public class MillesimalTable
    {
        public virtual int MillesimalTableId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual int CondominiumId { get; set; }
        public virtual string Code { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual decimal TotalMillesimal { get; set; }
        public virtual bool IsActive { get; set; }
        
        
        
        
        
        
        
        public virtual Tenant Tenant { get; set; }
        public virtual Condominium Condominium { get; set; }
        public MillesimalTable()
        {
            TotalMillesimal = 1000.0000M;
            IsActive = true;
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
