using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetKlinika.Data;
using VetKlinika.Infrastructure;
using VetKlinika.Reporting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.LoginPath = "/Identity/Account/Login";
});

builder.Services.AddControllersWithViews();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<RequestExceptionHandler>();
builder.Services.AddScoped<ServiceCatalog>();
builder.Services.AddScoped<IAppointmentReportRepository, AppointmentReportRepository>();
builder.Services.AddScoped<AppointmentReportService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    string[] roleNames = { "Admin", "Vet", "Client" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create the '{roleName}' role: {string.Join("; ", roleResult.Errors.Select(error => error.Description))}");
            }
        }
    }

    var adminEmail = builder.Configuration["Admin:Email"] ?? "admin@vetklinika.com";
    var adminPassword = builder.Configuration["Admin:Password"];
    if (string.IsNullOrWhiteSpace(adminPassword))
    {
        if (!app.Environment.IsDevelopment())
        {
            throw new InvalidOperationException("Configure Admin:Password before starting outside Development.");
        }

        adminPassword = "Admin123!";
    }

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var newAdmin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var createAdmin = await userManager.CreateAsync(newAdmin, adminPassword);
        if (!createAdmin.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create the default administrator: {string.Join("; ", createAdmin.Errors.Select(error => error.Description))}");
        }

        adminUser = newAdmin;
    }

    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to assign the default administrator role: {string.Join("; ", roleResult.Errors.Select(error => error.Description))}");
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseExceptionHandler("/Home/Error");
app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"),
    branch => branch.UseStatusCodePages());
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"),
    branch => branch.UseStatusCodePagesWithReExecute("/Home/Status", "?code={0}"));

app.UseHttpsRedirection();
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/uploads"),
    branch => branch.UseStaticFiles());

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();