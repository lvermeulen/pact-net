namespace PactNet.Comparers
{
    public class ErrorMessageComparisonFailure : ComparisonFailure
    {
        public ErrorMessageComparisonFailure(string errorMessage)
        {
            Result = errorMessage;
        }
    }
}