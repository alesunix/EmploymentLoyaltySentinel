using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SendMailOffenders
{
    internal class MainModel : BaseModel
    {
        PService pService = new PService();
        static Timer timer;
        static object syncLock = new object();
        static bool sent = false;
        private Dictionary<string, string> GetOffendersStatus()/// Список нарушителей которым нужно выставить статус
        {
            return GetDictionary($@"SELECT a.employee, b.nam as grpprgname
                                      FROM offenders a 
                                      left join sprgrpprg_v b on a.grpprg = b.grpprg 
                                      WHERE a.status = 'U'");
        }
        private List<string> GetListPservice()/// Почтовые номера получателей (Представители КРО)
        {
            return GetList($@"SELECT pservice FROM userid WHERE role_id = 2 AND deleted = 'F'");
        }
        private List<OffendersCheck> GetOffendersCheck()
        {
            return GetListOffendersCheck($@"SELECT a.fio, a.dr, a.dol, b.nam as grpprg, a.de
                                              FROM sprotv_mv a 
                                              left join sprgrpprg_v b on a.grpprg = b.grpprg 
                                              WHERE (fio, dr) in (select c.employee, c.dr from offenders c)
                                              AND de > sysdate");
        }
        private void CreateDocCheck()
        {
            var offenders = GetOffendersCheck();
            int i = 0;
            string[] index = new string[offenders.Count + 7];
            foreach (var item in offenders)
            {
                Console.WriteLine(item);
                index[0] = "Уведомление.";
                index[1] = "В списке представлены нарушители, требующие дополнительного контроля.";
                index[3 + i++] = $"{i}. {item.Employee}, {item.Dr.ToShortDateString()}, {item.Dol}, {item.Grpprg}";
            }
            index[4 + i] = "Попавший в этот список, имеет дату увольнения, отличную от даты, установленной в учётной карточке системы контроля нарушителей.";
            index[5 + i] = "Необходимо уточнить причины несовпадения дат (повторный приём, ошибка в дате увольнения) и принять соответствующие меры по устранению несоответствий.";
            index[6 + i] = $"{DateTime.Now.ToShortDateString()} Письмо сгенерировано автоматически программой SendMailOffenders!";
            File.WriteAllLines("MailCheck.txt", index);
        }
        private void CreateDocStatus()
        {
            var offenders = GetOffendersStatus();
            int i = 0;
            string[] index = new string[offenders.Count + 5];
            foreach (var item in offenders)
            {
                Console.WriteLine(item);
                index[0] = "Здравствуйте!";
                index[1] = "Необходимо утвердить статус нарушителей ( по адресу http://blid.local:8081/ ):";
                index[3 + i++] = $"{i}. {item.Key}, {item.Value}";
            }
            index[4 + i] = $"{DateTime.Now.ToShortDateString()} Письмо сгенерировано автоматически программой SendMailOffenders!";
            File.WriteAllLines("MailStatus.txt", index);
        }
        private void SendFile(string path)
        {
            var pservices = GetListPservice();
            string pservice = string.Empty;
            foreach (var item in pservices)
            {
                if (item.Trim() != "0")
                pservice += item.Trim() + '_';
            }
            //pservice = "262_255_";/// для теста
            if (File.Exists(path) == true)/// Проверка существования файла 
            {
                if (pService.Encript(path, pservice.TrimEnd('_').Trim()))
                {
                    pService.Sending();
                }
            }
        }
        private void Send()
        {
            var offendersStatus = GetOffendersStatus();
            var offendersCheck = GetOffendersCheck();
            var pservices = GetListPservice();
            if (offendersStatus.Count != 0 && pservices.Count != 0)/// Если есть хоть один нарушитель и получатель
            {
                CreateDocStatus();
                SendFile("MailStatus.txt");
            }
            if (offendersCheck.Count != 0 && pservices.Count != 0)
            {
                CreateDocCheck();
                SendFile("MailCheck.txt");
            }
        }
        public void Init()/// Запуск метода каждые 30 сек
        {
            timer = new Timer(new TimerCallback(SendMail), null, 0, 30000);
        }
        void SendMail(object obj)
        {
            lock (syncLock)
            {
                DateTime date = DateTime.Now;
                if (date.Hour == BaseModel.Hour && date.Minute == BaseModel.Minute && sent == false)
                {
                    Send();
                    sent = true;
                }
                else
                {
                    sent = false;
                }
            }
        }
    }
}
