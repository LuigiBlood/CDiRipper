using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace CDiRipper
{
    public static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }

        public static Analysis LoadedFile;

        public static int[] SearchBytePattern(byte[] array, byte[] pattern)
        {
            List<int> offsets = new List<int>();

            for (int i = 0; i < (array.Length - pattern.Length); i++)
            {
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (array[i + j] != pattern[j]) break;
                    if (j == pattern.Length - 1) offsets.Add(i);
                }
            }

            return offsets.ToArray();
        }

        public static byte[] GetSubarray(byte[] array, int start, int size)
        {
            byte[] output = new byte[size];

            Array.Copy(array, start, output, 0, size);

            return output;
        }

        public static byte[] GetCDiSectorArray(byte[] array, int sector)
        {
            int offset = sector * 0x930;
            int size = 0x930;

            return GetSubarray(array, offset, size);
        }

        public static byte[] CopyCDiRTVideoSectors(byte[] array, int sector)
        {
            List<byte> data = new List<byte>();
            int channel = -1;

            do
            {
                if ((sector * 0x930) >= array.Length) break;
                byte[] sectorData = Program.GetCDiSectorArray(array, sector);
                sector++;

                if (channel == -1) channel = sectorData[17];
                if (channel != sectorData[17]) continue;

                byte submode = sectorData[18];

                //Check if the Data is real time
				//0b01000000
                if ((submode & 64) == 0) break;

                //Check for Data Type
                if ((submode & 0x0E) == 8) break;    //Data
                if ((submode & 0x0E) == 4) continue; //Audio
                if ((submode & 0x0E) == 0) break;    //None

                //Copy Specific Data
                data.AddRange(GetSubarray(sectorData, 24, 0x930 - 24 - 4));

                //Check for End
				//0b10000001
                if ((submode & 129) != 0) break;
                //if (data.Count >= length) break;
            } while (true);

            return data.ToArray();
        }
    }
}
