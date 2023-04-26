# User Guide

This sample application was created for educational purposes as well as to serve as a quick base app you can quickly wire up and start building actual business logic on. This documentation explains the building blocks present in this solution.

You should also explore [Avalonia documentation](https://docs.avaloniaui.net/docs) and [MVVM Toolkit documentation](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm). You may also use [ReactiveUI](https://www.reactiveui.net) as a replacement for the MVVM Toolkit.

ℹ️ ***There is a short summary at the end of the guide.***

## MVVM

The solution follows the _Model-View-ViewModel_ (MVVM) design philosophy i.e. _Views_ are defined declaratively in _AXAML_ markup. They reflect data within the _View Model_ (a C# class) through _data-binding_. Often View Models present the state of a _Model_ object instance, but  tailor its data so that it is suitable for the View to consume.

There are a couple of ways how different MVVM frameworks typically bundle Views and View Models within the project structure. You may have seen a convention where all views are located in the "Views" folder, all view models in the "ViewModels" folder, and Models in their own folder. Due to the feature driven design in the sample application, it makes more sense to store these components into their Feature folder instead. For example:

``` 
📁Features/
  📁MyFeature/
    📄MyFeature.cs
    📄MyView.axaml
    📄MyView.axaml.cs
    📄MyViewModel.cs
    📄MyModel.cs
```

> 💡 **TIP:**  
> Of course nothing stops you from creating the "Views", "ViewModels", and "Models" sub-folders in your Feature folder if you prefer to arrange the files that way. This however will require you to use more `using` statements across your files.

As a rule of thumb:
- Derive your **view models** from the `ViewModel` base class
- Derive your **views** from Avalonia's `UserControl`

> 💡 **TIP:**  
> Make sure you have the [Avalonia Visual Studio extension](https://marketplace.visualstudio.com/items?itemName=AvaloniaTeam.AvaloniaVS) installed. It contains the templates for Avalonia-specific project items as well as the AXAML visual designer.

There is also a handy utility `ViewLocator` that implicitly attempts to match Views based on a view model DataContext [using a naming convention]. It is instantiated in **App.axaml** and thus applies to the entire application.

## Features

Features are autonomous modules that are meant to implement a single aspect of the application. A feature groups all views, view models, models, interfaces, and resources together to make up a logical feature. Features can contain visual UI (in the form of _Published Views_), or just be services that provide access to data that some other features may then utilize.

Examples of visual features include ToolBar, Menu, Navigation, or open file view. Examples of data-only features include ConfigurationProvider, CommandSink, or AuthenticationService.

By convention features are placed in the "Features" folder, in their respective sub-folders. Each feature folder should then contain at least the _Feature_ class that derives from `App.Feature` and its name ending with "Feature.cs", for example "MyCoolFeature.cs".

At start-up the application will enumerate all classes that derive from `App.Feature` and instantiates them. The Feature class should contain a parameterless constructor. This behavior is defined in the `App.LoadFeatures` method, and could be extended to also load features from external assemblies to support _plugins_.

### Feature life-cycle

Here is an example of a simple feature:

```csharp
public class MyCoolFeature : App.Feature
{
    public override Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public override Task StartingAsync(CancellationTokenSource cancel)
    {
        return Task.CompletedTask;
    }

    public override Task RunningAsync(string[] args)
    {
        return Task.CompletedTask;
    }

    public override Task ExitRequestedAsync(CancellationTokenSource cancel)
    {
        return Task.CompletedTask;
    }

    public override Task ShutdownAsync()
    {
        return Task.CompletedTask;
    }
}
```

The overrides shown in the above code snippet are optional, but it is important to understand when these are called.

When the application starts, the following sequence is performed:

1. The _Feature_ classes are discovered and instantiated. You may implement a parameterless constructor to do basic setup for the class, but you **may not** utilize the **Common Application Services** (`IHost`) yet, and you **may not** depend on any other feature at this point either.
2. _Published Views_ are discovered and added to an internal registry.
3. _Resources_ are discovered.
4. Features' _dependencies_ are enumerated and resolved into all feature instances.
5. At this point all required information is gathered, and features will begin their life-time cycle. All features' `InitializeAsync` methods are called in parallel. This is the earliest point you **may** utilize **Common Application Services** (`IHost`). Also all properties decorated with the `DependencyAttribute` have been populated, so you **may** also start utilizing other features' services (although it is generally advisable to do this in the next step instead).
6. All features' `StartingAsync` methods are called in sequence. A `CancellationTokenSource` is provided. Should any feature decide to cancel it, then the entire application will shut down. The purpose here is mainly to recognize some kind of incompatibility or misconfiguration where the feature would otherwise end up in an inoperable state.
7. The **Main Window** will show.
8. At this point all features are considered fully initialized, configured, and ready to go. All features' `RunningAsync` methods are called in parallel. This method also provides the command line argument array, so the chosen feature may for example, open the supplied file, or otherwise perform the instructed action.

The application is set to exit when the Main Window is fully closed. Closing the window either manually by user, or programmatically from code (`App.Current.Exit`) will _attempt_ to close the window, but this can be canceled.

Similarly to the start-up sequence, here is how the shut down goes:

1. All features' `ExitRequestedAsync` methods are called in parallel. Like with `StartingAsync` also this method is provided with a `CancellationTokenSource`. Should any feature choose to cancel it, the Main Window will remain open and the application continues to run normally. If no feature reacts to the event, the shutdown begins. A typical action to perform here is to check whether the user has any unsaved data and prompt saving if necessary.
2. All features' `ShutdownAsync` methods are called in parallel. This is where all features should stop any background services, clean-up their data and save state if needed. There is no time limit to complete this task, but generally speaking features should take no longer than a few seconds to finish up.

> 💡 **TIP:**  
> If you need to forcefully close the application (without invoking the shutdown mechanism), you can call the `Environment.Exit` method.

⚡ The example application demonstrates the "Unsaved changes" functionality when the user has made changes to a project's TextBox content (and not having clicked the 'Save' button) before attempting to exit the app.

#### Recommended actions during the Feature life-cycle methods

**In the class constructor you should:**
- The `Host` is not populated yet, so you should only perform simple setup work such as instantiating _lists_ or other _collections_ used by the class.
- Note that localized strings are not available yet (and they are accessed via the `Host` property anyway).
- **Do not** attempt to bypass the absence of the Host instance by accessing it via `App.Current.Host` - it is a bad practice and may result in undefined behavior.

**In `InitializeAsync` you should:**
- Check that all the required _Dependencies_ have been populated.
- Load necessary data from disk.
- Since the `Host` is available, you may read the _Settings_ and initialize feature state based on them.
- Use `Host.Register` (you can also register them in `StartingAsync`).
- Perform any initialization that is specific to the feature in question only.
- You **should not** utilize other features' services yet because `InitializeAsync` runs in parallel and they might not be ready for consumption yet.
- Register Messenger message handlers. However, you **should not** send any messages yet.
- Generally all services offered by the `Host` are ready.
- **For `AppSettings` feature only**: You may register the default setting categories so that they exist by the time the features start placing their own settings in them.
- The UI is not visible yet, so it is safe to manipulate Language and Theme specific properties in the App class "in advance". The **Shell** feature, for example, sets them according to saved settings values.

**In `StartingAsync` you should:**
- Use `Host.Register` (you can also register them in `InitializeAsync`).
- Start any background services, but assume that the application _may not run_, and that `RunningAsync` may not get called next, and `ShutdownAsync` may get called instead.
- All features have been _initialized_, so now it is possible to start communicating between them. This includes utilizing the _dependencies_, sending _Messenger_ messages, or registering to other features' lists (for example place new settings in a settings category).
- This is the only phase that is called in sequence (instead of in parallel) because the registrations mentioned in above bullet point would otherwise occur simultaneously, or at least in random order. Imagine if two features would attempt adding a custom ToolBar button in the same slot at the same time, or during an active enumeration.
- You have the option to trigger the supplied Cancellation Token. Doing so will stop the application start-up sequence, and cause the application to exit (`ShutdownAsync` will be called next). In this rare scenario the UI is never shown, and `RunningAsync` never gets called either.
    - One example of such cancellation would be a failed "license check".
    - Another example would be a serious malfunction or misconfiguration (without which the feature or application cannot run properly). If the feature for example is providing essential interfaces that you know other features rely upon, the absence of such service could be important enough to prevent the whole app from starting.

**In `RunningAsync` you should:**
- The UI is now visible and all features are up and running. You can utilize the supplied `args` array that contains the command line arguments to determine what to do next. For example, to activate a certain view in your application, open the requested file, and things like that...
- This is the earliest point may start using general Avalonia functionality (such as accessing `App.Current`, operating with _Resource Dictionaries_, or displaying message boxes etc.)

**In `ExitRequestedAsync` you should:**
- Application is about to close. You can prevent it by triggering the supplied Cancellation Token.
- You **should not** stop background services, or otherwise prepare to shutdown. Assume that the application may not close. Truly wind down only in the next step (`ShutdownAsync`).
- You **may** check whether there are unsaved changes, and optionally show a confirmation dialog to the user. Cancel the operation if the user clicked "Cancel". The sample app demonstrates this, and showing the MessageBox is awaitable.

**In `ShutdownAsync` you should:**
- Application is now shutting down (with no option to cancel anymore).
- **Do not** do any UI specific work anymore (such as showing dialogs).
- Gracefully shutdown any background services.
- Make sure that all settings you want to save are stored in `Host.Settings`.
- Save custom data to disk.
- **Avoid** performing any long running operations, the application should exit within a few seconds.

## Published Views

_Published Views_ can be seen as Avalonia _User Controls_ that are instantiated by name (and an optional parameter). In MVVM you typically define the view and its view model in pairs, and this is also true for Published Views. Published Views are defined by special methods in Feature classes. Here is an example:

```csharp
public class MyCoolFeature : App.Feature
{
    [PublishedView<MainView>("MyCoolFeature.MainView")]
    public ViewModel? CreateMainView(object? parameter)
    {
        return new MainViewModel();
    }

    // ...
}
```

Define a method that takes one parameter (of type `object`) and returns an instance of `ViewModel`. Decorate the method with the generic `PublishedViewAttribute` whose type parameter should refer to an Avalonia `UserControl` derived class. The sample application uses a naming convention `Create<ViewName>`.

Also create the view model class:

```csharp
public class MainViewModel : ViewModel
{
}
```

Then add the Avalonia view: **Add** → **New Item...** → **User Control (Avalonia)**, using the name "MainView.axaml".

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="MyApp.Features.MyCoolFeature.MainView">
    <Grid>
        <TextBlock>This is my cool main view!</TextBlock>
    </Grid>
</UserControl>
```

This is your published view! You can now spawn it in AXAML layouts (for example the MainWindow or any other AXAML view) via the `PublishedView` control:

```xml
<!-- xmlns:ui="clr-namespace:MyApp.UI" -->

<ui:PublishedView ViewName="MyCoolFeature.MainView"></ui:PublishedView>
```

Since you refer to the Published View simply by string name, you can utilize other features' views without having strong references to said features. The view may be implemented in some external assembly, too.

### View customization

Remember the `object` parameter in the view creator method's signature? You may also pass a value to the method using the `ViewParameter` attribute in AXAML:

```xml
<ui:PublishedView ViewName="MyCoolFeature.MainView" ViewParameter="Hello"></ui:PublishedView>
```

Both attributes can also be databinded:
```xml
<ui:PublishedView ViewName="{Binding Name}" ViewParameter="{Binding MyParam}"></ui:PublishedView>
```

> 📜 **NOTE:**  
> When either `ViewName` or `ViewParameter` value changes, the view will be re-created. This is good to keep in mind if the creation is an expensive operation. If this is not desired, pass an object that itself does not change, but it holds some other property whose value can change. Pass this value to your view model instance and handle the value change there.
> 
> Also, if you return **null** from the creation method, the view will **not** be created!

### View and view model configuration

The framework will instantiate the view and enrich its _Resources_ with _localized strings_ (if the owning Feature specified any).

Then, the view model instance will have its _dependencies_ resolved and its `Host` property set (this also causes the virtual `Initialize` method to be called). Finally, the view model is set as the view's **DataContext** and the newly created control is attached in the _Visual Tree_.

### View model base functionality

Similarly to Feature classes, the base `ViewModel` class also provides access to common application services via the `Host` property. Whereas this instance becomes available in `Feature.InitializeAsync`method, for view models it is available when the overridable `Initialize` method is called.

> 💡 **TIP:**  
> If you want to _test_ your view models, you can implement the `IHost` interface to create a custom testing Host. Setting the `Host` property from outside simulates the activation of the view model.

Like with Features, you should only perform basic instantiation and setup in the view model's constructor, and start using the Host services only inside `Initialize`.

Please notice that this mechanism only applies to view models that are created as part of the Published View creation process. You can of course also manually set the `Host` property of sub view models, like an `ObservableCollection`'s item view models.

In addition to `Initialize` the base `ViewModel` contains two additional overridable methods: `Activate` and `Deactivate`. They are automatically called when the Published View is attached or detached from the _Logical Tree_ i.e. used within an AXAML layout. You can use these for event subscription/unsubscription (including Messenger subscriptions) or winding up and down background tasks.

## Common Application services (`IHost`)

The sample application contains a number of common services and utilities that will come in handy across the board. In order to keep parts of the application testable (should you choose to do so), access to such resources via static members or a singleton is not ideal. Instead, the framework will auto-populate the instance to the **Host** services for all Feature classes and the view models of Published Views. The Host instance is a single access point to common folders, settings, service container, localization, diagnostics and Messenger.

**You can access the Host in three ways:**

- Feature classes have the `Host` property. The framework will set its value before the `InitializeAsync` method is called.
- View model classes have the `Host` property. The framework will set its value as part of Published View creation before the view model's `Initialize` method is called. If your view model is not for a Published View, you may manually set the `Host` property after object construction.
- All other UI parts, such as _Value Converters_ or views' _code-behinds_ may use the static `App.Current` instance, and its `Host` property.

### Common folders

The **Host** provides the following folder paths:

| Property | Purpose |
| -------- | ------- |
| `AppPath` | Location of the running executable. |
| `DataPath` | Location for user-specific miscellaneous data. |
| `ExtensionsPath` | Base path for external Features. |
| `PatchingPath` | Location for temporary update files and patch notes. |
| `TempPath` | Location for temporary files. |
| `ProjectsPath` | Base path for project data. |
| `UserSettingsPath` | Location for settings files. |

### UI

The **Host.UI** provides localization related functionality:

- The `Language` property gets the current language.
- The `GetLocalizedText(string)` method gets a localized string using the current language, or falls back to the default language.
- The `GetLocalizedText<TFeature>(string)` method gets a localized string using the current language, or falls back to the default language. The scope of available strings is limited to the specified Feature.

These utilities are suitable to be used from C#. In contrast, there are better ways to access localized strings from AXAML. For more information, see the **Localization** section later in this guide.

### Settings

The **Host.Settings** provides access to the settings dictionary. Settings are just key-value pairs.

- Use `GetValue<TValue>(string)` and `GetValue<TValue>(string, TValue)` to read a value from settings. The `TValue` should be of elementary data type such as `int`, `double`, or `string`. Any other type will internally use JSON serialization.
- Use `SetValue<TValue>(string, TValue)` to store a value. Same `TValue` rules apply as for reading.
- Use `ClearValue<TValue>(string)` to completely remove the value from the dictionary. After that, it will not be serialized when the application saves settings to disk.

To learn more about how to use _settings_, see the **Settings** section later in this guide.

### Diagnostics

The **Host.Diagnostics** provides statistics regarding Features, Published Views, and Resources.

- The `GetFeatures` method lists all loaded features by their fully qualified name. You may for example check whether a certain feature is present and adjust another feature's functionality based on that.
- The `GetPublishedViews` method lists all registered Published View names, and their current instance count.
- The `GetLocalizations` method lists all languages and their localized resource count. You may use this information to determine which languages are "complete enough" to be listed in a language selection.

### Messenger

The **Host.Messenger** provides access to the default _Weak-Event_ Messenger. Internally the framework utilizes the Messenger from MVVM Toolkit, and features may use that functionality also. However, the intent was to provide an alternative way to send and receive Messages without a dependency to the MVVM Toolkit (this is more important if Features are loaded from external assemblies).

The Messenger is an alternative way to the _dependency_ system, to send and receive signals. It is suitable for broadcasting messages to objects that do not have access to the Host. Message subscriptions also use _weak references_ (in contrast to events provided by a dependency's interface).

- Use the `Register` method (and its overloads) to introduce a target object that will receive certain type messages.
- Use the `Unregister` method (and its overloads) or `UnregisterAll` method to unsubscribe from messages of certain type.
- Use the `Send` method (and its overloads) to send a new message of certain type to all registered recipients.

Note that you cannot know whether the sent message found any recipients.

To learn more about how to use the Messenger, see the **Messaging** section later in this guide.

### Service container

The **Host** provides a service container that maps an interface type to its actual implementation (practically a singleton instance).

> 📜 **NOTE:**  
> Avalonia has its own ServiceLocator, but it seems to be for Avalonia-specific services only. For **Host** the intent was to provide a container dedicated for the application only. For example, interfaces published by Features are automatically available in this container. Nothing stops you from using the ServiceLocator though.

- Use the `Register<TInstance>(TInstance, object?)` method to register an object instance and have it be available by an interface type (`TInstance` should be an interface type). You can only register the same interface once. You can use the return value to determine whether the object was registered successfully. Usually the ideal time for registering instances is at the Features' `InitializeAsync` and `StartingAsync` methods.
- Use the `Unregister<TInstance>(object)` method to remove an instance from the container. Only the instance's owner (whoever registered it in the first place) should do this.
- Use the `GetInstance<TInstance>` method to request the object instance.

⚡ The example application demonstrates the Register/GetInstance functionality by registering the `ShellView` as a globally accessible `IViewContainer`.

## Localization

The example application has _Language selection_ on the _Home screen_. It is wired up to change the `App.Language` property, and when it does, all active Published Views will have their _Resources_ updated with language-specific strings. In other words, localization is tightly tied to Features and their Published Views.

In order to define localizable strings, you need to create a _Resource Dictionary_, and then introduce it in your Feature class. More specifically, decorate the class with the `LocalizedResources` and specify the language identifier and the (relative) location of the corresponding Resource Dictionary:

```csharp
[LocalizedResources(Culture = "en-us", Path = "Locales/en-us.axaml")]
public class MyCoolFeature : App.Feature
{
    // ...
}
```

The recommended way is to create a new sub-folder named "Locales" in your Feature folder, and then **Add** → **New Item...** → **Resource Dictionary (Avalonia)**, using the name like "en-us.axaml".

Here is an example how the Resource Dictionary might look like:

```xml
<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <x:String x:Key="MyCoolFeature.Yes">Yes</x:String>
    <x:String x:Key="MyCoolFeature.No">No</x:String>
    <x:String x:Key="MyCoolFeature.Cancel">Cancel</x:String>

</ResourceDictionary>
```

You can define as many `LocalizedResources` attributes (and Resource Dictionaries) as you like, however you should have only one dictionary per language.

> ⚠️ **IMPORTANT:**  
> When you localize a feature, it must have at least **en-us.axaml** defined. This is the default language, and its strings will be used as fall-back if a string is missing from the current language. The **en-us** version should always be the most complete set of strings.
 
When accessing localized strings, they resolve based on standard Avalonia layout hierarchy. For example in the demo application, since the **Shell** serves as a container for all other Published Views, its localized strings contain general-purpose terms like "Yes" and "No". This means that other features will not have to declare such strings the second time, and a `DynamicResource` reference will find the value from the `ShellView`'s resources (unless another View deeper in the Visual Tree overrides this resource key).

Overriding known existing resources as well as inheriting resources from closer to the Visual Tree root are both powerful mechanisms you may leverage.

See the [Avalonia Resource resolution](https://docs.avaloniaui.net/docs/styling/resources#resource-resolution) for more info about the discovery order.

### Usage via `DynamicResource`

```xml
<TextBlock Text="{DynamicResource MyCoolFeature.Yes}" />
```

### Usage via Value converter

```xml
<!-- xmlns:ui="clr-namespace:MyApp.UI" -->

<TextBlock Text="{Binding Key, Converter={x:Static ui:ConvertKeyToLocalizedString.Instance}}" />
```

Please note that in the above code snippet, the ValueConverter will not automatically react to `App.Language` change. You will need to manually notify that the binding has changed:

```csharp
public class MyViewModel : ViewModel
{
    [Dependency]
    public IShell? Shell { get; set; }

    public string Key => "MyCoolFeature.Yes";

    protected override void Initialize()
    {
        if (this.Shell is not null)
        {
            this.Shell.LanguageChanged += (sender, args) => this.OnPropertyChanged(nameof(this.Key));
        }
    }
}
```

### Usage via Markup extension

```xml
<!-- xmlns:ui="clr-namespace:MyApp.UI" -->

<TextBlock Text="{ui:Localize {Binding Key}}" />
```

Please note that in the above code snippet, the markup extension will not automatically react to `App.Language` change. You will need to manually notify that the binding has changed. See the **Value converter** example above.

### Usage via `IHost`

```csharp
// Limit the scope to MyCoolFeature's resources only
var localized1 = this.Host!.UI.GetLocalizedText<MyCoolFeature>("MyCoolFeature.Yes");

// Look up from the "flattened" list that contains all currently active strings
var localized2 = this.Host!.UI.GetLocalizedText("Yes");
```

The `GetLocalizedText` method returns `null` if the resource was not found. Consider using a fall-back string like this:

```csharp
var text = this.Host!.UI.GetLocalizedText("S0METH1ng") ?? "Some default";
```

---

> 💡 **SUMMARY:**  
> - Within AXAML, use `DynamicResource` whenever possible
> - Within C#, use the `IHost.UI.GetLocalizedText`

### Examples of usage

⚡ In the demo application, there is a Language selection on the **Home screen** where you can check how Language can be changed at runtime. This application is partially localized to Finnish. Observe how some strings do not change when switching the language. Those strings are missing from the Finnish Resource Dictionaries, and will fall-back to defaults (English).

⚡ In the `ShellFeature.InitializeAsync` method the "APP.Language" setting is loaded at application start-up, and stored every time the `ShellFeature.Language` property changes. The next time the app starts, the last used Language is thus remembered.

⚡ The `AppSettingsViewModel` subscribes to `IShell.LanguageChanged` event, and refreshes all properties in setting view models that are used as localization keys. This will ensure that the views' `Localize` markup extensions are updated. The reason for this is that Settings are created programmatically, and thus the AXAML layout is templated. The only way to localize them is to either have the view model serve an already localized string, or use value conversion from AXAML. The latter is more exciting so that is the implementation for now. Either way, the view model must react to the Language change event.

## Themes

The example application has the ability to switch the theme between Avalonia's Fluent **light** and **dark** variants freely at runtime. Like _Language_, the current Theme is considered an application-wide setting and thus it is controlled by the **Shell** feature. The theme related functionality is exposed in the `IShell` interface that can easily be imported as a _dependency_ to other features and view models. More specifically, please check the following `IShell` members:

- Use the `Theme` property to get or set the current theme. The choice is either  Light or Dark.
- Use the `AccentColor` property to get or set the accent color used in control styles.
- Subscribe to the `ThemeChanged` and/or `AccentColorChanged` events to get notified when their respective properties change.

Internally the **Shell** feature manipulates the `App.RequestedThemeVariant` and `App.ActualThemeVariant` properties (who do all the heavy lifting to change  control styles at runtime).

⚡ The **Home screen** provides selectors for the Light/Dark switch and a set of example accent colors. Basically these controls use the `IShell` interface for applying the custom appearance. These values are also written to `IHost.Settings`, and are remembered the next time the app starts.

⚡ The demo application uses a lot of partially transparent grayish colors to apply shading on various parts of the UI. This way shading works on both dark and light backgrounds. See **ProjectView.axaml** for example (`#33AAAAAA` in particular).

⚡ Sometimes the above mentioned shading trick is just not enough to make some UI elements work on both dark and light backgrounds. The **ViewSelectorFeature.cs** demonstrates how to change some Resources (such as `SolidColorBrush` used by the View) based on the theme. Basically, it updates the **MainWindow's** Resource Dictionary with theme specific brushes (to which DynamicResources then react).

## Dependency system

The general application architecture is heavily designed around _Features_ where each of them provides a coherent unit of functionality. Features may build on each other, where elementary features form the building blocks of more sophisticated features. This will require a way of features to communicate with each other. You could use the _Messenger_, but creating dozens of requests will not fly very far.

Enter dependencies. It allows features to **publish interfaces**, and also import said interfaces directly to other Feature classes or view models through _property injection_.

Add this interface:

```csharp
public interface IMyService
{
    Task<string> GetSecretAsync();
}
```

We want **MyCoolFeature** to implement this interface and publish it:

```csharp
public class MyCoolFeature : App.Feature, IMyService
{
    public async Task<string> GetSecretAsync()
    {
        return await Task.FromResult("My secret value.");
    }

    // ...
}
```

When another feature (or view model) wants to consume the interface, add a property (of the interface type) and decorate it with the `DependencyAttribute`:

```csharp
public class SomeOtherFeature : App.Feature
{
    [Dependency]
    public IMyService? MyService { get; set; }

    // Use the dependency
    private async Task DoStuffAsync()
    {
        var secret = await this.MyService!.GetSecretAsync();
    }

    // Ensure that the required dependencies are properly filled in
    public override Task StartingAsync(CancellationTokenSource cancel)
    {
        if (this.MyService is null)
        {
            Trace.TraceError("A critical dependency is missing. Forcing application shutdown.");
            cancel.Cancel();
        }

        return Task.CompletedTask;
    }
}
```

Dependencies are populated for **Feature classes** and those **view models** that are part of Published View creation.

> 💡 **TIP:**  
> You may check whether the dependencies are fulfilled in the Feature's `InitializeAsync`, or optionally in `StartingAsync` if you want to prevent the app from starting if a dependency is missing. For view models the corresponding check would be the `Initialize` method.

### Pattern: Implement a public API for a feature

Most features in the example application provide an interface that can be used to control how the feature's View(s) behave. These interfaces follow a naming convention where the feature class is named `ThingFeature` and its interface `IThing`. The interface may provide methods, properties, and events for synchronous and asynchronous use. The interface exists in the same namespace as the Feature.

If your Feature provides a "main view" you might want to pass the interface also to the view model because that is where interaction logic resides. Consider this example from the **StatusBar** feature:

```csharp
public class StatusBarFeature : App.Feature, IStatusBar
{
    [PublishedView<StatusBarView>("IDE.StatusBar", Description = "The status bar.")]
    public ViewModel? CreateStatusBar(object? parameter)
    {
        return new StatusBarViewModel(this); // Pass in IStatusBar
    }
}
```

Another scenario is that since the Published View creation method receives a _parameter_, you can databind it from AXAML, and pass that into the view model constructor:

```xml
<ui:PublishedView ViewName="MyCoolFeature.MainView" ViewParameter="{Binding SomeInterfaceComingFromOutside}" />
```

```csharp
[PublishedView<MainView>("MyCoolFeature.MainView")]
public ViewModel? CreateMain(object? parameter)
{
    if (parameter is ISomeExpectedInterface obj)
    {
        return new MainViewModel(obj);
    }

    return null; // Cannot create view
}
```


## Messaging

The Messenger can be accessed via `IHost` (`Feature.Host`, `ViewModel.Host`, `App.Current.Host`) that contains a few methods for sending signals, and a way to register handlers to react to them.

_Messages_ are just class instances and may thus contain as much information as necessary. However, message classes should be _immutable_. If you are only interested in declaring member properties and do not care about validation, this can be achieved by using C# **records**.

First create a message class:

```csharp
public record TestMessage
{
    public required string What { get; init; }
}
```

Use the **Host** to register a handler for that type of message:

```csharp
this.Host!.Messenger.Register<TestMessage>(this, (r, m) =>
{
    // Handle message
    Debug.WriteLine($"Recipient:{r}, Message:{m.What}");
});
```

A good place to register the message handler, is a Feature's `InitializeAsync` method, or a view model's `Initialize` method.

For view models, another pattern is to register to messages in the `Activate` method, and unregister those handlers in the corresponding `Deactivate` method. Please note that these methods are called by the framework only for those view models that were part of Published View construction:

```csharp
public override void Deactivate()
{
    this.Host!.Unregister<TestMessage>(this);
}
```

> 📜 **NOTE:**  
> Even though Messenger recipients are registered using weak references (which does not prevent garbage collection) it is a good practise to always unregister from message types you have previously registered.

> 💡 **TIP:**  
> If your recipient registers to multiple message types, you can use the `UnregisterAll` method to conveniently clear all message handlers at once (instead of calling `Unregister` for each one separately).

Please note that the `Unregister` method requires the **original recipient** as an argument. This is to make sure that the calling code at least knows who registered the message in the first place. Typically both register and unregister operations use `this` as the recipient.

Finally, use Messenger to send a message to all registered recipients:

```csharp
this.Host!.Messenger.Send(new TestMessage { What = "Yeah!" });
```

Note that you cannot know whether the message reached any recipient.

### Channels

There are overloads for `Register`, `Unregister` and `Send` to also accept an integer `channel` parameter. This allows you to use the same message class, but also to filter (by channel) which recipients should receive the message.

```csharp
// Send the message only to recipients who are listening to channel 2
this.Host!.Messenger.Send(new TestMessage { What = "Bla bla" }, 2);

// Send the message only to recipients who are listening to channel 5
this.Host!.Messenger.Send(new TestMessage { What = "Hihi" }, 5);
```

### More advanced functionality

Internally the framework uses the MVVM Toolkit (Weak-Event) Messenger. The Host provides an abstraction because Features could be loaded from external assemblies (like "plug-ins"), and utilizing the Messenger from these features would require a reference to the Toolkit. By providing the base functionality that does not depend on Toolkit Messenger types the choice of which MVVM framework to use is still at the hands of the plug-in developer.

There is one notable feature in the MVVM Toolkit's Messenger that is absent from the IHost version and that is _Request Messages_. It is missing because request messages must derive from a base class which the framework cannot expose in the Messaging API for reasons mentioned above. If your code uses the Toolkit, you may however, still utilize this feature. 

> 💡 **TIP:**  
> The work-around to Request Messages is to use _Feature Dependencies_, where a Feature implements an interface and exposes that as a dependency. Such an interface could provide synchronous or asynchronous methods that serve a similar purpose to sending request messages. For more information, see the **Dependency system** section earlier in this guide.

For more information about the MVVM Toolkit Messenger capabilities, check out its [documentation](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/messenger).

⚡ There is one example in the demo application where the **Shell.axaml.cs** publishes a `MainViewReadyMessage` when the view has been loaded.

## Icons

Apart from the program icon all graphics in the example application are vector based SVG shapes. Of course nothing stops you from using traditional raster images, but the SVG approach has some strong up-sides in a modern UI. Most importantly they are scalable to any size without getting blurry like bitmaps do. This trait fits very well to Avalonia layouts because they are DPI-aware; the SVG icons will always look crisp regardless of screen resolution or OS UI scaling.

The second trait is that since SVG graphics are built using shapes, individual elements [that make up the icon] can be colorized, and these colors can be dynamically changed at runtime. This enables all kinds of interesting scenarios including:

- Icon colors can dynamically be changed based on the selected theme
- Icon colors can dynamically be changed based on mouseover state or some view model property
- Icon variants can be created where the geometry of the icon is the same but the fill color varies - in other words less assets for more actions
- Create more engaging icons by animating their colors

> 📜 **NOTE:**  
> Icons in the demo application use single fill color, but the system could be easily extended to support multi-color icons.

### Where to get them

The sample icons are from the [Material icons gallery](https://fonts.google.com/icons).

Here is how you would import a new icon to the application:

1. On the website, make sure you select **"Material Icons"** (not "Material Symbols")
2. Click on the desired icon. A sidebar will appear.
3. Make sure that **Size** is "24dp" and **Color** is "Black".
4. Click on the "**SVG**" button at the bottom. This will download the SVG file.
5. Copy the downloaded SVG file to the project folder `/Assets/Icons` and give it a name with no special characters (follow the existing naming convention).
6. Make sure that the item's **Build Action** is "AvaloniaResource".

### Usage

Use the `Icon` control. It has databindable `IconName` and `Color` properties. The system assumes that all Icons are located in the aforementioned "Icon" project folder, and you refer to them simply by filename (excluding the file extension). For example, if your icon is named "**MyIcon.svg**", simply assign `IconName = "MyIcon"`.

```xml
<!-- xmlns:ui="clr-namespace:MyApp.UI" -->

<!-- Example: Use custom color -->
<ui:Icon IconName="Project" Color="#FFFFAA00" Width="24" Height="24" />

<!-- Example: Databind the name -->
<ui:Icon IconName="{Binding Icon}" Width="16" Height="16" />
```

⚡ Icons are used in **HomeScreenView.axaml** and **ViewSelectorView.axaml** where the latter demonstrates databinding and custom coloring.

SVG rendering is done with [Svg.Skia](https://github.com/wieslawsoltes/Svg.Skia). If you need other ways to display SVG content, for example using the `Svg` control, please refer to their code examples. The `Icon` control is just an easy-to-use databindable and colorizable helper control.

## Sample app UI features explained

The example project is quite minimal, but features the most prominent UI parts commonly found in desktop software: navigation, workspace, menu and status bar. Main Window is the root visual container, and hosts one Published View, the **Shell**. The Shell wires up the rest of the UI by specifying the general layout, and spawning other Published Views to fill those regions. Due to the "starting point" nature of the Shell it is also the most natural place to handle application-wide visual configuration such as theming and localization.

Home Screen, Menu and StatusBar are self-explanatory, so this section will focus on two topics, the _Main Navigation_ and the idea behind the _Settings_ view.

### Project handling

Applications often maintain a list of "Most Recently Used" files. Usually this list is accessible either by menu or some sort of _start view_. However in the demo application these project items have been combined into the _main navigation bar_ on the left. The idea is simple: Since the nav bar already presents buttons for activating views - why not make _recent projects_ just as easily accessible.

The navigation bar (ViewSelectorFeature) consists of three parts: At the top there is a static section of views - the **Home** button in this case. In the middle there is the project item list whose content is dynamic. And finally at the bottom there is another static section of views - the **Settings** button in this case.

The middle part occupies the remaining space, listing project items vertically. If the list takes more space than is available, it will scroll. The **Add** and **Open** buttons are appended to the project item list, however they will always remain on the screen and will not scroll as part of the items list.

You can easily generate new items to show by clicking on the **Add** button. The **Open** button will do the same thing, but shows how to use the Avalonia's _Folder picking dialog_. New items are automatically added to the end of the list, and then activated. They use seemingly random content (derived from the selected folder, for example). You can delete a project from the list via the right-click context menu, and selecting **Remove**. If you remove the currently active view, **Home** will be auto-selected.

Clicking an item in the navigation bar will load and display its view. The views are _lazy-loaded_ so that their Published View is not instantiated until the view is requested. The Shell's view contains a `Panel` control (a container that just layers multiple controls in it) and it is controlled by the code-behind that is also published into the Host's _service container_. This is OK because the Shell view is basically a single instance and remains in place during the entire application life-time.

> 📜 **NOTE:**  
> The Published Views hosted in the Shell's "views panel" will have their `ViewModel.Activate` method called when the navigation bar activates their view the first time. Correspondingly their `ViewModel.Deactivate` method is called only if the view was previously activated and then the user manually removes the item from the project list. This is **not** the same as showing and hiding the view when the user navigates to another view. To detect when a certain view is shown or hidden, subscribe to the `IViewSelector.ViewChanged` event.

Project items will activate a Published View from the `ProjectFeature` - with the associated `ProjectItem` passed to the creator method as `ViewParameter`. The same item is then also passed to the view model. The view model may then load the project's data (perhaps utilizing the `ProjectItem.Path` property).

The _workspace_ here is quite minimal in terms of functionality, and it mainly shows a title, editable property, and the _Dirty_ state. When the user types text in the TextBox, the `IsDirty` property is flagged `true`. Clicking on the "**Save**" button will clear the flag.

Since `IsDirty` is defined in `ProjectItem`, also the navigation bar can visualize this state. You will see an orange circle next to the project item's icon to denote that that project has "unsaved changes".

Speaking of _unsaved changes_, since the **ViewSelectorFeature** is aware of all projects (and is managing the project item list), it is the best feature to handle the exit behavior. In its `ExitRequestedAsync` method all dirty projects are checked, and if any, a dialog is shown to the user. The dialog lists all projects that have unsaved changes and asks whether they should be saved before exiting.

- The "**Yes**" button will call the `SaveAsync` delegate (set by the view model) on all affected items and then continue (resulting in the app shutting down).
- The "**No**" button will not do anything (resulting in the app shutting down).
- The "**Cancel**" button will just cancel the token (resulting in the app resuming normally).

It also demonstrates how to localize the dialog's content.

### The Settings view

**Host.Settings** provides access to the common settings dictionary where features and view models may store key-value pairs that will persist through application restarts. You could provide a custom view to present such options in the UI, but the demo application also has a **centralized place for all settings**.

The **Settings view** can be activated via the _Settings button_ at the bottom of the navigation bar. This view and all its associated functionality is managed by the **AppSettingsFeature**.

You can access the settings collection programmatically via the `IAppSettings` _dependency_. Using this interface you can create _Categories_ and place _Settings_ in them. (or access some pre-configured ones) and register Settings of different types into them. Categories may contain sub-categories, and you can build any kind of hierarchy this way.

The Category structure will translate into Headers (and their sub headers) and Settings in one big scrollable page. At the left side there is also a _Table of Contents_ that consists of clickable "hyperlinks" that immediately scroll the page to that section.

Categories are accessed by name (or via a _localization key_ to be more precise). The AppSettingsFeature will create these default Categories by default:

```csharp
var general = this.RegisterCategory(CommonCategories.General);
var test1 = this.RegisterCategory(CommonCategories.Test1);
var test1sub1 = this.RegisterCategory(CommonCategories.Test1Sub1, test1);
var test2 = this.RegisterCategory(CommonCategories.Test2);
```

They are created during `InitializeAsync`, so all other features should place their Settings to these Categories during `StartingAsync`. Use the constants found in the `CommonCategories` class to refer to these in-built Category names.

⚡ The **CustomStartupFeature** will generate some dummy Settings and Categories just for the show.

Although you may identify Categories and Settings using any string, it is recommended that you use a localization key instead. That way also your settings' language will change when the entire application Language changes. Moreover, you should provide at least the "en-us" `ResourceDictionary` for these setting names. For more information, see the **Localization** section earlier in this guide.

In the current implementation a Setting can be one of the following types:

- `IntegerSetting`
- `BooleanSetting`
- `StringSetting`

Each of these will use their own DataTemplate for displaying the setting widget. If you want to add more setting types (for example a _Folder picker_ setting etc.) just implement the `ISetting` and create the corresponding view model and DataTemplate (in **AppSettingsView.axaml**'s `Resources`). 

Here is how you would create a new integer setting and put it the in-built "General" category:

```csharp
var mySetting = this.AppSettings!.RegisterIntegerSetting(
    "MyFeature.ExampleSetting",
    new[] { CommonCategories.General },
    defaultValue: 0);
```

The first argument should be a unique identifier. It is recommended to also use a localization key here.

The second argument is an array of category keys that denotes a path (i.e. a category plus possible sub-categories).

You may also specify the default value and a validator call-back. You can also subscribe to its `ValueChanged` event to get notified when the value is changed by the user (you might want to set a view model property accordingly, or update a **Host.Setting** value). If you do so, remember to also unsubscribe from the event when it is no longer needed.

## Dos and Dont's

**✔️ DO** prefix localization keys, Published View names, and setting keys with your feature name. For example if your feature is called "MyFeature", a localization key could be "MyFeature.AskUserConfirmation", a Published View name could be "MyFeature.Options" and a setting key could be "MyFeature.ShowDetails".

**✔️ DO** only have one feature responsible for checking whether there are unsaved changes (and displaying a dialog) before application exits.

**❌ DO NOT** set global locale in Feature code (`System.Threading.Thread.CurrentThread.CurrentCulture`) because it may cause unexpected behavior in different features (such as formatters producing wrong results).

## Summary

The core design principle is that the application is built using self-contained **Features**. Think of a Feature like dedicated view or functionality such as Status Bar, Main Menu, Navigation Panel, or Workspace. The **Shell** Feature acts as the "root", defines the main layout, and places other features' views in those UI regions.

Features may define **Published Views** that bundle an Avalonia **UserControl** and a **ViewModel** together. Features may introduce **ResourceDictionaries** that are associated with a Language, and then those PublishedViews may use localizable resources (such as strings) from them via **DynamicResource**.

Features have life-time methods as follows:

- `InitializeAsync` is called for all features. Load data and prepare the feature's own state here.
- `StartingAsync` is called for all features. Start services and connect to other features' functionality.
- `RunningAsync` is called for all features. Main Window exists and the app is now running.
- When the user attempts to exit the app, `ExitRequestedAsync` is called for all features. The operation can be canceled here.
- `ShutdownAsync` is called for all features. Stop services, clean up, and save data here.

Derive your features from the `App.Feature` base class. Derive your view models from the `ViewModel` base class. Feature classes and view model classes have access to the **Host** (property). This instance provides many common services such as **Settings**, **Localization**, **Messenger** and a **Service Container**.

The **Messenger** can be used to send message objects to recipients in a decoupled way. Alternatively, feature classes may **implement interfaces**, and an instance to their implementation can be automatically injected into a view model's or feature's property using the `DependencyAttribute`.

You can use vector based (SVG) icons with the `Icon` control and easily colorize them. Vector graphics always render clearly without blurring, making them a great alternative to traditional bitmap images.

---

END OF GUIDE 😎

