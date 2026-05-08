using Microsoft.EntityFrameworkCore;
using ims_inv.Data;
using ims_inv.Helper;
using ims_inv.Services;
using ims_inv.Interceptors;
using ims_inv.Observers;
using ims_inv.Events;
using ims_inv.Repositories;
using ims_inv.Commands;

// Check if running as CLI command
if (await CommandServiceExtensions.TryRunCommandAsync(args))
{
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<DomainEventsInterceptor>();
builder.Services.AddScoped<IDomainEventHandler<UserRegisteredEvent>, UserObserver>();

builder.Services.AddDbContext<WebAppDbContext>((sp, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(sp.GetRequiredService<DomainEventsInterceptor>());
});

// Register repositories
builder.Services.AddScoped<UserRepository>();

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthUser>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailConfig"));
builder.Services.AddScoped<IRazorViewRenderer, RazorViewRenderer>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<UnitConversionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
