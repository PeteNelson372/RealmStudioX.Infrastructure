/**************************************************************************************************************************
* Copyright 2024, Peter R. Nelson
*
* This file is part of the RealmStudio application. The RealmStudio application is intended
* for creating fantasy maps for gaming and world building.
*
* RealmStudio is free software: you can redistribute it and/or modify it under the terms
* of the GNU General Public License as published by the Free Software Foundation,
* either version 3 of the License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
* without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
* See the GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License along with this program.
* The text of the GNU General Public License (GPL) is found in the LICENSE.txt file.
* If the LICENSE.txt file is not present or the text of the GNU GPL is not present in the LICENSE.txt file,
* see https://www.gnu.org/licenses/.
*
* For questions about the RealmStudio application or about licensing, please email
* support@brookmonte.com
*
***************************************************************************************************************************/
using RealmStudioShapeRenderingLib;
using RealmStudioX.Core;
using SkiaSharp;

namespace RealmStudioX.Infrastructure
{
    public sealed class AssetManager : IAssetProvider
    {
        private readonly List<AssetDescriptor> _descriptors = [];
        private readonly Dictionary<string, SKImage> _imageCache = [];

        private readonly Dictionary<AssetType, List<AssetDescriptor>> _byType = [];

        private static readonly Dictionary<string, AssetType> _folderTypeMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["boxes"] = AssetType.Box,
                ["brushes"] = AssetType.Brush,
                ["frames"] = AssetType.Frame,
                ["icons"] = AssetType.Icon,
                ["labelpresets"] = AssetType.LabelPreset,
                ["namegenerators"] = AssetType.NameGenerator,
                ["shapingfunctions"] = AssetType.ShapingFunction,
                ["stamps"] = AssetType.Stamp,
                ["symbols"] = AssetType.Symbol,
                ["background"] = AssetType.BackgroundTexture,
                ["clouds"] = AssetType.CloudTexture,
                ["floor"] = AssetType.FloorTexture,
                ["hatch"] = AssetType.HatchTexture,
                ["land"] = AssetType.LandTexture,
                ["path"] = AssetType.PathTexture,
                ["planet"] = AssetType.PlanetTexture,
                ["starfield"] = AssetType.StarfieldTexture,
                ["star"] = AssetType.StarTexture,
                ["wall"] = AssetType.WallTexture,
                ["water"] = AssetType.WaterTexture,
                ["themes"] = AssetType.Theme,
                ["vectors"] = AssetType.Vector,
            };

        public IReadOnlyList<AssetDescriptor> Assets => _descriptors;
        public int AssetCount => _descriptors.Count;

        private readonly List<MapSymbolCollection> _mapSymbolCollections = [];
        public List<MapSymbolCollection> SymbolCollections => _mapSymbolCollections;

        private readonly SymbolIndex _symbolIndex = new();

        private readonly List<MapSymbolDefinition> _symbolDefinitions = [];
        private readonly Dictionary<string, MapSymbolDefinition> _symbolByPath = [];

        private readonly SymbolImageCache _symbolImageCache = new();
        public SymbolImageCache SymbolImageCache => _symbolImageCache;

        private readonly SymbolThumbnailCache _symbolThumbnailCache;
        public SymbolThumbnailCache SymbolThumbnailCache => _symbolThumbnailCache;

        private readonly List<MapFrame> _mapFrames = [];
        public List<MapFrame> MapFrames => _mapFrames;

        private readonly List<MapBrush> _mapBrushes = [];
        public List<MapBrush> MapBrushes => _mapBrushes;

        public static readonly string DefaultThemeName = "Medieval Quest";

        // Backing lists (private, mutable)
        private static readonly List<string> s_originalSymbolTags = [];
        private static HashSet<string> s_symbolTags = [];
        private static List<string> s_structureSynonyms = [];
        private static List<string> s_terrainSynonyms = [];
        private static List<string> s_vegetationSynonyms = [];

