namespace MyApp;

using System;
using System.Collections.Generic;
using System.Windows.Input;

/// <summary>
/// Common application services.
/// </summary>
public interface IHost
{
    /// <summary>
    /// Gets the application path.
    /// It is the location of the running executable.
    /// </summary>
    string AppPath { get; }

    /// <summary>
    /// Gets the application data path.
    /// It is user-specific base folder for different kinds of application configuration and temporary files.
    /// </summary>
    string DataPath { get; }

    /// <summary>
    /// Gets the extensions path.
    /// </summary>
    string ExtensionsPath { get; }

    /// <summary>
    /// Gets the path for downloaded patches and patch notes.
    /// </summary>
    string PatchingPath { get; }

    /// <summary>
    /// Gets the path denoted for temporary files.
    /// </summary>
    string TempPath { get; }

    /// <summary>
    /// Gets the default path for new project files.
    /// </summary>
    string ProjectsPath { get; }

    /// <summary>
    /// Gets the user settings path.
    /// </summary>
    string UserSettingsPath { get; }

    /// <summary>
    /// Gets the UI specific services.
    /// </summary>
    IUIHost UI { get; }

    /// <summary>
    /// Gets the settings services.
    /// </summary>
    ISettingsHost Settings { get; }

    /// <summary>
    /// Gets the diagnostics.
    /// </summary>
    IDiagnosticsHost Diagnostics { get; }

    /// <summary>
    /// Gets the messenger.
    /// </summary>
    IMessenger Messenger { get; }

    /// <summary>
    /// Registers the specified instance into the service container.
    /// Only one instance may be registered to <typeparamref name="TInstance"/>.
    /// </summary>
    /// <typeparam name="TInstance">Type of the object.</typeparam>
    /// <param name="instance">The instance.</param>
    /// <param name="owner">The owner who registers the instance. Only the same owner may unregister the instance.</param>
    /// <returns><c>true</c>if the instance was successfully added, <c>false</c> if the type has already been registered.</returns>
    bool Register<TInstance>(TInstance instance, object? owner = null)
        where TInstance : class;

    /// <summary>
    /// Removes the specified instance from the service container.
    /// </summary>
    /// <typeparam name="TInstance">Type of the object.</typeparam>
    /// <param name="owner">The owner who registered the instance. If not matched, the instance will not be removed, and this method returns <c>false</c>.</param>
    /// <returns><c>true</c>if the instance was successfully removed.</returns>
    bool Unregister<TInstance>(object owner)
        where TInstance : class;

    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <typeparam name="TInstance">Type of the object.</typeparam>
    /// <returns>The instance, or <c>default</c> if not found.</returns>
    /// <remarks>Also contains features who implement an interface.</remarks>
    TInstance? GetInstance<TInstance>()
        where TInstance : class;
}

/// <summary>
/// Common UI services.
/// </summary>
public interface IUIHost
{
    /// <summary>
    /// Gets the language.
    /// </summary>
    /// <remarks>
    /// The format is <c>en-us</c> (for English).
    /// The <c>IShell</c> interface offers API to change the language at runtime.
    /// </remarks>
    string Language { get; }

    /// <summary>
    /// Gets the localized text.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The string, or <c>null</c> if not found.</returns>
    /// <remarks>
    /// All features' resources are examined, but there is no guarantee that the correct string is returned if many features have declared the same key.
    /// Consider using <see cref="GetLocalizedText{TFeature}"/> instead if you know the feature in context.
    /// </remarks>
    string? GetLocalizedText(string key);

    /// <summary>
    /// Gets the localized text specific to a feature.
    /// </summary>
    /// <typeparam name="TFeature">The feature who owns the Resource Dictionary where the string is expected to be declared.</typeparam>
    /// <param name="key">The key.</param>
    /// <returns>The string, or <c>null</c> if not found.</returns>
    /// <remarks>
    /// The feature of the specified type must be active within the application.
    /// </remarks>
    string? GetLocalizedText<TFeature>(string key)
        where TFeature : App.Feature;
}

