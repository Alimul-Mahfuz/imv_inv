using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using ims_inv.Data;

namespace ims_inv.Commands
{
    /// <summary>
    /// Extension methods for registering and running CLI commands.
    /// </summary>
    public static class CommandServiceExtensions
    {
        /// <summary>
        /// Attempts to run a CLI command if the first argument matches a registered command.
        /// Returns true if a command was executed, false if the app should continue normally.
        /// </summary>
        public static async Task<bool> TryRunCommandAsync(string[] args)
        {
            if (args.Length == 0 || !IsCommandArgument(args[0]))
            {
                return false;
            }

            var command = args[0];

            return command switch
            {
                "create-admin" => await RunCreateAdminCommandAsync(),
                _ => false
            };
        }

        /// <summary>
        /// Runs the create-admin command.
        /// </summary>
        private static async Task<bool> RunCreateAdminCommandAsync()
        {
            try
            {
                var cliHost = BuildCliHost();
                using (var scope = cliHost.Services.CreateScope())
                {
                    var command = scope.ServiceProvider.GetRequiredService<CreateAdminUserCommand>();
                    await command.ExecuteAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]✗ Error: {ex.Message}[/]");
                return true;
            }
        }

        /// <summary>
        /// Checks if the argument is a CLI command (starts with specific patterns).
        /// </summary>
        private static bool IsCommandArgument(string arg)
        {
            return arg switch
            {
                "create-admin" => true,
                _ => false
            };
        }

        /// <summary>
        /// Builds a Host with CLI-specific services configuration.
        /// </summary>
        private static IHost BuildCliHost()
        {
            var cliBuilder = Host.CreateDefaultBuilder();
            cliBuilder.ConfigureServices((context, services) =>
            {
                services.AddDbContext<WebAppDbContext>(options =>
                    options.UseSqlite(context.Configuration.GetConnectionString("DefaultConnection")));
                services.AddTransient<CreateAdminUserCommand>();
            });

            return cliBuilder.Build();
        }
    }
}
