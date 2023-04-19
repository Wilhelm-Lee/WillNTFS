using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;

using static WillNTFS.src.userinterface.exports.WilliamLogger.WPriority;
using static WillNTFS.src.userinterface.exports.WilliamLogger.WPurpose;

namespace WillNTFS.src.userinterface.exports
{
    // William
    internal class WilliamLogger
    {
        /* Static fields */
        public  readonly static Encoding globalEnc = Encoding.Unicode;
        public  readonly static string WILLIAM_LOG_DECORATION = ">>> ";
        public  readonly static string WILLIAM_SIGN = "William";
        public  readonly static string DEAFULT_WILLIAM_PURPOSE = WPurpose.LOGGING;
        private readonly static string DEFAULT_LOG_FILE_NAME = "UntitledLog";
        private readonly static string DEFAULT_LOG_FILE_PATH = $"C:\\Users\\{Environment.UserName}\\Documents\\WilliamNTFSLog";
        private static WilliamLogger globalWilliamLogger
            = new(WilliamLogger.WPriority.NONE, WilliamLogger.WPurpose.NOTHING);
        private readonly static bool[][] FILE_STREAM_ACCESS_PERMISSION_RESTRICTIONS_ALL
            = new bool[][] { new bool[] { true, true, true }, new bool[] { true, true, true }, new bool[] { true, true, true } };
        private readonly static bool[][] FILE_STREAM_ACCESS_PERMISSION_RESTRICTIONS_NONE
            = new bool[][] { new bool[] { false, false, false }, new bool[] { false, false, false }, new bool[] { false, false, false } };
        private readonly static bool[][] FILE_STREAM_ACCESS_PERMISSION_RESTRICTIONS_READONLY
            = new bool[][] { new bool[] { true, false, false }, new bool[] { true, false, false }, new bool[] { true, false, false } };
        private readonly static bool[][] FILE_STREAM_ACCESS_PERMISSION_RESTRICTIONS_WRITEONLY
            = new bool[][] { new bool[] { false, true, false }, new bool[] { false, true, false }, new bool[] { false, true, false } };
        private readonly static bool[][] FILE_STREAM_ACCESS_PERMISSION_RESTRICTIONS_SEEKONLY
            = new bool[][] { new bool[] { false, false, true }, new bool[] { false, false, true }, new bool[] { false, false, true } };

        /* Instance fields */
        private readonly DateTimeFormat mLogFileNameDateTimeFormat;
        private readonly DateTime mLogFileNameDateTimeValue;
        private readonly string mLogFileName;
        private readonly string mLogFilePath;
        private readonly string mLogFile;
        private readonly object[] mPriority;
        private readonly string mPurpose;
        private readonly StreamWriter[] mRedirections;
        private readonly FileStream[] mFileRedirestions;
        private readonly bool mRedirectionsOnly;

        public class WPriority
        {
            public static readonly object[] NONE         = { "NONE",        int.MinValue };
            public static readonly object[] MINOR        = { "MINOR",       10000        };
            public static readonly object[] NORMAL       = { "NORMAL",      20000        };
            public static readonly object[] MAJOR        = { "MAJOR",       30000        };
            public static readonly object[] SERIOUS      = { "SERIOUS",     40000        };
            public static readonly object[] DANDEROUS    = { "DANDEROUS",   50000        };
            public static readonly object[] FATAL        = { "FATAL",       60000        };
            public static readonly object[] DEBUG        = { "DEBUG",       70000        };
            public static readonly object[] ALL          = { "ALL",         int.MaxValue };
            public static readonly object[] DEFAULT      = NONE;

