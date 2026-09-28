using Microsoft.EntityFrameworkCore;
using PulsePoll.Data;
using PulsePoll.Data.Repositories;
using PulsePoll.Hubs;
using PulsePoll.Middleware;
using PulsePoll.PollCodes;
using PulsePoll.Services.Caching;
using PulsePoll.Services.Polls;
using PulsePoll.Services.Templates;
using PulsePoll.Validation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRClient", policy =>
    {
        policy
            .WithOrigins("http://127.0.0.1:5500")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSignalR();
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<PulsePollDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();

builder.Services.AddScoped<TemplateValidator>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IPollService, PollService>();

builder.Services.AddSingleton<IPollCacheStore, MemoryPollCacheStore>();
builder.Services.AddSingleton<PollCodeGenerator>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "PulsePoll API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseCors("SignalRClient");
app.MapControllers();
app.MapHub<PollHub>("/hubs/poll");

app.Run();
