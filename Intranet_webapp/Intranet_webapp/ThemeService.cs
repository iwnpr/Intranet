using Microsoft.JSInterop;

namespace Intranet_webapp
{
    public class ThemeService(IJSRuntime js)
    {
        private readonly IJSRuntime _js = js;
        private string _currentTheme = "light-mode";

        public string CurrentTheme => _currentTheme;

        public async Task SetThemeAsync(string themeClass)
        {
            _currentTheme = themeClass;
            await _js.InvokeVoidAsync("themeService.setBodyClass", themeClass);
        }

        public async Task LoadSavedThemeAsync()
        {
            var savedTheme = await _js.InvokeAsync<string>("themeService.getSavedTheme");
            if (!string.IsNullOrEmpty(savedTheme))
            {
                _currentTheme = savedTheme;
                await SetThemeAsync(savedTheme);
            }
        }

        public async Task ToggleThemeAsync()
        {
            var newTheme = _currentTheme == "dark-mode" ? "light-mode" : "dark-mode";
            await SetThemeAsync(newTheme);
        }
    }

}
