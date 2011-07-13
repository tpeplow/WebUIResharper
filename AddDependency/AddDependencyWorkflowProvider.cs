using System.Collections.Generic;
using JetBrains.Application.DataContext;
using JetBrains.ReSharper.Refactorings.Workflow;

namespace WebUIResharper.AddDependency
{
    [RefactoringWorkflowProvider]
    public class AddDependencyWorkflowProvider: IRefactoringWorkflowProvider
    {
        public IEnumerable<IRefactoringWorkflow> CreateWorkflow(IDataContext dataContext)
        {
            yield return new AddDependencyWorkflow();
        }
    }
}