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
        private bool _play;

        public bool Play
        {
            get
            {
                return _play;
            }
            set
            {
                _play = value;
                if (_play == false)
                    stopAllAnim();
                else
                    playAllAnim();
            }
        }

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

            if (curTime > lastTime + 34)
            {
                foreach (Animation anim in list)
                    anim.update();
                lastTime = curTime;
            }

		}

        public void stopAllAnim()
        {
            foreach (Animation anim in list)
                anim.stop(-1);
        }

        public void playAllAnim()
        {
            foreach (Animation anim in list)
                anim.play();
        }
    }
}
