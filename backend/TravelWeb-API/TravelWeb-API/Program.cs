using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.ActivityModel;
using TravelWeb_API.Models.attraction;
using TravelWeb_API.Models.Board;
using TravelWeb_API.Models.MemberSystem;
using TravelWeb_API.Models.TripProduct;
using TravelWeb_API.Models.TripProduct.ITripProduct;
using TravelWeb_API.Models.TripProduct.STripProduct;


var builder = WebApplication.CreateBuilder(args);
//設定CROS
var myAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
        policy =>
        {
            policy.AllowAnyOrigin()    // 允許任何來源（最鬆）
                  .AllowAnyHeader()    // 允許任何標頭
                  .AllowAnyMethod();   // 允許任何動詞 (GET, POST, etc.)
        });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddDbContext<>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("Travel")));

//===================================================
builder.Services.AddDbContext<AttractionsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Travel")));
//===================================================
builder.Services.AddDbContext<MemberSystemContext>(options =>
 options.UseSqlServer(builder.Configuration.GetConnectionString("Travel")));

//===================================================
builder.Services.AddDbContext<ActivityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Travel")));

//===================================================
//³o¬O¦æµ{°Ó«~ªº³s½u
builder.Services.AddDbContext<TripDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Travel")));

//===================================================
//Itinerary��DBContext�`�J
builder.Services.AddDbContext<TravelWeb_API.Models.Itinerary.DBContext.TravelContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Travel")));

//===================================================
// µù¥U BoardDbContext¡A¨Ã«ü©w¨Ï¥Î SQL Server ¥H¤Î³s±µ¦r¦ê
builder.Services.AddDbContext<BoardDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Travel")));
//===================================================
//行程商品表連線用DI
builder.Services.AddScoped<ITripproductTable,TripproductTable >();
//購物車連線DI
builder.Services.AddScoped<IShoppingCart,SShoppingCart>();
//訂單連線用DI
builder.Services.AddScoped<IOrder, SOrder>();

var app = builder.Build();
/////////////////////

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(myAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
