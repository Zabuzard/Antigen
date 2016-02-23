using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HomeworkAssignment
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public sealed class Game1 : Game
    {
        readonly GraphicsDeviceManager mGraphics;
        SpriteBatch mSpriteBatch;
        private Texture2D mBackground;
        private Texture2D mLogo;
        private const int ScreenWidth = 640;
        private const int ScreenHeight = 480;
        private float mXLogo;
        private float mYLogo;
        private int mAngle;
        private const int Radius = 125;
        private const float Scale = 0.2f;
        private SoundEffect mLogoHit;
        private SoundEffect mLogoMiss;
       
        public Game1()
        {
            mGraphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            mGraphics.PreferredBackBufferWidth = ScreenWidth;
            mGraphics.PreferredBackBufferHeight = ScreenHeight;
            mGraphics.IsFullScreen = false;
            mGraphics.ApplyChanges();
            IsMouseVisible = true;
            Window.Title = "Homework Assignment";

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            mSpriteBatch = new SpriteBatch(GraphicsDevice);

            mBackground = Content.Load<Texture2D>("Background");
            mLogo = Content.Load<Texture2D>("Unilogo");

            mLogoHit = Content.Load<SoundEffect>("Logo_hit");
            mLogoMiss = Content.Load<SoundEffect>("Logo_miss");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
           
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            var mouseState = Mouse.GetState();
            // Calculate horizontal distance between mouse and center of logo.
            float xDiff = mouseState.X - (mXLogo + Scale * mLogo.Width / 2f);
            // Calculate vertical distance between mouse and center of logo.
            float yDiff = mouseState.Y - (mYLogo + Scale * mLogo.Height / 2f);

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                // Check if mouse is inside logo using Pythagorean theorem.
                if (Math.Pow(xDiff, 2) + Math.Pow(yDiff, 2) <= Math.Pow(Scale * mLogo.Width / 2f, 2))
                {
                    mLogoHit.Play();
                }

                else
                {
                    mLogoMiss.Play();
                }
            }

            mAngle = (mAngle + 1)%360;
            // Calculate new coordinates of center of logo on circle.
            mXLogo = (float)(ScreenWidth / 2f + Math.Cos(mAngle * Math.PI / 180) * Radius - Scale * mLogo.Width / 2f);
            mYLogo = (float)(ScreenHeight / 2f + Math.Sin(mAngle * Math.PI / 180) * Radius - Scale * mLogo.Height / 2f);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            mSpriteBatch.Begin();
            var screenRectangle = new Rectangle(0, 0, ScreenWidth, ScreenHeight);
            mSpriteBatch.Draw(mBackground, screenRectangle, Color.White);
            // Draw logo with changing transparency values.
            mSpriteBatch.Draw(mLogo, new Vector2(mXLogo, mYLogo), null, Color.White * (float) Math.Abs(Math.Sin(mAngle * Math.PI / 180)), 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
            mSpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
