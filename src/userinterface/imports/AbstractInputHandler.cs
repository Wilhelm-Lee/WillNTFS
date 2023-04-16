namespace JackNTFS.src.userinterface.imports
{
    internal abstract class AbstractInputHandler
    {
        /* 回复限制 */
        private RestrictionStyle mRestr;
        /* 回复期盼 */
        private string mExpect;
        /* 输入流源 */
        protected readonly Stream mImportStream;
        /* 输出流源 */
        protected readonly Stream mExportStream;

        public enum RestrictionStyle
        {
            /* 单项回复 */
            SINGULARITY = 0,
            /* 多项回复 */
            MULTIPARITY
        }

        protected AbstractInputHandler(RestrictionStyle restr, String expect, Stream importStream, Stream exportStream)
        {
            this.mRestr = restr;
            this.mExpect = expect;
            this.mImportStream = importStream;
            this.mExportStream = exportStream;
        }

        public abstract int Handle();

        public RestrictionStyle GetRestrictionStyle()
        { return this.mRestr; }

        protected void SetRestrictionStyle(RestrictionStyle restr)
        { this.mRestr = restr; }

        public string GetExpect()
        { return mExpect; }

        protected void SetExpect(string expect)
        { this.mExpect = expect; }

        public Stream GetImportStream()
        { return this.mImportStream; }

        public Stream GetExportStream()
        { return this.mExportStream; }

    }
}