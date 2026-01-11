namespace Lumora.Application.DTOs
{
    public class OperationResult
    {
        public bool Succeeded { get; private set; }
        public bool IsLockedOut { get; private set; }
        public IEnumerable<string> Errors { get; private set; }

        protected OperationResult(bool succeeded, IEnumerable<string> errors, bool isLockedOut = false)
        {
            Succeeded = succeeded;
            Errors = errors ?? new List<string>();
            IsLockedOut = isLockedOut;
        }

        public static OperationResult Success() => new OperationResult(true, Enumerable.Empty<string>());

        // التحميل الزائد (Overload) لمعالجة فشل كلمة المرور العادي
        public static OperationResult Failed(params string[] errors) => new OperationResult(false, errors);

        // التحميل الزائد (Overload) لمعالجة حالة القفل (Lockout)
        public static OperationResult FailedLockedOut(string message = "Account is locked out.")
            => new OperationResult(false, new List<string> { message }, isLockedOut: true);
    }
}
