using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TemplateEngine.Docx;

namespace DocxTemplateExample
{
    class Program
    {
        public static async Task CopyFileSafeAsync(string sourcePath, string destPath)
        {
            const int maxAttempts = 5;
            const int delayMs = 500;
            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    using (var sourceStream = File.Open(sourcePath, FileMode.Open))
                    using (var destinationStream = File.Create(destPath))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }
                    return;
                }
                catch (IOException)
                {
                    if (i == maxAttempts - 1)
                        throw;
                    await Task.Delay(delayMs);
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

        static async Task Main(string[] args)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var jsonPath = Path.Combine(baseDir, "artifacts", "text.json");
            var templatePath = Path.Combine(
                baseDir,
                "artifacts",
                "TemplateWithRepeatingPayments.docx"
            );
            var outputPath = Path.Combine(baseDir, "result", "FilledDocument.docx");
            var json = await File.ReadAllTextAsync(jsonPath);
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
            await CopyFileSafeAsync(templatePath, outputPath);
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
