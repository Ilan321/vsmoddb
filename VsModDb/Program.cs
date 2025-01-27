var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c => c.EnableTryItOutByDefault());

app.MapControllers();

app.Run();
