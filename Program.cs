var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<KafkaConsumerService>();
builder.Services.AddSingleton<WebSocketConnectionManager>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8000);
});

builder.Services.AddControllers();

var app = builder.Build();

// Enable WebSocket support
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
};
app.UseWebSockets(webSocketOptions);

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/ws"))
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var connectionManager = app.Services.GetRequiredService<WebSocketConnectionManager>();
            var kafkaConsumer = app.Services.GetRequiredService<KafkaConsumerService>();

            await connectionManager.HandleWebSocketConnectionAsync(webSocket, kafkaConsumer);
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
    else
    {
        await next();
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.MapControllers();
app.Run();