/// <summary>
/// Common settings services.
/// </summary>
public interface ISettingsHost
{
    /// <summary>
    /// Gets the value by key.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="key">The key. Cannot be empty.</param>
    /// <returns>The deserialized value.</returns>
    TValue GetValue<TValue>(string key);

    /// <summary>
    /// Gets the value by key, or <paramref name="defaultValue"/> if the value has not been stored yet.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="key">The key. Cannot be empty.</param>
    /// <param name="defaultValue">The returned default value if value with the specified key has not yet been stored.</param>
    /// <returns>The deserialized value.</returns>
    TValue GetValue<TValue>(string key, TValue defaultValue);

    /// <summary>
    /// Sets the value by key.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="key">The key. Cannot be empty.</param>
    /// <param name="value">The value.</param>
    void SetValue<TValue>(string key, TValue value);

    /// <summary>
    /// Removes the key and value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="key">The key. Cannot be empty.</param>
    void ClearValue<TValue>(string key);
}

/// <summary>
/// Common diagnostics.
/// </summary>
public interface IDiagnosticsHost
{
    /// <summary>
    /// Gets the list of loaded features.
    /// </summary>
    /// <returns>Collection of feature names.</returns>
    IEnumerable<string> GetFeatures();

    /// <summary>
    /// Gets the list of published views.
    /// </summary>
    /// <returns>Collection of tuples of published view names, and the amount of their active instances.</returns>
    IEnumerable<(string ViewName, int Count)> GetPublishedViews();

    /// <summary>
    /// Gets the localizations that have been declared across all features.
    /// </summary>
    /// <returns>Collection of tuples of the language identifier, and the amount of associated string resources.</returns>
    IEnumerable<(string Language, int StringCount)> GetLocalizations();
}

/// <summary>
/// General-purpose messenger.
/// </summary>
public interface IMessenger
{
    /// <summary>
    /// Registers a recipient for a given type of message.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to receive.</typeparam>
    /// <param name="recipient">The recipient that will receive the messages.</param>
    /// <param name="handler">The message handler to invoke when a message is received.</param>
    void Register<TMessage>(object recipient, Action<object, TMessage> handler)
        where TMessage : class;

    /// <summary>
    /// Registers a recipient for a given type of message.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to receive.</typeparam>
    /// <param name="recipient">The recipient that will receive the messages.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="handler">The message handler to invoke when a message is received.</param>
    void Register<TMessage>(object recipient, int channel, Action<object, TMessage> handler)
        where TMessage : class;

    /// <summary>
    /// Unregisters a recipient from messages of a given type.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to stop receiving.</typeparam>
    /// <param name="recipient">The recipient to unregister.</param>
    void Unregister<TMessage>(object recipient)
        where TMessage : class;

    /// <summary>
    /// Unregisters a recipient from messages of a given type.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to stop receiving.</typeparam>
    /// <param name="recipient">The recipient to unregister.</param>
    /// <param name="channel">The channel.</param>
    void Unregister<TMessage>(object recipient, int channel)
        where TMessage : class;

    /// <summary>
    /// Unregisters a recipient from all registered messages.
    /// </summary>
    /// <param name="recipient">The recipient to unregister.</param>
    void UnregisterAll(object recipient);

    /// <summary>
    /// Sends a message of the specified type to all registered recipients.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to send.</typeparam>
    /// <param name="message">The message to send.</param>
    void Send<TMessage>(TMessage message)
        where TMessage : class;

    /// <summary>
    /// Sends a message of the specified type to all registered recipients.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to send.</typeparam>
    /// <param name="message">The message to send.</param>
    /// <param name="channel">The channel.</param>
    void Send<TMessage>(TMessage message, int channel)
        where TMessage : class;
}