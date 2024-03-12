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
            int offset = 24;

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
			var bm = output.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            //Check for IDAT
            int offset = 0;
            foreach (int i in Program.SearchBytePattern(imageData, Encoding.ASCII.GetBytes("IDAT")))
            {
                offset = i + 8;
                break;
            }

            MemoryStream dataArray = new MemoryStream(imageData);
            dataArray.Seek(offset, SeekOrigin.Begin);

            for (int i = 0; i < width * height; i++)
            {
                int dat = dataArray.ReadByte();
                if (dat == -1) break;
                int nb = 1;
                if ((dat & 0x80) != 0)
                {
                    nb = dataArray.ReadByte();
                    if (nb == -1) break;
                }
                if (nb == 0) nb = width - (i % width);

                while (nb > 1)
                {
                    if ((i / width) >= height) goto end;
                    //output.SetPixel(i % width, i / width, palette[dat & 0x7F]);
					SetBitmapPixel(bm, i, palette[dat & 0x7F]);
                    i++;
                    nb--;
                }

                if ((i / width) >= height) goto end;

                //output.SetPixel(i % width, i / width, palette[dat & 0x7F]);
				SetBitmapPixel(bm, i, palette[dat & 0x7F]);
            }
end:
			dataArray.Dispose();
			output.UnlockBits(bm);
            return output;
        }

        public static Bitmap GetCLUT7(Color[] palette, byte[] array, int sector)
        {
            if (palette == null) return null;

            byte[] imageData = Program.CopyCDiRTVideoSectors(array, sector);

            int width = 384;
            int height = 280;

            Bitmap output = new Bitmap(width, height);
			var bm = output.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            //Check for IDAT
            int offset = 0;
            foreach (int i in Program.SearchBytePattern(imageData, Encoding.ASCII.GetBytes("IDAT")))
            {
                offset = i + 8;
                break;
            }

            MemoryStream dataArray = new MemoryStream(imageData);
            dataArray.Seek(offset, SeekOrigin.Begin);

            for (int i = 0; i < width * height; i++)
            {
                int dat = dataArray.ReadByte();
                if (dat == -1) break;
                //output.SetPixel(i % width, i / width, palette[dat & 0x7F]);
				SetBitmapPixel(bm, i, palette[dat & 0x7F]);
            }
			dataArray.Dispose();
			output.UnlockBits(bm);
            return output;
        }

		public unsafe static void SetBitmapPixel(System.Drawing.Imaging.BitmapData bm, int pixel, Color clr)
		{
			*(byte*)(bm.Scan0 + pixel * 4 + 0) = clr.B;
			*(byte*)(bm.Scan0 + pixel * 4 + 1) = clr.G;
			*(byte*)(bm.Scan0 + pixel * 4 + 2) = clr.R;
			*(byte*)(bm.Scan0 + pixel * 4 + 3) = clr.A;
		}
    }
}
