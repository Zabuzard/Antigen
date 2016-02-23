using System;
using System.Collections;
using System.Collections.Generic;
using Antigen.Content;
using Antigen.Logic.Collision;
using Antigen.Map.Generation;
using Antigen.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IDrawable = Antigen.Graphics.IDrawable;

namespace Antigen.Map
{
    /// <summary>
    /// Provides images for rendering maps and data like flow, direction and walkability for each pixel.
    /// </summary>
    [Serializable]
    sealed class Map : IMapCollisionContainer, IDrawable
    {
        private readonly Random mRnd = new Random();

        /// <summary>
        /// If pixel (0,0) has this color the map provides non dynamic data with several stages for flow and angle.
        /// The data is saved in the polar-system where R-channel is flow and G-channel direction.
        /// </summary>
        private static readonly Color sStaticPolar = new Color(0, 0, 255);
        /// <summary>
        /// If pixel (0,0) has this color the map provides dynamic stageless data for flow and angle.
        /// The data is saved in the cartesian-system where R-channel is x-coordinate and G-channel
        /// y-coordinate of a (x,y) vector which represents flow as length and dir as angle.
        /// </summary>
        private static readonly Color sDynCartesian = new Color(255, 0, 0);

        private const float FlowVeryVeryFast = 2.0f,
            FlowVeryFast = 1.875f,
            FlowFast = 1.75f,
            FlowMiddleFast = 1.625f,
            FlowMiddleSlow = 1.5f,
            FlowSlow = 1.375f,
            FlowVerySlow = 1.25f,
            FlowVeryVerySlow = 1.125f,
            NoFlow = -1f;

        /// <summary>
        /// Directions in mathematically correct angles
        /// </summary>
        private const int DirNorth = 90,
            DirNorthWest = 135,
            DirWest = 180,
            DirSouthWest = 225,
            DirSouth = 270,
            DirSouthEast = 315,
            DirEast = 0,
            DirNorthEast = 45,
            NoDir = -1;

        /// <summary>
        /// Blue color code for mutationareas
        /// </summary>
        private const int MutationArea = 255;

        /// <summary>
        /// Color value of the dynamic cartesian system which stands for not walkable.
        /// </summary>
        private const int DynCartesianNotWalkable = 0;

        [NonSerialized]
        private Texture2D[,] mRenderImages;
        [NonSerialized]
        private Color[,] mDataTable;
        private Color mDataFormat;


        private readonly Hashtable mFlowTable = new Hashtable();
        private readonly Hashtable mDirTable = new Hashtable();
        private readonly Hashtable mMutationTable = new Hashtable();
        [NonSerialized]
        private MapTree mTree;

        /// <summary>
        /// The width of the map.
        /// </summary>
        private int Width { get; set; }

        /// <summary>
        /// The height of the map.
        /// </summary>
        private int Height { get; set; }

        /// <summary>
        /// The count of image splits in x direction.
        /// </summary>
        private int ImageSplitX { get; set; }

        /// <summary>
        /// The count of image splits in y direction.
        /// </summary>
        private int ImageSplitY { get; set; }

        /// <summary>
        /// The width of an splitted image.
        /// </summary>
        private int ImageSplitWidth { get; set; }

        /// <summary>
        /// The height of an splitted image.
        /// </summary>
        private int ImageSplitHeight { get; set; }

        /// <summary>
        /// Starting position of the player.
        /// </summary>
        public Vector2 PlayerStartPosition { get; private set; }
        /// <summary>
        /// Starting position of the enemy.
        /// </summary>
        public Vector2 EnemyStartPosition { get; private set; }

        /// <summary>
        /// Creates a new map which holds a rendering image and data layer.
        /// </summary>
        public Map()
        {
            CreateMapping();
        }

        /// <summary>
        /// Loads the rendering and data image. Creates the data table and checks if the map images are valid.
        /// Throws MapsCorruptException if map images are not valid.
        /// </summary>
        /// <param name="content">Content Manager to load content with</param>
        /// <param name="spriteBatch">Batch to draw with</param>
        /// <param name="seed">Seed to generate map with</param>
        public void LoadMapContent(ContentLoader content, SpriteBatch spriteBatch, int seed)
        {
            LoadContent(content, spriteBatch, seed);
            mTree = new MapTree(this);
        }

        /// <inheritdoc />
        public int GetWidth()
        {
            return Width;
        }

        /// <inheritdoc />
        public int GetHeight()
        {
            return Height;
        }

