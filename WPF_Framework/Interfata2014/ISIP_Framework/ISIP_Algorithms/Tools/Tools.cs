﻿using System;
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
            for (int y = dim / 2; y < InputImage.Height - dim / 2; y++)
            {
                for (int x = dim / 2; x < InputImage.Width - dim / 2; x++)
                {

                    int x0 = x - dim / 2;
                    int y0 = y - dim / 2;
                    int x1 = x + dim / 2;
                    int y1 = y + dim / 2;

                    double sum;
                    double mu;

                    if (x0 == 0 && y0 == 0)
                    {
                        sum = imagineIntegrala[y1, x1];
                        mu = sum / (dim * dim);

                    }
                    else if (y0 == 0)
                    {
                        sum = imagineIntegrala[y1, x1] -
                              imagineIntegrala[y1, x0 - 1];
                        mu = sum / (dim * dim);

                    }
                    else if (x0 == 0)
                    {
                        sum = imagineIntegrala[y1, x1] -
                              imagineIntegrala[y0 - 1, x1];
                        mu = sum / (dim * dim);

                    }
                    else
                    {
                        sum = imagineIntegrala[y1, x1] +
                              imagineIntegrala[y0 - 1, x0 - 1] -
                              imagineIntegrala[y1, x0 - 1] -
                              imagineIntegrala[y0 - 1, x1];
                        mu = sum / (dim * dim);

                    }


                    if (InputImage.Data[y, x, 0] < b * mu)
                    {
                        Result.Data[y, x, 0] = 0;
                    }
                    else
                    {
                        Result.Data[y, x, 0] = 255;
                    }

                }
            }

            return Result;

        }







    }
}
