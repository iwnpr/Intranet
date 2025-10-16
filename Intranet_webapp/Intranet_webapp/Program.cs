using Infrastructure_lib;
using Intranet_webapp.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Intranet_webapp;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var serilog = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

builder.Host.UseSerilog(serilog);
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

/* јутентификаци€ с использованием провайдера состо€ни€ */
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<CustomStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomStateProvider>());
builder.Services.AddScoped<ThemeService>();

/* ƒобавление сервисов в контейнер
 * с использованим extentions static классов
 */
builder.Services
    .AdaptersDIExtention(builder.Configuration)
    .ServicesDIExtention();

/* ƒобавление http-клиентов дл€ сервисов */
builder.Services.AddHttpClient("GitClient", x =>
{
    x.BaseAddress = new Uri($"{builder.Configuration.GetValue<string>("GitSettings:BaseUrl")}{builder.Configuration.GetValue<string>("GitSettings:Client:Api")}");
    x.Timeout = TimeSpan.FromMilliseconds(builder.Configuration.GetValue<int>("GitSettings:Client:Timeout"));
    x.DefaultRequestHeaders.Add("Authorization", $"Bearer {builder.Configuration.GetValue<string>("GitSettings:Client:Token")}");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapControllers();


Console.WriteLine($"Current user: {Environment.UserName}");


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Intranet_webapp.Client._Imports).Assembly);

app.Run();