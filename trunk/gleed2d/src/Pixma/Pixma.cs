using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Text.RegularExpressions;
using NPOI.SS.Formula.Functions;


namespace GLEED2D
{
    class Pixma
    {
        public static int STATE_EMPTY = 1;
		public static int STATE_HAD_DATA = 2;
		public static int STATE_FINISHED = 3;
		//General flags
		public static int EXPORT_FREE_TRANSFORM = (1 << 0);
		public static int EXPORT_MULTIPLE_MODULES = (1 << 1);
		public static int EXPORT_COLOR_TRANSFORM = (1 << 2);	
		public static int EXPORT_SINGLE_IMAGE = (1 << 4);
		
		//Quality flags
		public static int QUALITY_TRUE_COLOR = (1 << 0);
		public static int QUALITY_INDEX_COLOR = (1 << 1);
		
		//Data type flags
		public static int TYPE_TEXTURE_3D = (1 << 0);
		public static int TYPE_MODULE_IMAGES = (1 << 1);
		public static int TYPE_SINGLE_IMAGE = (1 << 2);
		
		//Pixel format flags
		public static int PIXEL_32 = (1 << 0);
		public static int PIXEL_16 = (1 << 1);
		
		//Image format flags
		public static int FORMAT_INDEX = (1 << 0);
		public static int FORMAT_INDEX_ALPHA = (1 << 1);
		public static int FORMAT_RAW_0888 = (1 << 2);
		public static int FORMAT_RAW_8888 = (1 << 3);
		public static int FORMAT_RAW_1888 = (1 << 4);
		public static int FORMAT_RAW_PNG = (1 << 5);
		public static int FORMAT_RAW_JPG = (1 << 6);	
		
		//Transform flags
		public static int TRANSFORM_FLIP_H = (1 << 0);	
		public static int TRANSFORM_FLIP_V = (1 << 1);	
		public static int TRANSFORM_FREE = (1 << 2);	
		public static int TRANSFORM_COLOR = (1 << 3);	
		
		//Anchor flags
		public static int ANCHOR_LEFT = (1 << 0);	
		public static int ANCHOR_RIGHT = (1 << 1);	
		public static int ANCHOR_HCENTER = (1 << 2);		
		public static int ANCHOR_TOP = (1 << 3);	
		public static int ANCHOR_BOTTOM = (1 << 4);	
		public static int ANCHOR_VCENTER = (1 << 5);

        static public string IMAGE_STR= "IMAGE";
        static public string MODULE_STR = "MODULE";
        static public string FRAME_STR = "FRAME";
        static public string ADD_FMODULE_STR = "\tADD_FMODULE";
        static public string ANIMATION_STR = "ANIMATION";
        static public string ADD_AFRAME_STR = "\tADD_AFRAME";

        private string[] images_str; //store image line
        private string[] modules_str;
        private string[] frames_str;
        private string[] anims_str;
        private string[] aframe_str;
        private string[] fmodules_str;
        private int[] num_fmodules; // store number of fmodule in  frame
        private int[] num_frames;// store number of frame in  anim
        public string name;
        private Texture2D[] textures;
        private int numModules;
        private Texture2D[] moduleImages;
		private int[] moduleInfos;
        private ArrayList module_ids = new ArrayList();

        private int numFrames;
		private int[] frameInfos;
		private Rectangle[] frameRects;
		private PixFrame[]	frame_caches;
        private string[] frame_names;
        private Dictionary<string, int> frame_id_index = new Dictionary<string, int>(); 

        private int numFModules;
		private int[] fmoduleInfos;	
		private float[] fmoduleMatrices;
		private float[] fmoduleMulColors;
		private int[] fmoduleOffColors;
		private string[] fmoduleBlends;
		
		private int numAnims;
		private int[] animInfos;
        private string[] anim_names;

		
		private int numAFrames;
		private int[] aframeInfos;
		
		private int numImages;
        private String[] imageFiles;
		
		private int numModuleDes;
		private string[] moduleDesNames;
		private int[] moduleDesIndexs;


        //=================== GDI
        private System.Drawing.Bitmap[] textures_bitmap;
        private System.Drawing.Bitmap[] modules_bitmap;

        private string pixma_path;
        public Pixma(string name)
        {
            this.name = name;
        }

