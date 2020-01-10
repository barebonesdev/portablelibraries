using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ToolsPortable.Helpers
{
    public static class LinkDetectionHelper
    {
        // https://gist.github.com/gruber/8891611

        private const string topLevelDomain = "(?:com|net|org|edu|gov|mil|aero|asia|biz|cat|coop|info|int|jobs|mobi|museum|name|post|pro|tel|travel|xxx|ac|ad|ae|af|ag|ai|al|am|an|ao|aq|ar|as|at|au|aw|ax|az|ba|bb|bd|be|bf|bg|bh|bi|bj|bm|bn|bo|br|bs|bt|bv|bw|by|bz|ca|cc|cd|cf|cg|ch|ci|ck|cl|cm|cn|co|cr|cs|cu|cv|cx|cy|cz|dd|de|dj|dk|dm|do|dz|ec|ee|eg|eh|er|es|et|eu|fi|fj|fk|fm|fo|fr|ga|gb|gd|ge|gf|gg|gh|gi|gl|gm|gn|gp|gq|gr|gs|gt|gu|gw|gy|hk|hm|hn|hr|ht|hu|id|ie|il|im|in|io|iq|ir|is|it|je|jm|jo|jp|ke|kg|kh|ki|km|kn|kp|kr|kw|ky|kz|la|lb|lc|li|lk|lr|ls|lt|lu|lv|ly|ma|mc|md|me|mg|mh|mk|ml|mm|mn|mo|mp|mq|mr|ms|mt|mu|mv|mw|mx|my|mz|na|nc|ne|nf|ng|ni|nl|no|np|nr|nu|nz|om|pa|pe|pf|pg|ph|pk|pl|pm|pn|pr|ps|pt|pw|py|qa|re|ro|rs|ru|rw|sa|sb|sc|sd|se|sg|sh|si|sj|Ja|sk|sl|sm|sn|so|sr|ss|st|su|sv|sx|sy|sz|tc|td|tf|tg|th|tj|tk|tl|tm|tn|to|tp|tr|tt|tv|tw|tz|ua|ug|uk|us|uy|uz|va|vc|ve|vg|vi|vn|vu|wf|ws|ye|yt|yu|za|zm|zw)";

        /// <summary>
        /// google.com, amazon.co.uk
        /// </summary>
        private static readonly string domain = $@"(?:[a-z0-9.\-]+[.]{topLevelDomain})";

        private static readonly string urlRegex = $@"(?:(?:https?://)?{domain})(?:(?:[^\s()<>{{}}\[\]]+|\([^\s()]*?\([^\s()]+\)[^\s()]*?\)|\([^\s]+?\))+(?:\([^\s()]*?\([^\s()]+\)[^\s()]*?\)|\([^\s]+?\)|[^\s`!()\[\]{{}};:'"".,<>?«»“”‘’]))?/?";

        // http://emailregex.com/
        private static readonly string emailRegex = $@"(?:[a-z0-9!#$%&'*+/=?^_`{{|}}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{{|}}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f] |\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@{domain}";

        private static readonly string finalRegex = $"({emailRegex})|({urlRegex})";
        private static Regex regex = new Regex(finalRegex);

        public static IEnumerable<PortableRun> DetectRuns(string text)
        {
            int index = 0;

            while (index < text.Length)
            {
                var match = regex.Match(text, index);

                if (match.Success)
                {
                    if (match.Index > index)
                    {
                        yield return new PortableRun()
                        {
                            Text = text.Substring(index, match.Index - index)
                        };
                    }

                    // If email address
                    Uri uri = null;

                    // Wrap in try/catch in case any of the uris ended up being invalid
                    try
                    {
                        // If email
                        if (match.Groups[1].Success)
                        {
                            uri = new Uri("mailto:" + match.Value);
                        }

                        // If url
                        else
                        {
                            if (match.Value.StartsWith("http://") || match.Value.StartsWith("https://"))
                            {
                                uri = new Uri(match.Value);
                            }
                            else
                            {
                                uri = new Uri("http://" + match.Value);
                            }
                        }
                    }
                    catch
                    {

                    }

                    if (uri != null)
                    {
                        yield return new PortableHyperlinkRun()
                        {
                            Text = match.Value,
                            Uri = uri
                        };
                    }
                    else
                    {
                        yield return new PortableRun()
                        {
                            Text = match.Value
                        };
                    }

                    index = match.Index + match.Length;
                }
                else
                {
                    yield return new PortableRun()
                    {
                        Text = text.Substring(index)
                    };

                    // Reached end of string
                    break;
                }
            }
        }
    }

    public class PortableRun
    {
        public string Text { get; set; }
    }

    public class PortableHyperlinkRun : PortableRun
    {
        public Uri Uri { get; set; }
    }
}
