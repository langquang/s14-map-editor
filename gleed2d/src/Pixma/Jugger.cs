using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace GLEED2D
{
    public class Jugger
    {
        private ArrayList list;
        private double lastTime;
        private double curTime;	

        public Jugger()
        {
            list = new ArrayList();
            lastTime = 0;
        }

        public void add( Animation animate)
        {
			list.Add(animate);
		}
		
		public void remove(Animation animate)
        {
            list.Remove(animate);
		}

        public void update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            curTime = gameTime.TotalGameTime.TotalMilliseconds;
            if (lastTime == 0)
                lastTime = curTime;

            if (curTime > lastTime + 34)
            {
                foreach (Animation anim in list)
                    anim.update();
                lastTime = curTime;
            }

		}
    }
}
