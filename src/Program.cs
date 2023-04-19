using System.Drawing;
using WillNTFS.src.userinterface.exports;
using static WillNTFS.src.userinterface.exports.WilliamLogger;
using static WillNTFS.src.userinterface.exports.WilliamLogger.WPriority;
using static WillNTFS.src.userinterface.exports.WilliamLogger.WPurpose;

namespace WillNTFS
{
    internal class Program
    {
        private static readonly WilliamLogger logger = new WilliamLogger(WPriority.DEFAULT, WPurpose.DEFAULT);

        private static readonly string CONTENT
            = "Lorem ipsum dolor sit amet, \nconsectetur adipiscing elit, \nsed do eiusmod tempor incididunt ut labore et dolore magna aliqua. \nMauris rhoncus aenean vel elit.\n";

        private static readonly string LOG_PATH = $"C:\\Users\\User\\Documents\\";
        private static readonly FileStream LOG1 = File.OpenWrite(LOG_PATH + "LOG1");
        private static readonly FileStream LOG2 = File.OpenWrite(LOG_PATH + "LOG2");
        private static readonly FileStream LOG3 = File.OpenWrite(LOG_PATH + "LOG3");

        public static void Main(string[] args)
        {
            /* Expect: Common text shows on terminal */
            Test00();
            /* Expect: Common text shows on terminal with WPriority and WPurpose modified */
            Test01();
            /* Expect: Common text shows on terminal and log file written with content */
            Test02();
        }

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

        private static void Test00()
        {
            logger.Log(
                new object[]
                {
                    $"Test00 START\n{CONTENT}",
                    "Test00 OVER"
                });
        }

        private static void Test01()
        {
            logger.Log(
                new object[]
                {
                    $"Test01 START\n{CONTENT}",
                    "Test01 OVER"
                },
                WPriority.DEBUG,
                WPurpose.TESTING);
        }

        private static void Test02()
        {
            logger.Log(
                new object[]
                {
                    $"Test02 START\n{CONTENT}",
                    "Test02 OVER"
                },
                WPriority.DEBUG,
                WPurpose.TESTING,
                new FileStream[]
                {
                    LOG1,
                    LOG2,
                    LOG3
                });
        }
    }
}
