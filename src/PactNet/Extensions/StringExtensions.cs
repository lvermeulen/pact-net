namespace PactNet.Extensions
{
    public static class StringExtensions
    {
        public static string ToLowerSnakeCase(this string input) => !string.IsNullOrEmpty(input) ?
    input.Replace(' ', '_').ToLower() :
    string.Empty;
    }
}