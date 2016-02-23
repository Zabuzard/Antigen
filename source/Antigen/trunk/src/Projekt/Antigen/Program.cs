using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AntigenTests")]
namespace Antigen
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //Resharper hint: Arguments are not used by the game but method signature needs to be like that.
// ReSharper disable UnusedParameter.Local
        static void Main(string[] args)
// ReSharper restore UnusedParameter.Local
        {
            using (var game = new Antigen())
            {
                game.Run();
            }
        }
    }
#endif
}

