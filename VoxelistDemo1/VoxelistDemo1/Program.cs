using System;

namespace VoxelistDemo1
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (DemoGame1 game = new DemoGame1())
            {
                game.Run();
            }
        }
    }
#endif
}

