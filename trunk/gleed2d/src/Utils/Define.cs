using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GLEED2D
{
    public enum MaskType
    {
        MASK_NONE, MASK_RESTRICTION_TILE, MASK_MAP
    }

    class Define
    {
        public static readonly string TWO_D = "2D";
        public static readonly string ISO_METRIC = "Iso Metric";
        public static bool is_iso = true;
        public static bool is_nagetive = true;
        public static int FlipHorizontally = 1;
        public static int FlipVertically = 2;
        public static string TYPE_IMAGE = "image";
        public static string TYPE_FOLDER = "folder";
        public static string TYPE_FRAME = "frame";
        public static string TYPE_ANIM = "anim";
        public static string TYPE_SEPARATE = ";";
        public static string file_data_path = "";
        public static string run_from_file = "";
        public static bool EXPORT_JSON = false;

    }
}
