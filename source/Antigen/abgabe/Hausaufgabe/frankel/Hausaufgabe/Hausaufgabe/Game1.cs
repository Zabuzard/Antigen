using System;
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
        SpriteBatch _spriteBatch;
        private Texture2D _dateiUnilogo;
        private Texture2D _dateiBackground;
        private SoundEffect _hitSound;
        private SoundEffect _missSound;
        private double _positionX;
        private double _positionY;
        private int _height;
        private int _width;
        private int _amplitudeX;
        private int _amplitudeY;


        public Game1()
        {
            // Needed for loading content, but is never used according to resharper.
            // ReSharper disable once ObjectCreationAsStatement
            new GraphicsDeviceManager(this);
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
            _height = Window.ClientBounds.Height;
            _width = Window.ClientBounds.Width;

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
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _dateiBackground = Content.Load<Texture2D>("background");
            _dateiUnilogo = Content.Load<Texture2D>("Unilogo");
            _amplitudeX = (int)(_width / 2.0 - _dateiUnilogo.Bounds.Width / 2.0) - 10;
            _amplitudeY = (int)(_height / 2.0 - _dateiUnilogo.Bounds.Height / 2.0) - 10;
            _hitSound = Content.Load<SoundEffect>("hit");
            _missSound = Content.Load<SoundEffect>("miss");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();
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

            _positionX = Math.Cos(gameTime.TotalGameTime.TotalSeconds) * _amplitudeX + _amplitudeX + 10;
            _positionY = Math.Sin(gameTime.TotalGameTime.TotalSeconds) * _amplitudeY + _amplitudeY + 10;
            Vector2 centre = new Vector2((float)(_positionX + _dateiUnilogo.Bounds.Width / 2.0), (float) (_positionY + _dateiUnilogo.Bounds.Height / 2.0));

            MouseState mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if ((new Vector2(mouse.X, mouse.Y) - centre).Length() <= _dateiUnilogo.Bounds.Height / 2.0)
                {
                    _hitSound.Play();
                }
                else
                {
                    _missSound.Play();
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(_dateiBackground, new Rectangle(0, 0, _width, _height), Color.White);
            var transparency = (int) ((Math.Sin(3 * gameTime.TotalGameTime.TotalSeconds) + 1)/2.0*155 + 100);
            _spriteBatch.Draw(_dateiUnilogo, new Vector2((float) _positionX, (float) _positionY), new Color(255, 255, 255, transparency));
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}