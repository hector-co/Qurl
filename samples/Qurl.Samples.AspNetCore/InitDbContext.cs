namespace Qurl.Samples.AspNetCore
{
    using global::Qurl.Samples.AspNetCore.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    namespace Qurl.Samples.AspNetCore
    {
        public class InitDbContext : IHostedService
        {
            private readonly IServiceProvider _serviceProvider;

            public InitDbContext(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SampleContext>();

                await context.Database.MigrateAsync(cancellationToken);
                if (!await context.Set<Person>().AnyAsync(cancellationToken))
                {
                    context.AddRange(new Person
                    {
                        Name = "Person1",
                        Birthday = new DateTime(2020, 1, 10).ToUniversalTime(),
                        Group = new Group
                        {
                            Title = "Group1",
                            Description = "Group1",
                            Active = true,
                        },
                        Active = true,
                        CreationDate = DateTime.UtcNow
                    },
                    new Person
                    {
                        Name = "Person2",
                        Birthday = new DateTime(2021, 2, 20).ToUniversalTime(),
                        Group = new Group
                        {
                            Title = "Group2",
                            Description = "Group2",
                            Active = true,
                        },
                        Active = true,
                        CreationDate = DateTime.UtcNow
                    },
                    new Person
                    {
                        Name = "Person3",
                        Birthday = new DateTime(2022, 3, 30).ToUniversalTime(),
                        CreationDate = DateTime.UtcNow
                    });

                    await context.SaveChangesAsync(cancellationToken);
                }
            }

            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        }
    }

}
