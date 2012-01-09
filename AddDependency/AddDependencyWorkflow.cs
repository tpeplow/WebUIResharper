using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JetBrains.Application.DataContext;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.CodeStyle;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Naming.Extentions;
using JetBrains.ReSharper.Psi.Naming.Impl;
using JetBrains.ReSharper.Psi.Naming.Settings;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Refactorings.Conflicts;
using JetBrains.ReSharper.Refactorings.Workflow;
using JetBrains.TextControl;

namespace WebUIResharper.AddDependency
{
    public class AddDependencyWorkflow : RefactoringWorkflowBase
    {
        private AddDependencyPage _page;
        private IDeclaredElementPointer<ITypeElement> _class;
        private IDeclaredElementPointer<IConstructor> _ctor;
        private string _parameterType;
        private ISolution _solution;

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
            get { return "Add Service Dependency"; }
        }

        public override Image Icon
        {
            get { return null; }
        }

        public override string ActionId
        {
            get { return "AddDependency"; }
        }

        public override RefactoringActionGroup ActionGroup
        {
            get { return RefactoringActionGroup.Blessed; }
        }

        public override string HelpKeyword
        {
            get { return ""; }
        }

        public override ISolution Solution { get { return _solution; } }

        public override IConflictSearcher ConflictSearcher
        {
            get { return null; }
        }

        public override IRefactoringPage FirstPendingRefactoringPage
        {
            get { return _page; }
        }

        public override bool MightModifyManyDocuments
        {
            get { return true; }
        }

        public override bool Execute(IProgressIndicator progressIndicator)
        {
            var languageService = CSharpLanguage.Instance.LanguageService();
            if (languageService == null)
                return false;
            var ctor = _ctor.FindDeclaredElement();
            if (ctor == null)
                return false;
            var definingClass = _class.FindDeclaredElement();
            if (definingClass == null)
                return false;
            var factory = CSharpElementFactory.GetInstance(ctor.Module);
            var ctorDecl = ctor.GetDeclarations().FirstOrDefault();
            if (ctorDecl == null)
            {
                var typeDecl = definingClass.GetDeclarations().FirstOrDefault() as IClassLikeDeclaration;
                if (typeDecl == null)
                    return false;
                var typeBody = typeDecl.Body;
                ctorDecl = factory.CreateTypeMemberDeclaration("public $0() {}", typeDecl.DeclaredName);
                if (typeBody.FirstChild == null)
                    return false;

                ctorDecl = ModificationUtil.AddChildBefore(
                    typeBody,
                    typeBody.FirstChild.NextSibling,
                    ctorDecl).GetContainingNode<IConstructorDeclaration>(true);
            }
            if (ctorDecl == null)
                return false;
            var type = CSharpTypeFactory.CreateType(_parameterType, ctorDecl);
            if (!type.IsResolved)
                type = CSharpTypeFactory.CreateType(_parameterType, ctorDecl.GetPsiModule());
            string recommendedName = null;
            if (!type.IsResolved)
            {
                var presentableName = type.GetPresentableName(CSharpLanguage.Instance);
                var indexOfGeneric = presentableName.IndexOf('<');
                if (indexOfGeneric != -1)
                {
                    var interfaceName = presentableName.Substring(1, indexOfGeneric - 1);
                    var genericArgument = presentableName.Substring(indexOfGeneric).Trim('<', '>');
                    recommendedName = type.GetPsiServices().Naming.Suggestion.GetDerivedName(
                        genericArgument + interfaceName,
                        NamedElementKinds.Parameters,
                        ScopeKind.Common,
                        CSharpLanguage.Instance,
                        new SuggestionOptions(),
                        ctorDecl.GetSourceFile());
                }
                var interfaceDecl = factory.CreateTypeMemberDeclaration("public interface IFoo {}");
                interfaceDecl.SetName(presentableName);
                languageService.CodeFormatter.Format(interfaceDecl, CodeFormatProfile.GENERATOR);
                var containingType = ctor.GetContainingType();
                if (containingType == null)
                    return false;
                var containingTypeDecl = containingType.GetDeclarations().First();
                ModificationUtil.AddChildBefore(containingTypeDecl, interfaceDecl);
            }
            type = CSharpTypeFactory.CreateType(_parameterType, ctorDecl);
            if (recommendedName == null)
            {
                var suggestionOptions = new SuggestionOptions();
                recommendedName = type.GetPsiServices().Naming.Suggestion.GetDerivedName(
                    type.GetPresentableName(CSharpLanguage.Instance),
                    NamedElementKinds.Parameters, 
                    ScopeKind.Common,
                    CSharpLanguage.Instance, 
                    suggestionOptions,
                    ctorDecl.GetSourceFile());
            }
            var parametersOwner = ctorDecl as ICSharpParametersOwnerDeclaration;
            var references = FindReferences(parametersOwner, progressIndicator);

            if (parametersOwner == null)
                return false;
            parametersOwner.AddParameterDeclarationAfter(
                ParameterKind.VALUE, type, recommendedName,
                parametersOwner.ParameterDeclarations.LastOrDefault());

            foreach (var reference in references)
                ChangeReference(reference, recommendedName, type);

            return true;
        }

