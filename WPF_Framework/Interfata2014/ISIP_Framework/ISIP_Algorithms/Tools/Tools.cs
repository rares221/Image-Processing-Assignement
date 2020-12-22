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
            for (int i = 0; i < 256; i++)
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
                    Result.Data[y, x, 0] = (byte)(Lookup.ElementAt(InputImage.Data[y, x, 0]));
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
        private static int Median(int y, int x, Image<Gray, byte> InputImage, int mask)
        {
            List<int> pixels = new List<int>();

            for (int i = 0; i < mask * mask - 1; i++)
            {
                int x1 = i / mask;
                int y1 = i % mask;
                pixels.Add(InputImage.Data[y1 + y, x1 + x, 0]);

            }

            pixels.Sort();
            int value = pixels.ElementAt(pixels.Count() / 2);
            return value;
        }
        public static Image<Gray, byte> MedianFilter(Image<Gray, byte> InputImage, int mask)
        {
            Image<Gray, byte> Result = new Image<Gray, byte>(InputImage.Size);
            for (int y = 0; y < InputImage.Height; y++)
            {
                for (int x = 0; x < InputImage.Width; x++)
                {
                    if (y >= InputImage.Height - mask - 1 || x >= InputImage.Width - mask - 1 || x < mask || y < mask)
                        Result.Data[y, x, 0] = (byte)InputImage.Data[y, x, 0];
                    else
                        Result.Data[y, x, 0] = (byte)Median(y, x, InputImage, mask);
                }
            }
            return Result;
        }
        public static Image<Gray, byte> BilateralFilter(Image<Gray, byte> InputImage, double sigmaD,double sigmaR)
        {
            Image<Gray, byte> Result = new Image<Gray, byte>(InputImage.Size);
            int dim;
            double pi = Math.PI;
            double e = Math.E;
            dim = (int)(sigmaD * 4) + 1;

            if (dim % 2 == 0)
            {
                dim++;
            }

            double[,] mat = new double[dim, dim];
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    mat[i, j] = (1 / (2 * pi * Math.Pow(sigmaD, 2)) * Math.Pow(e, -((i - dim / 2) * (i - dim / 2) + (j - dim / 2) * (j - dim / 2)) / (2 * Math.Pow(sigmaD, 2))));
                }
            }

            double c = 0;
            double sum = 0;
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    sum += mat[i, j];
                }
            }
            c = 1 / sum;
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    mat[i, j] = mat[i, j] * c;
                }
            }

            for (int y = dim / 2; y < InputImage.Height - (dim / 2); y++)
            {
                for (int x = dim / 2; x < InputImage.Width - (dim / 2); x++)
                {
                    double sum1 = 0;
                    double sum2 = 0;
                    double  diff1 = 0;
                    for (int i = 0; i < dim; i++)
                    {
                        for (int j = 0; j < dim; j++)
                        {
                            diff1 = mat[x, y] - mat[x + i, y + j];
                            sum1 += InputImage.Data[y + i - dim / 2, x + j - dim / 2, 0] * mat[i, j]*Hr(diff1,sigmaR);
                            sum2=mat[i, j] * Hr(diff1, sigmaR);
                        }
                    }
                    Result.Data[y, x, 0] = (byte)((sum1/sum2) + 0.5);
                }
            }
            return Result;
        }
        public static double Hr( double diff, double sigmaR)
        {

            double e = Math.E;
            return Math.Pow(e, (-(diff * diff) / 2 * (sigmaR * sigmaR)));


        }
     
        public static Image<Gray, byte> SobelDirectional(Image<Gray, byte> InputImage, int t)
        {
            Image<Gray, byte> Result = InputImage.Clone();
            int[,] Sx = new int[3, 3];
            int[,] Sy = new int[3, 3];

            int[] numbers = new int[] { 1, 2, 1 };

            int fx, fy;
            int grad = 0;
            int teta = 0;
            int partResult = 0;

            for (int i = 0; i < 3; i++)
            {
                Sx[i, 0] = -numbers[i];
                Sx[i, 2] = numbers[i];
                Sy[0, i] = -numbers[i];
                Sy[2, i] = numbers[i];
            }

            
            for (int y = 1; y < InputImage.Height - 1; y++)
            {
                for (int x = 1; x < InputImage.Width - 1; x++)
                {
                    fx = 0;
                    fy = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            fx += (InputImage.Data[y - 1 + i, x - 1 + j, 0] * Sx[i, j]);
                            fy += (InputImage.Data[y - 1 + i, x - 1 + j, 0] * Sy[i, j]);
                        }
                    }
                    grad = (int)Math.Sqrt(fx * fx + fy * fy);
                    if (grad < t)
                    {
                        Result.Data[y, x, 0] = 0;
                    }
                    else
                    {
                        teta = (int)(Math.Atan2(fx, fy) * (180 / Math.PI));
                        partResult = (((teta + 180) * (255 - 127)) / 360) + 127;
                        Result.Data[y, x, 0] = (byte)partResult;
                    }
                }
            }

            return Result;
        }
        public static Image<Gray, byte> BinarizareSimpla(Image<Gray, byte> InputImage, double threshold)
        {
            Image<Gray, byte> Result = InputImage.Clone();
            for (int y = 0; y < InputImage.Height; y++)
            {
                for (int x = 0; x < InputImage.Width; x++)
                {
                    if ((int)Result.Data[y, x, 0] <= threshold)
                        Result.Data[y, x, 0] = (Byte)0;
                    else
                        Result.Data[y, x, 0] = (Byte)255;
                }
            }
            return Result;
        }
        public static Image<Gray, byte> Xor(double thresold, Image<Gray, byte> originalImage)
        {
            Image<Gray, byte> resultImage1 = new Image<Gray, byte>(originalImage.Size);
            Image<Gray, byte> resultImageXor = new Image<Gray, byte>(resultImage1.Size);
            Image<Gray, byte> resultImage2 = new Image<Gray, byte>(resultImage1.Data);

            resultImage1 = BinarizareSimpla( originalImage, thresold);

            
            // partea de erodare

            for (int y = 1; y < resultImage1.Height - 1; y++)
            {
                for (int x = 1; x < resultImage1.Width - 1; x++)
                {
                    if (resultImage1.Data[y, x, 0] == 255 && resultImage1.Data[y, x - 1, 0] == 0)
                    {
                        resultImage2.Data[y, x, 0] = 0;
                    }
                    else
                         if (resultImage1.Data[y, x, 0] == 255 && resultImage1.Data[y - 1, x - 1, 0] == 0)
                         {
                        resultImage2.Data[y, x, 0] = 0;
                         }
                    else
                         if (resultImage1.Data[y, x, 0] == 255 && resultImage1.Data[y - 1, x, 0] == 0)
                         {
                        resultImage2.Data[y, x, 0] = 0;
                         }
                    else
                         if (resultImage1.Data[y, x, 0] == 255 && resultImage1.Data[y + 1, x + 1, 0] == 0)
                         {
                        resultImage2.Data[y, x, 0] = 0;
                         }
                }
            }

            // xor
            for (int y = 0; y < resultImage1.Height; y++)
            {
                for (int x = 0; x < resultImage1.Width; x++)
                {
                    if (resultImage1.Data[y, x, 0] == 0 && resultImage2.Data[y, x, 0] == 0)
                    {
                        resultImageXor.Data[y, x, 0] = 0;
                    }
                    else if (resultImage1.Data[y, x, 0] == 0 && resultImage2.Data[y, x, 0] == 255)
                    {
                        resultImageXor.Data[y, x, 0] = 255;
                    }
                    else if (resultImage1.Data[y, x, 0] == 255 && resultImage2.Data[y, x, 0] == 0)
                    {
                        resultImageXor.Data[y, x, 0] = 255;
                    }
                    else
                    {
                        resultImageXor.Data[y, x, 0] = 0;
                    }
                }
            }

            return resultImageXor;

        }
        public static Image<Gray, byte> BilinearRotation(Image<Gray, byte> inputImage, double degrees)
        {
            int sourceHeight = inputImage.Height;
            int sourceWidth = inputImage.Width;

            int y0 = sourceHeight / 2;
            int x0 = sourceWidth / 2;

            double radians = degrees * Math.PI / 180f;

            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {

                    double xC = (x - x0) * cos - (y - y0) * sin + x0;
                    double yC = (x - x0) * sin + (y - y0) * cos + y0;

                    if (yC >= 0 && yC < result.Height - 1 &&
                        xC >= 0 && xC < result.Width - 1
                     )
                    {
                        result.Data[(int)yC, (int)xC, 0] = inputImage.Data[y, x, 0];
                    }
                }
            }

            return result;
        }

    }

}


