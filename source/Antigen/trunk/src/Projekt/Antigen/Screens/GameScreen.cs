using System;
using System.Collections.Generic;
using System.Linq;
using Antigen.Achievements;
using Antigen.AI;
using Antigen.Content;
using Antigen.Graphics;
using Antigen.HUD;
using Antigen.Input;
using Antigen.Logic;
using Antigen.Logic.CellDivision;
using Antigen.Logic.Mutation;
using Antigen.Logic.Pathfinding;
using Antigen.Logic.Selection;
using Antigen.Objects;
using Antigen.Objects.Units;
using Antigen.Objects.Units.Values;
using Antigen.Screens.Menu;
using Antigen.Settings;
using Antigen.Sound;
using Antigen.Statistics;
using Antigen.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input;
using Effect = Antigen.Sound.Effect;

namespace Antigen.Screens
{
    /// <summary>
    /// Primary game screen, containing the map and all units
    /// but no UI or HUD elements. Should be layered below
    /// the Menu and HUD screens.
    /// </summary>
    [Serializable]
    sealed class GameScreen : IScreen, IKeyListener, IDivisionContentLoader, IStatisticIncrementer, IDestroy
    {
        /// <summary>
        /// Maximal lifespan loss for red blood cells at spawn.
        /// </summary>
        private const float RedBloodCellMaxLifespanLoss = 0.5f;

        /// <summary>
        /// It's possible to disable or enable the screen from outside.
        /// </summary>
        public bool ScreenEnabled { get; private set; }
        private readonly int mRedBloodcellThreshold;
        private readonly ObjectCaches mObjectCaches;
        private readonly Camera mCam;
        private readonly InputDispatcher mInput;
        private readonly Map.Map mMap;
        private readonly SelectionManager mSelectionManager;
        private readonly HudScreen mHudScreen;
        private readonly AiBrain mAiBrain;

        [NonSerialized]
        private SoundWrapper mSoundWrapper;
        [NonSerialized]
        private RenderTarget2D mSightRenderTarget;
        [NonSerialized]
        private Point mSightRenderAreaSize;
        [NonSerialized]
        private ContentLoader mContentLoader;
        [NonSerialized]
        private ScreenManager mScreens;
        [NonSerialized]
        private GameWindow mWindow;
        [NonSerialized]
        private Texture2D mSightTexture;
        [NonSerialized]
        private SettingsManager mSettings;        

        private TimeSpan mTimeElapsed;
        private readonly Difficulty mDifficulty;
        private readonly int mSeed;

        private readonly Random mRnd = new Random();

        public static Dictionary<StatName, int> mStats;
        public static List<Notification> mNotifications;

        /// <summary>
        /// Creates a new game screen and loads any content necessary to update
        /// and draw it.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="contentLoader">The content loader that may be call specific methods.</param>
        /// <param name="input">The input manager that may be used to fire events to objects.</param>
        /// <param name="selectionManager">The game's selection manager.</param>
        /// <param name="window">The window used to detect the borders</param>
        /// <param name="screenManager"> The ScreenManager is needed to load the pause screen. </param>
        /// <param name="objectCaches">The game's central object caches.</param>
        /// <param name="redBloodcellThreshold">Positive integer. If the number of red bloodcells
        /// falls below this threshold, the player loses the game.</param>
        /// <param name="difficulty">The difficulty of the game screen.</param>
        /// <param name="settings">The game's central settings manager.</param>
        public GameScreen(SpriteBatch spriteBatch, ContentLoader contentLoader, InputDispatcher input,
            SelectionManager selectionManager, GameWindow window, ScreenManager screenManager,
            ObjectCaches objectCaches, int redBloodcellThreshold, Difficulty difficulty, SettingsManager settings)
        {
            mSettings = settings;
            mDifficulty = difficulty;
            mContentLoader = contentLoader;
            mScreens = screenManager;
            mInput = input;
            mWindow = window;
            mObjectCaches = objectCaches;
            mSelectionManager = selectionManager;
            mObjectCaches.Add(mSelectionManager);
            mRedBloodcellThreshold = redBloodcellThreshold;
            mSoundWrapper = mScreens.SoundWrapper;
            ScreenEnabled = true;

            mSightRenderTarget = null;
            mSightRenderAreaSize = new Point(window.ClientBounds.Width, window.ClientBounds.Height);
            
            mHudScreen = new HudScreen(mInput, mWindow, mSelectionManager, objectCaches, screenManager);

            mMap = new Map.Map();
            mSeed = new Random().Next();
            mMap.LoadMapContent(contentLoader, spriteBatch, mSeed);
            mObjectCaches.Add(mMap);
            mObjectCaches.CurrentMap = mMap;

            mCam = new Camera(mInput, mWindow, mMap.GetWidth(), mMap.GetHeight(), settings, mMap.PlayerStartPosition);
            mAiBrain = new AiBrain(objectCaches);

            SpawnUnits();

            mInput.RegisterListener(this);

            window.ClientSizeChanged += OnClientSizeChanged;
            

            mStats = new Dictionary<StatName, int>
            {
                {StatName.Playing_Time, 0},
                {StatName.Collected_Antigens, 0},
                {StatName.Killed_Bacteriums, 0},
                {StatName.Killed_Viruses, 0},
                {StatName.Lost_red_Bloodcells, 0},
                {StatName.Performed_celldivisions, 0},
                {StatName.Performed_mutations, 0}
            };
            mTimeElapsed = new TimeSpan();
            mNotifications = new List<Notification>();
        }

