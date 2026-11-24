using System.Text.Encodings.Web;
using Api.Endpoints;
using Api.Errors;
using Api.Validation;
using Core;
using Core.Abstractions;
using Core.Services;
using Core.Storage;
using FluentValidation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping);

builder.Services.AddSingleton<ICatalogStore>(_ => new InMemoryCatalogStore(SampleData.Catalog()));
builder.Services.AddScoped<CatalogService>();
builder.Services.AddValidatorsFromAssemblyContaining<SaveBookRequestValidator>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapPost("/dev/fail", () => { throw new InvalidOperationException("Тестовий збій: внутрішні деталі сервера"); })
        .ExcludeFromDescription();
}

app.MapGet("/health", () => TypedResults.Ok(new { status = "ok" })).WithTags("Health");
app.MapBooks();

app.Run();

public partial class Program;
