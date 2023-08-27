using Azure;
using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.AI.FormRecognizer.Training;
using DocumentExtractor.Models;

namespace DocumentExtractor.Services;

public class DocumentExtractorService
{
    private readonly IConfiguration _config;
    private readonly FormRecognizerSettings _formRecognizerSettings;

    public DocumentExtractorService(IConfiguration config, FormRecognizerSettings formRecognizerSettings)
    {
        _config = config;
        _formRecognizerSettings = formRecognizerSettings;
    }


    public async Task<ExtractDocumentResponse> ExtractDataFromDocument(ExtractDocumentModel documentModel)
    {
        
        AzureKeyCredential credential = new AzureKeyCredential(_formRecognizerSettings.key);
        DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(_formRecognizerSettings.endpoint), credential);

        //sample invoice document
        string modelId = String.IsNullOrEmpty(documentModel.ModelId) ? "prebuilt-document": ( !documentModel.ModelId.StartsWith("prebuilt") ? String.Concat("prebuilt-", documentModel.ModelId.ToLower())
                                    : documentModel.ModelId);
        AnalyzeDocumentOperation operation;
        if (documentModel.FileName is null || documentModel.FileName?.Length < 0)
        {
            Uri invoiceUri = new Uri (documentModel.FileUri);
            operation = await client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, modelId, invoiceUri);
        }
        else
        {
            operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, modelId,
                documentModel.FileName?.OpenReadStream());
        }

        var response = new ExtractDocumentResponse();
        AnalyzeResult result = operation.Value;
        response.Content = result.Content;
        response.Paragraphs = result?.Paragraphs?.ToList();
        response.Pages = result?.Pages?.ToList();
        response.Tables = result?.Tables?.ToList();
        response.Documents = result?.Documents;

        return response;
    }

    public async Task<DocumentLayoutResponse> ExtractDocumentLayout(IFormFile file)
    {
        var credential = new AzureKeyCredential(_formRecognizerSettings.key);
        var client = new DocumentAnalysisClient(new Uri(_formRecognizerSettings.endpoint), credential);
        
       // Uri fileUri = new Uri("<fileUri>");
        using var stream = file.OpenReadStream();

        AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-layout", stream);
        AnalyzeResult result = operation.Value;

        var layoutResponse = new DocumentLayoutResponse();

        foreach (DocumentPage page in result.Pages)
        {
            Console.WriteLine($"Document Page {page.PageNumber} has {page.Lines.Count} line(s), {page.Words.Count} word(s),");
            Console.WriteLine($"and {page.SelectionMarks.Count} selection mark(s).");
            layoutResponse.pageResponses.Add(new DocumentPageResponse()
            {
                pages = page,
                WordCount = page.Words.Count,
                LineCount = page.Lines.Count
            });

            for (int i = 0; i < page.Lines.Count; i++)
            {
                DocumentLine line = page.Lines[i];
                Console.WriteLine($"  Line {i} has content: '{line.Content}'.");

                Console.WriteLine($"    Its bounding polygon (points ordered clockwise):");

                for (int j = 0; j < line.BoundingPolygon.Count; j++)
                {
                    Console.WriteLine($"      Point {j} => X: {line.BoundingPolygon[j].X}, Y: {line.BoundingPolygon[j].Y}");
                }
            }

            for (int i = 0; i < page.SelectionMarks.Count; i++)
            {
                DocumentSelectionMark selectionMark = page.SelectionMarks[i];

                Console.WriteLine($"  Selection Mark {i} is {selectionMark.State}.");
                Console.WriteLine($"    Its bounding polygon (points ordered clockwise):");

                for (int j = 0; j < selectionMark.BoundingPolygon.Count; j++)
                {
                    Console.WriteLine($"      Point {j} => X: {selectionMark.BoundingPolygon[j].X}, Y: {selectionMark.BoundingPolygon[j].Y}");
                }
            }
        }

        Console.WriteLine("Paragraphs:");

        layoutResponse.Paragraphs = result.Paragraphs.ToList();
        layoutResponse.tables = result.Tables.ToList();
        foreach (DocumentParagraph paragraph in result.Paragraphs)
        {
            Console.WriteLine($"  Paragraph content: {paragraph.Content}");

            if (paragraph.Role != null)
            {
                Console.WriteLine($"    Role: {paragraph.Role}");
            }
        }

        foreach (DocumentStyle style in result.Styles)
        {
            // Check the style and style confidence to see if text is handwritten.
            // Note that value '0.8' is used as an example.

            bool isHandwritten = style.IsHandwritten.HasValue && style.IsHandwritten == true;

            if (isHandwritten && style.Confidence > 0.8)
            {
                Console.WriteLine($"Handwritten content found:");

                foreach (DocumentSpan span in style.Spans)
                {
                    Console.WriteLine($"  Content: {result.Content.Substring(span.Index, span.Length)}");
                }
            }
        }

        Console.WriteLine("The following tables were extracted:");

        for (int i = 0; i < result.Tables.Count; i++)
        {
            DocumentTable table = result.Tables[i];
            Console.WriteLine($"  Table {i} has {table.RowCount} rows and {table.ColumnCount} columns.");

            foreach (DocumentTableCell cell in table.Cells)
            {
                Console.WriteLine($"    Cell ({cell.RowIndex}, {cell.ColumnIndex}) has kind '{cell.Kind}' and content: '{cell.Content}'.");
            }
        }

        return layoutResponse;
    }

    public async Task<TrainModelResponse> BuildCustomModel(TrainModelRequest request)
    {
        Uri trainingFileUri = new Uri(request.FileUri);
        var client = new DocumentModelAdministrationClient(new Uri(_formRecognizerSettings.endpoint), new AzureKeyCredential(_formRecognizerSettings.key));
        
        BuildDocumentModelOperation buildOperation = await client.BuildDocumentModelAsync(WaitUntil.Completed, trainingFileUri, DocumentBuildMode.Template);
        DocumentModelDetails customModel = buildOperation.Value;
        
        var trainingModelResponse = new TrainModelResponse();
        var operation = await client.BuildDocumentModelAsync(WaitUntil.Completed, trainingFileUri, DocumentBuildMode.Neural);
        DocumentModelDetails model = operation.Value;
    
        trainingModelResponse.ModelId = model.ModelId;
        trainingModelResponse.TrainingDescription = model.Description;
        trainingModelResponse.CreatedOn = model.CreatedOn;
       
        
        foreach (KeyValuePair<string, DocumentTypeDetails> documentType in model.DocumentTypes)
        {
            Console.WriteLine($"    Document type: {documentType.Key} which has the following fields:");
            foreach (KeyValuePair<string, DocumentFieldSchema> schema in documentType.Value.FieldSchema)
            {
                trainingModelResponse.FieldSchemata.Add(schema.Key, new DocumentFieldResponse()
                {
                    FieldSchema = schema.Value,
                    ConfidenceLevel = documentType.Value.FieldConfidence[schema.Key],
                    DocumentTypeDetails = documentType.Value,
                    DocumentTypeKey = documentType.Key
                });
                Console.WriteLine($"    Field: {schema.Key} with confidence {documentType.Value.FieldConfidence[schema.Key]}");
            }
        }

        return trainingModelResponse;
    }

    public async Task<object> TrainDocument(TrainModelRequest request)
    {
       // string modelId = "<modelId>";
        var credential = new AzureKeyCredential(_formRecognizerSettings.key);
        var client = new DocumentAnalysisClient(new Uri(_formRecognizerSettings.endpoint), credential);

        using var stream = request.documentFile.OpenReadStream();

        if (!request.ModelId.StartsWith("prebuilt"))
        {
            request.ModelId = String.Concat("prebuilt-", request.ModelId.ToLower());
        }
        AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, request.ModelId, stream);
        AnalyzeResult result = operation.Value;

        var response = new
        {
            Content = result.Content,
            PagesCount = result.Pages?.Count,
            TablesCount = result?.Tables?.Count,
            ParagraphCount = result?.Paragraphs?.Count
        };
        Console.WriteLine($"Document was analyzed with model with ID: {result.ModelId}");

        foreach (AnalyzedDocument document in result.Documents)
        {
            Console.WriteLine($"Document of type: {document.DocumentType}");

            foreach (KeyValuePair<string, DocumentField> fieldKvp in document.Fields)
            {
                string fieldName = fieldKvp.Key;
                DocumentField field = fieldKvp.Value;

                Console.WriteLine($"Field '{fieldName}': ");

                Console.WriteLine($"  Content: '{field.Content}'");
                Console.WriteLine($"  Confidence: '{field.Confidence}'");
            }
        }

        return response;
    }
}