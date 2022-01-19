namespace HQPdf.Models.Trojan;

public class Forms
{
    public int Id { get; set; }
    public string FormIdentifier { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    public string State { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ExpiredDate { get; set; }
}