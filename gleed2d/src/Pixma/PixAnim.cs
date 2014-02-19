using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GLEED2D
{
    public class PixAnim : PixFrame, Animation 
    {
        private ArrayList mFrames;
        private ArrayList mFrame_times;
        private PixFrame m1StFrame;
        private PixFrame mFrame;
        private int anim_id;
        private int mCurFrame;
        private int mCurFrame_time;
        private bool mIsStop;


        public PixAnim():base()
        {
            mFrames = new ArrayList();
            mFrame_times = new ArrayList();
            mCurFrame = 0;
        }

        public PixAnim(int animId, string fullpath, Vector2 position)
        {
            mFrames = new ArrayList();
            mFrame_times = new ArrayList();
            mCurFrame = 0;

            CustomProperties = new SerializableDictionary();
            this.anim_id = animId;
            this.texture_fullpath = fullpath;
            this.Position = position;
            this.Rotation = 0;
            this.Scale = Vector2.One;
            this.TintColor = Microsoft.Xna.Framework.Graphics.Color.White;
            FlipHorizontally = FlipVertically = false;
            this.Origin = Vector2.Zero;
            loadIntoEditor();

            Game1.Instance.jugger.add(this);
        }

        public void addFrame(PixFrame frame, int time)
        {
            mFrames.Add(frame);
            mFrame_times.Add(time);

            m1StFrame = (PixFrame)mFrames[0];
            mFrame = m1StFrame;
        }

        // 30fps
        public void update()
        {
            if (!mIsStop && mCurFrame_time > 0)
            {
                mCurFrame_time--;

                if (mCurFrame_time == 0)
                {
                    mCurFrame++;

                    if (mCurFrame < mFrames.Count)
                    {
                        mCurFrame_time = (int)mFrame_times[mCurFrame];
                        mFrame = (PixFrame)mFrames[mCurFrame];
                    }
                    else
                    {
                        mCurFrame = 0;
                        mCurFrame_time = (int)mFrame_times[0];
                    }
                }
            }
            else
            {
                mCurFrame_time = (int)mFrame_times[mCurFrame];
            }
        }

        public override void drawInEditor(SpriteBatch sb)
        {
            if (!Visible) return;

            worldMatrix = Matrix.Identity;
            worldMatrix *= Matrix.CreateScale(Scale.X, Scale.Y, 1);
            worldMatrix *= Matrix.CreateRotationZ(Rotation);
            if (pFlipHorizontally) worldMatrix *= Matrix.CreateScale(-1, 1, 1);
            if (pFlipVertically) worldMatrix *= Matrix.CreateScale(1, -1, 1);
            worldMatrix *= Matrix.CreateTranslation(Position.X, Position.Y, 0);
            // translate to view 
            worldMatrix *= Editor.Instance.camera.matrix;

            Color c = TintColor;
            if (hovering && Constants.Instance.EnableHighlightOnMouseOver) c = Constants.Instance.ColorHighlight;
            if (mFrame != null)
            {
                mFrame.drawInAnim(sb, worldMatrix, c);
            }

        }


        public void stop(int frame)
		{
			if( frame < 0 || frame >= mFrames.Count)
				frame = 0;
            mFrame = (PixFrame)mFrames[frame];
			mIsStop = true;
		}

        public void play()
        {
            mIsStop = false;
        }

        public int getAnimId()
        {
            return anim_id;
        }


        public override System.Drawing.Bitmap getBitmapView()
        {
            PixFrame frame = (PixFrame) mFrames[0];
            return frame.getBitmapView();
        }

        public override void OnTransformed()
        {
            m1StFrame.setPosition(Position.X, Position.Y);
            m1StFrame.setScale(new Vector2(Scale.X, Scale.Y));
            m1StFrame.pFlipHorizontally = pFlipHorizontally;
            m1StFrame.pFlipVertically = pFlipVertically;
            m1StFrame.Rotation = Rotation;

            m1StFrame.OnTransformed();

        }

        public override bool contains(Vector2 worldpos)
        {
            return m1StFrame.contains(worldpos);
        }

        public override bool intersectpixels(Vector2 worldpos)
        {
            return m1StFrame.intersectpixels(worldpos);
        }

        public override string getNamePrefix()
        {
            return "Anim_";
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

                if (this.anim_id < 0)
                    anim_id = pixma.getAnimIndex(FrameName);
                pixma.GetAnim_bitmap(this);
            }

            polygon = new Vector2[4];
            OnTransformed();

            return true;
        }

        public override void drawSelectionFrame(SpriteBatch sb, Matrix matrix, Color color)
        {
            m1StFrame.drawSelectionFrame(sb, matrix, color);
        }

    }
}
