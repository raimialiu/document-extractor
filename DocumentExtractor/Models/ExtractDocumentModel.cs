using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace DocumentExtractor.Models;

public class ExtractDocumentModel
{
    public string ModelId { get; set; }
    public IFormFile FileName { get; set; }
    public string FileUri { get; set; }
}

public class ExtractDocumentResponse
{
    public IReadOnlyList<AnalyzedDocument>? Documents { get; set; }
    public string Content { get; set; }
    public List<DocumentPage>? Pages { get; set; }
    public List<DocumentTable>? Tables { get; set; }
    public List<DocumentParagraph>? Paragraphs { get; set; }
    public List<DocumentKeyValueElement> KeyValueElements { get; set; }
}