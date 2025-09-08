using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DocxTemplateExample.Models;
using TemplateEngine.Docx;

namespace DocxTemplateExample.Services
{
    public class DocxTemplateService
    {
        public async Task CopyFileSafeAsync(string sourcePath, string destPath)
        {
            const int maxAttempts = 5;
            const int delayMs = 500;

            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    using var sourceStream = File.Open(sourcePath, FileMode.Open);
                    using var destinationStream = File.Create(destPath);
                    await sourceStream.CopyToAsync(destinationStream);
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

        public async Task<string> GenerateDocumentAsync(Data data)
        {
            try
            {
              
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;

              
                var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));

                
                var templatePath = Path.Combine(
                    projectRoot,
                    "artifacts",
                    "TemplateWithRepeatingPayments.docx"
                );
                var outputDir = Path.Combine(projectRoot, "result");

                if (!File.Exists(templatePath))
                    throw new FileNotFoundException($"Template not found: {templatePath}");

                Directory.CreateDirectory(outputDir);

                var outputPath = Path.Combine(outputDir, $"FilledDocument_{Guid.NewGuid()}.docx");

                var paymentsContent =
                    data.Payments?.ConvertAll(payment => new TableRowContent(
                        new FieldContent("Date", payment.Date ?? string.Empty),
                        new FieldContent("Amount", payment.Amount.ToString("N2")),
                        new FieldContent("Description", payment.Description ?? string.Empty)
                    )) ?? new List<TableRowContent>();

                var content = new Content(
                    new FieldContent("FullName", data.FullName ?? string.Empty),
                    new TableContent("Payments", paymentsContent)
                );

                await CopyFileSafeAsync(templatePath, outputPath);

                using var templateProcessor = new TemplateProcessor(
                    outputPath
                ).SetRemoveContentControls(true);

                templateProcessor.FillContent(content);
                templateProcessor.SaveChanges();

                return outputPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при генерации документа: {ex.Message}");
                throw;
            }
        }
    }
}
