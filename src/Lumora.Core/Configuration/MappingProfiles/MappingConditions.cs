namespace Lumora.Configuration.MappingProfiles
{
    public static class MappingConditions
    {
        public static bool PropertyNeedsMapping(object source, object target, object sourceValue, object targetValue)
        {
            if (sourceValue is null or (object)"")
            {
                return false;
            }

            var defaultValue = sourceValue.GetType().IsValueType ? Activator.CreateInstance(sourceValue.GetType()) : null;
            return !sourceValue.Equals(defaultValue);
        }
    }
}