        /// <summary>
        /// Load contents for the gamescreen after deserializing the screen.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="contentLoader">The contentLoader of the game.</param>
        /// <param name="screenManager">The Screenmanager of the game.</param>
        /// <param name="window">The gamewindow.</param>
        /// <param name="input">The inputmanager of the game.</param>
        /// <param name="statistics">The true statistics of this specific gamescreen in that state.</param>
        /// <param name="settings">Settings of the game state</param>
        public void LoadGameState(SpriteBatch spriteBatch,
            ContentLoader contentLoader,
            ScreenManager screenManager,
            GameWindow window,
            IInputService input,
            Dictionary<StatName, int> statistics,
            SettingsManager settings)
        {
            mSightTexture = contentLoader.LoadTexture("Circle");
            mSettings = settings;
            mStats = statistics;
            mNotifications = new List<Notification>();
            mContentLoader = contentLoader;
            mScreens = screenManager;
            mSoundWrapper = mScreens.SoundWrapper;
            mWindow = window;
            mCam.LoadGameState(mWindow, mInput, mSettings);
            mSightRenderAreaSize = new Point(window.ClientBounds.Width, window.ClientBounds.Height);
            mSightRenderTarget = null;
            mInput.LoadGameState(input, mSettings.CurrentSettings.Keymap);
            mHudScreen.LoadGameState(mScreens, mWindow, mContentLoader);
            mObjectCaches.CurrentMap.LoadMapContent(contentLoader, spriteBatch, mSeed);
            mObjectCaches.Pathfinder = new Pathfinder(mObjectCaches.CurrentMap, mObjectCaches.SpatialCache);
            mObjectCaches.ApplyDeferredUpdates();
            foreach (var obj in mObjectCaches.ListLoad)
                obj.LoadContent(contentLoader);
            window.ClientSizeChanged += OnClientSizeChanged;
        }

        /// <summary>
        /// Adjusts the render area size.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="eventArgs">Parameter not used.</param>
        private void OnClientSizeChanged(object sender, EventArgs eventArgs)
        {
            mSightRenderAreaSize = new Point(Math.Max(1, mWindow.ClientBounds.Width),
                Math.Max(1, mWindow.ClientBounds.Height));
            mSightRenderTarget = null;
        }


        /// <inheritdoc />
        public bool HandleKeyPress(Keys key)
        {
            if (key != Keys.Escape || mScreens.Peek() != this)
                return false;
            mScreens.PushMenuScreen(typeof(PauseScreen));
            return false;
        }

        /// <inheritdoc />
        public bool HandleKeyRelease(Keys key)
        {
            return false;
        }

        /// <inheritdoc />
        public Camera GetCam()
        {
            return mCam;
        }

        /// <inheritdoc />
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            mObjectCaches.ApplyDeferredUpdates();

            if (mSightRenderTarget == null)
            {
                mSightRenderTarget =
                    new RenderTarget2D(spriteBatch.GraphicsDevice, mSightRenderAreaSize.X, mSightRenderAreaSize.Y);
            }

