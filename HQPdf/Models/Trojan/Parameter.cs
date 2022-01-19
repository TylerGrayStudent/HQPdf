using HQPdf.Models.Enums;

namespace HQPdf.Models.Trojan;

public class Parameter
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string RegexString { get; set; }
    public string RegexDescription { get; set; }
    public string ParamaterValue { get; set; }
    public string FormId { get; set; }
    public RegexType RegexType { get; set; }
    public ViewType ViewType { get; set; }
}