        public void load(String pixma_path)
        {
            this.pixma_path = pixma_path;
            using (MemoryStream memory = new MemoryStream())
            {
                ArrayList images_list = new ArrayList();
                ArrayList modules_list = new ArrayList();
                ArrayList frames_list = new ArrayList();
                ArrayList anims_list = new ArrayList();
                ArrayList aframe_list = new ArrayList();
                ArrayList fmodules_list = new ArrayList();
                ArrayList num_fmodules_list = new ArrayList();
                ArrayList num_frames_list = new ArrayList();
                int cur_fmodule = 0;
                int cur_aframe = 0;


                StreamReader reader = new StreamReader(pixma_path);
                string line = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    //images
                    if (line.Length > 5 && line.Substring(0, 5) == IMAGE_STR)
                    {
                        images_list.Add(line);
                    }
                    // modules
                    if (line.Length > 6 && line.Substring(0, 6) == MODULE_STR)
                    {
                        modules_list.Add(line);
                    }
                    // frame
                    if (line.Length > 5 && line.Substring(0, 5) == FRAME_STR)
                    {
                        num_fmodules_list.Add(cur_fmodule);// store number of fmodule of previous frame
                        frames_list.Add(line);
                        cur_fmodule = 0;
                    }
                    // animation
                    if (line.Length > 9 && line.Substring(0, 9) == ANIMATION_STR)
                    {
                        num_frames_list.Add(cur_aframe);// store number of frame of previous anim
                        anims_list.Add(line);
                        cur_aframe = 0;
                    }
                    // animation frame
                    if (line.Length > 11 && line.Substring(0, 11) == ADD_AFRAME_STR)
                    {
                        aframe_list.Add(line);
                        cur_aframe++;
                    }
                    // module frame
                    if (line.Length > 12 && line.Substring(0, 12) == ADD_FMODULE_STR)
                    {
                        fmodules_list.Add(line);
                        cur_fmodule++;
                    }
                }
                reader.Close();

                num_fmodules_list.Add(cur_fmodule);// store number of fmodule of previous frame
                num_frames_list.Add(cur_aframe);// store number of frame of previous anim


                images_str = (string[])images_list.ToArray(typeof(string));
                modules_str = (string[])modules_list.ToArray(typeof(string));
                frames_str = (string[])frames_list.ToArray(typeof(string));
                anims_str = (string[])anims_list.ToArray(typeof(string));
                aframe_str = (string[])aframe_list.ToArray(typeof(string));
                fmodules_str = (string[])fmodules_list.ToArray(typeof(string));
                num_fmodules = (int[])num_fmodules_list.ToArray(typeof(int));
                num_frames = (int[])num_frames_list.ToArray(typeof(int));

                loadModules();
                loadFrames();
                calculateFrameRects();
                loadAnims();
                loadTextures();

            }
        }

        private void loadModules()
        {
            numModules = modules_str.Length;
            if (numModules > 0)
            {
                moduleImages = new Texture2D[numModules];
                modules_bitmap = new System.Drawing.Bitmap[numModules];
                moduleInfos = new int[numModules*5];
                for (int i = 0; i < numModules; i++)
                {
                    moduleInfos[i*5+0] = readInt(modules_str[i], _w, _h);// width
                    moduleInfos[i * 5 + 1] = readInt(modules_str[i], _h, _desc);//height
                    moduleInfos[i * 5 + 2] = readInt(modules_str[i], _x, _y);//x
                    moduleInfos[i * 5 + 3] = readInt(modules_str[i], _y, _w);//y
                    moduleInfos[i * 5 + 4] = readInt(modules_str[i], _imgid, _x);//image id

                    module_ids.Add(readString(modules_str[i], _mid, _type));
                }
            }
        }

        private void loadFrames()
        {
            int num;
            numFrames = frames_str.Length;
            if (numModules > 0)
            {
                frameInfos = new int[numFrames];
				frameRects = new Rectangle[numFrames];
				frame_caches = new PixFrame[numFrames];
                frame_names = new string[numFrames];
				
				numFModules = 0;		// number of frame	
				for (int i = 0; i<numFrames; i++)
				{
				    frame_names[i] = readString(frames_str[i], _desc_start, _desc_end);
                    frame_id_index.Add( readString(frames_str[i], _fid, _desc), i );
                    num = num_fmodules[i+1];				
					frameInfos[i] = numFModules;
					numFModules += num;
				}
				loadFModules();	
            }
        }

