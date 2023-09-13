using System;
using System.Text;

namespace Cube.Utility
{
    /// <summary>
    /// Cleans paths of invalid characters.
    /// </summary>
    public static class PathSanitizer
    {
        /// <summary>
        /// The set of invalid filename characters, kept sorted for fast binary search
        /// </summary>
        private static readonly char[] InvalidFilenameChars;
        /// <summary>
        /// The set of invalid path characters, kept sorted for fast binary search
        /// </summary>
        private static readonly char[] InvalidPathChars;

        static PathSanitizer()
        {
            InvalidFilenameChars = System.IO.Path.GetInvalidFileNameChars();
            InvalidPathChars = System.IO.Path.GetInvalidPathChars();
            Array.Sort(InvalidFilenameChars);
            Array.Sort(InvalidPathChars);
        }

        /// <summary>
        /// Cleans a filename of invalid characters
        /// </summary>
        /// <param name="input">the string to clean</param>
        /// <param name="errorChar">the character which replaces bad characters</param>
        /// <returns></returns>
        public static string SanitizeFilename(string input, char? errorChar = null)
        {
            return Sanitize(input, InvalidFilenameChars, errorChar);
        }

        /// <summary>
        /// Cleans a path of invalid characters
        /// </summary>
        /// <param name="input">the string to clean</param>
        /// <param name="errorChar">the character which replaces bad characters</param>
        /// <returns></returns>
        public static string SanitizePath(string input, char? errorChar = null)
        {
            return Sanitize(input, InvalidPathChars, errorChar);
        }

        /// <summary>
        /// Cleans a string of invalid characters.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="invalidChars"></param>
        /// <param name="errorChar"></param>
        /// <returns></returns>
        private static string Sanitize(string input, char[] invalidChars, char? errorChar = null)
        {
            // null always sanitizes to null
            if (input == null) { return null; }
            StringBuilder result = new StringBuilder();
            foreach (var characterToTest in input)
            {
                // we binary search for the character in the invalid set. This should be lightning fast.
                if (Array.BinarySearch(invalidChars, characterToTest) >= 0)
                {
                    if (errorChar.HasValue)
                    {
                        result.Append(errorChar);
                    }
                }
                else
                {
                    result.Append(characterToTest);
                }
            }

            return result.ToString();
        }


        public static bool IsInvalidFileName(string fileName)
        {
            return fileName.Split(System.IO.Path.GetInvalidFileNameChars()).Length > 1;
        }

        public static bool IsInvalidPath(string path)
        {
            return path.Split(System.IO.Path.GetInvalidPathChars()).Length > 1;
        }



    }

}