            private WPriority() { }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="current"></param>
            /// <param name="other"></param>
            /// <returns>2 For incomparable;
            ///          1 For CURRENT is greater than OTHER;
            ///          0 For CURRENT is equal as OTHER;
            ///         -1 For CURRENT is less than OTHER; </returns>
            public static int Compare(object[]? current, object[]? other)
            {
                if (current == null || other == null)
                    return 2;

                try
                {
                    Int64 i64Current = Convert.ToInt64(current[1]);
                    Int64 i64Other = Convert.ToInt64(other[1]);
                    if (i64Current > i64Other)
                    { return 1; }
                    else if (i64Current == i64Other)
                    { return 0; }
                    else
                    { return -1;}

                } catch (IndexOutOfRangeException outOfRangeExcep)
                {
                    globalWilliamLogger
                        .Log(new object[]
                             {
                                 $"Illegal parameter {nameof(current)} and parameter {nameof(other)}\n" +
                                 $"had not have proper length which is used to satisfy {nameof(WPriority)}.\n" +
                                 $"In this particular case:\nParameter {nameof(current)} have length being as {current.Length}\n" +
                                 $"Parameter {nameof(other)} have length being as {current.Length}\n" +
                                 $"While members of {nameof(WPriority)} all require at least having length as much as " +
                                 $"{WPriority.NONE.Length}"
                             },
                             WilliamLogger.WPriority.SERIOUS,
                             WilliamLogger.WPurpose.LOGGING
                        );
                    return 2;
                }
            }

            public static string GetName(object[] priority)
            {
                return (string)priority[0];
            }
        }

        public class WPurpose
        {
            public static readonly string NOTHING   = "Nothing";
            public static readonly string LOGGING   = "Logging";
            public static readonly string TESTING   = "Testing";
            public static readonly string EXCEPTION = "Exception";
            public static readonly string DEFAULT   = NOTHING;
        }

        public WilliamLogger(object[] priority, string wPurpose)
        {
            this.mPriority = priority;
            this.mPurpose = wPurpose;

            this.mLogFileNameDateTimeFormat = new DateTimeFormat("yyyyMMddHHmmss");
            this.mLogFileNameDateTimeValue = DateTime.Now;
            this.mLogFileName = mLogFileNameDateTimeFormat.ToString();
            this.mLogFilePath = DEFAULT_LOG_FILE_PATH;
            this.mLogFile = this.mLogFilePath + this.mLogFileName;
            mLogFileName ??= DEFAULT_LOG_FILE_NAME;

            /* We do not need the keyword "using" here.
             * It will be needed by function Log,
             * because function Log will operate to IO flow. */
            this.mRedirections = new StreamWriter[1];
            /* mRedirections[0] will be initialised in function Log */

            /* Same as above */
            this.mFileRedirestions = new FileStream[1];

            this.mRedirectionsOnly = false;
        }

        public WilliamLogger(object[] priority, string wPurpose, StreamWriter[] redirections)
        {
            this.mPriority = priority;
            this.mPurpose = wPurpose;

            this.mLogFileNameDateTimeFormat = new DateTimeFormat("yyyyMMddHHmmss");
            this.mLogFileNameDateTimeValue = DateTime.Now;
            this.mLogFileName = mLogFileNameDateTimeFormat.ToString();
            this.mLogFilePath = DEFAULT_LOG_FILE_PATH;
            this.mLogFile = this.mLogFilePath + this.mLogFileName;
            mLogFileName ??= DEFAULT_LOG_FILE_NAME;

            /* We do not need the keyword "using" here.
             * It will be needed by function Log,
             * because function Log will operate to IO flow. */
            this.mRedirections = redirections;

            /* Same as above */
            this.mFileRedirestions = new FileStream[1];

            this.mRedirectionsOnly = false;
        }

        public WilliamLogger(object[] priority, string wPurpose, StreamWriter[] redirections, bool redirectionsOnly)
        {
            this.mPriority = priority;
            this.mPurpose = wPurpose;

            this.mLogFileNameDateTimeFormat = new DateTimeFormat("yyyyMMddHHmmss");
            this.mLogFileNameDateTimeValue = DateTime.Now;
            this.mLogFileName = mLogFileNameDateTimeFormat.ToString();
            this.mLogFilePath = DEFAULT_LOG_FILE_PATH;
            this.mLogFile = this.mLogFilePath + this.mLogFileName;
            mLogFileName ??= DEFAULT_LOG_FILE_NAME;

            /* We do not need the keyword "using" here.
             * It will be needed by function Log,
             * because function Log will operate to IO flow. */
            this.mRedirections = redirections;

            /* Same as above */
            this.mFileRedirestions = new FileStream[1];

            this.mRedirectionsOnly = redirectionsOnly;
        }