        private void loadFModules()
        {
            int i = 0;
            int transf = 0;

            fmoduleInfos = new int[numFModules * 2];		//module id, transform
            fmoduleMatrices = new float[numFModules * 6];		// transform matrix 			
            fmoduleMulColors = new float[numFModules * 8];// alpha,r,g,b

            fmoduleBlends = new string[numFModules];// blend mode

            for (i = 0; i < numFModules; i++)
            {
                fmoduleInfos[i * 2 + 0] = module_ids.IndexOf(readString(fmodules_str[i], _mid, _x));// module id
                transf = readInt(fmodules_str[i], _transform, _m11);
                fmoduleInfos[i * 2 + 1] = transf;	// is transform?

                if (Utils.hasFlags(transf, TRANSFORM_FREE))
                {
                    fmoduleMatrices[i * 6 + 0] = readFloat(fmodules_str[i], _x, _y);//x
                    fmoduleMatrices[i * 6 + 1] = readFloat(fmodules_str[i], _y, _transform);//y
                    fmoduleMatrices[i * 6 + 2] = readFloat(fmodules_str[i], _m11, _m12);// m1--4
                    fmoduleMatrices[i * 6 + 3] = readFloat(fmodules_str[i], _m12, _m21);
                    fmoduleMatrices[i * 6 + 4] = readFloat(fmodules_str[i], _m21, _m22);
                    fmoduleMatrices[i * 6 + 5] = readFloat(fmodules_str[i], _m22, _blend_mode);
                }
                else
                {
                    fmoduleMatrices[i * 6 + 0] = readInt(fmodules_str[i], _x, _y);//x
                    fmoduleMatrices[i * 6 + 1] = readInt(fmodules_str[i], _y, _transform);//x			
                    fmoduleMatrices[i * 6 + 2] = 1;// m1--4
                    fmoduleMatrices[i * 6 + 3] = 0;
                    fmoduleMatrices[i * 6 + 4] = 0;
                    fmoduleMatrices[i * 6 + 5] = 1;
                }

                //if (Utils.hasFlags(transf , TRANSFORM_COLOR)) {
                //    fmoduleMulColors[i*8+0] = readFloat(fmodules_str[i], _mid, _x);//alpha
                //    fmoduleMulColors[i*8+1] = readInt(fmodules_str[i], _mid, _x);//
                //    fmoduleMulColors[i*8+2] = readFloat(fmodules_str[i], _mid, _x);//red
                //    fmoduleMulColors[i*8+3] = readInt(fmodules_str[i], _mid, _x);
                //    fmoduleMulColors[i*8+4] = readFloat(fmodules_str[i], _mid, _x);// green
                //    fmoduleMulColors[i*8+5] = readInt(fmodules_str[i], _mid, _x);
                //    fmoduleMulColors[i*8+6] = readFloat(fmodules_str[i], _mid, _x);// bule
                //    fmoduleMulColors[i*8+7] = readInt(fmodules_str[i], _mid, _x);
                //} else {
                fmoduleMulColors[i * 8 + 0] = 1;
                fmoduleMulColors[i * 8 + 1] = 0;
                fmoduleMulColors[i * 8 + 2] = 1;
                fmoduleMulColors[i * 8 + 3] = 0;
                fmoduleMulColors[i * 8 + 4] = 1;
                fmoduleMulColors[i * 8 + 5] = 0;
                fmoduleMulColors[i * 8 + 6] = 1;
                fmoduleMulColors[i * 8 + 7] = 0;
                //}

                fmoduleBlends[i] = Utils.getBlendModeint(1);
            }
        }

        private void loadAnims()
        {
			int i;
			int n;
			numAnims = anims_str.Length;	
			anim_names = new string[numAnims];
			if (numAnims > 0) {
                animInfos = new int[numAnims];
				numAFrames = 0;
				for (i = 0;i<numAnims;i++) {
                    anim_names[i] = readString(anims_str[i], _desc_start, _desc_end);
					n = num_frames[i+1];
					animInfos[i] = numAFrames;
					numAFrames += n;
				}
				
				loadAFrames();
			}
		}

