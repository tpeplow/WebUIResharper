using System;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Intentions;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;
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
            var addStub = new AddRhinoStub(_myReference.GetName(), _myReferenceExpression, _anchor);
            addStub.Execute();

            return tc => { };
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

        private static bool IsInvocationExpression(ICSharpExpression expression)
        {
            return InvocationExpressionNavigator.GetByInvokedExpression(expression) != null;
        }
    }
}
