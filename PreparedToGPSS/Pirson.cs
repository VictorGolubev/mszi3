using System;
using System.Collections.Generic;
using System.Text;

namespace PreparedToGPSS
{
    public class Pirson
    {
        public  static Dictionary<string,string> getParams(List<double> vector, double sum)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            double MX = getMX(vector);//Мат ожидание
            double DX = getDX(vector, MX); //Дисперсия

            //Параметры нормального для экспоненциального распределения
            double u = 1 / MX; //Плотность
            //Параметры распределение для экспоненциального распределения
            double a = MX; //Мат ожидание
            double b = Math.Sqrt(DX); //среднеквадратичное отклонение распределения

            double HiNorm = getHiNormalDistibution(vector, sum, a, b);
            double HiExp = getHiExpDistibution(vector, sum, u);
            if (HiNorm < HiExp)
                result.Add("Norm", "a = " + a + " b = " + b);
            else
                result.Add("Exp", "u = " + u);
            
            return result;
        }
        private static double getHiNormalDistibution(List<double> vector, double sum, double a, double b)
        {
            List<double> distribution = getNormalDistribution(vector, sum, a, b);
            double H = 0;
            for (int i = 0; i < distribution.Count; i++)
            {
                double temp;
                temp = (distribution[i] - vector[i]) * (distribution[i] - vector[i]) / distribution[i]; // вес i-го разряда на меру отклонения Пирсонаа (Отклонение от ожидаемого в квадрате)
                H += temp;
            }
            return H;
        }
        private static double getHiExpDistibution(List<double> vector, double sum, double u)
        {
            List<double> distribution = getExpDistribution(vector, sum, u);
            double H = 0;
            for (int i = 0; i < distribution.Count; i++)
            {
                double temp;
                temp = (distribution[i] - vector[i]) * (distribution[i] - vector[i]) / distribution[i]; // вес i-го разряда на меру отклонения Пирсонаа (Отклонение от ожидаемого в квадрате)
                H += temp;
            }
            return H;
        }

        private static List<double> getNormalDistribution(List<double> vector, double sum, double a, double b)
        {

            //Результат
            List<double> result = new List<double>();

            foreach (int temp in vector)
                result.Add(normalD(temp, a, b) * sum);

            return result;



        }

        private static List<double> getExpDistribution(List<double> vector, double sum, double u)
        {

            //Результат
            List<double> result = new List<double>();

            foreach (int temp in vector)
                result.Add(expD(temp, u) * sum);

            return result;



        }

        private static double getMX(List<double> vector)
        {
            double sum = 0;
            for (int i = 0; i < vector.Count; i++)
                sum += vector[i];
            return sum / vector.Count;
        }

        private static double getDX(List<double> vector, double MX)
        {
            double sum = 0;
            for (int i = 0; i < vector.Count; i++)
                sum += Math.Pow(vector[i] - MX, 2);
            return sum / vector.Count;
        }

        //Расчет по формуле нормального распределения
        private static double normalD(double X, double a, double b)
        {
            double res = 1 / (b * Math.Sqrt(2 * Math.PI));
            double power = (-1) * (X - a) * (X - a) / (2 * b * b);
            return res * Math.Exp(power);
        }

        private static double expD(double x, double u)
        {
            if (x >= 0)
                return u * Math.Exp((-1) * u * x);
            else
                return 0;
        }

    }
}
