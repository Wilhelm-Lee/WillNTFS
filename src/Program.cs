using System.Drawing;
using WillNTFS.src.userinterface.exports;
using static WillNTFS.src.userinterface.exports.WilliamLogger;
using static WillNTFS.src.userinterface.exports.WilliamLogger.WPriority;
using static WillNTFS.src.userinterface.exports.WilliamLogger.WPurpose;
using log4net;
using log4net.Repository.Hierarchy;
using WillNTFS.src.environment;
using System.Text;
using System;

namespace WillNTFS
{
    internal class Program
    {
        /*private static ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);*/
        private static WilliamLogger logger = new WilliamLogger(WPriority.DEFAULT, WPurpose.DEFAULT);

        public static void Main(string[] args)
        {
            Test();

            logger.Log(new object[] { "Test\nTest\n\tTest" });
        }

        private static void Test()
        {
            
            string file = "D:\\Projects\\WillNTFS\\WillNTFS_TEST";

            logger.CheckCreateOnFile(file);

            byte[] buffer = new byte[4096];

            /* Write first */
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Write))
            {

                /* Give value to buffer to write */
                for (int i = 0; i < 4096; i++)
                {
                    if ((i % 64 == 0))
                        buffer[i] = (byte)'\n';
                    else
                        buffer[i] = (byte)(65 + (i % 26));
                }

                fs.Write(buffer, 0, 4096);

                /* Empty buffer */
                for (int i = 0; i < 4096; i++)
                {
                    buffer[i] = 0;
                }
            }

            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                fs.ReadExactly(buffer, 0, 4096);

                /* Output into console */
                for (int i = 0; i < 4096; i++)
                {
                    Console.Write((char)buffer[i]);
                }
                logger.Log(new object[] { buffer });
            }
        }
    }
}
