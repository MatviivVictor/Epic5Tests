using System.Reflection;
using System.Text.Json.Serialization;
using Epic5Task.Api;
using Epic5Task.Api.Middlewares;
using Epic5Task.Application;
using Epic5Task.Application.Filters;
using Epic5Task.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => { o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Epic5Task API", Version = "v1" });

    //c.EnableAnnotations();
    c.UseInlineDefinitionsForEnums();

    c.OperationFilter<PhoneNumberHeaderOperationFilter>();
    c.SchemaFilter<AddDescriptionsToSchemaAttributes>();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// Опційно: авто-валидація моделей у контролерах (потрібен FluentValidation.AspNetCore)
// builder.Services.AddFluentValidationAutoValidation();

//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<PhoneNumberHeaderMiddleware>();

app.MapControllers();

app.Run();