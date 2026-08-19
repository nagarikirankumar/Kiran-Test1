# Kiran-Test1

ASP.NET Core Web API solution with a user creation module.

## Layout

- `src/KiranTest1.Api` — Web API (`Models`, `Dtos`, `Repositories`, `Services`, `Controllers`)
- `tests/KiranTest1.Tests` — xUnit unit tests (Moq, coverlet)

## Endpoints

| Method | Route                 | Description                  |
| ------ | --------------------- | ---------------------------- |
| POST   | `/api/users`          | Create a user (201/400/409)  |
| GET    | `/api/users/{id}`     | Get a user by id (200/404)   |

Users are stored in an in-memory repository; passwords are hashed with PBKDF2-SHA256.

## Commands

```bash
dotnet build
dotnet test --collect:"XPlat Code Coverage"
dotnet run --project src/KiranTest1.Api
```
