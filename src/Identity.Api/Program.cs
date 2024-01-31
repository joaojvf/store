using Identity.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddIdentityConfig(builder.Configuration);
builder.Services.AddApiConfiguration();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseIdentityConfiguration();
app.MapControllers();
app.Run();