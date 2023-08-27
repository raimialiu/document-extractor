namespace DocumentExtractor.Models;

public class FormRecognizerSettings
{
    public FormRecognizerSettings(string formKey, string formEndpioint)
    {
        this.key = formKey;
        endpoint = formEndpioint;
    }
    
    public string key { get; set; }
    public string endpoint { get; set; }
    
}