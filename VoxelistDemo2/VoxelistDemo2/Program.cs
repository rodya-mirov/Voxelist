using System;

namespace VoxelistDemo2
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (DemoGame2 game = new DemoGame2())
            {
                game.Run();
            }
        }
    }
#endif
}

