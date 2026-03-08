# Projects and dependencies analysis

This document provides a comprehensive overview of the projects and their dependencies in the context of upgrading to .NETCoreApp,Version=v10.0.

## Table of Contents

- [Executive Summary](#executive-Summary)
  - [Highlevel Metrics](#highlevel-metrics)
  - [Projects Compatibility](#projects-compatibility)
  - [Package Compatibility](#package-compatibility)
  - [API Compatibility](#api-compatibility)
- [Aggregate NuGet packages details](#aggregate-nuget-packages-details)
- [Top API Migration Challenges](#top-api-migration-challenges)
  - [Technologies and Features](#technologies-and-features)
  - [Most Frequent API Issues](#most-frequent-api-issues)
- [Projects Relationship Graph](#projects-relationship-graph)
- [Project Details](#project-details)

  - [SocialPulse\SocialPulse.csproj](#socialpulsesocialpulsecsproj)


## Executive Summary

### Highlevel Metrics

| Metric | Count | Status |
| :--- | :---: | :--- |
| Total Projects | 1 | All require upgrade |
| Total NuGet Packages | 10 | 5 need upgrade |
| Total Code Files | 3 |  |
| Total Code Files with Incidents | 1 |  |
| Total Lines of Code | 80 |  |
| Total Number of Issues | 6 |  |
| Estimated LOC to modify | 0+ | at least 0.0% of codebase |

### Projects Compatibility

| Project | Target Framework | Difficulty | Package Issues | API Issues | Est. LOC Impact | Description |
| :--- | :---: | :---: | :---: | :---: | :---: | :--- |
| [SocialPulse\SocialPulse.csproj](#socialpulsesocialpulsecsproj) | net9.0 | 🟢 Low | 5 | 0 |  | AspNetCore, Sdk Style = True |

### Package Compatibility

| Status | Count | Percentage |
| :--- | :---: | :---: |
| ✅ Compatible | 5 | 50.0% |
| ⚠️ Incompatible | 1 | 10.0% |
| 🔄 Upgrade Recommended | 4 | 40.0% |
| ***Total NuGet Packages*** | ***10*** | ***100%*** |

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 606 |  |
| ***Total APIs Analyzed*** | ***606*** |  |

## Aggregate NuGet packages details

| Package | Current Version | Suggested Version | Projects | Description |
| :--- | :---: | :---: | :--- | :--- |
| Hangfire.AspNetCore | 1.8.23 |  | [SocialPulse.csproj](#socialpulsesocialpulsecsproj) | ✅Compatible |
| Hangfire.PostgreSql | 1.21.1 |  | [SocialPulse.csproj](#socialpulsesocialpulsecsproj) | ✅Compatible |
| Microsoft.AspNetCore.Authentication.JwtBearer | 9.0.13 | 10.0.3 | [SocialPulse.csproj](#socialpulsesocialpulsecsproj) | NuGet package upgrade is recommended |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 9.0.13 | 10.0.3 | [SocialPulse.csproj](#socialpulsesocialpulsecsproj) | NuGet package upgrade is recommended |
| Microsoft.AspNetCore.OpenApi | 10.0.3 |  | [SocialPulse.csproj](#socialpulsesocialpulsecsproj) | ✅Compatible |
| Microsoft.EntityFrameworkCore.Design | 9.0.13 | 10.0.3 | [SocialPulse.csproj](#socialpulsesocialpulsecsproj) | NuGet package upgrade is recommended |
| Microsoft.Extensions.Caching.StackExchangeRedis | 9.0.13 | 10.0.3 | [SocialPulse.csproj](#socialpulsesocialpulsecsproj) | NuGet package upgrade is recommended |
| Microsoft.VisualStudio.Azure.Containers.Tools.Targets | 1.23.0 |  | [SocialPulse.csproj](#socialpulsesocialpulsecsproj) | ⚠️NuGet package is incompatible |
| Newtonsoft.Json | 13.0.4 |  | [SocialPulse.csproj](#socialpulsesocialpulsecsproj) | ✅Compatible |
| Npgsql.EntityFrameworkCore.PostgreSQL | 9.0.4 |  | [SocialPulse.csproj](#socialpulsesocialpulsecsproj) | ✅Compatible |

## Top API Migration Challenges

### Technologies and Features

| Technology | Issues | Percentage | Migration Path |
| :--- | :---: | :---: | :--- |

### Most Frequent API Issues

| API | Count | Percentage | Category |
| :--- | :---: | :---: | :--- |

## Projects Relationship Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart LR
    P1["<b>📦&nbsp;SocialPulse.csproj</b><br/><small>net9.0</small>"]
    click P1 "#socialpulsesocialpulsecsproj"

```

## Project Details

<a id="socialpulsesocialpulsecsproj"></a>
### SocialPulse\SocialPulse.csproj

#### Project Info

- **Current Target Framework:** net9.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** AspNetCore
- **Dependencies**: 0
- **Dependants**: 0
- **Number of Files**: 5
- **Number of Files with Incidents**: 1
- **Lines of Code**: 80
- **Estimated LOC to modify**: 0+ (at least 0.0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["SocialPulse.csproj"]
        MAIN["<b>📦&nbsp;SocialPulse.csproj</b><br/><small>net9.0</small>"]
        click MAIN "#socialpulsesocialpulsecsproj"
    end

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 606 |  |
| ***Total APIs Analyzed*** | ***606*** |  |

