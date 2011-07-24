using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.CSharp.Util;
using JetBrains.ReSharper.Psi.CodeStyle;
using JetBrains.ReSharper.Psi.ExpectedTypes;
using JetBrains.ReSharper.Psi.Services;
using JetBrains.ReSharper.Psi.Tree;

namespace WebUIResharper.CreateStubFromUsage
{
    public class AddRhinoStub
    {
        private readonly string _referenceName;
        private readonly IReferenceExpression _referenceExpression;
        private readonly IBlock _anchor;

        public AddRhinoStub(string referenceName, IReferenceExpression referenceExpression, IBlock anchor)
        {
            _referenceName = referenceName;
            _referenceExpression = referenceExpression;
            _anchor = anchor;
        }

        public void Execute()
        {
            var usages = CollectUsages(_anchor);
            var typeConstraint = GuessTypesForUsages(usages);

            var declarationStatement = GetDeclarationStatement(
                usages,
                typeConstraint);

            FormatCode(declarationStatement);
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

        private IList<ICSharpExpression> CollectUsages(ITreeNode scope)
        {
            var elementsWithUnresolvedReferences = CollectElementsWithUnresolvedReferences(
                scope,
                _referenceName);

            return FilterUsages(elementsWithUnresolvedReferences);
        }

        private static IExpectedTypeConstraint GuessTypesForUsages(IEnumerable<ICSharpExpression> usages)
        {
            return ExpectedTypesUtil.GuessTypesIntersection(usages.Cast<IExpression>().ToList());
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
                var factory = CSharpElementFactory.GetInstance(_referenceExpression.GetPsiModule());
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
                _referenceName,
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