            spriteBatch.GraphicsDevice.SetRenderTarget(mSightRenderTarget);
            var color = Color.Black;
            color.A = 100;
            spriteBatch.GraphicsDevice.Clear(color);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, mCam.TransformMatrix);
            foreach (var unit in mObjectCaches.ListUnit.Where(unit => unit.GetSide() == Unit.UnitSide.Friendly))
            {
                spriteBatch.Draw(mSightTexture, unit.Position, null, Color.White, 0, new Vector2(mSightTexture.Width / 2f, mSightTexture.Height / 2f), (float)ValueStore.ConvertSightToPixel(unit.GetSight()) / mSightTexture.Width, SpriteEffects.None, 0);
            }
            spriteBatch.End();
            spriteBatch.GraphicsDevice.SetRenderTarget(null);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, mCam.TransformMatrix);

            foreach (var obj in mObjectCaches.ListDrawable.Where(obj => !(obj is RectangularSelector)))
                obj.Draw(gameTime, spriteBatch, spriteFont);
            spriteBatch.End();

            var blendState = new BlendState
            {
                AlphaDestinationBlend = Blend.SourceColor,
                ColorDestinationBlend = Blend.SourceColor,
                AlphaSourceBlend = Blend.Zero,
                ColorSourceBlend = Blend.Zero
            };
            spriteBatch.Begin(SpriteSortMode.Deferred, blendState);
            spriteBatch.Draw(mSightRenderTarget,
                new Rectangle(0, 0, mWindow.ClientBounds.Width, mWindow.ClientBounds.Height),
                Color.White);
            spriteBatch.End();

            blendState = new BlendState
            {
                AlphaDestinationBlend = Blend.DestinationAlpha,
                ColorDestinationBlend = Blend.DestinationAlpha,
                AlphaSourceBlend = Blend.InverseDestinationAlpha,
                ColorSourceBlend = Blend.InverseDestinationAlpha,
            };
            spriteBatch.Begin(SpriteSortMode.Deferred, blendState, null, null, null, null, mCam.TransformMatrix);
            var map = mObjectCaches.CurrentMap;
            if (map != null)
            {
                map.Draw(gameTime, spriteBatch, spriteFont);
            }
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, mCam.TransformMatrix);
            foreach (var obj in mObjectCaches.ListDrawable.Where(obj => (obj is RectangularSelector || obj is FlowParticle)))
                obj.Draw(gameTime, spriteBatch, spriteFont);
            spriteBatch.End();

            mHudScreen.Draw(gameTime, spriteBatch, spriteFont);

            // Draw up to 5 notifications if any.
            if (mNotifications.Any())
            {
                for (int index = 0; index < mNotifications.Count; index++)
                {
                    if (index >= 5)
                    {
                        break;
                    }
                    var item = mNotifications[index];
                    var position = (int)(mWindow.ClientBounds.Height * (1 / 5f + index * 1 / 9f));
                    item.Draw(gameTime, spriteBatch, spriteFont, position);
                }
            }
        }

        /// <inheritdoc />
        public void Update(GameTime gameTime)
        {
            mObjectCaches.ApplyDeferredUpdates();
            mInput.ApplyDeferredUpdates();
            mInput.Update(gameTime);
            mCam.Update(gameTime);
            mAiBrain.Update(gameTime);
            mHudScreen.Update(gameTime);
            mObjectCaches.Pathfinder.Update(gameTime);

            foreach (var obj in mObjectCaches.ListUpdateable)
            {
                obj.Update(gameTime);

                //Play sound if unit is moving
                var unit = obj as Unit;
                if (unit != null && unit.IsMoving() && unit.GetSide() == Unit.UnitSide.Friendly)
                {
                    mSoundWrapper.PlayEffect(Effect.Move1, unit.Position);
                }

                if (unit != null && !unit.IsAlive())
                {
                    //Play sound at units destruction
                    if (unit.GetSide() == Unit.UnitSide.Friendly)
                    {
                        mSoundWrapper.PlayEffect(Effect.Destruction, unit.Position);
                    }

                    //Increment statistics if unit was killed by player
                    if (unit.GetLifepoints() <= ValueStore.MinLifepoints)
                    {
                        if (unit is Bacterium)
                        {
                            IncrementStatistic(StatName.Killed_Bacteriums);
                        }
                        else if (unit is Virus)
                        {
                            IncrementStatistic(StatName.Killed_Viruses);
                        }
                    }
                }

                if (unit != null && Functions.IsObjectOutOfMap(unit, mMap.GetWidth(), mMap.GetHeight()))
                    unit.Die();
            }

            //Play background music
            mSoundWrapper.PlayMusic(Music.Game);

            // Increment game time for statistics.
            mTimeElapsed += gameTime.ElapsedGameTime;
            if (mTimeElapsed.TotalSeconds >= 1)
            {
                IncrementStatistic(StatName.Playing_Time);
                mTimeElapsed = new TimeSpan();
            }
            
            //Check win/loss condition
            var win = WinCondition.PlayerHasWon(mObjectCaches.ListEnemy);
            var loss = LossCondition.PlayerHasLost(mObjectCaches.ListFriendly, mObjectCaches.ListRedBloodcell, mRedBloodcellThreshold);

            if (win == null && loss == null)
                return;

            //Game has ended: destroy this screen
            mScreens.Pop();
            GC.Collect();

            //Push a game end screen
            var reason = win == null ? loss.Value : win.Value;
            mScreens.PushGameEndScreen(reason);
        }

        /// <inheritdoc />
        public void LoadContent(ContentLoader contentLoader)
        {
            mSightTexture = contentLoader.LoadTexture("Circle");

            mObjectCaches.ApplyDeferredUpdates();

            foreach (var obj in mObjectCaches.ListLoad)
                obj.LoadContent(contentLoader);
            mHudScreen.LoadContent(contentLoader);
        }

        /// <inheritdoc />
        public void LoadDivisionContent(Unit target)
        {
            target.LoadContent(mContentLoader);
        }

        /// <inheritdoc />
        public bool UpdateLower()
        {
            return false;
        }

        /// <inheritdoc />
        public bool DrawLower()
        {
            return false;
        }

        /// <inheritdoc />
        public EventOrder EventOrder
        {
            get { return EventOrder.GameObjects; }
        }

        /// <summary>
        /// Call this to increment the stat counter for the given stat name.
        /// </summary>
        /// <param name="name">The name of the stat counter.</param>
        public void IncrementStatistic(StatName name)
        {
            mStats[name] += 1;
            var newAchievementUnlocked = false;
            var achievementName = AchievementNames.None;
            switch (name)
            {
                case StatName.Killed_Bacteriums :
                    if (mStats[name] == 100)
                    {
                        achievementName = AchievementNames.Kill_100_Bacteriums;
                    }
                    break;
                case StatName.Killed_Viruses :
                    if (mStats[name] == 100)
                    {
                        achievementName = AchievementNames.Kill_100_Viruses;
                    }
                    break;
                case StatName.Performed_celldivisions :
                    if (mStats[name] == 100)
                    {
                        achievementName = AchievementNames.Perform_100_Cell_Divisions;
                    }
                    break;
                case StatName.Performed_mutations :
                    if (mStats[name] == 100)
                    {
                        achievementName = AchievementNames.Perform_100_Mutations;
                    }
                    break;
                case StatName.Collected_Antigens :
                    if (mStats[name] == 1)
                    {
                        achievementName = AchievementNames.Collect_Antigen;
                    }
                    break;
            }
            if (achievementName != AchievementNames.None)
            {
                newAchievementUnlocked = Achievements.Achievements.UnlockAchievement(achievementName);
            }
            if (newAchievementUnlocked)
            {
                mNotifications.Add(new Notification(mContentLoader, mWindow, achievementName));
            }
        }

        /// <summary>
        /// Spawns all units.
        /// </summary>
        private void SpawnUnits()
        {
            var pathfinder = mObjectCaches.Pathfinder;

            //Set difficulty
            ValueStore.SetDifficulty(mDifficulty);
			
			//Spawn Flow particles
            const int flowParticleAmount = 200;
            var flowPoints = mMap.RandomWalkablePoints(flowParticleAmount);
            foreach (var point in flowPoints)
            {
                var flowParticle = new FlowParticle(new Vector2(point.X, point.Y), mMap);
                mObjectCaches.Add(flowParticle);
                mObjectCaches.ApplyDeferredUpdates();
            }
			
            //Spawn units
            var playerStart = mMap.PlayerStartPosition;
            var stemcell = new Stemcell(mObjectCaches, this, mInput, mSelectionManager,
                pathfinder.FindNearestValidPoint(playerStart, true, true), mMap, mSoundWrapper, this);
            mObjectCaches.Add(stemcell);
            mObjectCaches.ApplyDeferredUpdates();

            var enemyStart = mMap.EnemyStartPosition;
            var bacterium = new Bacterium(mObjectCaches, this, pathfinder.FindNearestValidPoint(enemyStart, true, true), mMap, mAiBrain, this);
            mObjectCaches.Add(bacterium);
            mObjectCaches.ApplyDeferredUpdates();

            var virus = new Virus(mObjectCaches, this, pathfinder.FindNearestValidPoint(enemyStart, true, true), mMap, mAiBrain, this);
            mObjectCaches.Add(virus);
            mObjectCaches.ApplyDeferredUpdates();

            var bloodPoints = mMap.RandomWalkablePoints(50).Select(pt => new Vector2(pt.X, pt.Y));
            foreach (var point in bloodPoints)
            {
                var redBloodCell = new RedBloodcell(mObjectCaches, pathfinder.FindNearestValidPoint(point, true, true), mMap);
                mObjectCaches.Add(redBloodCell);
                mObjectCaches.ApplyDeferredUpdates();

                //Reduce lifespan of spawned blood cells randomly
                var lossFactor = mRnd.NextDouble() * RedBloodCellMaxLifespanLoss;
                redBloodCell.ChangeLifespan(-lossFactor * redBloodCell.GetLifespan());
            }
        }

        /// <inheritdoc />
        public void Destroy()
        {
            mInput.DeregisterListener(this);
            mInput.Destroy();
            mCam.Destroy();
            mHudScreen.Destroy();
            mWindow.ClientSizeChanged -= OnClientSizeChanged;
            mObjectCaches.Pathfinder.Destroy();
            StrainGenerator.Destroy();
            ScreenEnabled = false;
        }

        public void OnDeactivate()
        {
            HandleKeyPress(Keys.Escape);
        }
    }
}
