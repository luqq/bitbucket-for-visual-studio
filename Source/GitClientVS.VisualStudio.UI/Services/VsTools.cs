﻿using System.ComponentModel.Composition;
using EnvDTE;
using GitClientVS.Contracts.Interfaces.Services;

namespace GitClientVS.VisualStudio.UI.Services
{
    [Export(typeof(IVsTools))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VsTools : IVsTools
    {
        private readonly IAppServiceProvider _appServiceProvider;
        private DTE _dte;

        [ImportingConstructor]
        public VsTools(IAppServiceProvider appServiceProvider)
        {
            _appServiceProvider = appServiceProvider;
        }

        public void RunDiff(string file1, string file2, string fileDisplayName1, string fileDisplayName2)
        {
            _dte = (DTE)_appServiceProvider.GetService(typeof(DTE));
            _dte.ExecuteCommand("Tools.DiffFiles", $"\"{file1}\" \"{file2}\" \"{fileDisplayName1}\" \"{fileDisplayName2}\"");
        }
    }
}
