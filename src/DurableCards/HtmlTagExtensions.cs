using AdaptiveCards.Rendering.Html;

namespace DurableCards
{
    public static class HtmlTagExtensions
    {
        public static HtmlTag Attr(this HtmlTag htmlTag, string name, bool enabled)
        {
            if (enabled)
            {
                htmlTag.Attr(name, name);
            }

            return htmlTag;
        }

        public static HtmlTag AddOptionalClass(this HtmlTag htmlTag, string className)
        {
            if (string.IsNullOrEmpty(className))
            {
                return htmlTag;
            }

            return htmlTag.AddClass(className);
        }
    }
}
