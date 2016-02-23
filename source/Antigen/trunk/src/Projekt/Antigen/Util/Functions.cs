using System;
using System.Collections.Generic;
using System.IO;
using Antigen.Logic.CellDivision;
using Antigen.Logic.Collision;
using Antigen.Logic.Mutation;
using Antigen.Logic.Offensive.Infection;
using Antigen.Objects.Units;
using Antigen.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Antigen.Util
{
    /// <summary>
    /// Various utility methods.
    /// </summary>
    internal static class Functions
    {
        /// <summary>
        /// Simultaneously extracts the minimum and maximum
        /// element from an array of objects.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="objs">An array of comparable objects.
        /// MUST have at least one element.</param>
        /// <param name="min">The minimum element.</param>
        /// <param name="max">The maximum element.</param>
        public static void MinMax<T>(T[] objs, out T min, out T max) where T : IComparable<T>
        {
            min = objs[0];
            max = objs[0];

            for (var i = 1; i < objs.Length; i++)
            {
                var obj = objs[i];
                if (obj.CompareTo(min) < 0)
                    min = obj;
                if (obj.CompareTo(max) > 0)
                    max = obj;
            }
        }

        /// <summary>
        /// Spans a rectangle between two points.
        /// </summary>
        /// <param name="a">A point.</param>
        /// <param name="b">A point.</param>
        /// <returns>A rectangle with <code>a</code> and <code>b</code>
        /// as corners.</returns>
        public static Rectangle RectangleFromTo(Point a, Point b)
        {
            var topLeftX = Math.Min(a.X, b.X);
            var topLeftY = Math.Min(a.Y, b.Y);
            var width = Math.Abs(a.X - b.X);
            var height = Math.Abs(a.Y - b.Y);

            return new Rectangle(topLeftX, topLeftY, width, height);
        }

        /// <summary>
        /// Truncates <code>vec</code> such that its
        /// length is less than or equal to <code>maxLength</code>.
        /// The direction of <code>vec</code> remains unchanged.
        /// </summary>
        /// <param name="vec">Any vector.</param>
        /// <param name="maxLength">Maximum length of the
        /// result vector.</param>
        /// <returns>A vector that points in the direction
        /// of <code>vec</code> but is at most
        /// <code>maxLength</code> long.</returns>
        public static Vector2 Truncate(this Vector2 vec, float maxLength)
        {
            if (vec.Length() <= maxLength)
                return vec;

            var copy = new Vector2(vec.X, vec.Y);
            copy.Normalize();
            return copy * maxLength;
        }

        /// <summary>
        /// Converts a given strain to a unique color.
        /// </summary>
        /// <param name="strain">Strain to convert</param>
        /// <returns>Unique color that represents the strain</returns>
        public static Color StrainToColor(string strain)
        {
            var index = StrainGenerator.GetInstance().GetFamilyIndex(strain);
            if (index == -1) return Color.Black;
            var count = StrainGenerator.GetInstance().GetFamiliyCount();
            return HsvToColor((float)index / count * 360, 1, 1);
        }

        /// <summary>
        /// Converts a color given in the hue,
        /// saturation and value format (HSV) to a color object.
        /// </summary>
        /// <param name="hue">Hue of the color</param>
        /// <param name="saturation">Saturation of the color</param>
        /// <param name="value">Value of the color</param>
        /// <returns>Color object that represents the given color of hsv format</returns>
        private static Color HsvToColor(float hue, float saturation, float value)
        {
            var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            var f = (float)(hue / 60 - Math.Floor(hue / 60));

            value = value * 255;
            var v = value;
            var p = value * (1 - saturation);
            var q = value * (1 - f * saturation);
            var t = value * (1 - (1 - f) * saturation);

            switch (hi)
            {
                case 0:
                    return new Color(v * 255, t * 255, p * 255, 1f);
                case 1:
                    return new Color(q * 255, v * 255, p * 255, 1f);
                case 2:
                    return new Color(p * 255, v * 255, t * 255, 1f);
                case 3:
                    return new Color(p * 255, q * 255, v * 255, 1f);
                case 4:
                    return new Color(t * 255, p * 255, v * 255, 1f);
                default:
                    return new Color(v * 255, p * 255, q * 255, 255);
            }
        }

        /// <summary>
        /// Distance between two points in 2D space.
        /// </summary>
        /// <param name="x">First point.</param>
        /// <param name="y">Second point.</param>
        /// <returns>Distance between <code>x</code> and <code>y</code>.</returns>
        public static double Distance(this Vector2 x, Vector2 y)
        {
            return Vector2.Distance(x, y);
        }

        /// <summary>
        /// Transforms a coordinate given in the polar to the cartesian system.
        /// </summary>
        /// <param name="radius">Radius of the coordinate</param>
        /// <param name="angle">Angle of the coordinate</param>
        /// <returns>Resulting vector in the cartesian system</returns>
        public static Vector2 PolarToCartesian(double radius, double angle)
        {
            var x = Math.Cos(angle) * radius;
            var y = Math.Sin(angle) * radius;
            return new Vector2((float) x, (float) y);
        }

        /// <summary>
        /// Transforms a coordinate given in the cartesian to the polar system.
        /// </summary>
        /// <param name="x">X-coordinate of the vector</param>
        /// <param name="y">Y-coordinate of the vector</param>
        /// <returns>Resulting vector in the polar system where the first variable is the radius and the second the angle</returns>
        public static Vector2 CartesianToPolar(double x, double y)
        {
            var radius = Math.Sqrt(x * x + y * y);
            var angle = RadianToDeg((Math.Atan(y / x)));

            //Calculate Atan for all 4 quadrants correct.
            //Difference to Atan2 is that the result is a full mathematical angle from [0,360)
            //And not [-180,180] like Atan2.
            if (x < 0 && y >= 0)
            {
                angle = 180 + angle;
            }
            else if (x < 0 && y < 0)
            {
                angle = 180 + angle;
            }
            else if (x >= 0 && y < 0)
            {
                angle = 360 + angle;
            }

            return new Vector2((float) radius, (float) angle);
        }

        /// <summary>
        /// Transforms a degree into a radian value.
        /// </summary>
        /// <param name="degree">Degree value</param>
        /// <returns>Resulting radian value</returns>
        public static double DegToRadian(int degree)
        {
            return (Math.PI / 180) * degree;
        }

        /// <summary>
        /// Returns if constellation of units is friendly e.g. player and player.
        /// Method is symmetric and reflexiv.
        /// </summary>
        /// <param name="first">First unit</param>
        /// <param name="second">Second unit</param>
        /// <returns>If constellation of units is friendly</returns>
        public static bool IsFriendlySideConstellation(Unit first, Unit second)
        {
            var playerVsPlayer = first.GetSide() == Unit.UnitSide.Friendly &&
                                  second.GetSide() == Unit.UnitSide.Friendly;
            var enemyVsEnemy = first.GetSide() == Unit.UnitSide.Enemy &&
                                  second.GetSide() == Unit.UnitSide.Enemy;
            var neutralVsPlayer = first.GetSide() == Unit.UnitSide.Neutral &&
                                  second.GetSide() == Unit.UnitSide.Friendly;
            var playerVsNeutral = first.GetSide() == Unit.UnitSide.Friendly &&
                                  second.GetSide() == Unit.UnitSide.Neutral;

            return playerVsPlayer || enemyVsEnemy || neutralVsPlayer || playerVsNeutral;
        }

        /// <summary>
        /// Returns if constellation of units is offensive e.g. a cell
        /// dividing unit can not attack.
        /// Method is symmetric and reflexiv.
        /// </summary>
        /// <param name="offensive">First unit</param>
        /// <param name="target">Second unit</param>
        /// <param name="isDeinfection">If constellation for a deinfection</param>
        /// <returns>If constellation of units is offensive</returns>
        public static bool IsOffensiveConstellation(Unit offensive, Unit target, bool isDeinfection)
        {
            if (IsFriendlySideConstellation(offensive, target))
            {
                return false;
            }
            if (!IsAbleToOffensive(offensive))
            {
                return false;
            }

            if (target is IInfectable && ((IInfectable)target).IsInfected())
            {
                return false;
            }
            var infect = target as ICanInfect;
            if (isDeinfection)
            {
                
                return infect != null && infect.IsInfecting();
            }
            return infect == null || !infect.IsInfecting();
        }

        /// <summary>
        /// Returns if a given unit is currently able to do an
        /// offensive action like attacking another object.
        /// </summary>
        /// <param name="offensive">Unit of interest</param>
        /// <returns>True if the unit is able to do an offensive action</returns>
        public static bool IsAbleToOffensive(this Unit offensive)
        {
            if (offensive is ICanCellDivision && ((ICanCellDivision)offensive).IsCellDividing())
            {
                return false;
            }
            if (offensive is IInfectable && ((IInfectable)offensive).IsInfected())
            {
                return false;
            }
            if (offensive is ICanInfect && ((ICanInfect)offensive).IsInfecting())
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns if a given target is currently able to receive an
        /// offensive action like an attack.
        /// </summary>
        /// <param name="target">Target of interest</param>
        /// <param name="isDeinfection">If action is a deinfection</param>
        /// <returns>True if the target is able to receive an offensive action</returns>
        public static bool IsTargetAbleToReceiveOffensive(Unit target, bool isDeinfection)
        {
            if (target is IInfectable && ((IInfectable)target).IsInfected())
            {
                return false;
            }
            var infect = target as ICanInfect;
            if (infect != null)
            {
                return infect.IsInfecting() && isDeinfection;
            }
            return true;
        }

        /// <summary>
        /// If gameobject is currently out of the map.
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <param name="mapWidth">Width of map</param>
        /// <param name="mapHeight">Height of map</param>
        /// <returns>True if gameobject is out of the map, false otherwhise</returns>
        public static bool IsObjectOutOfMap(ISpatial obj, int mapWidth, int mapHeight)
        {
            var box = obj.Hitbox;
            return
                box.X < 0 ||
                box.Y < 0 ||
                box.X + box.Width > mapWidth ||
                box.Y + box.Height > mapHeight;
        }

        /// <summary>
        /// Calculates the percentage that the given value takes in for the given intervall.
        /// </summary>
        /// <param name="value">Value to calculated percentage</param>
        /// <param name="minValue">Min value of the intervall</param>
        /// <param name="maxValue">Max value of the intervall</param>
        /// <returns>Percentage that the given value takes in for the given intervall</returns>
        public static float ValueInPerc(int value, int minValue, int maxValue)
        {
            var normedValue = value - minValue;
            var normedMaxValue = maxValue - minValue;
            return (float)normedValue / normedMaxValue;
        }

        /// <summary>
        /// Transforms a radian into a degree value.
        /// </summary>
        /// <param name="radian">Radian value</param>
        /// <returns>Resulting degree value</returns>
        private static double RadianToDeg(double radian)
        {
            return (180 / Math.PI) * radian;
        }

        /// <summary>
        /// Creates a folder for Antigen in AppData\\Roamig if none. Returns the path.
        /// </summary>
        /// <returns>The path to AppData\\Roamint\\Antigen</returns>
        public static string GetFolderPath()
        {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Antigen"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                          "\\Antigen");
            }
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Antigen";
        }


        /// <summary>
        /// Get all custom resolutions that are currently possible, depending on window / fullscreen mode.
        /// </summary>
        /// <param name="isFullScreen">True if the window is currently in fullscreen mode, false otherwise.</param>
        /// <returns>Return a list of the possible resolutions.</returns>
        public static List<Resolution> GetCustomResolutions(bool isFullScreen)
        {

            var customResolutions = new List<Resolution>();
            var maxHeight = 0;
            foreach (var mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                var res = new Resolution(mode.Width, mode.Height);
                if (res.Vertical > maxHeight)
                {
                    maxHeight = res.Vertical;
                }
                // Remove resolutions that are smaller than 800x600 because the hud cannot be displayed correctly then.
                if (!customResolutions.Contains(res) && res.Horizontal >= 800 && res.Vertical >= 600)
                {
                    customResolutions.Add(res);
                }
            }
            // Remove resolutions where the height is bigger than the display if in window mode.
            if (!isFullScreen)
            {
                for (var i = 0; i < customResolutions.Count; i++)
                {
                    if (customResolutions[i].Vertical >= maxHeight)
                    {
                        customResolutions.Remove(customResolutions[i]);
                    }
                }
            }
            return customResolutions;
        }
    }
}
