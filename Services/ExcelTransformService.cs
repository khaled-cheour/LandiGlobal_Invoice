using LandiGlobalTemplate.Models;
using OfficeOpenXml;
using System.Globalization;

namespace LandiGlobalTemplate.Services
{
    public class ExcelTransformService
    {
        private readonly ILogger<ExcelTransformService> _logger;

        static ExcelTransformService()
        {
            ExcelPackage.License.SetNonCommercialOrganization("LandiGlobalTemplate");
        }

        public ExcelTransformService(ILogger<ExcelTransformService> logger)
        {
            _logger = logger;
        }

        public async Task<ExcelTransformResult> TransformAsync(ExcelImportType importType, IFormFile file, IFormFile? mappingFile)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Le fichier Excel est requis.");
            }

            var mappings = await LoadProductMappingsAsync(mappingFile);
            var importTypeLabel = importType switch
            {
                ExcelImportType.LAN => "LAN",
                ExcelImportType.STOCK => "STOCK",
                ExcelImportType.SELLSOUT => "SELLS OUT",
                _ => "Fichier"
            };

            using var inputStream = new MemoryStream();
            await file.CopyToAsync(inputStream);
            inputStream.Position = 0;

            using var package = new ExcelPackage(inputStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                throw new InvalidOperationException("Aucune feuille Excel valide n'a été trouvée.");
            }

            var sourceHeaders = ReadHeaders(worksheet);
            if (!sourceHeaders.Any())
            {
                throw new InvalidOperationException("Le fichier Excel ne contient pas d'en-têtes de colonnes.");
            }

            var originalRows = ReadRows(worksheet, sourceHeaders.Count);
            var outputHeaders = BuildOutputHeaders(importType, sourceHeaders);
            var transformedRows = BuildTransformedRows(importType, sourceHeaders, originalRows, mappings);

            using var outputPackage = new ExcelPackage();
            var outputSheet = outputPackage.Workbook.Worksheets.Add("Transformed");

            for (int col = 0; col < outputHeaders.Count; col++)
            {
                outputSheet.Cells[1, col + 1].Value = outputHeaders[col];
                outputSheet.Cells[1, col + 1].Style.Font.Bold = true;
            }

            for (int row = 0; row < transformedRows.Count; row++)
            {
                for (int col = 0; col < transformedRows[row].Count; col++)
                {
                    outputSheet.Cells[row + 2, col + 1].Value = transformedRows[row][col];
                }
            }

            outputSheet.Cells[outputSheet.Dimension.Address].AutoFitColumns();

            using var outputStream = new MemoryStream();
            outputPackage.SaveAs(outputStream);
            var outputBytes = outputStream.ToArray();

