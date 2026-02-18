using System;
using DomuWave.Services.Models;

namespace DomuWave.Domain.Models
{
    public class CondominiumCadastralData : TenantEntity<int>
    {
        public virtual Condominium Condominium { get; set; }
        public virtual string Sheet { get; set; }
        public virtual string Parcel { get; set; }
        public virtual string Subordinate { get; set; }
        public virtual string UrbanSection { get; set; }
        public virtual string CadastralMunicipality { get; set; }
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
