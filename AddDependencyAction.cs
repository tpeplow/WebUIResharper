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
    [ActionHandler("AddDependency")]
    public class AddDependencyAction: ExtensibleRefactoringAction<AddDependencyWorkflowProvider>
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
            return false;
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
            get { return null; }
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
            return false;
        }

        public override bool PostExecute(IProgressIndicator pi)
        {
            return false;
        }

        public override bool Initialize(IDataContext context)
        {
            return false;
        }

        public override bool IsAvailable(IDataContext context)
        {
            return false;
        }
    }
}
