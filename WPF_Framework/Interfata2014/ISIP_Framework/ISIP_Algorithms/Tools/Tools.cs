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
        public static List<double> GammaLUT(double gamma)
        {
            double a = 255 / Math.Pow(255, gamma);
            List<double> LUT = new List<double>();
            for(int i=0;i<256;i++)
            {
                double f = (byte)(a * Math.Pow(i, gamma));
                LUT.Add((double)f);
               
            }
            return LUT;
        }
        public static Image<Gray, byte> GammaCorrection(Image<Gray, byte> InputImage, double gamma)
        {
            Image<Gray, byte> Result = new Image<Gray, byte>(InputImage.Size);
            List<double> Lookup = GammaLUT(gamma);

            for (int y = 0; y < InputImage.Height; y++)
            {
                for (int x = 0; x < InputImage.Width; x++)
                {
                    Result.Data[y, x, 0] = (byte)(Lookup.ElementAt (InputImage.Data[y, x, 0]));
                }
            }
            return Result;
        }
        public static Image<Gray, byte> AdaptativBinarization(Image<Gray, byte> InputImage, int dim, float b)
        {
            Image<Gray, byte> Result = new Image<Gray, byte>(InputImage.Size);
            int[,] imagineIntegrala = new int[InputImage.Height, InputImage.Width];
            double[,] T = new double[InputImage.Height, InputImage.Width];

            for (int y = 0; y < InputImage.Height; y++)
            {
                for (int x = 0; x < InputImage.Width; x++)
                {
                    if (y == 0 && x == 0)
                        imagineIntegrala[y, x] = InputImage.Data[y, x, 0];
                    else if (y == 0)
                        imagineIntegrala[y, x] = InputImage.Data[y, x, 0] + imagineIntegrala[y, x - 1];
                    else if (x == 0)
                        imagineIntegrala[y, x] = imagineIntegrala[y - 1, x] + InputImage.Data[y, x, 0];
                    else imagineIntegrala[y, x] = imagineIntegrala[y - 1, x] + imagineIntegrala[y, x - 1] - imagineIntegrala[y - 1, x - 1] + InputImage.Data[y, x, 0];

                }
            }
            float sum;
            for (int y = 0; y < InputImage.Height - dim; y++)
            {
                for (int x = 0; x < InputImage.Width - dim; x++)
                {
                    int x1 = x + dim - 1;
                    int y1 = y + dim - 1;
                    if (y == 0 && x == 0)
                    {
                        sum = imagineIntegrala[y, x];
                        T[y1, x1] = b * (sum / (dim * dim));
                        if (InputImage.Data[y1, x1, 0] < T[y1, x1])
                            Result.Data[y1, x1, 0] = 0;
                        else Result.Data[y1, x1, 0] = 255;
                    }
                    else if (x == 0)
                    {
                        sum = imagineIntegrala[y1, x1] - imagineIntegrala[y - 1, x1];
                        T[y1, x1] = b * (sum / (dim * dim));
                        if (InputImage.Data[y1, x1, 0] < T[y1, x1])
                            Result.Data[y1, x1, 0] = 0;
                        else Result.Data[y1, x1, 0] = 255;

                    }
                    else if (y == 0)
                    {
                        sum = imagineIntegrala[y1, x1] - imagineIntegrala[y1, x - 1];
                        T[y1, x1] = b * (sum / (dim * dim));
                        if (InputImage.Data[y1, x1, 0] < T[y1, x1])
                            Result.Data[y1, x1, 0] = 0;
                        else Result.Data[y1, x1, 0] = 255;
                    }
                    else
                    {
                        sum = imagineIntegrala[y1, x1] + imagineIntegrala[y - 1, x - 1] - imagineIntegrala[y - 1, x1] - imagineIntegrala[y1, x - 1];
                        T[y1, x1] = b * (sum / (dim * dim));
                        if (InputImage.Data[y1, x1, 0] < T[y1, x1])
                            Result.Data[y1, x1, 0] = 0;
                        else Result.Data[y1, x1, 0] = 255;
                    }

                }
            }

            return Result;
        }







    }
}
