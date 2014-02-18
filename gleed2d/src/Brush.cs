using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace GLEED2D
{

    class Brush
    {
        public String fullpath;
        public Texture2D texture;
        public PixFrame frame;
        public string type;
        public int pixma_id;

        public Brush(String fullpath, string type, string pixma_id)
        {
            this.fullpath = fullpath;
            this.type = type;
            this.pixma_id = Convert.ToInt32(pixma_id);
            if (this.type == Define.TYPE_IMAGE)
            {
                this.texture = TextureLoader.Instance.FromFile(Game1.Instance.GraphicsDevice, this.fullpath);
            }
            else if (this.type == Define.TYPE_FRAME)
            {
                Pixma pixma = PixmaManager.getCache(this.fullpath);
                frame = pixma.GetFrame_bitmap(this.pixma_id, Vector2.Zero);
                frame.Visible = true;
            }
        }

        public void draw(SpriteBatch sb, Vector2 pos)
        {

            if (this.type == Define.TYPE_IMAGE)
            {
                sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None, Editor.Instance.camera.matrix);
                sb.Draw(this.texture, pos, null, new Color(1f, 1f, 1f, 0.7f), 0, new Vector2(this.texture.Width / 2, this.texture.Height / 2), 1, SpriteEffects.None, 0);
                sb.End();
            }
            else if (this.type == Define.TYPE_FRAME)
            {
                frame.setPosition(pos.X, pos.Y);
                frame.drawInEditor(sb);
            }
        }
    }



}
