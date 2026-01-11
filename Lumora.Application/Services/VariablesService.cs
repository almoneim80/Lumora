namespace Lumora.Application.Services
{
    public class VariablesService(IEnumerable<IVariablesProvider> variableProviders) : IVariablesService
    {
        public Dictionary<string, string> GetVariables(string language)
        {
            var variables = new Dictionary<string, string>();

            foreach (var provider in variableProviders)
            {
                variables.AddRangeIfNotExists(provider.GetVariables(language));
            }

            return variables;
        }
    }
}
