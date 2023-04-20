/*using WillNTFS.src.userinterface.exports;*/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*using static WillNTFS.src.userinterface.exports.WilliamLogger.WPriority;
using static WillNTFS.src.userinterface.exports.WilliamLogger.WPurpose;*/
using log4net;
using log4net.Repository.Hierarchy;

namespace WillNTFS.src.sectorinterface
{
    internal abstract class SectorOperator
    {
        private ILog sectorLogger
            = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected readonly Stream stream;
        private Int16[] buffer;
        private int buffUsed;
        private int buffSize;

        protected SectorOperator(Stream stream, int buffSize)
        {
            this.stream = stream;
            this.buffSize = buffSize;
            this.buffer = new Int16[buffSize];
            this.buffUsed = 0;
        }

        protected int GetBuffUsed()
        {
            return this.buffUsed;
        }

        protected int GetBuffSize()
        {
            return this.buffSize;
        }

        /* Returns -1 once predicted overflow */
        protected int WriteBuff(Int16[] content)
        {
            for (int i = buffUsed; i < content.Length; i ++)
            {
                if (i > buffSize)
                {
                    buffUsed = i;
                    return -1;
                }

                buffer[i] = content[i];
            }

            return buffUsed;
        }

        /* Returns -1 once predicted overflow */
        protected int WriteBuff(Int16[] buff, int offset, int count)
        {
            /*int i = offset;
            for (; i < (offset + count); i ++)
            {
                if (i > buffSize)
                {
                    buffUsed = i;
                    return -1;
                }

                buffer[i] = buff[i];
            }
            buffUsed = i;
            return buffUsed;*/

            try
            {
                // TEST(William): Not Finished
                return -1;
                // TEST OVER
            } catch (IndexOutOfRangeException e) {
/*                WilliamLogger.GetGlobal()
                    .Log(
                         new object[]
                         {
                             $"{nameof(WriteBuff)} had a problem with range indexing."
                         },
                         WilliamLogger.WPriority.SERIOUS,
                         WilliamLogger.WPurpose.EXCEPTION
                    );
                using (FileStream redir = File.OpenWrite(WilliamLogger.GetGlobal().GetLogFile()))
                {
                    WilliamLogger.GetGlobal()
                    .Log(
                         new object[]
                         {
                             e.Message
                         },
                         WilliamLogger.WPriority.SERIOUS,
                         WilliamLogger.WPurpose.EXCEPTION,
                         new FileStream[]
                         {
                             redir
                         },
                         true
                         );
                }*/

                sectorLogger.Error($"{nameof(WriteBuff)} had a problem with range indexing.", e);

                return -1;
            } catch (IOException ioe)
            {
                sectorLogger.Error($"{nameof(WriteBuff)} had a problem with I/O.", ioe);

                return -1;
            }

        }

        protected Int16[] ReadBuff() { return buffer; }

        protected Int16[] ReadBuff(int offset, int length)
        {
            Int16[] buffRead = new Int16[length];

            for (int i = 0; i < length; i ++)
            {
                buffRead[i] = buffer[offset + i];
            }

            return buffRead;
        }

    }
}
