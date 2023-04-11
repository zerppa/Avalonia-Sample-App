namespace MyApp.Features.Project;

using System.Threading.Tasks;

using MyApp.Features.ViewSelector;

/// <summary>
/// Controls the Project.
/// </summary>
public interface IProject
{
    /// <summary>
    /// Creates a new project.
    /// </summary>
    /// <returns>The <see cref="ProjectItem"/>.</returns>
    Task<ProjectItem> CreateProject();
}