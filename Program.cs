using Residence.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddIdentityService(builder.Configuration);
builder.Services.AddFluentEmail(builder.Configuration.GetValue<string>("Mail:DefaultFrom"))
                .AddRazorRenderer()
                .AddSmtpSender(builder.Configuration.GetValue<string>("Mail:Server"), 587, builder.Configuration.GetValue<string>("Mail:User"), builder.Configuration.GetValue<string>("Mail:HashPassword"));

builder.Services.AddCors(opt => 
{
    opt.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseCors("CorsPolicy");

app.MapControllers();

app.Run();
