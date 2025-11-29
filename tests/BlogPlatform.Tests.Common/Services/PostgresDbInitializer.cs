using BlogPlatform.Tests.Common.Interfaces;
using Npgsql;

namespace BlogPlatform.Tests.Common.Services
{
    public class PostgresDbInitializer : IDbInitializer
    {
        private static readonly string _initScriptsPath = Path.Combine("docker", "init-scripts");

        public async Task ExecuteInitScript(string connectionString, string initScriptName)
        {
            var scriptPath = GetInitScriptPath(initScriptName);
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"Init script not found: {scriptPath}");
            }

            var scriptContent = await File.ReadAllTextAsync(scriptPath);

            await using var dataSource = NpgsqlDataSource.Create(connectionString);
            await using var command = dataSource.CreateCommand(scriptContent);

            await command.ExecuteNonQueryAsync();
        }

        private static string GetInitScriptPath(string initScriptName)
        {
            var testsDirectory = Directory.GetCurrentDirectory();
            var initScriptsBaseDirectory = _initScriptsPath.Split(Path.DirectorySeparatorChar)
                .First() ?? throw new DirectoryNotFoundException("Init scripts directory not found");

            while (true)
            {
                if (Directory.Exists(Path.Combine(testsDirectory, initScriptsBaseDirectory)))
                {
                    break;
                }
                testsDirectory = Directory.GetParent(testsDirectory)?.FullName
                    ?? throw new DirectoryNotFoundException("Solution directory not found");
            }

            var scriptPath = Path.Combine(
                testsDirectory,
                _initScriptsPath ?? string.Empty,
                initScriptName);

            return scriptPath;
        }
    }
}