        // Expose read-only views to avoid public mutable static fields (fix CA2211)
        public static IReadOnlyList<string> OriginalSymbolTags => s_originalSymbolTags;
        public static IReadOnlyList<string> SymbolTags => [.. s_symbolTags];
        public static IReadOnlyList<string> StructureSynonyms => s_structureSynonyms;
        public static IReadOnlyList<string> TerrainSynonyms => s_terrainSynonyms;
        public static IReadOnlyList<string> VegetationSynonyms => s_vegetationSynonyms;

        public static MapTheme? CurrentTheme { get; set; }

        public static List<NameGenerator> NameGenerators { get; } = [];
        public static List<NameBase> NameBases { get; } = [];
        public static List<NameBaseLanguage> NameLanguages { get; } = [];


        // Replace mutable public fields with properties
        public static string RootRealmStudioXDirectory { get; set; } = string.Empty;
        public static string RootRealmsDirectory { get; set; } = string.Empty;
        public static string RootAssetDirectory { get; set; } = string.Empty;

        private static string _defaultModelsDirectory = string.Empty;
        private static string _defaultSymbolDirectory = string.Empty;
        private static string _symbolTagsFilePath = string.Empty;

        private static string _structureSynonymsFilePath = string.Empty;
        private static string _terrainSynonymsFilePath = string.Empty;
        private static string _vegetationSynonymsFilePath = string.Empty;


        public static readonly string CollectionFileName = "collectionx.xml";
        public static readonly string LegacyCollectionFileName = "collection.xml";

        public static readonly string WonderdraftSymbolsFileName = ".wonderdraft_symbols";

        public AssetManager()
        {
            _symbolThumbnailCache = new SymbolThumbnailCache(_symbolImageCache);
        }

        // -------------------------------------------------
        // Async Loading with Progress
        // -------------------------------------------------

