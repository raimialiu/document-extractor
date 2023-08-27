using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using DocumentExtractor.Models;
using DocumentExtractor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace DocumentExtractor.Controllers;

[Route("api/v1")]
[ApiController]
public class DocumentExtractorController: ControllerBase
{
    private readonly DocumentExtractorService _docService;
    private readonly ILogger _logger;

    public DocumentExtractorController(DocumentExtractorService docService)
    {
        _docService = docService;
        _logger = new Logger<DocumentExtractorController>(new LoggerFactory());
    }
    
    [HttpPost]
    [Route("extract-layout")]
    public async Task<IActionResult> ExtracDocLayout([FromForm] IFormFile file)
    {
        try
        {
          //  _logger.Log(LogLevel.Information, message:"Begin execution of DocumentLayout Extraction");

            await _docService.ExtractDocumentLayout(file);

            return Ok(await _docService.ExtractDocumentLayout(file));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, "!!oops, you just hit a snag");
        }
       
    }

    [HttpPost]
    [Route("extract-data")]
    public async Task<IActionResult> ExtractDocument([FromForm] ExtractDocumentModel model)
    {
        try
        {
            var response = await _docService.ExtractDataFromDocument(model);

            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e);
        }
    }

    [HttpPost]
    [Route("build-model")]
    public async Task<IActionResult> BuildModel([FromBody] TrainModelRequest request)
    {
        try
        {
            _logger.LogInformation("begining of building custom model....");

            TrainModelResponse response = await _docService.BuildCustomModel(request);

            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
   
    }
    
    [HttpPost]
    [Route("train-document")]
    public async Task<IActionResult> TrainDocument([FromForm] TrainModelRequest request)
    {
        try
        {
            //  _logger.Log(LogLevel.Information, message:"Begin execution of DocumentLayout Extraction");
            
            return Ok(await _docService.TrainDocument(request));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, "!!oops, you just hit a snag");
        }
       
    }
}