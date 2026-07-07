using Microsoft.AspNetCore.Builder;
using VanDerTil.ContosoUniversity.Web;

var builder = WebApplication.CreateBuilder(args);

ApplicationComposition.ConfigureServices(builder);

var app = builder.Build();

ApplicationComposition.ConfigurePipeline(app);

app.Run();
