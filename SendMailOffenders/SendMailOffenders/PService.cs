using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SendMailOffenders
{
    class PService
    {
        RegistryKey rg;
        string pservicecmd;
        StringBuilder cmd = new StringBuilder(512);
        DataTable PgpDat;
        public PService()
        {
            if (Registry.CurrentUser.OpenSubKey(@"Software\NoName Inc\PService\Settings") != null)
            {
                rg = Registry.CurrentUser.OpenSubKey(@"Software\NoName Inc\PService\Settings");
                pservicecmd = rg.GetValue("Path").ToString() + @"PService.exe";
                InitTable();
            }
        }
        public bool Encript(string pathFile, string recipients)
        {
            string[] array = recipients.Split(new char[] { '_' });/// Разделить получателей на массив строк
            foreach (string item in array)
            {
                if (!CheckKey(item))
                    return false; /// Если найден не cуществующий получатель
            }
            // Создаем список аргументов
            cmd.Clear();
            cmd.Append("\"" + pathFile + "\"");
            if (array.Length > 1)/// Если больше одного получателя
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (i == 0)
                        cmd.Append(" -t" + array[i]);
                    else
                        cmd.Append("_" + array[i]);
                }
                cmd.Append(" -f" + array[array.Length - 1]).Append(" -h -d " + pathFile);/// Итоговая запись в cmd
            }
            else /// Если один получатель
                cmd.Append(" -t" + array[0]).Append(" -f" + array[0]).Append(" -h -d " + pathFile);/// Итоговая запись в cmd
            return true; /// Eсли всё хорошо
        }

        // Загружаем в табличку список существующих ключей
        public void InitTable()
        {
            PgpDat = new DataTable();
            PgpDat.Columns.Add("Keys", typeof(string));
            DataRow row;
            string path = rg.GetValue("Path").ToString() + "psgf.dat";
            StreamReader reader = new StreamReader(path);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] array = line.Split(';');
                if (array[0] != "key") continue;
                for (int i = 2; i < array.Length; i++)
                {
                    row = PgpDat.NewRow();
                    row["Keys"] = array[1];
                    PgpDat.Rows.Add(row);
                }
            }
            reader.Close();
        }

        // Проверяем существует ли ключ
        public bool CheckKey(string key)
        {
            string exrpession = string.Format("Keys = '{0}'", key);
            DataRow[] row = PgpDat.Select(exrpession);
            if (row.Length > 0)
                return true;
            else
                return false;
        }

        // Отправка
        public bool Sending()
        {
            try
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = pservicecmd;
                startInfo.Arguments = cmd.ToString();
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                return true;
            }
            catch (FormatException exc)
            {
                //MessageBox.Show(exc.ToString());
                return false;
            }
        }
    }
}
