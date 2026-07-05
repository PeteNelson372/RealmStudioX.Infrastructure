using RealmStudioShapeRenderingLib;
using SkiaSharp;

namespace RealmStudioX.Infrastructure
{
    public static class AssetInitializer
    {
        public static void InitializeMapShapeAssets(RealmStudioMap map, AssetManager assetManager, FontManager fontManager)
        {
            foreach (MapLayer layer in map.MapLayers)
            {
                foreach (MapComponent2D shape in layer.Shapes)
                {
                    switch (shape)
                    {
                        case Landform landform:
                            landform.ResolveAssets(assetManager);
                            break;
                        case River river:
                            // no op
                            break;
                        case Lake lake:
                            // no op
                            break;
                        case PaintedWaterBody paintedWaterBody:
                            // no op
                            break;
                        case MapPath mapPath:
                            mapPath.ResolveAssets(assetManager);
                            break;
                        case MapSymbol mapSymbol:
                            // no op - loading of symbol image is handled in Render method
                            break;
                        case MapLabel mapLabel:
                            // no op - loading of font is handled in Render method
                            break;
                        case PlacedMapBox placedMapBox:
                            if (placedMapBox.BaseBox != null && !string.IsNullOrEmpty(placedMapBox.BaseBox.BoxBitmapPath))
                            {
                                if (File.Exists(placedMapBox.BaseBox.BoxBitmapPath))
                                {
                                    placedMapBox.BaseBox.BoxBitmap = SKBitmap.Decode(placedMapBox.BaseBox.BoxBitmapPath);
                                    placedMapBox.BoxBitmap = placedMapBox.BaseBox.BoxBitmap.Copy();
                                }
                            }
                            break;
                        case PlacedMapFrame placedMapFrame:
                            // no op - frame is handled by special case in MainWindowViewModel.FinalizeMapLoad
                            break;
                        case MapGrid mapGrid:
                            // no op
                            break;
                        case MapWindrose mapWindrose:
                            // no op
                            break;
                        case MapScale mapScale:
                            // no op - font loading is handled in Render method
                            break;
                        case MapRegion mapRegion:
                            // no op
                            break;
                        case MapVignette mapVignette:
                            // no op
                            break;
                        case DrawnArrow drawnArrow:
                            if (!string.IsNullOrEmpty(drawnArrow.FillImageId))
                            {
                                drawnArrow.FillImage = assetManager.GetImage(drawnArrow.FillImageId);
                            }
                            break;
                        case DrawingErase drawingErase:
                            // no op
                            break;
                        case DrawnDiamond drawnDiamond:
                            if (!string.IsNullOrEmpty(drawnDiamond.FillImageId))
                            {
                                drawnDiamond.FillImage = assetManager.GetImage(drawnDiamond.FillImageId);
                            }
                            break;
                        case DrawnEllipse drawnEllipse:
                            if (!string.IsNullOrEmpty(drawnEllipse.FillImageId))
                            {
                                drawnEllipse.FillImage = assetManager.GetImage(drawnEllipse.FillImageId);
                            }
                            break;
                        case DrawnFivePointStar drawnFivePointStar:
                            if (!string.IsNullOrEmpty(drawnFivePointStar.FillImageId))
                            {
                                drawnFivePointStar.FillImage = assetManager.GetImage(drawnFivePointStar.FillImageId);
                            }
                            break;
                        case DrawnLine drawnLine:
                            if (!string.IsNullOrEmpty(drawnLine.TextureId))
                            {
                                drawnLine.Texture = assetManager.GetImage(drawnLine.TextureId);
                            }
                            break;
                        case DrawnPolygon drawnPolygon:
                            if (!string.IsNullOrEmpty(drawnPolygon.FillImageId))
                            {
                                drawnPolygon.FillImage = assetManager.GetImage(drawnPolygon.FillImageId);
                            }
                            break;
                        case DrawnRectangle drawnRectangle:
                            if (!string.IsNullOrEmpty(drawnRectangle.FillImageId))
                            {
                                drawnRectangle.FillImage = assetManager.GetImage(drawnRectangle.FillImageId);
                            }
                            break;
                        case DrawnRegularPolygon drawnRegularPolygon:
                            if (!string.IsNullOrEmpty(drawnRegularPolygon.FillImageId))
                            {
                                drawnRegularPolygon.FillImage = assetManager.GetImage(drawnRegularPolygon.FillImageId);
                            }
                            break;
                        case DrawnSixPointStar drawnSixPointStar:
                            if (!string.IsNullOrEmpty(drawnSixPointStar.FillImageId))
                            {
                                drawnSixPointStar.FillImage = assetManager.GetImage(drawnSixPointStar.FillImageId);
                            }
                            break;
                        case DrawnStamp drawnStamp:
                            if (!string.IsNullOrEmpty(drawnStamp.StampPath) && File.Exists(drawnStamp.StampPath))
                            {
                                SKBitmap img = SKBitmap.Decode(drawnStamp.StampPath);

                                SKRect r = new(
                                (float)(drawnStamp.TopLeft.X - map.MapWidth * drawnStamp.Scale / 2f),
                                (float)(drawnStamp.TopLeft.Y - map.MapHeight * drawnStamp.Scale / 2f),
                                (float)(drawnStamp.TopLeft.X + map.MapWidth * drawnStamp.Scale / 2f),
                                (float)(drawnStamp.TopLeft.Y + map.MapHeight * drawnStamp.Scale / 2f));

                                using SKBitmap resized = Utilities.ResizeBitmap(img, (int)r.Width, (int)r.Height);

                                using SKBitmap stampBitmap = Utilities.SetBitmapOpacity(resized, drawnStamp.Opacity);

                                drawnStamp.StampImage = SKImage.FromBitmap(stampBitmap);
                            }
                            break;
                        case DrawnTriangle drawnTriangle:
                            if (!string.IsNullOrEmpty(drawnTriangle.FillImageId))
                            {
                                drawnTriangle.FillImage = assetManager.GetImage(drawnTriangle.FillImageId);
                            }
                            break;
                        case PaintedLine paintedLine:

                            if (paintedLine.Brush != null && paintedLine.Brush.SourceBrush != null)
                            {
                                if (paintedLine.Brush.Bitmaps == null)
                                {
                                    continue;
                                }

                                if (paintedLine.Brush.Bitmaps.Count == 0)
                                {
                                    AssetBrowser brushBrowser = new(assetManager, AssetType.Brush);

                                    IReadOnlyList<AssetDescriptor> brushAssets = brushBrowser.GetAssets();

                                    foreach (string imageName in paintedLine.Brush.SourceBrush.BrushImages)
                                    {
                                        foreach (AssetDescriptor assetDescriptor in brushAssets)
                                        {
                                            if (imageName == assetDescriptor.FileName)
                                            {
                                                paintedLine.Brush.SourceBrush.BrushBitmaps.Add(SKBitmap.Decode(assetDescriptor.FilePath));
                                            }
                                        }
                                    }

                                    GetPreparedBrushBitmaps(paintedLine.Brush);
                                }
                            }

                            break;
                        case DrawnPixelEdits drawnPixelEdits:
                            // no op
                            break;
                    }
                }
            }
        }

        public static void GetPreparedBrushBitmaps(PreparedBrush brush)
        {
            if (brush == null || brush.SourceBrush == null || brush.SourceBrush.BrushBitmaps == null)
            {
                return;
            }

            for (int i = 0; i < brush!.SourceBrush.BrushBitmaps.Count; i++)
            {
                if (brush!.SourceBrush!.BrushBitmaps![i] is SKBitmap bitmap)
                {
                    if (brush!.SourceBrush!.PixelMode == BrushPixelMode.Color)
                    {
                        // bitmap is already colorized, just scale it
                        brush.Bitmaps.Add(
                            Utilities.ScaleSKBitmap(
                                bitmap,
                                brush.BrushSize / (float)bitmap.Width));
                    }
                    else
                    {
                        SKBitmap scaledBrushBitmap = Utilities.ScaleSKBitmap(
                            bitmap,
                            brush.BrushSize / (float)bitmap.Width);

                        SKBitmap colorizedBrushBitmap =
                            Utilities.BuildColorizedBrushBitmap(
                                scaledBrushBitmap,
                                brush.Color);

                        brush?.Bitmaps.Add(colorizedBrushBitmap);
                    }
                }

            }
        }
    }
}
