namespace MyApp;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Messaging;

/// <summary>
/// The application.
/// </summary>
/// <seealso cref="Avalonia.Application" />
/// <seealso cref="MyApp.IHost" />
public partial class App : IHost, IUIHost, ISettingsHost, IDiagnosticsHost, IMessenger
{
    /// <summary>
    /// The application name. Modify to suit your own app.
    /// </summary>
    public const string AppName = "My Application";

    /// <summary>
    /// The internal IoC container.
    /// </summary>
    private readonly Dictionary<Type, (object Instance, object? Owner)> serviceContainer = new();

    /// <summary>
    /// The common settings dictionary shared by features.
    /// </summary>
    private readonly Dictionary<string, string> settings = new();

    /// <inheritdoc />
    string IHost.AppPath => Path.GetDirectoryName(typeof(Program).Assembly.Location!)!;

    /// <inheritdoc />
    string IHost.DataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);

    /// <inheritdoc />
    string IHost.ExtensionsPath => Path.Combine(((IHost)this).AppPath, "Extensions");

    /// <inheritdoc />
    string IHost.PatchingPath => Path.Combine(((IHost)this).DataPath, "Patch");

    /// <inheritdoc />
    string IHost.TempPath => Path.Combine(((IHost)this).DataPath, "Temp");

    /// <inheritdoc />
    string IHost.ProjectsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), AppName);

    /// <inheritdoc />
    string IHost.UserSettingsPath => Path.Combine(((IHost)this).DataPath, "Settings");

    /// <inheritdoc />
    public IUIHost UI => this;

    /// <inheritdoc />
    public ISettingsHost Settings => this;

    /// <inheritdoc />
    public IDiagnosticsHost Diagnostics => this;

    /// <inheritdoc />
    public IMessenger Messenger => this;

    /// <inheritdoc />
    bool IHost.Register<TInstance>(TInstance instance, object? owner)
        where TInstance : class
    {
        if (instance is null || this.serviceContainer.ContainsKey(typeof(TInstance)))
        {
            return false;
        }

        this.serviceContainer.Add(typeof(TInstance), (instance, owner));
        return true;
    }

    /// <inheritdoc />
    bool IHost.Unregister<TInstance>(object owner)
        where TInstance : class
    {
        if (!this.serviceContainer.TryGetValue(typeof(TInstance), out var registration))
        {
            return false;
        }

        if (registration.Owner != owner)
        {
            return false;
        }

        return this.serviceContainer.Remove(typeof(TInstance));
    }

    /// <inheritdoc />
    TInstance? IHost.GetInstance<TInstance>()
        where TInstance : class
    {
        if (this.serviceContainer.TryGetValue(typeof(TInstance), out var obj) && obj.Instance is TInstance instance)
        {
            return instance;
        }

        // Check if any feature implements this, then auto-register it
        if (typeof(TInstance).IsInterface)
        {
            var implementors = (from feature in this.features
                                let interfaces = feature.GetType().GetInterfaces()
                                where interfaces.Contains(typeof(TInstance))
                                select feature).ToArray();

            if (implementors.Length > 1)
            {
                Trace.TraceWarning($"Interface '{typeof(TInstance).FullName}' is implemented by more than one feature: {string.Join(", ", $"'{implementors.Select(implementor => implementor.GetType().FullName)}'")}. Only the first one is used as service provider.");
            }

            if (implementors.Any())
            {
                this.serviceContainer.Add(typeof(TInstance), (implementors[0], default));

                object _ = implementors[0];
                return (TInstance)_;
            }
        }

        return default;
    }

    /// <inheritdoc />
    string? IUIHost.GetLocalizedText(string key)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        return this.coalescedStrings.TryGetValue(key, out var value) ? value : default;
    }

    /// <inheritdoc />
    string? IUIHost.GetLocalizedText<TFeature>(string key)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        var feature = this.features.OfType<TFeature>().FirstOrDefault();
        if (feature is null)
        {
            return default;
        }

        if (this.localizedResources.TryGetValue(feature, out var localizations))
        {
            return localizations.TryGetValue(this.language ?? DefaultCulture, out var currentResources) && currentResources.TryGetValue(key, out var localizedValue) && localizedValue is string localizedString && localizedString is not null
                ? localizedString
                : (localizations.TryGetValue(DefaultCulture, out var defaultResources) && defaultResources.TryGetValue(key, out var defaultValue) && defaultValue is string englishString && englishString is not null
                    ? englishString
                    : default);
        }

        return default;
    }

    /// <inheritdoc />
    TValue ISettingsHost.GetValue<TValue>(string key)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        return ((ISettingsHost)this).GetValue<TValue>(key, default!);
    }

    /// <inheritdoc />
    TValue ISettingsHost.GetValue<TValue>(string key, TValue defaultValue)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("The key cannot not be empty.", nameof(key));
        }

        if (!this.settings.TryGetValue(key, out var serialized))
        {
            return defaultValue;
        }

        try
        {
            return (TValue)JsonSerializer.Deserialize(serialized, typeof(TValue))!;
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <inheritdoc />
    void ISettingsHost.SetValue<TValue>(string key, TValue value)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("The key cannot not be empty.", nameof(key));
        }

        this.settings[key] = JsonSerializer.Serialize(value);
    }

    /// <inheritdoc />
    void ISettingsHost.ClearValue<TValue>(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("The key cannot not be empty.", nameof(key));
        }

        this.settings.Remove(key);
    }

    /// <inheritdoc />
    IEnumerable<string> IDiagnosticsHost.GetFeatures()
    {
        return this.Features.Select(f => f.GetType().FullName!);
    }

    /// <inheritdoc />
    IEnumerable<(string ViewName, int Count)> IDiagnosticsHost.GetPublishedViews()
    {
        return this.activeViews.Values.GroupBy(v => v.ViewName).Select(v => (v.Key, v.Count()));
    }

    /// <inheritdoc />
    IEnumerable<(string Language, int StringCount)> IDiagnosticsHost.GetLocalizations()
    {
        var localizations = this.localizedResources.Values
            .SelectMany(loc => loc)
            .GroupBy(loc => loc.Key)
            .Select(g => new { Language = g.Key, Resources = g.Select(r => r.Value).ToArray() })
            .ToArray();

        /* TODO:
         * When https://github.com/AvaloniaUI/Avalonia/issues/8790 is fixed, check resource entry's value's type (should be string) and only count those.
         * Currently untouched dictionaries (for example languages that have not been triggered yet) report the
         * value as an object of type Avalonia.Controls.ResourceDictionary+DeferredItem instead of the realized type.
         */
        var result = localizations
            .Select(loc => (loc.Language, loc.Resources.Sum(r => r.Count)))
            .ToArray();

        return result;
    }

    /// <inheritdoc />
    void IMessenger.Register<TMessage>(object recipient, Action<object, TMessage> handler)
    {
        ArgumentNullException.ThrowIfNull(recipient, nameof(recipient));
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        WeakReferenceMessenger.Default.Register<TMessage>(recipient, (r, m) => handler(r, m));
    }

    /// <inheritdoc />
    void IMessenger.Register<TMessage>(object recipient, int channel, Action<object, TMessage> handler)
    {
        ArgumentNullException.ThrowIfNull(recipient, nameof(recipient));
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        WeakReferenceMessenger.Default.Register<TMessage, int>(recipient, channel, (r, m) => handler(r, m));
    }

    /// <inheritdoc />
    void IMessenger.Unregister<TMessage>(object recipient)
    {
        ArgumentNullException.ThrowIfNull(recipient, nameof(recipient));

        WeakReferenceMessenger.Default.Unregister<TMessage>(recipient);
    }

    /// <inheritdoc />
    void IMessenger.Unregister<TMessage>(object recipient, int channel)
    {
        ArgumentNullException.ThrowIfNull(recipient, nameof(recipient));

        WeakReferenceMessenger.Default.Unregister<TMessage, int>(recipient, channel);
    }

    /// <inheritdoc />
    void IMessenger.UnregisterAll(object recipient)
    {
        ArgumentNullException.ThrowIfNull(recipient, nameof(recipient));

        WeakReferenceMessenger.Default.UnregisterAll(recipient);
    }

    /// <inheritdoc />
    void IMessenger.Send<TMessage>(TMessage message)
    {
        WeakReferenceMessenger.Default.Send(message);
    }

    /// <inheritdoc />
    void IMessenger.Send<TMessage>(TMessage message, int channel)
    {
        WeakReferenceMessenger.Default.Send(message, channel);
    }
}