using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System.Collections;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

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

        //Xls
        public static int SHEET_CONSTANCE = 0;
        public static int CLOLUMN_ID = 1;
        public static int CLOLUMN_VALUE = 2;
        public static int CLOLUMN_FILE = 4;
        public static int CLOLUMN_PIXMA = 5;



        public static void createImages(Layer layer, ArrayList files, ArrayList pixmas)
        {
            foreach (Item i in layer.Items)
            {
                if (i is TextureItem)
                {
                    TextureItem ti = (TextureItem)i;
                    string filename = getFileName(ti);
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
                module.setPosition(i.Position, true, false);
                module.setScale(i.getScale());
                module.setRotation(i.getRotation());

                if (i is TextureItem)
                {
                    TextureItem ti = (TextureItem)i;
                    module.setFlipHorizontally(ti.FlipHorizontally);
                    module.setFlipVertically(ti.FlipVertically);
                    string filename = getFileName(ti);
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
            Logger.Instance.log("CreateMapModules success!");
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
            Logger.Instance.log("CreateRestriction success!");
            return new JArray((int[])array.ToArray(typeof(int)));
        }

        public static JObject CreateTiles(Layer layer, int nCellX, int nCellY)
        {
            JObject obj = new JObject();
            foreach (Item i in layer.Items)
            {
                if (i is TextureItem)
                {
                    TextureItem ti = (TextureItem)i;
                    if (ti.texture_filename.ToUpper().IndexOf(NAME_MAP) != -1)
                    {
                        ModuleMap module = new ModuleMap();
                        module.setPosition(i.Position, false, false);
                        module.setScale(i.getScale());
                        module.setRotation(i.getRotation());
                        module.setFlipHorizontally(ti.FlipHorizontally);
                        module.setFlipVertically(ti.FlipVertically);
                        module.setImage(0, -1, i);

                        obj.Add("nCellX", nCellX);
                        obj.Add("nCellY", nCellY);
                        obj.Add("width", ti.getTexture().Width);
                        obj.Add("height", ti.getTexture().Height);
                        obj.Add("img", Path.GetFileName(ti.texture_fullpath));
                        obj.Add("bg", module.encode());
                        break;
                    }
                }
            }
            Logger.Instance.log("CreateTiles success!");
            return obj;
        }

        public static JArray CreateObjects(Layer layer)
        {
            ArrayList newFiles1 = new ArrayList();
            ArrayList newFiles2 = new ArrayList();
            ArrayList newPixmas1 = new ArrayList();
            ArrayList newPixmas2 = new ArrayList();
            ArrayList wObjs = new ArrayList();
            Dictionary<string, int> mapping = getXlsData();
            foreach (Item i in layer.Items)
            {
                WorldObjectMap wObj = new WorldObjectMap();
                wObj.setPosition(i.pIsoCell, true, true);
                wObj.setScale(i.getScale());
                wObj.setRotation(i.getRotation());

                if (i is PixFrame)
                {
                    PixFrame frame = (PixFrame)i;
                    wObj.setFlipHorizontally(frame.FlipHorizontally);
                    wObj.setFlipVertically(frame.FlipVertically);
                    string filename = Path.GetFileName(frame.texture_fullpath);
                    string key = filename + frame.FrameName;
                    if (mapping.ContainsKey(key))
                    {
                        wObj.setId(mapping[key]);
                        wObjs.Add(wObj.encode());
                    }
                    else
                    {
                        if (i is PixAnim && !newFiles1.Contains(filename) && !newPixmas1.Contains(frame.FrameName))
                        {
                            newFiles1.Add(filename);
                            newPixmas1.Add(frame.FrameName);
                            Logger.Instance.log("New World Object: {anim} - " + filename + " : " + frame.FrameName);
                        }
                        else if( !newFiles2.Contains(filename) && !newPixmas2.Contains(frame.FrameName))
                        {
                            newFiles2.Add(filename);
                            newPixmas2.Add(frame.FrameName);
                            Logger.Instance.log("New World Object: {frame} - " + filename + " : " + frame.FrameName);
                        }
                    }


                }
            }
            JArray entitys = new JArray((JObject[])wObjs.ToArray(typeof(JObject)));
            Logger.Instance.log("CreateObjects success!");
            return entitys;

        }

        public static JArray CreateItems(Layer layer)
        {
            ArrayList newFiles1 = new ArrayList();
            ArrayList newFiles2 = new ArrayList();
            ArrayList newPixmas1 = new ArrayList();
            ArrayList newPixmas2 = new ArrayList();
            ArrayList wObjs = new ArrayList();
            Dictionary<string, int> mapping = getXlsData();
            foreach (Item i in layer.Items)
            {
                WorldObjectMap wObj = new WorldObjectMap();
                wObj.setPosition(i.pIsoCell, true, true);
                wObj.setScale(i.getScale());
                wObj.setRotation(i.getRotation());

                if (i is PixFrame)
                {
                    PixFrame frame = (PixFrame)i;
                    wObj.setFlipHorizontally(frame.FlipHorizontally);
                    wObj.setFlipVertically(frame.FlipVertically);
                    string filename = getFileName(frame);
                    string key = filename + frame.FrameName;
                    if (mapping.ContainsKey(key))
                    {
                        wObj.setId(mapping[key]);
                        wObjs.Add(wObj.encode());
                    }
                    else
                    {
                        if (i is PixAnim && (!newFiles1.Contains(filename) || !newPixmas1.Contains(frame.FrameName)))
                        {
                            newFiles1.Add(filename);
                            newPixmas1.Add(frame.FrameName);
                            Logger.Instance.log("New World Object: {anim} - " + filename + " : " + frame.FrameName);
                        }
                        else if (!newFiles2.Contains(filename) || !newPixmas2.Contains(frame.FrameName))
                        {
                            newFiles2.Add(filename);
                            newPixmas2.Add(frame.FrameName);
                            Logger.Instance.log("New World Object: {frame} - " + filename + " : " + frame.FrameName);
                        }
                    }


                }
            }
            JArray entitys = new JArray((JObject[])wObjs.ToArray(typeof(JObject)));
            Logger.Instance.log("CreateItems success!");
            return entitys;
        }

        public static Dictionary<string, int> getXlsData()
        {
            Logger.Instance.log("copy temp file!");
            string xlsPath = Directory.GetParent(Constants.Instance.XlsConstancePath).ToString();
            string xlsFile = Path.GetFileNameWithoutExtension(Constants.Instance.XlsConstancePath);
            string xlsExtension = Path.GetExtension(Constants.Instance.XlsConstancePath);
            string xlsFileCopy = xlsFile + "_copy" + xlsExtension;
            xlsFileCopy = Path.Combine(xlsPath, xlsFileCopy);
            System.IO.File.Copy(Constants.Instance.XlsConstancePath, xlsFileCopy, true);
            Logger.Instance.log("load xls: " + xlsFileCopy);
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            using (FileStream file = new FileStream(xlsFileCopy, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                XSSFWorkbook hssfworkbook = new XSSFWorkbook(file);
                ISheet sheet = hssfworkbook.GetSheetAt(SHEET_CONSTANCE);
                System.Collections.IEnumerator rows = sheet.GetRowEnumerator();
                while (rows.MoveNext())
                {
                    IRow row = (XSSFRow)rows.Current;
                    ICell cell_value = row.GetCell(CLOLUMN_VALUE);
                    ICell cell_file = row.GetCell(CLOLUMN_FILE);
                    ICell cell_pixma = row.GetCell(CLOLUMN_PIXMA);
                    if (cell_value != null && cell_file != null && cell_pixma != null)
                    {
                        dictionary.Add(cell_file.ToString().Trim() + cell_pixma.ToString().Trim(), Convert.ToInt32(cell_value.ToString()));
                    }

                }
                file.Close();
            }
            Logger.Instance.log("getXlsData success!");
            return dictionary;

        }


        public static Vector2 GetMapSize(List<Layer> layers)
        {
            Vector2 mapsize = new Vector2();
            foreach (Layer l in layers)
            {
                if (l.Name.ToUpper().IndexOf(Generator.LAYER_RESTRICTION) != -1)
                {
                    foreach (Item i in l.Items)
                    {
                        if (i is TextureItem)
                        {
                            TextureItem ti = (TextureItem)i;
                            if (ti.texture_filename.ToUpper().IndexOf(NAME_MASK) != -1)
                            {
                                mapsize.X = mapsize.X < ti.pIsoCell.X ? ti.pIsoCell.X : mapsize.X;
                                mapsize.Y = mapsize.Y < ti.pIsoCell.Y ? ti.pIsoCell.Y : mapsize.Y;
                            }
                        }
                    }
                }
                else if (l.Name.ToUpper().IndexOf(Generator.LAYER_WORLD_OBJECT) != -1)
                {
                    foreach (Item i in l.Items)
                    {
                        if (i is TextureItem)
                        {
                            TextureItem ti = (TextureItem)i;
                            if (ti.texture_filename.ToUpper().IndexOf(NAME_MASK) != -1)
                            {
                                mapsize.X = mapsize.X < ti.pIsoCell.X ? ti.pIsoCell.X : mapsize.X;
                                mapsize.Y = mapsize.Y < ti.pIsoCell.Y ? ti.pIsoCell.Y : mapsize.Y;
                            }
                        }
                    }
                }
                else if (l.Name.ToUpper().IndexOf(Generator.LAYER_ITEMS) != -1)
                {
                    foreach (Item i in l.Items)
                    {
                        if (i is TextureItem)
                        {
                            TextureItem ti = (TextureItem)i;
                            if (ti.texture_filename.ToUpper().IndexOf(NAME_MASK) != -1)
                            {
                                mapsize.X = mapsize.X < ti.pIsoCell.X ? ti.pIsoCell.X : mapsize.X;
                                mapsize.Y = mapsize.Y < ti.pIsoCell.Y ? ti.pIsoCell.Y : mapsize.Y;
                            }
                        }
                    }
                }
            }
            if (mapsize.X > mapsize.Y)
            {
                mapsize.X += 5;
                mapsize.Y = mapsize.X;
            }
            else
            {
                mapsize.Y += 5;
                mapsize.X = mapsize.Y;
            }

            return mapsize;
        }

        public static string getFileName(TextureItem ti)
        {
            if (ti is PixFrame)
                return Path.GetFileNameWithoutExtension(ti.texture_fullpath);
            else
            {
                return Path.GetFileName(ti.texture_fullpath);
            }
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
        public static int FLAG_CELL = 256;

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

        public void setPosition( Vector2 pos, bool isIso, bool isCell )
        {
            Pos = new int[2];
            Pos[0] = (int) pos.X;
            Pos[1] = (int) pos.Y;

            if (isIso)
            {
                Flag |= FLAG_ISO;
            }

            if (isCell)
            {
                Flag |= FLAG_CELL;
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

            if (pixma >= 0)
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

        public  virtual JObject encode()
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

    public class WorldObjectMap : ModuleMap
    {
        public static string KEY_ID = "id";

        protected int Id;

        public void setId(int id)
        {
            Id = id;
        }

        public override JObject encode()
        {
            JObject obj = new JObject
            {
                {KEY_FLAG, Flag},
                {KEY_ID, Id},
                {KEY_POS, new JArray(Pos)},
            };

            if ((Flag & FLAG_ROT) != 0)
                obj.Add(KEY_ROT, Rot);
            if ((Flag & FLAG_SCALE) != 0)
                obj.Add(KEY_SCA, new JArray(Scale));

            return obj;
        }
    }
}
