using NSwag.AspNetCore;
using Microsoft.EntityFrameworkCore;
using TODOAPI;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config => 
{
    config.DocumentName = "TodoAPI";
    config.Title = "TodoAPI v1";
    config.Version = "v1";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "TodoAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}
var todoItems = app.MapGroup("/todoitems");

todoItems.MapGet("/",GetAllTodos);
todoItems.MapGet("/complete",GetCompleteTodos);
todoItems.MapGet("/{id}",GetTodo);
todoItems.MapPost("/",CreateTodo);
todoItems.MapPut("/{id}",UpdateTodo);
todoItems.MapDelete("/{id}",DeleteTodo);


static async Task<IResult> GetAllTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.ToArrayAsync());
}
static async Task<IResult> GetCompleteTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Where( t => t.IsComplete).ToListAsync());
}
static async Task<IResult> GetTodo(int id, TodoDb db)
{
    return await db.Todos.FindAsync(id)
        is Todo todo ? TypedResults.Ok(todo) : TypedResults.NotFound();
}
static async Task<IResult> CreateTodo(Todo todo, TodoDb db)
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/todoitems/{todo.Id}", todo);
}
static async Task<IResult> UpdateTodo (int id, Todo inputTodo, TodoDb db)
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return TypedResults.NotFound();
    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;
    
    await db.SaveChangesAsync();

    return TypedResults.NoContent();

}
static async Task<IResult> DeleteTodo (int id, TodoDb db)
{
    if (await db.Todos.FindAsync(id) is Todo todo){
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();

    } 
    return TypedResults.NotFound();
}

app.Run("http://localhost:5224");
// todoItems.MapGet(
//     "/",
//     async (TodoDb db) => 
//     await db.Todos.ToListAsync()
// );

// todoItems.MapGet(
//     "/Hola",() => "Hello World!");

// todoItems.MapGet(
//     "/complete",
//     async (TodoDb db) =>
//     await db.Todos.Where(t => t.IsComplete).ToListAsync()
// );
// todoItems.MapGet(
//     "/{id}",
//     async (int id, TodoDb db) =>
//     await db.Todos.FindAsync(id)
//         is Todo todo ? Results.Ok(todo) : Results.NotFound()  
// );
// todoItems.MapPost(
//     "/",
//     async (Todo todo, TodoDb db) =>
//     {
//         db.Todos.Add(todo);
//         await db.SaveChangesAsync();
//         return Results.Created($"/todoitems/{todo.Id}", todo);
//     }
// );
// todoItems.MapPut(
//     "/{id}",
//     async (int id, Todo inputTodo, TodoDb db) =>
//     {
//         var todo = await db.Todos.FindAsync(id);
//         if (todo is null) return Results.NotFound();

//         todo.Name = inputTodo.Name;
//         todo.IsComplete = inputTodo.IsComplete;
        
//         await db.SaveChangesAsync();
//         return Results.NoContent(); 
//     }
// );
// todoItems.MapDelete(
//     "/{id}",
//     async (int id, TodoDb db) =>
//     {
//         if (await db.Todos.FindAsync(id) is Todo todo)
//         {
//             db.Todos.Remove(todo);
//             await db.SaveChangesAsync();
//             return Results.NoContent();

//         }
//     return Results.NotFound();
//     }
// );
