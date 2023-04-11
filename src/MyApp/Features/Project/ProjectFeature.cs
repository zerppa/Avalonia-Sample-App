namespace MyApp.Features.Project;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using MyApp.Features.ViewSelector;

/// <summary>
/// The project.
/// </summary>
/// <seealso cref="MyApp.Features.Project.IProject" />
/// <seealso cref="MyApp.App.Feature" />
public class ProjectFeature : App.Feature, IProject
{
    /// <summary>
    /// Creates the main project view.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <returns>The view model.</returns>
    [PublishedView<ProjectView>("PROJECT.MyProjectType.Main", Description = "The main view for project workspace.")]
    public ViewModel? CreateMain(object? parameter)
    {
        if (parameter is ProjectItem item)
        {
            return new ProjectViewModel(item);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<ProjectItem> CreateProject()
    {
        await Task.Delay(1);
        var timestamp = DateTime.Now;
        var folder = $"__unnamed_{timestamp.Year}{timestamp.Month:D2}{timestamp.Day:D2}-{timestamp.Hour:D2}{timestamp.Minute:D2}-{Guid.NewGuid().ToString().Split('-').Last()}";
        var path = Path.Combine(this.Host!.ProjectsPath, folder);

        // TODO: Create the directory and prepare its contents (for example create a single file with default templated content)
        return new ProjectItem { Title = "My project", Path = path, ProjectType = ProjectType.ProjectType1 };
    }
}