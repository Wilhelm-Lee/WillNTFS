using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using static JackNTFS.src.userinterface.exports.WilliamLogger.WPriority;
using static JackNTFS.src.userinterface.exports.WilliamLogger.WPurpose;

namespace JackNTFS.src.userinterface.exports
{
    // William
    internal class WilliamLogger
    {
        /* Static fields */
        public  readonly static string WILLIAM_LOG_DECORATION = ">>> ";
        public  readonly static string WILLIAM_SIGN = "William";
        public  readonly static string WILLIAM_DEAFULT_PURPOSE = "Logging";
        private readonly static string DEFAULT_LOG_FILE_NAME = "UntitledLog";
        private static WilliamLogger globalWilliamLogger
            = new(WilliamLogger.WPriority.NONE, WilliamLogger.WPurpose.NOTHING);

        /* Instance fields */
        private readonly DateTimeFormat mLOG_FILE_NAME_DATE_TIME_FORMAT;
        private readonly DateTime mLOG_FILE_NAME_DATE_TIME_VALUE;
        private readonly string mLOG_FILE_NAME;
        private readonly object[] mPriority;
        private readonly string mPurpose;
        private readonly StreamWriter[] mRedirections;
        private readonly FileStream[] mFileRedirestions;

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
                        .Log(WilliamLogger.WPriority.SERIOUS,
                             WilliamLogger.WPurpose.LOGGING,
                             new object[]
                             {
                                 $"Illegal parameter {nameof(current)} and parameter {nameof(other)}\n" +
                                 $"had not have proper length which is used to satisfy {nameof(WPriority)}.\n" +
                                 $"In this particular case:\nParameter {nameof(current)} have length being as {current.Length}\n" +
                                 $"Parameter {nameof(other)} have length being as {current.Length}\n" +
                                 $"While members of {nameof(WPriority)} all require at least having length as much as " +
                                 $"{WPriority.NONE.Length}"
                             }
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
            public static readonly string DEAFULT   = NOTHING;
        }

        public WilliamLogger(object[] wPriority, string wPurpose)
        {
            this.mPriority = wPriority;
            this.mPurpose = wPurpose;

            this.mLOG_FILE_NAME_DATE_TIME_FORMAT = new DateTimeFormat("yyyyMMddHHmmss");
            this.mLOG_FILE_NAME_DATE_TIME_VALUE = DateTime.Now;
            this.mLOG_FILE_NAME = mLOG_FILE_NAME_DATE_TIME_FORMAT.ToString();
            mLOG_FILE_NAME ??= DEFAULT_LOG_FILE_NAME;

            /* 这里不需要用到 using。
             * Log 函数会有需要，因为会对流进行操作。 */
            this.mRedirections = new StreamWriter[1];
            /* mRedirections[0] 会在 Log 函数中初始化 */

            /* 同理 */
            this.mFileRedirestions = new FileStream[1];
        }

        public WilliamLogger(object[] wPriority, string wPurpose, StreamWriter[] redirections)
        {
            this.mPriority = wPriority;
            this.mPurpose = wPurpose;

            this.mLOG_FILE_NAME_DATE_TIME_FORMAT = new DateTimeFormat("yyyyMMddHHmmss");
            this.mLOG_FILE_NAME_DATE_TIME_VALUE = DateTime.Now;
            this.mLOG_FILE_NAME = mLOG_FILE_NAME_DATE_TIME_FORMAT.ToString();
            mLOG_FILE_NAME ??= DEFAULT_LOG_FILE_NAME;

            this.mRedirections = redirections;

            /* 这里不需要用到 using。
             * Log 函数会有需要，因为会对流进行操作。 */
            this.mRedirections = redirections;

            /* 同理 */
            this.mFileRedirestions = new FileStream[1];
        }

        public object[] Priority { get { return mPriority;} }

        public string Purpose { get { return mPurpose;} }

        /*
         *       msg:object[]
         *  priority:object[], purpose:object[], msg:object[]
         *  priority:object[], purpose:string,   msg:object[], innerException:Exception
         *  priority:object[], purpose:string,   msg:object[], innerException:Exception, redirections:StreamWriter[]
         *    logger:WilliamLogger, object[] msg, StreamWriter[] redirections
         */

        public void Log(object[] msg)
        {
            Log(this.Priority, this.Purpose, msg);
        }

        public void Log(object[] priority, string purpose, object[] msg)
        {
            priority ??= DEFAULT;
            purpose ??= DEAFULT;
            msg ??= new object[0];

            using (this.mRedirections[0] = new StreamWriter(Console.OpenStandardOutput()))
            {
                // Output -> stdout
                mRedirections[0].Write(GenerateLogContent(GenerateWilliamPrecontent(priority, purpose), msg));
            }
        }