        public WilliamLogger(object[] priority, string wPurpose, bool redirectionsOnly)
        {
            this.mPriority = priority;
            this.mPurpose = wPurpose;

            this.mLogFileNameDateTimeFormat = new DateTimeFormat("yyyyMMddHHmmss");
            this.mLogFileNameDateTimeValue = DateTime.Now;
            this.mLogFileName = mLogFileNameDateTimeFormat.ToString();
            this.mLogFilePath = DEFAULT_LOG_FILE_PATH;
            this.mLogFile = this.mLogFilePath + this.mLogFileName;
            mLogFileName ??= DEFAULT_LOG_FILE_NAME;

            /* We do not need the keyword "using" here.
             * It will be needed by function Log,
             * because function Log will operate to IO flow. */
            // FIXME: Use FileStream instead. Because StreamWriter does not write for output stream ports.
            this.mRedirections = new StreamWriter[] { new StreamWriter(Console.OpenStandardOutput()) };

            /* Same as above */
            this.mFileRedirestions = new FileStream[1];

            this.mRedirectionsOnly = redirectionsOnly;
        }

        public object[] Priority { get { return mPriority;} }

        public string Purpose { get { return mPurpose;} }

        /*
         * 00 func Log: (object[] info)                                                                                          // Uses @info, Priority.DEFAULT, Purpose.DEFAULT, stderr, Exception, false
         * 01 func Log: (object[] info, object[] priority, string purpose)                                                       // Uses @info, @priority, @purpose, stderr, Exception, false
         * 02 func Log: (object[] info, object[] priority, string purpose, FileStream[] redirections)                            // Uses @info, @priority, @purpose, @redirections, Exception, false
         * 03 func Log: (object[] info, object[] priority, string purpose, FileStream[] redirections, bool redirectionsOnly)     // Uses @info, @priority, @purpose, @redirections, Exception, @redirectionsOnly
         * 03 func Log: (object[] info, object[] priority, string purpose, Exception innerException)                             // Uses @info, @priority, @purpose, stderr, @innerException, false
         * 04 func Log: (object[] info, object[] priority, string purpose, FileStream[] redirections, Exception innerException)  // Uses @info, @priority, @purpose, @redirections, @innerException, false
         * 05 func Log: (object[] info, object[] priority, string purpose, FileStream[] redirections, Exception innerException,  // Uses @info, @priority, @purpose, @redirections, @innerException, @redirectionsOnly
         * ....................................................................................... bool redirectionsOnly)
         * 06 func Log: (object[] info, WilliamLogger logger)                                                                    // Uses @info, @logger.mPriority, @logger.mPurpose, stderr, Exception, false
         * 07 func Log: (object[] info, WilliamLogger logger, FileStream[] redirections)                                         // Uses @info, @logger.mPriority, @logger.mPurpose, @redirections, Exception, false
         * 08 func Log: (object[] info, WilliamLogger logger, FileStream[] redirections, bool redirectionsOnly)                  // Uses @info, @logger.mPriority, @logger.mPurpose, @redirections, Exception, @redirectionsOnly
         * 09 func Log: (object[] info, WilliamLogger logger, Exception innerException)                                          // Uses @info, @logger.mPriority, @logger.mPurpose, stderr, @innerException, false
         * 0A func Log: (object[] info, WilliamLogger logger, FileStream[] redirections, Exception innerException)               // Uses @info, @logger.mPriority, @logger.mPurpose, @redirections, @innerException, false
         * 0B func Log: (object[] info, WilliamLogger logger, FileStream[] redirections, Exception innerException,               // Uses @info, @logger.mPriority, @logger.mPurpose, @redirections, @innerException, @redirectionsOnly
         * .......................................................................... bool redirectionsOnly)
         */

