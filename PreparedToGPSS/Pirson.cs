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

            Dictionary<string, int> ranges = getFiveRanges(vector);
            double MX = getMXNormal(ranges, sum);
            double DX = getDX(ranges, MX);
            double zNorm = getZnablNormal(ranges, MX, DX, sum);

            double MXExp = getMxExp(ranges, sum);
            double zExp = getZnablExp(ranges, 1 / MXExp, sum);

            if(zNorm >= 0.103 && zNorm <=6)
                result.Add("Norm", "a = " + MX + " b = " + Math.Sqrt(DX));
            else if(zExp >= 0.103 && zExp <= 6)
                result.Add("Exp", "u = " + 1 / MXExp);
            else if(zNorm - 6 < zExp -6)
                result.Add("Norm", "a = " + MX + " b = " + Math.Sqrt(DX));
            else
                result.Add("Exp", "u = " + 1 / MXExp);

            return result;
        }

        private static double getMxExp(Dictionary<string, int> ranges, double amount)
        {
            double sum = 0;
            foreach (KeyValuePair<string, int> entry in ranges)
            {
                double xi = getSrednRange(entry.Key);
                int ni = entry.Value;
                sum += (xi * ni);
            }
            return sum / amount;
        }

        private static Dictionary<string, int> getFiveRanges(List<double> vector)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            double min = vector[0];
            double max = vector[0];
            for (int i = 0; i < vector.Count; i++)
            {
                if (min > vector[i])
                    min = vector[i];
                if (max < vector[i])
                    max = vector[i];
            }
            max += 1;
            double step = (max - min) / 5.0;
            double temp = min;
            while (temp != max)
            {
                result.Add(temp + "-" + (temp + step), 0);
                temp += step;
            }
            for (int i = 0; i < vector.Count; i++)
            {
                foreach (KeyValuePair<string, int> enty in result)
                {
                    string range = enty.Key;
                    if (containsInRange(range, vector[i]))
                    {
                        result[range] += 1;
                        break;
                    }
                }
            }
            return result;
        }

        private static double getZnablExp(Dictionary<string, int> ranges, double lambda, double amount)
        {
            double Znabl = 0;
            List<double> teoret_ver = new List<double>();
            List<double> fact_znach = new List<double>();
            foreach (KeyValuePair<string, int> entry in ranges)
            {
                fact_znach.Add(entry.Value);
                string range = entry.Key;
                double x_i = Convert.ToDouble(range.Substring(0, range.IndexOf('-')));
                double x_i_plus_1 = Convert.ToDouble(range.Substring(range.IndexOf('-') + 1));
                teoret_ver.Add(getExpRangeVer(lambda, x_i, x_i_plus_1));
            }
            for (int i = 0; i < teoret_ver.Count; i++)
            {
                double znach_ter = teoret_ver[i] * amount;
                Znabl += Math.Pow(fact_znach[i] - znach_ter, 2) / znach_ter;
            }
            return Znabl;
        }
        private static double getExpRangeVer(double lambda, double x_i, double x_i_plus_1)
        {
            return Math.Exp((-1) * lambda * x_i) - Math.Exp((-1) * lambda * x_i_plus_1);
        }
        private static double getZnablNormal(Dictionary<string, int> ranges, double MX, double DX, double amount)
        {
            double Znabl = 0;
            List<double> teoret_ver = new List<double>();
            int count = 1;
            List<double> fact_znach = new List<double>();
            foreach (KeyValuePair<string, int> entry in ranges)
            {
                fact_znach.Add(entry.Value);
                string range = entry.Key;
                double x_i = Convert.ToDouble(range.Substring(0, range.IndexOf('-')));
                double x_i_plus_1 = Convert.ToDouble(range.Substring(range.IndexOf('-') + 1));
                double y_i = 0;
                if (count == 1)
                    y_i = -5;
                else
                    y_i = getY(x_i, MX, DX);
                double y_i_plus_1 = 0;
                if (count == 5)
                    y_i_plus_1 = 5;
                else
                    y_i_plus_1 = getY(x_i_plus_1, MX, DX);

                teoret_ver.Add(Laplace.getF(y_i_plus_1) - Laplace.getF(y_i));
                count++;
            }
            for (int i = 0; i < teoret_ver.Count; i++)
            {
                double znach_ter = teoret_ver[i] * amount;
                Znabl += Math.Pow(fact_znach[i] - znach_ter, 2) / znach_ter;
            }
            return Znabl;
        }
        private static bool containsInRange(string range, double x)
        {
            double beginRange = Convert.ToDouble(range.Substring(0, range.IndexOf('-')));
            double endRange = Convert.ToDouble(range.Substring(range.IndexOf('-') + 1));
            return x >= beginRange && x < endRange;
        }

        private static double getSrednRange(string range)
        {
            double beginRange = Convert.ToDouble(range.Substring(0, range.IndexOf('-')));
            double endRange = Convert.ToDouble(range.Substring(range.IndexOf('-') + 1));
            return (beginRange + endRange) / 2;
        }

        private static double getMXNormal(Dictionary<string, int> ranges, double amount)
        {
            double sum_xi_ni = 0;
            foreach (KeyValuePair<string, int> entry in ranges)
            {
                double xi = getSrednRange(entry.Key);
                int ni = entry.Value;
                sum_xi_ni += (xi * ni);
            }
            return sum_xi_ni / amount;
        }

        private static double getDX(Dictionary<string, int> ranges, double MX)
        {
            double sum = 0;
            int amount = 0;
            foreach (KeyValuePair<string, int> entry in ranges)
            {
                double xi = getSrednRange(entry.Key);
                double temp = (xi - MX) * (xi - MX) * entry.Value;
                sum += temp;
                amount += entry.Value;
            }
            return sum / amount;
        }

        private static double getY(double X, double MX, double DX)
        {
            return (X - MX) / Math.Sqrt(DX);
        }
    }
}
