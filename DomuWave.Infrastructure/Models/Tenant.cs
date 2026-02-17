using System;

namespace DomuWave.Services.Models
{
    public class Tenant
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Code { get; set; }
        public virtual bool IsActive { get; set; }
        
        
        
        
        
        
        
        

        public Tenant()
        {
            Id = Guid.NewGuid();
            IsActive = true;
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
