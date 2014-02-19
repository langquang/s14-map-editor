using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Graphics.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;


namespace GLEED2D
{
    [Serializable]
    public partial class PixFrame : TextureItem
    {
        [XmlIgnore()]
        private string _frameName;

        [Browsable(false)]  
        public string FrameName
        {
            get{return _frameName;}
            set { _frameName = value; }
        }
        [XmlIgnore()]
        private int frameId = -1;
        [XmlIgnore()]
        private ArrayList aframes;
        [XmlIgnore()]
        private bool useBitmap;
        // use bitmap mode
        [XmlIgnore()]
        private System.Drawing.Bitmap bitmap;
        [XmlIgnore()]
        private Matrix bitmap_trans;
        [XmlIgnore()]
        private Rectangle frame_rect;

        public PixFrame()
        {
            
        }

        public PixFrame(int frameId, string fullpath, Vector2 position)
        {
            CustomProperties = new SerializableDictionary();
            this.frameId = frameId;
            this.texture_fullpath = fullpath;
            aframes = new ArrayList();
            this.Position = position;
            this.Rotation = 0;
            this.Scale = Vector2.One;
            this.TintColor = Microsoft.Xna.Framework.Graphics.Color.White;
            FlipHorizontally = FlipVertically = false;
            this.Origin = Vector2.Zero;
            loadIntoEditor();
        }

        // Auto call in Pixma, don't call it
        public void addModule(PixFmodule module)
        {
            aframes.Add(module);
            useBitmap = false;
        }

        public void addModule_bitmap(System.Drawing.Bitmap bitmap, Matrix bitmap_trans, Rectangle frame_rect)
        {
            this.bitmap = bitmap;
            texture = Utils.BitmapToTexture2D(Game1.Instance.GraphicsDevice, this.bitmap);
            this.bitmap_trans = bitmap_trans;
            this.frame_rect = frame_rect;
            _pos = new Vector2();
            useBitmap = true;
        }

        public override void drawInEditor(SpriteBatch sb)
        {
            if (!Visible) return;

            worldMatrix = Matrix.Identity;
            if (useBitmap) worldMatrix *= bitmap_trans;
            worldMatrix *= Matrix.CreateScale(Scale.X, Scale.Y, 1);
            worldMatrix *= Matrix.CreateRotationZ(Rotation);
            if (pFlipHorizontally) worldMatrix *= Matrix.CreateScale(-1, 1, 1);
            if (pFlipVertically) worldMatrix *= Matrix.CreateScale(1, -1, 1);
            
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
                sb.Draw(texture, _pos, c);
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
                sb.Draw(texture, _pos, c);
                sb.End();
            }
 
        }

        public System.Drawing.Bitmap getBitmapView()
        {
            int w = texture.Width;
            int h = texture.Height;
            Color c;
            System.Drawing.Bitmap bm = new System.Drawing.Bitmap(w, h);
            //Get the pixel data from the original texture:
            Color[] originalData = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(originalData);

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

        public void setPosition( float x, float y )
        {
            Position = new Vector2(x,y);
        }

        public override void OnTransformed()
        {
            transform = Matrix.Identity;
            transform *= Matrix.CreateScale(Scale.X, Scale.Y, 1);
            transform *= Matrix.CreateRotationZ(Rotation);
            if (pFlipHorizontally) transform *= Matrix.CreateScale(-1, 1, 1);
            if (pFlipVertically) transform *= Matrix.CreateScale(1, -1, 1);
            transform *= Matrix.CreateTranslation(Position.X, Position.Y, 0);

            Vector2 leftTop = new Vector2(frame_rect.X, frame_rect.Y );
            Vector2 rightTop = new Vector2(frame_rect.X + frame_rect.Width, frame_rect.Y);
            Vector2 leftBottom = new Vector2(frame_rect.X, frame_rect.Y + frame_rect.Height);
            Vector2 rightBottom = new Vector2(frame_rect.X + frame_rect.Width, frame_rect.Y + frame_rect.Height);

            // Transform all four corners into work space
            Vector2.Transform(ref leftTop, ref transform, out leftTop);
            Vector2.Transform(ref rightTop, ref transform, out rightTop);
            Vector2.Transform(ref leftBottom, ref transform, out leftBottom);
            Vector2.Transform(ref rightBottom, ref transform, out rightBottom);

            polygon[0] = leftTop;
            polygon[1] = rightTop;
            polygon[3] = leftBottom;
            polygon[2] = rightBottom;

            // Find the minimum and maximum extents of the rectangle in world space
            Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop),
                                      Vector2.Min(leftBottom, rightBottom));
            Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop),
                                      Vector2.Max(leftBottom, rightBottom));

            // Return as a rectangle
            boundingrectangle = new Rectangle((int)min.X, (int)min.Y,
                                 (int)(max.X - min.X), (int)(max.Y - min.Y));
        }

        public override bool contains(Vector2 worldpos)
        {
            if (boundingrectangle.Contains((int)worldpos.X, (int)worldpos.Y))
            {
                return intersectpixels(worldpos);
            }
            return false;
        }

        public override bool intersectpixels(Vector2 worldpos)
        {
            Vector2 positionInB = Vector2.Transform(worldpos, Matrix.Invert(transform));
            int xB = (int)Math.Round(positionInB.X);
            int yB = (int)Math.Round(positionInB.Y);
            xB -= frame_rect.X;
            yB -= frame_rect.Y;

            // If the pixel lies within the bounds of B
            if (0 <= xB && xB < texture.Width && 0 <= yB && yB < texture.Height)
            {
                Color colorB = coldata[xB + yB * texture.Width];
                if (colorB.A != 0)
                {
                    return true;
                }
            }
            return false;
        }

        public override string getNamePrefix()
        {
            return "Frame_";
        }

        public override bool loadIntoEditor()
        {
            if (layer != null) this.texture_fullpath = System.IO.Path.Combine(layer.level.ContentRootFolder + "\\", texture_filename);

            if (!File.Exists(texture_fullpath))
            {
                DialogResult dr = System.Windows.Forms.MessageBox.Show("The file \"" + texture_fullpath + "\" doesn't exist!\n"
                    + "The texture path is a combination of the Level's ContentRootFolder and the TextureItem's relative path.\n"
                    + "Please adjust the XML file before trying to load this level again.\n"
                    + "For now, a dummy texture will be used. Continue loading the level?", "Error loading texture file",
                    MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Error);
                if (dr == DialogResult.No) return false;
                texture = Editor.Instance.dummytexture;
            }
            else
            {
                Pixma pixma = PixmaManager.getCache(this.texture_fullpath);
                if (pixma == null)
                {
                    pixma = new Pixma(texture_filename);
                    pixma.load(this.texture_fullpath);
                    PixmaManager.cache(this.texture_fullpath, pixma);
                }

                if( this.frameId < 0 )
                    frameId = pixma.getFrameIndex(FrameName);
                pixma.GetFrame_bitmap(this);
            }

            //for per-pixel-collision
            coldata = new Color[texture.Width * texture.Height];
            texture.GetData(coldata);

            polygon = new Vector2[4];
            OnTransformed();

            return true;
        }

        public int getFrameId()
        {
            return frameId;
        }
    }

}
