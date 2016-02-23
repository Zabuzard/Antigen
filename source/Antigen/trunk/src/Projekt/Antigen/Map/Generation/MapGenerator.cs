using System;
using System.Collections.Generic;
using System.Linq;
using Antigen.Content;
using Antigen.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Antigen.Map.Generation
{
    static class MapGenerator
    {
        private const int MaxTriesNewCirclePosition = 10;
        private const int SideBloodStreamValueCount = 10;       
        private const int SideBloodStreamResolution = 1000;
        private const int SideBloodStreamCount = 3;
        private const int Size = 4000;
        private const int TileSize = 2000;
        private const int MainBloodStreamCount = 10;
        private const int MinRadius = 700;
        private const int MaxRadius = 1500;
        private const int MainBloodStreamResolution = 1000;       
        private const int MutationTypeCount = 8;
        private const int MutationTypeStart = 38;
        private const int MutationTypeMaxStrength = 30;
        private static Tuple<RenderTarget2D[,], RenderTarget2D[,]> sMap; 
        private static readonly Range sSideBloodStreamWidth = new Range(50, 100);
        private static readonly Range sMainBloodStreamWidth = new Range(150, 200);
        private static readonly Range sMutationAreaCount = new Range(10, 20);
        private static readonly Range sCircleCount = new Range(5, 10);
        private static readonly Range sCircleRadius = new Range(50, 500);
        private static readonly Range sCircleDistance = new Range(50, 500);

        private static void DrawBloodStream(PerlinCircle bloodStream, PerlinLine width, float widthFactor, int resolution, float speed, Color color, bool directionAsColor, GraphicsDevice graphicsDevice)
        {
            var bloodStreamData = new VertexPositionColor[resolution * 2 + 2];
            for (var i = 0; i <= resolution; i++)
            {
                
                var radians = (float)i / resolution * 2 * Math.PI;
                var rotatedVector = Vector2.Transform(bloodStream.GetDirection(radians),
                    Matrix.CreateRotationZ((float)Math.PI / 2));
                var vectorA = bloodStream.GetPoint(radians) + rotatedVector * (float) width.GetValue(radians) * widthFactor;
                var vectorB = bloodStream.GetPoint(radians) - rotatedVector * (float) width.GetValue(radians) * widthFactor;
                var directionColor =
                        new Color(new Vector4(bloodStream.GetDirection(radians) * new Vector2(0.5f, -0.5f) * speed + new Vector2(0.5f), 0, 0.5f));
                bloodStreamData[i * 2].Color = directionAsColor ? directionColor: color;
                bloodStreamData[i * 2].Position = new Vector3(vectorA, 0);
                bloodStreamData[i * 2 + 1].Color = directionAsColor ? directionColor : color;
                bloodStreamData[i * 2 + 1].Position = new Vector3(vectorB, 0);
            }
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip,
                bloodStreamData,
                0,
                bloodStreamData.Length - 2);
        }

        private static void DrawMutationAreas(IEnumerable<Circle> circles, SpriteBatch spriteBatch, Texture2D circleTexture, Color color)
        {
            foreach (var circle in circles)
            {
                spriteBatch.Draw(circleTexture,
                            circle.Position,
                            null,
                            color == Color.Black ? circle.Color : color,
                            0,
                            new Vector2(circleTexture.Width / 2f, circleTexture.Height / 2f),
                            (float)circle.Radius * 2 / circleTexture.Width,
                            SpriteEffects.None,
                            0);
            }
        }

        private static IEnumerable<Circle> GenerateMutationArea(Random random, Range circleCount, Range radius, Range distance, HashSet<Circle> otherCircles)
        {
            var lastPosition = new Vector2((float)random.NextDouble() * Size, (float)random.NextDouble() * Size);
            var result = new HashSet<Circle>();
            var color = new Color(0, 0, MutationTypeStart + MutationTypeMaxStrength * random.Next(MutationTypeCount));
            for (var i = 0; i < circleCount.GetRandomInt(random); i++)
            {               
                var thisRadius = radius.GetRandomDouble(random);
                Vector2 position;
                var tries = 0;
                do
                {
                    var radians = random.NextDouble() * 2 * Math.PI;
                    var thisDistance = (float) distance.GetRandomDouble(random);
                    position = lastPosition +
                               new Vector2((float) Math.Cos(radians) * thisDistance,
                                   (float) Math.Sin(radians) * thisDistance);
                    tries++;
                } while (otherCircles.Any(circle => circle.Position.Distance(position) <= circle.Radius + thisRadius) && tries < MaxTriesNewCirclePosition);
                
                if (tries < MaxTriesNewCirclePosition) result.Add(new Circle(color, position, thisRadius));
            }
            return result;
        }

        public static Texture2D GetRenderMapTile(int x, int y, SpriteBatch spriteBatch)
        {
            var data = new Color[TileSize * TileSize];
            sMap.Item2[x, y].GetData(data);
            var renderTexture = new Texture2D(spriteBatch.GraphicsDevice, TileSize, TileSize);
            renderTexture.SetData(data);
            return renderTexture;
        }

        public static Texture2D GetDataMapTile(int x, int y, SpriteBatch spriteBatch)
        {
            var data = new Color[TileSize * TileSize];
            sMap.Item1[x, y].GetData(data);
            var dataTexture = new Texture2D(spriteBatch.GraphicsDevice, TileSize, TileSize);
            dataTexture.SetData(data);
            return dataTexture;
        }

        public static GeneratedMapData GenerateMap(int seed, SpriteBatch spriteBatch, ContentLoader contentLoader)
        {
            var graphicsDevice = spriteBatch.GraphicsDevice;
            var random = new Random(seed);
            var boneTexture = contentLoader.LoadTexture("bone");
            var bloodTexture = contentLoader.LoadTexture("blood");
            var circleTexture = contentLoader.LoadTexture("Circle");

            
            
            var circles = new HashSet<Circle>();
            for (var i = 0; i < sMutationAreaCount.GetRandomInt(random); i++)
            {
                circles.UnionWith(GenerateMutationArea(random, sCircleCount, sCircleRadius, sCircleDistance, circles));
            }

            var mainBloodStream = new PerlinCircle(random,
                        MainBloodStreamCount,
                        new Range(MinRadius, MaxRadius));
            var mainBloodStreamRadius = new PerlinLine(random, MainBloodStreamCount, sMainBloodStreamWidth, 2 * Math.PI);
            var sideBloodStreams = new List<Tuple<PerlinCircle, PerlinLine>>();          
            for (var i = 0; i < SideBloodStreamCount; i++)
            {
                sideBloodStreams.Add(
                    new Tuple<PerlinCircle, PerlinLine>(
                        new PerlinCircle(random, SideBloodStreamValueCount, new Range(MinRadius, MaxRadius)),
                        new PerlinLine(random, SideBloodStreamValueCount, sSideBloodStreamWidth, 2 * Math.PI)));
            }
            
            var basicEffect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                World = Matrix.CreateTranslation(Size / 2, Size / 2, 0)
            };
            var rasterizerState = new RasterizerState {CullMode = CullMode.None};
            graphicsDevice.RasterizerState = rasterizerState;

            var map =
                new Tuple<RenderTarget2D[,], RenderTarget2D[,]>(
                    new RenderTarget2D[(Size - 1) / TileSize + 1, (Size - 1) / TileSize + 1],
                    new RenderTarget2D[(Size - 1) / TileSize + 1, (Size - 1) / TileSize + 1]);
            for (var x = 0; x * TileSize < Size; x++)
            {
                for (var y = 0; y * TileSize < Size; y++)
                {
                    basicEffect.Projection = Matrix.CreateOrthographicOffCenter(x * TileSize,
                        (x + 1) * TileSize,
                        (y + 1) * TileSize,
                        y * TileSize,
                        -1,
                        1);

                    var mutationAreaTarget = new RenderTarget2D(graphicsDevice, TileSize, TileSize);
                    graphicsDevice.SetRenderTarget(mutationAreaTarget);
                    graphicsDevice.Clear(Color.Black);
                    spriteBatch.Begin(SpriteSortMode.Deferred,
                        BlendState.AlphaBlend,
                        null,
                        null,
                        null,
                        null,
                        Matrix.CreateTranslation(-TileSize * x, -TileSize * y, 0));
                    DrawMutationAreas(circles, spriteBatch, circleTexture, Color.Black); 
                    spriteBatch.End();

                    map.Item1[x, y] = new RenderTarget2D(graphicsDevice, TileSize, TileSize);
                    graphicsDevice.SetRenderTarget(map.Item1[x, y]);
                  
                    


                    basicEffect.CurrentTechnique.Passes[0].Apply();
                    graphicsDevice.BlendState = new BlendState
                    {
                        AlphaDestinationBlend = Blend.InverseDestinationAlpha,
                        ColorDestinationBlend = Blend.InverseDestinationAlpha,
                        AlphaSourceBlend = Blend.DestinationAlpha,
                        ColorSourceBlend = Blend.DestinationAlpha,
                    };

                                      
                    DrawDataMap(graphicsDevice, mainBloodStream, mainBloodStreamRadius, sideBloodStreams);

                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
                    spriteBatch.Draw(mutationAreaTarget, new Rectangle(0, 0, TileSize, TileSize), Color.White);
                    spriteBatch.End();

                    mutationAreaTarget = new RenderTarget2D(graphicsDevice, TileSize, TileSize);
                    graphicsDevice.SetRenderTarget(mutationAreaTarget);
                    graphicsDevice.Clear(Color.Black);
                    spriteBatch.Begin(SpriteSortMode.Deferred,
                        BlendState.AlphaBlend,
                        null,
                        null,
                        null,
                        null,
                        Matrix.CreateTranslation(-TileSize * x, -TileSize * y, 0));
                    DrawMutationAreas(circles, spriteBatch, circleTexture, new Color(0, 0, 0.5f));
                    spriteBatch.End();

                    basicEffect.CurrentTechnique.Passes[0].Apply();
                    map.Item2[x, y] = new RenderTarget2D(graphicsDevice, TileSize, TileSize);
                    graphicsDevice.SetRenderTarget(map.Item2[x, y]);

                    graphicsDevice.Clear(Color.Transparent);
                    

                    foreach (var sideBloodStream in sideBloodStreams)
                    {
                        DrawBloodStream(sideBloodStream.Item1, sideBloodStream.Item2, 1, SideBloodStreamResolution, 1, Color.White, false, graphicsDevice);
                    }
                    DrawBloodStream(mainBloodStream, mainBloodStreamRadius, 1, MainBloodStreamResolution, 1, Color.White, false, graphicsDevice);

                    var blendState = new BlendState
                    {
                        AlphaDestinationBlend = Blend.Zero,
                        ColorDestinationBlend = Blend.Zero,
                        AlphaSourceBlend = Blend.InverseDestinationAlpha,
                        ColorSourceBlend = Blend.InverseDestinationAlpha,
                    };
                    spriteBatch.Begin(SpriteSortMode.Deferred,
                        blendState,
                        null,
                        null,
                        null,
                        null,
                        Matrix.CreateTranslation(-TileSize * x, -TileSize * y, 0));
                    spriteBatch.Draw(boneTexture, new Rectangle(0, 0, Size, Size), Color.Brown);
                    spriteBatch.End();

                    blendState = new BlendState
                    {
                        AlphaDestinationBlend = Blend.DestinationAlpha,
                        ColorDestinationBlend = Blend.DestinationAlpha,
                        AlphaSourceBlend = Blend.InverseDestinationAlpha,
                        ColorSourceBlend = Blend.InverseDestinationAlpha,
                    };
                    spriteBatch.Begin(SpriteSortMode.Deferred,
                        blendState,
                        null,
                        null,
                        null,
                        null,
                        Matrix.CreateTranslation(-TileSize * x, -TileSize * y, 0));
                    spriteBatch.Draw(bloodTexture, new Rectangle(0, 0, Size, Size), Color.White);
                    spriteBatch.End();

                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
                    spriteBatch.Draw(mutationAreaTarget, new Rectangle(0, 0, TileSize, TileSize), Color.White);
                    spriteBatch.End();
                }
            }

            graphicsDevice.SetRenderTarget(null);

            var radians = random.NextDouble() * 2 * Math.PI;
            var playerStart = mainBloodStream.GetPoint(radians) + new Vector2(Size / 2);
            var enemyStart = mainBloodStream.GetPoint(radians + Math.PI) + new Vector2(Size / 2);
            sMap = map;
            return new GeneratedMapData(playerStart, enemyStart, map.Item1.GetLength(0), TileSize);
        }

        private static void DrawDataMap(GraphicsDevice graphicsDevice, PerlinCircle mainBloodStream, PerlinLine mainBloodStreamRadius, IEnumerable<Tuple<PerlinCircle, PerlinLine>> sideBloodStreams)
        {
            graphicsDevice.Clear(new Color(0, 0, 0, 255));             

                    foreach (var sideBloodStream in sideBloodStreams)
                    {
                        DrawBloodStream(sideBloodStream.Item1, sideBloodStream.Item2, 1, SideBloodStreamResolution, 0.5f, Color.White, true, graphicsDevice);
                        DrawBloodStream(sideBloodStream.Item1, sideBloodStream.Item2, 0.75f, SideBloodStreamResolution, 0.75f, Color.White, true, graphicsDevice);
                        DrawBloodStream(sideBloodStream.Item1, sideBloodStream.Item2, 0.5f, SideBloodStreamResolution, 1f, Color.White, true, graphicsDevice);
                    }

                    DrawBloodStream(mainBloodStream, mainBloodStreamRadius, 1, MainBloodStreamResolution, 0.5f, Color.White, true, graphicsDevice);
                    DrawBloodStream(mainBloodStream, mainBloodStreamRadius, 0.75f, MainBloodStreamResolution, 0.75f, Color.White, true, graphicsDevice);
                    DrawBloodStream(mainBloodStream, mainBloodStreamRadius, 0.5f, MainBloodStreamResolution, 1f, Color.White, true, graphicsDevice);
        }
    }
}
