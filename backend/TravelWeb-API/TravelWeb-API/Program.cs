using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.attraction;
using TravelWeb_API.Models.MemberSystem;
using TravelWeb_API.Models.ActivityModel;
using TravelWeb_API.Models.Board;
using TravelWeb_API.Models.TripProduct;
using TravelWeb_API.Services;


var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<ActivityCardService>();
builder.Services.AddScoped<ActivityInfoService>();
builder.Services.AddScoped<ActivityTicketService>();

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


var app = builder.Build();
/////////////////////
///

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ActivityDbContext>();
    await dbContext.Database.CanConnectAsync();
    await dbContext.Activities.FirstOrDefaultAsync();
}



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
