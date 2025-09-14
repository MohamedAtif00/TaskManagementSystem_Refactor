
//using AutomatedTaskSystem.Data;
//using Microsoft.EntityFrameworkCore;

//namespace AutomatedTaskSystem.Test
//{
//    public class MonthlyDatabaseOperationWorkerTests
//    {
//        private DataContext GetInMemoryDbContext()
//        {
//            // Use a unique database name for each test to ensure isolation
//            var options = new DbContextOptionsBuilder<DataContext>()
//                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//                .Options;

//            var dbContext = new DataContext(options);
//            dbContext.Database.EnsureCreated(); // Ensure the in-memory database is created
//            return dbContext;
//        }

//        [Fact]
//        public async Task PerformDatabaseOperation_ResetsPermissionAndWorkFromHome()
//        {
//            // Arrange
//            var loggerMock = new Mock<ILogger<MonthlyDatabaseOperationWorker>>();
//            var services = new ServiceCollection();

//            // Add the in-memory DbContext to the service collection
//            services.AddSingleton(GetInMemoryDbContext()); // Use Singleton for this test setup, DbContext will be disposed via scope

//            // Build a service provider to simulate the runtime environment
//            var serviceProvider = services.BuildServiceProvider();

//            // Create an instance of the worker
//            var worker = new MonthlyDatabaseOperationWorker(loggerMock.Object, serviceProvider);

//            // Seed data into the in-memory database
//            using (var scope = serviceProvider.CreateScope())
//            {
//                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//                dbContext.Users.AddRange(new List<User>
//                {
//                    new User { Id = 1, Code = "USER01", Name = "Alice", Permission = 5, WorkFromHome = 2 },
//                    new User { Id = 2, Code = "USER02", Name = "Bob", Permission = 10, WorkFromHome = 3 },
//                    new User { Id = 3, Code = "USER03", Name = "Charlie", Permission = 0, WorkFromHome = 0 } // Already zero
//                });
//                await dbContext.SaveChangesAsync();
//            }

//            // Act
//            // Call the private method using reflection or by making it internal/public for testing
//            // For simplicity, let's assume PerformDatabaseOperation is made internal or public for testing purposes.
//            // If it must remain private, you'd use reflection:
//            // var method = typeof(MonthlyDatabaseOperationWorker).GetMethod("PerformDatabaseOperation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//            // await (Task)method.Invoke(worker, null);
//            await worker.PerformDatabaseOperation(); // Assuming it's made public for testing

//            // Assert
//            using (var scope = serviceProvider.CreateScope())
//            {
//                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//                var users = await dbContext.Users.OrderBy(u => u.Id).ToListAsync();

//                Assert.NotNull(users);
//                Assert.Equal(3, users.Count);

//                Assert.Equal(0, users[0].Permission);
//                Assert.Equal(0, users[0].WorkFromHome);

//                Assert.Equal(0, users[1].Permission);
//                Assert.Equal(0, users[1].WorkFromHome);

//                Assert.Equal(0, users[2].Permission); // Should remain 0
//                Assert.Equal(0, users[2].WorkFromHome); // Should remain 0
//            }

//            // Verify logger calls (optional but good practice)
//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Performing database operation: Resetting Permission and WorkFromHome for all users")),
//                    It.IsAny<Exception>(),
//                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
//                Times.Once);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully reset Permission and WorkFromHome for 3 users.")),
//                    It.IsAny<Exception>(),
//                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
//                Times.Once);
//        }

//        // You might need to make PerformDatabaseOperation public or internal for direct testing.
//        // If you make it internal, add [assembly: InternalsVisibleTo("AutomatedTaskSystem.Tests")]
//        // to your main project's AssemblyInfo.cs or .csproj file.
//    }
//}
