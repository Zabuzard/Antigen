namespace Hausaufgabe
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        // ReSharper disable once UnusedParameter.Local
        // Reason: args param can obviously not be removed
        // but is currently unused.
        static void Main(string[] args)
        {
            using (var game = new Hausaufgabe())
            {
                game.Run();
            }
        }
    }
#endif
}

