namespace Lumora.Application.DTOs
{
    public class MxCheckResultDto
    {
        public MxCheckResultDto(string? mxhost, string? email, int code, string? value, bool success)
        {
            TimeStamp = DateTime.UtcNow;
            MxHost = mxhost;
            Email = email;
            StatusCode = (MxResultCode)code;
            StatusValue = value;
            Successfull = success;
        }

        public MxCheckResultDto(string? mxhost, string? email, MxResultCode code, string? value, bool success)
        {
            TimeStamp = DateTime.UtcNow;
            MxHost = mxhost;
            Email = email;
            StatusCode = code;
            StatusValue = value;
            Successfull = success;
        }

        public DateTime TimeStamp { get; set; }

        public string? MxHost { get; set; }

        public string? Email { get; set; }

        public MxResultCode StatusCode { get; set; }

        public string? StatusValue { get; set; }

        public bool Successfull { get; set; }

        public override string ToString()
        {
            return $"[{(Successfull ? "ok" : "FAIL")}] {MxHost} - {StatusCode} :: {StatusValue}";
        }
    }
}
