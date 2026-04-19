using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyPlaces.Client;
using MyPlaces.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<AuthState>();
builder.Services.AddTransient<AuthTokenHandler>();

var apiBase = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5160/";
if (!apiBase.EndsWith('/'))
    apiBase += "/";

builder.Services.AddHttpClient("Api", client => client.BaseAddress = new Uri(apiBase))
    .AddHttpMessageHandler<AuthTokenHandler>();
builder.Services.AddHttpClient("ApiAnonymous", client => client.BaseAddress = new Uri(apiBase));

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