        /// <summary>
        /// Gets all map data of the pixel at once.
        /// </summary>
        /// <param name="x">X-Coord of the pixel</param>
        /// <param name="y">Y-Coord of the pixel</param>
        /// <returns>All map data of the pixel</returns>
        public MapData GetData(int x, int y)
        {
            return new MapData(GetMutationArea(x, y), GetFlow(x, y), GetDir(x, y));
        }

        /// <summary>
        /// Gets all map data of the rectangle at once.
        /// Whereas walkability will be false if one pixel in the rectangle is not walkable,
        /// for flow and dir the center of the rectangle will be used.
        /// </summary>
        /// <param name="rect">Rectangle for the map data</param>
        /// <returns>All map data of the circle at once.</returns>
        public MapData GetData(Rectangle rect)
        {
            if (IsBlocking(rect))
            {
                return new MapData(new MutationArea(global::Antigen.Map.MutationArea.MutationType.None, 0), NoFlow, NoDir);
            }

            return new MapData(GetMutationArea(rect.Center.X, rect.Center.Y),
                GetFlow(rect.Center.X, rect.Center.Y), GetDir(rect.Center.X, rect.Center.Y));
        }

        /// <inheritdoc />
        public bool IsBlocking(Rectangle rect)
        {
            return mTree.IsBlocking(rect);
        }

        /// <inheritdoc />
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            for (int x = 0; x < ImageSplitX; x++)
            {
                for (int y = 0; y < ImageSplitY; y++)
                {
                    spriteBatch.Draw(mRenderImages[x, y], new Vector2(x * ImageSplitWidth, y * ImageSplitHeight), Color.White);
                }
            }
        }

        /// <summary>
        /// Returns if the pixel is walkable.
        /// </summary>
        /// <param name="x">X-Coord of the pixel</param>
        /// <param name="y">Y-Coord of the pixel</param>
        /// <returns>If the pixel is walkable</returns>
        public bool IsWalkable(int x, int y)
        {
            var pixelColor = mDataTable[x, y];
            if (mDataFormat == sStaticPolar)
            {
                bool hasFlow = !((float)mFlowTable[(int)pixelColor.R]).Equals(NoFlow);
                bool hasDir = !((int)mDirTable[(int)pixelColor.G]).Equals(NoDir);
                return hasFlow && hasDir;
            }
            if (mDataFormat == sDynCartesian)
            {
                bool walkableRed = pixelColor.R != DynCartesianNotWalkable;
                bool walkableGreen = pixelColor.G != DynCartesianNotWalkable;
                return walkableRed && walkableGreen;
            }
            return false;
        }

        /// <summary>
        /// Returns a list of random walkable
        /// points on the map with size of amount.
        /// </summary>
        /// <param name="amount">Amount of random walkable points</param>
        /// <returns>List of random walkable points</returns>
        public IEnumerable<Point> RandomWalkablePoints(int amount)
        {
            var result = new List<Point>();
            var resultSet = new HashSet<Point>();
            do
            {
                int x = mRnd.Next(0, Width);
                int y = mRnd.Next(0, Height);

                if (!IsWalkable(x, y))
                {
                    continue;
                }

                var p = new Point(x, y);
                if (resultSet.Add(p))
                {
                    result.Add(p);
                }
            } while (result.Count < amount);
            return result;
        }

        /// <summary>
        /// Returns the flow of the pixel.
        /// </summary>
        /// <param name="x">X-Coord of the pixel</param>
        /// <param name="y">Y-Coord of the pixel</param>
        /// <returns>The flow of the pixel from 1 (basic) to 2 (fast) or NoFlow if there is no</returns>
        private float GetFlow(int x, int y)
        {
            if (x < 0 || y < 0 || x > mDataTable.GetUpperBound(0) || y > mDataTable.GetUpperBound(1))
            {
                return NoFlow;
            }

            var pixelColor = mDataTable[x, y];
            if (mDataFormat == sStaticPolar)
            {
                return (float)mFlowTable[(int)pixelColor.R];
            }
            if (mDataFormat == sDynCartesian)
            {
                if (pixelColor.R == DynCartesianNotWalkable || pixelColor.G == DynCartesianNotWalkable)
                {
                    return NoFlow;
                }
                //Cartesian vector coordinates from [1,255] to [-127,127]
                int xCart = pixelColor.R - 128;
                int yCart = pixelColor.G - 128;

                var radius = Functions.CartesianToPolar(xCart, yCart).X;
                var maxRadius = Math.Sqrt(127 * 127 * 2);

                //Radius from [1,2] instead of [0, maxRadius]
                return (float) ((radius / maxRadius) + 1);
            }
            return NoFlow;
        }

