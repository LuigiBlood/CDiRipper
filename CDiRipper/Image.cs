using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace CDiRipper
{
    public static class Image
    {
        public static Color[] GetPalette(byte[] array, int sector, int amount)
        {
            byte[] sectorData = Program.GetCDiSectorArray(array, sector);
            int offset = 24;    //TODO PLTE check

            Color[] pal = new Color[amount];

            for (int i = 0; i < pal.Length; i++)
            {
                int r = sectorData[(i * 3) + 0 + offset];
                int g = sectorData[(i * 3) + 1 + offset];
                int b = sectorData[(i * 3) + 2 + offset];
                pal[i] = Color.FromArgb(r, g, b);
            }

            return pal;
        }

        public static Bitmap GetRL7(Color[] palette, byte[] array, int sector)
        {
            if (palette == null) return null;

            byte[] imageData = Program.CopyCDiRTVideoSectors(array, sector);

            int width = 384;
            int height = 280;

            Bitmap output = new Bitmap(width, height);

            MemoryStream dataArray = new MemoryStream(imageData);
            dataArray.Seek(0, SeekOrigin.Begin);

            for (int i = 0; i < width * height; i++)
            {
                int dat = dataArray.ReadByte();
                int nb = 1;
                if ((dat & 0x80) != 0)
                {
                    nb = dataArray.ReadByte();
                }
                if (nb == 0) nb = width - (i % width);

                while (nb > 1)
                {
                    if ((i / width) >= height) return output;
                    output.SetPixel(i % width, i / width, palette[dat & 0x7F]);
                    i++;
                    nb--;
                }

                if ((i / width) >= height) return output;

                output.SetPixel(i % width, i / width, palette[dat & 0x7F]);
            }

            return output;
        }

        public static Bitmap GetCLUT7(Color[] palette, byte[] array, int sector)
        {
            if (palette == null) return null;

            byte[] imageData = Program.CopyCDiRTVideoSectors(array, sector);

            int width = 384;
            int height = 280;

            Bitmap output = new Bitmap(width, height);

            for (int i = 0; i < width * height; i++)
            {
                if (i >= imageData.Length) break;
                output.SetPixel(i % width, i / width, palette[imageData[i] & 0x7F]);
            }

            return output;
        }
    }
}
