namespace PactNet.Matchers
{
    internal abstract class MatcherCheck
    {
        private const string PATH_PREFIX = "$.";
        private string _path;

        public string Path
        {
            get { return _path; }
            protected set { _path = value.StartsWith(PATH_PREFIX) ? value : PATH_PREFIX + value; }
        }
    }
}