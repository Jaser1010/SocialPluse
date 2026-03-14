# SocialPlus API

> A production-style social media backend API built with .NET 10, Clean Architecture, PostgreSQL, Redis, SignalR, and Hangfire.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Features](#features)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Environment Configuration](#environment-configuration)
- [API Reference](#api-reference)
- [Database Schema](#database-schema)
- [Real-Time & Background Jobs](#real-time--background-jobs)
- [Feed Scaling](#feed-scaling)

---

## Overview

SocialPlus is a fully-featured social media backend API — think a mini Twitter/Instagram. It supports authentication, user profiles, posts, follows, likes, comments, real-time notifications, full-text search, safety controls, and a Redis-backed scalable feed.

Built as a learning project to apply **Clean/Onion Architecture** in a real-world system with genuine engineering trade-offs — not a tutorial, not a todo app.

---

## Architecture

SocialPlus follows **Clean/Onion Architecture** with strict dependency direction — inner layers never reference outer layers.

```
┌─────────────────────────────────────────┐
│              Web (Startup)              │  ← DI wiring, middleware, SignalR hubs
├─────────────────────────────────────────┤
│           Presentation                  │  ← Controllers (thin, no business logic)
├─────────────────────────────────────────┤
│             Services                    │  ← Business logic implementations
├──────────────────┬──────────────────────┤
│ Services.        │    Persistence       │  ← EF Core, Identity, DbContext
│ Abstraction      │                      │  ← Interfaces/contracts only
├──────────────────┴──────────────────────┤
│           Shared (DTOs)                 │  ← Request/Response models
├─────────────────────────────────────────┤
│              Domain                     │  ← Pure entities, enums, zero dependencies
└─────────────────────────────────────────┘
```

**Core rule:** Dependencies always point inward. No EF Core in Domain. No business logic in controllers. No infrastructure concerns leaking into the core.

**Key architectural decision:** `INotificationSender` in `Services.Abstraction` bridges SignalR (Web layer) from the Services layer — avoids layer violations while enabling real-time push.

---

## Tech Stack

| Technology | Purpose |
|---|---|
| ASP.NET Core Web API (.NET 10) | Backend framework |
| Entity Framework Core (Npgsql) | ORM + migrations |
| PostgreSQL 16 | Primary database |
| Redis 7 | Feed cache (sorted sets) |
| ASP.NET Core Identity | User management + password hashing |
| JWT Bearer Authentication | Stateless auth |
| SignalR | Real-time notification delivery |
| Hangfire | Background job processing |
| Docker Compose | Local development environment |
| Scalar | API documentation UI |

---

## Features

### ✅ Authentication & Authorization
- Register and login with JWT Bearer tokens
- Claims-based identity (`sub`, `email`, `unique_name`, `jti`)
- Passwords hashed via ASP.NET Core Identity

### ✅ User Profiles
- Public profile lookup by username
- Authenticated profile update (display name, bio, avatar URL)

### ✅ Posts
- Create, view, and delete posts
- Author-only delete enforcement
- MediaUrl support for external image/video links

### ✅ Feed (Cursor Pagination)
- Infinite scroll feed from followees
- Cursor-based pagination using timestamps — scales where OFFSET doesn't
- Redis-backed feed with fan-out on write (see [Feed Scaling](#feed-scaling))

### ✅ Follow System
- Follow / unfollow users
- Self-follow prevention via DB check constraint
- Duplicate follow prevention via composite PK

### ✅ Likes & Comments
- One like per user per post (composite PK guarantee)
- Cursor-paginated comments per post
- Batch username fetching — zero N+1 queries

### ✅ Notifications
- Stored notifications in DB (available offline)
- Real-time push via SignalR when recipient is connected
- Background job processing via Hangfire (non-blocking)
- Notification types: Follow, Like, Comment
- Mark as read endpoint

### ✅ Full-Text Search
- Post search using PostgreSQL `tsvector` computed column + GIN index
- `websearch_to_tsquery` — supports phrases, AND/OR, exclusions
- User search using `ILIKE` for case-insensitive partial matching

### ✅ Safety (Block, Mute, Report)
- **Block** — bidirectional content hiding
- **Mute** — one-directional silent hiding
- **Report** — content moderation with status workflow (`pending` → `reviewed` → `dismissed`)

### ✅ Feed Scaling (Redis Fan-out)
- Fan-out on write via Hangfire background jobs
- Redis sorted sets keyed by `feed:{userId}`
- Unix timestamp scores for natural newest-first ordering
- 500 item cap per feed + 7-day TTL

---

## Project Structure

```
SocialPlus/
├── SocialPluse.Domain/
│   ├── Entities/          # Post, Follow, Like, Comment, Notification,
│   │                      # Block, Mute, Report, RefreshToken
│   └── Enums/             # NotificationType
│
├── SocialPluse.Shared/
│   └── DTOs/              # Auth, Users, Posts, Comments, Likes,
│                          # Notifications, Search, Safety
│
├── SocialPluse.Services.Abstraction/
│   └── Interfaces/        # IAuthService, IUserService, IPostService,
│                          # IFollowService, ILikeService, ICommentService,
│                          # INotificationService, INotificationSender,
│                          # ISearchService, ISafetyService
│
├── SocialPluse.Services/
│   └── Implementations/   # Business logic for all features
│
├── SocialPluse.Persistence/
│   ├── DbContexts/        # AppDbContext
│   ├── IdentityData/      # AppUser (IdentityUser<Guid>)
│   ├── Data/
│   │   └── Configurations/ # EF entity configurations
│   └── Migrations/        # EF Core migrations
│
├── SocialPluse.Presentation/
│   └── Controllers/       # AuthController, UsersController, PostsController,
│                          # FollowsController, LikesController, CommentsController,
│                          # NotificationsController, SearchController,
│                          # BlocksController, MutesController, ReportsController
│
├── SocialPluse.Web/
│   ├── Hubs/              # NotificationHub, SignalRNotificationSender,
│   │                      # SubClaimUserIdProvider
│   ├── Middleware/        # GlobalExceptionMiddleware
│   ├── Extensions/        # MigrationExtensions
│   └── Program.cs         # Startup + DI wiring
│
└── docker-compose.yml     # PostgreSQL + Redis containers
```

---

## Getting Started

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2022+ or VS Code

### 1. Clone the repository

```bash
git clone https://github.com/Jaser1010/SocialPluse.git
cd SocialPluse
```

### 2. Start infrastructure

```bash
docker compose up -d
```

This starts:
- `socialpulse_postgres` on port `5432`
- `socialpulse_redis` on port `6379`

### 3. Run the API

```bash
cd SocialPluse.Web
dotnet run
```

Migrations are applied automatically on startup via `ApplyMigrationsAsync()`.

### 4. Open API docs

```
http://localhost:6500/scalar/v1
```

### 5. Hangfire dashboard (dev only)

```
http://localhost:6500/hangfire
```

---

## Environment Configuration

**`appsettings.json`** (Docker / Production):
```json
{
  "Jwt": {
    "Issuer": "SocialPulse",
    "Audience": "SocialPulse",
    "Key": "YOUR_SECRET_KEY_MINIMUM_32_CHARS",
    "ExpiryMinutes": 60
  },
  "ConnectionStrings": {
    "Postgres": "Host=socialpulse_postgres;Port=5432;Database=socialpulse;Username=socialpulse;Password=socialpulse",
    "Redis": "socialpulse_redis:6379"
  }
}
```

**`appsettings.Development.json`** (Local):
```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=socialpulse;Username=socialpulse;Password=socialpulse",
    "Redis": "localhost:6379"
  }
}
```

---

## API Reference

### Authentication
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | ❌ | Register new user |
| POST | `/api/auth/login` | ❌ | Login, returns JWT |

### Users
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/users/{username}` | ❌ | Get public profile |
| GET | `/api/users/me` | ✅ | Get own profile |
| PUT | `/api/users/me` | ✅ | Update own profile |

### Posts & Feed
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/posts` | ✅ | Create post |
| GET | `/api/posts/{id}` | ❌ | Get post by ID |
| DELETE | `/api/posts/{id}` | ✅ | Delete own post |
| GET | `/api/feed?cursor=&limit=` | ✅ | Get feed (Redis) |

### Follow
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/follows/{userId}` | ✅ | Follow user |
| DELETE | `/api/follows/{userId}` | ✅ | Unfollow user |

### Likes
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/posts/{postId}/likes` | ✅ | Like post |
| DELETE | `/api/posts/{postId}/likes` | ✅ | Unlike post |

### Comments
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/posts/{postId}/comments` | ✅ | Add comment |
| GET | `/api/posts/{postId}/comments?cursor=&limit=` | ❌ | List comments |

### Notifications
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/notifications?cursor=&limit=` | ✅ | Get notifications |
| POST | `/api/notifications/{id}/read` | ✅ | Mark as read |

### Search
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/search/posts?q=&limit=` | ❌ | Full-text post search |
| GET | `/api/search/users?q=&limit=` | ❌ | User search |

### Safety
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/blocks/{userId}` | ✅ | Block user |
| DELETE | `/api/blocks/{userId}` | ✅ | Unblock user |
| POST | `/api/mutes/{userId}` | ✅ | Mute user |
| DELETE | `/api/mutes/{userId}` | ✅ | Unmute user |
| POST | `/api/reports` | ✅ | Submit report |
| GET | `/api/reports/me` | ✅ | Get my reports |

### Real-Time (SignalR)
| Endpoint | Description |
|----------|-------------|
| `/hubs/notifications` | WebSocket hub — receives `notification` events |

---

## Database Schema

### Core Tables

```
AspNetUsers       → AppUser (Identity)
Posts             → Id, AuthorId, Text, MediaUrl, CreatedAt, SearchVector (tsvector)
Follows           → (FollowerId, FolloweeId) composite PK
Likes             → (UserId, PostId) composite PK
Comments          → Id, PostId, AuthorId, Text, CreatedAt
Notifications     → Id, RecipientUserId, ActorUserId, Type, PostId?, CommentId?, IsRead, CreatedAt
```

### Safety Tables

```
Blocks            → (BlockerId, BlockedId) composite PK + CK_Block_NotSelf
Mutes             → (MuterId, MutedId) composite PK + CK_Mute_NotSelf
Reports           → Id, ReporterId, TargetType, TargetId, Reason, Status (default: 'pending')
```

### Key Indexes

```sql
-- Feed/profile queries
INDEX ON Posts (AuthorId, CreatedAt)

-- Comment pagination
INDEX ON Comments (PostId, CreatedAt)

-- Notification queries
INDEX ON Notifications (RecipientUserId, IsRead, CreatedAt)

-- Full-text search
SearchVector tsvector GENERATED ALWAYS AS (to_tsvector('english', coalesce("Text", ''))) STORED
INDEX ON Posts USING GIN (SearchVector)

-- Report lookups
INDEX ON Reports (ReporterId)
INDEX ON Reports (TargetType, TargetId)
```

---

## Real-Time & Background Jobs

### Notification Flow

```
User likes a post
  └─► LikeService saves Like to DB
  └─► BackgroundJob.Enqueue(CreateLikeNotificationAsync)
  └─► HTTP returns 200 immediately ⚡

  (Hangfire background worker)
  └─► NotificationService creates Notification record in DB
  └─► INotificationSender.SendAsync called
  └─► SignalRNotificationSender pushes to group "user_{recipientId}"
        ├─ Recipient online  → instant delivery via WebSocket
        └─ Recipient offline → notification waits in DB
```

### SignalR Authentication

Clients connect to `/hubs/notifications` with a JWT token:

```javascript
const connection = new HubConnectionBuilder()
    .withUrl("/hubs/notifications", {
        accessTokenFactory: () => token
    })
    .build();

connection.on("notification", (data) => {
    console.log("New notification:", data);
});
```

---

## Feed Scaling

### The Problem

A naive feed query gets slower as the social graph grows:
```sql
-- Gets slower at scale
SELECT * FROM Posts
WHERE AuthorId IN (SELECT FolloweeId FROM Follows WHERE FollowerId = @userId)
ORDER BY CreatedAt DESC
LIMIT 20
```

### The Solution — Fan-out on Write

**Write path** (post creation):
```
POST /api/posts
  └─► Save post to PostgreSQL
  └─► BackgroundJob.Enqueue(FanoutPostToFeedAsync)
  └─► Return 200 immediately

  (Hangfire background worker)
  └─► Fetch all follower IDs from DB
  └─► For each follower:
        ZADD feed:{followerId} {unix_timestamp_ms} {postId}
        ZREMRANGEBYRANK (trim to 500 items max)
        EXPIRE 7 days
```

**Read path** (feed request):
```
GET /api/feed
  └─► ZRANGEBYSCORE feed:{userId} (Redis — microseconds, no joins)
  └─► Batch fetch post details by IDs from PostgreSQL
  └─► Batch fetch author usernames
  └─► Return feed
```

### Why Redis Sorted Sets?

- Score = Unix timestamp → naturally ordered newest-first
- O(log N) inserts regardless of feed size
- TTL keeps memory bounded automatically
- 500 item cap prevents unbounded growth

---

## Patterns Used

| Pattern | Applied In |
|---|---|
| Cursor pagination | Feed, Comments, Notifications |
| Batch `ToDictionaryAsync` (no N+1) | Feed, Comments, Notifications, Search |
| Composite PK uniqueness | Follow, Like, Block, Mute |
| DB check constraints | Follow (not self), Block (not self), Mute (not self) |
| Background jobs (non-blocking) | Notifications, Feed fan-out |
| Interface bridge (layer isolation) | `INotificationSender` |
| Computed DB columns | `SearchVector` tsvector |
| Redis sorted sets | Feed fan-out |
| Global exception middleware | All controllers |

---

## License

This project is for educational purposes.

---

*Built by [Jaser](https://github.com/Jaser1010) — documented every step of the way.*
