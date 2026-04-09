using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using MultiTenantInventory.Application.DTOs;

namespace MultiTenantInventory.Client.Services;

public class AuthStateService : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private string? _token;
    private UserInfoDto? _user;

    public AuthStateService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public string? Token => _token;
    public UserInfoDto? CurrentUser => _user;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _token = await _localStorage.GetItemAsStringAsync("authToken");
        _user = await _localStorage.GetItemAsync<UserInfoDto>("userInfo");

        if (string.IsNullOrEmpty(_token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        // Parse JWT and create claims
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var jwt = handler.ReadJwtToken(_token);
            if (jwt.ValidTo < DateTime.UtcNow)
            {
                await LogoutAsync();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var identity = new ClaimsIdentity(jwt.Claims, "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public async Task LoginAsync(string token, UserInfoDto user)
    {
        _token = token;
        _user = user;
        await _localStorage.SetItemAsStringAsync("authToken", token);
        await _localStorage.SetItemAsync("userInfo", user);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task LogoutAsync()
    {
        _token = null;
        _user = null;
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("userInfo");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
