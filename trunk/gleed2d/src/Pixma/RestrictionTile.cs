using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GLEED2D
{
    class RestrictionTile : TextureItem
    {
        public static Texture2D tileTexture = null;

        public RestrictionTile()
        {
            
        }

        public RestrictionTile(Vector2 cellspace)
        {
            if (tileTexture == null)
            {
                System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                Stream myStream = myAssembly.GetManifestResourceStream("GLEED2D.tile70.png");
                System.Drawing.Bitmap image = new System.Drawing.Bitmap(myStream);
            }
        }
    }
}
