using LogEveryThingMiddleware;
using LogEveryThingMiddleware.BL;
using LogEveryThingMiddleware.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<SendTraceHandler>();
builder.Services.AddHttpClient(ConstantNames.InternalHttpClient)
    .AddHttpMessageHandler<SendTraceHandler>();

builder.Services.AddScoped<IBusinessService, BusinessService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<LogsMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