        public async Task LoadAsync()
        {
            if (string.IsNullOrEmpty(RootRealmStudioXDirectory) || !Directory.Exists(RootRealmStudioXDirectory))
            {
                throw new InvalidOperationException($"Root RealmStudioX directory '{RootRealmStudioXDirectory}' does not exist.");
            }

            RootRealmsDirectory = Path.Combine(RootRealmStudioXDirectory, "Realms");
            RootAssetDirectory = Path.Combine(RootRealmStudioXDirectory, "Assets");

            _defaultModelsDirectory = Path.Combine(RootRealmsDirectory, "Models");

            _defaultSymbolDirectory = Path.Combine(RootAssetDirectory, "Symbols");

            _symbolTagsFilePath = Path.Combine(_defaultSymbolDirectory, "SymbolTags.txt");
            _structureSynonymsFilePath = Path.Combine(_defaultSymbolDirectory, "StructureSynonyms.txt");
            _terrainSynonymsFilePath = Path.Combine(_defaultSymbolDirectory, "TerrainSynonyms.txt");
            _vegetationSynonymsFilePath = Path.Combine(_defaultSymbolDirectory, "VegetationSynonyms.txt");

            // -------------------------------------------------
            // Load global tag/synonym files
            // -------------------------------------------------

            s_symbolTags = [.. File.ReadAllLines(_symbolTagsFilePath)];
            s_structureSynonyms = [.. File.ReadAllLines(_structureSynonymsFilePath)];
            s_terrainSynonyms = [.. File.ReadAllLines(_terrainSynonymsFilePath)];
            s_vegetationSynonyms = [.. File.ReadAllLines(_vegetationSynonymsFilePath)];

            _descriptors.Clear();
            _symbolDefinitions.Clear();
            _symbolByPath.Clear();

            // -------------------------------------------------
            // Phase 1: Load symbol collections FIRST
            // -------------------------------------------------

            var directories = Directory
                .EnumerateDirectories(RootAssetDirectory, "*", SearchOption.AllDirectories)
                .Prepend(RootAssetDirectory); // include root itself

            foreach (var dir in directories)
            {
                var newPath = Path.Combine(dir, CollectionFileName);
                var oldPath = Path.Combine(dir, LegacyCollectionFileName);

                string? selectedPath = null;

                if (File.Exists(newPath))
                {
                    selectedPath = newPath;
                }
                else if (File.Exists(oldPath))
                {
                    selectedPath = oldPath;
                }

                if (selectedPath == null)
                {
                    continue;
                }

                var collection = LoadSymbolCollection(selectedPath);

                if (collection == null)
                {
                    continue;
                }

                SymbolCollections.Add(collection);

                foreach (var def in collection.Symbols)
                {
                    _symbolDefinitions.Add(def);
                    _symbolIndex.Add(def);

                    var normalized = Utilities.NormalizePath(def.SymbolFilePath);
                    _symbolByPath[normalized] = def;
                    SymbolThumbnailCache.GetOrCreate(def, 52);
                }
            }

            // -------------------------------------------------
            // Phase 2: Scan all files and build descriptors
            // -------------------------------------------------

            var files = Directory
                .EnumerateFiles(RootAssetDirectory, "*.*", SearchOption.AllDirectories)
                .ToList();

            foreach (var file in files)
            {
                // load name generators and namebases
                if (file.Contains("NameGenerators"))
                {
                    var extension = Path.GetExtension(file).ToLowerInvariant();
                    string path = Path.GetFullPath(file);
                    
                    // name generator or namebase files
                    if (extension == ".csv")
                    {                        
                        LoadNameGeneratorFile(path);
                    }

                    if (extension == ".txt")
                    {
                        LoadNameBaseFile(path);
                    }
                }
                else
                {
                    // load descriptors
                    var descriptor = CreateDescriptor(file);

                    if (descriptor != null)
                    {
                        AddDescriptor(descriptor);
                    }
                }
            }

            // -------------------------------------------------
            // Phase 3: Load Map Frames
            // -------------------------------------------------

            IReadOnlyList<AssetDescriptor> frames = GetByType(AssetType.Frame);

            foreach (AssetDescriptor frameDescriptor in frames)
            {
                if (frameDescriptor.FileExtension.Equals(".xml", StringComparison.CurrentCultureIgnoreCase))
                {
                    MapFrame? frame = MapFileMethods.ReadFrameAssetFromXml(frameDescriptor.FilePath);

                    if (frame != null)
                    {
                        _mapFrames.Add(frame);
                    }
                }
            }

            // -------------------------------------------------
            // Phase 4: Load Brushes
            // -------------------------------------------------

            AssetBrowser BrushBrowser = new(this, AssetType.Brush);

            IReadOnlyList<AssetDescriptor> brushes = BrushBrowser.Assets;

            foreach (AssetDescriptor brush in brushes)
            {
                // Resolve XML path relative from descriptor

                string filename = Path.GetFileName(brush.FilePath);

                if (filename.EndsWith(".brush.xml", StringComparison.OrdinalIgnoreCase))
                {
                    MapBrush? mapBrush = MapFileMethods.LoadBrush(brush.FilePath);

                    if (mapBrush != null)
                    {
                        MapBrushes.Add(mapBrush);
                    }
                }
            }
        }

        private static MapSymbolCollection? LoadSymbolCollection(string xmlPath)
        {
            var collection = MapFileMethods.ReadCollection(xmlPath);

            if (collection == null)
            {
                return null;
            }

            var baseDir = Path.GetDirectoryName(xmlPath)!;

            foreach (var symbolDef in collection.Symbols)
            {
                // Resolve path
                var fullPath = Path.GetFullPath(Path.Combine(baseDir, symbolDef.SymbolFilePath));
                symbolDef.SymbolFilePath = Utilities.NormalizePath(fullPath);

                // Post-load fixup
                symbolDef.FinalizeAfterLoad(collection.Id, collection.Name, baseDir);

                // Infer format if missing
                if (symbolDef.SymbolFormat == SymbolFileFormat.NotSet)
                {
                    symbolDef.SymbolFormat = Utilities.InferFileFormat(symbolDef.SymbolFilePath);
                }

                ValidateSymbol(symbolDef);
            }

            return collection;
        }

