using RealmStudioShapeRenderingLib;
using SkiaSharp;

namespace RealmStudioX.Infrastructure
{
    public static class MapProjectHandler
    {
        /*
            --- project package structure ---

            MyProject.rsmpkz
            │
            ├── project.xml
            │
            ├── Maps/
            │   ├── World
            │   ├──── World.rsmx
            │   ├──── World.metadata.xml
            │   ├──── World.png
            │   ├── Region1
            │   ├──── Region1.rsmx
            │   ├──── Region1.metadata.xml
            │   ├──── Region1.png
            │   ├── DungeonLevel1
            │   ├──── DungeonLevel1.rsmx
            │   ├──── DungeonLevel1.metadata.xml
            │   ├──── DungeonLevel1.png
   
        */

        public static MapProjectEntry CreateProjectEntry(RealmStudioMap map, SKBitmap? preview)
        {
            MapMetadata mapMeta = new()
            {
                Name = map.MapName,
                Description = map.RealmDescription,
                RealmType = map.RealmType,
                MapWidth = map.MapWidth,
                MapHeight = map.MapHeight,
                AreaWidth = map.MapAreaWidth,
                AreaHeight = map.MapAreaHeight,
                AreaUnits = map.MapAreaUnits,
                Theme = map.MapTheme,
                Created = DateTime.Now,
            };

            MapProjectEntry mapEntry = new()
            {
                MapId = map.MapId,
                Map = map,
                Metadata = mapMeta,
                Preview = preview != null ? preview.Copy() : new SKBitmap()
            };

            return mapEntry;
        }
    }

    public sealed class ProjectManifest
    {
        public int FormatVersion { get; set; }

        public string ActiveMapId { get; set; } = "";

        public MapProjectMetadata Metadata { get; set; } = new();

        public List<ProjectMapManifest> Maps { get; set; } = [];
    }

    public sealed class ProjectMapManifest
    {
        public string MapId { get; set; } = string.Empty;

        public string MapName { get; set; } = string.Empty;

        public string MapFile { get; set; } = string.Empty;

        public string MetadataFile { get; set; } = string.Empty;

        public string PreviewFile { get; set; } = string.Empty;
    }
}
