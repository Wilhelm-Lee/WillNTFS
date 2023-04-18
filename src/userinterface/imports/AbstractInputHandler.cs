namespace WillNTFS.src.userinterface.imports
{
    internal abstract class AbstractInputHandler
    {
        /* 回复限制 */
        private RestrictionStyle mStyle;
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

        protected AbstractInputHandler(RestrictionStyle style, string expect, Stream importStream, Stream exportStream)
        {
            this.mStyle = style;
            this.mExpect = expect;
            this.mImportStream = importStream;
            this.mExportStream = exportStream;
        }

        public abstract int Handle();

        public RestrictionStyle GetRestrictionStyle()
        { return this.mStyle; }

        protected void SetRestrictionStyle(RestrictionStyle style)
        { this.mStyle = style; }

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