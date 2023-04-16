using JackNTFS.src.userinterface.exports;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static JackNTFS.src.userinterface.exports.WilliamLogger.WPriority;
using static JackNTFS.src.userinterface.exports.WilliamLogger.WPurpose;

namespace JackNTFS.src.sectorinterface
{
    internal abstract class SectorOperator
    {
        protected readonly Stream stream;
        private char[] buffer;
        private int buffUsed;
        private int buffSize;

        protected SectorOperator(Stream stream, int buffSize)
        {
            this.stream = stream;
            this.buffSize = buffSize;
            this.buffer = new char[buffSize];
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
        protected int WriteBuff(char[] content)
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
        protected int WriteBuff(int offset, int length, char[] content)
        {
            /*int i = offset;
            for (; i < (offset + length); i ++)
            {
                if (i > buffSize)
                {
                    buffUsed = i;
                    return -1;
                }

                buffer[i] = content[i];
            }
            buffUsed = i;
            return buffUsed;*/

            try
            {
                // TEST(William): Not Finished
                return -1;
                // TEST OVER
            } catch (IndexOutOfRangeException e) {
                WilliamLogger.GetGlobal()
                    .Log(WilliamLogger.WPriority.SERIOUS,
                         WilliamLogger.WPurpose.LOGGING,
                         new object[]
                         {
                             $"{nameof(WriteBuff)} had a problem with range indexing."
                         }
                    );

                return -1;
            }

        }

        protected char[] ReadBuff() { return buffer; }

        protected char[] ReadBuff(int offset, int length)
        {
            char[] buffRead = new char[length];

            for (int i = 0; i < length; i ++)
            {
                buffRead[i] = buffer[offset + i];
            }

            return buffRead;
        }

    }
}
