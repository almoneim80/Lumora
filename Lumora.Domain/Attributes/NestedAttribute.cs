namespace Lumora.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NestedAttribute : Attribute
    {
        // يمكن إضافة الخصائص التي تحتاجها فقط من المكتبة الأصلية
        public bool IncludeInParent { get; set; }
        public bool IncludeInRoot { get; set; }
    }
}
