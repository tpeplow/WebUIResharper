using JetBrains.ActionManagement;
using JetBrains.ReSharper.Refactorings.Workflow;
using JetBrains.UI.RichText;

namespace WebUIResharper.AddDependency
{
    [ActionHandler(new[]{ "AddDependency" })]
    public class AddDependencyAction: ExtensibleRefactoringAction<AddDependencyWorkflowProvider>
    {
        protected override RichText Caption
        {
            get { return "Add service dependency..."; }
        }
    }
}