        /// <summary>
        /// Returns the direction of the pixel.
        /// </summary>
        /// <param name="x">X-Coord of the pixel</param>
        /// <param name="y">Y-Coord of the pixel</param>
        /// <returns>The direction of the pixel in degrees (0 to 359)</returns>
        private int GetDir(int x, int y)
        {
            if (x < 0 || y < 0 || x > mDataTable.GetUpperBound(0) || y > mDataTable.GetUpperBound(1))
            {
                return NoDir;
            }

            var pixelColor = mDataTable[x, y];
            if (mDataFormat == sStaticPolar)
            {
                return (int)mDirTable[(int)pixelColor.G];
            }
            if (mDataFormat == sDynCartesian)
            {
                if (pixelColor.R == DynCartesianNotWalkable || pixelColor.G == DynCartesianNotWalkable)
                {
                    return NoDir;
                }
                //Cartesian vector coordinates from [1,255] to [-127,127]
                int xCart = pixelColor.R - 128;
                int yCart = pixelColor.G - 128;

                var angle = (int) Functions.CartesianToPolar(xCart, yCart).Y;

                return angle;
            }
            return NoDir;
        }

        /// <summary>
        /// Returns the mutation area at given coordinates.
        /// </summary>
        /// <param name="x">X-coord of the pixel</param>
        /// <param name="y">Y-coord of the pixel</param>
        /// <returns>The mutation area at the given position</returns>
        private MutationArea GetMutationArea(int x, int y)
        {
            if (x < 0 || y < 0 || x > mDataTable.GetUpperBound(0) || y > mDataTable.GetUpperBound(1))
            {
                return new MutationArea(global::Antigen.Map.MutationArea.MutationType.None, 0);
            }

            int pixelBlue = mDataTable[x, y].B;
            foreach (var color in mMutationTable.Keys)
            {
                if (pixelBlue >= (int)color && pixelBlue <= (int)color + global::Antigen.Map.MutationArea.MaxStrength)
                {
                    return new MutationArea((MutationArea.MutationType)mMutationTable[color], pixelBlue - (int)color);
                }
            }
            return new MutationArea(global::Antigen.Map.MutationArea.MutationType.None, 0);
        }

        /// <summary>
        /// Maps the flow and dir tables from the corresponding colors to values.
        /// </summary>
        private void CreateMapping()
        {
            mFlowTable.Add(100, FlowVeryVeryFast);
            mFlowTable.Add(120, FlowVeryFast);
            mFlowTable.Add(140, FlowFast);
            mFlowTable.Add(160, FlowMiddleFast);
            mFlowTable.Add(180, FlowMiddleSlow);
            mFlowTable.Add(200, FlowSlow);
            mFlowTable.Add(220, FlowVerySlow);
            mFlowTable.Add(240, FlowVeryVerySlow);
            mFlowTable.Add(0, NoFlow);

            mDirTable.Add(100, DirNorth);
            mDirTable.Add(120, DirNorthWest);
            mDirTable.Add(140, DirWest);
            mDirTable.Add(160, DirSouthWest);
            mDirTable.Add(180, DirSouth);
            mDirTable.Add(200, DirSouthEast);
            mDirTable.Add(220, DirEast);
            mDirTable.Add(240, DirNorthEast);
            mDirTable.Add(0, NoDir);

            mMutationTable.Add(8, global::Antigen.Map.MutationArea.MutationType.Sight);
            mMutationTable.Add(39, global::Antigen.Map.MutationArea.MutationType.InfectionResistance);
            mMutationTable.Add(70, global::Antigen.Map.MutationArea.MutationType.CellDivisionRate);
            mMutationTable.Add(101, global::Antigen.Map.MutationArea.MutationType.MovementSpeed);
            mMutationTable.Add(132, global::Antigen.Map.MutationArea.MutationType.AttackPower);
            mMutationTable.Add(163, global::Antigen.Map.MutationArea.MutationType.Lifespan);
            mMutationTable.Add(194, global::Antigen.Map.MutationArea.MutationType.Lifepoints);
            mMutationTable.Add(225, global::Antigen.Map.MutationArea.MutationType.Average);
        }

