using Microsoft.FluentUI.AspNetCore.Components;
using PdfExtractor.Web.Components;
using PdfExtractor.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient<PdfExtractorService>((serviceProvider, client) =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseAddress"] ?? throw new InvalidOperationException("ApiBaseAddress configuration is missing."));
});

builder.Services.AddFluentUIComponents(options =>
{
    options.ValidateClassNames = false;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();