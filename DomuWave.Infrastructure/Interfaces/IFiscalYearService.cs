using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;
using DomuWave.Services.Dto.Contabilita.FiscalYear;

namespace DomuWave.Services.Interfaces
{
    /// <summary>
    /// Gestione degli esercizi condominiali (FiscalYear).
    /// Un esercizio definisce il periodo temporale entro cui vengono registrati
    /// spese, rate e incassi di un condominio.
    /// Ciclo di vita: Open → Closing → Closed → Locked.
    /// Vincolo: un solo esercizio con IsActive = true per condominio alla volta.
    /// </summary>
    public interface IFiscalYearService : IBaseService<FiscalYear, int>
    {
        /// <summary>
        /// Restituisce l'esercizio attualmente attivo (IsActive = true) per il condominio specificato.
        /// Ritorna null se nessun esercizio è attivo.
        /// </summary>
        Task<FiscalYear?> GetActiveAsync(int condominiumId, IUser currentUser, CancellationToken ct = default);

        /// <summary>
        /// Restituisce tutti gli esercizi di un condominio, ordinati per StartDate discendente.
        /// </summary>
        Task<IList<FiscalYear>> GetByCondominiumAsync(int condominiumId, IUser currentUser, CancellationToken ct = default);

        /// <summary>
        /// Crea un nuovo esercizio in stato Draft per il condominio specificato.
        /// Verifica che le date non si sovrappongano con esercizi esistenti e che EndDate > StartDate.
        /// Non verifica esercizi aperti (si può avere più bozze).
        /// </summary>
        Task<FiscalYear> OpenAsync(
            int condominiumId,
            string code,
            string? description,
            DateTime startDate,
            DateTime endDate,
            IUser currentUser,
            CancellationToken ct = default,
            int? previousFiscalYearId = null);

        /// <summary>
        /// Transizione Draft → Open.
        /// Verifica che non esista già un esercizio Open o Closing per il condominio.
        /// </summary>
        Task<bool> OpenFromDraftAsync(int fiscalYearId, IUser currentUser, CancellationToken ct = default);

        /// <summary>
        /// Aggiorna descrizione e/o data di fine di un esercizio in stato Open.
        /// Non è consentito modificare esercizi in stato Closing, Closed o Locked.
        /// Ritorna null se l'esercizio non viene trovato.
        /// </summary>
        Task<FiscalYear?> UpdateAsync(
            int fiscalYearId,
            FiscalYearUpdateDto dto,
            IUser currentUser,
            CancellationToken ct = default);

        /// <summary>
        /// Porta l'esercizio in stato Closing.
        /// L'esercizio rimane tecnicamente aperto a registrazioni di rettifica
        /// (es. fatture in ritardo con data documento nel periodo precedente).
        /// Prerequisito: Status = Open.
        /// </summary>
        Task<bool> StartClosingAsync(int fiscalYearId, IUser currentUser, string? notes = null, CancellationToken ct = default);

        /// <summary>
        /// Chiude definitivamente l'esercizio (Status = Closed).
        /// IsActive passa a false, rendendo possibile l'apertura di un nuovo esercizio.
        /// Prerequisito: Status = Open o Closing.
        /// Verifica che non esistano spese in stato provvisorio pendente.
        /// Ritorna false se l'esercizio non viene trovato o i prerequisiti non sono soddisfatti.
        /// </summary>
        Task<bool> CloseAsync(int fiscalYearId, IUser currentUser, string? notes = null, CancellationToken ct = default);

        /// <summary>
        /// Blocca definitivamente l'esercizio (Status = Locked).
        /// Nessuna modifica è consentita dopo questo stato.
        /// Tipicamente invocato dopo l'approvazione del bilancio consuntivo in assemblea.
        /// Prerequisito: Status = Closed.
        /// </summary>
        Task<bool> LockAsync(int fiscalYearId, IUser currentUser, string? notes = null, CancellationToken ct = default);

        /// <summary>
        /// Riporta un esercizio dallo stato Open a Draft.
        /// Consentito solo se è il primo esercizio del condominio e non ha movimenti registrati
        /// (nessuna spesa, nessuna rata, nessun budget approvato).
        /// Usato per correggere i saldi iniziali prima della riapertura.
        /// </summary>
        Task<bool> RevertToDraftAsync(int fiscalYearId, IUser currentUser, CancellationToken ct = default);

        /// <summary>
        /// Verifica se una data documento cade fuori dal range dell'esercizio indicato.
        /// Restituisce un DTO con l'esito del controllo e l'eventuale esercizio suggerito per competenza.
        /// Usato dal frontend per mostrare un warning prima di confermare la registrazione di una spesa.
        /// </summary>
        Task<DocumentDateWarningDto?> CheckDocumentDateAsync(
            int fiscalYearId,
            DateTime documentDate,
            IUser currentUser,
            CancellationToken ct = default);

        /// <summary>
        /// Verifica se due periodi di date si sovrappongono con esercizi esistenti dello stesso condominio.
        /// Usato internamente da OpenAsync e UpdateAsync.
        /// </summary>
        Task<bool> HasOverlapAsync(int condominiumId, DateTime startDate, DateTime endDate, int? excludeId = null, CancellationToken ct = default);
    }
}
