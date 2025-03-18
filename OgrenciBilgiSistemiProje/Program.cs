using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemiProje.Services;
using Microsoft.AspNetCore.Identity;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


// burada veritaban� ba�lant�s� yap�l�yor
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

builder.Services.AddSession(); // Session kullanabilmek i�in ekledik. ��nk� session default olarak gelmiyor.
builder.Services.AddDistributedMemoryCache(); //
builder.Services.AddControllersWithViews(); // Controller ve view'lar� kullanabilmek i�in ekledik.


builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Account/StuTeaLog"; // Login olunmad���nda y�nlendirilecek sayfa
        options.AccessDeniedPath = "/Account/StuTeaLog"; // Yetkisiz eri�imde y�nlendirilecek sayfa
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
app.UseAuthorization(); // Yetkilendirme i�lemleri i�in ekledik.


app.MapStaticAssets(); // Static dosyalar� kullanabilmek i�in ekledik.

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
