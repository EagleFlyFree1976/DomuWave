using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DomuWave.Application.Code;
using DomuWave.Domain.Models;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using CPQ.Core.ActionFilters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CPQ.Core.Settings;

namespace DomuWave.Microservice.Controllers
{
    /// <summary>
    /// Gestione comunicazioni condominiali
    /// </summary>
    [Route("api/[controller]")]
    public class CommunicationsController(
        ILogger<CommunicationsController> logger,
        IOptionsMonitor<OxCoreSettings> configuration,
        ICommunicationService communicationService)
        : PrivateControllerBase(logger, configuration)
    {
        private readonly ICommunicationService _communicationService = communicationService;

        // ─── REQUEST DTO ──────────────────────────────────────────────────────────

        public class CreateCommunicationRequest
        {
            public int CondominiumId { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public string CommunicationType { get; set; }
            public string Priority { get; set; }
            public DateTime PublicationDate { get; set; }
            public DateTime? ExpirationDate { get; set; }
            public bool SendEmail { get; set; }
            public bool IsVisible { get; set; }
            public string AttachmentPath { get; set; }
        }

        public class UpdateCommunicationRequest
        {
            public string Title { get; set; }
            public string Content { get; set; }
            public string CommunicationType { get; set; }
            public string Priority { get; set; }
            public DateTime PublicationDate { get; set; }
            public DateTime? ExpirationDate { get; set; }
            public bool SendEmail { get; set; }
            public bool IsVisible { get; set; }
            public string AttachmentPath { get; set; }
        }

        // ─── GET /api/communications ──────────────────────────────────────────────

        /// <summary>
        /// Restituisce tutte le comunicazioni non eliminate
        /// </summary>
        [HttpGet("")]
        [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations,
            AuthorizationKeys.DomuWaveModule)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Communication>))]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var items = await _communicationService.GetAllAsync(CurrentUser, cancellationToken).ConfigureAwait(false);
            return Ok(items);
        }

        // ─── GET /api/communications/{id} ─────────────────────────────────────────

        /// <summary>
        /// Restituisce una comunicazione per ID
        /// </summary>
        [HttpGet("{id:int}")]
        [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations,
            AuthorizationKeys.DomuWaveModule)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Communication))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var item = await _communicationService.GetByIdAsync(id, CurrentUser, cancellationToken).ConfigureAwait(false);
            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // ─── GET /api/communications/condominium/{condominiumId} ─────────────────

        /// <summary>
        /// Restituisce tutte le comunicazioni di un condominio
        /// </summary>
        [HttpGet("condominium/{condominiumId:int}")]
        [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations,
            AuthorizationKeys.DomuWaveModule)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Communication>))]
        public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken cancellationToken)
        {
            var items = await _communicationService
                .GetByCondominiumIdAsync(condominiumId, CurrentUser, cancellationToken)
                .ConfigureAwait(false);

            return Ok(items);
        }

        // ─── GET /api/communications/condominium/{condominiumId}/visible ─────────

        /// <summary>
        /// Restituisce le comunicazioni visibili (pubblicate) di un condominio
        /// </summary>
        [HttpGet("condominium/{condominiumId:int}/visible")]
        [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations,
            AuthorizationKeys.DomuWaveModule)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Communication>))]
        public async Task<IActionResult> GetVisible(int condominiumId, CancellationToken cancellationToken)
        {
            var items = await _communicationService
                .GetVisibleCommunicationsAsync(condominiumId, CurrentUser, cancellationToken)
                .ConfigureAwait(false);

            return Ok(items);
        }

        // ─── GET /api/communications/condominium/{condominiumId}/unread/{userId} ──

        /// <summary>
        /// Restituisce le comunicazioni non lette da un utente
        /// </summary>
        [HttpGet("condominium/{condominiumId:int}/unread/{userId:long}")]
        [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations,
            AuthorizationKeys.DomuWaveModule)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Communication>))]
        public async Task<IActionResult> GetUnread(int condominiumId, long userId, CancellationToken cancellationToken)
        {
            var items = await _communicationService
                .GetUnreadByUserAsync(condominiumId, userId, CurrentUser, cancellationToken)
                .ConfigureAwait(false);

            return Ok(items);
        }

        // ─── GET /api/communications/condominium/{condominiumId}/type/{type} ──────

        /// <summary>
        /// Restituisce le comunicazioni filtrate per tipo
        /// </summary>
        [HttpGet("condominium/{condominiumId:int}/type/{type}")]
        [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations,
            AuthorizationKeys.DomuWaveModule)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Communication>))]
        public async Task<IActionResult> GetByType(int condominiumId, string type, CancellationToken cancellationToken)
        {
            var items = await _communicationService
                .GetByTypeAsync(condominiumId, type, CurrentUser, cancellationToken)
                .ConfigureAwait(false);

            return Ok(items);
        }

        // ─── GET /api/communications/condominium/{condominiumId}/priority/{priority}

        /// <summary>
        /// Restituisce le comunicazioni filtrate per priorità
        /// </summary>
        [HttpGet("condominium/{condominiumId:int}/priority/{priority}")]
        [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations,
            AuthorizationKeys.DomuWaveModule)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Communication>))]
        public async Task<IActionResult> GetByPriority(int condominiumId, string priority, CancellationToken cancellationToken)
        {
            var items = await _communicationService
                .GetByPriorityAsync(condominiumId, priority, CurrentUser, cancellationToken)
                .ConfigureAwait(false);

            return Ok(items);
        }

        // ─── POST /api/communications ─────────────────────────────────────────────

        /// <summary>
        /// Crea una nuova comunicazione
        /// </summary>
        [HttpPost("")]
        [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Authorizations,
            AuthorizationKeys.DomuWaveModule)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Communication))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCommunicationRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest("Il corpo della richiesta è obbligatorio.");

            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest("Il campo Title è obbligatorio.");

            if (string.IsNullOrWhiteSpace(request.CommunicationType))
                return BadRequest("Il campo CommunicationType è obbligatorio.");

            if (string.IsNullOrWhiteSpace(request.Priority))
                return BadRequest("Il campo Priority è obbligatorio.");

            var entity = new Communication
            {
                Title = request.Title,
                Content = request.Content,
                CommunicationType = request.CommunicationType,
                Priority = request.Priority,
                PublicationDate = request.PublicationDate,
                ExpirationDate = request.ExpirationDate,
                SendEmail = request.SendEmail,
                IsVisible = request.IsVisible,
                AttachmentPath = request.AttachmentPath
            };

            var created = await _communicationService.CreateAsync(entity, CurrentUser, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ─── PUT /api/communications/{id} ─────────────────────────────────────────

        /// <summary>
        /// Aggiorna una comunicazione esistente
        /// </summary>
        [HttpPut("{id:int}")]
        [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Authorizations,
            AuthorizationKeys.DomuWaveModule)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Communication))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCommunicationRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest("Il corpo della richiesta è obbligatorio.");

            var existing = await _communicationService.GetByIdAsync(id, CurrentUser, cancellationToken).ConfigureAwait(false);
            if (existing == null)
                return NotFound();

            existing.Title = request.Title;
            existing.Content = request.Content;
            existing.CommunicationType = request.CommunicationType;
            existing.Priority = request.Priority;
            existing.PublicationDate = request.PublicationDate;
            existing.ExpirationDate = request.ExpirationDate;
            existing.SendEmail = request.SendEmail;
            existing.IsVisible = request.IsVisible;
            existing.AttachmentPath = request.AttachmentPath;

            var updated = await _communicationService.UpdateAsync(existing, CurrentUser, cancellationToken).ConfigureAwait(false);
            return Ok(updated);
        }

        // ─── POST /api/communications/{id}/publish ────────────────────────────────

        /// <summary>
        /// Pubblica una comunicazione (imposta IsVisible = true e aggiorna PublicationDate)
        /// </summary>
        [HttpPost("{id:int}/publish")]
        [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Authorizations,
            AuthorizationKeys.DomuWaveModule)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Publish(int id, CancellationToken cancellationToken)
        {
            var result = await _communicationService
                .PublishCommunicationAsync(id, CurrentUser.Id, CurrentUser, cancellationToken)
                .ConfigureAwait(false);

            if (!result)
                return NotFound();

            return Ok();
        }

        // ─── DELETE /api/communications/{id} ──────────────────────────────────────

        /// <summary>
        /// Soft-delete di una comunicazione
        /// </summary>
        [HttpDelete("{id:int}")]
        [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Authorizations,
            AuthorizationKeys.DomuWaveModule)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _communicationService.DeleteAsync(id, CurrentUser, cancellationToken).ConfigureAwait(false);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}