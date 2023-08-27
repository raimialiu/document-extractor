using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace DocumentExtractor.Models;

public class DocumentLayoutResponse
{
    public DocumentLayoutResponse()
    {
        pageResponses = new List<DocumentPageResponse>();
        tables = new List<DocumentTable>();
    }
    public List<DocumentPageResponse> pageResponses { get; set; }
    public List<DocumentParagraph> Paragraphs { get; set; }
    public List<DocumentTable>  tables { get; set; }
}

public class DocumentPageResponse
{
    public DocumentPageResponse()
    {
        
    }
    public DocumentPage pages { get; set; }
    public int LineCount { get; set; }
    public int WordCount { get; set; }
    
    
}