        private void loadAFrames()
        {
			int i;
            aframeInfos = new int[numAFrames * 5];
			
			for (i = 0;i<numAFrames;i++) {
                aframeInfos[i * 5 + 0] = frame_id_index[readString(aframe_str[i], _fid, _x)];//id
                aframeInfos[i * 5 + 1] = readInt(aframe_str[i], _x, _y);//x
                aframeInfos[i * 5 + 2] = readInt(aframe_str[i], _y, _transform);//y
                aframeInfos[i * 5 + 3] = readInt(aframe_str[i], _transform, _time);//transform	
                aframeInfos[i * 5 + 4] = readInt(aframe_str[i], _time, _end);//time
			}
		}

        private void loadTextures()
        {
            numImages = images_str.Length;
            textures = new Texture2D[numImages];
            imageFiles = new String[numImages];

            textures_bitmap = new System.Drawing.Bitmap[numImages];

            string folder = Path.GetDirectoryName(pixma_path);
            for (int i = 0; i < numImages; i++)
            {
                imageFiles[i] = readString(images_str[i], "-imgfile:\"", "\"");
                textures[i] = TextureLoader.Instance.FromFile(Game1.Instance.GraphicsDevice, folder + "\\" + imageFiles[i]);
                textures_bitmap[i] = new System.Drawing.Bitmap(folder + "\\" + imageFiles[i]);
                Texture2D modBm;
                System.Drawing.Bitmap modBm_bitmap;
				int off;
				int j;
				for (j = 0;j<numModules;j++) 
                {
                    off = j * 5;
					if (moduleInfos[off + 4] == i) 
                    {				
						rectHelper.Width = moduleInfos[off + 0];
						rectHelper.Height = moduleInfos[off + 1];					
						rectHelper.X = moduleInfos[off + 2];
						rectHelper.Y = moduleInfos[off + 3];										
						modBm = new Texture2D(Game1.Instance.GraphicsDevice, rectHelper.Width , rectHelper.Height);					
						Utils.copyPixel(textures[i] ,modBm, rectHelper , new Vector2(0,0));
						moduleImages[j] = modBm;

                        modBm_bitmap = new System.Drawing.Bitmap(rectHelper.Width, rectHelper.Height);
                        Utils.copyPixel(textures_bitmap[i], modBm_bitmap, rectHelper, new Vector2(0, 0));
                        modules_bitmap[j] = modBm_bitmap;
					}

                }
            }
        }

        public Rectangle getFrameRect(int frameId)
        {
			return frameRects[frameId];
		}
		
		private void calculateFrameRects()
        {
			int i;
			for (i =0;i<numFrames;i++) {
				frameRects[i] = createFrameRect(i);
			}
		}

        private Rectangle createFrameRect(int frameId)
        {
			Rectangle frame_rect = new Rectangle();	
			Rectangle module_rect = new Rectangle(); 			
			
			int fmodule_min = 0;
			int fmodule_max = 0;			
			int i;
			
			int module = 0;						
			int ox , oy;

            if (frameId < (numFrames - 1))
            {
                fmodule_min = frameInfos[frameId];
                fmodule_max = frameInfos[frameId + 1] - 1;
            }
            else if (frameId == (numFrames - 1))
            {
                fmodule_min = frameInfos[frameId];
                fmodule_max = numFModules - 1;
            }

            for (i = fmodule_min; i <= fmodule_max; i++)
            {
                module = fmoduleInfos[i * 2];
                int fmodule_transf = fmoduleInfos[i * 2 + 1];

				module_rect.X = 0;
				module_rect.Y = 0;
				module_rect.Width = moduleInfos[module * 5];
				module_rect.Height = moduleInfos[module * 5 + 1];	

                fm_trans_matrix = Matrix.Identity;
                fm_trans_matrix.M11 = fmoduleMatrices[i * 6 + 2];
                fm_trans_matrix.M12 = fmoduleMatrices[i * 6 + 3];
                fm_trans_matrix.M21 = fmoduleMatrices[i * 6 + 4];
                fm_trans_matrix.M22 = fmoduleMatrices[i * 6 + 5];
                fm_trans_matrix.M41 = fmoduleMatrices[i * 6];
                fm_trans_matrix.M42 = fmoduleMatrices[i * 6 + 1];

                //// [CS.2012/06/27] incorrect flipping
                if ((fmodule_transf & TRANSFORM_FLIP_H) != 0)
                {
                    fm_trans_matrix *= Matrix.CreateScale(-1, 1, 1);
                }
                if ((fmodule_transf & TRANSFORM_FLIP_V) != 0)
                {
                    fm_trans_matrix *= Matrix.CreateScale(1, -1, 1);
                }

                frame_rect = Rectangle.Union(frame_rect, Utils.transformRect(module_rect, fm_trans_matrix));
            }

            if (frame_rect.Width <= 0 || frame_rect.Height <= 0)
            {
                frame_rect = new Rectangle(0,0,1,1);
            }
			
			return frame_rect;
		}

