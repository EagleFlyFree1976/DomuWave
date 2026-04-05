using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;

namespace DomuWave.Tests.Helpers;

/// <summary>
/// Classe base astratta per tutti gli unit test del progetto.
///
/// Configura un'istanza di <see cref="IFixture"/> (AutoFixture) con l'integrazione Moq
/// (<see cref="AutoMoqCustomization"/>) che automatizza la creazione di mock per le
/// dipendenze non esplicitamente specificate.
///
/// Gestisce il comportamento di ricorsione: le entità del dominio con navigazioni circolari
/// (es. Condominium → FiscalYear → Condominium) causerebbero una <see cref="StackOverflowException"/>
/// durante la generazione automatica dei dati; il comportamento <see cref="OmitOnRecursionBehavior"/>
/// tronca la ricorsione restituendo null/default invece di esplodere.
/// </summary>
public abstract class TestBase
{
    /// <summary>
    /// Istanza di AutoFixture condivisa tra tutti i metodi di test della classe derivata.
    /// Usata per generare dati di test casuali e realistici senza hardcoding manuale.
    /// </summary>
    protected readonly IFixture Fixture;

    protected TestBase()
    {
        // AutoMoqCustomization: AutoFixture crea automaticamente Mock<T> per le interfacce
        // e li inietta nelle classi concrete che le richiedono nel costruttore.
        // ConfigureMembers = true → mockano anche i metodi dell'interfaccia (non solo il costruttore).
        Fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

        // Rimuove il comportamento predefinito che lancia eccezione in caso di ricorsione
        // (es. entità con navigazione bidirezionale: Condominium.FiscalYears[0].Condominium...).
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
               .ToList()
               .ForEach(b => Fixture.Behaviors.Remove(b));

        // Sostituisce con OmitOnRecursionBehavior: al posto di esplodere, tronca il grafo
        // restituendo null/default per le proprietà che formerebbero un ciclo.
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    /// <summary>
    /// Crea un <see cref="Mock{T}"/> standard (senza comportamento di default) per il tipo
    /// specificato. Preferito a <c>Fixture.Create&lt;Mock&lt;T&gt;&gt;()</c> perché garantisce
    /// un mock pulito che va configurato esplicitamente test per test (principio di minima
    /// configurazione).
    /// </summary>
    /// <typeparam name="T">Tipo dell'interfaccia o classe da mockare.</typeparam>
    /// <returns>Un nuovo <see cref="Mock{T}"/> con comportamento <c>MockBehavior.Default</c>.</returns>
    protected Mock<T> MockOf<T>() where T : class => new();
}
