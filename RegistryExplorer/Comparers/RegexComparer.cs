
namespace RegistryExplorer.Comparers
{

    class RegexComparer: Comparer
    {
        System.Text.RegularExpressions.Regex regEx;

        public RegexComparer(string pattern, bool ignoreCase) : base(pattern, ignoreCase)
        {
            System.Text.RegularExpressions.RegexOptions opts = System.Text.RegularExpressions.RegexOptions.Compiled;
            if (IgnoreCase)
                opts |= System.Text.RegularExpressions.RegexOptions.IgnoreCase;

            try
            {
                regEx = new System.Text.RegularExpressions.Regex(Pattern, opts);
            }
            catch (System.ArgumentException ex)
            {
                throw new System.ArgumentException("Invalid regular expression", ex);
            }
        }

        public override bool IsMatch(string value)
        {
            return regEx.IsMatch(value);
        }

    }

}
