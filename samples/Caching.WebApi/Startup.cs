using Caching.Extensions;
using Caching.WebApi.Models;
using Microsoft.OpenApi.Models;

namespace Caching.WebApi;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Caching.WebApi", Version = "v1" });
        });

        services.AddCache(Configuration)
            .AddEntity<Sample>("sample")
            .AddEntity<List<Sample>>("samples")
            .AddEntity<Sample[]>("samples");
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Caching.WebApi v1"));

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
