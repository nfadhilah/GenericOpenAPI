using System.Data;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Dapper;
using GenericOpenAPI.Helpers;
using GenericOpenAPI.Middleware;
using GenericOpenAPI.Models;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddScoped(_ => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors();
builder.Services.AddSwaggerGen(o =>
{
    o.OperationFilter<SwaggerCustomHeaderFilter>();
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
}


// global cors policy
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

//app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    try
    {
        if (context.Request.Headers.TryGetValue("X-Authorization", out var authHeader) && authHeader != builder.Configuration.GetSection("SecretKey").Value)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            var responseBody = new BaseResponse<object>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid Secret Key",
                Data = null
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(responseBody));
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
    finally
    {
        if (!context.Response.HasStarted)
        {
            await next.Invoke(context);
        }
    }
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapGet("/realisasi_pendapatan/{tgl}", async (string tgl, SqlConnection db) =>
    {
        var result = new BaseResponse<IEnumerable<object>>();

        if (!Regex.IsMatch(tgl, "^\\d{4}-((0[1-9])|(1[012]))-((0[1-9]|[12]\\d)|3[01])$"))
        {
            result.Data = null;
            result.Message = "Invalid date format";
            result.StatusCode = StatusCodes.Status400BadRequest;

            return Results.BadRequest(result);
        }

        const string query = "EXEC GET_REALPEND @tgl";
        result.Data = await db.QueryAsync<RealisasiPendapatan>(query, new { tgl });
        return Results.Ok(result);
    })
    .WithName("GetRealisasiPendapatan");



app.Run();