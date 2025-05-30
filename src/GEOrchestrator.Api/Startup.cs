using GEOrchestrator.Api.Formatters;
using GEOrchestrator.Business.Extensions;
using GEOrchestrator.Business.Requests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GEOrchestrator.Api
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
            services.AddControllers(options =>
            {
                options.InputFormatters.Insert(0, new YamlInputFormatter());
            });
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RunJobRequest).Assembly));
            services.AddServices();
            services.AddFactories();

            services.AddObjectStorageProviders(Configuration);
            services.AddDatabaseRepositories(Configuration);
            services.AddContainerProviders(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
