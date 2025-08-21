using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using TemplateEngine.Docx;

namespace DocxTemplateExample
{
    class Program
    {
        public static void CopyFileSafe(string sourcePath, string destPath)
        {
            const int maxAttempts = 5;
            const int delayMs = 500;

            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    File.Copy(sourcePath, destPath, true);
                    return;
                }
                catch (IOException)
                {
                    if (i == maxAttempts - 1)
                        throw;
                    Thread.Sleep(delayMs);
                }
            }
        }

        public class Payment
        {
            public string Date { get; set; }
            public decimal Amount { get; set; }
            public string Description { get; set; }
        }

        public class Data
        {
            public string FullName { get; set; }
            public List<Payment> Payments { get; set; }
        }

        static void Main(string[] args)
        {
            var json = File.ReadAllText("D:\\True_Test_Task\\Test_template\\artifacts\\text.json");
            var data = JsonSerializer.Deserialize<Data>(json);

            var paymentsContent = data.Payments.ConvertAll(payment => new TableRowContent(
                new FieldContent("Date", payment.Date ?? string.Empty),
                new FieldContent("Amount", payment.Amount.ToString("N2")),
                new FieldContent("Description", payment.Description ?? string.Empty)
            ));

            var content = new Content(
                new FieldContent("FullName", data.FullName ?? string.Empty),
                new TableContent("Payments", paymentsContent)
            );

            string templatePath =
                "D:\\True_Test_Task\\Test_template\\artifacts\\OutputDocument.docx";
            string outputPath = "D:\\True_Test_Task\\Test_template\\result\\FilledDocument.docx";

            CopyFileSafe(templatePath, outputPath);

            using (
                var templateProcessor = new TemplateProcessor(outputPath).SetRemoveContentControls(
                    true
                )
            )
            {
                templateProcessor.FillContent(content);
                templateProcessor.SaveChanges();
            }

            Console.WriteLine("Документ успешно создан: FilledDocument.docx");
        }
    }
}
