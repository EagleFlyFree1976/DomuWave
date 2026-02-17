using System;

namespace DomuWave.Services.Models
{
    public class CondominiumAddress
    {
        public virtual int AddressId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual int CondominiumId { get; set; }
        public virtual string Street { get; set; }
        public virtual string StreetNumber { get; set; }
        public virtual string PostalCode { get; set; }
        public virtual string City { get; set; }
        public virtual string Province { get; set; }
        public virtual string Country { get; set; }
        public virtual decimal? Latitude { get; set; }
        public virtual decimal? Longitude { get; set; }
        
        
        
        
        
        
        
        

        public virtual Tenant Tenant { get; set; }
        public virtual Condominium Condominium { get; set; }

        public CondominiumAddress()
        {
            Country = "IT";
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
