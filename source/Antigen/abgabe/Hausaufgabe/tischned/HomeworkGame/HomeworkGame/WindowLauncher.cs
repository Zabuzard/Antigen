namespace HomeworkGame
{
#if WINDOWS || XBOX
    static class WindowLauncher
    {
        /// <summary>
        /// Starts the game in a new window.
        /// </summary>
        /// <param name="args">Not used</param>
// ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            using (var game = new HomeworkGame())
            {
                game.Run();
            }
        }
    }
#endif
}