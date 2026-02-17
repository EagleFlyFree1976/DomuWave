using System;

namespace DomuWave.Services.Models
{
    public class CondominiumCadastralData
    {
        public virtual int CadastralDataId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual int CondominiumId { get; set; }
        public virtual string Sheet { get; set; }
        public virtual string Parcel { get; set; }
        public virtual string Subordinate { get; set; }
        public virtual string UrbanSection { get; set; }
        public virtual string CadastralMunicipality { get; set; }
        
        
        
        
        
        
        
        

        public virtual Tenant Tenant { get; set; }
        public virtual Condominium Condominium { get; set; }

        public CondominiumCadastralData()
        {
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
