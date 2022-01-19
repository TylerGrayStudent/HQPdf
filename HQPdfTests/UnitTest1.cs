using System;
using System.IO;
using HQPdf;
using Xunit;
using Xunit.Abstractions;

namespace HQPdfTests;

public class UnitTest1
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void ShouldParseCorrectly()
    {
        var tool = new PdfTool();
        var pdf = File.ReadAllBytes("./TestPdfFiles/filledoutform.pdf");
        var result = tool.Parse(pdf);
        Assert.Equal("{\"Given Name Text Box\":\"Bob\",\"Family Name Text Box\":\"Bobby\",\"House nr Text Box\":\"123\",\"Address 2 Text Box\":\"﻿\",\"Postcode Text Box\":\"29414\",\"Country Combo Box\":\"\",\"Height Formatted Field\":\"123\",\"City Text Box\":\"Charleston\",\"Driving License Check Box\":\"True\",\"Favourite Colour List Box\":\"\",\"Language 1 Check Box\":\"True\",\"Language 2 Check Box\":\"True\",\"Language 3 Check Box\":\"False\",\"Language 4 Check Box\":\"False\",\"Language 5 Check Box\":\"False\",\"Gender List Box\":\"\",\"Address 1 Text Box\":\"Main St\"}", result);
    }
    
    
}