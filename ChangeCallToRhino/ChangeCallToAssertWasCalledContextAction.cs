using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;

namespace WebUIResharper.ChangeCallToRhino
{
    [ContextAction(Description = "Change this call to AssertWasCalled(x => <call>)", Name = "ChangeCallToAssertWasCalled", Priority = 0x7FFD)]
    public class ChangeCallToAssertWasCalledContextAction : ChangeCallToRhinoCallContextAction
    {
        public ChangeCallToAssertWasCalledContextAction(ICSharpContextActionDataProvider provider)
            : base(provider)
        {
        }

        protected override string MethodName
        {
            get { return "AssertWasCalled"; }
        }
    }
}