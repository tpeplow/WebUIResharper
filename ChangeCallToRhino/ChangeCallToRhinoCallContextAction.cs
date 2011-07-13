using System;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace WebUIResharper.ChangeCallToRhino
{
    public abstract class ChangeCallToRhinoCallContextAction : BulbItemImpl, IContextAction
    {
        private readonly ICSharpContextActionDataProvider _provider;

        protected ChangeCallToRhinoCallContextAction(ICSharpContextActionDataProvider provider)
        {
            _provider = provider;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var invocation = _provider.GetSelectedElement<IInvocationExpression>(false, false);
            if (invocation == null || invocation.Reference == null)
                return null;

            var referenceExpression = invocation.InvokedExpression as IReferenceExpression;
            if (referenceExpression == null)
                return null;

            var textRange = new TextRange(referenceExpression.Reference.GetTreeTextRange().StartOffset.Offset,
                                          invocation.RPar.GetTreeStartOffset().Offset + 1);
            var argumentsText = _provider.Document.GetText(textRange);

            _provider.Document.ReplaceText(textRange, MethodName + "(x => x." + argumentsText + ")");
            return null;
        }

        protected abstract string MethodName { get; }

        public override string Text
        {
            get { return "Change this call to " + MethodName + "(x => <call>)"; }
        }

        public bool IsAvailable(IUserDataHolder cache)
        {
            var invocation = _provider.GetSelectedElement<IInvocationExpression>(false, false);
            if (invocation == null || invocation.Reference == null)
                return false;

            var referenceExpression = invocation.InvokedExpression as IReferenceExpression;
            if (referenceExpression == null)
                return false;

            switch (referenceExpression.Reference.GetName())
            {
                case "Stub":
                case "Expect":
                case "AssertWasCalled":
                case "AssertWasNotCalled":
                    return false;
            }

            var qualifierExpression = referenceExpression.QualifierExpression as IReferenceExpression;
            if (qualifierExpression == null)
                return false;

            var qualifierType = qualifierExpression.Type();
            if (!qualifierType.GetPresentableName(CSharpLanguage.Instance).StartsWith("I"))
                return false;

            return true;
        }
    }
}