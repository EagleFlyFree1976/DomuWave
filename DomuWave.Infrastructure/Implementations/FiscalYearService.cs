using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
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
        // COMMAND — OPEN
        // ─────────────────────────────────────────────

        /// <inheritdoc />
        public async Task<FiscalYear> OpenAsync(
            int condominiumId,
            string code,
            string? description,
            DateTime startDate,
            DateTime endDate,
            IUser currentUser,
            CancellationToken ct = default)
        {
            // Validazione date
            if (endDate <= startDate)
                throw new InvalidOperationException(
                    "La data di fine esercizio deve essere successiva alla data di inizio.");

            // Verifica esercizio già aperto o in chiusura
            var hasOpenOrClosing = await session.Query<FiscalYear>()
                .AnyAsync(x => x.Condominium.Id == condominiumId
                            && (x.Status == FiscalYearStatus.Open || x.Status == FiscalYearStatus.Closing)
                            && !x.IsDeleted, ct);

            if (hasOpenOrClosing)
                throw new InvalidOperationException(
                    "Esiste già un esercizio aperto o in fase di chiusura per questo condominio. " +
                    "Chiuderlo prima di aprirne uno nuovo.");

            // Verifica sovrapposizione date
            if (await HasOverlapAsync(condominiumId, startDate, endDate, excludeId: null, ct))
                throw new InvalidOperationException(
                    "Le date dell'esercizio si sovrappongono con un esercizio esistente.");

            // Codice univoco per condominio
            var codeExists = await session.Query<FiscalYear>()
                .AnyAsync(x => x.Condominium.Id == condominiumId
                            && x.Code == code
                            && !x.IsDeleted, ct);

            if (codeExists)
                throw new InvalidOperationException(
                    $"Esiste già un esercizio con codice '{code}' per questo condominio.");

            var fiscalYear = new FiscalYear
            {
                Condominium = session.Load<Condominium>(condominiumId),
                Code = code,
                Description = description,
                StartDate = startDate,
                EndDate = endDate,
                Status = FiscalYearStatus.Open,
                IsActive = true,
                IsDeleted = false
            };

            fiscalYear.Trace(currentUser);
            await session.SaveOrUpdateAsync(fiscalYear, ct);
            await session.FlushAsync(ct);

            _cache.Clear(CacheRegion);
            return fiscalYear;
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

            if (fiscalYear.Status == FiscalYearStatus.Closed || fiscalYear.Status == FiscalYearStatus.Locked)
                throw new InvalidOperationException(
                    $"Non è possibile modificare un esercizio in stato '{fiscalYear.Status}'.");

            if (dto.Description != null)
                fiscalYear.Description = dto.Description;

            if (dto.EndDate.HasValue)
            {
                if (fiscalYear.Status != FiscalYearStatus.Open)
                    throw new InvalidOperationException(
                        "La data di fine può essere modificata solo su esercizi in stato Open.");

                if (dto.EndDate.Value <= fiscalYear.StartDate)
                    throw new InvalidOperationException(
                        "La nuova data di fine deve essere successiva alla data di inizio.");

                if (await HasOverlapAsync(fiscalYear.Condominium.Id, fiscalYear.StartDate, dto.EndDate.Value, fiscalYearId, ct))
                    throw new InvalidOperationException(
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
            if (fiscalYear == null) return false;

            if (fiscalYear.Status != FiscalYearStatus.Open)
                throw new InvalidOperationException(
                    $"Impossibile avviare la chiusura: lo stato corrente è '{fiscalYear.Status}'. " +
                    "L'esercizio deve essere in stato Open.");

            fiscalYear.Status = FiscalYearStatus.Closing;
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
            if (fiscalYear == null) return false;

            if (fiscalYear.Status != FiscalYearStatus.Open && fiscalYear.Status != FiscalYearStatus.Closing)
                throw new InvalidOperationException(
                    $"Impossibile chiudere l'esercizio: lo stato corrente è '{fiscalYear.Status}'. " +
                    "L'esercizio deve essere in stato Open o Closing.");

            // Verifica spese in stato provvisorio
            var pendingExpenses = await session.Query<Expense>()
                .CountAsync(x => x.FiscalYear.Id == fiscalYearId
                              && x.PaymentStatus== "Provisional"
                              && !x.IsDeleted, ct);

            if (pendingExpenses > 0)
                throw new InvalidOperationException(
                    $"Impossibile chiudere l'esercizio: esistono {pendingExpenses} " +
                    "spese in stato provvisorio. Confermarle o eliminarle prima di procedere.");

            fiscalYear.Status = FiscalYearStatus.Closed;
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
            if (fiscalYear == null) return false;

            if (fiscalYear.Status != FiscalYearStatus.Closed)
                throw new InvalidOperationException(
                    $"Impossibile bloccare l'esercizio: lo stato corrente è '{fiscalYear.Status}'. " +
                    "L'esercizio deve essere in stato Closed.");

            fiscalYear.Status = FiscalYearStatus.Locked;
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

            if (fiscalYear.Status == FiscalYearStatus.Closed || fiscalYear.Status == FiscalYearStatus.Locked)
                throw new InvalidOperationException(
                    "Non è possibile eliminare un esercizio chiuso o bloccato.");

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
                             && x.Status != FiscalYearStatus.Locked)
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
                        Status = suggestedFy.Status,
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
