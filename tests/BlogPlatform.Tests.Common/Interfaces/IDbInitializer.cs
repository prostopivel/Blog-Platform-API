namespace BlogPlatform.Tests.Common.Interfaces
{
    public interface IDbInitializer
    {
        Task ExecuteInitScript(string connectionString, string initScriptName);
    }
}
