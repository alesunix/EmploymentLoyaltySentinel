using ClosedXML.Excel;
using Microsoft.JSInterop;

namespace EmploymentLoyaltySentinel.Models
{
    public class ExcelModel
    {
        OffendersModel oModel = new();
        private async Task ExportExcel(IJSRuntime js, string fileName, byte[] data)
        {
            await js.InvokeAsync<object>("SaveXls", fileName, Convert.ToBase64String(data));
        }
        public async void GenerateOffendersExcel(IJSRuntime js, List<OffendersModel> Table, string fileName)
        {
            XLWorkbook wb = new();
            wb.Properties.Author = "BLID";
            wb.Properties.Title = "Выгрузка";
            wb.Properties.Subject = fileName;

            var ws = wb.Worksheets.Add("Данные по нарушителям");
            ws.Cell(1, 1).Value = "ФИО";
            ws.Cell(1, 2).Value = "Должность";
            ws.Cell(1, 3).Value = "Дата рождения";
            ws.Cell(1, 4).Value = "Дата увольнения";
            ws.Cell(1, 5).Value = "Подразделение";
            ws.Cell(1, 6).Value = "Вид нарушения";
            ws.Cell(1, 7).Value = "Сумма финансовых потерь";
            ws.Cell(1, 8).Value = "Остаток долга";
            ws.Cell(1, 9).Value = "Валюта";
            ws.Cell(1, 10).Value = "Аудит";
            ws.Cell(1, 11).Value = "Статус приёма на работу";
            ws.Cell(1, 12).Value = "Представитель КРО";
            ws.Cell(1, 13).Value = "Удален";
            for (int i = 0; i < 13; i++)
            {
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Font.FontSize = 12;
            }
            ws.SheetView.FreezeRows(1);/// Закрепить строку
            for (int i = 0; i < Table.Count; i++)
            {
                int row = i + 2;
                ws.Cell(row, 1).Value = Table[i].Employee;
                ws.Cell(row, 2).Value = Table[i].Dol;
                ws.Cell(row, 3).Value = Table[i].Dr.ToShortDateString();
                ws.Cell(row, 4).Value = Table[i].Dp.ToShortDateString();
                ws.Cell(row, 5).Value = Table[i].Grpprgname;
                string offenses = string.Empty;
                foreach (var offense in oModel.GetOffenses(Table[i].Id))
                {
                    offenses += offense.Value + '.' + "\r\n";
                }
                char[] lineBreaks = { '\r', '\n' };
                ws.Cell(row, 6).Value = offenses.TrimEnd(lineBreaks);
                ws.Cell(row, 7).Value = Table[i].Summ;
                ws.Cell(row, 8).Value = Table[i].Balance;
                ws.Cell(row, 9).Value = Table[i].Curname;
                ws.Cell(row, 10).Value = "Оператор: " + oModel.GetEmployeeForUserId(Table[i].Loginid) + "\r\n" + "Дата записи: " + Table[i].Date_record + "\r\n" + "Оператор последнего редактирования: " + oModel.GetEmployeeForUserId(Table[i].Loginid_lastedit) + "\r\n" + "Дата последнего редактирования: " + Table[i].Date_lastedit;
                ws.Cell(row, 11).Value = Table[i].Status == "F" ? "Нет" : Table[i].Status == "U" ? "Не определен" : "Да";
                ws.Cell(row, 12).Value = Table[i].Auditor;
                ws.Cell(row, 13).Value = Table[i].Deleted == "F" ? "Нет" : "Да";
                
            }
            //ws.Rows().AdjustToContents();/// Высота строк по содержимому
            ws.Columns().AdjustToContents();/// Ширина столбца по содержимому
            var rowHeader = ws.Row(1);
            //rowHeader.Style.Fill.BackgroundColor = XLColor.Orange;
            rowHeader.Height = 20;
            byte[] bytes = Array.Empty<byte>();
            using (var ms = new MemoryStream())
            {
                wb.SaveAs(ms);
                bytes = ms.ToArray();
            }
            await ExportExcel(js, $"{fileName}.xlsx", bytes);
        }
        public async void GenerateReviseExcel(IJSRuntime js, List<ReviseModel> Table, string fileName)
        {
            XLWorkbook wb = new();
            wb.Properties.Author = "BLID";
            wb.Properties.Title = "Выгрузка";
            wb.Properties.Subject = fileName;

            var ws = wb.Worksheets.Add("Проверка нарушителей");
            ws.Cell(1, 1).Value = "ФИО";
            ws.Cell(1, 2).Value = "Дата рождения";
            ws.Cell(1, 3).Value = "Дата увольнения";
            ws.Cell(1, 4).Value = "Должность";
            ws.Cell(1, 5).Value = "Подразделение";
            ws.Cell(1, 6).Value = "Регион";           
            for (int i = 0; i < 13; i++)
            {
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Font.FontSize = 12;
            }
            ws.SheetView.FreezeRows(1);/// Закрепить строку
            for (int i = 0; i < Table.Count; i++)
            {
                int row = i + 2;
                //Stream stream = new MemoryStream((byte[])Table[i].Photo);
                //var img = ws.AddPicture(stream).MoveTo(ws.Cell(row, 1)).Scale(0.5);
                ws.Cell(row, 1).Value = Table[i].Fio;
                ws.Cell(row, 2).Value = Table[i].Dr.ToShortDateString();
                ws.Cell(row, 3).Value = Table[i].De.ToShortDateString();
                ws.Cell(row, 4).Value = Table[i].Dol;
                ws.Cell(row, 5).Value = Table[i].Grpprg;
                ws.Cell(row, 6).Value = Table[i].Region;
            }
            ws.Columns().AdjustToContents();/// Ширина столбца по содержимому
            var rowHeader = ws.Row(1);
            rowHeader.Height = 20;
            byte[] bytes = Array.Empty<byte>();
            using (var ms = new MemoryStream())
            {
                wb.SaveAs(ms);
                bytes = ms.ToArray();
            }
            await ExportExcel(js, $"{fileName}.xlsx", bytes);
        }
    }
}
