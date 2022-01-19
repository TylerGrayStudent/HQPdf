namespace HQPdf.Models;

public class SignatureBoxNotFoundException : Exception
{
    public SignatureBoxNotFoundException(string message) : base(message)
    {
    }
}