        Matrix fm_trans_matrix = new Matrix();
        Rectangle rectHelper = new Rectangle();


        public PixFrame GetFrame(int frame_id)
        {
            if (moduleImages.Length == 0)
                return null;

            int fmodule_min = 0;
            int fmodule_max = 0;

            int module = 0;
            int fmodule_transf = 0;
            float ox = 0;
            float oy = 0;
            String blend_mode = null;
            int i = 0;
            Texture2D module_bmp_data;
            int mw;
            int mh;
            Rectangle module_rect = new Rectangle();
            PixFrame frame = new PixFrame(frame_id, String.Empty, Vector2.Zero);

            if (frame_id < (numFrames - 1))
            {
                fmodule_min = frameInfos[frame_id];
                fmodule_max = frameInfos[frame_id + 1] - 1;
            }
            else if (frame_id == (numFrames - 1))
            {
                fmodule_min = frameInfos[frame_id];
                fmodule_max = numFModules - 1;
            }

            for (i = fmodule_min; i <= fmodule_max; i++)
            {
                module = fmoduleInfos[i * 2];
                fmodule_transf = fmoduleInfos[i * 2 + 1];
                blend_mode = fmoduleBlends[i];

                ox = fmoduleMatrices[i * 6];
                oy = fmoduleMatrices[i * 6 + 1];

                module_bmp_data = moduleImages[module];
                mw = moduleInfos[module * 5];
                mh = moduleInfos[module * 5 + 1];
                if (mw <= 0 || mh <= 0) break;



                module_rect.X = 0;
				module_rect.Y = 0;
                module_rect.Width = mw;
                module_rect.Height = mh;

                if (fmodule_transf == 0) 	//if there are no transform , copy pixel
                {

                    fm_trans_matrix = Matrix.Identity;
                    fm_trans_matrix.M11 = fmoduleMatrices[i * 6 + 2];
                    fm_trans_matrix.M12 = fmoduleMatrices[i * 6 + 3];
                    fm_trans_matrix.M21 = fmoduleMatrices[i * 6 + 4];
                    fm_trans_matrix.M22 = fmoduleMatrices[i * 6 + 5];
                    fm_trans_matrix.M41 = fmoduleMatrices[i * 6];
                    fm_trans_matrix.M42 = fmoduleMatrices[i * 6 + 1];

                    //// [CS.2012/06/27] incorrect flipping
                    if ((fmodule_transf & TRANSFORM_FLIP_H) != 0)
                    {
                        fm_trans_matrix *= Matrix.CreateScale(-1, 1, 1);
                    }
                    if ((fmodule_transf & TRANSFORM_FLIP_V) != 0)
                    {
                        fm_trans_matrix *= Matrix.CreateScale(1, -1, 1);
                    }
                    PixFmodule f_module = new PixFmodule(module_bmp_data, fm_trans_matrix);
                    frame.addModule(f_module);
                }
                else
                {
                    fm_trans_matrix = Matrix.Identity;
                    fm_trans_matrix.M11 = fmoduleMatrices[i * 6 + 2];
                    fm_trans_matrix.M12 = fmoduleMatrices[i * 6 + 3];
                    fm_trans_matrix.M21 = fmoduleMatrices[i * 6 + 4];
                    fm_trans_matrix.M22 = fmoduleMatrices[i * 6 + 5];
                    fm_trans_matrix.M41 = fmoduleMatrices[i * 6];
                    fm_trans_matrix.M42 = fmoduleMatrices[i * 6 + 1];

                    //// [CS.2012/06/27] incorrect flipping
                    if ((fmodule_transf & TRANSFORM_FLIP_H) != 0)
                    {
                        fm_trans_matrix *= Matrix.CreateScale(new Vector3(-1, 1, 1));
                    }
                    if ((fmodule_transf & TRANSFORM_FLIP_V) != 0)
                    {
                        fm_trans_matrix *= Matrix.CreateScale(new Vector3(1, -1, 1));
                    }

                    PixFmodule f_module = new PixFmodule(module_bmp_data, fm_trans_matrix);
                    frame.addModule(f_module);
                }			
            }
            return frame;
        }

