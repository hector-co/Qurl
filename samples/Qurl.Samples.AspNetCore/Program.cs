using Microsoft.EntityFrameworkCore;
using Qurl;
using Qurl.Samples.AspNetCore.Models;
using Qurl.Samples.AspNetCore.Qurl.Samples.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SampleContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("QurlSample"))
);

builder.Services.AddHostedService<InitDbContext>();

builder.Services.AddQurl(o =>
{
    o.SetDateTimeConverter((dateTime) => dateTime.ToUniversalTime());
    o.SetDateTimeOffsetConverter((dateTime) => dateTime.ToUniversalTime());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
