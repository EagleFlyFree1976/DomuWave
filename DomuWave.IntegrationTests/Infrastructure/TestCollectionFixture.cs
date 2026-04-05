using Xunit;

namespace DomuWave.IntegrationTests.Infrastructure;

/// <summary>
/// xUnit collection definition that shares a single IntegrationTestFactory
/// (= one application host + one NHibernate session factory) across all
/// test classes in the "Integration" collection.
///
/// Usage on a test class:
///   [Collection("Integration")]
///   public class MyTests(IntegrationTestFactory factory) { ... }
/// </summary>
[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<IntegrationTestFactory> { }
