﻿// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the ApplicationController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace NinjaCoder.MvvmCross.Controllers
{
    using System.Collections.Generic;
    using System.Windows;

    using EnvDTE;

    using MahApps.Metro;

    using NinjaCoder.MvvmCross.Infrastructure.Services;
    using NinjaCoder.MvvmCross.ViewModels;
    using NinjaCoder.MvvmCross.ViewModels.Options;
    using NinjaCoder.MvvmCross.Views;

    using Scorchio.Infrastructure.Entities;
    using Scorchio.Infrastructure.EventArguments;
    using Scorchio.Infrastructure.Services;
    using Scorchio.Infrastructure.Translators;
    using Scorchio.VisualStudio.Services;
    using Services.Interfaces;

    /// <summary>
    /// Defines the ApplicationController type.
    /// </summary>
    internal class ApplicationController : BaseController
    {
        /// <summary>
        /// The application service.
        /// </summary>
        private readonly IApplicationService applicationService;

        /// <summary>
        /// The translator.
        /// </summary>
        private readonly ITranslator<IList<Accent>, IEnumerable<AccentColor>> translator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationController" /> class.
        /// </summary>
        /// <param name="applicationService">The application service.</param>
        /// <param name="configurationService">The configuration service.</param>
        /// <param name="visualStudioService">The visual studio service.</param>
        /// <param name="readMeService">The read me service.</param>
        /// <param name="settingsService">The settings service.</param>
        /// <param name="messageBoxService">The message box service.</param>
        /// <param name="resolverService">The resolver service.</param>
        /// <param name="translator">The translator.</param>
        public ApplicationController(
            IApplicationService applicationService,
            IConfigurationService configurationService,
            IVisualStudioService visualStudioService,
            IReadMeService readMeService,
            ISettingsService settingsService,
            IMessageBoxService messageBoxService,
            IResolverService resolverService,
            ITranslator<IList<Accent>, IEnumerable<AccentColor>> translator)
            : base(
            configurationService,
            visualStudioService,
            readMeService,
            settingsService,
            messageBoxService,
            resolverService)
        {
            TraceService.WriteLine("ApplicationController::Constructor");

            this.applicationService = applicationService;
            this.translator = translator;
        }

        /// <summary>
        /// Checks for updates.
        /// </summary>
        public void CheckForUpdates()
        {
            TraceService.WriteLine("ApplicationController::CheckForUpdates");

            this.applicationService.CheckForUpdates();
        }

        /// <summary>
        /// Determines whether [is update available].
        /// </summary>
        /// <returns>True or false.</returns>
        public bool IsUpdateAvailable()
        {
            TraceService.WriteLine("ApplicationController::IsUpdateAvailable");

            return this.applicationService.IsUpdateAvailable();
        }

        /// <summary>
        /// Checks for updates if ready.
        /// </summary>
        public void CheckForUpdatesIfReady()
        {
            TraceService.WriteLine("ApplicationController::CheckForUpdatesIfReady");

            if (this.SettingsService.CheckForUpdates)
            {
                bool available = this.IsUpdateAvailable();

                if (available == false)
                {
                    this.CheckForUpdates();
                }

                else
                {
                    this.ShowDialog<DownloadViewModel>(new DownloadView());
                }
            }
        }

        /// <summary>
        /// Shows the options.
        /// </summary>
        public void ShowOptions()
        {
            TraceService.WriteLine("ApplicationController::ShowOptions");

            OptionsView view = new OptionsView();

            ResourceDictionary resourceDictionary = this.GetLanguageDictionary();
            
            view.SetLanguageDictionary(resourceDictionary);

            OptionsViewModel viewModel = this.ResolverService.Resolve<OptionsViewModel>();
            viewModel.LanguageDictionary = resourceDictionary;

            view.DataContext = viewModel;

            viewModel.VisualViewModel.Colors = translator.Translate(view.Colors);

            //// use weak references.
            WeakEventManager<VisualViewModel, ThemeChangedEventArgs>
                .AddHandler(viewModel.VisualViewModel, "ThemeChanged", view.ThemeChanged);

            //// set the theme.
            view.ChangeTheme(
                this.CurrentTheme, 
                this.SettingsService.ThemeColor);

            view.ShowDialog();

            WeakEventManager<VisualViewModel, ThemeChangedEventArgs>
                    .RemoveHandler(viewModel.VisualViewModel, "ThemeChanged", view.ThemeChanged);

            //// in case any of the setting have changed to do with logging reset them!
            TraceService.Initialize(
                this.SettingsService.LogToTrace, 
                false,  //// log to console.
                this.SettingsService.LogToFile, 
                this.SettingsService.LogFilePath, 
                this.SettingsService.DisplayErrors);
        }

        /// <summary>
        /// Shows the about box.
        /// </summary>
        public void ShowAboutBox()
        {
            TraceService.WriteLine("ApplicationController::ShowAboutBox");

            this.ShowDialog<AboutViewModel>(new AboutView());
        }

        /// <summary>
        /// Gets the projects.
        /// </summary>
        /// <returns>The projects.</returns>
        public IEnumerable<Project> GetProjects()
        {
            TraceService.WriteLine("ApplicationController::GetProjects");

            return this.VisualStudioService.SolutionService.GetProjects();
        }

        /// <summary>
        /// Views the log file.
        /// </summary>
        public void ViewLogFile()
        {
            TraceService.WriteLine("ApplicationController::ViewLogFile");

            this.applicationService.ViewLogFile();
        }

        /// <summary>
        /// Clears the log file.
        /// </summary>
        public void ClearLogFile()
        {
            TraceService.WriteLine("ApplicationController::ClearLogFile");
            
            this.applicationService.ClearLogFile();
        }
    }
}
