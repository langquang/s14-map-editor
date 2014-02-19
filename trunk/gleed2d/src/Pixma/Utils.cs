using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;



namespace GLEED2D
{
    class Utils
    {
        public static bool hasFlags(int flags ,int value)
        {
			return ((flags & value) == value);
		}

        public static  string getBlendModeint(int mode)
        {
            return "normal";
        }

        public static Rectangle transformRect(Rectangle bounds, Matrix m)
        {
            Vector2 topLeft = new Vector2(bounds.Left, bounds.Top);
            topLeft = Vector2.Transform(topLeft, m);


            Vector2 topRight = new Vector2(bounds.Right, bounds.Top);
            topRight = Vector2.Transform(topRight, m);


            Vector2 bottomRight = new Vector2(bounds.Right, bounds.Bottom);
            bottomRight = Vector2.Transform(bottomRight, m);


            Vector2 bottomLeft = new Vector2(bounds.Left, bounds.Bottom);
            bottomLeft = Vector2.Transform(bottomLeft, m);


            float left = Math.Min(Math.Min(Math.Min(topLeft.X, topRight.X) , bottomRight.X) , bottomLeft.X);
			float top = Math.Min(Math.Min(Math.Min(topLeft.Y, topRight.Y), bottomRight.Y), bottomLeft.Y);
			float right = Math.Max(Math.Max(Math.Max(topLeft.X, topRight.X), bottomRight.X), bottomLeft.X);
			float bottom = Math.Max(Math.Max(Math.Max(topLeft.Y, topRight.Y), bottomRight.Y), bottomLeft.Y);
			return new Rectangle((int)left, (int)top, (int)(right - left),(int)(bottom - top));
        }

        public static float getRectangleScaleX(Rectangle src, Rectangle dest)
        {
            Vector2 topLeft = new Vector2(src.Left, src.Top);
            Vector2 topRight = new Vector2(src.Right, src.Top);
            float length_src = Vector2.Distance(topLeft, topRight);

            topLeft = new Vector2(dest.Left, dest.Top);
            topRight = new Vector2(dest.Right, dest.Top);
            float length_dest = Vector2.Distance(topLeft, topRight);

            return length_dest / length_src;
        }

        public static float getRectangleScaleY(Rectangle src, Rectangle dest)
        {
            Vector2 topLeft = new Vector2(src.Left, src.Top);
            Vector2 bottomLeft = new Vector2(src.Left, src.Bottom);
            float length_src = Vector2.Distance(topLeft, bottomLeft);

            topLeft = new Vector2(dest.Left, dest.Top);
            bottomLeft = new Vector2(dest.Right, dest.Top);
            float length_dest = Vector2.Distance(topLeft, bottomLeft);

            return length_dest / length_src;
        }

        /// <summary>
        /// Splits a texture into an array of smaller textures of the specified size.
        /// </summary>
        /// <param name="original">The texture to be split into smaller textures</param>
        /// <param name="partWidth">The width of each of the smaller textures that will be contained in the returned array.</param>
        /// <param name="partHeight">The height of each of the smaller textures that will be contained in the returned array.</param>
        public Texture2D[] Split(Texture2D original, int partWidth, int partHeight, out int xCount, out int yCount)
        {
            yCount = original.Height / partHeight + (partHeight % original.Height == 0 ? 0 : 1);//The number of textures in each horizontal row
            xCount = original.Height / partHeight + (partHeight % original.Height == 0 ? 0 : 1);//The number of textures in each vertical column
            Texture2D[] r = new Texture2D[xCount * yCount];//Number of parts = (area of original) / (area of each part).
            int dataPerPart = partWidth * partHeight;//Number of pixels in each of the split parts

            //Get the pixel data from the original texture:
            Color[] originalData = new Color[original.Width * original.Height];
            original.GetData<Color>(originalData);

            int index = 0;
            for (int y = 0; y < yCount * partHeight; y += partHeight)
                for (int x = 0; x < xCount * partWidth; x += partWidth)
                {
                    //The texture at coordinate {x, y} from the top-left of the original texture
                    Texture2D part = new Texture2D(original.GraphicsDevice, partWidth, partHeight);
                    //The data for part
                    Color[] partData = new Color[dataPerPart];

                    //Fill the part data with colors from the original texture
                    for (int py = 0; py < partHeight; py++)
                        for (int px = 0; px < partWidth; px++)
                        {
                            int partIndex = px + py * partWidth;
                            //If a part goes outside of the source texture, then fill the overlapping part with Color.Transparent
                            if (y + py >= original.Height || x + px >= original.Width)
                                partData[partIndex] = Color.TransparentWhite;
                            else
                                partData[partIndex] = originalData[(x + px) + (y + py) * original.Width];
                        }

                    //Fill the part with the extracted data
                    part.SetData<Color>(partData);
                    //Stick the part in the return array:                    
                    r[index++] = part;
                }
            //Return the array of parts.
            return r;
        }

