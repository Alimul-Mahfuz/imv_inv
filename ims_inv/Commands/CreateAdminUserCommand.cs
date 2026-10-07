using Spectre.Console;
using ims_inv.Repositories;
using ims_inv.Services;

namespace ims_inv.Commands
{
    /// <summary>
    /// Console command to create an admin user with interactive Spectre.Console prompts.
    /// Usage: dotnet run -- create-admin
    /// </summary>
    public class CreateAdminUserCommand
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;

        public CreateAdminUserCommand(IAuthService authService, IUserRepository userRepository)
        {
            _authService = authService;
            _userRepository = userRepository;
        }

        public async Task ExecuteAsync()
        {
            using var cts = new CancellationTokenSource();

            DisplayHeader();

            var name = GetName(cts.Token);
            var email = await GetEmail(cts.Token);
            var password = GetPassword();

            var user = await _authService.CreateAdminUserAsync(name, email, password);

            DisplaySuccess(name, email, user.Id);
        }

        private static void DisplayHeader()
        {
            AnsiConsole.MarkupLine("[bold cyan]╔═══════════════════════════════════╗[/]");
            AnsiConsole.MarkupLine("[bold cyan]║     Create Admin User Command      ║[/]");
            AnsiConsole.MarkupLine("[bold cyan]╚═══════════════════════════════════╝[/]");
            AnsiConsole.WriteLine();
        }

        private static string GetName(CancellationToken cancellationToken)
        {
            var name = AnsiConsole.Ask<string>("[bold green]Admin Name:[/]");
            while (string.IsNullOrWhiteSpace(name))
            {
                AnsiConsole.MarkupLine("[red]Name cannot be empty[/]");
                name = AnsiConsole.Ask<string>("[bold green]Admin Name:[/]");
            }
            return name;
        }

        private async Task<string> GetEmail(CancellationToken cancellationToken)
        {
            var email = AnsiConsole.Ask<string>("[bold green]Admin Email:[/]");
            while (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                AnsiConsole.MarkupLine("[red]Invalid email format[/]");
                email = AnsiConsole.Ask<string>("[bold green]Admin Email:[/]");
            }

            var isTaken = await _userRepository.IsEmailTakenAsync(email, cancellationToken: cancellationToken);
            if (isTaken)
            {
                AnsiConsole.MarkupLine("[bold red]✗ User with this email already exists![/]");
                throw new InvalidOperationException("User with this email already exists");
            }

            return email;
        }

        private static string GetPassword()
        {
            var password = AnsiConsole.Ask<string>("[bold green]Admin Password:[/]", "*");
            while (string.IsNullOrWhiteSpace(password))
            {
                AnsiConsole.MarkupLine("[red]Password cannot be empty[/]");
                password = AnsiConsole.Ask<string>("[bold green]Admin Password:[/]", "*");
            }

            var confirmPassword = AnsiConsole.Ask<string>("[bold green]Confirm Password:[/]", "*");
            if (password != confirmPassword)
            {
                AnsiConsole.MarkupLine("[bold red]✗ Passwords do not match![/]");
                throw new InvalidOperationException("Passwords do not match");
            }

            return password;
        }

        private static void DisplaySuccess(string name, string email, int userId)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold green]✓ Admin user created successfully![/]");
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine($"[bold]Name:[/] {name}");
            AnsiConsole.MarkupLine($"[bold]Email:[/] {email}");
            AnsiConsole.MarkupLine($"[bold]User ID:[/] {userId}");
            AnsiConsole.WriteLine();
        }
    }
}
