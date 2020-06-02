using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using Tutorial.SqlConn;
using PreparedToGPSS;


namespace ConnectMySQL
{
    class Program
    {

        static void Main(string[] args)
            {
            Dictionary<string, List<string>> tableNameAndVectors = new Dictionary<string, List<string>>();

            Console.WriteLine("Getting Connection ...");
            MySqlConnection conn = DBUtils.GetDBConnection();

            //Открытие соединения с БЛ
            try
            {
                Console.WriteLine("Openning Connection ...");

                conn.Open();

                //Запрос на использование схемы БД
                string sqlQuery = "USE golubev;";
                MySqlCommand command = new MySqlCommand(sqlQuery, conn);
                command.ExecuteNonQuery();

                //sqlQuery = "CREATE TABLE `params_for_distribution` (`id` INT NOT NULL AUTO_INCREMENT, `table_name` VARCHAR(450) NULL, `type_of_distributions` VARCHAR(450) NULL, `params` VARCHAR(450) NULL, PRIMARY KEY(`id`));";
                //command = new MySqlCommand(sqlQuery, conn);
                //command.ExecuteNonQuery();


                //Получение таблиц для которых будут подсчитаны векторы     
                string sqlQuerySelectTebleName = "SELECT table_name FROM sample;";
                command = new MySqlCommand(sqlQuerySelectTebleName, conn);
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string tableName = reader[0].ToString();
                    tableNameAndVectors.Add(tableName, new List<string>());
                }
                reader.Close();

                //Получение порогового значения и векторов
                foreach (KeyValuePair<string, List<string>> entry in tableNameAndVectors)
                {
                    Console.WriteLine("*");
                    string tableName = entry.Key;
                    List<DateTime> dateTimes = new List<DateTime>(); // Время всех пакетов с соблюдением чередности
                    List<TimeSpan> listForVector = new List<TimeSpan>(); // параметр промежуток между пакетами

                    string queryTimeStamps = getQueryTimeStampsFromTable(tableName);
                    command = new MySqlCommand(queryTimeStamps, conn);
                    //string beginTime = "00:00:00";
                    string beginTime = command.ExecuteScalar().ToString();
                    DateTime tempTime = DateTime.Parse(beginTime);
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string current = reader[0].ToString();
                        DateTime currentTime = DateTime.Parse(current);

                        dateTimes.Add(currentTime);

                        insertToSortedList(listForVector, currentTime - tempTime);
                        tempTime = currentTime;
                    }
                    reader.Close();

                    listForVector.Reverse(); // Теперь промежутки между пакетами идут на убывание

                    //Пороговое значение
                    TimeSpan limitValue = listForVector[100];
                    if (limitValue.CompareTo(new TimeSpan(0)) != 0)
                    {
                        List<double> durationGroups = new List<double>();//последовательность длительности групп
                        List<double> spanGroups = new List<double>();//последовательность интервалов между группами
                        double sumDuration = 0;
                        double sumSpan = 0;

                        DateTime beginBlock = dateTimes[0];
                        DateTime endBlock = dateTimes[0];
                        DateTime newBeginBlock = dateTimes[0];
                        for (int i = 1; i < dateTimes.Count; i++)
                        {
                            TimeSpan tempSpan = dateTimes[i] - dateTimes[i - 1];
                            if (tempSpan <= limitValue)
                            {
                                //Если интервал не превышает заданное пороговое значение
                                continue;
                            }
                            else
                            {
                                //Если интервал превышает заданное пороговое значение
                                endBlock = dateTimes[i - 1];
                                newBeginBlock = dateTimes[i];
                                double temp = (endBlock - beginBlock).TotalMinutes;
                                durationGroups.Add(temp);
                                sumDuration += temp;
                                temp = (newBeginBlock - beginBlock).TotalMinutes;
                                spanGroups.Add(temp);
                                sumSpan += temp;
                                beginBlock = dateTimes[i];
                            }
                        }

                        entry.Value.Add(durationGroups.ToString());
                        entry.Value.Add(spanGroups.ToString());
                        //Тут у нас уже есть два вектора durationGroups и spanGroups
                        Dictionary<string, string> parametrs = Pirson.getParams(durationGroups, sumDuration);
                        foreach (KeyValuePair<string, string> temp in parametrs)
                        {
                            string queryInsert = getQueryInsert("Длит.Блока "+tableName, temp.Key, temp.Value);
                            command = new MySqlCommand(queryInsert, conn);
                            command.ExecuteNonQuery();
                        }
                        parametrs = Pirson.getParams(spanGroups, sumSpan);
                        foreach (KeyValuePair<string, string> temp in parametrs)
                        {
                            string queryInsert = getQueryInsert("Длит.Инт. " + tableName, temp.Key, temp.Value);
                            command = new MySqlCommand(queryInsert, conn);
                            command.ExecuteNonQuery();
                        }


                    }
                }

                //Выполнение без ошибок
                Console.WriteLine("End...");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
            finally
            {
                //Закрытие соединенися с БЛ
                conn.Close();
            }

            Console.Read();
        }

        private static string getQueryTimeStampsFromTable(string tableName)
        {
            return "SELECT time_stamp FROM `" + tableName + "`;";
        }

        private static void insertToSortedList(List<TimeSpan> list, TimeSpan value)
        {
            if (list.Count == 0)
                list.Add(value);
            else
            {
                int i = 0;
                while (i != list.Count && value > list[i])
                    i++;
                list.Insert(i, value);
            }
        }
        private static string getQueryInsert(string tableName, string type, string parametrs)
        {
            return "INSERT INTO `params_for_distribution` (table_name, type_of_distributions, params) VALUES ('" + tableName + "','" + type + "','" + parametrs + "');";
        }
    }
}