        public PixAnim GetAnim(int anim_id)
        {
            PixAnim pixAnim = new PixAnim();

            int minAFrame = 0;
			int maxAFrame = 0;
			int i;				
			PixFrame frame ;

			
			minAFrame = animInfos[anim_id];		
			if (anim_id == (numAnims - 1)) {			
				maxAFrame = numAFrames - 1;
			} else {
				maxAFrame = animInfos[anim_id + 1];
			}

			for (i = minAFrame; i<maxAFrame+1; i++)
			{
				frame = GetFrame(i);
                frame.Position = new Vector2(aframeInfos[i * 5 + 1], aframeInfos[i * 5 + 2]);
                frame.Scale = new Vector2(1, 1);
                pixAnim.addFrame(frame, aframeInfos[i * 5 + 4]);
			}
			
			return pixAnim;
        }

        public bool hasFrameName(int frame_id)
        {
           return frame_id < numFrames && frame_names[frame_id] != null && frame_names[frame_id] != "\"\"";
        }

        public bool hasAnimName(int anim_id)
        {
            return anim_id < numAnims && anim_names[anim_id] != null && anim_names[anim_id] != "\"\"";
            
        }
			




    

        static public string _x = "-x:";
        static public string _y = "-y:";
        static public string _w = "-w:";
        static public string _h = "-h:";
        static public string _mid = "-mid:";
        static public string _type = "-type:";
        static public string _imgid = "-imgid:";
        static public string _desc = "-desc:";
        static public string _transform = "-transform:";
        static public string _m11 = "-m11:";
        static public string _m12 = "-m12:";
        static public string _m21 = "-m21:";
        static public string _m22 = "-m22:";
        static public string _blend_mode = "-blend_mode:";
        static public string _time = "-time:";
        static public string _end = "endLine";
        static public string _fid = "-fid:";
        static public string _desc_start = "-desc:\"";
        static public string _desc_end = "\"";


        private int readInt(string line, string start, string end)
        {
            end = "\t\t";
            string data = Regex.Split(line, start)[1];
            int length = data.IndexOf(end);
            if (length > 0)
            {
                data = data.Substring(0, length);
            }
            data = data.Trim();
            if (start == _mid || start == _fid)
                return Convert.ToInt32(data, 16);
            return Convert.ToInt32(data);
        }

        private float readFloat(string line, string start, string end)
        {
            end = "\t\t";
            string data = Regex.Split(line, start)[1];
            int length = data.IndexOf(end);
            if (length > 0)
            {
                data = data.Substring(0, length);
            }
            data = data.Trim();
            return float.Parse(data, System.Globalization.CultureInfo.InvariantCulture); 
        }

        private string readString(string line, string start, string end)
        {
            string data = Regex.Split(line, start)[1];
            if (end != _end)
            {
                data = data.Substring(0, data.IndexOf(end));
            }
            return data.Trim();
        }

        private bool IsHexString(String text)
        {
            for (int i = 0; i < text.Length; i++)
                if (Uri.IsHexDigit(text[i]))
                    return true;
            return false;
        }

        public int NumFrame
        {
            get { return numFrames; }
        }

        public int NumAnim
        {
            get { return numAnims; }
        }



