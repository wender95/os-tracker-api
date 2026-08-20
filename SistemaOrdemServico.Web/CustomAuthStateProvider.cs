using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace SistemaOrdemServico.Web;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private AuthenticationState _anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public CustomAuthStateProvider(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            var email = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userEmail");
            var setor = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userSetor");
            var role = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userRole");

            if (string.IsNullOrWhiteSpace(token))
                return _anonymous;

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, email ?? "Usuario"),
                new(ClaimTypes.Role, role ?? "Operador"),
                new("Setor", setor ?? "Criacao")
            };

            var identity = new ClaimsIdentity(claims, "CustomAuth");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return _anonymous;
        }
    }

    public async Task MarkUserAsAuthenticated(string token, string email, string setor, string role)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userEmail", email);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userSetor", setor);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userRole", role);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userEmail");
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userSetor");
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userRole");

        NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
        }
        catch
        {
            return null;
        }
    }
}