using System.Globalization;
using System.Text;
using CsvHelper;
using Lumora.Application.Interfaces.ImportIntf;
using OfficeOpenXml;

namespace Lumora.Infrastructure.Services.ExternalServices.Files
{
    public class ImportFileReader : IImportFileReader
    {
        public ImportFileReader()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Your Name");
        }

        public async Task<List<TI>> ReadFileAsync<TI>(Stream fileStream, string fileExtension) where TI : new()
        {
            if (fileStream.CanSeek) fileStream.Position = 0;

            if (fileExtension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
                return await ReadCsv<TI>(fileStream);

            if (fileExtension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                fileExtension.Equals(".xls", StringComparison.OrdinalIgnoreCase))
                return await ReadExcel<TI>(fileStream);

            throw new ArgumentException("Unsupported file format");
        }

        private async Task<List<TI>> ReadCsv<TI>(Stream stream) where TI : new()
        {
            var records = new List<TI>();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            await foreach (var record in csv.GetRecordsAsync<TI>())
            {
                records.Add(record);
            }
            return records;
        }

        private Task<List<TI>> ReadExcel<TI>(Stream stream) where TI : new()
        {
            var records = new List<TI>();
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                if (worksheet?.Dimension == null) return Task.FromResult(records);

                var rowCount = worksheet.Dimension.Rows;
                var properties = typeof(TI).GetProperties();

                for (int row = 2; row <= rowCount; row++)
                {
                    var record = new TI();
                    foreach (var prop in properties)
                    {
                        var colIndex = GetColumnIndex(worksheet, prop.Name);
                        if (colIndex > 0)
                        {
                            var cellValue = worksheet.Cells[row, colIndex].Text;
                            if (!string.IsNullOrEmpty(cellValue))
                            {
                                try
                                {
                                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                                    prop.SetValue(record, Convert.ChangeType(cellValue, targetType));
                                }
                                catch
                                { /* Skip conversion errors */
                                }
                            }
                        }
                    }
                    records.Add(record);
                }
            }
            return Task.FromResult(records);
        }

        private int GetColumnIndex(ExcelWorksheet sheet, string name)
        {
            if (sheet.Dimension == null) return 0;
            for (int col = 1; col <= sheet.Dimension.Columns; col++)
            {
                if (sheet.Cells[1, col].Text.Trim().Equals(name, StringComparison.OrdinalIgnoreCase))
                    return col;
            }
            return 0;
        }

        public async Task<byte[]> GenerateTemplateAsync<TDto>(string format) where TDto : class
        {
            if (format.ToLower().Contains("csv"))
            {
                using var memoryStream = new MemoryStream();
                using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(true)))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    var properties = typeof(TDto).GetProperties();
                    foreach (var property in properties)
                    {
                        csv.WriteField(property.Name);
                    }
                    await csv.NextRecordAsync();
                }
                return memoryStream.ToArray();
            }
            else
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Template");
                    var properties = typeof(TDto).GetProperties();
                    for (int i = 0; i < properties.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = properties[i].Name;
                    }
                    return await package.GetAsByteArrayAsync();
                }
            }
        }
    }
}
