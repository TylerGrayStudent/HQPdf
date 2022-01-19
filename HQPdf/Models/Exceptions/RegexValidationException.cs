namespace HQPdf.Models;

public class RegexValidationException : Exception
{
    public RegexValidationException(string message) : base(message)
    {
    }
}
