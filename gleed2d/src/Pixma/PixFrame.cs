using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GLEED2D
{
    public partial class PixFrame : TextureItem
    {
        private ArrayList aframes;
        private Matrix worldMatrix;

        public PixFrame()
        {
            aframes = new ArrayList();
        }

        // Auto call in Pixma, don't call it
        public void addModule(PixFmodule module)
        {
            aframes.Add(module);
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
            foreach (PixFmodule module in aframes)
            {
                module.draw(sb, worldMatrix, c);
            }
        }

        public void drawInAnim(SpriteBatch sb, Matrix worldMatrix, Color c)
        {
            foreach (PixFmodule module in aframes)
            {
                module.draw(sb, worldMatrix, c);
            }
        }


    }

}
