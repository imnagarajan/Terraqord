using System.Text;
using System.Text.RegularExpressions;
using Terraria;
using TShockAPI;

namespace Terraqord.Extensions
{
    public static class StringUtilities
    {
        private class Tag
        {
            // Source: Terraria
            public static Regex Regex = new("(?<!\\\\)\\[(?<tag>[a-zA-Z]{1,10})(\\/(?<options>[^:]+))?:(?<text>.*?)(?<!\\\\)\\]", RegexOptions.Compiled);

            public enum TagType
            {
                None,
                Color,
                Item,
                Name,
                Achievement,
                Glyph
            }

            private string Raw { get; }

            public TagType Type { get; }

            public string Options { get; }

            public string Text { get; }

            public Tag(Match match)
                : this(match.Groups["tag"].Value, match.Groups["options"].Value, match.Groups["text"].Value)
            {
                Raw = match.Value;
            }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            private Tag(string type, string options, string text)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            {
                Type = type switch
                {
                    "c" or "color" => TagType.Color,
                    "i" or "item" => TagType.Item,
                    "n" or "name" => TagType.Name,
                    "a" or "achievement" => TagType.Achievement,
                    "g" or "glyph" => TagType.Glyph,
                    _ => TagType.None,
                };

                Options = options;
                Text = text;
            }

            public string Parse(bool quotes = false)
            {
                switch (Type)
                {
                    case TagType.None:
                    case TagType.Color:
                    case TagType.Name:
                    // Sadly there is no way left in TSAPI for getting an achievement name, so we have to keep it as the tag text
                    case TagType.Achievement:
                    case TagType.Glyph:
                    default:
                        return Text;

                    case TagType.Item:
                        Item item = TShock.Utils.GetItemFromTag(Raw);
                        if (item != null)
                        {
                            string stack = item.stack > 1 ? $"{item.stack} " : "";
                            if (quotes)
                                return $"` -{stack}{item.AffixName()}- `";
                            else
                                return $"{stack}{item.AffixName()}, ";
                        }
                        else
                            return Text;
                }
            }

            public override string ToString()
                => Raw;
        }

        public static string StripTags(this string input, bool quoteResult = false)
        {
            MatchCollection matches = Tag.Regex.Matches(input);

            foreach (Match match in matches)
            {
                Tag tag = new(match);

                input = input.Replace(match.Value, tag.Parse(quoteResult));
            }

            input = input.Replace("@", "");

            return input;
        }

        public static List<string> ParseParameters(this string input)
        {
            var ret = new List<string>();
            var sb = new StringBuilder();
            var instr = false;
            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];

                if (c == '\\' && ++i < input.Length)
                {
                    if (input[i] != '"' && input[i] != ' ' && input[i] != '\\')
                        sb.Append('\\');
                    sb.Append(input[i]);
                }
                else if (c == '"')
                {
                    instr = !instr;
                    if (!instr)
                    {
                        ret.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (sb.Length > 0)
                    {
                        ret.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else if (char.IsWhiteSpace(c) && !instr)
                {
                    if (sb.Length > 0)
                    {
                        ret.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            if (sb.Length > 0)
                ret.Add(sb.ToString());

            return ret;
        }

        public static string ToReadable(this TimeSpan span)
        {
            StringBuilder sb = new();

            if (span.Days > 0)
            {
                sb.Append($"{span.Days} day{(span.Days > 1 ? "s" : "")}");
                if (span.Hours > 0 && (span.Minutes > 0 || span.Seconds > 0))
                    sb.Append(", ");
                else if (span.Hours > 0)
                    sb.Append(" and ");
                else return sb.ToString();
            }
            if (span.Hours > 0)
            {
                sb.Append($"{span.Hours} hour{(span.Hours > 1 ? "s" : "")}");
                if (span.Minutes > 0 && span.Seconds > 0)
                    sb.Append(", ");
                else if (span.Minutes > 0)
                    sb.Append(" and ");
                else return sb.ToString();
            }
            if (span.Minutes > 0)
            {
                sb.Append($"{span.Minutes} minute{(span.Minutes > 1 ? "s" : "")}");
                if (span.Seconds > 0)
                    sb.Append(" and ");
                else return sb.ToString();
            }
            if (span.Seconds > 0)
                sb.Append($"{span.Seconds} second{(span.Seconds > 1 ? "s" : "")}");
            return sb.ToString();
        }
    }
}
