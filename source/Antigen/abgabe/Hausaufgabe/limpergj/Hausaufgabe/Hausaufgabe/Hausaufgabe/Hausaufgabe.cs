using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hausaufgabe
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public sealed class Hausaufgabe : Game
    {
        private Rectangle      mClientBounds;
        private SpriteBatch    mSpriteBatch;
        private MouseWrapper   mMouseWrapper;

        private readonly GraphicsDeviceManager mGraphicsDevice;
        private          Texture2D             mBackgroundTexture;
        private          Logo                  mLogo;

        public Hausaufgabe()
        {
            mGraphicsDevice = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Initializes game objects.
        /// </summary>
        protected override void Initialize()
        {
            mGraphicsDevice.IsFullScreen = false;
            IsMouseVisible               = true;
            mClientBounds                = Window.ClientBounds;
            mMouseWrapper                = new MouseWrapper();

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
            var logoTexture    = Content.Load<Texture2D>("Unilogo");
            var hitSound       = Content.Load<SoundEffect>("Logo_hit");
            var missSound      = Content.Load<SoundEffect>("Logo_miss");

            var logoSquare = Util.MaximumCenteredSquare(WindowRectangle());
            mLogo = new Logo(logoTexture, logoSquare, hitSound, missSound);
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
            mMouseWrapper.Update(Mouse.GetState());
            mLogo.Update(gameTime, mMouseWrapper);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            mSpriteBatch.Begin();

            DrawBackground(mSpriteBatch);
            mLogo.Draw(mSpriteBatch);

            mSpriteBatch.End();
            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws the background texture, scaled to the window size.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch.</param>
        private void DrawBackground(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(mBackgroundTexture, WindowRectangle(), Color.White);
        }

        /// <summary>
        /// A rectangle covering the entire client window.
        /// </summary>
        /// <returns>The client window's dimensions.</returns>
        private Rectangle WindowRectangle()
        {
            return new Rectangle(0, 0, mClientBounds.Width, mClientBounds.Height);
        }
    }
}
