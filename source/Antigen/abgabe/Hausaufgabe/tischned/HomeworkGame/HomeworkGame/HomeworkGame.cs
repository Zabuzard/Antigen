using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HomeworkGame
{
    /// <summary>
    /// Main type of HomeworkGame, uses the XNA framework.
    /// Displays a graphic with a logo which rotates around the center.
    /// Also plays sounds when left-clicking.
    /// </summary>
    public sealed class HomeworkGame : Game
    {

        private const int LogoOffsetY = 100;
        private const float LogoAlpha = 0.2f;

        private readonly GraphicsDeviceManager mGraphics;
        private SpriteBatch mSpriteBatch;
        private int mWindowHeight,
            mWindowWidth;
        private float mRotationAngle;
        private Vector2 mLogoOrigin,
            mLogoCenter,
            mScreenCenter;
        private Texture2D mBackgroundImg,
            mLogo;
        private SoundEffect mHitSnd,
            mMissSnd;
        private SoundEffectInstance mHitSndInst,
            mMissSndInst;

        public HomeworkGame()
        {
            mGraphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Initializes the game and all its components.
        /// </summary>
        protected override void Initialize()
        {
            mGraphics.IsFullScreen = false;
            mGraphics.PreferredBackBufferHeight = 800;
            mGraphics.PreferredBackBufferWidth = 640;
            mWindowHeight = Window.ClientBounds.Height;
            mWindowWidth = Window.ClientBounds.Width;
            IsMouseVisible = true;

            base.Initialize();
        }

        /// <summary>
        /// Loads all needed content once per game.
        /// </summary>
        protected override void LoadContent()
        {
            mSpriteBatch = new SpriteBatch(GraphicsDevice);
            mBackgroundImg = Content.Load<Texture2D>(".\\res\\img\\background");
            mLogo = Content.Load<Texture2D>(".\\res\\img\\logo");
            mHitSnd = Content.Load<SoundEffect>(".\\res\\snd\\hit");
            mMissSnd = Content.Load<SoundEffect>(".\\res\\snd\\miss");
            mHitSndInst = mHitSnd.CreateInstance();
            mMissSndInst = mMissSnd.CreateInstance();

            mScreenCenter.X = (float)(mWindowWidth / 2.0);
            mScreenCenter.Y = (float)(mWindowHeight / 2.0);
            mLogoOrigin.X = mScreenCenter.X;
            mLogoOrigin.Y = mScreenCenter.Y - LogoOffsetY;
        }

        /// <summary>
        /// Updates the game, representates a logical tick.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            var elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float circle = (2 * MathHelper.Pi);
            mRotationAngle = (mRotationAngle + elapsed) % circle;

            //Rotates mLogoOrigin around mScreenCenter
            mLogoCenter = Vector2.Transform(mLogoOrigin - mScreenCenter, Matrix.CreateRotationZ(mRotationAngle)) +
                           mScreenCenter;

            MouseState mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (mLogoCenter.X - (mLogo.Width / 2.0) <= mouse.X && mouse.X <= mLogoCenter.X + (mLogo.Width / 2.0)
                    && mLogoCenter.Y - (mLogo.Height / 2.0) <= mouse.Y && mouse.Y <= mLogoCenter.Y + (mLogo.Height / 2.0))
                {
                    mHitSndInst.Play();
                }
                else
                {
                    mMissSndInst.Play();
                }

            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Renders the game on the window.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            mSpriteBatch.Begin();
            mSpriteBatch.Draw(mBackgroundImg, new Rectangle(0, 0, mWindowWidth, mWindowHeight), Color.White);
            mSpriteBatch.Draw(mLogo, mLogoCenter
                - new Vector2((float)(mLogo.Width / 2.0), (float)(mLogo.Height / 2.0)), Color.White * LogoAlpha);
            mSpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}