using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using studie_meter.server;

var builder = WebApplication.CreateBuilder(args);

// LISTEN ON HTTP (port 5000)
builder.WebHost.UseKestrel(options => {
    options.ListenAnyIP(5000); // Only HTTP
});

builder.Services.AddSignalR();

// Allow all origins, with credentials
builder.Services.AddCors(options => {
    options.AddPolicy("CorsPolicy", policy =>
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
}

// ? CORRECT ORDER
app.UseStaticFiles();

app.UseRouting();

app.UseCors("CorsPolicy");

// ? REMOVE THIS — causes 307 redirect without CORS headers
// app.UseHttpsRedirection();

app.UseEndpoints(endpoints => {
    endpoints.MapHub<FeedbackHub>("/feedbackHub");
    endpoints.MapFallbackToFile("index.html");
});

app.Run();
