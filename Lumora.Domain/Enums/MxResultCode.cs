namespace Lumora.Domain.Enums
{
    public enum MxResultCode
    {
        GeneralException = -9,
        SenderRejected = -3,
        HeloFailed = -2,
        ConnectionFailed = -1,

        Ok = 250,
        OkButTooManyEmailsGot = 450,
        RetryLater = 451,
        OkButMailboxFull = 452,
        NotFound = 550,
    }
}
