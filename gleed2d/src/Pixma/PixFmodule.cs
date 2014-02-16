using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace GLEED2D
{
    public class PixFmodule
    {
        Texture2D mTexture;
        Matrix mLocalTrans;
        

        // cache for render
        Matrix _drawMatrix;
        Vector2 _pos;

        public PixFmodule(Texture2D texture, Matrix transform)
        {
            this.mTexture = texture;
            this.mLocalTrans = transform;
            // cache for render
            _drawMatrix = Matrix.Identity;
            _pos = new Vector2();
        }

        public void draw(SpriteBatch sb, Matrix worldMatrix, Color c)
        {
            _drawMatrix = mLocalTrans * worldMatrix;
           
            sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, _drawMatrix);
            sb.GraphicsDevice.RenderState.CullMode = CullMode.None; 
            sb.Draw(mTexture, _pos, c);
            sb.End();

            
        }
    }
}
