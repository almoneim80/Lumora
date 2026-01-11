namespace Lumora.Application.Interfaces.VariablesIntf
{
    public interface IVariablesService
    {
        public Dictionary<string, string> GetVariables(string language);
    }
}
