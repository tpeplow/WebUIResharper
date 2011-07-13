using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Intentions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.CSharp.Util;
using JetBrains.ReSharper.Psi.CodeStyle;
using JetBrains.ReSharper.Psi.ExpectedTypes;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Services;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace WebUIResharper.CreateStubFromUsage
{
    [QuickFix(null, BeforeOrAfter.Before)]
    public class CreateStubFromUsageFix : BulbItemImpl, IQuickFix
    {
        private readonly IReference _myReference;
        private readonly IReferenceExpression _myReferenceExpression;
        private readonly IBlock _anchor;

        public CreateStubFromUsageFix(NotResolvedError error)
        {
            _myReference = error.Reference;
            _myReferenceExpression = GetReferenceExpression();
            _anchor = ContainingElement<IBlock>();
        }

        public override string Text
        {
            get { return string.Format("Introduce stub '{0}'", _myReference.GetName()); }
        }

        public bool IsAvailable(IUserDataHolder cache)
        {
            return _myReferenceExpression != null
                && _anchor != null
                && !IsInvocationExpression(_myReferenceExpression)
                && !InsideConstantExpression
                && IsUnqualifiedExpression
                && IsUnresolvedOrWrongNameCase();
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var usages = CollectUsages(_anchor);
            var typeConstraint = GuessTypesForUsages(usages);

            var declarationStatement = GetDeclarationStatement(
                usages,
                typeConstraint);

            FormatCode(declarationStatement);

            return tc => { };
        }

        private static IExpectedTypeConstraint GuessTypesForUsages(IEnumerable<ICSharpExpression> usages)
        {
            return ExpectedTypesUtil.GuessTypesIntersection(usages.Cast<IExpression>().ToList());
        }

        private static void FormatCode(IDeclarationStatement declarationStatement)
        {
            var languageService = declarationStatement.Language.LanguageService();
            if (languageService == null)
                return;
            languageService.CodeFormatter.Format(
                declarationStatement,
                CodeFormatProfile.GENERATOR);
        }

        private IReferenceExpression GetReferenceExpression()
        {
            return _myReference != null
                ? _myReference.GetTreeNode() as IReferenceExpression
                : null;
        }

        private bool IsUnresolvedOrWrongNameCase()
        {
            var type = _myReference.CheckResolveResult();
            return type == ResolveErrorType.NOT_RESOLVED
                || type == ResolveErrorType.WRONG_NAME_CASE;
        }

        private T ContainingElement<T>() where T : class, ITreeNode
        {
            return _myReferenceExpression != null
                ? _myReferenceExpression.GetContainingNode<T>()
                : null;
        }

        private bool InsideConstantExpression
        {
            get { return ContainingElement<IGotoCaseStatement>() != null; }
        }

        private bool IsUnqualifiedExpression
        {
            get { return (_myReferenceExpression.QualifierExpression == null); }
        }

        private IList<ICSharpExpression> CollectUsages(ITreeNode scope)
        {
            var elementsWithUnresolvedReferences = CollectElementsWithUnresolvedReferences(
                scope,
                _myReference.GetName());

            return FilterUsages(elementsWithUnresolvedReferences);
        }

        private static IEnumerable<IReferenceExpression> CollectElementsWithUnresolvedReferences(ITreeNode scope, string referenceName)
        {
            return ReferencesCollectingUtil
                .CollectElementsWithUnresolvedReference<IReferenceExpression>(
                    scope,
                    referenceName,
                    x => x.Reference);
        }

        private static IList<ICSharpExpression> FilterUsages(IEnumerable<ICSharpExpression> expressions)
        {
            return expressions.Where(x => !IsInvocationExpression(x)).ToList();
        }

        private static bool IsInvocationExpression(ICSharpExpression expression)
        {
            return InvocationExpressionNavigator.GetByInvokedExpression(expression) != null;
        }

        private IDeclarationStatement GetDeclarationStatement(IList<ICSharpExpression> usages, IExpectedTypeConstraint typeConstraint)
        {
            try
            {
                var factory = CSharpElementFactory.GetInstance(_myReferenceExpression.GetPsiModule());
                var statement = CreateStubDeclaration(factory, typeConstraint);
                var insertLocation = CSharpExpressionUtil.GetStatementToBeVisibleFromAll(usages);
                return StatementUtil.InsertStatement(statement, ref insertLocation, true);
            }
            catch (Exception ex)
            {
                File.AppendAllText("c:\\temp\\MillimanPluginErrors.txt", "Exception on " + DateTime.Now + "\n" + ex + "\n\n");
                throw;
            }
        }

        private IDeclarationStatement CreateStubDeclaration(CSharpElementFactory factory, IExpectedTypeConstraint typeConstraint)
        {
            return (IDeclarationStatement)factory.CreateStatement(
                "var $0 = MockRepository.GenerateStub<$1>();",
                _myReference.GetName(),
                GetStubInterfaceName(typeConstraint));
        }

        private string GetStubInterfaceName(IExpectedTypeConstraint typeConstraint)
        {
            var languageType = _anchor.Language;
            var firstInterfaceType = typeConstraint.GetDefaultTypes().First();
            return firstInterfaceType.GetPresentableName(languageType);
        }
    }
}
