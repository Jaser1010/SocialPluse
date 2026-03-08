# SocialPulse

### Enterprise-Grade Social Networking Platform · ASP.NET Core 10 · Clean Architecture

A production-ready, modular social networking API built with clean architecture principles — featuring JWT-based authentication, ASP.NET Core Identity, Entity Framework Core, automatic migrations, and Scalar API documentation. Designed for scalability, testability, and future real-time social features.

[📖 Docs](#-api-reference) · [🚀 Quick Start](#-quick-start) · [🐛 Report Bug](https://github.com/Jaser1010/SocialPluse/issues) · [💡 Request Feature](https://github.com/Jaser1010/SocialPluse/issues)

[![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14.0-239120?style=for-the-badge&logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/license-MIT-blue?style=for-the-badge)](LICENSE)

---

## 📑 Table of Contents

- [Highlights](#-highlights)
- [Architecture Overview](#-architecture-overview)
- [Solution Structure](#-solution-structure)
- [Tech Stack](#%EF%B8%8F-tech-stack)
- [API Reference](#-api-reference)
- [Quick Start](#-quick-start)
- [Configuration](#%EF%B8%8F-configuration)
- [Design Patterns](#-design-patterns)
- [Roadmap](#-roadmap)
- [Contributing](#-contributing)
- [License](#-license)
- [Author](#-author)

---

## ⚡ Highlights

| Feature | Description |
|---|---|
| 🏛️ **Clean Architecture** | 7 dedicated projects with strict inward-only dependency flow |
| 🔐 **JWT Authentication** | User registration, login & role-based authorization via ASP.NET Core Identity |
| 📖 **Interactive API Docs** | Beautiful Scalar UI + OpenAPI specification |
| 🔄 **Auto Migrations** | Database schema updates applied automatically in development |
| 🛡️ **Global Exception Handling** | Consistent error responses with proper HTTP status codes |
| 📦 **Modular Design** | Clear separation: Domain → Application → Infrastructure → Presentation |
| ⚡ **High Performance** | Built on .NET 10 with optimized EF Core patterns |
| 🔒 **Security First** | HTTPS, authentication & authorization middleware configured |

---

## 🏛 Architecture Overview

Clean Architecture (Onion) with **strict inward-only dependencies**:

```
┌────────────────────────────────────────────────────────────────┐
│              Presentation (Web + Controllers)                  │
├────────────────────────────────────────────────────────────────┤
│           Application (Services + Abstractions)                │
├────────────────────────────────────────────────────────────────┤
│               Infrastructure (Persistence)                     │
├────────────────────────────────────────────────────────────────┤
│          Domain (Entities + Contracts) [No Dependencies]       │
└────────────────────────────────────────────────────────────────┘
              ▲ Dependencies flow inward only ▲
```

---

## 📁 Solution Structure

```
SocialPluse.sln
│
├── SocialPluse.Domain/              # Core business entities (zero dependencies)
│   ├── Entities/                    # User, Post, Comment, etc.
│   └── Contracts/                   # Repository interfaces
│
├── SocialPluse.Persistence/         # Data access implementation
│   ├── Data/                        # DbContext, Migrations, Configurations
│   └── Repositories/                # Repository implementations
│
├── SocialPluse.Services.Abstraction/
│   ├── IAuthService.cs
│   └── DTOs/                        # LoginRequest, RegisterRequest, AuthResponse
│
├── SocialPluse.Services/            # Business logic implementation
│   ├── AuthService.cs
│   ├── MappingProfiles/
│   └── DependencyInjection.cs
│
├── SocialPluse.Presentation/
│   └── Controllers/                 # AuthController, ApiBaseController
│
├── SocialPluse.Shared/              # Cross-cutting utilities
│   ├── Result/                      # Result pattern
│   └── Extensions/
│
└── SocialPluse.Web/                 # Application entry point
    ├── Extensions/                  # MigrationExtensions
    ├── Program.cs
    └── appsettings.json
```

---

## 🛠️ Tech Stack

| Category | Technology |
|---|---|
| **Framework** | ASP.NET Core 10 |
| **Language** | C# 14.0 |
| **ORM** | Entity Framework Core |
| **Database** | SQL Server |
| **Authentication** | ASP.NET Core Identity + JWT Bearer |
| **API Docs** | Scalar + OpenAPI 3.0 |
| **Architecture** | Clean/Onion Architecture |

**Future Integrations:** Redis (Phase 2), SignalR (Phase 2), Azure Storage (Phase 1), Elasticsearch (Phase 4), Docker (Phase 6)

---

## 📖 API Reference

### 🔐 Authentication — `/api/auth`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | ❌ | Register new user |
| `POST` | `/api/auth/login` | ❌ | Login and receive JWT |
| `GET` | `/api/auth/me` | ✅ JWT | Get current user (Future) |

**Register/Login Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "displayName": "John Doe"
}
```

**Response:**
```json
{
  "userId": "guid",
  "email": "user@example.com",
  "displayName": "John Doe",
  "token": "<jwt-token>",
  "expiresAt": "2025-01-15T12:00:00Z"
}
```

---

### 👥 Users — `/api/users` *(Phase 1)*

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/users/{id}` | ✅ JWT | Get user profile |
| `PUT` | `/api/users/{id}` | ✅ JWT | Update profile |
| `POST` | `/api/users/{id}/avatar` | ✅ JWT | Upload avatar |
| `GET` | `/api/users/search?q=` | ✅ JWT | Search users |

---

### 📝 Posts — `/api/posts` *(Phase 1)*

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/posts` | ✅ JWT | Get feed (paginated) |
| `GET` | `/api/posts/{id}` | ✅ JWT | Get single post |
| `POST` | `/api/posts` | ✅ JWT | Create post |
| `PUT` | `/api/posts/{id}` | ✅ JWT | Update post |
| `DELETE` | `/api/posts/{id}` | ✅ JWT | Delete post |
| `POST` | `/api/posts/{id}/like` | ✅ JWT | Like/unlike |

---

### 💬 Comments — `/api/comments` *(Phase 1)*

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/posts/{postId}/comments` | ✅ JWT | Add comment |
| `PUT` | `/api/comments/{id}` | ✅ JWT | Edit comment |
| `DELETE` | `/api/comments/{id}` | ✅ JWT | Delete comment |

---

### 🤝 Connections — `/api/connections` *(Phase 1)*

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/connections/request` | ✅ JWT | Send friend request |
| `POST` | `/api/connections/accept` | ✅ JWT | Accept request |
| `DELETE` | `/api/connections/{id}` | ✅ JWT | Remove connection |
| `GET` | `/api/connections/followers` | ✅ JWT | Get followers |
| `GET` | `/api/connections/following` | ✅ JWT | Get following |

---

### 💬 Messages — `/api/messages` *(Phase 2 - SignalR)*
### 🔔 Notifications — `/api/notifications` *(Phase 2)*
### 👥 Groups — `/api/groups` *(Phase 3)*

*Full endpoint documentation coming with implementation*

---

## 🚀 Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server](https://www.microsoft.com/sql-server) or SQL Server Express
- [Visual Studio 2026](https://visualstudio.microsoft.com/) or VS Code
- [Git](https://git-scm.com/)

### Step-by-Step Setup

**1. Clone the repository**
```powershell
git clone https://github.com/Jaser1010/SocialPluse.git
cd SocialPulse
```

**2. Configure the database** (`SocialPluse.Web/appsettings.Development.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SocialPulseDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "<your-secret-key-min-32-characters>",
    "Issuer": "SocialPulse",
    "Audience": "SocialPulseUsers",
    "ExpiryMinutes": 60
  }
}
```

**3. Restore & build**
```powershell
dotnet restore
dotnet build
```

**4. Run the application**
```powershell
dotnet run --project SocialPluse.Web
```

Or press **F5** in Visual Studio.

**5. Access the API**
- 🌐 Base: `https://localhost:5001`
- 📖 Scalar Docs: `https://localhost:5001/scalar`
- 📝 OpenAPI: `https://localhost:5001/openapi`

> Migrations apply automatically in Development mode

---

## ⚙️ Configuration

| Key | Description | Example |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection | `Server=.;Database=SocialPulseDb;Trusted_Connection=True;` |
| `JwtSettings:SecretKey` | JWT signing key (32+ chars) | `your-super-secret-key-here` |
| `JwtSettings:Issuer` | Token issuer | `SocialPulse` |
| `JwtSettings:Audience` | Token audience | `SocialPulseUsers` |
| `JwtSettings:ExpiryMinutes` | Token expiration | `60` |

---

## 🔬 Design Patterns

### Result Pattern
Service methods return `Result<T>` for predictable error handling without exceptions:

```csharp
var result = await _authService.LoginAsync(request);
if (result.IsFailure)
    return BadRequest(result.Error);
return Ok(result.Value);
```

### Dependency Injection
All dependencies registered via extension methods:

```csharp
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddServices();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
```

### Repository Pattern *(Future)*
Generic repository for data access abstraction.

### Unit of Work Pattern *(Future)*
Transactional consistency across multiple repositories.

---

## 🗺️ Roadmap

### Phase 1: Core Social Features (Q1-Q2 2025)
- [ ] User profiles (view, edit, avatar/cover upload)
- [ ] Social connections (friend requests, follow/unfollow)
- [ ] Content creation (posts with rich media)
- [ ] Social interactions (like, comment, share, mentions)

### Phase 2: Advanced Engagement (Q3 2025)
- [ ] Real-time messaging (SignalR one-on-one & group chat)
- [ ] Notifications system (real-time push, in-app, email)
- [ ] Content discovery (personalized feed, trending, hashtags)
- [ ] Stories (24-hour ephemeral content)

### Phase 3: Community & Groups (Q4 2025)
- [ ] Groups & communities (public/private with roles)
- [ ] Events management (RSVP, reminders, calendar)
- [ ] Content moderation (AI-powered, reporting, admin tools)

### Phase 4: Intelligence & Analytics (Q1 2026)
- [ ] AI-powered features (recommendations, sentiment analysis)
- [ ] Analytics & insights (engagement metrics, growth tracking)
- [ ] Advanced search (Elasticsearch full-text & semantic search)

### Phase 5: Enterprise & Monetization (Q2 2026)
- [ ] Business accounts (verified badges, sponsored posts)
- [ ] Marketplace (product listings, reviews, payments)
- [ ] Live streaming (broadcast, chat, recording)
- [ ] Public API & webhooks (OAuth2, rate limiting)

### Phase 6: Global Scale & Performance (Q3-Q4 2026)
- [ ] Multi-region support (CDN, localization, i18n)
- [ ] Performance optimization (Redis, GraphQL, message queues)
- [ ] Mobile & cross-platform (SDKs, PWA, offline sync)

---

## 🤝 Contributing

Contributions welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m "feat: add amazing feature"`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

**Commit Convention:** `feat:`, `fix:`, `docs:`, `refactor:`, `test:`, `chore:`

---

## 📄 License

This project is licensed under the **MIT License**.

---

## 👤 Author

**Jaser Kasim**

[![GitHub](https://img.shields.io/badge/GitHub-Jaser1010-181717?style=for-the-badge&logo=github)](https://github.com/Jaser1010)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Connect-0077B5?style=for-the-badge&logo=linkedin)](https://linkedin.com/in/jaser-kasim)

---

<div align="center">

### 🚀 Built with .NET 10 for the future of social networking

**[⬆ Back to Top](#socialpluse)**

<sub>Made with ❤️ by [Jaser Kasim](https://github.com/Jaser1010) · ⭐ Star if you find this helpful!</sub>

</div>