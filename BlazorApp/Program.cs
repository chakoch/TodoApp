using BlazorApp;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// För Blazor WebAssembly måste vi använda externa URL:er eftersom det körs i browsern
var apiBaseUrl = builder.HostEnvironment.IsProduction()
    ? "https://todoapp-api-271428348092.europe-west1.run.app"  // Din riktiga GCP Cloud Run URL
    : "https://localhost:4000";

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

await builder.Build().RunAsync();
