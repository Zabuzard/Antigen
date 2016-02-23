using Antigen.Content;
using IDrawable = Antigen.Graphics.IDrawable;
using IUpdateable = Antigen.Logic.IUpdateable;

namespace Antigen.Screens
{
    /// <summary>
    /// Interface for screens.
    /// 
    /// A screen is essentially a collection
    /// of game objects and/or UI elements that
    /// form a logical unit and can be enabled
    /// and disabled together.
    /// </summary>
    interface IScreen : IUpdateable, IDrawable, ILoad
    {
        /// <summary>
        /// Whether the screen is enabled.
        /// </summary>
        bool ScreenEnabled { get; }

        /// <summary>
        /// Whether to update screens located below
        /// this one in the <see cref="ScreenManager"/>
        /// stack.
        /// </summary>
        bool UpdateLower();

        /// <summary>
        /// Whether to draw screens located below
        /// this one in the <see cref="ScreenManager"/>
        /// stack.
        /// </summary>
        bool DrawLower();
    }
}