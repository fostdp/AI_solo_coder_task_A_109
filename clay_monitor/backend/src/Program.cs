using ClayMonitor.Core.Channels;
using ClayMonitor.Core.Configuration;
using ClayMonitor.AlertDispatch;
using ClayMonitor.MaterialScore;
using ClayMonitor.SaltTransport;
using ClayMonitor.WifiIngest;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services.Configure<AppSettings>(builder.Configuration);
builder.Services.Configure<WifiIngestOptions>(builder.Configuration.GetSection("WifiIngest"));
builder.Services.Configure<SaltTransportOptions>(builder.Configuration.GetSection("SaltTransport"));
builder.Services.Configure<MaterialScoreOptions>(builder.Configuration.GetSection("MaterialScore"));
builder.Services.Configure<AlertDispatchOptions>(builder.Configuration.GetSection("AlertDispatch"));
builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("Database"));

builder.Services.AddSingleton<IMessageBus, MessageBus>();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IWifiIngestService, WifiIngestService>();
builder.Services.AddHostedService<SaltTransportService>();
builder.Services.AddScoped<ISaltTransportService>(sp =>
    sp.GetServices<IHostedService>().OfType<SaltTransportService>().First()
    ?? new SaltTransportService(
        sp.GetRequiredService<IMessageBus>(),
        sp.GetRequiredService<IOptions<SaltTransportOptions>>()));
builder.Services.AddHostedService<MaterialScoreService>();
builder.Services.AddScoped<IMaterialScoreService>(sp =>
    sp.GetServices<IHostedService>().OfType<MaterialScoreService>().First()
    ?? new MaterialScoreService(
        sp.GetRequiredService<IMessageBus>(),
        sp.GetRequiredService<IOptions<MaterialScoreOptions>>()));
builder.Services.AddHostedService<AlertDispatchService>();
builder.Services.AddScoped<IAlertDispatchService>(sp =>
    sp.GetServices<IHostedService>().OfType<AlertDispatchService>().First()
    ?? new AlertDispatchService(
        sp.GetRequiredService<IMessageBus>(),
        sp.GetRequiredService<IOptions<AlertDispatchOptions>>(),
        sp.GetRequiredService<IHttpClientFactory>()));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "古代泥塑彩绘层盐分迁移与加固材料适配系统 API",
        Version = "v1",
        Description = @"模块架构：
            • WifiIngest - Wi-Fi传感器数据接入
            • SaltTransport - Richards方程盐分迁移（Penman蒸发）
            • MaterialScore - 加固材料6维度适配评分
            • AlertDispatch - 告警规则引擎+钉钉推送
            通过 System.Threading.Channels 异步解耦"
    });
});

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:5173", "http://localhost:3000" };
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ClayMonitor v1"));
}

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

var lifetime = app.Lifetime;
lifetime.ApplicationStarted.Register(() =>
{
    var bus = app.Services.GetRequiredService<IMessageBus>();
    app.Logger.LogInformation(@"
╔══════════════════════════════════════════════════════════════╗
║     古代泥塑彩绘层盐分迁移与加固材料适配系统 启动成功           ║
╠══════════════════════════════════════════════════════════════╣
║  模块架构 (Channel 异步解耦)                                   ║
║  ┌──────────┐   ┌──────────┐   ┌──────────┐   ┌──────────┐  ║
║  │ WifiIngest│──▶│SaltTrans │──▶│MaterialSc│──▶│AlertDisp │  ║
║  │ Wi-Fi接入 │   │ Richards │   │ 6维度评分 │   │ 钉钉推送  │  ║
║  └──────────┘   └──────────┘   └──────────┘   └──────────┘  ║
║  通过 System.Threading.Channels<T> 异步发布订阅                ║
╚══════════════════════════════════════════════════════════════╝
");
});

app.Run();
