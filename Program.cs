using GCTWeb;
using Microsoft.AspNetCore.Identity;
using GCTWeb.Data;
using GCTWeb.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options => // Hoặc services.Configure<RazorPagesOptions>(options =>
{
    // --- ÁNH XẠ LẠI ĐƯỜNG DẪN CHO TRANG IDENTITY ---

    // /Identity/Account/Login -> /login
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Login", "login");
    
    // /Identity/Account/Register -> /register
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Register", "register");

    // /Identity/Account/Logout -> /logout
    // Logout thường là POST, nhưng convention này áp dụng cho GET nếu có trang xác nhận logout.
    // Nếu Logout chỉ là một action POST từ form, bạn không cần ánh xạ route cho nó theo cách này.
    // Tuy nhiên, nếu có trang Logout.cshtml, bạn có thể ánh xạ:
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Logout", "logout");


    // Ví dụ: /Identity/Account/Manage/Index -> /manage/profile (hoặc /account/profile)
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/Index", "manage/profile");
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/Email", "manage/email");
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/ChangePassword", "manage/change-password");
    // ... và các trang Manage khác ...

    // Ví dụ: /Identity/Account/ForgotPassword -> /forgot-password
    options.Conventions.AddAreaPageRoute("Identity", "/Account/ForgotPassword", "forgot-password");
    options.Conventions.AddAreaPageRoute("Identity", "/Account/ResetPassword", "reset-password");

    // Bạn có thể thêm nhiều AddAreaPageRoute cho các trang Identity khác mà bạn muốn tùy chỉnh URL
    // options.Conventions.AddAreaPageRoute("Identity", "/Account/AccessDenied", "access-denied");
    // options.Conventions.AddAreaPageRoute("Identity", "/Account/ConfirmEmail", "confirm-email");
    
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseMySql(connectionString, serverVersion: MySqlServerVersion.AutoDetect(connectionString));
});

/*
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
*/

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

//Cart and session setup
builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMySqlCache(options =>
{
    options.ConnectionString = connectionString;
    options.SchemaName = "gctweb"; 
    options.TableName = "AppDistributedCache"; 
    options.ExpiredItemsDeletionInterval = TimeSpan.FromMinutes(30);
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.Name = ".GCTWeb.AppSession"; // Tên cookie riêng
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
    options.Cookie.MaxAge = TimeSpan.FromDays(14); // Cookie tồn tại 14 ngày
    options.Cookie.Path = "/";
});

builder.Services.AddScoped<CartService>();
//End setup

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//Use session
app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();