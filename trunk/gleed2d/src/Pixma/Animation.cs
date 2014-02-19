using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GLEED2D
{
    public interface Animation
    {
        void update();
        void stop(int frame);
        void play();
    }
}
