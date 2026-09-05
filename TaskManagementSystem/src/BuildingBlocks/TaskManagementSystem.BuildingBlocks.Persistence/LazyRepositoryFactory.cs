namespace TaskManagementSystem.BuildingBlocks.Persistence;

public static class LazyRepositoryFactory
{
    public static Lazy<T> Create<T>(Func<T> factory) =>
        new(factory, LazyThreadSafetyMode.ExecutionAndPublication);
}
