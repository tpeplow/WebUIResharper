using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;

namespace WebUIResharper.ChangeCallToRhino
{
    [ContextAction(Description = "Change this call to Expect(x => <call>)", Name = "ChangeCallToExpect", Priority = 0x7FFE)]
    public class ChangeCallToExpectContextAction : ChangeCallToRhinoCallContextAction
    {
        public ChangeCallToExpectContextAction(ICSharpContextActionDataProvider provider)
            : base(provider)
        {
        }

        protected override string MethodName
        {
            get { return "Expect"; }
        }
    }
}