using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GLEED2D
{
    public class PixMatrix
    {
        public float a;
        public float b;
        public float c;
        public float d;
        public float tx;
        public float ty;

        public bool flip_h;
        public bool flip_v;

        public PixMatrix()
        {

        }

        public Vector2 transform(Vector2 v)
        {
            Vector2 v2 = new Vector2();
            v2.X = v.X * a + v.Y * c + tx;
            v2.Y = v.X * b + v.Y * d + ty;
            return v2;
        }

        public static void decomposit(PixMatrix transform)
        {
            /**
             * scaleX = √(a^2+c^2)
             * scaleY = √(b^2+d^2)
             * rotation = tan^-1(c/d) = tan^-1(-b/a) it will not work sometimes 
             * rotation = a / scaleX  = d / scaleY
             */
            Vector2 Scale = new Vector2();
            double Rotation;
            double a = transform.a;
            double b = transform.b;
            double c = transform.c;
            double d = transform.d;
            Scale.X = (float)Math.Sqrt((a * a) + (c * c));
            Scale.Y = (float)Math.Sqrt((b * b) + (d * d));

            double sign = Math.Atan(-c / a);
            double rad = Math.Acos(a / Scale.X);
            double deg = rad * 180 / Math.PI;
            if (deg > 90 && sign > 0)
            {
                Rotation = (360 - deg) * Math.PI / 180;
            }
            else if (deg < 90 && sign < 0)
            {
                Rotation = (360 - deg) * Math.PI / 180;
            }
            else
            {
                Rotation = rad;
            }
        }
    }
}

            