        public void Log(object[] info)
        {
            Log(info, this.Priority, this.Purpose);
        }
        public void Log(object[] info, object[] priority, string purpose)
        {
            priority ??= WPriority.DEFAULT;
            purpose ??= WPurpose.DEFAULT;
            info ??= new object[0];

            if (!this.mRedirectionsOnly)
            {
                using (this.mRedirections[0] = new StreamWriter(Console.OpenStandardOutput()))
                {
                    // Output -> stdout
                    mRedirections[0].Write(GenerateLogContent(GenerateWilliamPrecontent(priority, purpose), info));
                }
            }
        }
        public void Log(object[] info, object[] priority, string purpose, FileStream[] redirections)
        {
            if (priority is null)
            {
                throw new ArgumentNullException(nameof(priority));
            }

            if (string.IsNullOrEmpty(purpose))
            {
                throw new ArgumentException($"\"{nameof(purpose)}\" cannot be empty nor nulled", nameof(purpose));
            }

            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (redirections is null)
            {
                throw new ArgumentNullException(nameof(redirections));
            }

            if (redirections is null)
            {
                throw new ArgumentNullException(nameof(redirections));
            }

            for (int i = 0; i < redirections.Length; i ++)
            {
                try
                {
                    if (!this.mRedirectionsOnly)
                    {
                        using (this.mRedirections[i] = new StreamWriter(Console.OpenStandardOutput()))
                        {
                            // Output -> mRedirections[i]
                            redirections[i].Write(GenerateLogContentByteArray(GenerateWilliamPrecontent(priority, purpose), info));
                        }
                    }
                } catch (Exception e)
                {
                    WilliamLogger.GetGlobal()
                        .Log(new object[]
                             {
                                 $"An exception was thrown when processing {nameof(redirections)}[{i}]:\n",
                                 $"{e.Message} ",
                                 $"This exception will be ignored."
                             },
                             SERIOUS, e.GetType().ToString()
                        );
                }
            }
        }
        public void Log(object[] info, object[] priority, string purpose, FileStream[] redirections, bool redirectionsOnly)
        {
            if (priority is null)
            {
                throw new ArgumentNullException(nameof(priority));
            }

            if (string.IsNullOrEmpty(purpose))
            {
                throw new ArgumentException($"\"{nameof(purpose)}\" cannot be empty nor nulled", nameof(purpose));
            }

            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (redirections is null)
            {
                throw new ArgumentNullException(nameof(redirections));
            }

            if (redirections is null)
            {
                throw new ArgumentNullException(nameof(redirections));
            }

            for (int i = 0; i < redirections.Length; i++)
            {
                try
                {
                    if (!redirectionsOnly)
                    {
                        using (this.mRedirections[i] = new StreamWriter(Console.OpenStandardOutput()))
                        {
                            // Output -> mRedirections[i]
                            redirections[i].Write(GenerateLogContentByteArray(GenerateWilliamPrecontent(priority, purpose), info));
                        }
                    }
                }
                catch (Exception e)
                {
                    WilliamLogger.GetGlobal()
                        .Log(new object[]
                             {
                                 $"An exception was thrown when processing {nameof(redirections)}[{i}]:\n",
                                 $"{e.Message} ",
                                 $"This exception will be ignored."
                             },
                             SERIOUS, e.GetType().ToString()
                        );
                }
            }
        }
        public void Log(object[] info, object[] priority, string purpose, Exception innerException)
        {
            if (priority is null)
            {
                throw new ArgumentNullException(nameof(priority));
            }

            if (string.IsNullOrEmpty(purpose))
            {
                throw new ArgumentException($"\"{nameof(purpose)}\" cannot be empty nor nulled", nameof(purpose));
            }

            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (innerException is null)
            {
                throw new ArgumentNullException(nameof(innerException));
            }

            Log(info, SERIOUS, EXCEPTION);
        }
        public void Log(object[] info, object[] priority, string purpose, FileStream[] redirections, Exception innerException)
        {
            priority ??= WPriority.DEFAULT;
            purpose ??= WPurpose.DEFAULT;
            info ??= new object[0];
            _ = innerException ?? new();

            if (redirections is null)
            {
                throw new ArgumentNullException(nameof(redirections));
            }

            for (int i = 0; i < redirections.Length; i++)
            {
                try
                {
                    if (!this.mRedirectionsOnly)
                    {
                        using (this.mRedirections[i] = new StreamWriter(Console.OpenStandardOutput()))
                        {
                            // Output -> mRedirections[i]
                            redirections[i].Write(Convert.FromBase64String(GenerateLogContent(GenerateWilliamPrecontent(priority, purpose), info)));
                        }
                    }
                }
                catch (Exception e)
                {
                    WilliamLogger.GetGlobal()
                        .Log(new object[]
                             {
                                 $"An exception was thrown when processing {nameof(redirections)}[{i}]:\n",
                                 $"{e.Message} ",
                                 $"This exception will be ignored."
                             },
                             SERIOUS, e.GetType().ToString()
                        );
                }
            }
        }
        public void Log(object[] info, object[] priority, string purpose, FileStream[] redirections, Exception innerException, bool redirectionsOnly)
        {
            priority ??= WPriority.DEFAULT;
            purpose ??= WPurpose.DEFAULT;
            info ??= new object[0];
            _ = innerException ?? new();

            if (redirections is null)
            {
                throw new ArgumentNullException(nameof(redirections));
            }

            for (int i = 0; i < redirections.Length; i++)
            {
                try
                {
                    if (!redirectionsOnly)
                    {
                        using (this.mRedirections[i] = new StreamWriter(Console.OpenStandardOutput()))
                        {
                            // Output -> mRedirections[i]
                            redirections[i].Write(Convert.FromBase64String(GenerateLogContent(GenerateWilliamPrecontent(priority, purpose), info)));
                        }
                    }
                }
                catch (Exception e)
                {
                    WilliamLogger.GetGlobal()
                        .Log(new object[]
                             {
                                 $"An exception was thrown when processing {nameof(redirections)}[{i}]:\n",
                                 $"{e.Message} ",
                                 $"This exception will be ignored."
                             },
                             SERIOUS, e.GetType().ToString()
                        );
                }
            }
        }
        public void Log(object[] info, WilliamLogger logger)
        {
            logger ??= GetGlobal();
            info ??= new object[] { $"{nameof(info)} should never be null." };

            if (!this.mRedirectionsOnly)
            {
                using (this.mRedirections[0] = new StreamWriter(Console.OpenStandardOutput()))
                {
                    // Output -> stdout
                    mRedirections[0].Write(GenerateLogContent(GenerateWilliamPrecontent(logger.Priority, logger.Purpose), info));
                }
            }
        }
        public void Log(object[] info, WilliamLogger logger, FileStream[] redirections)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (redirections is null)
            {
                throw new ArgumentNullException(nameof(redirections));
            }

