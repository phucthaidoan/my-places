using Microsoft.JSInterop;

namespace MyPlaces.Client.Services;

public class AuthState(IJSRuntime js)
{
    private const string StorageKey = "myplaces.jwt";
    private string? _memory;

    public async ValueTask<string?> GetTokenAsync()
    {
        if (_memory is not null)
            return _memory;
        return await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
    }

    public async ValueTask SetTokenAsync(string? token)
    {
        _memory = token;
        if (string.IsNullOrEmpty(token))
            await js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        else
            await js.InvokeVoidAsync("localStorage.setItem", StorageKey, token);
    }

    public void ForgetCachedToken() => _memory = null;
}
