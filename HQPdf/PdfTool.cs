namespace HQPdf;

/// <summary>
/// Tool to create, update, and validate a pdf
/// </summary>
public class PdfTool
{
    /// <summary>
    /// Telerik tool to convert to and from pdfs
    /// </summary>
    private readonly PdfFormatProvider _formatProvider;
    /// <summary>
    /// Tool to create, update, and validate a pdf
    /// </summary>
    public PdfTool()
    {
        _formatProvider = new PdfFormatProvider();
    }
    /// <summary>
    /// "Converts" from an un-editable pdf to one that is editable
    /// </summary>
    /// <param name="stream">Stream of pdf</param>
    /// <returns>Filled out pdf</returns>
    public byte[] FixLockedFormFields(Stream stream)
    {
        var document = _formatProvider.Import(stream);
        return _fixLockedFormFields(document);
    }
    /// <summary>
    /// "Converts" from an un-editable pdf to one that is editable
    /// </summary>
    /// <param name="bytes">Byte array of pdf</param>
    /// <returns>Filled out pdf</returns>
    public byte[] FixLockedFormFields(byte[] bytes)
    {
        var document = _formatProvider.Import(bytes);
        return _fixLockedFormFields(document);
    }
    /// <summary>
    /// Opens are re-exports a pdf. This allows you to be able to edit the form fields
    /// </summary>
    /// <param name="doc">Telerik pdf object</param>
    /// <returns>Filled out pdf</returns>
    private byte[] _fixLockedFormFields(RadFixedDocument doc)
    {
        var updated = _formatProvider.Export(doc);
        var updatedStream = new MemoryStream();
        updatedStream.Write(updated, 0, updated.Length);
        return updatedStream.ToArray();
    }
    /// <summary>
    ///  Maps the fileContent data into the pdf form fields and exports it to a new pdf
    /// </summary>
    /// <param name="stream">Stream of the pdf</param>
    /// <param name="fileContent">JSON string of data to be filled into the pdf</param>
    /// <param name="signature">base64 string of jpg signature</param>
    /// <returns>Filled out pdf</returns>
    public byte[] FillOutPdf(Stream stream, string fileContent, string signature = "")
    {
        var ms = new MemoryStream();
        stream.CopyTo(ms);
        return FillOutPdf(ms.ToArray(), fileContent, signature);
    }
    /// <summary>
    ///  Maps the fileContent data into the pdf form fields and exports it to a new pdf
    /// </summary>
    /// <param name="bytes">Byte array of the pdf</param>
    /// <param name="fileContent">JSON string of data to be filled into the pdf</param>
     /// <param name="signature">base64 string of jpg signature</param>
    /// <returns>Filled out pdf</returns>
    public byte[] FillOutPdf(byte[] bytes, string fileContent, string signature = "")
    {
        var document = _formatProvider.Import(bytes);
        var data = JsonConvert.DeserializeObject<dynamic>(fileContent) ?? new {};
        foreach (var field in document.AcroForm.FormFields)
        {
            switch (field)
            {
                case TextBoxField f:
                {
                    foreach (var fieldWidget in field.Widgets)
                    {
                        fieldWidget.TextProperties.Font = FontsRepository.Courier;
                    }
                    f.Value = data[f.Name] ?? "";
                    break;
                }
                case RadioButtonField c:
                {
                    var a = data[c.Name];
                    // If the data is a string with a value, then set the radio button to that value
                    if (data[c.Name] is JValue {Type: JTokenType.String, Value: { }} v)
                    {
                        var radio = c.Widgets.FirstOrDefault(x => x.Option.Value == v.Value.ToString());
                        c.Value = radio?.Option;
                        foreach (var cWidget in c.Widgets)
                        {
                            cWidget.RecalculateContent();
                        }
                    }
                    break;
                }
                case CheckBoxField b:
                {
                    if (data[b.Name] is JValue {Type: JTokenType.Boolean})
                    {
                        b.IsChecked = data[b.Name];
                    }
                    break;
                }
            }
        }
        // Handles adding a signature if one is passed in
        if (!string.IsNullOrWhiteSpace(signature))
        {
            var signatureData = Convert.FromBase64String(signature);
            var signatureStream = new MemoryStream(signatureData);
            var image = new Image
            {
                ImageSource = new ImageSource(signatureStream)
            };

            var signatureBox = document.AcroForm.FormFields.FirstOrDefault(x => x.Name.ToLower() == "signature");
            if (signatureBox is { })
            {
                var rect = signatureBox.Widgets.First().Rect;
                var ratio =  rect.Width / image.Height  ;
                var newHeight =  (image.ImageSource.Height / ratio);
                var newWidth = (image.ImageSource.Width / ratio);
                image.Height = newHeight;
                image.Width = newWidth;
                var page = document.Pages[0];
                image.Position.Translate(rect.X, rect.Y-(image.Height-rect.Height));
                page.Content.Add(image);
            }
        }
        var updated = _formatProvider.Export(document);
        return updated;
    }
    /// <summary>
    /// Creates a json string of all the data in a pdf
    /// </summary>
    /// <param name="stream">Stream of the pdf</param>
    /// <returns>JSON representation of the data filled in the pdf</returns>
    public string Parse(Stream stream)
    {
        var document = _formatProvider.Import(stream);
        return _parse(document);
    }
    /// <summary>
    /// Creates a json string of all the data in a pdf
    /// </summary>
    /// <param name="stream">Stream of the pdf</param>
    /// <returns>JSON representation of the data filled in the pdf</returns>
    public string Parse(byte[] stream)
    {
        var document = _formatProvider.Import(stream);
        return _parse(document);
    }
    /// <summary>
    /// Creates a json string of all the data in a pdf
    /// </summary>
    /// <param name="doc">Telerik pdf object</param>
    /// <returns></returns>
    private static string _parse(RadFixedDocument doc)
    {
        dynamic dynamic = new ExpandoObject();
        foreach (var field in doc.AcroForm.FormFields)
        {
            var v = field switch
            {
                TextBoxField f => f.Value,
                CheckBoxField b => b.IsChecked.ToString(),
                RadioButtonField c => c.Value.ToString(),
                _ => ""
            };
            ((IDictionary<string, object>)dynamic).Add(field.Name, v);
        }
        return JsonConvert.SerializeObject(dynamic);
    }
    /// <summary>
    /// Validates the pdf based on the parameters being passed in 
    /// </summary>
    /// <param name="pdf">Stream of the pdf</param>
    /// <param name="parameters">Parameters from Trojan DB</param>
    /// <returns>The validity of the form values, based on the regex's in parameters.</returns>
    public bool ValidateFormFields(Stream pdf, IEnumerable<Parameter> parameters)
    {
        var doc = _formatProvider.Import(pdf);
        return _validateFormFields(doc, parameters);
    }
    /// <summary>
    /// Validates the pdf based on the parameters being passed in 
    /// </summary>
    /// <param name="pdf">Byte array of the pdf</param>
    /// <param name="parameters">Parameters from Trojan DB</param>
    /// <returns>The validity of the form values, based on the regex's in parameters.</returns>
    public bool ValidateFormFields(byte[] pdf, IEnumerable<Parameter> parameters)
    {
        var doc = _formatProvider.Import(pdf);
        return _validateFormFields(doc, parameters);
    }
    /// <summary>
    /// Validates the pdf based on the parameters being passed in 
    /// </summary>
    /// <param name="doc">Telerik object of the pdf</param>
    /// <param name="parameters">Parameters from Trojan DB</param>
    /// <returns>he validity of the form values, based on the regex's in parameters.</returns>
    /// <exception cref="RegexValidationException"></exception>
    private static bool _validateFormFields(RadFixedDocument doc, IEnumerable<Parameter> parameters)
    {
        var formFields = doc.AcroForm.FormFields;
        foreach (var parameter in parameters)
        {
            var regex = new Regex(parameter.RegexString);
            switch (formFields[parameter.Name])
            {
                case TextBoxField f:
                {
                    var value = f.Value;
                    if (parameter.RegexType != RegexType.Text)
                    {
                        throw new RegexValidationException("Invalid regex type. Expected Text, got " + parameter.RegexType);
                    }
                    if (!regex.IsMatch(value))
                    {
                        return false;
                    }

                    break;
                }
                case ComboBoxField c:
                {
                    var value = c.Value.Value;
                    if (parameter.RegexType != RegexType.List)
                    {
                        throw new RegexValidationException("Invalid regex type. Expected List, got " + parameter.RegexType);
                    }
                    if (!regex.IsMatch(value))
                    {
                        return false;
                    }
                    break;
                }
                case CheckBoxField b:
                {
                    var value = b.IsChecked.ToString();
                    if (parameter.RegexType != RegexType.Bool)
                    {
                        throw new RegexValidationException("Invalid regex type. Expected Bool, got " + parameter.RegexType);
                    }
                    if(!regex.IsMatch(value))
                    {
                        return false;
                    }
                    break;
                }
            }
        }
        return true;
    }
    /// <summary>
    /// Adds a signature to the pdf
    /// </summary>
    /// <param name="pdf">Stream of pdf</param>
    /// <param name="signature">Base 64 stream of jpg signature</param>
    /// <param name="parameter">Parameter that indicates the name of the signature form field</param>
    /// <returns>Signed pdf</returns>
    public byte[] SignDocument(Stream pdf, string signature, Parameter parameter)
    {
        var doc = _formatProvider.Import(pdf);
        return _signDocument(doc, signature, parameter.Name);
    }
    /// <summary>
    /// Adds a signature to the pdf
    /// </summary>
    /// <param name="pdf">Byte array of pdf</param>
    /// <param name="signature">Base 64 stream of jpg signature</param>
    /// <param name="parameter">Parameter that indicates the name of the signature form field</param>
    /// <returns>Signed pdf</returns>
    public byte[] SignDocument(byte[] pdf, string signature, Parameter parameter)
    {
        var doc = _formatProvider.Import(pdf);
        return _signDocument(doc,signature, parameter.Name);
    }
    /// <summary>
    /// Adds a signature to the pdf
    /// </summary>
    /// <param name="pdf">Stream of pdf</param>
    /// <param name="signature">Base 64 stream of jpg signature</param>
    /// <param name="signatureFieldName">Optional name of field name for the signature, if not just signature</param>
    /// <returns>Signed pdf</returns>
    public byte[] SignDocument(Stream pdf, string signature, string signatureFieldName = "")
    {
        var doc = _formatProvider.Import(pdf);
        return _signDocument(doc, signature, signatureFieldName);
    }
    /// <summary>
    /// Adds a signature to the pdf
    /// </summary>
    /// <param name="pdf">Byte array of pdf</param>
    /// <param name="signature">Base 64 stream of jpg signature</param>
    /// <param name="signatureFieldName">Optional name of field name for the signature, if not just signature</param>
    /// <returns>Signed pdf</returns>
    public byte[] SignDocument(byte[] pdf, string signature, string signatureFieldName = "")
    {
        var doc = _formatProvider.Import(pdf);
        return _signDocument(doc, signature, signatureFieldName);
    }
    /// <summary>
    /// Adds a signature to the pdf
    /// </summary>
    /// <param name="doc">Telerik object of pdf</param>
    /// <param name="signature">Base 64 stream of jpg signature</param>
    /// <param name="signatureFieldName">Optional name of field name for the signature, if not just signature</param>
    /// <returns>Signed pdf</returns>
    private byte[] _signDocument(RadFixedDocument doc, string signature, string signatureFieldName = "")
    {
        var signatureData = Convert.FromBase64String(signature);
        var signatureStream = new MemoryStream(signatureData);
        var image = new Image
        {
            ImageSource = new ImageSource(signatureStream)
        };
        var signatureBoxName = string.IsNullOrWhiteSpace(signatureFieldName) ? "signature" : signatureFieldName.ToLower();    
        var signatureBox = doc.AcroForm.FormFields.FirstOrDefault(x => x.Name.ToLower() == signatureBoxName);
        if (signatureBox is { })
        {
            var rect = signatureBox.Widgets.First().Rect;
            var ratio =  rect.Width / image.Height  ;
            var newHeight =  (image.ImageSource.Height / ratio);
            var newWidth = (image.ImageSource.Width / ratio);
            image.Height = newHeight;
            image.Width = newWidth;
            var page = doc.Pages[0];
            image.Position.Translate(rect.X, rect.Y-(image.Height-rect.Height));
            page.Content.Add(image);
        }
        else
        {
            throw new SignatureBoxNotFoundException("Signature box not found");
        }
        return _formatProvider.Export(doc);
    }
}
