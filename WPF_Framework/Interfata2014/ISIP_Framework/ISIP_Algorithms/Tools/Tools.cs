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
        //255 inseamna alb
        //0 inseamna negru
        public static Image<Gray, byte> Invert(Image<Gray, byte> InputImage)
        {
            /*
             * ia fiecare pixel al imaginii
             * inverseaza culorile intre alb si negru
             */
            Image<Gray, byte> Result = new Image<Gray, byte>(InputImage.Size);
            for (int y = 0; y < InputImage.Height; y++)
            {
                for (int x = 0; x < InputImage.Width; x++)
                {
                    //fiecare pixel primeste culoarea inversa
                    Result.Data[y, x, 0] = (byte)(255 - InputImage.Data[y, x, 0]);
                }
            }
            return Result;
        }
        public static List<double> GammaLUT(double gamma)
        {
            //o lista in care imi pun valorile operatorului gamma
            double a = 255 / Math.Pow(255, gamma);
            // lista efectiva
            List<double> LUT = new List<double>();
            for (int i = 0; i < 256; i++)
            {
                //formula din curs
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
                    // iau fiecare pixel si il inlocuiesc cu valoarea din lista
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
            //parcurg dimensiunea mastii
            for (int i = 0; i < mask * mask - 1; i++)
            {
                int x1 = i / mask;
                int y1 = i % mask;
                pixels.Add(InputImage.Data[y1 + y, x1 + x, 0]);

            }

            pixels.Sort();
            int value = pixels.ElementAt(pixels.Count() / 2);
            // mediana basic
            return value;
        }
        public static Image<Gray, byte> MedianFilter(Image<Gray, byte> InputImage, int mask)
        {
            Image<Gray, byte> Result = new Image<Gray, byte>(InputImage.Size);
            //parcurg imaginea
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
        public static double[,] Gauss_m(double sigma)
        {

            int dim = (int)(4 * sigma) + 1;

            if (dim % 2 == 0)
            {
                dim++;
            }

            double[,] mat = new double[dim, dim];

            double sum = 0;

            int interval = (int)(sigma * 2);

            for (int y = -interval; y <= interval; y++)
            {
                for (int x = -interval; x <= interval; x++)
                {
                    //HD formula din curs
                    mat[y + interval, x + interval] = 1d / (2 * Math.PI * sigma * sigma) * Math.Exp(-((y * y) + (x * x)) / (2 * sigma * sigma));

                    sum += mat[y + interval, x + interval];
                }
            }

            for (int y = 0; y < dim; y++)
            {
                for (int x = 0; x < dim; x++)
                {
                    // inmultesc cu fiecare element din masca
                    mat[y, x] = mat[y, x] * 1d / sum;
                }
            }
            return mat;
        }
       
            
        public static Image<Gray, byte> Filtrare_bilaterala(Image<Gray, byte> InputImage, double sigmaD, double sigmaR)
        {
            Image<Gray, byte> Result = InputImage.Clone();
            int width = InputImage.Width;
            int height = InputImage.Height;
            //iau masca 
            double[,] mask = Gauss_m(sigmaD);

            int interval = (mask.GetLength(0) - 1) / 2;
            //iau intervalul
            int dim = mask.GetLength(0);


            for (int y = interval; y < height - interval; y++)
            {
                for (int x = interval; x < width - interval; x++)
                {
                    double sum1 = 0.0;
                    double sum2 = 0.0;
                    for (int i = -interval; i < interval; i++)
                    {
                        for (int j = -interval; j < interval; j++)
                        {
                            //prima parte a sumei
                            sum1 += (InputImage.Data[y + i, x + j, 0] * mask[i + interval, j + interval]) * Hr(InputImage.Data[y + i, x + j, 0] - InputImage.Data[y, x, 0], sigmaR);
                            // a doua
                            //ambele dupa formula
                            sum2 += (mask[i + interval, j + interval]) * Hr(InputImage.Data[y + i, x + j, 0] - InputImage.Data[y, x, 0], sigmaR);

                            

                        }
                    }

                    //sum1/suma2
                   double var = sum1 / sum2;
                  
                    Result.Data[y, x, 0] = (byte)(var + 0.5);
                }
            }
            return Result;
        }
        public static double Hr(double diff, double sigmaR)
        {
            double e = Math.E;
            return Math.Pow(e, (-(diff * diff) / 2 * (sigmaR * sigmaR)));
        }


        public static Image<Gray,byte>Sobel(Image<Gray, byte> InputImage, int prag)
        {
            Image<Gray, byte> ResultImage = InputImage.Clone();
            //masca pt x
            double[,] Sx = new double[3, 3] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            // masca pt x
            double[,] Sy = new double[3, 3] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
            // masca pt y
           double Fy;
           double Fx;
           
          

            for(int y=1;y<InputImage.Height;y++)
            {
                for(int x=1;x<InputImage.Width;x++)
                {
                    Fx = 0;
                    Fy = 0;
                    for(int i=0;i<3;i++)
                        for(int j=0;j<3; j++)
                        {
                            Fx = Fx + InputImage.Data[y - 1, x - 1, 0] * Sx[i, j];
                            Fy = Fy + InputImage.Data[y - 1, x - 1, 0] * Sy[i, j];

                        }
                    double grad = Math.Sqrt(Fx * Fx + Fy * Fy);
                    if (grad < prag)
                    {
                        ResultImage.Data[y, x, 0] = 0;
                    }
                    else {
                        ResultImage.Data[y, x, 0] = 255;


                    }

                }
            }
            return ResultImage;


        }
        public static Image<Gray, byte> SobelVertical(Image<Gray, byte> InputImage, double t)
        {
            Image<Gray, byte> Result = new Image<Gray, byte>(InputImage.Size);
            // doua 
            double[,] Sx = new double[3, 3] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            // masca pt x
            double[,] Sy = new double[3, 3] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
            // masca pt y
            //sy e transpusa lui sy
            const double rad = 180 / Math.PI;
            //conversia la radiani
            for (int y = 1; y < InputImage.Height - 1; y++)
            {
                for (int x = 1; x < InputImage.Width - 1; x++)
                {
                    double Fx = 0;
                    double Fy = 0;

                    // parcurg vecinatatea pixelului cu mastile sx si sy
                    for (int i = 0; i <= 2; i++)
                    {
                        for (int j = 0; j <= 2; j++)
                        {
                            // fuctiile rezultat 
                            // inmultesc o masca cu imaginea
                            // in f-uri fac suma la inmutirile astea 
                            // vezi foile numerotate, cred ca foaia 8
                            Fx += InputImage.Data[y + i - 1, x + j - 1, 0] * Sx[i, j];
                            Fy += InputImage.Data[y + i - 1, x + j - 1, 0] * Sy[i, j];
                        }
                    }

                    double grad = Math.Sqrt(Fx * Fx + Fy * Fy);
                    //formula  , vezi curs
                    double theta = 0;

                    // compar cu un treshold initial
                    if (grad < t)
                    {
                        Result.Data[y, x, 0] = (byte)(0);
                        // devine negru
                    }
                    else
                    {
                        theta = Math.Atan2(Fy, Fx);
                        // arctangenta dintre cele 2 puncte
                        double grade = theta * rad;
                        // ca sa verific cat de mare e gradul, convertesc la grade

                        if (grade >= -180 && grade <= -175)
                        {
                            Result.Data[y, x, 0] = (byte)(255);
                            // devine alb
                        }
                        else if (grade >= -5 && grade <= 5)
                        {
                            Result.Data[y, x, 0] = (byte)(255);

                        }
                        else if (grade >= 175 && grade <= 180)
                        {
                            Result.Data[y, x, 0] = (byte)(255);
                        }
                        else
                        {
                            Result.Data[y, x, 0] = (byte)(0);
                            //devine negru
                        }
                    }
                }
            }

            return Result;
        }
        public static Image<Gray, byte> SobelOrizontal(Image<Gray, byte> InputImage, double t)
        {
            Image<Gray, byte> Result = new Image<Gray, byte>(InputImage.Size);
            // doua masti sobel
            double[,] Sx = new double[3, 3] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            // masca pt x
            double[,] Sy = new double[3, 3] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
            // masca pt y
            //sy e transpusa lui sy
            const double rad = 180 / Math.PI;
            //conversia la radiani
            for (int y = 1; y < InputImage.Height - 1; y++)
            {
                for (int x = 1; x < InputImage.Width - 1; x++)
                {
                    double Fx = 0;
                    double Fy = 0;

                    // parcurg vecinatatea pixelului 
                    for (int i = 0; i <= 2; i++)
                    {
                        for (int j = 0; j <= 2; j++)
                        {
                            // fuctiile rezultat 
                            // inmultesc o masca cu imaginea
                            // in f-uri fac suma la inmutirile astea 
                            // vezi foile numerotate, cred ca foaia 8
                            Fx += InputImage.Data[y + i - 1, x + j - 1, 0] * Sx[i, j];
                            Fy += InputImage.Data[y + i - 1, x + j - 1, 0] * Sy[i, j];
                        }
                    }

                    double grad = Math.Sqrt(Fx * Fx + Fy * Fy);
                    //formula  , vezi curs
                    double theta = 0;

                    // compar cu un treshold initial
                    if (grad < t)
                    {
                        Result.Data[y, x, 0] = (byte)(0);
                        // devine negru
                    }
                    else
                    {
                        theta = Math.Atan2(Fy, Fx);
                        // arctangenta dintre cele 2 puncte
                        double grade = theta * rad;
                        // ca sa verific cat de mare e gradul, convertesc la grade

                        if (grade >= -95 && grade <= -85)
                        {
                            Result.Data[y, x, 0] = (byte)(255);
                            // devine alb
                        }
                        else if (grade >= 85 && grade <= 95)
                        {
                            Result.Data[y, x, 0] = (byte)(255);
                        }
                        else
                        {
                            Result.Data[y, x, 0] = (byte)(0);
                            //devine negru
                        }
                    }
                }
            }

            return Result;
        }

        public static Image<Gray, byte> BinarizareSimpla(Image<Gray, byte> InputImage, double threshold)
        {
            // cand converstesc toate culorile la alb si negru
            //compar culoarea cu un anume prag
            Image<Gray, byte> Result = InputImage.Clone();
            //pare ca iau fiecare pixel din imagine
            for (int y = 0; y < InputImage.Height; y++)
            {
                for (int x = 0; x < InputImage.Width; x++)
                {
                    if ((int)Result.Data[y, x, 0] <= threshold)
                        Result.Data[y, x, 0] = (Byte)0;
                    // sub prag devine negru
                    else
                        Result.Data[y, x, 0] = (Byte)255;
                    //peste prag devine alb
                }
            }
            return Result;
        }
        public static Image<Gray, byte> Xor(double thresold, Image<Gray, byte> originalImage)
        {
            // aplic operatia matematica xor
            //a==1&&b==0 devine 1
            // a==0 &&b==1, devine 1
            //a==1&&b==1, devine 0
            //a==0&&b==0 ,devine 0
            // de notat restu ca sa inteleg

            Image<Gray, byte> resultImage1 = new Image<Gray, byte>(originalImage.Size);
            //pe asta binarizez 
            Image<Gray, byte> resultImageXor = new Image<Gray, byte>(resultImage1.Size);

            Image<Gray, byte> resultImage2 = new Image<Gray, byte>(resultImage1.Data);

            resultImage1 = BinarizareSimpla( originalImage, thresold);
            //pun o imagine binarizata simplu in resultimage 1
            //dilatare
            // iau o matrice de 3x3 pe care ma plimb pe toata imaginea
            //in jurul pixelului care ma intereseaza, daca gasesc un pixel colorat atunci fac ,il igros 
            for (int y = 1; y < resultImage1.Height - 1; y++)
            {
                for (int x = 1; x < resultImage1.Width - 1; x++)
                {
                    
                    // toata vecinatatea pixelului de interes
                    if (resultImage1.Data[y - 1, x - 1, 0] == 255 || resultImage1.Data[y - 1, x, 0] == 255 || resultImage1.Data[y - 1, x + 1, 0] == 255 ||
                        resultImage1.Data[y, x - 1, 0] == 255 || resultImage1.Data[y, x, 0] == 255 || resultImage1.Data[y, x + 1, 0] == 255 ||
                        resultImage1.Data[y + 1, x - 1, 0] == 255 || resultImage1.Data[y + 1, x, 0] == 255 || resultImage1.Data[y + 1, x + 1, 0] == 255)
                    {
                        resultImage2.Data[y, x, 0] = 255;
                        //aici colorez pixelul, il fac alb
                    }
                }
            }

            // xor
            // folosind imaginea dilatata si cea binarizata eu aplic regula matematica
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

            int yO = sourceHeight / 2;
            int xO = sourceWidth / 2;
            //sa aflu centrul imaginii,dupa el rotesc

            double radians = degrees * Math.PI / 180f;
            //conversia la radiani

            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);
            //partea de mate

            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {

                    double xC = (x - xO) * cos - (y - yO) * sin + xO;
                    double yC = (x - xO) * sin+ (y - yO) * cos + yO;
                    //formulele din explicatia de laborator/vezi foile numerotate
                    int y0 = (int)yC;
                    int x0 = (int)xC;
                    //convertesc la int

                    if (yC >= 0 && yC < result.Height - 1 &&
                        xC >= 0 && xC < result.Width - 1
                     )
                    {
                        double val1 = (inputImage.Data[y0, x0 + 1, 0] - inputImage.Data[y0, x0, 0]) * (xC - x0) + inputImage.Data[y0, x0, 0];

                        double val2 = (inputImage.Data[y0 + 1, x0 + 1, 0] - inputImage.Data[y0 + 1, x0, 0]) * (xC - x0) + inputImage.Data[y0 + 1, x0, 0];

                        byte val3 = (byte)((val2 - val1) * (yC - y0) + val1);

                        result.Data[y, x, 0] = val3;
                       
                    }
                    else
                    {
                        result.Data[y, x, 0] = 0;
                    }
                }
            }

            return result;
        }

    }

}


