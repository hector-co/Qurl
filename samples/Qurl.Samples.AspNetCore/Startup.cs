using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Qurl.Samples.AspNetCore.Models;

namespace Qurl.Samples.AspNetCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // By default LHS mode
            services.AddQurlModelBinder();

            // Available configs
            // services.AddQurlModelBinder(options => options.UseRhsMode());
            // services.AddQurlModelBinder(options => options.UseLhsMode());

            services.AddDbContext<SampleContext>(options => options.UseSqlServer(Configuration.GetConnectionString("SampleAspNetCore")));

            services.AddSwaggerGen(options =>
            {
                options.AddQurlDefinitions();
            });

            services.AddHostedService<InitData>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
