using ims_inv.Commands;
using ims_inv.Data;
using ims_inv.Helper;
using ims_inv.Repositories;
using ims_inv.Services;
using Microsoft.EntityFrameworkCore;

// Check if running as CLI command
if (await CommandServiceExtensions.TryRunCommandAsync(args))
{
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<WebAppDbContext>((sp, options) =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();
builder.Services.AddScoped<IUnitConversionRepository, UnitConversionRepository>();

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

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUnitConversionService, UnitConversionService>();
builder.Services.AddScoped<UnitConversionService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IStockMovementService, StockMovementService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

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

await DbInitializer.InitializeAsync(app.Services);

app.Run();
