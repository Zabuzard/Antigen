using System;
using Hausaufgabe.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hausaufgabe
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        [UsedImplicitly]
        GraphicsDeviceManager mGraphics;
        SpriteBatch mSpriteBatch;

        private const float LogoDiameter = 100;

        private Texture2D mBackgroundTexture;
        private Texture2D mLogoTexture;

        private SoundEffect mMissSound;
        private SoundEffect mHitSound;

        private double mRotation;
        private Vector2 mPosition;
        private float mAlpha;

        private MouseState mMouse;

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
            IsMouseVisible = true;

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

            mBackgroundTexture = Content.Load<Texture2D>("Background");
            mLogoTexture = Content.Load<Texture2D>("Unilogo");
            mHitSound = Content.Load<SoundEffect>("Logo_hit");
            mMissSound = Content.Load<SoundEffect>("Logo_miss");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
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

            mMouse = Mouse.GetState();
            if (mMouse.LeftButton == ButtonState.Pressed)
            {

                if (Math.Pow(mMouse.X - mPosition.X - LogoDiameter / 2, 2) +
                    Math.Pow(mMouse.Y - mPosition.Y - LogoDiameter / 2, 2) < Math.Pow(LogoDiameter / 2, 2))
                {
                    mHitSound.Play();
                }
                else
                {
                    mMissSound.Play();
                }
            }

            mRotation = gameTime.TotalGameTime.TotalSeconds;
            mPosition.X = (float)(Math.Sin(mRotation) * (Window.ClientBounds.Width - LogoDiameter) / 2) + (Window.ClientBounds.Width - LogoDiameter) / 2;
            mPosition.Y = (float)(Math.Cos(mRotation) * (Window.ClientBounds.Height - LogoDiameter) / 2) + (Window.ClientBounds.Height-LogoDiameter) / 2;

            mAlpha += 0.01f;
            if (mAlpha > 1)
            {
                mAlpha = 0;
            }

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
            mSpriteBatch.Draw(mBackgroundTexture, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);
            mSpriteBatch.Draw(mLogoTexture, new Rectangle((int)(mPosition.X), (int)(mPosition.Y), (int)LogoDiameter, (int)LogoDiameter), Color.White * mAlpha);
            mSpriteBatch.End();

            base.Draw(gameTime);
        }
    }

}
