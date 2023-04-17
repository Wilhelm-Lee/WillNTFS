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
        protected readonly Exception mExcep;
        protected object[] mAssets;

        public ExceptionHunter(Exception e)
        {
            this.mExcep = e;
            this.mAssets = new object[0];
        }

        public ExceptionHunter(Exception e, object[] assets)
        {
            this.mExcep = e;
            this.mAssets = assets;
        }

        public void SetAssets(object[] newAssets)
        {
            this.mAssets = newAssets;
        }
    }
}
