using RealmStudioShapeRenderingLib;
using System.Xml.Serialization;

namespace RealmStudioX.Infrastructure
{
    [XmlRoot("MapSymbolCollection", Namespace = "RealmStudio")]
    public class LegacyRealmStudioCollection
    {
        public string CollectionName { get; set; } = string.Empty;
        public string CollectionGuid { get; set; } = string.Empty;

        public LegacySymbolContainer CollectionMapSymbols { get; set; } = new();
    }

    public class LegacySymbolContainer
    {
        [XmlElement("MapSymbol")]
        public List<LegacyMapSymbol> Symbols { get; set; } = [];
    }

    public class LegacyMapSymbol
    {
        // Attributes on <MapSymbol>
        [XmlAttribute] public float X { get; set; }
        [XmlAttribute] public float Y { get; set; }
        [XmlAttribute] public float Width { get; set; }
        [XmlAttribute] public float Height { get; set; }

        public string SymbolGuid { get; set; } = string.Empty;

        public string SymbolName { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string SymbolFilePath { get; set; } = string.Empty;

        public SymbolFileFormat SymbolFormat { get; set; }

        public MapSymbolType SymbolType { get; set; }

        public bool IsGrayscale { get; set; }

        public bool UseCustomColors { get; set; }

        public LegacyTagContainer? SymbolTags { get; set; }
    }

    public class LegacyTagContainer
    {
        [XmlElement("Tag")]
        public List<string> Tags { get; set; } = [];
    }
}
