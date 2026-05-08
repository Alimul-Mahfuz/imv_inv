# Admin User Console Command

## Overview
This console command allows you to create admin users interactively using Spectre.Console for a beautiful CLI experience.

## Usage

Run the command from the solution root:

```bash
# Using dotnet CLI
dotnet run -- create-admin

# Or if using Visual Studio Package Manager Console
dotnet run -- create-admin
```

## What It Does

The `create-admin` command will:

1. Display a styled header using Spectre.Console
2. Prompt you to enter:
   - **Admin Name** - Full name of the admin user
   - **Admin Email** - Email address (must be unique and valid format)
   - **Admin Password** - Secure password (hidden input)
   - **Confirm Password** - Password confirmation to prevent typos
3. Validate all inputs:
   - Names and passwords cannot be empty
   - Email must contain '@' and be unique
   - Passwords must match
4. Hash the password using ASP.NET Core Identity's PasswordHasher
5. Create the user in the database
6. Display a success message with the new user's details

## Features

✅ **Interactive CLI** - Beautiful prompts with Spectre.Console formatting
✅ **Input Validation** - All fields are validated with helpful error messages
✅ **Password Hashing** - Uses ASP.NET Core Identity PasswordHasher for secure storage
✅ **Duplicate Prevention** - Prevents creating users with duplicate email addresses
✅ **Styled Output** - Color-coded messages and formatted displays

## Example Session

```
╔═══════════════════════════════════╗
║     Create Admin User Command      ║
╚═══════════════════════════════════╝

Admin Name: John Administrator
Admin Email: admin@example.com
Admin Password: ••••••••••
Confirm Password: ••••••••••

✓ Admin user created successfully!

Name: John Administrator
Email: admin@example.com
User ID: 1
```

## Implementation Details

- **Location**: `Program.cs` - Inline implementation
- **Dependencies**: 
  - `Microsoft.EntityFrameworkCore`
  - `Microsoft.AspNetCore.Identity`
  - `Spectre.Console.Cli`
- **Database**: Automatically uses the configured connection string from `appsettings.json`
- **Authentication**: Passwords are securely hashed using `PasswordHasher<User>`

## Error Handling

The command handles:
- Empty field validation
- Invalid email format detection
- Duplicate email prevention
- Password mismatch detection
- Database connection failures
- All errors are displayed with helpful messages

## Extensibility

To add more CLI commands in the future:

1. Add another `if` condition in Program.cs for your new command
2. Create an async Task method following the same pattern as `CreateAdminUserInteractive`
3. Register it in the command check at the beginning of Program.cs
