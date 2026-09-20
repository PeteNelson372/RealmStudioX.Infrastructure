/**************************************************************************************************************************
* Copyright 2026, Peter R. Nelson
*
* This file is part of the RealmStudioX application. The RealmStudioX application is intended
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
using RealmStudioShapeRenderingLib.Logging;
using RealmStudioX.Core;
using SkiaSharp;
using System.Diagnostics;
using System.IO.Compression;
using System.Xml;
using System.Xml.Serialization;
namespace RealmStudioX.Infrastructure
{
    public static class MapFileMethods
    {
        private static void Serializer_UnknownNode(object? sender, XmlNodeEventArgs e)
        {
            Debug.WriteLine("Exception on Load. Unknown Node: " + e.Name + "\t" + e.Text);
        }

        private static void Serializer_UnknownAttribute(object? sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            Debug.WriteLine("Exception on Load. Unknown Attribute: " + attr.Name + "\t" + attr.Value);
        }


        public static RealmStudioMap DeserializeMap(string xml)
        {
            XmlSerializer serializer = new(typeof(RealmStudioMap));

            // If the XML document has been altered with unknown
            // nodes or attributes, handle them with the
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(Serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(Serializer_UnknownAttribute);

            using StringReader reader = new(xml);

            return (RealmStudioMap)serializer.Deserialize(reader)!;
        }


        public static RealmStudioMap? OpenMap(string mapPath)
        {
            string xml = File.ReadAllText(mapPath);
            RealmStudioMap map = DeserializeMap(xml);

            return map;
        }

        public static string SerializeMap(RealmStudioMap map)
        {
            XmlSerializer serializer = new(typeof(RealmStudioMap));

            using StringWriter writer = new();

            serializer.Serialize(writer, map);

            return writer.ToString();
        }

        public static void SaveMap(RealmStudioMap map, Stream stream)
        {
            XmlSerializer serializer = new(typeof(RealmStudioMap));

            serializer.Serialize(stream, map);
        }

        public static void SaveMap(RealmStudioMap map)
        {
            string xml = SerializeMap(map);

            File.WriteAllText(map.MapPath, xml);
        }

        private static readonly object _saveLock = new();

        public static void SaveProject(string projectPath, RealmStudioProject project)
        {
            // save the project using an atomic save with backup to prevent loss of the project
            lock (_saveLock)
            {
                string projectTmpPath = projectPath + ".tmp";
                string projectBackupPath = projectPath + RealmStudioFileFormat.BackupFileExtension;

                try
                {
                    using (ZipArchive archive = ZipFile.Open(projectTmpPath, ZipArchiveMode.Create))
                    {

                        //--------------------------------------------------
                        // Build manifest
                        //--------------------------------------------------

                        project.Metadata.ProjectFilePath = projectPath;

                        ProjectManifest manifest = new()
                        {
                            FormatVersion = RealmStudioProject.ProjectFormatVersion,
                            ActiveMapId = project.ActiveMapId,
                            Metadata = project.Metadata
                        };

                        foreach (MapProjectEntry mapEntry in project.Maps)
                        {
                            string mapName = mapEntry.Map!.MapName;
                            string mapId = mapEntry.MapId;

                            string folder = $"Maps/{mapEntry.MapId}/";

                            manifest.Maps.Add(
                                new ProjectMapManifest
                                {
                                    MapId = mapEntry.MapId,
                                    MapName = mapEntry.Metadata!.Name,

                                    MapFile =
                                        folder + $"{mapId}.rsmx",

                                    MetadataFile =
                                        folder + $"{mapId}.metadata.xml",

                                    PreviewFile =
                                        folder + $"{mapId}.png"
                                });
                        }

                        //--------------------------------------------------
                        // Save project.xml
                        //--------------------------------------------------

                        string manifestXml = SerializeObject(manifest);

                        ZipArchiveEntry projectEntry = archive.CreateEntry("project.xml");

                        using (StreamWriter writer = new(projectEntry.Open()))
                        {
                            writer.Write(manifestXml);
                        }

                        //--------------------------------------------------
                        // Save maps
                        //--------------------------------------------------

                        foreach (MapProjectEntry mapEntry in project.Maps)
                        {
                            if (mapEntry.Map != null)
                            {
                                string folder = $"Maps/{mapEntry.MapId}/";

                                //
                                // Map
                                //

                                string mapXml = SerializeMap(mapEntry.Map);

                                ZipArchiveEntry mapFile = archive.CreateEntry(folder + $"{mapEntry.MapId}.rsmx");

                                using (StreamWriter writer = new(mapFile.Open()))
                                {
                                    writer.Write(mapXml);
                                }

                                //
                                // Metadata
                                //

                                string metadataXml = SerializeObject(mapEntry.Metadata);

                                ZipArchiveEntry metadataFile =
                                    archive.CreateEntry(
                                        folder +
                                        $"{mapEntry.MapId}.metadata.xml");

                                using (StreamWriter writer =
                                    new(metadataFile.Open()))
                                {
                                    writer.Write(metadataXml);
                                }

                                //
                                // Preview
                                //

                                if (mapEntry.Preview != null && mapEntry.Preview.ByteCount > 0)
                                {
                                    ZipArchiveEntry previewFile =
                                        archive.CreateEntry(folder + $"{mapEntry.MapId}.png");

                                    using Stream stream = previewFile.Open();

                                    using SKData data =
                                        mapEntry.Preview.Encode(SKEncodedImageFormat.Png, 100);

                                    data.SaveTo(stream);
                                }
                            }
                        }
                    }

                    // validate the project zip file
                    ValidateProjectFile(projectTmpPath);

                    if (File.Exists(projectPath))
                    {
                        File.Replace(
                            projectTmpPath,
                            projectPath,
                            projectBackupPath);
                    }
                    else
                    {
                        File.Move(
                            projectTmpPath,
                            projectPath);
                    }
                }
                catch (Exception ex)
                {
                    // log the error
                    RealmStudioXLogger.Exception("MapFileMethods.SaveProject", ex);
                    throw;
                }
                finally
                {
                    try
                    {
                        if (File.Exists(projectTmpPath))
                        {
                            File.Delete(projectTmpPath);
                        }
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        private static void ValidateProjectFile(string projectFilePath)
        {
            // the only validation that is done is to open the project;
            // if it can be opened, it (presumably) is valid
            try
            {
                RealmStudioProject project = OpenProject(projectFilePath);
            }
            catch (Exception ex)
            {
                RealmStudioXLogger.Exception($"Error validating project: {projectFilePath}", ex);
                throw;
            }
        }

        public static RealmStudioProject OpenProject(string projectPath)
        {
            using ZipArchive archive = ZipFile.OpenRead(projectPath);

            //--------------------------------------------------
            // Load project.xml
            //--------------------------------------------------

            ZipArchiveEntry? projectEntry =
                archive.GetEntry("project.xml") ?? throw new Exception("project.xml not found.");

            string manifestXml;

            using (StreamReader reader = new(projectEntry.Open()))
            {
                manifestXml = reader.ReadToEnd();
            }

            if (string.IsNullOrEmpty(manifestXml))
            {
                throw new Exception("Project is invalid. No XML data found for project manifest");
            }

            ProjectManifest manifest = DeserializeObject<ProjectManifest>(manifestXml);

            RealmStudioProject project = new()
            {
                Metadata = manifest.Metadata,
                ActiveMapId = manifest.ActiveMapId
            };

            //--------------------------------------------------
            // Load maps
            //--------------------------------------------------

            foreach (ProjectMapManifest mapManifest in manifest.Maps)
            {
                //
                // Map
                //

                ZipArchiveEntry? mapFile = archive.GetEntry(mapManifest.MapFile);

                if (mapFile == null)
                {
                    throw new Exception("Project is invalid. MapFile entry is null."); ;
                }

                string mapXml;

                using (StreamReader reader = new(mapFile.Open()))
                {
                    mapXml = reader.ReadToEnd();
                }

                if (string.IsNullOrEmpty(mapXml))
                {
                    throw new Exception("Project is invalid. No XML data for map.");
                }

                RealmStudioMap map = DeserializeMap(mapXml);

                //
                // Metadata
                //

                MapMetadata metadata = new();

                ZipArchiveEntry? metadataFile = archive.GetEntry(mapManifest.MetadataFile);

                if (metadataFile != null)
                {
                    string metadataXml;

                    using (StreamReader reader = new(metadataFile.Open()))
                    {
                        metadataXml = reader.ReadToEnd();
                    }

                    if (string.IsNullOrEmpty(metadataXml))
                    {
                        throw new Exception("Project is invalid. No XML data for map metadata.");
                    }

                    metadata = DeserializeObject<MapMetadata>(metadataXml);
                }

                //
                // Preview
                //

                SKBitmap? preview = null;

                ZipArchiveEntry? previewFile = archive.GetEntry(mapManifest.PreviewFile);

                if (previewFile != null)
                {
                    using Stream zipStream = previewFile.Open();

                    using MemoryStream ms = new();

                    zipStream.CopyTo(ms);

                    byte[] bytes = ms.ToArray();

                    preview = SKBitmap.Decode(bytes);

                    preview ??= new SKBitmap();
                }

                project.Maps.Add(
                    new MapProjectEntry
                    {
                        MapId = mapManifest.MapId,
                        Map = map,
                        Metadata = metadata,
                        Preview = preview != null ? preview : new SKBitmap()
                    });
            }

            return project;
        }

        private static void SaveMap(MapProjectEntry entry, ZipArchive archive)
        {
            string mapXml = SerializeMap(entry.Map);

            ZipArchiveEntry mapEntry = archive.CreateEntry($"Maps/{entry.MapId}.rsmx");

            using (StreamWriter writer = new(mapEntry.Open()))
            {
                writer.Write(mapXml);
            }
        }

        // generic serialization method
        public static string SerializeObject<T>(T obj)
        {
            XmlSerializer serializer = new(typeof(T));

            // If the XML document has been altered with unknown
            // nodes or attributes, handle them with the
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(Serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(Serializer_UnknownAttribute);

            using StringWriter writer = new();

            serializer.Serialize(writer, obj);

            return writer.ToString();
        }

        // generic deserialization method
        public static T DeserializeObject<T>(string xml)
        {
            XmlSerializer serializer = new(typeof(T));

            // If the XML document has been altered with unknown
            // nodes or attributes, handle them with the
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(Serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(Serializer_UnknownAttribute);

            using StringReader reader = new(xml);

            return (T)serializer.Deserialize(reader)!;
        }

        public static object DeserializeObject(Type objectType, string xml)
        {
            XmlSerializer serializer = new(objectType);

            serializer.UnknownNode += Serializer_UnknownNode;
            serializer.UnknownAttribute += Serializer_UnknownAttribute;

            using StringReader reader = new(xml);

            return serializer.Deserialize(reader)!;
        }

        public static MapTheme? ReadThemeFromXml(string path)
        {
            XmlSerializer? serializer = new(typeof(MapTheme));

            // If the XML document has been altered with unknown
            // nodes or attributes, handle them with the
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(Serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(Serializer_UnknownAttribute);

            // A FileStream is needed to read the XML document.            
            using FileStream fs = new(path, FileMode.Open);
            using XmlReader reader = XmlReader.Create(fs);

            // Declares an object variable of the type to be deserialized.            
            MapTheme? theme;

            try
            {
                // Uses the Deserialize method to restore the object's state
                // with data from the XML document. */
                theme = serializer.Deserialize(reader) as MapTheme;
                return theme;
            }
            catch (Exception ex)
            {
                theme = null;
                RealmStudioXLogger.Exception($"An exception occured deserializing theme from {path}", ex);
                throw;
            }
            finally
            {
                serializer = null;
            }
        }


        public static void SerializeTheme(MapTheme theme, string themePath)
        {
            if (theme.ThemeName != null && theme.ThemeName.Length > 0 && !string.IsNullOrEmpty(themePath))
            {
                TextWriter? writer = new StreamWriter(themePath);
                XmlSerializer? serializer = new(typeof(MapTheme));

                try
                {
                    // Serializes the theme and closes the TextWriter.
                    serializer.Serialize(writer, theme);
                }
                catch (Exception ex)
                {
                    RealmStudioXLogger.Exception($"An exception occured serializing theme to {themePath}", ex);
                    throw;
                }
                finally
                {
                    writer?.Dispose();
                }
            }
        }


        public static MapSymbolCollection? ReadCollection(string path)
        {
            var isConverted = path.EndsWith(AssetManager.CollectionFileName, StringComparison.OrdinalIgnoreCase);

            // Try load
            var collection = ReadCollectionFromXml(path);

            if (collection != null && collection.Symbols?.Count > 0)
                return collection;

            // -------------------------------------------------
            // If this is a broken converted file then fallback
            // -------------------------------------------------

            if (isConverted)
            {
                var originalPath = path.Replace(AssetManager.CollectionFileName, AssetManager.LegacyCollectionFileName);

                if (File.Exists(originalPath) && IsOldFormat(originalPath))
                {
                    // Invalid converted collection detected. Rebuilding
                    var converted = ConvertOldCollection(originalPath);

                    if (converted != null)
                    {
                        var newPath = GetConvertedCollectionPath(originalPath);
                        converted.Path = newPath;

                        SerializeSymbolCollection(converted); // overwrite

                        return ReadCollectionFromXml(path);
                    }
                }

                return null;
            }

            // -------------------------------------------------
            // Normal legacy fallback
            // -------------------------------------------------

            if (IsOldFormat(path))
            {
                // Detected legacy collection format

                var converted = ConvertOldCollection(path);

                if (converted != null)
                {
                    var newPath = GetConvertedCollectionPath(path);
                    converted.Path = newPath;

                    SerializeSymbolCollection(converted);

                    return ReadCollectionFromXml(newPath);
                }
            }

            return null;
        }

        public static MapSymbolCollection? ReadCollectionFromXml(string path)
        {
            XmlSerializer? serializer = new(typeof(MapSymbolCollection));

            // If the XML document has been altered with unknown
            // nodes or attributes, handle them with the
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(Serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(Serializer_UnknownAttribute);

            // A FileStream is needed to read the XML document.            
            FileStream fs = new(path, FileMode.Open);
            using XmlReader reader = XmlReader.Create(fs);

            // Declares an object variable of the type to be deserialized.            
            MapSymbolCollection? symbolCollection;

            try
            {
                // Uses the Deserialize method to restore the object's state
                // with data from the XML document.
                symbolCollection = serializer.Deserialize(reader) as MapSymbolCollection;

                return symbolCollection;
            }
            catch (Exception ex)
            {

                symbolCollection = null;
                throw new Exception("Exception deserializing " + path + " Message: " + ex.Message);

            }
            finally
            {
                serializer = null;
                fs.Dispose();
            }
        }

        public static void SerializeSymbolCollection(MapSymbolCollection collection)
        {
            if (collection.Name.Length > 0 && collection.Path.Length > 0)
            {
                var serializer = new XmlSerializer(typeof(MapSymbolCollection));

                using var fs = new FileStream(collection.Path, FileMode.Create);
                using var writer = XmlWriter.Create(fs, new XmlWriterSettings
                {
                    Indent = true
                });

                serializer.Serialize(writer, collection);
            }
        }

        public static MapFrame? ReadFrameAssetFromXml(string path)
        {
            XmlSerializer? serializer = new(typeof(MapFrame));

            // If the XML document has been altered with unknown
            // nodes or attributes, handle them with the
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(Serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(Serializer_UnknownAttribute);

            // A FileStream is needed to read the XML document.            
            using FileStream fs = new(path, FileMode.Open);

            // Declares an object variable of the type to be deserialized.            
            MapFrame? frame;

            try
            {
                using XmlReader reader = new IgnoreNamespaceXmlTextReader(new StreamReader(fs));

                // Uses the Deserialize method to restore the object's state
                // with data from the XML document.
                frame = serializer.Deserialize(reader) as MapFrame;
                return frame;
            }
            catch (Exception ex)
            {
                frame = null;
                throw new Exception("Exception deserializing frame XML at " + path + " Message: " + ex.Message);
            }
            finally
            {
                serializer = null;
            }
        }

        public static void SerializeFrameAsset(MapFrame frame)
        {
            if (!string.IsNullOrEmpty(frame.FrameXmlFilePath))
            {
                TextWriter? writer = new StreamWriter(frame.FrameXmlFilePath);
                XmlSerializer? serializer = new(typeof(MapFrame));

                try
                {
                    // Serializes the frame and closes the TextWriter.
                    serializer.Serialize(writer, frame);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error saving frame: " + ex.Message);
                }
                finally
                {
                    writer?.Dispose();
                }
            }
        }

        public static MapBox? ReadBoxAssetFromXml(string path)
        {
            XmlSerializer? serializer = new(typeof(MapBox));

            // If the XML document has been altered with unknown
            // nodes or attributes, handle them with the
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(Serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(Serializer_UnknownAttribute);

            // A FileStream is needed to read the XML document.            
            using FileStream fs = new(path, FileMode.Open);

            // Declares an object variable of the type to be deserialized.            
            MapBox? box;

            try
            {
                using XmlReader reader = XmlReader.Create(fs);

                // Uses the Deserialize method to restore the object's state
                // with data from the XML document.
                box = serializer.Deserialize(reader) as MapBox;
                return box;
            }
            catch (Exception ex)
            {
                box = null;
                throw new Exception("Exception deserializing box XML at " + path + " Message: " + ex.Message);

            }
            finally
            {
                serializer = null;
            }
        }

        public static void SerializeBoxAsset(MapBox box)
        {
            if (!string.IsNullOrEmpty(box.BoxXmlFilePath))
            {
                TextWriter? writer = new StreamWriter(box.BoxXmlFilePath);
                XmlSerializer? serializer = new(typeof(MapBox));

                try
                {
                    // Serializes the box and closes the TextWriter.
                    serializer.Serialize(writer, box);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error saving box: " + ex.Message);
                }
                finally
                {
                    writer?.Dispose();
                }
            }
        }


        public static void SerializeLabelPreset(LabelPreset preset, string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                TextWriter? writer = new StreamWriter(filePath);
                XmlSerializer? serializer = new(typeof(LabelPreset));

                try
                {
                    // Serializes the label preset and closes the TextWriter.
                    serializer.Serialize(writer, preset);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error saving label preset: " + ex.Message);
                }
                finally
                {
                    writer?.Dispose();
                }
            }
        }

        public static LabelPreset? ReadLabelPreset(string path)
        {
            XmlSerializer? serializer = new(typeof(LabelPreset));

            // If the XML document has been altered with unknown
            // nodes or attributes, handle them with the
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(Serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(Serializer_UnknownAttribute);

            // A FileStream is needed to read the XML document.            
            using FileStream fs = new(path, FileMode.Open);

            // Declares an object variable of the type to be deserialized.            
            LabelPreset? labelPreset;

            try
            {
                using XmlReader reader = XmlReader.Create(fs);

                // Uses the Deserialize method to restore the object's state
                // with data from the XML document.
                labelPreset = serializer.Deserialize(reader) as LabelPreset;
                return labelPreset;
            }
            catch (Exception ex)
            {
                labelPreset = null;
                throw new Exception("Exception deserializing label preset XML at " + path + " Message: " + ex.Message);
            }
            finally
            {
                serializer = null;
            }
        }

        public static MapBrush? LoadBrush(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            XmlSerializer serializer = new(typeof(MapBrush));

            using FileStream stream = File.OpenRead(filePath);

            MapBrush? brush = serializer.Deserialize(stream) as MapBrush;

            if (brush == null)
            {
                return null;
            }

            // Resolve bitmap path relative to XML

            string directory = Path.GetDirectoryName(filePath) ?? "";

            foreach (string imgPath in brush.BrushImages)
            {
                string bmpPath = Path.Combine(directory, imgPath);

                if (File.Exists(bmpPath))
                {
                    brush.BrushBitmaps.Add(SKBitmap.Decode(bmpPath));
                }
            }

            return brush;
        }

        /*
        internal static LandformShapingFunction? ReadShapingFunction(string path)
        {
            LandformShapingFunction lsf = new();

            try
            {
                lsf.ShapingBitmap = ((Bitmap)(Image.FromFile(path))).ToSKBitmap();

                if (Path.GetFileNameWithoutExtension(path).Contains("Region"))
                {
                    lsf.LandformShapeType = GeneratedLandformType.Region;
                }
                else if (Path.GetFileNameWithoutExtension(path).Contains("Continent"))
                {
                    lsf.LandformShapeType = GeneratedLandformType.Continent;
                }
                else if (Path.GetFileNameWithoutExtension(path).Contains("Island"))
                {
                    lsf.LandformShapeType = GeneratedLandformType.Island;
                }
                else if (Path.GetFileNameWithoutExtension(path).Contains("Archipelago"))
                {
                    lsf.LandformShapeType = GeneratedLandformType.Archipelago;
                }
                else if (Path.GetFileNameWithoutExtension(path).Contains("Atoll"))
                {
                    lsf.LandformShapeType = GeneratedLandformType.Atoll;
                }
                else if (Path.GetFileNameWithoutExtension(path).Contains("World"))
                {
                    lsf.LandformShapeType = GeneratedLandformType.World;
                }
                else if (Path.GetFileNameWithoutExtension(path).Contains("Icecap"))
                {
                    lsf.LandformShapeType = GeneratedLandformType.Icecap;
                }
            }
            catch { }

            return lsf;
        }
        */

        //=================
        // Helper Methods
        //=================

        private static void FinalizeCollection(MapSymbolCollection collection, string xmlPath)
        {
            var baseDir = Path.GetDirectoryName(xmlPath)!;

            foreach (var symbol in collection.Symbols)
            {
                var fullPath = Path.GetFullPath(Path.Combine(baseDir, symbol.SymbolFilePath));
                symbol.SymbolFilePath = Utilities.NormalizePath(fullPath);

                symbol.FinalizeAfterLoad(collection.Id, collection.Name, baseDir);

                if (symbol.SymbolFormat == SymbolFileFormat.NotSet)
                {
                    symbol.SymbolFormat = Utilities.InferFileFormat(symbol.SymbolFilePath);
                }
            }
        }

        private static bool IsOldFormat(string path)
        {
            using var reader = XmlReader.Create(path);

            reader.MoveToContent();

            return reader.NamespaceURI.Contains("RealmStudio");
        }

        private static MapSymbolCollection? ConvertOldCollection(string path)
        {
            try
            {
                return LegacyCollectionConverter.Convert(path);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string GetConvertedCollectionPath(string originalPath)
        {
            var dir = Path.GetDirectoryName(originalPath)!;
            return Path.Combine(dir, "collectionx.xml");
        }
    }

    public class IgnoreNamespaceXmlTextReader : XmlTextReader
    {
        public IgnoreNamespaceXmlTextReader(TextReader reader)
            : base(reader)
        {
        }

        public override string NamespaceURI => "";
    }
}
