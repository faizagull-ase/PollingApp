using Microsoft.EntityFrameworkCore;
using PulsePoll.Data;
using PulsePoll.Data.Repositories;
using PulsePoll.Hubs;
using PulsePoll.Services.Caching;
using PulsePoll.Services.PollCodes;
using PulsePoll.Services.Polls;
using PulsePoll.Services.Templates;
using PulsePoll.Validation;

var builder = WebApplication.CreateBuilder(args);

const string SignalRClientCorsPolicy = "SignalRClient";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PulsePollDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMemoryCache();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy(SignalRClientCorsPolicy, policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:SignalRClient"])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();
builder.Services.AddScoped<IPollRepository, PollRepository>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IPollService, PollService>();
builder.Services.AddSingleton<IPollCacheStore, MemoryPollCacheStore>();
builder.Services.AddSingleton<IPollCodeGenerator, PollCodeGenerator>();
builder.Services.AddSingleton<TemplateValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(SignalRClientCorsPolicy);

app.UseAuthorization();

app.MapControllers();
app.MapHub<PollHub>("/hubs/poll");


app.Run();
