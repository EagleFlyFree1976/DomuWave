using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using DomuWave.Services.Models;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Interfaces;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations
{
    /// <summary>
    /// Implementazione concreta di <see cref="IFiscalYearService"/>.
    /// Gestisce il ciclo di vita degli esercizi condominiali con logica di validazione
    /// su sovrapposizioni date, unicità dell'esercizio attivo e prerequisiti di stato.
    /// CacheRegion: "FiscalYears".
    /// </summary>
    public class FiscalYearService : BaseService, IFiscalYearService
    {
        public override string CacheRegion => "FiscalYears";

        public FiscalYearService(
            ISessionFactoryProvider sessionFactoryProvider,
            ICacheManager cache)
            : base(sessionFactoryProvider, cache)
        {
        }

        // ─────────────────────────────────────────────
        // QUERY
        // ─────────────────────────────────────────────

        /// <inheritdoc />
        public async Task<FiscalYear?> GetActiveAsync(
            int condominiumId,
            IUser currentUser,
            CancellationToken ct = default)
        {
            return await session.Query<FiscalYear>()
                .Where(x => x.Condominium.Id == condominiumId
                         && x.IsActive
                         && !x.IsDeleted)
                .FirstOrDefaultAsync(ct);
        }

        /// <inheritdoc />
        public async Task<FiscalYear?> GetByIdAsync(int id, IUser currentUser, CancellationToken ct)
        {
            return await session.Query<FiscalYear>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        }

        /// <inheritdoc />
        public async Task<IList<FiscalYear>> GetAllAsync(IUser currentUser, CancellationToken ct)
        {
            return await session.Query<FiscalYear>()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync(ct);
        }

        /// <inheritdoc />
        public async Task<IList<FiscalYear>> GetByCondominiumAsync(
            int condominiumId,
            IUser currentUser,
            CancellationToken ct = default)
        {
            return await session.Query<FiscalYear>()
                .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync(ct);
        }

        /// <inheritdoc />
        public async Task<IList<FiscalYear>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken ct)
        {
            return await session.Query<FiscalYear>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(ct);
        }

        /// <inheritdoc />
        public async Task<IList<FiscalYear>> FindAsync(
            Expression<Func<FiscalYear, bool>> predicate,
            IUser currentUser,
            CancellationToken ct)
        {
            return await session.Query<FiscalYear>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(ct);
        }

        // ─────────────────────────────────────────────
        // COMMAND — CREATE (Draft)
        // ─────────────────────────────────────────────

        /// <inheritdoc />
        public async Task<FiscalYear> OpenAsync(
            int condominiumId,
            string code,
            string? description,
            DateTime startDate,
            DateTime endDate,
            IUser currentUser,
            CancellationToken ct = default,
            int? previousFiscalYearId = null)
        {
            // Validazione date
            if (endDate <= startDate)
                throw new ValidatorException(
                    "La data di fine esercizio deve essere successiva alla data di inizio.");

            // Validazione predecessore (prima della sovrapposizione: errori più specifici hanno priorità)
            FiscalYear? previousFiscalYear = null;
            if (previousFiscalYearId.HasValue)
            {
                previousFiscalYear = await session.GetAsync<FiscalYear>(previousFiscalYearId.Value, ct)
                    .ConfigureAwait(false)
                    ?? throw new ValidatorException("Esercizio di provenienza non trovato.");

                if (previousFiscalYear.Condominium.Id != condominiumId)
                    throw new ValidatorException("L'esercizio di provenienza non appartiene allo stesso condominio.");

                // La data di inizio deve essere esattamente il giorno successivo alla fine del precedente
                var expectedStartDate = previousFiscalYear.EndDate.Date.AddDays(1);
                if (startDate.Date != expectedStartDate)
                    throw new ValidatorException(
                        $"La data di inizio deve essere il giorno successivo alla data di fine dell'esercizio precedente ({expectedStartDate:dd/MM/yyyy}).");

                // Un esercizio già usato come precedente non può essere riutilizzato
                var alreadyUsedAsPrevious = await session.Query<FiscalYear>()
                    .AnyAsync(x => x.PreviousFiscalYear.Id == previousFiscalYearId.Value
                                && !x.IsDeleted, ct)
                    .ConfigureAwait(false);
                if (alreadyUsedAsPrevious)
                    throw new ValidatorException(
                        "L'esercizio selezionato come precedente è già collegato a un altro esercizio successivo.");
            }

            // Verifica sovrapposizione date
            if (await HasOverlapAsync(condominiumId, startDate, endDate, excludeId: null, ct))
                throw new ValidatorException(
                    "Le date dell'esercizio si sovrappongono con un esercizio esistente.");

            // Codice univoco per condominio
            var codeExists = await session.Query<FiscalYear>()
                .AnyAsync(x => x.Condominium.Id == condominiumId
                            && x.Code == code
                            && !x.IsDeleted, ct);
            if (codeExists)
                throw new ValidatorException(
                    $"Esiste già un esercizio con codice '{code}' per questo condominio.");

            var condominium = await session.GetAsync<Condominium>(condominiumId, ct)
                ?? throw new ValidatorException($"Condominio con ID {condominiumId} non trovato.");

            var draftStatus = await session.GetAsync<FiscalYearStatus>(FiscalYearStatus.Draft, ct)
                ?? throw new ValidatorException("Stato 'Draft' non trovato nella tabella di lookup.");

            var fiscalYear = new FiscalYear
            {
                Condominium        = condominium,
                Tenant             = condominium.Tenant,
                Code               = code,
                Description        = description,
                StartDate          = startDate,
                EndDate            = endDate,
                Status             = draftStatus,
                IsActive           = false,
                IsDeleted          = false,
                PreviousFiscalYear = previousFiscalYear,
            };

            fiscalYear.Trace(currentUser);
            await session.SaveOrUpdateAsync(fiscalYear, ct);
            await session.FlushAsync(ct);

            _cache.Clear(CacheRegion);
            return fiscalYear;
        }

        // ─────────────────────────────────────────────
        // COMMAND — OPEN (Draft → Open)
        // ─────────────────────────────────────────────

        /// <inheritdoc />
        public async Task<bool> OpenFromDraftAsync(int fiscalYearId, IUser currentUser, CancellationToken ct = default)
        {
            var fiscalYear = await GetByIdAsync(fiscalYearId, currentUser, ct);
            if (fiscalYear == null)
                throw new NotFoundException("Esercizio fiscale non trovato.");

            if (fiscalYear.Status?.Id != FiscalYearStatus.Draft)
                throw new ValidatorException(
                    $"Impossibile aprire l'esercizio: lo stato corrente è '{fiscalYear.Status?.Name}'. " +
                    "L'esercizio deve essere in stato Draft.");

            // Verifica che non esista già un esercizio Open o Closing per il condominio
            var hasOpenOrClosing = await session.Query<FiscalYear>()
                .AnyAsync(x => x.Condominium.Id == fiscalYear.Condominium.Id
                            && x.Id != fiscalYearId
                            && (x.Status.Id == FiscalYearStatus.Open || x.Status.Id == FiscalYearStatus.Closing)
                            && !x.IsDeleted, ct);

            if (hasOpenOrClosing)
                throw new ValidatorException(
                    "Esiste già un esercizio aperto o in fase di chiusura per questo condominio. " +
                    "Chiuderlo prima di aprirne uno nuovo.");

            var openStatus = await session.GetAsync<FiscalYearStatus>(FiscalYearStatus.Open, ct)
                ?? throw new ValidatorException("Stato 'Open' non trovato nella tabella di lookup.");

            fiscalYear.Status = openStatus;
            fiscalYear.IsActive = true;
            fiscalYear.Trace(currentUser);

            await session.SaveOrUpdateAsync(fiscalYear, ct);
            await session.FlushAsync(ct);

            _cache.Clear(CacheRegion);
            return true;
        }

        // ─────────────────────────────────────────────
        // COMMAND — UPDATE
        // ─────────────────────────────────────────────

        /// <inheritdoc />
        public async Task<FiscalYear?> UpdateAsync(
            int fiscalYearId,
            FiscalYearUpdateDto dto,
            IUser currentUser,
            CancellationToken ct = default)
        {
            var fiscalYear = await GetByIdAsync(fiscalYearId, currentUser, ct);
            if (fiscalYear == null) return null;

            if (fiscalYear.Status?.Id == FiscalYearStatus.Closed || fiscalYear.Status?.Id == FiscalYearStatus.Locked)
                throw new ValidatorException(
                    $"Non è possibile modificare un esercizio in stato '{fiscalYear.Status?.Name}'.");

            if (dto.Description != null)
                fiscalYear.Description = dto.Description;

            if (dto.EndDate.HasValue)
            {
                if (fiscalYear.Status?.Id != FiscalYearStatus.Open && fiscalYear.Status?.Id != FiscalYearStatus.Draft)
                    throw new ValidatorException(
                        "La data di fine può essere modificata solo su esercizi in stato Draft o Open.");

                if (dto.EndDate.Value <= fiscalYear.StartDate)
                    throw new ValidatorException(
                        "La nuova data di fine deve essere successiva alla data di inizio.");

                if (await HasOverlapAsync(fiscalYear.Condominium.Id, fiscalYear.StartDate, dto.EndDate.Value, fiscalYearId, ct))
                    throw new ValidatorException(
                        "La nuova data di fine causa sovrapposizione con un altro esercizio.");

                fiscalYear.EndDate = dto.EndDate.Value;
            }

            fiscalYear.Trace(currentUser);
            await session.SaveOrUpdateAsync(fiscalYear, ct);
            await session.FlushAsync(ct);

            _cache.Clear(CacheRegion);
            return fiscalYear;
        }

        // ─────────────────────────────────────────────
        // COMMAND — TRANSIZIONI DI STATO
        // ─────────────────────────────────────────────

        /// <inheritdoc />
        public async Task<bool> StartClosingAsync(
            int fiscalYearId,
            IUser currentUser,
            string? notes = null,
            CancellationToken ct = default)
        {
            var fiscalYear = await GetByIdAsync(fiscalYearId, currentUser, ct);
            if (fiscalYear == null)
                throw new NotFoundException("Esercizio fiscale non trovato.");

            if (fiscalYear.Status?.Id != FiscalYearStatus.Open)
                throw new ValidatorException(
                    $"Impossibile avviare la chiusura: lo stato corrente è '{fiscalYear.Status?.Name}'. " +
                    "L'esercizio deve essere in stato Open.");

            var closingStatus = await session.GetAsync<FiscalYearStatus>(FiscalYearStatus.Closing, ct)
                ?? throw new ValidatorException("Stato 'Closing' non trovato nella tabella di lookup.");

            fiscalYear.Status = closingStatus;
            fiscalYear.ClosingDate = DateTime.UtcNow;
            if (notes != null) fiscalYear.ClosingNotes = notes;

            fiscalYear.Trace(currentUser);
            await session.SaveOrUpdateAsync(fiscalYear, ct);
            await session.FlushAsync(ct);

            _cache.Clear(CacheRegion);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> CloseAsync(
            int fiscalYearId,
            IUser currentUser,
            string? notes = null,
            CancellationToken ct = default)
        {
            var fiscalYear = await GetByIdAsync(fiscalYearId, currentUser, ct);
            if (fiscalYear == null)
                throw new NotFoundException("Esercizio fiscale non trovato.");

            if (fiscalYear.Status?.Id != FiscalYearStatus.Open && fiscalYear.Status?.Id != FiscalYearStatus.Closing)
                throw new ValidatorException(
                    $"Impossibile chiudere l'esercizio: lo stato corrente è '{fiscalYear.Status?.Name}'. " +
                    "L'esercizio deve essere in stato Open o Closing.");

            // Verifica che esista un budget Consuntivo approvato (o chiuso) per questo esercizio
            var hasApprovedConsuntivo = await session.Query<Budget>()
                .AnyAsync(x => x.FiscalYear.Id == fiscalYearId
                            && x.Type == BudgetType.Consuntivo
                            && (x.Status.Id == BudgetStatus.Approved || x.Status.Id == BudgetStatus.Closed)
                            && !x.IsDeleted, ct)
                .ConfigureAwait(false);

            if (!hasApprovedConsuntivo)
                throw new ValidatorException(
                    "Impossibile chiudere l'esercizio: è necessario approvare il budget Consuntivo prima di procedere alla chiusura.");

            // Verifica spese in stato provvisorio
            var pendingExpenses = await session.Query<Expense>()
                .CountAsync(x => x.FiscalYear.Id == fiscalYearId
                              && x.PaymentStatus.Id== ExpensePaymentStatus.PagataParzialmente
                              && !x.IsDeleted, ct);

            if (pendingExpenses > 0)
                throw new ValidatorException(
                    $"Impossibile chiudere l'esercizio: esistono {pendingExpenses} " +
                    "spese in stato provvisorio. Confermarle o eliminarle prima di procedere.");

            var closedStatus = await session.GetAsync<FiscalYearStatus>(FiscalYearStatus.Closed, ct)
                ?? throw new ValidatorException("Stato 'Closed' non trovato nella tabella di lookup.");

            fiscalYear.Status = closedStatus;
            fiscalYear.IsActive = false;
            fiscalYear.ClosedDate = DateTime.UtcNow;
            fiscalYear.ClosedByUserId = currentUser.Id;
            if (notes != null) fiscalYear.ClosingNotes = notes;

            fiscalYear.Trace(currentUser);
            await session.SaveOrUpdateAsync(fiscalYear, ct);
            await session.FlushAsync(ct);

            _cache.Clear(CacheRegion);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> LockAsync(
            int fiscalYearId,
            IUser currentUser,
            string? notes = null,
            CancellationToken ct = default)
        {
            var fiscalYear = await GetByIdAsync(fiscalYearId, currentUser, ct);
            if (fiscalYear == null)
                throw new NotFoundException("Esercizio fiscale non trovato.");

            if (fiscalYear.Status?.Id != FiscalYearStatus.Closed)
                throw new ValidatorException(
                    $"Impossibile bloccare l'esercizio: lo stato corrente è '{fiscalYear.Status?.Name}'. " +
                    "L'esercizio deve essere in stato Closed.");

            var lockedStatus = await session.GetAsync<FiscalYearStatus>(FiscalYearStatus.Locked, ct)
                ?? throw new ValidatorException("Stato 'Locked' non trovato nella tabella di lookup.");

            fiscalYear.Status = lockedStatus;
            fiscalYear.LockedDate = DateTime.UtcNow;
            if (notes != null) fiscalYear.ClosingNotes = notes;

            fiscalYear.Trace(currentUser);
            await session.SaveOrUpdateAsync(fiscalYear, ct);
            await session.FlushAsync(ct);

            _cache.Clear(CacheRegion);
            return true;
        }

        // ─────────────────────────────────────────────
        // COMMAND — DELETE (soft)
        // ─────────────────────────────────────────────

        /// <summary>
        /// Soft delete di un esercizio. Consentito solo su esercizi in stato Draft/Open
        /// senza movimenti associati.
        /// </summary>
        public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken ct)
        {
            var fiscalYear = await GetByIdAsync(id, currentUser, ct);
            if (fiscalYear == null) return false;

            if (fiscalYear.Status?.Id != FiscalYearStatus.Draft)
                throw new ValidatorException(
                    "È possibile eliminare solo un esercizio in stato Bozza.");

            fiscalYear.IsDeleted = true;
            fiscalYear.IsActive = false;
            fiscalYear.Trace(currentUser);

            await session.SaveOrUpdateAsync(fiscalYear, ct);
            await session.FlushAsync(ct);

            _cache.Clear(CacheRegion);
            return true;
        }

        // ─────────────────────────────────────────────
        // UTILITY
        // ─────────────────────────────────────────────

        /// <inheritdoc />
        public async Task<DocumentDateWarningDto?> CheckDocumentDateAsync(
            int fiscalYearId,
            DateTime documentDate,
            IUser currentUser,
            CancellationToken ct = default)
        {
            var fiscalYear = await GetByIdAsync(fiscalYearId, currentUser, ct);
            if (fiscalYear == null) return null;

            var isOutOfRange = documentDate < fiscalYear.StartDate || documentDate > fiscalYear.EndDate;

            FiscalYearListItemDto? suggested = null;
            string? warningMessage = null;

            if (isOutOfRange)
            {
                // Cerca l'esercizio corretto per competenza
                var suggestedFy = await session.Query<FiscalYear>()
                    .Where(x => x.Condominium.Id == fiscalYear.Condominium.Id
                             && x.StartDate <= documentDate
                             && x.EndDate >= documentDate
                             && !x.IsDeleted
                             && x.Status.Id != FiscalYearStatus.Locked)
                    .FirstOrDefaultAsync(ct);

                if (suggestedFy != null)
                {
                    suggested = new FiscalYearListItemDto
                    {
                        Id = suggestedFy.Id,
                        Code = suggestedFy.Code,
                        Description = suggestedFy.Description,
                        StartDate = suggestedFy.StartDate,
                        EndDate = suggestedFy.EndDate,
                        StatusId = suggestedFy.Status?.Id ?? 0,
                        StatusName = suggestedFy.Status?.Name ?? string.Empty,
                        IsActive = suggestedFy.IsActive
                    };

                    warningMessage =
                        $"La data documento ({documentDate:dd/MM/yyyy}) cade fuori dal periodo dell'esercizio selezionato " +
                        $"({fiscalYear.StartDate:dd/MM/yyyy} - {fiscalYear.EndDate:dd/MM/yyyy}). " +
                        $"Per competenza appartiene all'esercizio '{suggestedFy.Code}'.";
                }
                else
                {
                    warningMessage =
                        $"La data documento ({documentDate:dd/MM/yyyy}) cade fuori dal periodo dell'esercizio selezionato " +
                        $"({fiscalYear.StartDate:dd/MM/yyyy} - {fiscalYear.EndDate:dd/MM/yyyy}) " +
                        "e non è stato trovato un esercizio corrispondente per competenza.";
                }
            }

            return new DocumentDateWarningDto
            {
                IsOutOfRange = isOutOfRange,
                DocumentDate = documentDate,
                FiscalYearStart = fiscalYear.StartDate,
                FiscalYearEnd = fiscalYear.EndDate,
                SuggestedFiscalYear = suggested,
                WarningMessage = warningMessage
            };
        }

        /// <inheritdoc />
        public async Task<bool> HasOverlapAsync(
            int condominiumId,
            DateTime startDate,
            DateTime endDate,
            int? excludeId = null,
            CancellationToken ct = default)
        {
            var query = session.Query<FiscalYear>()
                .Where(x => x.Condominium.Id == condominiumId
                         && !x.IsDeleted
                         && x.StartDate < endDate
                         && x.EndDate > startDate);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync(ct);
        }


        public Task<FiscalYear> CreateAsync(FiscalYear entity, IUser currentUser, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<FiscalYear> UpdateAsync(FiscalYear entity, IUser currentUser, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync(Expression<Func<FiscalYear, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<(IList<FiscalYear> Items, int TotalCount)> GetPagedAsync(Expression<Func<FiscalYear, bool>> filter, int pageNumber, int pageSize, Expression<Func<FiscalYear, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
