# ClothingStoreApp

ASP.NET Core MVC application using **ASP.NET Identity** + **Role-Based Access Control (RBAC)** and a **SQLite** database. Users can register/login and place orders; Managers/Admins can view/manage orders; Admins can manage users/roles and review audit logs.

## Repository Link (for submission)

After you push this project to GitHub, paste your repository URL here:

`https://github.com/<anamika-rawat98>/<MVC_Final_Project>`

## Features

- Authentication (Register / Login / Logout) using ASP.NET Core Identity
- Roles: **Admin**, **Manager**, **User**
- Orders:
  - Users can create orders and view their own orders
  - Managers/Admins can view all orders; Managers can update order status
- Admin dashboard:
  - Manage users (create/edit/lock/unlock/reset password/delete)
  - Manage roles (create/edit/delete/assign/remove)
  - View audit logs (last 500)

## Tech Stack

- .NET **10** (ASP.NET Core MVC)
- Entity Framework Core **10**
- ASP.NET Core Identity (users + roles)
- SQLite database (`ClothingStoreApp.db`)

## Setup / Run Locally

### Prerequisites

- .NET SDK **10.0**
- (Optional) EF Core CLI for migrations: `dotnet-ef`

### Run

```bash
dotnet restore
dotnet run
```

The console output shows the local URL (for example `https://localhost:xxxx`). Open it in your browser.

### Database / Migrations

This project uses SQLite via the connection string in `appsettings.json`:

```json
"DefaultConnection": "Data Source=ClothingStoreApp.db"
```

If you delete `ClothingStoreApp.db` (or are cloning fresh), apply migrations before running:

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update
dotnet run
```

## Roles & Pages

### User

- Place an order: `GET /Orders/Create`
- My orders: `GET /Orders`

### Manager (and Admin)

- View all orders: `GET /ManagerOrders`
- Update order status: `POST /ManagerOrders/UpdateStatus`

### Admin

- Users: `GET /AdminUsers`
- Roles: `GET /AdminRoles`
- Orders: `GET /AdminOrders`
- Audit logs: `GET /AdminAuditLogs`

## Default Test Accounts (Seeded)

On startup, `Data/DbInitializer.cs` seeds these roles and demo accounts:

| Role | Username / Email | Password |
|------|-------------------|----------|
| Admin | `admin@clothingstore.com` | `Admin123!` |
| Manager | `manager@clothingstore.com` | `Manager123!` |
| User | `user@clothingstore.com` | `User123!` |

## Notes

- Password policy requires: min 8 chars, upper/lowercase, digit, and a non-alphanumeric character.
- Lockout is enabled after repeated failed login attempts.

