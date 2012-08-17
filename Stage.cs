using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace autostage
{
    public class Stage
    {
        public float altitude;
        public string altitudeTxt;
        public int throttle;
        public string throttleTxt;
        public bool staged;


        public Stage(float a, int t)
        {
            this.altitude = a;
            this.altitudeTxt = string.Format("{0}", a);
            this.throttle = t;
            this.throttleTxt = string.Format("{0}", t);
            this.staged = false;
        }

    }
}
