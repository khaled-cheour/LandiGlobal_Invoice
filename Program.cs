using LandiGlobalTemplate.Data;
using LandiGlobalTemplate.Models;
using LandiGlobalTemplate.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

const string SingleUserEmail = "gnagna.monote@landiglobal.com";
const string SingleUserPassword = "Khaled.5555";

if (args.Contains("--clean-user-data", StringComparer.OrdinalIgnoreCase))
{
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
}

// Add Database Context - SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<InvoiceValidationService>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ExcelTransformService>();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    await ProductCatalogDatabaseInitializer.EnsureReadyAsync(db);
    
    // Seed roles
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedRoles(roleManager);

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await SeedSingleUserAsync(db, userManager, SingleUserEmail, SingleUserPassword);

    if (args.Contains("--clean-user-data", StringComparer.OrdinalIgnoreCase))
    {
        var confirmed = args.Contains("--yes", StringComparer.OrdinalIgnoreCase);
        await CleanUserDataAsync(db, confirmed);
        return;
    }
}

static async Task SeedSingleUserAsync(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    string email,
    string password)
{
    var normalizedEmail = userManager.NormalizeEmail(email);
    var otherUserIds = await db.Users
        .Where(user => user.NormalizedEmail != normalizedEmail)
        .Select(user => user.Id)
        .ToListAsync();

    if (otherUserIds.Count > 0)
    {
        await db.AuditLogs
            .Where(log => log.UserId != null && otherUserIds.Contains(log.UserId))
            .ExecuteUpdateAsync(setters => setters.SetProperty(log => log.UserId, (string?)null));
        await db.UserTokens.Where(token => otherUserIds.Contains(token.UserId)).ExecuteDeleteAsync();
        await db.UserLogins.Where(login => otherUserIds.Contains(login.UserId)).ExecuteDeleteAsync();
        await db.UserClaims.Where(claim => otherUserIds.Contains(claim.UserId)).ExecuteDeleteAsync();
        await db.UserRoles.Where(role => otherUserIds.Contains(role.UserId)).ExecuteDeleteAsync();
        await db.Users.Where(user => otherUserIds.Contains(user.Id)).ExecuteDeleteAsync();
    }

    var user = await userManager.FindByEmailAsync(email);
    if (user == null)
    {
        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = "Gnagna",
            LastName = "Monote",
            EmailConfirmed = true,
            IsActive = true
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                "Unable to create the default login user: " +
                string.Join("; ", createResult.Errors.Select(error => error.Description)));
        }
    }

    user.UserName = email;
    user.Email = email;
    user.FirstName = "Gnagna";
    user.LastName = "Monote";
    user.EmailConfirmed = true;
    user.IsActive = true;
    user.LockoutEnd = null;
    user.AccessFailedCount = 0;
    user.PasswordHash = userManager.PasswordHasher.HashPassword(user, password);
    user.SecurityStamp = Guid.NewGuid().ToString();

    var updateResult = await userManager.UpdateAsync(user);
    if (!updateResult.Succeeded)
    {
        throw new InvalidOperationException(
            "Unable to update the default login user: " +
            string.Join("; ", updateResult.Errors.Select(error => error.Description)));
    }

    if (!await userManager.IsInRoleAsync(user, "Admin"))
    {
        await userManager.AddToRoleAsync(user, "Admin");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
{
    string[] roles = { "Admin", "Manager", "User", "Auditor" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

static async Task CleanUserDataAsync(ApplicationDbContext db, bool confirmed)
{
    var usersCount = await db.Users.CountAsync();
    var userRolesCount = await db.UserRoles.CountAsync();
    var userClaimsCount = await db.UserClaims.CountAsync();
    var userLoginsCount = await db.UserLogins.CountAsync();
    var userTokensCount = await db.UserTokens.CountAsync();
    var linkedAuditLogsCount = await db.AuditLogs
        .CountAsync(log => log.UserId != null || EF.Property<string?>(log, "ApplicationUserId") != null);

    Console.WriteLine("User data cleanup preview:");
    Console.WriteLine($"- AspNetUsers: {usersCount}");
    Console.WriteLine($"- AspNetUserRoles: {userRolesCount}");
    Console.WriteLine($"- AspNetUserClaims: {userClaimsCount}");
    Console.WriteLine($"- AspNetUserLogins: {userLoginsCount}");
    Console.WriteLine($"- AspNetUserTokens: {userTokensCount}");
    Console.WriteLine($"- AuditLogs linked to users: {linkedAuditLogsCount}");

    if (!confirmed)
    {
        Console.WriteLine("No data deleted. Run with --clean-user-data --yes to confirm.");
        return;
    }

    await db.AuditLogs
        .Where(log => log.UserId != null || EF.Property<string?>(log, "ApplicationUserId") != null)
        .ExecuteDeleteAsync();
    await db.UserTokens.ExecuteDeleteAsync();
    await db.UserLogins.ExecuteDeleteAsync();
    await db.UserClaims.ExecuteDeleteAsync();
    await db.UserRoles.ExecuteDeleteAsync();
    await db.Users.ExecuteDeleteAsync();

    Console.WriteLine("User data cleanup completed.");
}
