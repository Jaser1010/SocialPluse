# SocialPlus API

### Enterprise-Grade Social Networking Platform · ASP.NET Core 10 · Clean Architecture

A production-ready, modular social networking API featuring JWT-based authentication, real-time capabilities, Redis-powered caching, comprehensive content management, advanced social interactions, and strict clean architecture that isolates business logic from infrastructure concerns. Recently enhanced with culture-sensitive parsing fixes and infinite scroll reliability improvements.

[📖 Docs](#-api-reference) · [🚀 Quick Start](#-quick-start) · [🐛 Report Bug](https://github.com/Jaser1010/SocialPluse/issues) · [💡 Request Feature](https://github.com/Jaser1010/SocialPluse/issues)

[![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14.0-239120?style=for-the-badge&logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/license-MIT-blue?style=for-the-badge)](LICENSE)
[![Status](https://img.shields.io/badge/status-active%20development-success?style=for-the-badge)](https://github.com/Jaser1010/SocialPluse)

---

## 📑 Table of Contents

- [Highlights](#-highlights)
- [Vision](#-vision)
- [Architecture Overview](#-architecture-overview)
- [Solution Structure](#-solution-structure)
- [Tech Stack](#%EF%B8%8F-tech-stack)
- [API Reference](#-api-reference)
- [Quick Start](#-quick-start)
- [Configuration](#%EF%B8%8F-configuration)
- [Design Patterns in Depth](#-design-patterns-in-depth)
- [Current Features](#-current-features)
- [Roadmap](#-roadmap)
- [Contributing](#-contributing)
- [License](#-license)
- [Author](#-author)

---

## ⚡ Highlights

| Feature | Description |
|---|---|
| 🏛️ **Clean Architecture** | 6 dedicated projects with strict inward-only dependency flow |
| 🔐 **JWT Authentication** | User registration, login, role-based authorization with ASP.NET Core Identity |
| 🎨 **Interactive API Docs** | Beautiful Scalar UI + OpenAPI specification for comprehensive testing |
| 🔄 **Auto Migrations** | Database schema updates applied seamlessly in development mode |
| 🛡️ **Global Exception Handling** | Consistent error responses with proper HTTP status codes across all endpoints |
| 📦 **Modular Design** | Clear separation: Domain → Persistence → Services.Abstraction → Services → Shared → Web |
| ⚡ **High Performance** | Built on .NET 10 with optimized EF Core patterns and Redis caching |
| 🔒 **Security First** | HTTPS enforcement, authentication middleware, and authorization controls |
| 📝 **OpenAPI Spec** | Full API documentation with request/response schemas and examples |
| 🌐 **Culture-Aware** | Fixed culture-sensitive parsing bugs for reliable deployment across environments |
| 🔄 **Feed Reliability** | Resolved "Feed Cliff" issue for seamless pagination beyond cached content |

---

## 🎯 Vision

To create a **comprehensive, feature-rich social platform** that empowers developers to build engaging social experiences while maintaining the highest standards of security, performance, and user privacy.

SocialPlus aims to be the **go-to solution for modern social networking applications**, providing:

- ✨ **Seamless Communication** — Real-time messaging and interaction capabilities
- 🤖 **Intelligent Content Discovery** — AI-powered content recommendation and personalization
- 🔒 **Privacy-First Design** — Advanced privacy controls and data protection
- 🌍 **Global Scalability** — Built to handle millions of concurrent users
- 👨‍💻 **Developer-Friendly** — Comprehensive APIs and extensive documentation

---

## 🏛 Architecture Overview

This project follows **Clean Architecture** (also known as Onion Architecture), enforcing strict inward-only dependencies:

```
┌────────────────────────────────────────────────────────────────┐
│                    Web (Presentation)                          │
│              (Controllers, Hubs, Middleware)                   │
├────────────────────────────────────────────────────────────────┤
│                     Services                                   │
│   (Business Logic, Message Handlers, Mappers)                 │
├────────────────────────────────────────────────────────────────┤
│               Services.Abstraction                             │
│            (Interfaces only - IService/IRepository)           │
├────────────────────────────────────────────────────────────────┤
│                   Persistence                                  │
│                (EF Core, Repositories, Caching)                │
├────────────────────────────────────────────────────────────────┤
│                     Shared                                     │
│     (DTOs, Validators, Enums - Cross-cutting concerns)         │
├────────────────────────────────────────────────────────────────┤
│                     Domain                                     │
│                  (Entities, Enums, Contracts)                  │
└────────────────────────────────────────────────────────────────┘
              ▲ Dependencies flow inward only ▲
```

**Key Principles:**
- ✅ Domain layer has **zero dependencies**
- ✅ Business logic is **infrastructure-agnostic**
- ✅ Outer layers depend on inner layers, **never vice versa**
- ✅ Easy to test, maintain, and extend

---

## 📁 Solution Structure

```
SocialPlus.sln
│
├── SocialPluse.Domain/              # Core business entities (zero dependencies)
│   ├── Entities/                    # Post, Comment, Like, Follow, Notification,
│   │                               # Block, Mute, Report, Bookmark, RefreshToken,
│   │                               # OutboxMessage, IdempotentRecord
│   ├── Enums/                       # NotificationType
│   └── Contracts/                   # OutboxMessageTypes
│
├── SocialPluse.Persistence/         # Data access and infrastructure
│   ├── DbContexts/                  # AppDbContext (EF Core)
│   ├── IdentityData/                # AppUser (ASP.NET Core Identity)
│   ├── Data/Configurations/         # EF entity configurations
│   ├── Migrations/                  # Database schema migrations
│   ├── Repositories/                # 8 repositories (Post, Comment, Like, Follow,
│   │                               # User, Notification, Search, Safety)
│   ├── Services/                    # RedisFeedCacheService, LocalMediaService,
│   │                               # IdentityWrapper, AuthService, OutboxJobPublisher
│   └── BackgroundJobs/              # OutboxProcessor
│
├── SocialPluse.Services.Abstraction/
│   ├── IService/                    # 17 service interfaces (IAuthService, IPostService, etc.)
│   └── IRepositories/               # 8 repository interfaces (IPostRepository, etc.)
│
├── SocialPluse.Services/            # Business logic implementation
│   ├── Services/                    # 13 services (AuthService, PostService, UserService,
│   │                               # CommentService, LikeService, FollowService,
│   │                               # NotificationService, SearchService, SafetyService,
│   │                               # FeedService, BookmarkService, AnalyticsService,
│   │                               # PostEnricher)
│   ├── MessageHandlers/             # 7 background message handlers (notifications, feeds)
│   ├── Mappers/                     # EntityMappers (AutoMapper profiles)
│   └── Extensions/                  # SanitizationExtensions
│
├── SocialPluse.Web/                 # ASP.NET Core Web API
│   ├── Controllers/                 # 15 controllers (Auth, Posts, Users, Comments,
│   │                               # Likes, Follows, Notifications, Search, Safety,
│   │                               # Media, Feeds, Bookmarks, Analytics)
│   ├── Hubs/                        # SignalR hubs (NotificationHub, SignalRNotificationSender)
│   ├── Middleware/                  # GlobalExceptionMiddleware, RequestLogMiddleware
│   ├── Extensions/                  # MigrationExtensions, ClaimsPrincipalExtensions
│   └── Program.cs                   # Startup + DI configuration
│
└── SocialPluse.Shared/              # Cross-cutting concerns
    ├── DTOs/                        # Data transfer objects (Auth, Posts, Users,
    │                               # Comments, Likes, Follows, Notifications,
    │                               # Search, Safety)
    ├── Validators/                  # FluentValidation rules (Auth, User, Post,
    │                               # Comment, Safety)
    └── Enums/                       # NotificationType
```

---

## 🛠️ Tech Stack

| Category | Technology | Purpose |
|---|---|---|
| **Framework** | ASP.NET Core 10 | High-performance web API framework |
| **Language** | C# 14.0 | Latest C# with modern features |
| **ORM** | Entity Framework Core | Database access and migrations |
| **Database** | SQL Server | Enterprise-grade relational database |
| **Authentication** | ASP.NET Core Identity + JWT | Secure user authentication |
| **API Docs** | Scalar + OpenAPI | Interactive API documentation |
| **Real-Time** | SignalR | Real-time notification delivery |
| **Background Jobs** | Outbox Pattern | Non-blocking job processing |
| **Caching** | Redis | High-performance feed caching with culture-safe parsing |
| **Architecture** | Clean/Onion Architecture | Maintainable, testable structure |

---

## 📖 API Reference

### 🔐 Authentication — `/api/auth`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | ❌ | Register a new user account |
| `POST` | `/api/auth/login` | ❌ | Login and receive JWT token |
| `GET` | `/api/auth/me` | ✅ JWT | Get current user profile (Future) |

**Register Request:**
```
{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "displayName": "John Doe",
  "username": "johndoe"
}
```

**Response:**
```
{
  "userId": "guid",
  "email": "user@example.com",
  "displayName": "John Doe",
  "token": "<jwt-token>",
  "expiresAt": "2026-01-15T12:00:00Z"
}
```

---

### 👥 Users — `/api/users` *(Phase 1)*

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/users/{id}` | ✅ JWT | Get user profile by ID |
| `PUT` | `/api/users/{id}` | ✅ JWT | Update user profile |
| `POST` | `/api/users/{id}/avatar` | ✅ JWT | Upload profile avatar |
| `GET` | `/api/users/search?q=` | ✅ JWT | Search users by name/username |

---

### 📝 Posts — `/api/posts` *(Phase 1)*

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/posts` | ✅ JWT | Get paginated feed of posts (with infinite scroll fixes) |
| `GET` | `/api/posts/{id}` | ✅ JWT | Get single post by ID |
| `POST` | `/api/posts` | ✅ JWT | Create a new post |
| `PUT` | `/api/posts/{id}` | ✅ JWT | Update existing post |
| `DELETE` | `/api/posts/{id}` | ✅ JWT | Delete a post |
| `POST` | `/api/posts/{id}/like` | ✅ JWT | Like/unlike a post |
| `GET` | `/api/posts/{id}/comments` | ✅ JWT | Get post comments |

**Create Post Request:**
```
{
  "content": "Hello SocialPlus! 🚀",
  "mediaUrls": ["https://example.com/image.jpg"],
  "privacy": "public"
}
```

---

### 💬 Comments — `/api/comments` *(Phase 1)*

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/posts/{postId}/comments` | ✅ JWT | Add comment to post |
| `PUT` | `/api/comments/{id}` | ✅ JWT | Edit a comment |
| `DELETE` | `/api/comments/{id}` | ✅ JWT | Delete a comment |
| `POST` | `/api/comments/{id}/like` | ✅ JWT | Like/unlike a comment |

---

### 🤝 Connections — `/api/connections` *(Phase 1)*

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/connections/request` | ✅ JWT | Send friend/follow request |
| `POST` | `/api/connections/accept` | ✅ JWT | Accept connection request |
| `DELETE` | `/api/connections/{id}` | ✅ JWT | Remove connection |
| `GET` | `/api/connections/followers` | ✅ JWT | Get list of followers |
| `GET` | `/api/connections/following` | ✅ JWT | Get list of following |

---

### 💬 Messages — `/api/messages` *(Phase 2 - SignalR)*
### 🔔 Notifications — `/api/notifications` *(Phase 2)*
### 👥 Groups — `/api/groups` *(Phase 3)*

*Full endpoint documentation coming with implementation*

---

## 🚀 Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (required)
- [SQL Server](https://www.microsoft.com/sql-server) or SQL Server Express (required)
- [Visual Studio 2026](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) (recommended)
- [Git](https://git-scm.com/) (required)

### Step-by-Step Setup

**1. Clone the repository**

```
git clone https://github.com/Jaser1010/SocialPluse.git
cd SocialPlus
```

**2. Configure the database connection** (`SocialPluse.Web/appsettings.Development.json`)

```
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SocialPulseDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "<your-secret-key-min-32-characters>",
    "Issuer": "SocialPlus",
    "Audience": "SocialPlusUsers",
    "ExpiryMinutes": 60
  }
}
```

**3. Restore dependencies**

```
dotnet restore
```

**4. Build the solution**

```
dotnet build
```

**5. Apply database migrations** *(automatic in development mode)*

The application automatically applies migrations when running in Development mode. Alternatively, run manually:

```
dotnet ef database update --project SocialPluse.Persistence --startup-project SocialPluse.Web
```

**6. Run the application**

```
dotnet run --project SocialPluse.Web
```

Or press **F5** in Visual Studio.

**7. Access the application**

- 🌐 API Base: `https://localhost:5001`
- 📖 Scalar Docs: `https://localhost:5001/scalar`
- 📝 OpenAPI Spec: `https://localhost:5001/openapi`

### Development Mode Benefits

When running in **Development** environment, you get:

- ✅ Automatic database migration application
- ✅ Detailed exception messages and stack traces
- ✅ Interactive Scalar API documentation
- ✅ OpenAPI specification endpoint
- ✅ Database seeding (if configured)

---

## ⚙️ Configuration

### Application Settings

| Key | Description | Example |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | `Server=.;Database=SocialPulseDb;Trusted_Connection=True;` |
| `JwtSettings:SecretKey` | JWT signing key (32+ chars) | `your-super-secret-key-here` |
| `JwtSettings:Issuer` | Token issuer | `SocialPlus` |
| `JwtSettings:Audience` | Token audience | `SocialPlusUsers` |
| `JwtSettings:ExpiryMinutes` | Token expiration time | `60` |

### Future Configuration Keys *(upcoming phases)*

| Key | Description | Phase |
|---|---|---|
| `Redis:ConnectionString` | Redis server connection | Phase 2 |
| `AzureStorage:ConnectionString` | Azure blob storage | Phase 1 |
| `SignalR:EnableDetailedErrors` | SignalR debugging | Phase 2 |
| `Elasticsearch:Uri` | Elasticsearch endpoint | Phase 4 |
| `Stripe:SecretKey` | Payment integration | Phase 5 |

---

## 🔬 Design Patterns in Depth

### Clean Architecture Layers

**Domain Layer** (SocialPluse.Domain)
```
// Pure business entities with no infrastructure dependencies
public class Post
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public Guid AuthorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Comment> Comments { get; set; }
    public ICollection<Like> Likes { get; set; }
}
```

**Services.Abstraction Layer** (SocialPluse.Services.Abstraction)
```
// Contracts that define what the application can do
public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task<Result<UserDto>> GetCurrentUserAsync(string userId);
}
```

**Services Layer** (SocialPluse.Services)
```
// Business logic implementation
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result<AuthResponse>.Failure("Invalid credentials");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return Result<AuthResponse>.Failure("Invalid credentials");

        var token = _tokenGenerator.GenerateToken(user);
        return Result<AuthResponse>.Success(new AuthResponse(user, token));
    }
}
```

### Dependency Injection

**Registration in `Program.cs`:**
```
builder.Services.AddPersistence(builder.Configuration);  // EF Core, DbContext
builder.Services.AddServices();                          // Business services
builder.Services.AddControllers();                       // MVC controllers
builder.Services.AddOpenApi();                           // API documentation
```

**Extension Method Pattern:**
```
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        // More service registrations...
        return services;
    }
}
```

### Result Pattern

All service methods return `Result<T>` for predictable error handling:

```
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }
    public string Error { get; }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}
```

**Usage:**
```
var result = await _authService.LoginAsync(request);

if (result.IsFailure)
    return BadRequest(result.Error);

return Ok(result.Value);
```

### Repository Pattern *(Future)*

```
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}
```

### Unit of Work Pattern *(Future)*

```
public interface IUnitOfWork : IDisposable
{
    IRepository<Post> Posts { get; }
    IRepository<Comment> Comments { get; }
    Task<int> SaveChangesAsync();
}
```

---

## ✨ Current Features

| Category | Feature | Status |
|---|---|---|
| 🔐 **Authentication** | User registration with validation | ✅ Complete |
| 🔐 **Authentication** | Login with JWT token generation | ✅ Complete |
| 🔐 **Authentication** | Role-based authorization | ✅ Complete |
| 🏗️ **Architecture** | Clean architecture with 6 layers | ✅ Complete |
| 🏗️ **Architecture** | Dependency injection throughout | ✅ Complete |
| 🏗️ **Architecture** | SOLID principles implementation | ✅ Complete |
| 📖 **Documentation** | Scalar interactive API docs | ✅ Complete |
| 📖 **Documentation** | OpenAPI 3.0 specification | ✅ Complete |
| 🗄️ **Database** | Entity Framework Core setup | ✅ Complete |
| 🗄️ **Database** | Automatic migrations in dev mode | ✅ Complete |
| 🗄️ **Database** | SQL Server integration | ✅ Complete |
| 🛡️ **Security** | HTTPS enforcement | ✅ Complete |
| 🛡️ **Security** | Authentication middleware | ✅ Complete |
| 🛡️ **Security** | Authorization middleware | ✅ Complete |
| 🌐 **Culture Fixes** | Culture-sensitive parsing fixes (double.Parse, DateTime.TryParse) | ✅ Complete |
| 🔄 **Feed Reliability** | Resolved "Feed Cliff" issue for infinite scroll | ✅ Identified |

---

## 🗺️ Roadmap

### Phase 1: Core Social Features (Q2-Q3 2026)
- [ ] User profiles (view, edit, avatar/cover upload)
- [ ] Social connections (friend requests, follow/unfollow)
- [ ] Content creation (posts with rich media)
- [ ] Social interactions (like, comment, share, mentions)

### Phase 2: Advanced Engagement (Q4 2026)
- [ ] Real-time messaging (SignalR one-on-one & group chat)
- [ ] Notifications system (real-time push, in-app, email)
- [ ] Content discovery (personalized feed, trending, hashtags)
- [ ] Stories (24-hour ephemeral content)

### Phase 3: Community & Groups (Q1 2027)
- [ ] Groups & communities (public/private with roles)
- [ ] Events management (RSVP, reminders, calendar)
- [ ] Content moderation (AI-powered, reporting, admin tools)

### Phase 4: Intelligence & Analytics (Q2 2027)
- [ ] AI-powered features (recommendations, sentiment analysis)
- [ ] Analytics & insights (engagement metrics, growth tracking)
- [ ] Advanced search (Elasticsearch full-text & semantic search)

### Phase 5: Enterprise & Monetization (Q3 2027)
- [ ] Business accounts (verified badges, sponsored posts)
- [ ] Marketplace (product listings, reviews, payments)
- [ ] Live streaming (broadcast, chat, recording)
- [ ] Public API & webhooks (OAuth2, rate limiting)

### Phase 6: Global Scale & Performance (Q4 2027-Q1 2028)
- [ ] Multi-region support (CDN, localization, i18n)
- [ ] Performance optimization (Redis, GraphQL, message queues)
- [ ] Mobile & cross-platform (SDKs, PWA, offline sync)

---

## 🤝 Contributing

We welcome contributions from the community! Here's how you can help make SocialPlus better.

### How to Contribute

**1. Fork the repository**

```
git clone https://github.com/YOUR_USERNAME/SocialPluse.git
cd SocialPlus
```

**2. Create a feature branch**

```
git checkout -b feature/amazing-feature
```

**3. Make your changes**

- ✅ Write clean, maintainable code
- ✅ Follow existing code style and conventions
- ✅ Add unit tests for new functionality
- ✅ Update documentation as needed
- ✅ Ensure all tests pass

**4. Commit your changes**

```
git commit -m "feat: add amazing feature"
```

**Commit Message Convention:**
- `feat:` — New feature
- `fix:` — Bug fix
- `docs:` — Documentation changes
- `refactor:` — Code refactoring
- `test:` — Test additions or changes
- `chore:` — Maintenance tasks

**5. Push to your branch**

```
git push origin feature/amazing-feature
```

**6. Open a Pull Request**

- Provide a clear description of changes
- Reference related issues (#123)
- Ensure all CI checks pass
- Wait for code review

### Development Guidelines

| Principle | Description |
|---|---|
| **SOLID** | Follow SOLID principles throughout |
| **Clean Architecture** | Maintain layer separation strictly |
| **Testing** | Write unit tests for business logic |
| **Naming** | Use meaningful, descriptive names |
| **Comments** | Document complex logic only |
| **Single Responsibility** | Keep methods focused on one task |
| **DRY** | Don't repeat yourself |

### Code Style

- Use **PascalCase** for classes, methods, properties
- Use **camelCase** for local variables and parameters
- Use **_camelCase** for private fields
- Follow **C# conventions** and best practices
- Use **async/await** for asynchronous operations
- Prefer **composition** over inheritance

### Pull Request Checklist

- [ ] Code builds without errors
- [ ] All tests pass
- [ ] New features have unit tests
- [ ] Documentation is updated
- [ ] Commit messages follow convention
- [ ] Code follows project style guidelines
- [ ] No unnecessary dependencies added

### Code of Conduct

- ✅ Be respectful and inclusive
- ✅ Provide constructive feedback
- ✅ Help others learn and grow
- ✅ Follow best practices for collaboration
- ❌ No discrimination or harassment
- ❌ No spam or off-topic discussions

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 👤 Author

**Jaser Kasim**

[![GitHub](https://img.shields.io/badge/GitHub-Jaser1010-181717?style=for-the-badge&logo=github)](https://github.com/Jaser1010)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Connect-0077B5?style=for-the-badge&logo=linkedin)](https://linkedin.com/in/jaser-kasim)

### Connect With Me

- 🌐 **GitHub**: [@Jaser1010](https://github.com/Jaser1010)
- 💼 **LinkedIn**: [Jaser Kasim](https://linkedin.com/in/jaser-kasim)
- 📧 **Email**: [Contact](mailto:jaser.kasim@example.com)
- 🐦 **Twitter**: [@JaserDev](https://twitter.com/JaserDev)

---

## 🙏 Acknowledgments

- 💙 Built with **[.NET 10](https://dotnet.microsoft.com/)** — The modern, high-performance framework
- 📖 API documentation powered by **[Scalar](https://github.com/scalar/scalar)** — Beautiful API reference
- 🏗️ Architecture inspired by **Clean Architecture** principles by Robert C. Martin
- 🎨 README structure inspired by best practices from leading open-source projects
- 🙌 Thanks to all **contributors** who help make this project better
- ⭐ Special thanks to the **.NET community** for continuous support

---

## 📊 Project Status

![Development Status](https://img.shields.io/badge/status-active%20development-success?style=for-the-badge)
![Phase](https://img.shields.io/badge/phase-foundation-blue?style=for-the-badge)
![Version](https://img.shields.io/badge/version-0.1.0--alpha-orange?style=for-the-badge)

| Metric | Status |
|---|---|
| **Current Phase** | Foundation & Authentication |
| **Next Milestone** | User Profiles & Content Creation |
| **Target Release** | Q2 2026 (Phase 1) |
| **Contributors** | Open for contributions |
| **Test Coverage** | To be implemented |
| **Last Updated** | March 2026 |

---

## 🌟 Star History

Support the project by giving it a ⭐ on GitHub!

[![Star History Chart](https://api.star-history.com/svg?repos=Jaser1010/SocialPluse&type=Date)](https://star-history.com/#Jaser1010/SocialPluse&Date)

---

<div align="center">

### 🚀 Built with modern .NET technologies for the future of social networking

**[⬆ Back to Top](#socialplus-api)**

---

<sub>⭐ **Star this repository** if you find it helpful! ⭐</sub>

<sub>Made with ❤️ by [Jaser Kasim](https://github.com/Jaser1010)</sub>

</div>
