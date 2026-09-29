using PulsePoll.Mvc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.Configure<PulsePollOptions>(builder.Configuration.GetSection("PulsePollApi"));

builder.Services.AddHttpClient<IPulsePollApiClient, PulsePollApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<PulsePollOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
