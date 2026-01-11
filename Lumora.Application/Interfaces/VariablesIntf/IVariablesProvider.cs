namespace Lumora.Application.Interfaces.VariablesIntf
{
    public interface IVariablesProvider
    {
        public Dictionary<string, string> GetVariables(string language);
    }
}