        private static void ValidateSymbol(MapSymbolDefinition def)
        {
            if (def.SymbolFormat == SymbolFileFormat.JPG &&
                def.BaseColorType == MapSymbolBaseColorType.RGBMask)
            {
                throw new InvalidOperationException(
                    $"Symbol '{def.SymbolName}' uses RGBMask but JPG does not support reliable color masking.");
            }

            if (def.BaseColorType == MapSymbolBaseColorType.NotSet)
            {
                throw new InvalidOperationException(
                    $"Symbol '{def.SymbolName}' is missing BaseColorType.");
            }

            if (def.SymbolType == MapSymbolType.NotSet)
            {
                // Optional: allow but warn
                // throw or log depending on strictness
            }
        }

        private void AddDescriptor(AssetDescriptor descriptor)
        {
            _descriptors.Add(descriptor);

            if (!_byType.TryGetValue(descriptor.Type, out var list))
            {
                list = [];
                _byType[descriptor.Type] = list;
            }

            list.Add(descriptor);
        }

        private AssetDescriptor? CreateDescriptor(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var relativePath = Path.GetRelativePath(RootAssetDirectory, filePath);

            var assetType = ResolveAssetType(relativePath);
            if (assetType == AssetType.None)
                return null;

            var name = Path.GetFileNameWithoutExtension(filePath);

            object? metadata = null;
            string? metadataPath = null;

            // -------------------------------------------------
            // XML metadata files
            // -------------------------------------------------

            if (extension == ".xml")
            {
                return new AssetDescriptor(
                    id: GenerateId(relativePath),
                    name: name,
                    type: assetType,
                    symbolType: MapSymbolType.NotSet,
                    filePath: filePath,
                    metadataPath: metadataPath,
                    metadata: metadata,
                    collection: ExtractCollection(relativePath),
                    tags: ExtractTags(relativePath));
            }

            if (extension == RealmStudioFileFormat.RealmStudioLabelPresetExtension)
            {
                return new AssetDescriptor(
                    id: GenerateId(relativePath),
                    name: name,
                    type: assetType,
                    symbolType: MapSymbolType.NotSet,
                    filePath: filePath,
                    metadataPath: metadataPath,
                    metadata: metadata,
                    collection: null,
                    tags: null);
            }

            // -------------------------------------------------
            // Image / vector assets
            // -------------------------------------------------

            if (IsImageFile(extension) || IsVectorFile(extension))
            {
                _symbolByPath.TryGetValue(Utilities.NormalizePath(filePath), out var symbolDef);

                return new AssetDescriptor(
                    id: GenerateId(relativePath),
                    name: name,
                    type: assetType,
                    symbolType: symbolDef?.SymbolType ?? MapSymbolType.NotSet,

                    filePath: filePath,
                    metadataPath: null,
                    metadata: symbolDef,

                    collection: symbolDef?.CollectionId,
                    tags: symbolDef?.SymbolTags ?? []);
            }

            return null;
        }

