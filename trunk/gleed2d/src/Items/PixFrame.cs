using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Forms = System.Windows.Forms;
using CustomUITypeEditors;
using System.Windows.Forms;
using System.IO;

namespace GLEED2D
{
    public partial class PixFrame
    {
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



            Scale = new Vector2(1,1);
            TintColor = new Color(255,255,255,255);
           // texture = TextureLoader.Instance.FromFile(Game1.Instance.GraphicsDevice, @"d:\JavaServer\Code\CSharp\gleed2d_svn_client\gleed2d\bin\x86\Debug\images\11.png");
            
            //for per-pixel-collision
            //coldata = new Color[texture.Width * texture.Height];
            //texture.GetData(coldata);

            polygon = new Vector2[4];

            //OnTransformed();
            return true;
        }

    }
}
