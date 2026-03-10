using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.ActivityModel;
using TravelWeb_API.Models.attraction;
using TravelWeb_API.Models.Board;
using TravelWeb_API.Models.MemberSystem;
using TravelWeb_API.Models.TripProduct;


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

//===================================================
builder.Services.AddDbContext<TripDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Travel")));

//===================================================
builder.Services.AddDbContext<TravelWeb_API.Models.Itinerary.DBContext.TravelContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Travel")));

//===================================================
builder.Services.AddDbContext<BoardDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Travel")));
//===================================================
builder.Services.AddScoped<TravelWeb_API.Models.Itinerary.Service.IItineraryservice, TravelWeb_API.Models.Itinerary.Service.ItineraryService>();

var app = builder.Build();
/////////////////////

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
