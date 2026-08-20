using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using SistemaOrdemServico.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => 
{
    var nav = sp.GetRequiredService<NavigationManager>();
    var baseUri = nav.BaseUri;

    // Se estiver no Codespaces (substitui a porta 5050 pela 5000 na URL inteira)
    if (baseUri.Contains("-5050."))
    {
        baseUri = baseUri.Replace("-5050.", "-5000.");
    }
    else if (baseUri.Contains(":5050"))
    {
        baseUri = baseUri.Replace(":5050", ":5000");
    }
    else
    {
        baseUri = "http://localhost:5000/";
    }

    // Garante que a URL termine com /
    if (!baseUri.EndsWith("/"))
    {
        baseUri += "/";
    }

    return new HttpClient { BaseAddress = new Uri(baseUri) };
});

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthStateProvider>());

await builder.Build().RunAsync();