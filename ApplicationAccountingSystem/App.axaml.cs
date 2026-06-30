using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System;

namespace ApplicationAccountingSystem
{
    public partial class App : Avalonia.Application
    {
        public static bool IsLightTheme { get; private set; } = true;

        public static event EventHandler<bool>? ThemeChanged;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            RequestedThemeVariant = ThemeVariant.Light;
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        public static void ToggleTheme()
        {
            IsLightTheme = !IsLightTheme;
            if (Current != null)
            {
                Current.RequestedThemeVariant = IsLightTheme ? ThemeVariant.Light : ThemeVariant.Dark;
            }
            ThemeChanged?.Invoke(null, IsLightTheme);
        }
    }
}
