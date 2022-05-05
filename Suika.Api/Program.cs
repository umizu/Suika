using FluentValidation;
using FluentValidation.Results;
using Suika.Api.Data;
using Suika.Api.Extensions;
using Suika.Api.Models;
using Suika.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddSingleton<IDbConnectionFactory>(_ => new SqliteConnectionFactory(
    builder.Configuration.GetValue<string>("Database:ConnectionString")
));
builder.Services.AddSingleton<DatabaseInitializer>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/users", async (IUserService userService, string? searchTerm) =>
{
    if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
    {
        var matchedUsers = await userService.SearchByUsernameAsync(searchTerm);
        var matchedUsersAsDtos = matchedUsers.Select(user => user.AsDto());
        return Results.Ok(matchedUsersAsDtos);
    }
    var users = await userService.GetAllAsync();
    var usersAsDtos = users.Select(user => user.AsDto());
    return Results.Ok(usersAsDtos);
});

app.MapGet("/users/{username}", async (string username, IUserService userService) =>
{
    var user = await userService.GetByUsernameAsync(username);
    return user is not null ? Results.Ok(user.AsDto()) : Results.NotFound();
});

app.MapPost("/users", async (User user, IUserService userService, IValidator<User> validator) =>
{
    var validationResult = await validator.ValidateAsync(user);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors);
    }
    var created = await userService.CreateAsync(user);
    if (!created)
    {
        return Results.BadRequest(new List<ValidationFailure>
        {
            new ("Username", "This username already exists")
        });
    }
    return Results.Created($"/users/{user.Username}", user.AsDto());
});

app.MapPut("/users/{username}", async (string username, User user, IUserService userService,
    IValidator<User> validator) =>
{
    user.Username = username;
    var validationResult = await validator.ValidateAsync(user);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors);
    }
    var updated = await userService.UpdateAsync(user);
    return (updated) ? Results.NoContent() : Results.NotFound();
});

app.MapDelete("/users/{username}", async (string username, IUserService userService) =>
{
    return await userService.DeleteAsync(username) ? Results.NoContent() : Results.NotFound();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();