        private void ChangeReference(IArgumentsOwner reference, string recommendedName, IType type)
        {
            var csharpOwner = reference as ICSharpArgumentsOwner;
            if (csharpOwner == null)
                return;
            var factory = CSharpElementFactory.GetInstance(type.Module);
            var inField = false;
            if (csharpOwner.GetContainingNode<IFieldDeclaration>() != null)
                inField = true;
            var expression = factory.CreateExpression(inField ? "TODO" : recommendedName);
            var cSharpArgument = factory.CreateArgument(ParameterKind.VALUE, null, expression);
            csharpOwner.AddArgumentAfter(
                cSharpArgument,
                csharpOwner.Arguments.LastOrDefault());

            if (!inField) AddStub(cSharpArgument, reference, recommendedName);
        }

        protected virtual void AddStub(ICSharpArgument cSharpArgument, IArgumentsOwner reference, string recommendedName)
        {
        }

        private static IEnumerable<IArgumentsOwner> FindReferences(ICSharpParametersOwnerDeclaration parametersOwner, IProgressIndicator progressIndicator)
        {
            var references = new List<IArgumentsOwner>();
            var consumer = new FindResultConsumer(
                r =>
                {
                    var owners = new HashSet<IArgumentsOwner>();
                    var reference = r as FindResultReference;
                    if (reference != null)
                    {
                        var ref2 = reference.Reference;
                        var resolveType = ref2.CheckResolveResult();
                        if (resolveType == ResolveErrorType.INCORRECT_PARAMETER_NUMBER)
                            return FindExecution.Continue;
                        var argumentsOwner = ref2.GetTreeNode().GetContainingNode<IArgumentsOwner>(true);
                        if (argumentsOwner != null)
                        {
                            if (owners.Contains(argumentsOwner))
                                return FindExecution.Continue;
                            owners.Add(argumentsOwner);
                            references.Add(argumentsOwner);
                        }
                    }
                    return FindExecution.Continue;
                });
            var searchAction = new SearchAction(
                parametersOwner.GetPsiServices().Finder,
                parametersOwner.DeclaredElement,
                consumer,
                SearchPattern.FIND_USAGES);
            searchAction.Task(progressIndicator);
            return references;
        }

        public override bool Initialize(IDataContext context)
        {
            _solution = context.GetData(JetBrains.ProjectModel.DataContext.DataConstants.SOLUTION);
            _page = new AddDependencyPage(
                s => _parameterType = s,
                context.GetData(JetBrains.ProjectModel.DataContext.DataConstants.SOLUTION));
            var @class = GetClass(context);
            _class = @class.CreateElementPointer();
            var ctor = @class.Constructors.FirstOrDefault();
            if (ctor != null)
                _ctor = ctor.CreateElementPointer();
            return true;
        }

        public override bool IsAvailable(IDataContext context)
        {
            var classElement = GetClass(context);
            if (classElement == null)
                return false;

            if (classElement.Constructors.Count() > 1)
                return false;

            var textControl = context.GetData(JetBrains.TextControl.DataContext.DataConstants.TEXT_CONTROL);
            if ((textControl != null) && textControl.Selection.HasSelection())
                return false;

            return true;
        }

        private static ITypeElement GetClass(IDataContext context)
        {
            var declaredElement = context.GetData(JetBrains.ReSharper.Psi.Services.DataConstants.DECLARED_ELEMENT);
            var classDecl = declaredElement as ITypeElement;
            if (classDecl == null)
            {
                var parametersOwner = declaredElement as IParametersOwner;
                if (parametersOwner == null)
                    return null;
                var containingType = parametersOwner.GetContainingType();
                if (containingType == null)
                    return null;
                var elementPointer = containingType.CreateElementPointer();
                classDecl = elementPointer.FindDeclaredElement();
            }
            return classDecl;
        }

        public override bool PostExecute(IProgressIndicator pi)
        {
            return true;
        }
    }
}