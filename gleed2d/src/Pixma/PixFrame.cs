﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Graphics.Color;


namespace GLEED2D
{
    public partial class PixFrame : TextureItem
    {
        private string name;
        private ArrayList aframes;
        private bool useBitmap;
        // use bitmap mode
        private System.Drawing.Bitmap bitmap;
        Texture2D mTexture;
        private Matrix bitmap_trans;
        // cache for render
        Vector2 _pos;
        private Matrix worldMatrix;

        public PixFrame(string name)
        {
            this.name = name;
            aframes = new ArrayList();
        }

        // Auto call in Pixma, don't call it
        public void addModule(PixFmodule module)
        {
            aframes.Add(module);
            useBitmap = false;
        }

        public string getName()
        {
            return name;
        }

        public void addModule_bitmap(System.Drawing.Bitmap bitmap, Matrix bitmap_trans)
        {
            this.bitmap = bitmap;
            mTexture = Utils.BitmapToTexture2D(Game1.Instance.GraphicsDevice, this.bitmap);
            this.bitmap_trans = bitmap_trans;
            _pos = new Vector2();
            useBitmap = true;   
        }

        public override void drawInEditor(SpriteBatch sb)
        {
            if (!Visible) return;

            worldMatrix = Matrix.Identity;
            worldMatrix *= Matrix.CreateScale(Scale.X, Scale.Y, 1);
            worldMatrix *= Matrix.CreateRotationZ(Rotation);
            if (pFlipHorizontally) worldMatrix *= Matrix.CreateScale(-1, 1, 1);
            if (pFlipVertically) worldMatrix *= Matrix.CreateScale(1, -1, 1);
            if (useBitmap) worldMatrix *= bitmap_trans;
            worldMatrix *= Matrix.CreateTranslation(Position.X, Position.Y, 0);
            // translate to view 
            worldMatrix *= Editor.Instance.camera.matrix;

            Color c = TintColor;
            if (hovering && Constants.Instance.EnableHighlightOnMouseOver) c = Constants.Instance.ColorHighlight;
            if (!useBitmap)
            {
                foreach (PixFmodule module in aframes)
                {
                    module.draw(sb, worldMatrix, c);
                }
            }
            else
            {
                sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, worldMatrix);
                sb.GraphicsDevice.RenderState.CullMode = CullMode.None;
                sb.Draw(mTexture, _pos, c);
                sb.End();
            }
        }

        public void drawInAnim(SpriteBatch sb, Matrix worldMatrix, Color c)
        {
            if (!useBitmap)
            {
                foreach (PixFmodule module in aframes)
                {
                    module.draw(sb, worldMatrix, c);
                }
            }
            else
            {
                worldMatrix = bitmap_trans * worldMatrix;

                sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, worldMatrix);
                sb.GraphicsDevice.RenderState.CullMode = CullMode.None;
                sb.Draw(mTexture, _pos, c);
                sb.End();
            }
 
        }

        public System.Drawing.Bitmap getBitmapView()
        {
            int w = mTexture.Width;
            int h = mTexture.Height;
            Color c;
            System.Drawing.Bitmap bm = new Bitmap(w,h);
            //Get the pixel data from the original texture:
            Color[] originalData = new Color[mTexture.Width * mTexture.Height];
            mTexture.GetData<Color>(originalData);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    c = originalData[y*w + x];
                    bm.SetPixel(x, y, System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B));
                }
            }
            return bm;
        }
    }

}
