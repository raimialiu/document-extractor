using DocumentExtractor.Models;
using DocumentExtractor.Services;
using Microsoft.AspNetCore.Mvc;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddControllers();
    builder.Services.Configure<ApiBehaviorOptions>(x =>
    {
        x.SuppressModelStateInvalidFilter = true;
    });
    
    IConfigurationBuilder configurationBuilderbuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
    IConfigurationRoot configuration = configurationBuilderbuilder.Build();
    string formEndpoint = configuration["FormRecognizerSettings:endpoint"];
    string formKey = configuration["FormRecognizerSettings:key"];

    FormRecognizerSettings formRecognizerSettings = new FormRecognizerSettings(formKey, formEndpoint);
    builder.Services.AddSingleton<FormRecognizerSettings>(formRecognizerSettings);
    builder.Services.AddSingleton<DocumentExtractorService>();

    var app = builder.Build();
    app.UseRouting();
    app.UseEndpoints(enpoint =>
    {
        
        enpoint.MapControllers();
    });
    app.MapGet("/", () => "Welcome to document extractor service....");

    app.Run();
}
catch (Exception es)
{
    Console.WriteLine("app failed to start....shutting down");
    throw;
}

