using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MessageBoardService.Models;

namespace MessageBoardService.Tests.Utilities
{
    public class TestWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup: class
    {
        private void InitializeDbForTests(MessageDBContext db)
        {
            var dbMessages = new List<Message>()
            {
                new Message() {
                    Id = 1,
                    UserId = 1,
                    CreationDateTime = DateTime.Now,
                    Title = "Test message title #1",
                    Text = "Test message text #1"
                },
                new Message() {
                    Id = 2,
                    UserId = 2,
                    CreationDateTime = DateTime.Now,
                    Title = "Test message title #2",
                    Text = "Test message text #2"
                }
            };

            db.Messages.AddRange(dbMessages);
            db.SaveChanges();
        }

        public void ReinitializeDbForTests(MessageDBContext db)
        {
            db.Messages.RemoveRange(db.Messages);
            InitializeDbForTests(db);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's MessageDBContext registration.
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<MessageDBContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add MessageDBContext using an in-memory database for testing.
                services.AddDbContext<MessageDBContext>(options =>
                {
                    options.UseInMemoryDatabase("MessageBoard_Test");
                });

                // Build the service provider.
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database
                // context (MessageDBContext).
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<MessageDBContext>();
                    var logger = scopedServices
                        .GetRequiredService<ILogger<TestWebApplicationFactory<TStartup>>>();

                    // Ensure the database is created.
                    db.Database.EnsureCreated();

                    try
                    {
                        // Seed the database with test data.
                        ReinitializeDbForTests(db);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred seeding the " +
                            "database with test messages. Error: {Message}", ex.Message);
                    }
                }
            });
        }
    }
}