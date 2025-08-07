using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TemplateEngine.Docx;

namespace DocxTemplateExample
{
	class Program
	{
		
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
			
			var json = File.ReadAllText("D:\\Users\\User\\source\\repos\\TestTask\\TestTask\\artifacts\\text.json");
			var data = JsonSerializer.Deserialize<Data>(json);


			var paymentsContent = data.Payments.ConvertAll(payment => new TableRowContent(
			new FieldContent("Date", payment.Date),
			new FieldContent("Amount", payment.Amount.ToString("N2")),
			new FieldContent("Description", payment.Description)
			));

			var content = new Content(
			new FieldContent("FullName", data.FullName),
			new TableContent("Payments", paymentsContent)
			);

			string templatePath = "D:\\Users\\User\\source\\repos\\TestTask\\TestTask\\artifacts\\OutputDocument.docx";
			string outputPath = "D:\\Users\\User\\source\\repos\\TestTask\\TestTask\\SaveDocument\\FilledDocument.docx";

			File.Copy(templatePath, outputPath, true);

			using (var templateProcessor = new TemplateProcessor(outputPath)
				.SetRemoveContentControls(true))
			{
				templateProcessor.FillContent(content);
				templateProcessor.SaveChanges(); 
			}

			Console.WriteLine("Документ успешно создан: FilledDocument.docx");
		}
	}
}
