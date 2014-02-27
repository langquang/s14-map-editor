using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using GLEED2D.src.Pixma;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Forms = System.Windows.Forms;
using System.ComponentModel;
using System.Text;
using System.Drawing.Design;
using System.Windows.Forms;
using CustomUITypeEditors;
using Microsoft.Xna.Framework;
using Formatting = System.Xml.Formatting;

namespace GLEED2D
{
    public partial class Level
    {
        [XmlIgnore()]
        public string selectedlayers;
        [XmlIgnore()]
        public string selecteditems;

        public class EditorVars
        {
            public int NextItemNumber;
            public string ContentRootFolder;
            public Vector2 CameraPosition;
            public string Version;
        }

        [XmlIgnore()]
        [Category(" General")]
        [Description("When the level is saved, each texture is saved with a path relative to this folder."
                     + "You should set this to the \"Content.RootDirectory\" of your game project.")]
        [EditorAttribute(typeof(FolderUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public String ContentRootFolder
        {
            get
            {
                return EditorRelated.ContentRootFolder;
            }
            set
            {
                EditorRelated.ContentRootFolder = value;
            }
        }

        EditorVars editorrelated = new EditorVars();
        [Browsable(false)]
        public EditorVars EditorRelated
        {
            get
            {
                return editorrelated;
            }
            set
            {
                editorrelated = value;
            }
        }

        [XmlIgnore()]
        public Forms.TreeNode treenode;


        public string getNextItemNumber()
        {
            return (++EditorRelated.NextItemNumber).ToString("0000");
        }


        public void export(string filename)
        {
            foreach (Layer l in Layers)
            {
                foreach (Item i in l.Items)
                {
                    if (i is TextureItem)
                    {
                        TextureItem ti = (TextureItem)i;
                        ti.texture_filename = RelativePath(ContentRootFolder, ti.texture_fullpath);
                        ti.asset_name = ti.texture_filename.Substring(0, ti.texture_filename.LastIndexOf('.'));
                    }
                }
            }



            if (!Define.EXPORT_JSON)
            {
                XmlTextWriter writer = new XmlTextWriter(filename, null);
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;

                XmlSerializer serializer = new XmlSerializer(typeof(Level));
                serializer.Serialize(writer, this);

                writer.Close();
            }
            else
            {
                Logger.Instance.log("Export JSON: " + filename);

                JObject modules = null;
                JObject decos = null;
                JArray restrictions = null;
                JObject tiles = null;
                JArray wObjs = null;
                JArray items = null;


                foreach (Layer l in Layers)
                {
                    if (l.Name.ToUpper().IndexOf(Generator.LAYER_TILE) != -1)
                    {
                        tiles = Generator.CreateTiles(l);
                    }
                    else if (l.Name.ToUpper().IndexOf(Generator.LAYER_RESTRICTION) != -1)
                    {
                        restrictions = Generator.CreateRestriction(l);
                    }
                    else if (l.Name.ToUpper().IndexOf(Generator.LAYER_MODULE) != -1)
                    {
                        modules = Generator.CreateMapModules(l);
                    }
                    else if (l.Name.ToUpper().IndexOf(Generator.LAYER_DECOS) != -1)
                    {
                        decos = Generator.CreateMapModules(l);
                    }
                    else if (l.Name.ToUpper().IndexOf(Generator.LAYER_WORLD_OBJECT) != -1)
                    {
                        wObjs = Generator.CreateObjects(l);
                    }
                    else if (l.Name.ToUpper().IndexOf(Generator.LAYER_ITEMS) != -1)
                    {
                        items = Generator.CreateItems(l);
                    }
                }

                JObject map = new JObject();
                map.Add(Generator.LAYER_MODULE, modules);
                map.Add(Generator.LAYER_DECOS, decos);
                map.Add(Generator.LAYER_RESTRICTION, restrictions);
                map.Add(Generator.LAYER_TILE, tiles);
                map.Add(Generator.LAYER_WORLD_OBJECT, wObjs);
                map.Add(Generator.LAYER_ITEMS, items);

                using (StreamWriter outfile = new StreamWriter(filename))
                {
                    outfile.Write(map.ToString());
                    outfile.Close();
                }

                Logger.Instance.log("End Export JSON");


                //XmlWriterSettings settings = new XmlWriterSettings();
                //settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
                //settings.Indent = true;
                //settings.OmitXmlDeclaration = true;

                //XmlSerializer serializer = new XmlSerializer(typeof(Level));

                //using (StringWriter textWriter = new StringWriter())
                //{
                //    using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                //    {
                //        serializer.Serialize(xmlWriter, this);
                //        XmlDocument xml = new XmlDocument();
                //        xml.LoadXml(textWriter.ToString());
                //        JObject jobj = JObject.Parse(JsonConvert.SerializeXmlNode(xml));
                //        jobj = jobj.get

                //        //using (StreamWriter outfile = new StreamWriter(filename))
                //        //{
                //        //    outfile.Write(JsonConvert.SerializeXmlNode(xml));
                //        //    outfile.Close();
                //        //}
                //    }

                //}


            }

           

        }



        public string RelativePath(string relativeTo, string pathToTranslate)
        {
            string[] absoluteDirectories = relativeTo.Split('\\');
            string[] relativeDirectories = pathToTranslate.Split('\\');

            //Get the shortest of the two paths
            int length = absoluteDirectories.Length < relativeDirectories.Length ? absoluteDirectories.Length : relativeDirectories.Length;

            //Use to determine where in the loop we exited
            int lastCommonRoot = -1;
            int index;

            //Find common root
            for (index = 0; index < length; index++)
                if (absoluteDirectories[index] == relativeDirectories[index])
                    lastCommonRoot = index;
                else
                    break;

            //If we didn't find a common prefix then throw
            if (lastCommonRoot == -1)
                // throw new ArgumentException("Paths do not have a common base");
                return pathToTranslate;

            //Build up the relative path
            StringBuilder relativePath = new StringBuilder();

            //Add on the ..
            for (index = lastCommonRoot + 1; index < absoluteDirectories.Length; index++)
                if (absoluteDirectories[index].Length > 0) relativePath.Append("..\\");

            //Add on the folders
            for (index = lastCommonRoot + 1; index < relativeDirectories.Length - 1; index++)
                relativePath.Append(relativeDirectories[index] + "\\");

            relativePath.Append(relativeDirectories[relativeDirectories.Length - 1]);

            return relativePath.ToString();
        }








    }








}
