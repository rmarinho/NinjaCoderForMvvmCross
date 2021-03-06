﻿// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the ViewModelViewsService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace NinjaCoder.MvvmCross.Services
{
    using System.Collections.Generic;

    using NinjaCoder.MvvmCross.Constants;
    using NinjaCoder.MvvmCross.Factories.Interfaces;
    using NinjaCoder.MvvmCross.Infrastructure.Services;
    using NinjaCoder.MvvmCross.Services.Interfaces;

    using Scorchio.Infrastructure.Extensions;
    using Scorchio.Infrastructure.Services.Testing.Interfaces;
    using Scorchio.VisualStudio.Entities;
    using Scorchio.VisualStudio.Extensions;
    using Scorchio.VisualStudio.Services;
    using Scorchio.VisualStudio.Services.Interfaces;

    /// <summary>
    ///  Defines the ViewModelViewsService type.
    /// </summary>
    internal class ViewModelViewsService : BaseService, IViewModelViewsService
    {
        /// <summary>
        /// The code snippet factory.
        /// </summary>
        private readonly ICodeSnippetFactory codeSnippetFactory;

        /// <summary>
        /// The settings service.
        /// </summary>
        private readonly ISettingsService settingsService;

        /// <summary>
        /// The testing service.
        /// </summary>
        private readonly ITestingService testingService;

        /// <summary>
        /// The core project service.
        /// </summary>
        private IProjectService projectService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelViewsService" /> class.
        /// </summary>
        /// <param name="codeSnippetFactory">The code snippet factory.</param>
        /// <param name="settingsService">The settings service.</param>
        /// <param name="testingServiceFactory">The testing service factory.</param>
        public ViewModelViewsService(
            ICodeSnippetFactory codeSnippetFactory,
            ISettingsService settingsService,
            ITestingServiceFactory testingServiceFactory)
        {
            TraceService.WriteLine("ViewModelViewsService::Constructor");

            this.codeSnippetFactory = codeSnippetFactory;
            this.settingsService = settingsService;
            this.testingService = testingServiceFactory.GetTestingService();
        }

        /// <summary>
        /// Adds the view model and views.
        /// </summary>
        /// <param name="coreProjectService">The core project service.</param>
        /// <param name="visualStudioService">The visual studio service.</param>
        /// <param name="templateInfos">The template infos.</param>
        /// <param name="viewModelName">Name of the view model.</param>
        /// <param name="addUnitTests">if set to <c>true</c> [add unit tests].</param>
        /// <param name="viewModelInitiateFrom">The view model initiate from.</param>
        /// <param name="viewModelNavigateTo">The view model navigate to.</param>
        /// <returns>The messages.</returns>
        public IEnumerable<string> AddViewModelAndViews(
            IProjectService coreProjectService,
            IVisualStudioService visualStudioService,
            IEnumerable<ItemTemplateInfo> templateInfos,
            string viewModelName,
            bool addUnitTests,
            string viewModelInitiateFrom,
            string viewModelNavigateTo)
        {
            TraceService.WriteLine("ViewModelViewsService::AddViewModelAndViews");

            this.projectService = coreProjectService;

             IEnumerable<string> messages = visualStudioService.SolutionService.AddItemTemplateToProjects(templateInfos);

            //// we now need to amend code in the unit test file that references FirstViewModel to this ViewModel

            if (addUnitTests)
            {
                this.UpdateUnitTestFile(
                    visualStudioService,
                    viewModelName);
            }

            if (string.IsNullOrEmpty(viewModelInitiateFrom) == false)
            {
                this.UpdateInitViewModel(
                    viewModelName,
                    viewModelInitiateFrom);
            }

            if (string.IsNullOrEmpty(viewModelNavigateTo) == false)
            {
                this.UpdateNavigateToViewModel(
                    viewModelName,
                    viewModelNavigateTo);
            }

            return messages;
        }

        /// <summary>
        /// Gets the code snippet.
        /// </summary>
        /// <returns>The code snippet.</returns>
        internal CodeSnippet GetCodeSnippet()
        {
            return this.codeSnippetFactory.GetSnippet(this.settingsService.ViewModelNavigationSnippetFile);
        }

        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        /// <param name="viewModelName">Name of the view model.</param>
        /// <returns>The command name.</returns>
        internal string GetCommandName(string viewModelName)
        {
            TraceService.WriteLine("ViewModelViewsService::GetCommandName ViewModelName=" + viewModelName);

            string command = "UnknownCommand";

            int index = viewModelName.IndexOf("ViewModel", System.StringComparison.Ordinal);

            if (index != -1)
            {
                command = string.Format("{0}Command", viewModelName.Substring(0, index));
            }

            return command;
        }

        /// <summary>
        /// Gets the command name instance.
        /// </summary>
        /// <param name="viewModelName">Name of the view model.</param>
        /// <returns>The command name instance.</returns>
        internal string GetCommandNameInstance(string viewModelName)
        {
            TraceService.WriteLine("ViewModelViewsService::GetCommandNameInstance ViewModelName=" + viewModelName);

            string command = this.GetCommandName(viewModelName);

            int index = viewModelName.IndexOf("ViewModel", System.StringComparison.Ordinal);

            if (index != -1)
            {
                command = string.Format("{0}Command", viewModelName.Substring(0, index).ToLower());
            }

            return command;
        }

        /// <summary>
        /// Gets the view model item service.
        /// </summary>
        /// <param name="viewModelName">Name of the view model.</param>
        /// <returns>The project item service.</returns>
        internal IProjectItemService GetViewModelItemService(string viewModelName)
        {
            return this.projectService.GetProjectItem(viewModelName + ".cs");
        }

        /// <summary>
        /// Updates the unit test file.
        /// </summary>
        /// <param name="visualStudioService">The visual studio service.</param>
        /// <param name="viewModelName">Name of the view model.</param>
        internal void UpdateUnitTestFile(
            IVisualStudioService visualStudioService,
            string viewModelName)
        {
            TraceService.WriteLine("ViewModelViewsService::UpdateUnitTestFile ViewModelName=" + viewModelName);

            IProjectService testProjectService = visualStudioService.CoreTestsProjectService;

            if (testProjectService != null)
            {
                IProjectItemService projectItemService = testProjectService.GetProjectItem("Test" + viewModelName);

                if (projectItemService != null)
                {
                    this.testingService.UpdateTestClassAttribute(projectItemService);
                    this.testingService.UpdateTestMethodAttribute(projectItemService);

                    List<KeyValuePair<string, string>> replacementVariables = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("FirstViewModel", viewModelName),
                            new KeyValuePair<string, string>("firstViewModel", viewModelName.LowerCaseFirstCharacter())
                        };

                    this.testingService.UpdateFile(projectItemService, replacementVariables);
                }
            }
        }

        /// <summary>
        /// Updates the init view model.
        /// </summary>
        /// <param name="viewModelName">Name of the view model.</param>
        /// <param name="initViewModelName">Name of the init view model.</param>
        internal void UpdateInitViewModel(
            string viewModelName,
            string initViewModelName)
        {
            TraceService.WriteLine("ViewModelViewsService::UpdateInitViewModel ViewModelName=" + initViewModelName);

            IProjectItemService projectItemService = this.GetViewModelItemService(initViewModelName);

            if (projectItemService != null)
            {
                //// add code to the viewModel

                CodeSnippet codeSnippet = this.GetCodeSnippet();
                codeSnippet.AddReplacementVariable(Variables.ViewModelName, viewModelName);
                codeSnippet.AddReplacementVariable(Variables.CommandNameInstance, this.GetCommandNameInstance(viewModelName));
                codeSnippet.AddReplacementVariable(Variables.CommandName, this.GetCommandName(viewModelName));
                
                projectItemService.ImplementCodeSnippet(
                    codeSnippet,
                    this.settingsService.FormatFunctionParameters);
            }
        }

        /// <summary>
        /// Updates the navigate to view model.
        /// </summary>
        /// <param name="viewModelName">Name of the view model.</param>
        /// <param name="navigateToViewModelName">Name of the navigate to view model.</param>
        internal void UpdateNavigateToViewModel(
            string viewModelName,
            string navigateToViewModelName)
        {
            TraceService.WriteLine("ViewModelViewsService::UpdateNavigateToViewModel ViewModelName=" + navigateToViewModelName);

            IProjectItemService projectItemService = this.GetViewModelItemService(viewModelName);

            if (projectItemService != null)
            {
                //// add code to the viewModel

                CodeSnippet codeSnippet = this.GetCodeSnippet();
                codeSnippet.AddReplacementVariable(Variables.ViewModelName, navigateToViewModelName);
                codeSnippet.AddReplacementVariable(Variables.CommandName, this.GetCommandName(navigateToViewModelName));
                codeSnippet.AddReplacementVariable(Variables.CommandNameInstance, this.GetCommandNameInstance(navigateToViewModelName));

                projectItemService.ImplementCodeSnippet(
                    codeSnippet,
                    this.settingsService.FormatFunctionParameters);
            }
        }
    }
}
