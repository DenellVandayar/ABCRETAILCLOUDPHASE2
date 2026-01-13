using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Test4712.Controllers;
using Test4712.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Enable session management





// Add authentication services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Redirect to the login page if not authenticated
    });

//Add authorization globally
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddDistributedMemoryCache(); // Required for session state
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout duration
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Mark the session cookie as essential
});

var configuration = builder.Configuration;

builder.Services.AddSingleton<FileShareService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("AzureStorageConnectionString");
    var shareName = "productfiles";

    return new FileShareService(connectionString, shareName);
});

builder.Services.AddSingleton(new QueueService(configuration.GetConnectionString("AzureStorageConnectionString")));
builder.Services.AddSingleton(new BlobService(configuration.GetConnectionString("AzureStorageConnectionString")));
builder.Services.AddSingleton(new TableStorageService(configuration.GetConnectionString("AzureStorageConnectionString")));
// Retrieve the connection string
string? azureStorageConnectionString = builder.Configuration.GetConnectionString("AzureStorageConnectionString");

if (string.IsNullOrEmpty(azureStorageConnectionString))
{
    throw new InvalidOperationException("Azure Storage connection string is not configured or is empty.");
}

builder.Services.AddSingleton(new UserTableService(azureStorageConnectionString, "UsersTable"));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

//app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
