using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace GLEED2D.src.Pixma
{
    public class Generator
    {

        public static string LAYER_TILE = "TILES";
        public static string LAYER_MODULE = "MODULES";
        public static string LAYER_DECOS = "DECOS";
        public static string LAYER_RESTRICTION = "RESTRICTION";
        public static string LAYER_WORLD_OBJECT = "OBJECTS";
        public static string LAYER_ITEMS = "ITEMS";
        public static string NAME_MASK = "MASK";
        public static string NAME_MAP = "MAP";


        public static void createImages(Layer layer, ArrayList files, ArrayList pixmas)
        {
            foreach (Item i in layer.Items)
            {
                if (i is TextureItem)
                {
                    TextureItem ti = (TextureItem)i;
                    string filename = Path.GetFileNameWithoutExtension(ti.texture_fullpath);
                    if (filename != null && !files.Contains(filename))
                    {
                        files.Add(filename);
                    }
                }

                if (i is PixFrame)
                {
                    PixFrame frame = (PixFrame)i;
                    if (!pixmas.Contains(frame.FrameName))
                    {
                        pixmas.Add(frame.FrameName);
                    }
                }
            }
        }


        public static JObject CreateMapModules(Layer layer)
        {
            ArrayList files  = new ArrayList();
            ArrayList pixmas = new ArrayList();
            ArrayList modules = new ArrayList();

            createImages(layer, files, pixmas);

            foreach (Item i in layer.Items)
            {
                ModuleMap module = new ModuleMap();
                module.setPosition(i.Position, false);
                module.setScale(i.getScale());
                module.setRotation(i.getRotation());

                if (i is TextureItem)
                {
                    TextureItem ti = (TextureItem)i;
                    module.setFlipHorizontally(ti.FlipHorizontally);
                    module.setFlipVertically(ti.FlipVertically);
                    string filename = Path.GetFileNameWithoutExtension(ti.texture_fullpath);
                    int file_index = files.IndexOf(filename);
                    module.setImage(file_index, -1, i);

                    if (i is PixFrame)
                    {
                        PixFrame frame = (PixFrame)i;
                        module.setImage(file_index, pixmas.IndexOf(frame.FrameName), i);
                    }
                    
                }

                modules.Add(module.encode());
            }

            JArray entitys = new JArray((JObject[])modules.ToArray(typeof(JObject)));

            JObject json = new JObject();
            json.Add("imgs", new JArray((string[])files.ToArray(typeof(string))));
            json.Add("pixs", new JArray((string[])pixmas.ToArray(typeof(string))));
            json.Add("list", entitys);

            return json;
        }

        public static JArray CreateRestriction(Layer layer)
        {
            ArrayList array = new ArrayList();
            foreach (Item i in layer.Items)
            {
                if (i is TextureItem)
                {
                    TextureItem ti = (TextureItem) i;
                    if (ti.texture_filename.ToUpper().IndexOf(NAME_MASK) != -1)
                    {
                        Vector2 cell = i.pIsoCell;
                        array.Add((int)cell.X);
                        array.Add((int)cell.Y);
                    }
                }
            }
            return new JArray((int[])array.ToArray(typeof(int)));
        }

        public static JObject CreateTiles(Layer layer)
        {
            JObject obj = new JObject();
            foreach (Item i in layer.Items)
            {
                if (i is TextureItem)
                {
                    TextureItem ti = (TextureItem)i;
                    if (ti.texture_filename.ToUpper().IndexOf(NAME_MAP) != -1)
                    {
                        obj.Add("x", (int)ti.Position.X);
                        obj.Add("y", (int)ti.Position.Y);
                        obj.Add("img", Path.GetFileNameWithoutExtension(ti.texture_fullpath));
                        break;
                    }
                }
            }
            return obj;
        }


       


    }



    public class ModuleMap
    {
        public static int FLAG_FlipH = 1;
        public static int FLAG_FlipV = 2;
        public static int FLAG_ROT = 4;
        public static int FLAG_SCALE = 8;
        public static int FLAG_PNG = 16;
        public static int FLAG_FRAME = 32;
        public static int FLAG_ANIM = 64;
        public static int FLAG_ISO = 128;

        public static string KEY_POS = "pos";
        public static string KEY_ROT = "rot";
        public static string KEY_SCA = "sca";
        public static string KEY_IMG = "img";
        public static string KEY_FLAG = "flag";



        protected int Flag;
        protected int[] Img;
        protected int[] Pos;
        protected int Rot;
        protected int[] Scale;

        public void setFlipHorizontally(bool value)
        {
            if (value)
            {
                Flag |= FLAG_FlipH;
            }
        }

        public void setFlipVertically(bool value)
        {
            if (value)
            {
                Flag |= FLAG_FlipV;
            }
        }

        public void setRotation( float rad )
        {
            if (rad != 0)
            {
                Rot = (int)Math.Round(rad * 180 / Math.PI);
                Flag |= FLAG_ROT;
            }
        }

        public void setPosition( Vector2 pos, bool iso )
        {
            Pos = new int[2];
            Pos[0] = (int) pos.X;
            Pos[1] = (int) pos.Y;

            if (iso)
            {
                Flag |= FLAG_ISO;
            }
        }

        public void setScale(Vector2 scale)
        {
            if (scale.X != 1 && scale.Y != 1)
            {
                Scale = new int[2];
                Scale[0] = (int)Math.Round(scale.X * 100);
                Scale[1] = (int)Math.Round(scale.Y * 100);
                Flag |= FLAG_SCALE;
            }
        }

        public void setImage(int file, int pixma, Item i)
        {

            if (pixma > 0)
            {
                Img = new int[2];
                Img[0] = file;
                Img[1] = pixma;
            }
            else
            {
                Img = new int[1];
                Img[0] = pixma;
            }

            if (i is PixAnim)
                Flag |= FLAG_ANIM;
            else if (i is PixFrame)
                Flag |= FLAG_FRAME;
            else if (i is TextureItem)
                Flag |= FLAG_PNG;
        }

        public JObject encode()
        {
            JObject obj = new JObject
            {
                {KEY_FLAG, Flag},
                {KEY_IMG, new JArray(Img)},
                {KEY_POS, new JArray(Pos)},
            };

            if( (Flag & FLAG_ROT) != 0 )
                 obj.Add(KEY_ROT, Rot);
            if ((Flag & FLAG_SCALE) != 0)
                 obj.Add(KEY_SCA, new JArray(Scale));

            return obj;
        }

    }
}
