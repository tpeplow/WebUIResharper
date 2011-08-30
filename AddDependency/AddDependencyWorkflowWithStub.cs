using System.Linq;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using WebUIResharper.CreateStubFromUsage;

namespace WebUIResharper.AddDependency
{
    public class AddDependencyWorkflowWithStub : AddDependencyWorkflow
    {
        public override string Title
        {
            get { return "Add Service Dependency with Stub"; }
        }

        public override string ActionId
        {
            get { return "AddDependencyWithStub"; }
        }

        protected override void AddStub(ICSharpArgument cSharpArgument, IArgumentsOwner reference, string recommendedName)
        {
            var referenceExpression = cSharpArgument.Value as IReferenceExpression;
            var anchor = reference.GetContainingNode<IBlock>();
            var containingMethod = anchor.Parent as IMethodDeclaration;
            if (containingMethod == null)
                return;
            if (!containingMethod.Attributes.Select(x => x.Name.QualifiedName).Any(x => x == "SetUp" || x == "Test" || x == "TestCase"))
                return;
            var addStub = new AddRhinoStub(recommendedName, referenceExpression, anchor);
            addStub.Execute();
        }
    }
}