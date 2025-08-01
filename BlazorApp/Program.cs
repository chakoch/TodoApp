using BlazorApp;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// F�r Blazor WebAssembly m�ste vi anv�nda externa URL:er eftersom det k�rs i browsern
var apiBaseUrl = builder.HostEnvironment.IsProduction()
    ? "http://localhost:5000"  // WebAPI �r tillg�nglig p� localhost:5000 fr�n browsern
    : "https://localhost:4000";

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

await builder.Build().RunAsync();
