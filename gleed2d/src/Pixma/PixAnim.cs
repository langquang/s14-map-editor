using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GLEED2D
{
    class PixAnim : TextureItem, Animation 
    {
        private ArrayList mFrames;
        private ArrayList mFrame_times;
        private PixFrame mFrame;
        private int mCurFrame;
        private int mCurFrame_time;
        private bool mIsStop;


        public PixAnim()
        {
            mFrames = new ArrayList();
            mFrame_times = new ArrayList();
            mCurFrame = 0;
        }

        public void addFrame(PixFrame frame, int time)
        {
            mFrames.Add(frame);
            mFrame_times.Add(time);
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

        public override bool loadIntoEditor()
        {
            //if (layer != null) this.texture_fullpath = System.IO.Path.Combine(layer.level.ContentRootFolder + "\\", texture_filename);

            //if (!File.Exists(texture_fullpath))
            //{
            //    DialogResult dr = Forms.MessageBox.Show("The file \"" + texture_fullpath + "\" doesn't exist!\n"
            //        + "The texture path is a combination of the Level's ContentRootFolder and the TextureItem's relative path.\n"
            //        + "Please adjust the XML file before trying to load this level again.\n"
            //        + "For now, a dummy texture will be used. Continue loading the level?", "Error loading texture file",
            //        MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Error);
            //    if (dr == DialogResult.No) return false;
            //    texture = Editor.Instance.dummytexture;
            //}
            //else
            //{
            //    Pixma pixma = new Pixma("test");
            //    pixma.load(@"D:\JavaServer\Code\CSharp\gleed2d_svn_client\gleed2d\bin\x86\Debug\images\atiso.anim");
            //    texture = pixma.GetFrame(0);
            //}



            Scale = new Vector2(1, 1);
            TintColor = new Color(255, 255, 255, 255);
            // texture = TextureLoader.Instance.FromFile(Game1.Instance.GraphicsDevice, @"d:\JavaServer\Code\CSharp\gleed2d_svn_client\gleed2d\bin\x86\Debug\images\11.png");

            //for per-pixel-collision
            //coldata = new Color[texture.Width * texture.Height];
            //texture.GetData(coldata);

            polygon = new Vector2[4];

            //OnTransformed();
            return true;
        }

        public void stop(int frame)
		{
			if( frame < 0 || frame >= mFrames.Count)
				frame = mFrames.Count - 1;
            mFrame = (PixFrame)mFrames[frame];
			mIsStop = true;
		}

        public void play()
        {
            mIsStop = false;
        }
    }
}