        /// <summary>
        /// Loads the rendering and data image. Creates the data table and checks if the map images are valid.
        /// Throws MapsCorruptException if map images are not valid.
        /// </summary>
        /// <param name="contentLoader">Loader to load content with</param>
        /// <param name="spriteBatch">Batch to draw with</param>
        /// <param name="seed">Seed of the map to generate</param>
        private void LoadContent(ContentLoader contentLoader, SpriteBatch spriteBatch, int seed)
        {
            var generatedMapdata = MapGenerator.GenerateMap(seed, spriteBatch, contentLoader);
            ImageSplitX = generatedMapdata.TilesCount;
            ImageSplitY = generatedMapdata.TilesCount;
            ImageSplitWidth = generatedMapdata.TileSize;
            ImageSplitHeight = generatedMapdata.TileSize;
            mRenderImages = new Texture2D[ImageSplitX, ImageSplitY];
            for (int x = 0; x < ImageSplitX; x++)
            {
                for (int y = 0; y < ImageSplitY; y++)
                {
                    mRenderImages[x, y] = MapGenerator.GetRenderMapTile(x, y, spriteBatch);
                }
            }
            PlayerStartPosition = generatedMapdata.PlayerStart;
            EnemyStartPosition = generatedMapdata.EnemyStart;
            Width = generatedMapdata.TileSize * generatedMapdata.TilesCount;
            Height = generatedMapdata.TileSize * generatedMapdata.TilesCount;
            mDataTable = new Color[Width, Height];

            //Convert single data tables and merge them into mDataTable
            for (int outerX = 0; outerX < ImageSplitX; outerX++)
            {
                for (int outerY = 0; outerY < ImageSplitY; outerY++)
                {
                    bool dataIdentifier = outerX == 0 && outerY == 0;

                    if (dataIdentifier)
                    {
                        var texture = MapGenerator.GetDataMapTile(0, 0, spriteBatch);
                        var table1D = new Color[texture.Width * texture.Height];
                        texture.GetData(table1D);
                        mDataFormat = sDynCartesian;
                    }

                    var dataTable = ConvertTex2Table(MapGenerator.GetDataMapTile(outerX, outerY, spriteBatch), dataIdentifier);
                 
                    int relativeX = 0;
                    int relativeY = 0;
                    for (int absoluteX = outerX * ImageSplitWidth; absoluteX < (outerX + 1) * ImageSplitWidth; absoluteX++)
                    {
                        for (int absoluteY = outerY * ImageSplitHeight; absoluteY < (outerY + 1) * ImageSplitHeight; absoluteY++)
                        {
                            mDataTable[absoluteX, absoluteY] = dataTable[relativeX, relativeY];
                            relativeY++;
                        }
                        relativeY = 0;
                        relativeX++;
                    }
                    GC.Collect();
                }
            }           
        }

        /// <summary>
        /// Converts a Texture2D image to a Color[x, y] matrix and checks if the image data is valid.
        /// Throws MapCorruptException if image data are not valid.
        /// </summary>
        /// <param name="texture">Texture to convert to</param>
        /// <param name="dataIdentifier">True if the texture is a data format identifier where pixel (0,0) is special</param>
        /// <returns></returns>
        private Color[,] ConvertTex2Table(Texture2D texture, bool dataIdentifier)
        {
            var table1D = new Color[texture.Width * texture.Height];
            texture.GetData(table1D);

            var table2D = new Color[texture.Width, texture.Height];

            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    table2D[x, y] = table1D[x + y * texture.Width];
                    var clr = table2D[x, y];

                    //Check if data is valid
                    if (x == 0 && y == 0 & dataIdentifier)
                    {
                        clr = sDynCartesian;
                        if (mDataFormat != clr)
                        {
                            throw new MapCorruptException("Expected data format does not match given format.");
                        }
                        if (clr != sStaticPolar && clr != sDynCartesian)
                        {
                            throw new MapCorruptException("Data format is unknown.");
                        }
                        //Don't do more checks with special pixel (0,0)
                        continue;
                    }

                    var validRed = true;
                    var validGreen = true;
                    var validBlue = true;
                    if (mDataFormat == sStaticPolar)
                    {
                        validRed = mFlowTable.ContainsKey((int)clr.R);
                        validGreen = mDirTable.ContainsKey((int)clr.G);
                        validBlue = (clr.B == 0) || (clr.B == MutationArea);
                    }
                    //In DynCartesian all values are valid
                    if (!(validRed && validGreen && validBlue))
                    {
                        throw new MapCorruptException("Pixel [" + x + "," + y + "] is corrupt. R:" + clr.R + ", G:" + clr.G + ", B:" + clr.B);
                    }
                }
            }
            return table2D;
        }
    }
}
