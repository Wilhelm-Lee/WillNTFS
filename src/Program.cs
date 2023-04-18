using WillNTFS.src.userinterface.exports;
using static WillNTFS.src.userinterface.exports.WilliamLogger;

namespace WillNTFS
{
    internal class Program
    {
        static void Main(string[] args)
        {
            FileStream target1 = File.OpenWrite("D:\\WillNTFS1.txt");
            FileStream target2 = File.OpenWrite("D:\\WillNTFS2.txt");
            WilliamLogger logger = new(WPriority.ALL, WPurpose.LOGGING, true);

            WilliamLogger.GetGlobal().Log(
                new object[]
                {
                    "TEST TEXT\nSECOND LINE"
                },
                logger,
                new FileStream[]
                {
                    target1,
                    target2
                });
        }
    }
}