        public void Log(WilliamLogger logger, object[] msg)
        {
            logger ??= GetGlobal();
            msg ??= new object[] { $"{nameof(msg)} should never be null." };

            using (this.mRedirections[0] = new StreamWriter(Console.OpenStandardOutput()))
            {
                // Output -> stdout
                mRedirections[0].Write(GenerateLogContent(GenerateWilliamPrecontent(logger.Priority, logger.Purpose), msg));
            }
        }

        public void Log(object[] priority, string purpose, object[] msg, Exception innerException)
        {
            if (priority is null)
            {
                throw new ArgumentNullException(nameof(priority));
            }

            if (string.IsNullOrEmpty(purpose))
            {
                throw new ArgumentException($"“{nameof(purpose)}”不能为 null 或空。", nameof(purpose));
            }

            if (msg is null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            if (innerException is null)
            {
                throw new ArgumentNullException(nameof(innerException));
            }

            Log(SERIOUS, EXCEPTION, msg);
        }

        // Rewrite Log functions with adding parameter mRedirections : StreamWriter[]
        public void Log(object[] priority, string purpose, object[] msg, StreamWriter[] redirections)
        {
            if (priority is null)
            {
                throw new ArgumentNullException(nameof(priority));
            }

            if (string.IsNullOrEmpty(purpose))
            {
                throw new ArgumentException($"“{nameof(purpose)}”不能为 null 或空。", nameof(purpose));
            }

            if (msg is null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            if (redirections is null)
            {
                throw new ArgumentNullException(nameof(redirections));
            }

            for (int i = 0; i < redirections.Length; i ++)
            {
                try
                {
                    using (this.mRedirections[i] = new StreamWriter(Console.OpenStandardOutput()))
                    {
                        // Output -> mRedirections[i]
                        redirections[i].Write(GenerateLogContent(GenerateWilliamPrecontent(priority, purpose), msg));
                    }
                } catch (Exception e)
                {
                    WilliamLogger.GetGlobal()
                        .Log(SERIOUS, EXCEPTION,
                             new object[]
                             {
                                 $"An exception was thrown when processing {nameof(redirections)}[{i}]:\n",
                                 $"{e.Message}",
                                 $"This exception will be ignored."
                             }
                        );
                }
            }
        }

        public void Log(WilliamLogger logger, object[] msg, StreamWriter[] redirections)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (msg is null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            if (redirections is null)
            {
                throw new ArgumentNullException(nameof(redirections));
            }

            for (int i = 0; i < redirections.Length; i++)
            {
                try
                {
                    using (this.mRedirections[i] = new StreamWriter(Console.OpenStandardOutput()))
                    {
                        // Output -> mRedirections[i]
                        redirections[i].Write(GenerateLogContent(GenerateWilliamPrecontent(logger.Priority, logger.Purpose), msg));
                    }
                }
                catch (Exception e)
                {
                    WilliamLogger.GetGlobal()
                        .Log(SERIOUS, EXCEPTION,
                             new object[]
                             {
                                 $"An exception was thrown when processing {nameof(redirections)}[{i}]:\n",
                                 $"{e.Message}",
                                 $"This exception will be ignored."
                             }
                        );
                }
            }
        }

        public void Log(object[] priority, string purpose, object[] msg, Exception innerException, StreamWriter[] redirections)
        {
            if (priority is null)
            {
                throw new ArgumentNullException(nameof(priority));
            }

            if (string.IsNullOrEmpty(purpose))
            {
                throw new ArgumentException($"“{nameof(purpose)}”不能为 null 或空。", nameof(purpose));
            }

            if (msg is null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            if (innerException is null)
            {
                throw new ArgumentNullException(nameof(innerException));
            }

            if (redirections is null)
            {
                throw new ArgumentNullException(nameof(redirections));
            }

            for (int i = 0; i < redirections.Length; i++)
            {
                try
                {
                    using (this.mRedirections[i] = new StreamWriter(Console.OpenStandardOutput()))
                    {
                        // Output -> mRedirections[i]
                        redirections[i].Write(GenerateLogContent(GenerateWilliamPrecontent(priority, purpose), msg));
                    }
                }
                catch (Exception e)
                {
                    WilliamLogger.GetGlobal()
                        .Log(SERIOUS, EXCEPTION,
                             new object[]
                             {
                                 $"An exception was thrown when processing {nameof(redirections)}[{i}]:\n",
                                 $"{e.Message}",
                                 $"This exception will be ignored."
                             }
                        );
                }
            }
        }

        public void Log(object[] priority, string purpose, object[] msg, FileStream[] fileRedirections)
        {
            if (priority is null)
            {
                throw new ArgumentNullException(nameof(priority));
            }

            if (string.IsNullOrEmpty(purpose))
            {
                throw new ArgumentException($"“{nameof(purpose)}”不能为 null 或空。", nameof(purpose));
            }

            if (msg is null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            if (fileRedirections is null)
            {
                throw new ArgumentNullException(nameof(fileRedirections));
            }

            try
            {
                // Permission [Read|Write|Seek] checking.
                CheckPermissions(fileRedirections,
                                 new bool[][]
                                 {
                                     new bool[] { true, true, false }
                                 });

                // HERE
            }
            catch (IOException ioe)
            {
                WilliamLogger.GetGlobal()
                    .Log(MAJOR,
                         EXCEPTION,
                         new object[]
                         {
                             ioe.Message + "\n",
                             "Program shall not proceed.\n" +
                             "You will experience unsuspected procedure if you insist."
                         });
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Make sure every line for outputing is covered with WilliamPrecontent ahead.
        /// </summary>
        /// <param name="targetStr"></param>\
        /// <returns>A single string which contains content of generated info to be logged out.
        ///          It returns "" when content is null or empty. </returns>
        public static string GenerateLogContent(string WilliamPrecontent, object[] targetStr)
        {
            if (WilliamPrecontent is null)
            {
                throw new ArgumentNullException($"{nameof(WilliamPrecontent)} should never be null.");
            }

            if (targetStr is null)
            {
                WilliamLogger.GetGlobal()
                    .Log(DEBUG,
                         EXCEPTION,
                         new object[] { $"{nameof(targetStr)} should never be null." });
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

                    /*byte[] byteCharr = new byte[currStr.Length];
                    int k = 0;

                    string charrResult = "";
                    *//* Loop through every char, until we meet a '\n' *//*
                    for (int j = 0; j < currStr.Length; j ++)
                    {
                        currStrChar = currStr[j];

                        if (currStrChar == '\n')
                        {
                            *//* Add all collected chars into byteCharr *//*
                            for (; k < j; k ++)
                            {
                                byteCharr[k] = Convert.ToByte(currStr[k]);
                            }
                            *//* Skip '\n' *//*
                            k ++;

                            *//* Apply changes *//*
                            charrResult += new String(Encoding.UTF8.GetChars((byteCharr)));

                            *//* Flush all(used) from byteCharr *//*
                            for (int l = 0; l < k; l ++)
                            {
                                byteCharr[l] = 0;
                            }

                            *//* Reset indexer k *//*
                            k = 0;
                        }
                        else
                            currStrChar = currStr[j];
                    }*/
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
                    .Log(WilliamLogger.WPriority.SERIOUS,
                         WilliamLogger.WPurpose.EXCEPTION,
                         new object[] { e.Message });
            }

            /* Finish logging by printting a line breaker. */
            rtn += '\n';
            return rtn; // :)
        }

        private static string GenerateWilliamPrecontent(object[] priority, string purpose)
        {
            return ($"{WILLIAM_LOG_DECORATION}[{priority[0]}]({WILLIAM_SIGN} - {purpose}): ");
        }

        private static byte[] GenerateWilliamPrecontentByteArray(object[] priority, string purpose)
        {
            string williamPrecontentString = GenerateWilliamPrecontent(priority, purpose);

            byte[] rtn = new byte[williamPrecontentString.Length];
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

        private static void LogToVariantStreams(WilliamLogger wLogger, object[] streams)
        {
            for (int i = 0; i < streams.Length; i ++)
            {
                if (streams[i] is not Stream)
                {
                    throw new ArgumentException($"Function {LogToVariantStreams} should never have " +
                        $"non-Stream item as its parameters");
                }

                /* Keyword "using" is used from caller */
                ((FileStream)streams[i]).Write(GenerateWilliamPrecontentByteArray(wLogger.mPriority, wLogger.mPurpose));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="streams"></param>
        /// <param name="restrictions"></param> An 2D array stores restrictions on
        /// making specified permission of item being accessible.
        /// { item1, item2, item3... } is for items;
        /// { READING, WRITTING, SEEKING } is for permissions;
        /// Each item have those 3 permissions to be optionally required.
        /// Formular: restrictions[ITEM][PERM] : bool
        /// <exception cref="IOException"></exception>
        private static void CheckPermissions(FileStream[] streams, bool[][] restrictions)
        {
            for (int i = 0; i < streams.Length; i ++)
            {
                // Check for reading permission
                if (!streams[i].CanRead && restrictions[i][0])
                    throw new IOException($"File \"{streams[i]}\" cannot be read.");
                // Check for writting permission
                if (!streams[i].CanWrite && restrictions[i][1])
                    throw new IOException($"File \"{streams[i]}\" cannot be written.");
                // Check for seeking permission
                if (!streams[i].CanSeek && restrictions[i][2])
                    throw new IOException($"File \"{streams[i]}\" cannot be seeken.");
            }
        }
    }
}
