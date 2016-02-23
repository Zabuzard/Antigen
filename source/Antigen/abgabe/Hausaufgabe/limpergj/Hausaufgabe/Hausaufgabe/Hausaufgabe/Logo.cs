using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Hausaufgabe
{
    /// <summary>
    /// Logo animation that manages movement, transparency and sound.
    /// </summary>
    /// The animation behaves as follows:
    /// 
    /// A texture, scaled to fit into the animation space, circles around
    /// its center. It periodically changes its transparency.
    /// 
    /// When the user clicks on the circle of maximum size that fits into
    /// the texture, a sound is played. When the user clicks elsewhere, a
    /// different sound is played.
    internal sealed class Logo
    {
        private const float RotationSpeed = 3; // rad/s
        private const int   AlphaChange   = 1; // RGB value (0 to 255) per second

        private readonly Texture2D   mTexture;
        private readonly int         mSize;
        private readonly Circle      mTrajectory;
        private readonly SoundEffect mHitSound;
        private readonly SoundEffect mMissSound;
        private          float       mAngle;
        private          int         mAlpha;

        /// <summary>
        /// Initializes default values.
        /// </summary>
        private Logo()
        {
            mAngle = 0;
            mAlpha = 0;
        }

        /// <summary>
        /// Creates a new Logo animation.
        /// </summary>
        /// <param name="texture">Texture used for the Logo.</param>
        /// <param name="targetSquare">Square space within which the animation should be played.</param>
        /// <param name="hitSound">Sound effect played when the user clicks on the Logo.</param>
        /// <param name="missSound">Sound effect played when the user clicks outside the Logo.</param>
        internal Logo(Texture2D texture, Rectangle targetSquare, SoundEffect hitSound, SoundEffect missSound) : this()
        {
            mTexture    = texture;
            mHitSound   = hitSound;
            mMissSound  = missSound;

            mSize       = targetSquare.Width  / 3;
            mTrajectory = new Circle(Util.RectangleCenter(targetSquare), targetSquare.Width / 3);
        }

        /// <summary>
        /// Aarea of the texture that may collide with other objects.
        /// </summary>
        /// <returns>The Logo texture's collision area.</returns>
        private Circle CollisionArea()
        {
            return new Circle(TextureCenter(), mSize / 2);
        }

        /// <summary>
        /// Center point of the texture at its current location.
        /// </summary>
        /// <returns>Texture's center point.</returns>
        private Point TextureCenter()
        {
            return Circle.PointAtAngle(mTrajectory, mAngle);
        }

        /// <summary>
        /// Leftmost, topmost point of the texture square at its current
        /// location.
        /// </summary>
        /// <returns>The texture's current position.</returns>
        private Point Position()
        {
            var center = TextureCenter();
            return new Point(center.X - (mSize / 2), center.Y - (mSize / 2));
        }

        /// <summary>
        /// Square area into which the texture should be drawn according
        /// to its current position.
        /// </summary>
        /// <returns>The texture's destination draw area.</returns>
        private Rectangle DestinationSquare()
        {
            var pos = Position();
            return new Rectangle(pos.X, pos.Y, mSize, mSize);
        }

        /// <summary>
        /// Predicate for collision with a point.
        /// </summary>
        /// Returns true iff the point is within the texture's collision
        /// area.
        /// <param name="point">A point.</param>
        /// <returns>Whether the point collides with the Logo.</returns>
        private bool CollidesWith(Point point)
        {
            return Circle.CollidesWith(CollisionArea(), point);
        }

        /// <summary>
        /// Plays a sound that depends on the current mouse input.
        /// </summary>
        /// If no click has been registered, no sound is played.
        /// Otherwise, different sounds are played depending on
        /// whether or not the user clicked on the Logo collision
        /// area.
        /// <param name="mouse">Current mouse state.</param>
        private void PlaySound(MouseWrapper mouse)
        {
            if (!mouse.LeftClicked) { return; }

            if (CollidesWith(mouse.CursorPosition))
                mHitSound.Play();
            else
                mMissSound.Play();
        }

        /// <summary>
        /// Updates the Logo animation.
        /// </summary>
        /// <param name="gameTime">Current game time.</param>
        /// <param name="mouse">Current mouse state.</param>
        public void Update(GameTime gameTime, MouseWrapper mouse)
        {
            mAngle += (float) (RotationSpeed * gameTime.ElapsedGameTime.TotalSeconds);
            mAlpha = (mAlpha + AlphaChange) % 255;
            PlaySound(mouse);
        }

        /// <summary>
        /// Draws the Logo texture to its current position.
        /// </summary>
        /// <param name="spriteBatch">A sprite batch to draw to.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(mTexture, DestinationSquare(), new Color(255, 255, 255, mAlpha));
        }
    }
}
