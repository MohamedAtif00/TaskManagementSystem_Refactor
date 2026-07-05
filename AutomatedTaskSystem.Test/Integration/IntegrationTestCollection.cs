namespace AutomatedTaskSystem.Test.Integration;

[CollectionDefinition("Integration", DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>;
