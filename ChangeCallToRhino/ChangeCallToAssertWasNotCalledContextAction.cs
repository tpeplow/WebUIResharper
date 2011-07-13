using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;

namespace WebUIResharper.ChangeCallToRhino
{
    [ContextAction(Description = "Change this call to AssertWasNotCalled(x => <call>)", Name = "ChangeCallToAssertWasNotCalled", Priority = 0x7FFC)]
    public class ChangeCallToAssertWasNotCalledContextAction : ChangeCallToRhinoCallContextAction
    {
        public ChangeCallToAssertWasNotCalledContextAction(ICSharpContextActionDataProvider provider)
            : base(provider)
        {
        }

        protected override string MethodName
        {
            get { return "AssertWasNotCalled"; }
        }
    }
}
