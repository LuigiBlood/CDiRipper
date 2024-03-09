using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;

namespace CDiRipper
{
    public class Analysis
    {
        public byte[] Data;
        public int[] IDATSectors;
        public int[] RTStartSectors;

        public Analysis(string filename)
        {
            Data = File.ReadAllBytes(filename);

            //Analyze
            IDATSectors = GetIDATSectors();
            RTStartSectors = GetRTRecords();
        }

        private int[] GetIDATSectors()
        {
            List<int> list = new List<int>();
            foreach (int offset in Program.SearchBytePattern(Data, Encoding.ASCII.GetBytes("IDAT")))
            {
                list.Add(offset / 0x930);
            }
            return list.ToArray();
        }

        private int[] GetRTRecords()
        {
            List<int> list = new List<int>();

            int amountSectors = Data.Length / 0x930;
            byte[] sectorSync = { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00 };
            for (int i = 0; i < amountSectors; i++)
            {
                //Check Sector Synchronization
                //if (!Program.GetSubarray(Data, i * 0x930, sectorSync.Length).Equals(sectorSync)) break;
                //Must be Mode 2 Form
                if (Data[(i * 0x930) + 0x0F] != 0x02) break;
                //Look for Real Time Sector
                if ((Data[(i * 0x930) + 0x12] & 0x40) != 0)
                {
                    list.Add(i);
                    while (true)
                    {
                        i++;
                        if (i >= amountSectors) break;
                        byte submode = Data[(i * 0x930) + 0x12];
                        if ((submode & 0x81) != 0) break;
                        if ((submode & 0x40) == 0) break;
                    }
                }
            }

            return list.ToArray();
        }

        public Bitmap GetImage(int firstrtsector, int id)
        {
            int maxSectors = Data.Length / 0x930;

            int width = 384;
            int height = 280;
            int curId = -1;

            //Bitmap output = new Bitmap(width, height);

            Color[] palette = null;
            bool insideVideo = false;

            for (int i = firstrtsector; i < maxSectors; i++)
            {
                byte channel = Data[(i * 0x930) + 0x11];
                byte submode = Data[(i * 0x930) + 0x12];
                byte codeInfo = Data[(i * 0x930) + 0x13];
                //Ensure Sector is real time
                if ((submode & 0x40) == 0) break;
                //Last Sector
                if ((submode & 0x81) != 0) maxSectors = i + 1;
                //Find first sector that's not audio/none
                if ((submode & 0x0E) == 0)
                {
                    //None
                    insideVideo = false;
                    continue;
                }

                if ((submode & 0x0E) == 4)
                {
                    //Audio
                    continue;
                }

                if ((submode & 0x0E) == 8)
                {
                    //Data (should have palette)
                    insideVideo = false;
                    palette = Image.GetPalette(Data, i, 128);
                }

                if ((submode & 0x0E) == 2)
                {
                    //Video
                    //Add FORM check here later
                    if (!insideVideo)
                    {
                        insideVideo = true;
                        curId++;
                    }
                    if (curId != id) continue;
                    switch (codeInfo & 0x0F)
                    {
                        case 0x01:
                            return Image.GetCLUT7(palette, Data, i);
                        case 0x04:
                            return Image.GetRL7(palette, Data, i);
                    }
                }
            }

            return null;
        }
    }
}
