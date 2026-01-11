namespace Lumora.Application.DTOs.Import
{
    public class ImportResult
    {
        public int Added { get; set; }

        public int Updated { get; set; }

        public int Failed { get; set; }

        public int Skipped { get; set; }

        public List<ImportError>? Errors { get; set; }
        public List<string>? Messages { get; set; }

        public void AddError(int row, string message)
        {
            Failed++;
            Errors ??= new List<ImportError>();

            Errors.Add(new ImportError
            {
                Row = row,
                Message = message,
            });
        }

        public void AddMessage(string message)
        {
            Messages ??= new List<string>();
            Messages.Add(message);
        }
    }
}
