using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

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

	static void Main()
	{
		var json = File.ReadAllText("text.json");
		var data = JsonSerializer.Deserialize<Data>(json);

		string templatePath = "OutputDocument.docx";       
		string outputPath = "OutputDocument2.docx";

		File.Copy(templatePath, outputPath, true);  

		using (var wordDoc = WordprocessingDocument.Open(outputPath, true))
		{
			var mainPart = wordDoc.MainDocumentPart;

			
			ReplaceContentControlText(mainPart, "FullName", data.FullName);

			
			FillPaymentsTable(mainPart, "Payments", data.Payments);

			mainPart.Document.Save();
		}

		Console.WriteLine("Документ успешно создан с заполненными контролами: " + outputPath);
	}

	
	static void ReplaceContentControlText(MainDocumentPart mainPart, string tagName, string textToInsert)
	{
		var sdtElements = mainPart.Document.Body.Descendants<SdtElement>();

		foreach (var sdt in sdtElements)
		{
			var tag = sdt.SdtProperties?.GetFirstChild<Tag>();
			if (tag != null && tag.Val == tagName)
			{
			
				var texts = sdt.Descendants<Text>();
				foreach (var txt in texts)
				{
					txt.Text = string.Empty; 
				}

				
				var run = sdt.Descendants<Run>().FirstOrDefault();
				if (run != null)
				{
					var firstText = run.GetFirstChild<Text>();
					if (firstText != null)
						firstText.Text = textToInsert;
					else
						run.AppendChild(new Text(textToInsert));
				}
				else
				{
					
					sdt.AppendChild(new Run(new Text(textToInsert)));
				}
			}
		}
	}


	static void FillPaymentsTable(MainDocumentPart mainPart, string tagName, List<Payment> payments)
	{
		var sdtElements = mainPart.Document.Body.Descendants<SdtElement>();

		foreach (var sdt in sdtElements)
		{
			var tag = sdt.SdtProperties?.GetFirstChild<Tag>();
			if (tag != null && tag.Val == tagName)
			{
				var table = sdt.Descendants<Table>().FirstOrDefault();
				if (table == null)
					continue;

			
				var rows = table.Elements<TableRow>().ToList();
				if (rows.Count < 2)
					continue;

				var templateRow = rows[1];

				
				while (table.Elements<TableRow>().Count() > 2)
					table.RemoveChild(table.Elements<TableRow>().ElementAt(2));

				foreach (var payment in payments)
				{
					var newRow = (TableRow)templateRow.CloneNode(true);

					
					foreach (var text in newRow.Descendants<Text>())
					{
						if (text.Text.Contains("{{Date}}"))
							text.Text = payment.Date ?? "";
						else if (text.Text.Contains("{{Amount}}"))
							text.Text = payment.Amount.ToString("N2");
						else if (text.Text.Contains("{{Description}}"))
							text.Text = payment.Description ?? "";
					}

					table.AppendChild(newRow);
				}

				
				table.RemoveChild(templateRow);
			}
		}
	}
}
