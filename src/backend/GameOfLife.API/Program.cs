using GameOfLife.Core.Abstractions;
using GameOfLife.Core.Rules;
using GameOfLife.Core.Services;
using GameOfLife.API.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<IGameOfLifeEngine, GameOfLifeEngine>();
builder.Services.AddSingleton<IRuleParser, DefaultBsRuleParser>();


var dataPath = builder.Configuration["Data:Path"] ?? "/app/data/boards.json";
builder.Services.AddSingleton<IBoardStore>(sp =>
    new FileBoardStore(dataPath, sp.GetRequiredService<IRuleParser>()));

builder.Services.AddScoped<IBoardService, BoardService>();

builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler("/error");
app.Map("/error", (HttpContext http) => Results.Problem());


app.UseDefaultFiles();
app.UseStaticFiles();


app.MapControllers();


app.MapFallbackToFile("index.html");

app.Run();

public sealed class DefaultBsRuleParser : IRuleParser
{
    public IGameOfLifeRule Default => BsRule.Conway();

    public bool TryParse(string notation, out IGameOfLifeRule? rule, out string? error)
    {
        var ok = BsRule.TryParse(notation, out var parsed, out error);
        rule = parsed;
        return ok;
    }
}