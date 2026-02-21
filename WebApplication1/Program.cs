//using System.Reflection;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAngular",
//        policy =>
//        {
//            policy
//                .WithOrigins("http://localhost:4200")
//                .AllowAnyHeader()
//                .AllowAnyMethod();
//        });
//});

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen(c =>
//{
//    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
//    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
//    c.IncludeXmlComments(xmlPath);
//});

//var app = builder.Build();

//// Ensure output folder exists
//Directory.CreateDirectory("output");

//app.UseStaticFiles();

//// Swagger
//app.UseSwagger();
//app.UseSwaggerUI();

//app.UseHttpsRedirection();

//// CORS must be before Authorization
//app.UseCors("AllowAngular");

//app.UseAuthorization();

//app.MapControllers();

//app.Run();




using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Ensure output folder exists
Directory.CreateDirectory("output");

app.UseHttpsRedirection();

// Serve Angular static files
app.UseDefaultFiles();     // ✅ VERY IMPORTANT
app.UseStaticFiles();      // ✅ VERY IMPORTANT

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

// 🔥 Angular routing support
app.MapFallbackToFile("index.html");

app.Run();