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
using System.Diagnostics;
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

        public static RealmStudioMapRoot? OpenMapRoot(string mapPath)
        {
            XmlSerializer? serializer = new(typeof(RealmStudioMapRoot));
            
            // If the XML document has been altered with unknown
            // nodes or attributes, handle them with the
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(Serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(Serializer_UnknownAttribute);
            
            // A FileStream is needed to read the XML document.            
            FileStream fs = new(mapPath, FileMode.Open);
            using XmlReader reader = XmlReader.Create(fs);

            // Declares an object variable of the type to be deserialized.
            RealmStudioMapRoot? mapRoot;
            try
            {
                // Uses the Deserialize method to restore the object's state
                // with data from the XML document. */
                mapRoot = serializer.Deserialize(reader) as RealmStudioMapRoot;
            }
            catch (Exception ex)
            {
                mapRoot = null;
                throw new Exception("Exception deserializing " + mapPath + ": " + ex.Message);
            }
            finally
            {
                serializer = null;
                fs.Dispose();
            }

            return mapRoot;
        }

        public static RealmStudioMap? OpenMap(string mapPath)
        {
            XmlSerializer? serializer = new(typeof(RealmStudioMap));

            // If the XML document has been altered with unknown
            // nodes or attributes, handle them with the
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(Serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(Serializer_UnknownAttribute);

            // A FileStream is needed to read the XML document.            
            FileStream fs = new(mapPath, FileMode.Open);
            using XmlReader reader = XmlReader.Create(fs);

            // Declares an object variable of the type to be deserialized.
            RealmStudioMap? map;

            try
            {
                // Uses the Deserialize method to restore the object's state
                // with data from the XML document. */
                map = serializer.Deserialize(reader) as RealmStudioMap;

            }
            catch (Exception ex)
            {
                map = null;
                throw new Exception("Exception deserializing " + mapPath + ": " + ex.Message);
            }
            finally
            {
                serializer = null;
                fs.Dispose();
            }

            return map;
        }

        public static RealmStudioMapSet? OpenMapSet(string mapSetPath)
        {
            XmlSerializer? serializer = new(typeof(RealmStudioMapSet));

            // If the XML document has been altered with unknown
            // nodes or attributes, handle them with the
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(Serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(Serializer_UnknownAttribute);

            // A FileStream is needed to read the XML document.            
            FileStream fs = new(mapSetPath, FileMode.Open);
            using XmlReader reader = XmlReader.Create(fs);

            // Declares an object variable of the type to be deserialized.
            RealmStudioMapSet? mapSet;

            try
            {
                // Uses the Deserialize method to restore the object's state
                // with data from the XML document. */
                mapSet = serializer.Deserialize(reader) as RealmStudioMapSet;
            }
            catch (Exception)
            {
                mapSet = null;
                throw;
            }
            finally
            {
                serializer = null;
                fs.Dispose();
            }

            return mapSet;
        }

        public static void SaveMap(RealmStudioMap map)
        {
            using TextWriter? writer = new StreamWriter(map.MapPath);
            XmlSerializer? serializer = new(typeof(RealmStudioMap));

            try
            {
                // Serializes the map and closes the TextWriter.
                serializer.Serialize(writer, map);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception serializing " + map.MapPath + " Message: " + ex.Message);
            }
        }

        public static void SaveMapSet(RealmStudioMapSet mapSet)
        {
            using TextWriter? writer = new StreamWriter(mapSet.MapSetPath);
            XmlSerializer? serializer = new(typeof(RealmStudioMapSet));

            try
            {
                // Serializes the map and closes the TextWriter.
                serializer.Serialize(writer, mapSet);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception serializing " + mapSet.MapSetPath + " Message: " + ex.Message);
            }
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
                throw new Exception("Exception deserializing " + path + " Message: " + ex.Message);

            }
            finally
            {
                serializer = null;
            }
        }

        /*
        internal static void SerializeTheme(MapTheme theme)
        {
            if (theme.ThemeName != null && theme.ThemeName.Length > 0 && theme.ThemePath != null && theme.ThemePath.Length > 0)
            {
                TextWriter? writer = new StreamWriter(theme.ThemePath);
                XmlSerializer? serializer = new(typeof(MapTheme));

                try
                {
                    // Serializes the theme and closes the TextWriter.
                    serializer.Serialize(writer, theme);
                }
                catch (Exception ex)
                {
                    Program.LOGGER.Error("Error saving theme: " + ex.Message);

                    MessageBox.Show("Error saving theme: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

                    throw;
                }
                finally
                {
                    writer?.Dispose();
                }
            }
        }
        */

        public static MapSymbolCollection? ReadCollection(string path)
        {
            var isConverted = path.EndsWith(AssetManager.CollectionFileName, StringComparison.OrdinalIgnoreCase);

            // Try load
            var collection = ReadCollectionFromXml(path);

            if (collection != null && collection.Symbols?.Count > 0)
                return collection;

            // -------------------------------------------------
            // If this is a broken converted file → fallback
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
        public static void SerializeLabelPreset(LabelPreset preset)
        {
            if (!string.IsNullOrEmpty(preset.PresetXmlFilePath))
            {
                TextWriter? writer = new StreamWriter(preset.PresetXmlFilePath);
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