            for (int i = 0; i < redirections.Length; i++)
            {
                try
                {
                    if (!this.mRedirectionsOnly)
                    {
                        using (this.mRedirections[i] = new StreamWriter(Console.OpenStandardOutput()))
                        {
                            // Output -> mRedirections[i]
                            redirections[i].Write(GenerateLogContentByteArray(GenerateWilliamPrecontent(logger.Priority, logger.Purpose), info));
                        }
                    }
                }
                catch (Exception e)
                {
                    WilliamLogger.GetGlobal()
                        .Log(new object[]
                             {
                                 $"An exception was thrown when processing {nameof(redirections)}[{i}]:\n",
                                 $"{e.Message} ",
                                 $"This exception will be ignored."
                             },
                             SERIOUS, e.GetType().ToString()
                        );
                }
            }
        }
        public void Log(object[] info, WilliamLogger logger, FileStream[] redirections, bool redirectionsOnly)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (redirections is null)
            {
                throw new ArgumentNullException(nameof(redirections));
            }


        }
        public void Log(object[] info, WilliamLogger logger, Exception innerException) { }
        public void Log(object[] info, WilliamLogger logger, FileStream[] redirections, Exception innerException) { }
        public void Log(object[] info, WilliamLogger logger, FileStream[] redirections, Exception innerException, bool redirectionsOnly) { }

        /// <summary>
        /// Make sure every line for outputing is covered with WilliamPrecontent ahead.
        /// </summary>
        /// <param name="targetStr"></param>\
        /// <returns>A single string which contains content of generated info to be logged out.
        ///          It returns "" when content is null or empty. </returns>
        public static string GenerateLogContent(string WilliamPrecontent, object[] targetStr)
        {
            if (targetStr is null)
            {
                WilliamLogger.GetGlobal()
                    .Log(new object[] { $"{nameof(targetStr)} should never be null." },
                         DEBUG,
                         EXCEPTION);
                return "";
            }

            /* Don't forget the first WilliamPreContent~~ */
            string rtn = WilliamPrecontent;

            try
            {
                for (int i = 0; i < targetStr.Length; i++)
                {
                    if (targetStr[i].ToString() == null)
                    {
                        throw new ArgumentNullException();
                    }

                    string currStr = targetStr[i].ToString();
                    char currStrChar = char.MaxValue;

                    for (int j = 0; j < currStr.Length; j++)
                    {
                        currStrChar = currStr[j];
                        if (currStrChar == '\n')
                        {
                            rtn += '\n';
                            rtn += WilliamPrecontent;
                            continue;
                        }
                        rtn += currStrChar;
                    }
                }
            } catch (ArgumentNullException e)
            {
                WilliamLogger.GetGlobal()
                    .Log(new object[] { e.Message },
                         WilliamLogger.WPriority.SERIOUS,
                         WilliamLogger.WPurpose.EXCEPTION);
            }

            /* Finish logging by printting a line breaker. */
            rtn += '\n';
            return rtn; // :)
        }

        /// <summary>
        /// Make sure every line for outputing is covered with WilliamPrecontent ahead.
        /// </summary>
        /// <param name="targetStr"></param>\
        /// <returns>A single string which contains content of generated info to be logged out.
        ///          It returns "" when content is null or empty. </returns>
        public static byte[] GenerateLogContentByteArray(string WilliamPrecontent, object[] targetStr)
        {
            if (targetStr is null)
            {
                WilliamLogger.GetGlobal()
                    .Log(new object[] { $"{nameof(targetStr)} should never be null." },
                         DEBUG,
                         EXCEPTION);
                return new byte[0];
            }

            /* Don't forget the first WilliamPreContent~~ */
            byte[] rtn = Convert.FromBase64String(WilliamPrecontent);

            try
            {
                for (int i = 0; i < targetStr.Length; i++)
                {
                    if (targetStr[i].ToString() == null)
                    {
                        throw new ArgumentNullException();
                    }

                    string currStr = targetStr[i].ToString();
                    char currStrChar = char.MaxValue;

                    for (int j = 0; j < currStr.Length; j++)
                    {
                        currStrChar = currStr[j];
                        if (currStrChar == '\n')
                        {
                            rtn = ByteArrayAppend(rtn, Convert.ToByte('\n'));
                            for (int k = 0; k < WilliamPrecontent.Length; k ++)
                            {
                                rtn = ByteArrayAppend(rtn, Convert.ToByte(WilliamPrecontent[i]));
                            }
                            continue;
                        }
                        rtn = ByteArrayAppend(rtn, Convert.ToByte(currStrChar));
                    }
                }
            }
            catch (ArgumentNullException e)
            {
                WilliamLogger.GetGlobal()
                    .Log(new object[] { e.Message },
                         WilliamLogger.WPriority.SERIOUS,
                         WilliamLogger.WPurpose.EXCEPTION);
            }

            /* Finish logging by printting a line breaker. */
            rtn = ByteArrayAppend(rtn, Convert.ToByte('\n'));
            return rtn; // :)
        }

        private static string GenerateWilliamPrecontent(object[] priority, string purpose)
        {
            return ($"{WILLIAM_LOG_DECORATION}[{priority[0]}]({WILLIAM_SIGN} - {purpose}): ");
        }

        private static byte[] GenerateWilliamPrecontentByteArray(object[] priority, string purpose)
        {
            /* Pick example */
            string williamPrecontentString = GenerateWilliamPrecontent(priority, purpose);

            /* Initiate result container */
            byte[] rtn = new byte[williamPrecontentString.Length];
            /* Fullfill result container */
            for (int i = 0; i < rtn.Length; i ++)
            {
                rtn[i] = Convert.ToByte(williamPrecontentString[i]);
            }

            return rtn;
        }

        public static WilliamLogger GetGlobal()
        {
            return globalWilliamLogger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wLogger"></param>
        /// <param name="streams">Target indexed stream array.</param>
        /// <exception cref="ArgumentException">Thrown once target indexed object is not Stream nor sub-Stream.</exception>
        private void LogToVariantStreams(WilliamLogger wLogger, FileStream[] streams)
        {
            CheckRedirectionsPermissions(streams, FILE_STREAM_ACCESS_PERMISSION_RESTRICTIONS_ALL);

            for (int i = 0; i < streams.Length; i ++)
            {
                if (streams[i] is not Stream)
                {
                    throw new ArgumentException($"Function {LogToVariantStreams} should never have " +
                        $"non-Stream item as its parameters");
                }

                try
                {
                    /* Keyword "using" is used from caller */
                    streams[i].Write(GenerateWilliamPrecontentByteArray(wLogger.mPriority, wLogger.mPurpose));
                } catch (IOException ioe)
                {

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="streams"></param>
        /// <param name="restrictions"></param> An 2D array stores restriction on
        /// making specified permission of item being accessed.
        /// { item1, item2, item3... } is for items;
        /// { READING, WRITTING, SEEKING } is for permissions;
        /// Each item have those 3 permissions to be optionally required.
        /// Formular: restriction[ITEM][PERM] : bool
        /// <exception cref="IOException">Thrown once errored accessing.</exception>
        private void CheckRedirectionsPermissions(FileStream[] streams, bool[][] restrictions)
        {
            for (int i = 0; i < streams.Length; i ++)
            {
                CheckRedirectionPermissions(streams[i], restrictions[i]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="restriction">A linear array stores restriction on
        /// making specified permission of item being accessed.
        /// { READING, WRITTING, SEEKING } is for item's permissions;
        /// Each item have those 3 permissions to be optionally required.
        /// Formular: restriction[ITEM][PERM] : bool */
        /// <exception cref="IOException">Thrown once errored accessing.</exception>
        private void CheckRedirectionPermissions(FileStream stream, bool[] restriction)
        {
            for (int i = 0; i < 3; i ++)
            {
                /* Restricted */
                if (restriction[i])
                {
                    /* Readable -> Do nothing */
                    /* Unreadable -> Exception */
                    if (!stream.CanRead)
                    {
                        throw new IOException($"File \"{stream.Name}\" cannot be read.");
                    }
                    /* Same */
                    if (!stream.CanWrite)
                    {
                        throw new IOException($"File \"{stream.Name}\" cannot be written.");
                    }
                    /* Same */
                    if (!stream.CanSeek)
                    {
                        throw new IOException($"File \"{stream.Name}\" cannot be seeken.");
                    }
                }
                else /* Inrestricted */
                {
                    /* Readable -> Do nothing */
                    /* Unreadable -> Log warnning */
                    if (!stream.CanRead)
                    {
                        string info = $"File \"{stream.Name}\" was neither readable nor restricted.";
                        WilliamLogger.GetGlobal()
                            .Log(new object[] { info },
                            MAJOR, LOGGING, new FileStream[] { File.OpenWrite(this.mLogFile) });
                    }
                    /* Same */
                    if (!stream.CanWrite)
                    {
                        string info = $"File \"{stream.Name}\" was neither writable nor restricted.";
                        WilliamLogger.GetGlobal()
                            .Log(new object[] { info },
                            MAJOR, LOGGING, new FileStream[] { File.OpenWrite(this.mLogFile) });
                    }
                    /* Same */
                    if (!stream.CanSeek)
                    {
                        string info = $"File \"{stream.Name}\" was neither seekable nor restricted.";
                        WilliamLogger.GetGlobal()
                            .Log(new object[] { info },
                            MAJOR, LOGGING, new FileStream[] { File.OpenWrite(this.mLogFile) });
                    }
                }
            }
        }

        public string GetLogFileName()
        {
            return this.mLogFileName;
        }

        public string GetLogFilePath()
        {
            return this.mLogFilePath;
        }

        public string GetLogFile()
        {
            return this.mLogFile;
        }

        private static byte[] ByteArrayAppend(byte[] array, byte content)
        {
            int len = array.Length;

            /* Create a new array<byte> object */
            byte[] newArray = new byte[len + 1];
            /* Copy elements from original array */
            for (int i = 0; i < len; i ++)
            {
                newArray[i] = array[i];
            }
            /* Have the last desired element added into the new array */
            newArray[len - 1] = content;

            return newArray;
        }

        private static bool[][] FileStreamAccessPermissionRestrictionsAdding(bool[][] A, bool[][] B)
        {
            bool[][] rtn = FILE_STREAM_ACCESS_PERMISSION_RESTRICTIONS_NONE;

            for (int i = 0; i < A.Length; i ++)
                for (int j = 0; j < A[0].Length; j ++)
                    rtn[i][j] = (A[i][j] || B[i][j]);
            return rtn;
        }

        private static bool[][] FileStreamAccessPermissionRestrictionsMinusing(bool[][] A, bool[][] B)
        {
            bool[][] rtn = FILE_STREAM_ACCESS_PERMISSION_RESTRICTIONS_NONE;

            for (int i = 0; i < A.Length; i ++)
                for (int j = 0; j < A[0].Length; j ++)
                    rtn[i][j] = (A[i][j] ^ B[i][j]);
            return rtn;
        }
    }
}
