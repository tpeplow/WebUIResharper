using System;
using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.Application.Progress;
using JetBrains.CommonControls.Validation;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.Util;
using JetBrains.ReSharper.Feature.Services.Search;
using JetBrains.ReSharper.Features.Common.UI;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Refactorings.Workflow;
using JetBrains.UI.Application;
using JetBrains.UI.CommonControls;
using JetBrains.UI.CrossFramework;

namespace WebUIResharper.AddDependency
{
    public class AddDependencyPage : SafeUserControl, IValidatorProvider, IRefactoringPage
    {
        private readonly ISolution _solution;
        private readonly IProperty<bool> _continueEnabled;

        private readonly CompletionPickerEdit _typeEditBox;
        private readonly CSharpTypeValidator _typeValidator;

        public AddDependencyPage(Action<string> updateParameterType, ISolution solution)
        {
            _typeValidator = new CSharpTypeValidator();
            _solution = solution;
            _continueEnabled = new Property<bool>("ContinueEnabled", true);

            var shellLocks = Shell.Instance.GetComponent<IShellLocks>();
            _typeEditBox = new CompletionPickerEdit(shellLocks, Shell.Instance.Components.Tooltips())
                           {
                               Width = 300, 
                           };
            var lifetime = this.DefineWinFormsLifetime();
            _typeEditBox.Settings.Value = TypeChooser.CreateSettings(
                lifetime,
                solution,
                LibrariesFlag.SolutionAndLibraries,
                CSharpLanguage.Instance,
                shellLocks);
            _typeEditBox.Text.Change.Advise_HasNew(
                lifetime,
                args =>
                {
                    updateParameterType(args.New);
                    UpdateUI();
                });
            
            Controls.Add(_typeEditBox);
        }

        private void UpdateUI()
        {
            var shellLocks = Shell.Instance.GetComponent<IShellLocks>();
            shellLocks.ReentrancyGuard.ExecuteOrQueue(
                "AddDependencyPage.UpdateUI",
                delegate
                {
                    if (IsDisposed)
                        return;
                    using (ReadLockCookie.Create())
                        PsiManager.GetInstance(_solution).CommitAllDocuments();

                    Shell.Instance.GetComponent<FormValidators>().GetOrCreate(this).Update();
                });
        }

        public IEnumerable<IValidator> Validators
        {
            get
            {
                return new[]
                       {
                           new TextValidatorReentrantSafe(
                               _typeEditBox, 
                               ValidatorSeverity.Error, 
                               "Dependency type is not valid", 
                               returnTypeText => _typeValidator.IsValidReturnType(returnTypeText),
                               Shell.Instance.GetComponent<IShellLocks>(),
                               Shell.Instance.GetComponent<WindowsMessageHookManager>())
                       };
            }
        }

        public IRefactoringPage Commit(IProgressIndicator pi)
        {
            return null;
        }

        public bool Initialize(IProgressIndicator pi)
        {
            pi.Start(1);
            pi.Stop();
            return true;
        }

        public bool RefreshContents(IProgressIndicator pi)
        {
            pi.Start(1);
            pi.Stop();
            return true;
        }

        public IProperty<bool> ContinueEnabled
        {
            get { return _continueEnabled; }
        }

        public string Description
        {
            get { return "Provide the type of the dependency you would like to add."; }
        }

        public string Title
        {
            get { return "Add service dependency"; }
        }

        public EitherControl View
        {
            get { return this; }
        }

        public bool DoNotShow
        {
            get { return false; }
        }
    }
}