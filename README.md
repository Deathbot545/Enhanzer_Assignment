# Enhanzer Purchase Bill Application

A full-stack web application for managing purchase bills with item selection, batch management, and real-time calculations.

**Tech Stack**: Angular 20 | .NET 7 | SQL Server LocalDB

## 📋 Quick Start

### Prerequisites
- **Node.js** v18+ - [Download](https://nodejs.org/)
- **.NET 7 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/7.0)
- **SQL Server LocalDB** - [Download](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- **Git** - [Download](https://git-scm.com/)

### Clone & Install
```bash
git clone https://github.com/Deathbot545/Enhanzer_Assignment.git
cd applican

# Backend setup
cd backend
dotnet restore
dotnet build

# Frontend setup (new terminal)
cd frontend
npm install
```

### Run
```bash
# Terminal 1: Backend API
cd backend
dotnet run
# Expected: https://localhost:5007

# Terminal 2: Frontend Dev Server
cd frontend
npm start
# Expected: http://localhost:4200
```

## 🔐 Login Credentials

| Field | Value |
|-------|-------|
| **Email** | info@enhanzer.com |
| **Password** | Welcome#3 |

**Note:** Authenticates against: `https://ez-staging-api.azurewebsites.net/api/External_Api/POS_Api/Invoke`

On successful login:
- Creates `ApplicanAssignmentDb` database
- Saves all user locations to database
- Redirects to Purchase Bill page

---

## ✨ Features

### Login Page
✅ Email and password validation  
✅ Password visibility toggle  
✅ External API authentication  
✅ Automatic database initialization  
✅ Error message display  

### Purchase Bill Page
✅ **Item Autocomplete** - 7 items (Mango, Apple, Banana, Orange, Grapes, Kiwi, Strawberry)  
✅ **Batch Selection** - Dropdown from saved locations  
✅ **Real-time Calculations**:
   - Total Cost = (Standard Cost × Quantity) × (1 - Discount/100)
   - Total Selling = Standard Price × Quantity  
✅ **Dynamic Item Table** - Add/Remove items with full calculations  
✅ **Summary Dashboard** - Shows totals and financial metrics  
✅ **Professional UI** - Responsive gradient design  
✅ **Route Protection** - Auth guard prevents unauthorized access  

---

### Login Page
![Login Page](./screenshots/login-page.png)

**Key Elements:**
- Blue gradient header
- Email input with validation
- Password field with toggle
- Professional LOGIN button
- Error message area

### Purchase Bill Page
![Purchase Bill Add Page](./screenshots/purchase-bill-page.png)

**Key Elements:**
- Header with action buttons
- Item autocomplete form
- Batch dropdown from database
- Real-time calculation display
- Dynamic item table
- Right sidebar with financial summary

---

## 🔌 API Endpoints

```bash
# Login
POST /api/auth/login
{ "email": "info@enhanzer.com", "password": "Welcome#3" }
Response: Array of locations

# Get Locations
GET /api/locations
Response: Array of location objects

# Create Purchase Bill
POST /api/purchasebills
{ "items": [...] }
Response: Bill created with ID
```

---

## 🛠 Project Structure

```
applican/
├── backend/                    # .NET 7 Web API
│   ├── Controllers/            # API endpoints
│   ├── Services/               # Business logic
│   ├── Models/                 # Data models
│   └── Program.cs              # Startup config
│
├── frontend/                   # Angular 20 App
│   ├── src/app/
│   │   ├── pages/              # Login & Purchase Bill pages
│   │   ├── services/           # HTTP services
│   │   ├── guards/             # Auth protection
│   │   └── app.routes.ts       # Routes
│
├── screenshots/                # Add images here
├── .gitignore
└── README.md
```

---

## 🐛 Troubleshooting

### "Address already in use (5007)"
```bash
# Windows PowerShell
$process = Get-NetTCPConnection -LocalPort 5007 -State Listen | Select-Object OwningProcess
Stop-Process -Id $process.OwningProcess -Force
```

### "Cannot connect to database"
```bash
# Start SQL Server LocalDB
sqllocaldb start mssqllocaldb
```

### "Can't find module"
```bash
cd frontend
npm install
```

### Port 4200 in use
```bash
ng serve --port 4300
```

### Invalid credentials
- Verify email: `info@enhanzer.com`
- Verify password: `Welcome#3`
- Check for extra spaces
- Ensure backend is running

---

## 📝 Notes

- Database persists across logins
- Only one database created (reused on subsequent logins)
- Frontend uses Reactive Forms with validation
- Backend uses Entity Framework Core
- CORS enabled for localhost:4200

---

## 🔗 Links

- **Repository**: https://github.com/Deathbot545/Enhanzer_Assignment
- **.NET 7 Docs**: https://docs.microsoft.com/dotnet/core/whats-new/dotnet-7
- **Angular Docs**: https://angular.io/docs
- **SQL Server LocalDB**: https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb

---

**Version**: 1.0.0 | **Last Updated**: March 19, 2026