        //============================== GDI
        public PixFrame GetFrame_bitmap(PixFrame frame)
        {
            if (moduleImages.Length == 0)
                return null;

            int frame_id = frame.getFrameId();
            if (frame_caches[frame_id] != null)
            {
                frame = frame_caches[frame_id].FillEmptyFrame(frame);
                return frame;
            }

                
            int fmodule_min = 0;
            int fmodule_max = 0;
            int module = 0;
            int fmodule_transf = 0;
            float ox = 0;
            float oy = 0;
            String blend_mode = null;
            int i = 0;
            System.Drawing.Bitmap module_bmp_data;
            int mw;
            int mh;
            Rectangle frame_rect = getFrameRect(frame_id);
            System.Drawing.Bitmap frame_bitmap = new System.Drawing.Bitmap(frame_rect.Width, frame_rect.Height);
            System.Drawing.Graphics back_buffer = System.Drawing.Graphics.FromImage(frame_bitmap);
            Matrix  bitmap_trans = Matrix.CreateTranslation(frame_rect.X, frame_rect.Y, 0);

            if (frame_id < (numFrames - 1))
            {
                fmodule_min = frameInfos[frame_id];
                fmodule_max = frameInfos[frame_id + 1] - 1;
            }
            else if (frame_id == (numFrames - 1))
            {
                fmodule_min = frameInfos[frame_id];
                fmodule_max = numFModules - 1;
            }

            for (i = fmodule_min; i <= fmodule_max; i++)
            {
                module = fmoduleInfos[i * 2];
                fmodule_transf = fmoduleInfos[i * 2 + 1];
                blend_mode = fmoduleBlends[i];

                ox = fmoduleMatrices[i * 6];
                oy = fmoduleMatrices[i * 6 + 1];

                module_bmp_data = modules_bitmap[module];
                mw = moduleInfos[module * 5];
                mh = moduleInfos[module * 5 + 1];
                if (mw <= 0 || mh <= 0) break;


                fm_trans_matrix = Matrix.Identity;
                fm_trans_matrix.M11 = fmoduleMatrices[i * 6 + 2];
                fm_trans_matrix.M12 = fmoduleMatrices[i * 6 + 3];
                fm_trans_matrix.M21 = fmoduleMatrices[i * 6 + 4];
                fm_trans_matrix.M22 = fmoduleMatrices[i * 6 + 5];
                fm_trans_matrix.M41 = fmoduleMatrices[i * 6] - frame_rect.X;
                fm_trans_matrix.M42 = fmoduleMatrices[i * 6 + 1] - frame_rect.Y;

                if ((fmodule_transf & TRANSFORM_FLIP_H) != 0)
                {
                    fm_trans_matrix *= Matrix.CreateScale(-1, 1, 1);
                }

                if ((fmodule_transf & TRANSFORM_FLIP_V) != 0)
                {
                    fm_trans_matrix *= Matrix.CreateScale(1, -1, 1);
                }

                Utils.draw(back_buffer, module_bmp_data, fm_trans_matrix);
            }

            frame.addModule_bitmap(frame_bitmap, bitmap_trans, frame_rect);
            frame.FrameName = frame_names[frame_id];
            frame_caches[frame_id] = frame;
            return frame;
        }

        public PixAnim GetAnim_bitmap(PixAnim pixAnim)
        {
            int anim_id = pixAnim.getAnimId();
            int minAFrame = 0;
            int maxAFrame = 0;
            int i;
            PixFrame frame;


            minAFrame = animInfos[anim_id];
            if (anim_id == (numAnims - 1))
            {
                maxAFrame = numAFrames - 1;
            }
            else
            {
                maxAFrame = animInfos[anim_id + 1];
            }

            for (i = minAFrame; i < maxAFrame; i++)
            {
                frame = new PixFrame(aframeInfos[i * 5], pixAnim.texture_fullpath, new Vector2(aframeInfos[i * 5 + 1], aframeInfos[i * 5 + 2]));
                pixAnim.addFrame(frame, aframeInfos[i * 5 + 4]);
            }
            pixAnim.FrameName = anim_names[anim_id];
            return pixAnim;
        }

        public System.Drawing.Bitmap GetAnimIcon(int animId)
        {
            int i = animInfos[animId];
            PixFrame frame = new PixFrame(aframeInfos[i * 5], pixma_path, new Vector2(aframeInfos[i * 5 + 1], aframeInfos[i * 5 + 2]));
            return frame.getBitmapView();
        }

        public string GetAnimName(int animId)
        {
            return anim_names[animId];
        }

        public int GetFrameIndex(string frameName)
        {
            //frameName = frameName.Replace("\"", "");
            int index = Array.IndexOf(frame_names, frameName);
            if( index == -1 )
                Logger.Instance.log("Frame: " + frameName + " not exist in " + name);
            return index;
        }

        public int GetAnimIndex(string animName)
        {
           // animName = animName.Replace("\"", "");
            int index = Array.IndexOf(anim_names, animName);
            if (index == -1)
                Logger.Instance.log("Anim: " + animName + " not exist in " + name);
            return index;
        }
    }
}
