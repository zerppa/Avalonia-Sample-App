namespace MyApp.Testing.CustomStartup;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Threading;

using MyApp.Features.AppSettings;
using MyApp.Features.Shell;
using MyApp.Features.ViewSelector;

/// <summary>
/// Development feature for testing.
/// </summary>
/// <seealso cref="MyApp.App.Feature" />
public class CustomStartupFeature : App.Feature
{
    /// <summary>
    /// Gets or sets the view selector.
    /// </summary>
    [Dependency]
    public IViewSelector? ViewSelector { get; set; }

    /// <summary>
    /// Gets or sets the application settings.
    /// </summary>
    [Dependency]
    public IAppSettings? AppSettings { get; set; }

    /// <inheritdoc />
    public override Task InitializeAsync()
    {
        // This is how you would activate a view when app starts
        // this.Host!.Messenger.Register<MainViewReadyMessage>(this, (_, _) =>
        // {
        //     Dispatcher.UIThread.InvokeAsync(() => this.ViewSelector?.ActivateSettings(), DispatcherPriority.Background);
        // });

        // Push some test settings
        var nonexistant = this.Host!.Settings.GetValue("DoesNotExist", 12345);
        var previouslySaved = this.Host!.Settings.GetValue<int>("TestInt32Value");
        this.Host!.Settings.SetValue("TestInt32Value", -123);
        this.Host!.Settings.SetValue("TestDoubleValue", 111.1);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override async Task StartingAsync(CancellationTokenSource cancel)
    {
        // Generate some dummy settings
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var r = new Random(123456);

            for (var category = 'A'; category <= 'N'; category++)
            {
                var itemCount = r.Next(5, 20);
                for (var i = 0; i < itemCount; i++)
                {
                    var randomType = r.Next(0, 3);
                    if (randomType == 0)
                    {
                        var testSetting = this.AppSettings!.RegisterIntegerSetting(
                            Guid.NewGuid().ToString(),
                            new[] { $"Category {category}" },
                            defaultValue: default);

                        testSetting.Value = r.Next(100);

                        testSetting.ValueChanged += newValue => Debug.WriteLine($"Setting '{testSetting.Key}' value changed to '{newValue}'");
                    }
                    else if (randomType == 1)
                    {
                        var testSetting = this.AppSettings!.RegisterBooleanSetting(
                            Guid.NewGuid().ToString(),
                            new[] { $"Category {category}" },
                            defaultValue: r.Next(0, 2) == 0);

                        testSetting.ValueChanged += newValue => Debug.WriteLine($"Setting '{testSetting.Key}' value changed to '{newValue}'");
                    }
                    else if (randomType == 2)
                    {
                        var testSetting = this.AppSettings!.RegisterStringSetting(
                            Guid.NewGuid().ToString(),
                            new[] { $"Category {category}" });

                        if (r.Next(4) == 2)
                        {
                            testSetting.Value = "asd";
                        }

                        testSetting.ValueChanged += newValue => Debug.WriteLine($"Setting '{testSetting.Key}' value changed to '{newValue}'");
                    }
                }
            }
        });
    }
}