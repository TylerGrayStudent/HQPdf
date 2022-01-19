using System.Text.Json;
using HQPdf;
using HQPdf.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PdfApi.Models;
using Repo = Repo.Repo;

namespace PdfApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PdfController : ControllerBase
{
    private readonly PdfTool _pdfTool;
    private readonly global::Repo.Repo _repo;

    public PdfController(ILogger<PdfController> logger)
    {
        _pdfTool = new PdfTool();
        _repo = new global::Repo.Repo();
    }
    

    [HttpPost("parse")]
    public async Task<IActionResult> Parse(UploadFile file)
    {
        var bytes = Convert.FromBase64String(file.File.Split(",")[1]);
        var data = _pdfTool.Parse(bytes);
        return Ok(data);
    }
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var pdf = System.IO.File.ReadAllBytes("filledoutform.pdf");
        var data = _pdfTool.Parse(pdf);
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> FixPdfForm(UploadFile file)
    {
        var bytes = Convert.FromBase64String(file.File.Split(",")[1]);
        var signature = await System.IO.File.ReadAllBytesAsync("signature.jpg");
        var signature64 = Convert.ToBase64String(signature);
        //var paramaters = await _repo.GetParametersForForm(116);
        var newFile = _pdfTool.FillOutPdf(bytes, file.Content,signature64);
        //var validated = _pdfTool.ValidateFormFields(newFile, paramaters);
        return Ok(newFile);
    }
}