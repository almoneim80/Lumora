namespace Lumora.Application.Exceptions
{
    public class InvalidModelStateException : Exception
    {
        // نستخدم Dictionary بسيط لتخزين الأخطاء: اسم الحقل وقائمة رسائل الخطأ
        public IDictionary<string, string[]> Errors { get; }
        public InvalidModelStateException(IDictionary<string, string[]> errors)
        {
            Errors = errors;
        }
    }
}
