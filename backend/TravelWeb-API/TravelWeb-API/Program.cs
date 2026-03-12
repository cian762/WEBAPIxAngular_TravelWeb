using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TravelWeb_API.Models.ActivityModel;
using TravelWeb_API.Models.attraction;
using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.IService;
using TravelWeb_API.Models.Board.Service;
using TravelWeb_API.Models.Board;
using TravelWeb_API.Models.MemberSystem;
using TravelWeb_API.Models.TripProduct;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(
    x =>
    {
        x.SwaggerDoc("Board", new OpenApiInfo
        {
            Title = "Board",
            Version = "版本"
        });
        x.SwaggerDoc("TravelWeb-API", new OpenApiInfo
        {
            Title = "TravelWeb-API",
            //Version = "版本"
        });
        
        x.DocInclusionPredicate((docName, apiDesc) =>
        {
            // 1. 如果該 API 有設定 GroupName，則必須與 DocName 完全匹配
            if (!string.IsNullOrEmpty(apiDesc.GroupName))
            {
                return apiDesc.GroupName == docName;
            }

            // 2. 如果該 API 沒設定 GroupName，則通通塞進 "other" 這組
            return docName == "TravelWeb-API";
        });
    }
    );

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
#region ItineraryDI
builder.Services.AddDbContext<TravelWeb_API.Models.Itinerary.DBContext.TravelContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Travel")));
builder.Services.AddScoped<TravelWeb_API.Models.Itinerary.Service.IItineraryservice, TravelWeb_API.Models.Itinerary.Service.ItineraryService>();
var config = TypeAdapterConfig.GlobalSettings;
builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();
#endregion

//===================================================
// 註冊Board功能相關
builder.Services.AddDbContext<BoardDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Travel")));
builder.Services.AddScoped<IArticlesService, ArticleService>();
builder.Services.AddScoped<ICommentsService, CommentsService>();
//===================================================

var app = builder.Build();
/////////////////////

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(
        x =>
    {
        x.SwaggerEndpoint("/swagger/Board/swagger.json", "Board");
        x.SwaggerEndpoint("/swagger/TravelWeb-API/swagger.json", "TravelWeb-API");
        
    }
    );
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
