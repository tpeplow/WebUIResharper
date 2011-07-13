using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;

namespace WebUIResharper.ChangeCallToRhino
{
    [ContextAction(Description = "Change this call to Stub(x => <call>)", Name = "ChangeCallToStub", Priority = 0x7FFE)]
    public class ChangeCallToStubContextAction : ChangeCallToRhinoCallContextAction
    {
        public ChangeCallToStubContextAction(ICSharpContextActionDataProvider provider)
            : base(provider)
        {
        }

        protected override string MethodName
        {
            get { return "Stub"; }
        }
    }
}