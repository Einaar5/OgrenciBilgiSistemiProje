using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemiProje.Services;
using Microsoft.AspNetCore.Identity;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


// burada veritabaný baðlantýsý yapýlýyor
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

builder.Services.AddSession(); // Session kullanabilmek için ekledik. çünkü session default olarak gelmiyor.
builder.Services.AddDistributedMemoryCache(); //
builder.Services.AddControllersWithViews(); // Controller ve view'larý kullanabilmek için ekledik.


builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Account/StuTeaLog"; // Login olunmadýðýnda yönlendirilecek sayfa
        options.AccessDeniedPath = "/Account/StuTeaLog"; // Yetkisiz eriþimde yönlendirilecek sayfa
    });

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
app.UseSession();
app.UseAuthorization(); // Yetkilendirme iþlemleri için ekledik.


app.MapStaticAssets(); // Static dosyalarý kullanabilmek için ekledik.

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
