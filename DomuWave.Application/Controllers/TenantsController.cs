using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.ActionFilters;
using CPQ.Core.Controllers;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DomuWave.Microservice.Controllers
{
    [Route("api/[controller]")]
    public class TenantsController(
        ILogger<TenantsController> logger,
        IOptionsMonitor<OxCoreSettings> configuration,
        ITenantService tenantService)
        : PrivateAdminControllerBase(logger, configuration)
    {
        private readonly ITenantService _tenantService = tenantService;

        // ─── REQUEST DTO ──────────────────────────────────────────────────────────

        public class CreateTenantRequest
        {
            /// <summary>Nome del tenant</summary>
            public string Name { get; set; }

            /// <summary>Codice univoco del tenant</summary>
            public string Code { get; set; }

            /// <summary>Indica se il tenant è attivo</summary>
            public bool IsActive { get; set; } = true;
        }

        public class UpdateTenantRequest
        {
            /// <summary>Nome del tenant</summary>
            public string Name { get; set; }

            /// <summary>Codice univoco del tenant</summary>
            public string Code { get; set; }

            /// <summary>Indica se il tenant è attivo</summary>
            public bool IsActive { get; set; }
        }

        // ─── GET /api/tenants ─────────────────────────────────────────────────────

        /// <summary>
        /// Restituisce tutti i tenant non eliminati
        /// </summary>
        [HttpGet("")]
        //[AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, AuthorizationKeys.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Tenant>))]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var items = await _tenantService.GetAllAsync(CurrentUser, cancellationToken).ConfigureAwait(false);
            return Ok(items);
        }

        // ─── GET /api/tenants/active ──────────────────────────────────────────────

        /// <summary>
        /// Restituisce solo i tenant attivi
        /// </summary>
        [HttpGet("active")]
        [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations,
            AuthorizationKeys.DomuWaveModule)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Tenant>))]
        public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
        {
            var items = await _tenantService.GetActiveTenantsAsync(CurrentUser, cancellationToken).ConfigureAwait(false);
            return Ok(items);
        }

        // ─── GET /api/tenants/{id} ────────────────────────────────────────────────

        /// <summary>
        /// Restituisce un tenant per ID (Guid)
        /// </summary>
        [HttpGet("{id:guid}")]
        [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations,
            AuthorizationKeys.DomuWaveModule)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Tenant))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var item = await _tenantService.GetByIdAsync(id, CurrentUser, cancellationToken).ConfigureAwait(false);
            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // ─── GET /api/tenants/code/{code} ─────────────────────────────────────────

        /// <summary>
        /// Restituisce un tenant per codice univoco
        /// </summary>
        [HttpGet("code/{code}")]
        [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations,
            AuthorizationKeys.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Tenant))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest("Il codice è obbligatorio.");

            var item = await _tenantService.GetByCodeAsync(code, CurrentUser, cancellationToken).ConfigureAwait(false);
            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // ─── POST /api/tenants ────────────────────────────────────────────────────

        /// <summary>
        /// Crea un nuovo tenant
        /// </summary>
        [HttpPost("")]
        [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Tenants, Modules.Bizlio)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Tenant))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateTenantRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest("Il corpo della richiesta è obbligatorio.");

            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Il campo Name è obbligatorio.");

            if (string.IsNullOrWhiteSpace(request.Code))
                return BadRequest("Il campo Code è obbligatorio.");

            var entity = new Tenant
            {
                Name = request.Name,
                Code = request.Code,
                IsActive = request.IsActive
            };

            var created = await _tenantService.CreateAsync(entity, CurrentUser, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ─── PUT /api/tenants/{id} ────────────────────────────────────────────────

        /// <summary>
        /// Aggiorna un tenant esistente
        /// </summary>
        [HttpPut("{id:guid}")]
        [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Authorizations,
            AuthorizationKeys.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Tenant))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest("Il corpo della richiesta è obbligatorio.");

            var existing = await _tenantService.GetByIdAsync(id, CurrentUser, cancellationToken).ConfigureAwait(false);
            if (existing == null)
                return NotFound();

            existing.Name = request.Name;
            existing.Code = request.Code;
            existing.IsActive = request.IsActive;

            var updated = await _tenantService.UpdateAsync(existing, CurrentUser, cancellationToken).ConfigureAwait(false);
            return Ok(updated);
        }

        // ─── DELETE /api/tenants/{id} ─────────────────────────────────────────────

        /// <summary>
        /// Soft-delete di un tenant
        /// </summary>
        [HttpDelete("{id:guid}")]
        [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Authorizations,
            AuthorizationKeys.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await _tenantService.DeleteAsync(id, CurrentUser, cancellationToken).ConfigureAwait(false);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}