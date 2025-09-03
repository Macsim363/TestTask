using System.Collections.Generic;

namespace DocxTemplateExample.Models
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
}
