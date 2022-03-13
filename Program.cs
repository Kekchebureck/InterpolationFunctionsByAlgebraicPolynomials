﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using org.mariuszgromada.math.mxparser;

namespace _1._1laba
{
    internal class Program
    {
        private struct ToSave
        {
            public double[] X { get; set; }
            public double[] Y { get; set; }
            public double[] InterpolationValues { get; set; }
            public double[] XForDrawing { get; set; }
        };

        static void Main()
        {
            int n;
            double a, b;
            int degree;
            Console.Write("Введите n: ");
            n = int.Parse(Console.ReadLine());
            Console.Write("Введите a: ");
            a = double.Parse(Console.ReadLine());
            Console.Write("Введите b: ");
            b = double.Parse(Console.ReadLine());

            double[,] matrix = generate(n + 1, a, b);

            Console.WriteLine("Исходная табличная функция");
            print(matrix);

            Console.Write("Введите степень многочлена: ");
            degree = int.Parse(Console.ReadLine());

            double[] coefficients = calculationCoefficients(matrix, n + 1);

            double[] interpolationValues = new double[n + 1];
            for (int i = 0; i < n + 1; ++i)
            {
                interpolationValues[i] = p(matrix, degree + 1, coefficients, matrix[0, i]);
            }

            double maxError = countError(matrix, n + 1, coefficients, a, b, degree);

            Console.WriteLine($"Погрешность = {maxError}");

            double[] interpolationValuesForDrawing = createInterpolationValuesForDrawing(matrix, n + 1, coefficients, a, b, degree, out double[] xForDrawing);

            drawingGraph(matrix, interpolationValuesForDrawing, xForDrawing, n + 1);  

            Console.Read();
        }

        static double[] createInterpolationValuesForDrawing(double[,] matrix, int size, double[] coefficients, double a, double b, int degree, out double[] xForDrawing)
        {
            double[] res = new double[size * 3];
            xForDrawing = new double[size * 3];
            double step = (b - a) / (size * 3 - 1);
            for (int i = 0; i < size * 3; ++i)
            {
                xForDrawing[i] = a + i * step;
                res[i] = p(matrix, degree, coefficients, xForDrawing[i]);
            }
            return res;
        }

        static double[] createErrorValues(double[,] matrix, int size, double[] coefficients, double a, double b, int degree, out double[] xForDrawing)
        {
            double[] res = new double[size * 3];
            xForDrawing = new double[size * 3];
            double step = (b - a) / (size * 3 - 1);
            for (int i = 0; i < size * 3; ++i)
            {
                xForDrawing[i] = a + i * step;
                res[i] = Math.Abs(p(matrix, degree, coefficients, xForDrawing[i]) - f(xForDrawing[i]));
            }
            return res;
        }

        static double countError(double[,] matrix, int size, double[] coefficients, double a, double b, int degree)
        {
            double res = 0;
            double step = (b - a) / (size * 3 - 1);
            for (int i = 0; i < size * 3; ++i)
            {
                double x = a + i * step;
                double temp = Math.Abs(p(matrix, degree, coefficients, x) - f(x));
                if (temp > res)
                {
                    res = temp;
                }
            }
            return res;
        }

        static void drawingGraph(double[,] matrix, double[] interpolationValues, double[] xForDrawing, int size)
        {
            try
            {
                ToSave data = new ToSave();
                double[] x = new double[size];
                double[] y = new double[size];
                for (int i = 0; i < size; ++i)
                {
                    x[i] = matrix[0, i];
                    y[i] = matrix[1, i];
                }
                data.X = x;
                data.Y = y;
                data.InterpolationValues = interpolationValues;
                data.XForDrawing = xForDrawing;

                string json = JsonSerializer.Serialize(data);
                File.WriteAllText(@"D:\лабы\6 семестр\ЧМ\1.1laba\Save\temp.json", json);

                Process p = Process.Start(@"D:\лабы\6 семестр\ЧМ\1.1laba\visualizationGraphs\visualizationGraphs.py");
                p.WaitForExit();
            }
            catch (Win32Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static double[,] generate(int n, double a, double b)
        {
            double[,] result = new double[2, n];

            //uniformPartitioning(result, a, b, n);
            ChebyshevPartition(result, a, b, n);

            return result;
        }

        private static void ChebyshevPartition(double[,] matrix, double a, double b, int size)
        {
            for (int i = 0; i < size; ++i)
            {
                matrix[0, i] = (a + b) / 2 - (b - a) * Math.Cos((2 * i + 1) * Math.PI / (2 * size + 2)) / 2;
                matrix[1, i] = f(matrix[0, i]);
            }
        }

        private static void uniformPartitioning(double[,] matrix, double a, double b, int size)
        {
            for (int i = 0; i < size; ++i)
            {
                matrix[0, i] = a + (b - a) * i / size;
                matrix[1, i] = f(matrix[0, i]);
            }
        }

        private static double f(double x)
        {
            return 1.2 * x * x * x - 1;
        }

        private static double p(double[,] matrix, int size, double[] coefficients, double x)
        {
            double result = coefficients[0];
            for (int i = 1; i < size; ++i)
            {
                double temp = coefficients[i];
                for(int j = 0; j < i; ++j)
                {
                    temp *= x - matrix[0, j];
                }
                result += temp;
            }
            return result;
        }

        private static string p(double[,] matrix, int size, double[] coefficients)
        {
            string result = "p(x) = " + coefficients[0].ToString();
            for (int i = 1; i < size; ++i)
            {
                if (coefficients[i] >= 0)
                {
                    result += "+";
                }

                result += coefficients[i].ToString();
                for (int j = 0; j < i; ++j)
                {
                    result += "*(x";
                    result += matrix[0, j] < 0 ? "+" : "-";
                    result += Math.Abs(matrix[0, j]).ToString() + ")";
                }
            }
            return result;
        }

        private static void print(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            int m = matrix.GetLength(1);
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < m; ++j)
                {
                    Console.Write(Math.Round(matrix[i, j], 3) + " ");
                }
                Console.WriteLine();
            }
        }

        private static double[] calculationCoefficients(double[,] matrix, int size)
        {
            double[] coefficients = new double[size];

            double[,] tempMatrix = new double[size * 2 - 1, size];
            double[] tempX = new double[size * 2 - 1];
            for (int i = 0, j = 0; i < size * 2 - 1 && j < size; i += 2, ++j)
            {
                tempMatrix[i, 0] = matrix[1, j];
                tempX[i] = matrix[0, j];
            }

            int indent = 1;
            while (indent <= size - 1)
            {
                for (int i = indent; i < (size * 2 - 1) - indent; i += 2) 
                {
                    tempMatrix[i, indent] = (tempMatrix[i + 1, indent - 1] - tempMatrix[i - 1, indent - 1]) / (tempX[i + indent] - tempX[i - indent]);
                }
                indent++;
            }

            for (int i = 0; i < size; i++)
            {
                coefficients[i] = tempMatrix[i, i];
            }

            return coefficients;
        }
    }
}
