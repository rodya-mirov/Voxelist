using System;

namespace VoxelistDemo3
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (DemoGame3 game = new DemoGame3())
            {
                game.Run();
            }
        }
    }
#endif
}

