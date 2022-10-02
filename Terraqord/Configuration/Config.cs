using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Terraqord.Configuration
{
    public static class Config
    {
        public const string FilePath = "Terraqord.json";

        public static JsonSerializerOptions JsonConfig { get; } = new()
        {
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
        };

        public static Settings Settings { get; } = Settings.Read();
    }
}
