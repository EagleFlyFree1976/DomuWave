using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomuWave.Services.Models;
using DomuWave.Services.Dto.Tenant;
using DomuWave.Services.Dto.UserTenants;
using DomuWave.Services.Models;

namespace DomuWave.Services.Extensions
{
    public static class DtoExtensions
    {

        public static TenantReadDto ToDto(this UserTenant sorce)
        {
            TenantReadDto dto = new TenantReadDto();
            dto.Id = sorce.Tenant.Id;
            dto.Name = sorce.Tenant.Name;
            
            dto.IsPrimary = sorce.IsDefault;
            dto.OwnerId = sorce.UserId;

            return dto;
        }

        public static UserTenant FillEntity(this UserTenantUpdateDto source, UserTenant entity)
        {
            
            
            entity.IsDefault = source.IsDefault;
            entity.IsActive = source.IsActive;

            return entity;
        }
        public static UserTenant ToEntity(this UserTenantCreateDto source)
        {
            UserTenant entity = new UserTenant();
            entity.UserId = source.UserId;
            entity.Tenant = new Tenant
            {
                Id = source.TenantId,

            };
            entity.IsDefault = source.IsDefault;
            entity.IsActive = source.IsActive;

            return entity;
        }
    }
}