        private static void LoadNameBaseFile(string path)
        {
            IEnumerable<string> lines = File.ReadLines(path);

            if (lines.Any())
            {
                NameBase nameBase = new()
                {
                    NameBaseName = Path.GetFileNameWithoutExtension(path)
                };

                foreach (var line in lines)
                {
                    string[] lineParts = line.Split('|');

                    if (lineParts.Length == 6)
                    {
                        NameBaseLanguage language = new()
                        {
                            Language = lineParts[0].Trim(),
                            IsLanguageSelected = true,
                            MinNameLength = int.Parse(lineParts[1]),
                            MaxNameLength = int.Parse(lineParts[2])
                        };

                        foreach (char c in lineParts[3])
                        {
                            language.RepeatableCharacters.Add(c);
                        }

                        language.SingleWordTransformProportion = float.Parse(lineParts[4]);

                        string[] nameBaseNames = lineParts[5].Split(",");

                        for (int i = 0; i < nameBaseNames.Length; i++)
                        {
                            nameBaseNames[i] = nameBaseNames[i].Trim();
                        }

                        language.NameStrings.AddRange(nameBaseNames);

                        if (!string.IsNullOrEmpty(language.Language) && language.NameStrings.Count > 0)
                        {
                            nameBase.Languages.Add(language);
                        }

                        if (!string.IsNullOrEmpty(language.Language))
                        {
                            if (!NameLanguages.Any(l =>
                                    string.Equals(
                                        l.Language,
                                        language.Language,
                                        StringComparison.OrdinalIgnoreCase)))
                            {
                                int index = NameLanguages.FindIndex(l =>
                                    string.Compare(
                                        language.Language,
                                        l.Language,
                                        StringComparison.OrdinalIgnoreCase) < 0);

                                if (index >= 0)
                                {
                                    NameLanguages.Insert(index, language);
                                }
                                else
                                {
                                    NameLanguages.Add(language);
                                }
                            }
                        }
                    }
                }

                if (nameBase.Languages.Count > 0)
                {
                    NameBases.Add(nameBase);
                }
            }
        }

        private static void LoadNameGeneratorFile(string path)
        {
            IEnumerable<string> lines = File.ReadLines(path);

            if (lines.Any())
            {
                NameGenerator generator = new()
                {
                    NameGeneratorName = Path.GetFileNameWithoutExtension(path)
                };

                foreach (var line in lines)
                {
                    string[] lineParts = line.Split(',');

                    if (!string.IsNullOrEmpty(lineParts[0]))
                    {
                        generator.Column1.Add(lineParts[0].Trim());
                    }

                    if (lineParts.Length > 1)
                    {
                        if (!string.IsNullOrEmpty(lineParts[1]))
                        {
                            generator.Column2.Add(lineParts[1].Trim());
                        }
                    }
                }

                NameGenerators.Add(generator);
            }
        }

        public static List<INameGenerator> GetAllNameGenerators()
        {
            List<INameGenerator> nameGenList = [];

            nameGenList.AddRange(NameGenerators);
            nameGenList.AddRange(NameBases);
            
            return nameGenList;
        }

        public List<MapSymbolDefinition> QuerySymbols(SymbolQuery query)
        {
            return _symbolIndex.Query(
                query.Type,
                query.Collections,
                query.Tags,
                query.TextFilter);
        }


        public IReadOnlyList<AssetDescriptor> GetByType(AssetType type)
        {
            if (_byType.TryGetValue(type, out var list))
                return list;

            return [];
        }

        public IReadOnlyList<AssetDescriptor> GetByTypeAndTag(AssetType type, string tag)
        {
            if (!_byType.TryGetValue(type, out var list))
                return [];

            return [.. list.Where(d => d.Tags.Contains(tag))];
        }

