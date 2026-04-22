namespace RealmStudioX.Infrastructure
{
    using RealmStudioShapeRenderingLib;
    using System.IO;
    using System.Xml.Serialization;

    internal class LegacyCollectionConverter
    {
        public static MapSymbolCollection Convert(string path)
        {
            var serializer = new XmlSerializer(typeof(LegacyRealmStudioCollection));

            LegacyRealmStudioCollection old;

            var settings = new System.Xml.XmlReaderSettings
            {
                DtdProcessing = System.Xml.DtdProcessing.Prohibit,
                XmlResolver = null
            };

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = System.Xml.XmlReader.Create(fs, settings))
            {
                old = (LegacyRealmStudioCollection)serializer.Deserialize(reader)!;
            }

            var baseDir = Path.GetDirectoryName(path)!;

            var newCollection = new MapSymbolCollection
            {
                Id = old.CollectionGuid,
                Name = old.CollectionName,
                Symbols = []
            };

            foreach (var s in old.CollectionMapSymbols.Symbols)
            {
                var def = new MapSymbolDefinition
                {
                    Id = s.SymbolGuid,

                    // preserve system name
                    SymbolName = s.SymbolName,

                    SymbolFilePath = NormalizePath(s.SymbolFilePath),

                    SymbolFormat = s.SymbolFormat,
                    SymbolType = s.SymbolType,

                    BaseColorType = MapBaseColorType(s),

                    SymbolTags = MergeTags(
                        s.SymbolTags?.Tags,
                        ExtractTagsFromName(s.SymbolName)),

                    BoundsMetadata = new SymbolBoundsMetadata()
                    {
                        X = -s.Width / 2,
                        Y = -s.Height / 2,
                        Width = s.Width,
                        Height = s.Height,
                    }
                };

                def.FinalizeAfterLoad(newCollection.Id, newCollection.Name, baseDir);

                newCollection.Symbols.Add(def);
            }

            return newCollection;
        }

        // -------------------------------------------------
        // Helpers
        // -------------------------------------------------

        private static MapSymbolBaseColorType MapBaseColorType(LegacyMapSymbol s)
        {
            if (s.IsGrayscale)
                return MapSymbolBaseColorType.GrayScale;

            if (s.UseCustomColors)
                return MapSymbolBaseColorType.RGBMask;

            return MapSymbolBaseColorType.FullColor;
        }

        private static List<string> MergeTags(
            IEnumerable<string>? explicitTags,
            IEnumerable<string> autoTags)
        {
            return (explicitTags ?? Enumerable.Empty<string>())
                .Concat(autoTags)
                .Select(t => t.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();
        }

        private static IEnumerable<string> ExtractTagsFromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Enumerable.Empty<string>();

            return name
                .ToLowerInvariant()
                .Split([' ', '_', '-', '(', ')'],
                    StringSplitOptions.RemoveEmptyEntries);
        }

        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(path)
                .Replace('\\', '/')
                .ToLowerInvariant();
        }
    }
}
