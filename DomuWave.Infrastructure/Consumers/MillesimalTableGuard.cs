using CPQ.Core.Exceptions;
using DomuWave.Services.Models;
using NHibernate;
using NHibernate.Linq;

namespace DomuWave.Services.Consumers;

/// <summary>
/// Verifica l'integrità della tabella millesimale abilitata di un condominio
/// prima di eseguire operazioni che dipendono dalla sua correttezza
/// (approvazione budget, generazione rate, calcolo conguaglio).
///
/// Anomalie rilevate:
///   - Nessuna tabella millesimale abilitata per il condominio
///   - La tabella non ha voci (nessun UnitMillesimal)
///   - La somma dei millesimi delle voci si discosta di più di 0,01 dal TotalMillesimal dichiarato
/// </summary>
internal static class MillesimalTableGuard
{
    /// <summary>Tolleranza di arrotondamento accettata sulla somma dei millesimi.</summary>
    private const decimal Tolerance = 0.01m;

    /// <summary>
    /// Carica la tabella millesimale abilitata del condominio e verifica che sia integra.
    /// Lancia <see cref="ValidatorException"/> se sono presenti anomalie bloccanti.
    /// </summary>
    /// <returns>
    /// La <see cref="MillesimalTable"/> abilitata insieme alle sue voci <see cref="UnitMillesimal"/>.
    /// </returns>
    public static async Task<(MillesimalTable Table, IList<UnitMillesimal> UnitMillesimals)>
        LoadAndValidateAsync(ISession session, int condominiumId, CancellationToken ct)
    {
        // 1. Esiste almeno una tabella millesimale abilitata?
        var table = await session.Query<MillesimalTable>()
            .Where(x => x.Condominium.Id == condominiumId && x.IsEnabled && !x.IsDeleted)
            .OrderBy(x => x.Code)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (table == null)
            throw new ValidatorException(
                "Impossibile procedere: nessuna tabella millesimale abilitata trovata per questo condominio. " +
                "Abilitare almeno una tabella millesimale prima di continuare.");

        // 2. La tabella ha voci?
        var unitMillesimals = await session.Query<UnitMillesimal>()
            .Where(x => x.MillesimalTable.Id == table.Id && !x.IsDeleted)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        if (!unitMillesimals.Any())
            throw new ValidatorException(
                $"Impossibile procedere: la tabella millesimale '{table.Code} – {table.Name}' " +
                "non ha voci assegnate alle unità. Compilare le voci millesimali prima di continuare.");

        // 3. Il totale dei millesimi è coerente con TotalMillesimal?
        if (table.TotalMillesimal > 0)
        {
            var sumCalc = unitMillesimals.Sum(x => x.Millesimal);
            if (Math.Abs(sumCalc - table.TotalMillesimal) > Tolerance)
                throw new ValidatorException(
                    $"Impossibile procedere: la tabella millesimale '{table.Code} – {table.Name}' " +
                    $"presenta un'anomalia nel totale dei millesimi " +
                    $"(somma voci: {sumCalc:0.###}, totale dichiarato: {table.TotalMillesimal:0.###}). " +
                    "Correggere le voci millesimali prima di continuare.");
        }

        return (table, unitMillesimals);
    }
}
