using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GLEED2D.src.Utils
{
    class IsoMath
    {
        public static float ratio = 2;
        public static Vector2 screenToIso(Vector2 screenPt, bool createNew)
        {
			float y = screenPt.Y - screenPt.X / ratio;
            float x = screenPt.X / ratio + screenPt.Y;

            if (!createNew)
            {
                screenPt.X = x;
                screenPt.Y = y;
                return screenPt;
            }
            else
            {
                return new Vector2(x, y);
            }
        }

        public static Vector2 isoToScreen(Vector2 isoPt, bool createNew)
        {
			float y = (isoPt.X + isoPt.Y) / ratio;
            float x = isoPt.X - isoPt.Y;

            if (!createNew)
            {
                isoPt.X = x;
                isoPt.Y = y;
                return isoPt;
            }
            else
            {
                return new Vector2(x, y);
            }
        }
    }
}
