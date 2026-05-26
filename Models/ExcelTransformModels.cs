using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace LandiGlobalTemplate.Models
{
    public enum ExcelImportType
    {
        LAN,
        STOCK,
        SELLSOUT
    }

    public class ProductMappingEntry
    {
        public string ItemNo { get; set; } = string.Empty;
        public string ProductFamily { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
    }

    public class ExcelTransformResult
    {
        public string ImportTypeLabel { get; set; } = string.Empty;
        public List<string> Headers { get; set; } = new();
        public List<List<string>> Rows { get; set; } = new();
        public int TotalRows { get; set; }
        public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    }

    public class ExcelTransformViewModel
    {
        public ExcelImportType ImportType { get; set; } = ExcelImportType.LAN;
        public IFormFile? File { get; set; }
        public IFormFile? MappingFile { get; set; }
        public ExcelTransformResult? Result { get; set; }
    }
}
