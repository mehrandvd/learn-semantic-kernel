namespace LearnSemanticKernel.Utils;

public class SupportIntentUtil
{
    public static Dictionary<SupportIntent, string> GetIntents() => new()
    {
        [SupportIntent.None] = "Can not specify the intent.",
        [SupportIntent.QuestionAboutProduct] = "User is asking a question about product",
        [SupportIntent.WantToPurchase] = "User needs or wants to purchase something",
        [SupportIntent.AngryWithSomething] = "User is angry with something and need care"
    };

    public static string GetIntentsText() => string.Join(",", GetIntents().Keys.Select(v => v.ToString()));
}