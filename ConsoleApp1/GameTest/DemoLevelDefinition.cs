using System.Collections.Generic;
using System.Drawing;

namespace GameTest
{
    class DemoTilePlacement
    {
        public string Texture { get; set; }
        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }
        public int SourceX { get; set; }
        public int SourceY { get; set; }
        public int SourceWidth { get; set; }
        public int SourceHeight { get; set; }
        public int Column { get; set; }
        public int Row { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Scale { get; set; }
    }

    class DemoPickupPlacement
    {
        public string ClipId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Scale { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public int Value { get; set; }
    }

    class DemoEnemyPlacement
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float MinX { get; set; }
        public float MaxX { get; set; }
        public float Speed { get; set; }
        public float Scale { get; set; }
    }

    class DemoMushroomPlacement
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Scale { get; set; }
        public string SoundAsset { get; set; }
        public float MaxDistance { get; set; }
    }

    class DemoGroundBand
    {
        public RectangleF Bounds { get; set; }
        public int TopHeight { get; set; }
        public Color TopColor { get; set; }
        public Color FillColor { get; set; }
        public Color ShadowColor { get; set; }
    }

    class DemoLevelDefinition
    {
        public int LevelNumber { get; set; }
        public string Title { get; set; }
        public string MusicAsset { get; set; }
        public Color BackgroundColor { get; set; }
        public Color AccentColor { get; set; }
        public float PlayerSpawnX { get; set; }
        public float PlayerSpawnY { get; set; }
        public RectangleF ExitBounds { get; set; }
        public DemoTilePlacement ExitVisual { get; set; }
        public List<DemoTilePlacement> BackgroundTiles { get; set; } = new List<DemoTilePlacement>();
        public List<DemoTilePlacement> Tiles { get; set; } = new List<DemoTilePlacement>();
        public List<DemoGroundBand> GroundBands { get; set; } = new List<DemoGroundBand>();
        public List<RectangleF> Solids { get; set; } = new List<RectangleF>();
        public List<DemoPickupPlacement> Pickups { get; set; } = new List<DemoPickupPlacement>();
        public List<DemoEnemyPlacement> Enemies { get; set; } = new List<DemoEnemyPlacement>();
        public List<DemoMushroomPlacement> Mushrooms { get; set; } = new List<DemoMushroomPlacement>();
    }

    static class DemoLevelCatalog
    {
        private const float TileScale = 4.0f;
        private const float TileSize = 64.0f;
        private const float PlatformSolidHeight = 36.0f;
        private const float SceneWidth = 20 * TileSize;
        private const float SceneHeight = 864.0f;
        private const float BoundaryThickness = TileSize;

        public static DemoLevelDefinition GetLevel(int levelNumber)
        {
            return levelNumber == 1 ? buildLevelOne() : buildLevelTwo();
        }

        private static DemoLevelDefinition buildLevelOne()
        {
            DemoLevelDefinition level = new DemoLevelDefinition
            {
                LevelNumber = 1,
                Title = "Level 1 - Meadow Run",
                MusicAsset = "bgm1.wav",
                BackgroundColor = Color.FromArgb(118, 202, 242),
                AccentColor = Color.FromArgb(221, 243, 255),
                PlayerSpawnX = 96,
                PlayerSpawnY = 724
            };

            addGroundBand(
                level,
                0,
                800,
                SceneWidth,
                64,
                18,
                Color.FromArgb(94, 191, 83),
                Color.FromArgb(144, 104, 63),
                Color.FromArgb(103, 71, 42));
            addSolid(level, 0, 800, SceneWidth, PlatformSolidHeight);
            addSceneBounds(level);
            addPlatform(level, 3, 672, 4, 0);
            addPlatform(level, 8, 608, 3, 0);
            addPlatform(level, 13, 544, 3, 0);
            addPlatform(level, 16, 672, 3, 0);

            addPalmTree(level, 696, 544);

            level.Pickups.Add(new DemoPickupPlacement { ClipId = "demo.coin.spin", X = 260, Y = 618, Scale = 3.0f, Width = 40, Height = 40, Value = 100 });
            level.Pickups.Add(new DemoPickupPlacement { ClipId = "demo.coin.spin", X = 360, Y = 618, Scale = 3.0f, Width = 40, Height = 40, Value = 100 });
            level.Pickups.Add(new DemoPickupPlacement { ClipId = "demo.coin.spin", X = 548, Y = 554, Scale = 3.0f, Width = 40, Height = 40, Value = 100 });
            level.Pickups.Add(new DemoPickupPlacement { ClipId = "demo.coin.spin", X = 972, Y = 490, Scale = 3.0f, Width = 40, Height = 40, Value = 100 });
            level.Pickups.Add(new DemoPickupPlacement { ClipId = "demo.fruit.static", X = 1120, Y = 618, Scale = 3.2f, Width = 40, Height = 40, Value = 250 });

            level.Enemies.Add(new DemoEnemyPlacement { X = 254, Y = 614, MinX = 206, MaxX = 326, Speed = 85, Scale = 2.5f });
            level.Enemies.Add(new DemoEnemyPlacement { X = 878, Y = 486, MinX = 846, MaxX = 918, Speed = 95, Scale = 2.5f });

            level.ExitBounds = new RectangleF(1152, 704, 56, 96);
            level.ExitVisual = new DemoTilePlacement
            {
                Texture = "fruit.png",
                FrameWidth = 16,
                FrameHeight = 16,
                Column = 0,
                Row = 3,
                X = 1144,
                Y = 704,
                Scale = 4.0f
            };

            return level;
        }

        private static DemoLevelDefinition buildLevelTwo()
        {
            DemoLevelDefinition level = new DemoLevelDefinition
            {
                LevelNumber = 2,
                Title = "Level 2 - Cavern Relay",
                MusicAsset = "bgm2.wav",
                BackgroundColor = Color.FromArgb(38, 52, 90),
                AccentColor = Color.FromArgb(118, 143, 206),
                PlayerSpawnX = 96,
                PlayerSpawnY = 724
            };

            addGroundBand(
                level,
                0,
                800,
                SceneWidth,
                64,
                18,
                Color.FromArgb(206, 181, 131),
                Color.FromArgb(130, 88, 64),
                Color.FromArgb(86, 56, 44));
            addSolid(level, 0, 800, SceneWidth, PlatformSolidHeight);
            addSceneBounds(level);
            addPlatform(level, 4, 688, 3, 1);
            addPlatform(level, 9, 592, 3, 1);
            addPlatform(level, 13, 496, 3, 1);
            addPlatform(level, 16, 368, 3, 1);

            level.Pickups.Add(new DemoPickupPlacement { ClipId = "demo.coin.spin", X = 324, Y = 634, Scale = 3.0f, Width = 40, Height = 40, Value = 100 });
            level.Pickups.Add(new DemoPickupPlacement { ClipId = "demo.fruit.static", X = 676, Y = 538, Scale = 3.2f, Width = 40, Height = 40, Value = 250 });
            level.Pickups.Add(new DemoPickupPlacement { ClipId = "demo.coin.spin", X = 996, Y = 442, Scale = 3.0f, Width = 40, Height = 40, Value = 100 });
            level.Pickups.Add(new DemoPickupPlacement { ClipId = "demo.fruit.static", X = 1180, Y = 314, Scale = 3.2f, Width = 40, Height = 40, Value = 250 });

            level.Enemies.Add(new DemoEnemyPlacement { X = 300, Y = 744, MinX = 224, MaxX = 384, Speed = 100, Scale = 2.5f });
            level.Enemies.Add(new DemoEnemyPlacement { X = 656, Y = 744, MinX = 576, MaxX = 736, Speed = 110, Scale = 2.5f });
            level.Enemies.Add(new DemoEnemyPlacement { X = 878, Y = 438, MinX = 846, MaxX = 918, Speed = 120, Scale = 2.5f });

            level.Mushrooms.Add(new DemoMushroomPlacement
            {
                X = 676,
                Y = 536,
                Scale = 3.0f,
                SoundAsset = "beep_loop.wav",
                MaxDistance = 400.0f
            });

            level.ExitBounds = new RectangleF(1184, 272, 56, 96);
            level.ExitVisual = new DemoTilePlacement
            {
                Texture = "fruit.png",
                FrameWidth = 16,
                FrameHeight = 16,
                Column = 2,
                Row = 1,
                X = 1176,
                Y = 272,
                Scale = 4.0f
            };

            return level;
        }

        private static void addGroundBand(DemoLevelDefinition level, float x, float y, float width, float height, int topHeight, Color topColor, Color fillColor, Color shadowColor)
        {
            level.GroundBands.Add(new DemoGroundBand
            {
                Bounds = new RectangleF(x, y, width, height),
                TopHeight = topHeight,
                TopColor = topColor,
                FillColor = fillColor,
                ShadowColor = shadowColor
            });
        }

        private static void addSolid(DemoLevelDefinition level, float x, float y, float width, float height)
        {
            level.Solids.Add(new RectangleF(x, y, width, height));
        }

        private static void addSceneBounds(DemoLevelDefinition level)
        {
            addSolid(level, -BoundaryThickness, 0, BoundaryThickness, SceneHeight);
            addSolid(level, SceneWidth, 0, BoundaryThickness, SceneHeight);
        }

        private static void addPlatform(DemoLevelDefinition level, int startColumn, float y, int tileCount, int platformRow)
        {
            addSolid(level, startColumn * TileSize, y, tileCount * TileSize, PlatformSolidHeight);

            for (int i = 0; i < tileCount; i++)
            {
                int textureColumn = 1;
                if (i == 0)
                {
                    textureColumn = 0;
                }
                else if (i == tileCount - 1)
                {
                    textureColumn = 2;
                }

                level.Tiles.Add(new DemoTilePlacement
                {
                    Texture = "platforms.png",
                    FrameWidth = 16,
                    FrameHeight = 16,
                    SourceX = textureColumn * 16,
                    SourceY = platformRow * 16,
                    SourceWidth = 16,
                    SourceHeight = 9,
                    Column = textureColumn,
                    Row = platformRow,
                    X = (startColumn + i) * TileSize,
                    Y = y,
                    Scale = TileScale
                });
            }
        }

        private static void addPalmTree(DemoLevelDefinition level, float x, float y)
        {
            addBackdropTile(level, x, y - TileSize, 3, 4, 4.0f);
            addBackdropTile(level, x - TileSize, y, 2, 5, 4.0f);
            addBackdropTile(level, x, y, 3, 5, 4.0f);
            addBackdropTile(level, x + TileSize, y, 4, 5, 4.0f);
            addBackdropTile(level, x, y + TileSize, 3, 6, 4.0f);
            addBackdropTile(level, x, y + (TileSize * 2), 3, 7, 4.0f);
            addBackdropTile(level, x, y + (TileSize * 3), 3, 8, 4.0f);
        }

        private static void addBackdropPlatform(DemoLevelDefinition level, float startX, float y, int tileCount, int platformRow, float scale)
        {
            for (int i = 0; i < tileCount; i++)
            {
                int textureColumn = 1;
                if (i == 0)
                {
                    textureColumn = 0;
                }
                else if (i == tileCount - 1)
                {
                    textureColumn = 2;
                }

                level.BackgroundTiles.Add(new DemoTilePlacement
                {
                    Texture = "platforms.png",
                    FrameWidth = 16,
                    FrameHeight = 16,
                    SourceX = textureColumn * 16,
                    SourceY = platformRow * 16,
                    SourceWidth = 16,
                    SourceHeight = 9,
                    Column = textureColumn,
                    Row = platformRow,
                    X = startX + (i * 16.0f * scale),
                    Y = y,
                    Scale = scale
                });
            }
        }

        private static void addBackdropTile(DemoLevelDefinition level, float x, float y, int textureColumn, int textureRow, float scale)
        {
            level.BackgroundTiles.Add(new DemoTilePlacement
            {
                Texture = "world_tileset.png",
                FrameWidth = 16,
                FrameHeight = 16,
                Column = textureColumn,
                Row = textureRow,
                X = x,
                Y = y,
                Scale = scale
            });
        }

        private static void addWorldTile(DemoLevelDefinition level, int xColumn, float y, int textureColumn, int textureRow, float scale)
        {
            level.Tiles.Add(new DemoTilePlacement
            {
                Texture = "world_tileset.png",
                FrameWidth = 16,
                FrameHeight = 16,
                Column = textureColumn,
                Row = textureRow,
                X = xColumn * TileSize,
                Y = y,
                Scale = scale
            });
        }
    }
}
