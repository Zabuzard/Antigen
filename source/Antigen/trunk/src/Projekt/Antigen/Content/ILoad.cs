using Antigen.Objects;

namespace Antigen.Content
{
    /// <summary>
    /// Interface for game objects which have to load resources when
    /// the Game's <code>LoadContent</code> method is executed.
    /// </summary>
    interface ILoad
    {
        /// <summary>
        /// Uses the supplied <code>ContentLoader</code> to load
        /// any content required by the implementing object.
        /// 
        /// This method is called at least once for all objects
        /// added to the game's <see cref="ObjectCaches"/>,
        /// when <see cref="Antigen.LoadContent"/> is called.
        /// </summary>
        /// <param name="contentLoader">The project's central
        /// content loader.</param>
        void LoadContent(ContentLoader contentLoader);
    }
}
