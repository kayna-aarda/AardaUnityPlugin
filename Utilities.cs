
using System;

// namespace MySharedLibrary
namespace AardaLibrary
{
    public static class Utilities
    {
        /// <summary>
        /// Returns the sum of two integers.
        /// </summary>
        public static int Add(int a, int b)
        {
            return a + b;
        }

        /// <summary>
        /// Logs a message with a timestamp to the console (for demonstration).
        /// </summary>
        public static void LogWithTimestamp(string message)
        {
            string timestamp = DateTime.UtcNow.ToString("o");
            Console.WriteLine($"[{timestamp}] {message}");
        }

        /// <summary>
        /// Example method to simulate a utility function in a game context.
        /// </summary>
        public static float CalculateDistance(float x1, float y1, float x2, float y2)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}

