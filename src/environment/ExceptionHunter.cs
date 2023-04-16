using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackNTFS.src.environment
{
    internal class ExceptionHunter
    {
        protected readonly Exception mE;
        protected object[] mAssets;

        public ExceptionHunter(Exception e)
        {
            this.mE = e;
            this.mAssets = new object[0];
        }

        public ExceptionHunter(Exception e, object[] assets)
        {
            this.mE = e;
            this.mAssets = assets;
        }

        public void SetAssets(object[] newAssets)
        {
            this.mAssets = newAssets;
        }
    }
}