            return new ExcelTransformResult
            {
                ImportTypeLabel = importTypeLabel,
                Headers = outputHeaders,
                Rows = transformedRows.Take(50).ToList(),
                TotalRows = transformedRows.Count,
                FileBytes = outputBytes
            };
        }

        private async Task<Dictionary<string, ProductMappingEntry>> LoadProductMappingsAsync(IFormFile? mappingFile)
        {
            var mappings = new Dictionary<string, ProductMappingEntry>(StringComparer.OrdinalIgnoreCase);
            if (mappingFile != null && mappingFile.Length > 0)
            {
                try
                {
                    using var stream = new MemoryStream();
                    await mappingFile.CopyToAsync(stream);
                    stream.Position = 0;
                    using var package = new ExcelPackage(stream);
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet != null)
                    {
                        var headers = ReadHeaders(worksheet);
                        var itemCol = GetHeaderIndex(headers, new[] { "Item No", "Item No.", "SKU" });
                        var familyCol = GetHeaderIndex(headers, new[] { "Product Family", "Family" });
                        var nameCol = GetHeaderIndex(headers, new[] { "Family Name", "Product Name", "Family" });

                        if (itemCol >= 0)
                        {
                            var rows = ReadRows(worksheet, headers.Count);
                            foreach (var row in rows)
                            {
                                var itemNo = GetCellValue(row, itemCol);
                                if (string.IsNullOrWhiteSpace(itemNo))
                                {
                                    continue;
                                }

                                var productFamily = familyCol >= 0 ? GetCellValue(row, familyCol) : string.Empty;
                                var familyName = nameCol >= 0 ? GetCellValue(row, nameCol) : string.Empty;

                                if (!mappings.ContainsKey(itemNo))
                                {
                                    mappings[itemNo] = new ProductMappingEntry
                                    {
                                        ItemNo = itemNo,
                                        ProductFamily = productFamily,
                                        FamilyName = familyName
                                    };
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Impossible de lire le fichier de correspondance. Le service utilisera le mapping statique.");
                }
            }

            if (!mappings.Any())
            {
                foreach (var item in GetDefaultProductMappings())
                {
                    if (!mappings.ContainsKey(item.ItemNo))
                    {
                        mappings[item.ItemNo] = item;
                    }
                }
            }

            return mappings;
        }

        private List<ProductMappingEntry> GetDefaultProductMappings()
        {
            return new List<ProductMappingEntry>
            {
                new ProductMappingEntry { ItemNo = "YB01001731", ProductFamily = "Windows License", FamilyName = "Windows License" },
                new ProductMappingEntry { ItemNo = "600B1-00--", ProductFamily = "Warranty extension", FamilyName = "Warranty extension" },
                new ProductMappingEntry { ItemNo = "8561S-00--", ProductFamily = "ECR Android", FamilyName = "C20Pro" },
                new ProductMappingEntry { ItemNo = "53106-00--", ProductFamily = "Mobility", FamilyName = "M20" },
                new ProductMappingEntry { ItemNo = "8561R-00--", ProductFamily = "ECR Android", FamilyName = "C20Pro" },
                new ProductMappingEntry { ItemNo = "WX01000012", ProductFamily = "Acc", FamilyName = "C20/Cx20 Acc" },
                new ProductMappingEntry { ItemNo = "WX01000013", ProductFamily = "Acc", FamilyName = "C20/Cx20 Acc" },
                new ProductMappingEntry { ItemNo = "85304-00--", ProductFamily = "ECR Android", FamilyName = "C20Lite" },
                new ProductMappingEntry { ItemNo = "PP01000186", ProductFamily = "Acc", FamilyName = "Cx20 Acc" },
                new ProductMappingEntry { ItemNo = "86201-00--", ProductFamily = "ECR Windows", FamilyName = "Cx20SE" },
                new ProductMappingEntry { ItemNo = "86305-00--", ProductFamily = "ECR Android", FamilyName = "C20DS" },
                new ProductMappingEntry { ItemNo = "72401-00--", ProductFamily = "Acc", FamilyName = "M20SE/M20 Acc" },
                new ProductMappingEntry { ItemNo = "85801-00--", ProductFamily = "ECR Windows", FamilyName = "Cx20" },
                new ProductMappingEntry { ItemNo = "53009-00--", ProductFamily = "Mobility", FamilyName = "M20SE" },
                new ProductMappingEntry { ItemNo = "6009U-00--", ProductFamily = "Landi Connect", FamilyName = "Landi Connect" }
            };
        }

        private List<string> ReadHeaders(ExcelWorksheet worksheet)
        {
            var headers = new List<string>();
            var columnCount = worksheet.Dimension?.Columns ?? 0;
            for (int col = 1; col <= columnCount; col++)
            {
                var text = worksheet.Cells[1, col].Text?.Trim();
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                headers.Add(text);
            }

            return headers;
        }

        private List<List<string>> ReadRows(ExcelWorksheet worksheet, int columnCount)
        {
            var rows = new List<List<string>>();
            var rowCount = worksheet.Dimension?.Rows ?? 0;

            for (int row = 2; row <= rowCount; row++)
            {
                var rowValues = new List<string>();
                var isEmpty = true;
                for (int col = 1; col <= columnCount; col++)
                {
                    var value = GetCellText(worksheet.Cells[row, col]);
                    rowValues.Add(value);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        isEmpty = false;
                    }
                }

                if (!isEmpty)
                {
                    rows.Add(rowValues);
                }
            }

            return rows;
        }

        private int GetHeaderIndex(List<string> headers, string[] candidates)
        {
            for (int i = 0; i < headers.Count; i++)
            {
                if (candidates.Any(candidate => string.Equals(headers[i], candidate, StringComparison.OrdinalIgnoreCase)))
                {
                    return i;
                }
            }

            return -1;
        }

        private List<List<string>> BuildTransformedRows(ExcelImportType importType, List<string> headers, List<List<string>> rows, Dictionary<string, ProductMappingEntry> mappings)
        {
            var itemColIndex = GetHeaderIndex(headers, new[] { "Item No", "Item No.", "SKU" });
            var dateColumnName = importType switch
            {
                ExcelImportType.LAN => "Order Date",
                ExcelImportType.STOCK => "Reporting Date",
                ExcelImportType.SELLSOUT => "Posted Date",
                _ => string.Empty
            };
            var alternativeDateName = importType == ExcelImportType.SELLSOUT ? "Ship Date" : string.Empty;
            var dateColIndex = GetHeaderIndex(headers, new[] { dateColumnName, alternativeDateName }.Where(s => !string.IsNullOrEmpty(s)).ToArray());

            var transformedRows = new List<List<string>>();
            var nWeekValue = GetSystemWeekNumber();
            foreach (var row in rows)
            {
                var itemNo = itemColIndex >= 0 && itemColIndex < row.Count ? row[itemColIndex] : string.Empty;
                mappings.TryGetValue(itemNo, out var mapping);
                var familyNameValue = mapping?.ProductFamily ?? string.Empty;
                var productNameValue = mapping?.FamilyName ?? string.Empty;

                var transformedRow = new List<string> { nWeekValue };
                for (int i = 0; i < headers.Count; i++)
                {
                    transformedRow.Add(i < row.Count ? row[i] : string.Empty);
                    if (ShouldInsertMappingColumns(importType, headers[i]))
                    {
                        transformedRow.Add(familyNameValue);
                        transformedRow.Add(productNameValue);
                    }
                }

                transformedRows.Add(transformedRow);
            }

            return transformedRows;
        }

        private List<string> BuildOutputHeaders(ExcelImportType importType, List<string> sourceHeaders)
        {
            var outputHeaders = new List<string> { "N.Week" };

            foreach (var header in sourceHeaders)
            {
                outputHeaders.Add(header);
                if (ShouldInsertMappingColumns(importType, header))
                {
                    outputHeaders.Add("family name");
                    outputHeaders.Add("product name");
                }
            }

            return outputHeaders;
        }

        private bool ShouldInsertMappingColumns(ExcelImportType importType, string header)
        {
            if (importType == ExcelImportType.LAN || importType == ExcelImportType.SELLSOUT)
            {
                return string.Equals(header, "Item No", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(header, "Item No.", StringComparison.OrdinalIgnoreCase);
            }

            if (importType == ExcelImportType.STOCK)
            {
                return string.Equals(header, "Product Description", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private string GetCellValue(List<string> row, int index)
        {
            if (index < 0 || index >= row.Count)
            {
                return string.Empty;
            }

            return row[index]?.Trim() ?? string.Empty;
        }

        private bool TryGetRowDateValue(List<string> headers, List<string> row, out DateTime date)
        {
            var dateColumnCandidates = new[]
            {
                "Order Date",
                "Shipment Date",
                "Ship Date",
                "Reporting Date",
                "Posted Date",
                "Delivery Date",
                "Date"
            };

            foreach (var columnName in dateColumnCandidates)
            {
                var index = GetHeaderIndex(headers, new[] { columnName });
                if (index >= 0 && index < row.Count && TryParseExcelDate(row[index], out date))
                {
                    return true;
                }
            }

            for (int i = 0; i < headers.Count && i < row.Count; i++)
            {
                if (TryParseExcelDate(row[i], out date))
                {
                    return true;
                }
            }

            date = default;
            return false;
        }

        private string GetCellText(ExcelRange cell)
        {
            if (cell.Value is DateTime date)
            {
                return date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            if (cell.Value is double numericValue && IsDateFormatted(cell.Style.Numberformat.Format))
            {
                var parsedDate = DateTime.FromOADate(numericValue);
                return parsedDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            return cell.Text?.Trim() ?? string.Empty;
        }

        private bool IsDateFormatted(string? format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return false;
            }

            var normalizedFormat = format.ToLowerInvariant();
            return normalizedFormat.Contains("y")
                || normalizedFormat.Contains("m")
                || normalizedFormat.Contains("d")
                || normalizedFormat.Contains("h")
                || normalizedFormat.Contains("s");
        }

        private bool TryParseExcelDate(string? value, out DateTime date)
        {
            date = default;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var trimmedValue = value.Trim();
            if (double.TryParse(trimmedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var numericValue)
                && numericValue > 31
                && numericValue <= 2958465)
            {
                date = DateTime.FromOADate(numericValue);
                return true;
            }

            var patterns = new[]
            {
                "d/M/yyyy",
                "dd/MM/yyyy",
                "d/MM/yyyy",
                "dd/M/yyyy",
                "M/d/yyyy",
                "MM/dd/yyyy",
                "M/dd/yyyy",
                "MM/d/yyyy",
                "yyyy-MM-dd",
                "yyyy-M-d",
                "dd/MM/yy",
                "d/M/yy",
                "MM/dd/yy",
                "M/d/yy"
            };

            if (DateTime.TryParseExact(trimmedValue, patterns, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return true;
            }

            if (DateTime.TryParseExact(trimmedValue, patterns, CultureInfo.GetCultureInfo("fr-FR"), DateTimeStyles.None, out date))
            {
                return true;
            }

            return false;
        }

        private string GetIsoWeekString(DateTime date)
        {
            var weekNumber = ISOWeek.GetWeekOfYear(date);
            return $"W{weekNumber:00}";
        }

        private string GetWeekDayString(DateTime date)
        {
            return date.ToString("dddd", CultureInfo.GetCultureInfo("fr-FR"));
        }

        private string GetSystemWeekNumber()
        {
            var today = DateTime.Today;
            var currentWeek = WeekRanges().FirstOrDefault(w => today >= w.Start && today <= w.End);
            return currentWeek.WeekNumber == 0 ? ISOWeek.GetWeekOfYear(today).ToString() : currentWeek.WeekNumber.ToString();
        }

        private IEnumerable<(int WeekNumber, DateTime Start, DateTime End)> WeekRanges()
        {
            yield return (1, new DateTime(2025, 12, 29), new DateTime(2026, 1, 4));
            yield return (2, new DateTime(2026, 1, 5), new DateTime(2026, 1, 11));
            yield return (3, new DateTime(2026, 1, 12), new DateTime(2026, 1, 18));
            yield return (4, new DateTime(2026, 1, 19), new DateTime(2026, 1, 25));
            yield return (5, new DateTime(2026, 1, 26), new DateTime(2026, 2, 1));
            yield return (6, new DateTime(2026, 2, 2), new DateTime(2026, 2, 8));
            yield return (7, new DateTime(2026, 2, 9), new DateTime(2026, 2, 15));
            yield return (8, new DateTime(2026, 2, 16), new DateTime(2026, 2, 22));
            yield return (9, new DateTime(2026, 2, 23), new DateTime(2026, 3, 1));
            yield return (10, new DateTime(2026, 3, 2), new DateTime(2026, 3, 8));
            yield return (11, new DateTime(2026, 3, 9), new DateTime(2026, 3, 15));
            yield return (12, new DateTime(2026, 3, 16), new DateTime(2026, 3, 22));
            yield return (13, new DateTime(2026, 3, 23), new DateTime(2026, 3, 29));
            yield return (14, new DateTime(2026, 3, 30), new DateTime(2026, 4, 5));
            yield return (15, new DateTime(2026, 4, 6), new DateTime(2026, 4, 12));
            yield return (16, new DateTime(2026, 4, 13), new DateTime(2026, 4, 19));
            yield return (17, new DateTime(2026, 4, 20), new DateTime(2026, 4, 26));
            yield return (18, new DateTime(2026, 4, 27), new DateTime(2026, 5, 3));
            yield return (19, new DateTime(2026, 5, 4), new DateTime(2026, 5, 10));
            yield return (20, new DateTime(2026, 5, 11), new DateTime(2026, 5, 17));
            yield return (21, new DateTime(2026, 5, 18), new DateTime(2026, 5, 24));
            yield return (22, new DateTime(2026, 5, 25), new DateTime(2026, 5, 31));
            yield return (23, new DateTime(2026, 6, 1), new DateTime(2026, 6, 7));
            yield return (24, new DateTime(2026, 6, 8), new DateTime(2026, 6, 14));
            yield return (25, new DateTime(2026, 6, 15), new DateTime(2026, 6, 21));
            yield return (26, new DateTime(2026, 6, 22), new DateTime(2026, 6, 28));
            yield return (27, new DateTime(2026, 6, 29), new DateTime(2026, 7, 5));
            yield return (28, new DateTime(2026, 7, 6), new DateTime(2026, 7, 12));
            yield return (29, new DateTime(2026, 7, 13), new DateTime(2026, 7, 19));
            yield return (30, new DateTime(2026, 7, 20), new DateTime(2026, 7, 26));
            yield return (31, new DateTime(2026, 7, 27), new DateTime(2026, 8, 2));
            yield return (32, new DateTime(2026, 8, 3), new DateTime(2026, 8, 9));
            yield return (33, new DateTime(2026, 8, 10), new DateTime(2026, 8, 16));
            yield return (34, new DateTime(2026, 8, 17), new DateTime(2026, 8, 23));
            yield return (35, new DateTime(2026, 8, 24), new DateTime(2026, 8, 30));
            yield return (36, new DateTime(2026, 8, 31), new DateTime(2026, 9, 6));
            yield return (37, new DateTime(2026, 9, 7), new DateTime(2026, 9, 13));
            yield return (38, new DateTime(2026, 9, 14), new DateTime(2026, 9, 20));
            yield return (39, new DateTime(2026, 9, 21), new DateTime(2026, 9, 27));
            yield return (40, new DateTime(2026, 9, 28), new DateTime(2026, 10, 4));
            yield return (41, new DateTime(2026, 10, 5), new DateTime(2026, 10, 11));
            yield return (42, new DateTime(2026, 10, 12), new DateTime(2026, 10, 18));
            yield return (43, new DateTime(2026, 10, 19), new DateTime(2026, 10, 25));
            yield return (44, new DateTime(2026, 10, 26), new DateTime(2026, 11, 1));
            yield return (45, new DateTime(2026, 11, 2), new DateTime(2026, 11, 8));
            yield return (46, new DateTime(2026, 11, 9), new DateTime(2026, 11, 15));
            yield return (47, new DateTime(2026, 11, 16), new DateTime(2026, 11, 22));
            yield return (48, new DateTime(2026, 11, 23), new DateTime(2026, 11, 29));
            yield return (49, new DateTime(2026, 11, 30), new DateTime(2026, 12, 6));
            yield return (50, new DateTime(2026, 12, 7), new DateTime(2026, 12, 13));
            yield return (51, new DateTime(2026, 12, 14), new DateTime(2026, 12, 20));
            yield return (52, new DateTime(2026, 12, 21), new DateTime(2026, 12, 27));
            yield return (53, new DateTime(2026, 12, 28), new DateTime(2027, 1, 3));
        }
    }
}
