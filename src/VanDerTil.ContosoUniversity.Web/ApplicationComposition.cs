using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VanDerTil.ContosoUniversity.Diagnostics;
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
        Guard.NotNull(builder);

        builder.AddServiceDefaults();
        builder.AddNpgsqlDataSource("contosodb");

        builder.Services.AddLocalization(opts => opts.ResourcesPath = "Resources");
        builder.Services.AddRouting(opts => opts.LowercaseUrls = true);
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();

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

        builder.Services.AddScoped<IDatabaseSessionManager, DbDataSourceDatabaseSessionManager>();
        builder.Services.AddScoped<IDatabaseSession>(container =>
        {
            IDatabaseSessionManager database = container.GetRequiredService<IDatabaseSessionManager>();

            Ensure.That(database.CurrentSession is not null);

            return database.CurrentSession;
        });
        builder.Services.AddScoped<IQueryExecutor, DefaultQueryExecutor>();
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        Guard.NotNull(app);

        app.UseForwardedHeaders();
        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRequestLocalization(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("nl-NL");
            options.SupportedCultures = [new("nl-NL"), new("nl")];
            options.SupportedUICultures = [new("nl-NL"), new("nl")];
        });

        app.MapDefaultEndpoints();

        app.UseMiddleware<DatabaseSessionMiddleware>();

        app.MapStaticAssets();
        app.MapControllers()
            .WithStaticAssets();
    }
}