        public IReadOnlyList<AssetDescriptor> GetByName(AssetType type, string name)
        {
            if (!_byType.TryGetValue(type, out var list))
                return [];

            return [.. list.Where(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase))];
        }

        // -------------------------------------------------
        // Helper methods
        // -------------------------------------------------

        private static AssetType ResolveAssetType(string relativePath)
        {
            var parts = relativePath
                .Split(Path.DirectorySeparatorChar,
                       StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return AssetType.None;

            var topFolder = parts[0];

            if (topFolder.Equals("boxes", StringComparison.OrdinalIgnoreCase))
            {
                return AssetType.Box;
            }
            else if (topFolder.Equals("frames", StringComparison.OrdinalIgnoreCase))
            {
                return AssetType.Frame;
            }
            else if (topFolder.Equals("labelpresets", StringComparison.OrdinalIgnoreCase))
            {
                return AssetType.LabelPreset;
            }
            else if (topFolder.Equals("palettes", StringComparison.OrdinalIgnoreCase))
            {
                return AssetType.ColorPalette;
            }
            else if (topFolder.Equals("symbols", StringComparison.OrdinalIgnoreCase))
            {
                return AssetType.Symbol;
            }
            else if (topFolder.Equals("textures", StringComparison.OrdinalIgnoreCase) && parts.Length >= 2)
            {
                var subFolder = parts[1];
                if (_folderTypeMap.TryGetValue(subFolder, out var subType))
                {
                    return subType;
                }
            }
            else if (topFolder.Equals("themes", StringComparison.OrdinalIgnoreCase))
            {
                return AssetType.Theme;
            }

            return _folderTypeMap.TryGetValue(topFolder, out var type)
                ? type
                : AssetType.None;
        }

        private static string GenerateId(string relativePath)
        {
            return relativePath
                .Replace('\\', '/')
                .ToLowerInvariant();
        }

        private static string? ExtractCollection(string relativePath)
        {
            var parts = relativePath
                .Split(Path.DirectorySeparatorChar,
                       StringSplitOptions.RemoveEmptyEntries);

            return parts.Length >= 3 ? parts[1] : null;
        }

        private static IEnumerable<string> ExtractTags(string relativePath)
        {
            return relativePath
                .Split(Path.DirectorySeparatorChar,
                       StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.ToLowerInvariant());
        }

        private static bool IsImageFile(string extension)
            => extension is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".webp";

        private static bool IsVectorFile(string extension)
            => extension == ".svg";

        // -------------------------------------------------
        // Runtime image loading (lazy)
        // -------------------------------------------------

        public SKImage? GetImage(string assetId)
        {
            if (_imageCache.TryGetValue(assetId, out var img))
                return img;

            var descriptor = _descriptors.FirstOrDefault(d => d.Id == assetId);
            if (descriptor == null)
                return null;

            if (!File.Exists(descriptor.FilePath))
                return null;

            using var stream = File.OpenRead(descriptor.FilePath);
            using var bitmap = SKBitmap.Decode(stream);

            img = SKImage.FromBitmap(bitmap);
            _imageCache[assetId] = img;

            return img;
        }

        public AssetDescriptor? GetAsset(string assetId)
        {
            var descriptor = _descriptors.FirstOrDefault(d => d.Id == assetId);
            return descriptor;
        }


        /*
        internal static void LoadThemes()
        {
            THEME_LIST.Clear();

            string assetDirectory = Settings.Default.MapAssetDirectory;

            if (string.IsNullOrEmpty(assetDirectory))
            {
                assetDirectory = UtilityMethods.DEFAULT_ASSETS_FOLDER;
            }

            if (!Directory.Exists(assetDirectory) || (Directory.Exists(assetDirectory) && Directory.GetDirectories(assetDirectory).Length == 0))
            {
                MessageBox.Show("No map assets could be found. Realm Studio will close.", "Realm Studio Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                Application.Exit();
                return;
            }

            string themeDirectory = assetDirectory + Path.DirectorySeparatorChar + "Themes";

            var files = from file in Directory.EnumerateFiles(assetDirectory, "*.*", SearchOption.AllDirectories).Order()
                        where file.Contains(".rstheme")
                        select new
                        {
                            File = file
                        };

            foreach (var f in files)
            {
                bool rewriteTheme = false;

                string path = Path.GetFullPath(f.File);

                MapTheme? t = MapFileMethods.ReadThemeFromXml(path);

                if (t != null)
                {
                    if (!File.Exists(t.ThemePath))
                    {
                        rewriteTheme = true;
                        t.ThemePath = path;
                    }

                    if (t.BackgroundTexture != null)
                    {
                        if (!File.Exists(t.BackgroundTexture.TexturePath))
                        {
                            string fileName = System.IO.Path.GetFileName(t.BackgroundTexture.TexturePath);

                            string textureDirectory = assetDirectory + Path.DirectorySeparatorChar + "Textures"
                                + Path.DirectorySeparatorChar + "Background" + Path.DirectorySeparatorChar;

                            string bitmapPath = textureDirectory + fileName;

                            if (File.Exists(bitmapPath))
                            {
                                t.BackgroundTexture.TexturePath = bitmapPath;
                                rewriteTheme = true;
                            }
                        }
                    }

                    if (t.OceanTexture != null)
                    {
                        if (!File.Exists(t.OceanTexture.TexturePath))
                        {
                            string fileName = Path.GetFileName(t.OceanTexture.TexturePath);

                            string textureDirectory = assetDirectory + Path.DirectorySeparatorChar + "Textures"
                                + Path.DirectorySeparatorChar + "Water" + Path.DirectorySeparatorChar;

                            string bitmapPath = textureDirectory + fileName;

                            if (File.Exists(bitmapPath))
                            {
                                t.OceanTexture.TexturePath = bitmapPath;
                                rewriteTheme = true;
                            }
                        }
                    }

                    if (t.LandformTexture != null)
                    {
                        if (!File.Exists(t.LandformTexture.TexturePath))
                        {
                            string fileName = Path.GetFileName(t.LandformTexture.TexturePath);

                            string textureDirectory = assetDirectory + Path.DirectorySeparatorChar + "Textures"
                                + Path.DirectorySeparatorChar + "Land" + Path.DirectorySeparatorChar;

                            string bitmapPath = textureDirectory + fileName;

                            if (File.Exists(bitmapPath))
                            {
                                t.LandformTexture.TexturePath = bitmapPath;
                                rewriteTheme = true;
                            }
                        }
                    }

                    THEME_LIST.Add(t);

                    if (t.IsDefaultTheme)
                    {
                        CURRENT_THEME = t;
                    }

                    if (rewriteTheme)
                    {
                        MapFileMethods.SerializeTheme(t);
                    }
                }
            }
        }
        */     




        /*
        public static void LoadSymbolTags()
        {
            SYMBOL_TAGS.Clear();
            ORIGINAL_SYMBOL_TAGS.Clear();

            IEnumerable<string> tags = File.ReadLines(SymbolTagsFilePath);
            foreach (string tag in tags)
            {
                if (!string.IsNullOrEmpty(tag))
                {
                    AddSymbolTag(tag);
                }
            }

            SYMBOL_TAGS.Sort();

            foreach (string tag in SYMBOL_TAGS)
            {
                ORIGINAL_SYMBOL_TAGS.Add(tag);
            }
        }
        */

        /*
        private static void LoadSymbolTypeSynonyms()
        {
            STRUCTURE_SYNONYMS.Clear();
            TERRAIN_SYNONYMS.Clear();
            VEGETATION_SYNONYMS.Clear();

            IEnumerable<string> structureSynonyms = File.ReadLines(StructureSynonymsFilePath);
            foreach (string synonym in structureSynonyms)
            {
                if (!string.IsNullOrEmpty(synonym))
                {
                    string trimmedSynonym = synonym.Trim([' ', ',']).ToLowerInvariant();

                    if (!STRUCTURE_SYNONYMS.Contains(trimmedSynonym))
                    {
                        STRUCTURE_SYNONYMS.Add(trimmedSynonym);
                    }
                }
            }

            IEnumerable<string> terrainSynonyms = File.ReadLines(TerrainSynonymsFilePath);
            foreach (string synonym in terrainSynonyms)
            {
                if (!string.IsNullOrEmpty(synonym))
                {
                    string trimmedSynonym = synonym.Trim([' ', ',']).ToLowerInvariant();

                    if (!TERRAIN_SYNONYMS.Contains(trimmedSynonym))
                    {
                        TERRAIN_SYNONYMS.Add(trimmedSynonym);
                    }
                }
            }

            IEnumerable<string> vegetationSynonyms = File.ReadLines(VegetationSynonymsFilePath);
            foreach (string synonym in vegetationSynonyms)
            {
                if (!string.IsNullOrEmpty(synonym))
                {
                    string trimmedSynonym = synonym.Trim([' ', ',']).ToLowerInvariant();

                    if (!VEGETATION_SYNONYMS.Contains(trimmedSynonym))
                    {
                        VEGETATION_SYNONYMS.Add(trimmedSynonym);
                    }
                }
            }
        }
        */

    }
}
