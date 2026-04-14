using SV22T1020670.DomainModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using SV22T1020670.Shop.AppCodes;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews()
                .AddMvcOptions(option =>
                {
                    option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                });

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(option =>
                {
                    option.Cookie.Name = "SV22T1020670.Shop";
                    option.LoginPath = "/Account/Login";
                    option.AccessDeniedPath = "/Account/AccessDenied";
                    option.ExpireTimeSpan = TimeSpan.FromDays(30);
                    option.SlidingExpiration = true;
                    option.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

                    option.Cookie.SameSite = SameSiteMode.Lax;

                    option.Cookie.HttpOnly = true;
                    option.Cookie.IsEssential = true;
                });

builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromDays(30);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});

var app = builder.Build();

// ================== 2. PIPELINE REQUEST ==================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//Configure default format
var cultureInfo = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;



// ================== 3. KHỞI TẠO CẤU HÌNH ==================

// 3.1. Cấu hình ApplicationContext
ApplicationContext.Configure
(
    httpContextAccessor: app.Services.GetRequiredService<IHttpContextAccessor>(),
    webHostEnvironment: app.Services.GetRequiredService<IWebHostEnvironment>(),
    configuration: app.Configuration
);

// 3.2. Khởi tạo kết nối CSDL (Business Layer)
var connectionString = builder.Configuration.GetConnectionString("SV22T1020670");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Missing connection string: SV22T1020670");
}
SV22T1020670.BusinessLayers.Configuration.Initialize(connectionString);

// 3.3. ĐỌC CẤU HÌNH ẢNH TỪ APPSETTINGS (FIX LỖI ẢNH)
// Đoạn này sẽ lấy link "https://localhost:44379" từ json gán vào code
string? adminUrl = builder.Configuration["AppConfig:AdminServerUrl"];
SV22T1020670.Shop.AppCodes.WebConfig.AdminServerUrl = adminUrl ?? "";

// Đọc đường dẫn thư mục ảnh
SV22T1020670.Shop.AppCodes.WebConfig.ProductImgPath = builder.Configuration["AppConfig:ProductImgPath"] ?? "/images/products/";
SV22T1020670.Shop.AppCodes.WebConfig.BannerImgPath = builder.Configuration["AppConfig:BannerImgPath"] ?? "/images/banners/";
SV22T1020670.Shop.AppCodes.WebConfig.CategoryImgPath = builder.Configuration["AppConfig:CategoryImgPath"] ?? "/images/category/";

app.Run();