using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace DocumentExtractor.Models;

public class TrainModelResponse
{
    public TrainModelResponse()
    {
        FieldSchemata = new Dictionary<string, DocumentFieldResponse>();
    }
    public string ModelId { get; set; }
    public string TrainingStartedOn { get; set; }
    public string TrainingCompletedOn { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    
    public Dictionary<string, DocumentFieldResponse> FieldSchemata { get; set; }
    public string TrainingDescription { get; set; }
    public string Status { get; set; }
}

public class DocumentFieldResponse
{
    public DocumentFieldSchema FieldSchema { get; set; }
    public float ConfidenceLevel { get; set; }
    public DocumentTypeDetails DocumentTypeDetails { get; set; }
    public string DocumentTypeKey { get; set; }
}

public class TrainModelRequest
{
    public string FileUri { get; set; }
    public string ModelId { get; set; }
    public IFormFile documentFile { get; set; }
    public string FolderPath { get; set; }
    public Stream? FileStream { get; set; }
}