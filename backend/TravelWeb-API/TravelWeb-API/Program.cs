using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using System.Text;
using TravelWeb_API.Models.ActivityModel;
using TravelWeb_API.Models.attraction;
using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.IService;
using TravelWeb_API.Models.Board.Service;
using TravelWeb_API.Models.Itinerary.Service;
using TravelWeb_API.Models.MemberSystem;
using TravelWeb_API.Models.TripProduct;
using TravelWeb_API.Models.TripProduct.ITripProduct;
using TravelWeb_API.Models.TripProduct.STripProduct;
using TravelWeb_API.Models.TripProduct.TripDTO;
using TravelWeb_API.Services;

QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 🚀 關鍵修復：替換為正確的 CORS 設定
// ==========================================
// 將原本的 myAllowSpecificOrigins 設定刪除，改用這個：
var myAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });

});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var signKey = jwtSettings["SignKey"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.IncludeErrorDetails = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signKey!)),

            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["AuthToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TravelWeb_API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "請在下方輸入您的 JWT Token \n\n (注意：不需要輸入 Bearer 這個字，直接貼上 Token 即可)"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


builder.Services.AddSwaggerGen(
    x =>
    {

        x.SwaggerDoc("TravelWeb-API", new OpenApiInfo
        {
            Title = "TravelWeb-API",
            //Version = "版本"
        });
        x.SwaggerDoc("Board", new OpenApiInfo
        {
            Title = "Board"
        });

        x.DocInclusionPredicate((docName, apiDesc) =>
        {
            if (!string.IsNullOrEmpty(apiDesc.GroupName))
            {
                return apiDesc.GroupName == docName;
            }

            return docName == "TravelWeb-API";
        });
    }
    );

builder.Services.AddDbContext<AttractionsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Travel")));
builder.Services.AddDbContext<MemberSystemContext>(options =>
 options.UseSqlServer(builder.Configuration.GetConnectionString("Travel")));

#region ActivityDI
builder.Services.AddDbContext<ActivityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Travel")));
builder.Services.AddScoped<ActivityCardService>();
builder.Services.AddScoped<ActivityInfoService>();
builder.Services.AddScoped<ActivityTicketService>();
builder.Services.AddHttpClient<GoogleRouteForActivityService>();
builder.Services.AddScoped<ActivityReviewService>();
builder.Services.AddScoped<CloudinaryPhotoService>();

//QRCode 相關組態強型別引用、QRCode Service 註冊
builder.Services.Configure<QrCodeSettings>(
    builder.Configuration.GetSection("QrCodeSettings"));
builder.Services.AddScoped<QRCodeService>();

//SMTP 相關組態強型別引用、Email Service 註冊
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<EmailService>();
#endregion

builder.Services.AddDbContext<TripDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Travel")));


#region ItineraryDI
builder.Services.AddDbContext<TravelWeb_API.Models.Itinerary.DBContext.TravelContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Travel")));
builder.Services.AddScoped<CloudinaryService>();
builder.Services.AddScoped<TravelWeb_API.Models.Itinerary.Service.IItineraryservice, TravelWeb_API.Models.Itinerary.Service.ItineraryService>();
var config = TypeAdapterConfig.GlobalSettings;
builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();
builder.Services.AddScoped<IItineraryservice, ItineraryService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IAIItineraryService, AIItineraryService>();
builder.Services.AddScoped<IGooglePlaceService, GooglePlaceService>();
builder.Services.AddHttpClient<GooglePlaceService>();
//builder.Services.AddControllers()
//    .AddJsonOptions(options =>
//    {
//        // 自動將屬性名稱轉為小寫開頭 (camelCase)，符合 Angular 習慣
//        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
//        // 忽略掉循環引用
//        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
//    });
#endregion

//========留言功能=======================================
builder.Services.AddDbContext<BoardDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Travel")));
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentsService, CommentsService>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<ITagsService, TagsService>();
//===================================================

//行程商品表連線用DI
builder.Services.AddScoped<ITripproductTable, TripproductTable>();
builder.Services.AddScoped<IShoppingCart, SShoppingCart>();
//行程商品表連線用DI
builder.Services.AddScoped<ITripproductTable, TripproductTable>();
//購物車連線DI
builder.Services.AddScoped<IShoppingCart, SShoppingCart>();
//訂單連線用DI
builder.Services.AddScoped<IOrder, SOrder>();
//綠界連線用DI
builder.Services.AddScoped<IECPay, SECPay>();

builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
//////綠界
builder.Services.Configure<ECPaySetting>(builder.Configuration.GetSection("ECPay"));
// 3. 註冊 Http 客戶端 (之後查詢訂單會用到)
builder.Services.AddHttpClient();
//builder.Services.AddControllers(options =>
//{
//    options.Filters.Add(new AuthorizeFilter());
//});

builder.Services.AddScoped<IMemberEmailService, MemberEmailService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ActivityDbContext>();
    await dbContext.Database.CanConnectAsync();
    await dbContext.Activities.FirstOrDefaultAsync();
}



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

// 🔥 關鍵順序：必須是 Routing -> Cors -> Auth -> MapControllers
app.UseRouting();

app.UseCors(myAllowSpecificOrigins);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
