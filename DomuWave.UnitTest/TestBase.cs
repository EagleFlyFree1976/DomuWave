using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;

namespace DomuWave.Tests.Helpers;

/// <summary>
/// Classe base per tutti i test, fornisce AutoFixture + Moq e un IUser fittizio.
/// </summary>
public abstract class TestBase
{
    protected readonly IFixture Fixture;

    protected TestBase()
    {
        Fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

        // Evita ricorsione infinita su entità con proprietà circolari
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
               .ToList()
               .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    /// <summary>
    /// Crea un mock generico tramite Moq.
    /// </summary>
    protected Mock<T> MockOf<T>() where T : class => new();
}
