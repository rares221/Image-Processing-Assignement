using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using Emgu.CV;
using Emgu.CV.Structure;
namespace ISIP_Algorithms.Tools
{
    public class Tools
    {
        public static Image<Gray, byte> Invert(Image<Gray, byte> InputImage)
        {
            Image<Gray, byte> Result = new Image<Gray, byte>(InputImage.Size);
            for (int y = 0; y < InputImage.Height; y++)
            {
                for (int x = 0; x < InputImage.Width; x++)
                {
                    Result.Data[y, x, 0] = (byte)(255 - InputImage.Data[y, x, 0]);
                }
            }
            return Result;
        }
        public static List<byte> GammaLUT(double gamma)
        {
            double a = 255 / Math.Pow(255, gamma);
            List<byte> LUT = new List<byte>();
            for(int i=0;i<256;i++)
            {
                double f = (byte)(a * Math.Pow(i, gamma));
                LUT.Add((byte)f);
            }
            return LUT;
        }
        public static Image<Gray, byte> GammaCorrection(Image<Gray, byte> InputImage, double gamma)
        {
            Image<Gray, byte> Result = new Image<Gray, byte>(InputImage.Size);
            List<byte> Lookup = GammaLUT(gamma);

            for (int y = 0; y < InputImage.Height; y++)
            {
                for (int x = 0; x < InputImage.Width; x++)
                {
                    Result.Data[y, x, 0] = (byte)(Lookup.ElementAt (InputImage.Data[y, x, 0]));
                }
            }
            return Result;
        }







    }
}
