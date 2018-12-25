using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITNVPlayMsgManager
{
    public class HuntInfo
    {
        public int total;
        public int free;

        public HuntInfo()
        {
            this.total = 0;
            this.free = 0;
        }

        public HuntInfo(int total, int free)
        {
            this.total = total;
            this.free = free;
        }
    }
}