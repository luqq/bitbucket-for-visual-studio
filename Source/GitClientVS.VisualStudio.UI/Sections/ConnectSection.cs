﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GitClientVS.UI.Views;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.MVVM;
using GitClientVS.VisualStudio.UI.TeamFoundation;
using GitClientVS.Contracts;
using GitClientVS.Contracts.Interfaces.Services;
using GitClientVS.Contracts.Interfaces.ViewModels;
using GitClientVS.Contracts.Interfaces.Views;
using GitClientVS.Contracts.Models;
using GitClientVS.Infrastructure;
using Reactive.EventAggregator;
using GitClientVS.Infrastructure.Extensions;
using GitClientVS.Services;
using log4net;

namespace GitClientVS.VisualStudio.UI.Sections
{
    [TeamExplorerSection(Id, TeamExplorerPageIds.Connect, 10)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ConnectSection : TeamExplorerBaseSection
    {
        private readonly IUserInformationService _userInfoService;
        private readonly IAppServiceProvider _appServiceProvider;
        private readonly IGitClientService _gitClient;
        private ITeamExplorerSection _section;
        private const string Id = "a6701970-28da-42ee-a0f4-9e02f486de2c";
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [ImportingConstructor]
        public ConnectSection(
            IGitClientService bucketService,
            IUserInformationService userInfoService,
            IAppServiceProvider appServiceProvider,
            IGitClientService gitClient,
            IConnectSectionView sectionView) : base(sectionView)
        {
            _userInfoService = userInfoService;
            _appServiceProvider = appServiceProvider;
            _gitClient = gitClient;
            Title = "Bitbucket Extension";
        }


        public override async void Initialize(object sender, SectionInitializeEventArgs e)
        {
            ServiceProvider = e.ServiceProvider;
            LoggerConfigurator.Setup(); // TODO this needs to be set in the entry point like package
            _userInfoService.LoadUserStoreInformation();

            await GitClientLogin();

            _section = GetSection(TeamExplorerConnectionsSectionId);
            _appServiceProvider.GitServiceProvider = e.ServiceProvider;

            base.Initialize(sender, e);
        }

        private async Task GitClientLogin()
        {
            if (_userInfoService.ConnectionData.IsLoggedIn)
            {
                try
                {
                    await _gitClient.LoginAsync(_userInfoService.ConnectionData.UserName, _userInfoService.ConnectionData.Password);
                }
                catch (Exception ex)
                {
                    Logger.Warn("Couldn't login user using stored credentials. Credentials must have been changed or there is no internet connection", ex);
                    _userInfoService.CleanUserStoreInformation();
                }
            }
        }

        protected ITeamExplorerSection GetSection(Guid section)
        {
            return ((ITeamExplorerPage)ServiceProvider.GetService(typeof(ITeamExplorerPage))).GetSection(section);
        }

        #region JustInCaseLoadingAssemblies

        static readonly string[] OurAssemblies =
      {
            "GitClientVS.Api",
            "GitClientVS.Contracts",
            "GitClientVS.Infrastructure",
            "GitClientVS.Services",
            "GitClientVS.UI",
            "GitClientVS.VisualStudio.UI"
        };

        private Assembly LoadNotLoadedAssemblies(object sender, ResolveEventArgs e)
        {
            AppDomain.CurrentDomain.AssemblyResolve += LoadNotLoadedAssemblies;
            try
            {
                var name = new AssemblyName(e.Name);
                if (!OurAssemblies.Contains(name.Name))
                    return null;
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var filename = Path.Combine(path, name.Name + ".dll");
                if (!File.Exists(filename))
                    return null;
                return Assembly.LoadFrom(filename);
            }
            catch (Exception ex)
            {
                var log = string.Format(CultureInfo.CurrentCulture,
                    "Error occurred loading {0} from {1}.{2}{3}{4}",
                    e.Name,
                    Assembly.GetExecutingAssembly().Location,
                    Environment.NewLine,
                    ex,
                    Environment.NewLine);

                Logger.Error(log);

            }
            return null;
        }
        #endregion

    }
}