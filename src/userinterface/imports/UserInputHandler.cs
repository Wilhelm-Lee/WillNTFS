namespace JackNTFS.src.userinterface.imports
{
    internal class UserInputHandler : AbstractInputHandler
    {
        private string mUserInput;
        private bool mResult;

        public UserInputHandler(RestrictionStyle restr, string expect, Stream importStream, Stream exportStream)
            : base(restr, expect, importStream, exportStream)
        {
            mUserInput = "";
            mResult = false;
        }

        public override int Handle()
        {
            mUserInput = new StreamReader(mImportStream).ReadLine();

            if (mUserInput == null)
            {
                mUserInput = "";
                mResult = false;
                return -1;
            }

            return base.GetRestrictionStyle() switch
            {
                /* If mUserInput is more than ONE character, then we only compare the first character
                 * in that string regardless to the difference of length. And same to mExpect */
                (RestrictionStyle.SINGULARITY) => ((mUserInput[0] == GetExpect()[0]) ? 0 : 1),
                (RestrictionStyle.MULTIPARITY) => (string.Compare(mUserInput, GetExpect())),
                _ => -1,
            };
        }

        public string GetLastInput()
        { return mUserInput; }

        public bool GetLastResult()
        { return mResult; }
    }
}
