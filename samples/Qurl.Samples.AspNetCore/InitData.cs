using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Qurl.Samples.AspNetCore.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Qurl.Samples.AspNetCore
{
    public class InitData : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public InitData(IServiceProvider serviceProvider)
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
                    Birthday = DateTime.Now,
                    Group = new Group
                    {
                        Title = "Group1",
                        Description = "Group1",
                        Active = true,
                    },
                    Active = true,
                },
                new Person
                {
                    Name = "Person2",
                    Birthday = DateTime.Now,
                    Group = new Group
                    {
                        Title = "Group2",
                        Description = "Group2",
                        Active = true,
                    },
                    Active = true,
                },
                new Person
                {
                    Name = "Person3",
                    Birthday = DateTime.Now,
                });

                await context.SaveChangesAsync(cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
