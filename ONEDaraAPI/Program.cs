var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

/*app.UseCors(corsPolicyBuilder =>
    corsPolicyBuilder.WithOrigins("http://localhost:8100", "http://10.2.0.21:8081", "http://itonee.co:8081", "http://203.146.192.21:8081", "http://www.itonee.com:8081", "http://itonee.com:8081")
  .AllowAnyMethod()
  .AllowAnyHeader()

);*/

//app.UseCors(corsPolicyBuilder =>
//   corsPolicyBuilder
//  .AllowAnyOrigin()
//  .AllowAnyMethod()
//  .AllowAnyHeader()

//);

app.UseCors(x => x
         .AllowAnyMethod()
         .AllowAnyHeader()
         .AllowCredentials()
         //.WithOrigins("https://localhost:44351")); // Allow only this origin can also have multiple origins seperated with comma
         .SetIsOriginAllowed(origin => true));


app.Run();
