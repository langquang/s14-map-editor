using System.Collections.Generic;

namespace GLEED2D
{
    class PixmaManager
    {
        private static Dictionary<string, Pixma> dictionary = new Dictionary<string, Pixma>();

        public static Pixma cache(string full_path, Pixma pixma)
        {
            dictionary.Add(full_path, pixma);
            return pixma;
        }

        public static Pixma getCache(string full_path)
        {
            if (dictionary.ContainsKey(full_path))
            {
                return dictionary[full_path];
            }
            else
            {
                return null;
            }
        }
    }
}