        public static void copyPixel(Texture2D src, Texture2D dest, Rectangle rect_src, Vector2 dest_point)
        {
            //Get the pixel data from the original texture:
            Color[] originalData = new Color[src.Width * src.Height];
            src.GetData<Color>(originalData);

            Color[] partData = new Color[dest.Width * dest.Height];
            dest.GetData<Color>(partData);

            //Fill the part data with colors from the original texture
            int i = 0;
            for (int py = rect_src.Top; py < rect_src.Bottom; py++)
                for (int px = rect_src.Left; px < rect_src.Right; px++)
                {
                    int partIndex = px + py * src.Width;
                    //If a part goes outside of the source texture, then fill the overlapping part with Color.Transparent
                    if (py < src.Height && px < src.Width)
                    {
                        partData[i] = originalData[partIndex];
                    }
                        
                    i++;
                }

            //Fill the part with the extracted data
            dest.SetData<Color>(partData);
        }

        public static void draw(System.Drawing.Graphics backbuffer, System.Drawing.Bitmap drawData, Matrix transform)
        {
            System.Drawing.Drawing2D.Matrix _matrix = new System.Drawing.Drawing2D.Matrix(transform.M11, transform.M12, transform.M21, transform.M22, transform.M41, transform.M42);
            backbuffer.Transform = _matrix;
            backbuffer.DrawImage(drawData, 0,0);

        }

        public static void copyPixel(System.Drawing.Bitmap src, System.Drawing.Bitmap dest, Rectangle rect_src, Vector2 dest_point)
        {
            //Fill the part data with colors from the original texture
            int x = 0, y = 0;
            System.Drawing.Color c;
            for (int py = rect_src.Top; py < rect_src.Bottom; py++)
            {
                x = 0;
                for (int px = rect_src.Left; px < rect_src.Right; px++)
                {
                    c = src.GetPixel(px, py);
                    dest.SetPixel(x, y, c);
                    x++;
                }
                y++;
            }
        }

        public static Texture2D BitmapToTexture2D(  
        	    GraphicsDevice GraphicsDevice,   
        	    System.Drawing.Bitmap image)  
    	{
//            int width = image.Width;
//            int height = image.Height;
//
//            Color[] pixels = new Color[width * height];
//            for (int y = 0; y < height; y++)
//            {
//                for (int x = 0; x < width; x++)
//                {
//                    System.Drawing.Color c = image.GetPixel(x, y);
//                    pixels[(y * width) + x] = new Color(c.R, c.G, c.B, c.A);
//                }
//            }
//
//            Texture2D myTex = new Texture2D(GraphicsDevice,width,height);
//            myTex.SetData<Color>(pixels);
//            return myTex;

            Texture2D tx = null;
            using (MemoryStream s = new MemoryStream())
            {
                image.Save(s, System.Drawing.Imaging.ImageFormat.Png);
                s.Seek(0, SeekOrigin.Begin); //must do this, or error is thrown in next line
                tx = Texture2D.FromFile(GraphicsDevice, s);
            }
            return tx;
    	} 

    }
}
