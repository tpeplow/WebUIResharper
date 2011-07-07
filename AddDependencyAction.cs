using System.Collections.Generic;
using System.Drawing;
using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;
using JetBrains.Application.Progress;
using JetBrains.ReSharper.Refactorings.Conflicts;
using JetBrains.ReSharper.Refactorings.Workflow;
using JetBrains.UI.RichText;

namespace WebUIResharper
{
    [ActionHandler]
    public class WebUI_AddDependencyAction: ExtensibleRefactoringAction<AddDependencyWorkflowProvider>
    {
        protected override RichText Caption
        {
            get { return "Add service dependency..."; }
        }
    }

    public class AddDependencyWorkflowProvider: IRefactoringWorkflowProvider
    {
        public IEnumerable<IRefactoringWorkflow> CreateWorkflow(IDataContext dataContext)
        {
            yield return new AddDependencyWorkflow();
        }
    }

    public class AddDependencyWorkflow: RefactoringWorkflowBase
    {
        public override bool PreExecute(IProgressIndicator progressIndicator)
        {
            return true;
        }

        public override bool HasUI
        {
            get { return true; }
        }

        public override string Title
        {
            get { return "Add Service Dependency..."; }
        }

        public override Image Icon
        {
            get { return null; }
        }

        public override string ActionId
        {
            get { return "WebUI.AddDependency"; }
        }

        public override RefactoringActionGroup ActionGroup
        {
            get { return RefactoringActionGroup.Blessed; }
        }

        public override string HelpKeyword
        {
            get { return ""; }
        }

        public override IConflictSearcher ConflictSearcher
        {
            get { return null; }
        }

        public override IRefactoringPage FirstPendingRefactoringPage
        {
            get { return null; }
        }

        public override bool MightModifyManyDocuments
        {
            get { return true; }
        }

        public override bool Execute(IProgressIndicator progressIndicator)
        {
            return true;
        }

        public override bool PostExecute(IProgressIndicator pi)
        {
            return true;
        }

        public override bool Initialize(IDataContext context)
        {
            return true;
        }

        public override bool IsAvailable(IDataContext context)
        {
            return true;
        }
    }
}
