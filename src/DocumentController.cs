using System.IO;
using System.Threading.Tasks;
using DocxTemplateExample.Models;
using DocxTemplateExample.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocxTemplateExample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly DocxTemplateService _docxService;

        public DocumentController(DocxTemplateService docxService)
        {
            _docxService = docxService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] Data data)
        {
            var filePath = await _docxService.GenerateDocumentAsync(data);

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

            var fileName = Path.GetFileName(filePath);

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                fileName
            );
        }
    }
}
