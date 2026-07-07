using System.Diagnostics;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VanDerTil.ContosoUniversity.Web.Infrastructure;
using VanDerTil.ContosoUniversity.Web.Infrastructure.DataAccess;
using VanDerTil.ContosoUniversity.Web.Infrastructure.Filters;
using VanDerTil.ContosoUniversity.Web.Infrastructure.Middleware;
using VanDerTil.ContosoUniversity.Web.Infrastructure.Requests;

namespace VanDerTil.ContosoUniversity.Web;

public static class ApplicationComposition
{
    public static void ConfigureServices<TBuilder>(TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.AddServiceDefaults();
        builder.AddNpgsqlDataSource("contosodb");

        builder.Services.AddScoped<IRequestDispatcher, ReflectionRequestDispatcher>();

        // Register all IRequestHandlers in the assembly into services.
        // This uses reflection to find all classes that implement IRequestHandler<,> and registers them with the DI container.
        // Slow, but works.
        // AOT friendly would be using a source generator to generate the registration code at compile time.
        typeof(Program).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
            .Where(x => x.Interface.IsGenericType && x.Interface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
            .ToList()
            .ForEach(x => builder.Services.AddScoped(x.Interface, x.Type));

        builder.Services.AddTransient<FluentValidationActionFilter>();

        builder.Services
            .AddMvc(opts =>
            {
                opts.Filters.AddService<FluentValidationActionFilter>(500);
                opts.Filters.Add<ModelStateActionFilter>(600);
            })
            .AddRazorOptions(opts =>
            {
                opts.ViewLocationExpanders.Add(new FeatureFolderViewLocationExpander());
            })
            .AddViewLocalization();

        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        builder.Services.AddLocalization();
        builder.Services.AddRouting(opts => opts.LowercaseUrls = true);
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();

        builder.Services.AddScoped<IDatabaseSessionManager, DbDataSourceDatabaseSessionManager>();
        builder.Services.AddScoped(container =>
        {
            IDatabaseSessionManager database = container.GetRequiredService<IDatabaseSessionManager>();

            Debug.Assert(database.CurrentSession is not null, $"{nameof(IDatabaseSessionManager.CurrentSession)} was unexpectedly null");

            return database.CurrentSession;
        });
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseForwardedHeaders();
        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapDefaultEndpoints();

        app.UseMiddleware<DatabaseSessionMiddleware>();

        app.MapStaticAssets();
        app.MapControllers()
            .WithStaticAssets();
    }
}
