# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SocialPluse.Web/SocialPluse.Web.csproj", "SocialPluse.Web/"]
COPY ["SocialPluse.Persistence/SocialPluse.Persistence.csproj", "SocialPluse.Persistence/"]
COPY ["SocialPluse.Domain/SocialPluse.Domain.csproj", "SocialPluse.Domain/"]
COPY ["SocialPluse.Services.Abstraction/SocialPluse.Services.Abstraction.csproj", "SocialPluse.Services.Abstraction/"]
COPY ["SocialPluse.Shared/SocialPluse.Shared.csproj", "SocialPluse.Shared/"]
COPY ["SocialPluse.Presentation/SocialPluse.Presentation.csproj", "SocialPluse.Presentation/"]
COPY ["SocialPluse.Services/SocialPluse.Services.csproj", "SocialPluse.Services/"]
RUN dotnet restore "./SocialPluse.Web/SocialPluse.Web.csproj"
COPY . .
WORKDIR "/src/SocialPluse.Web"
RUN dotnet build "./SocialPluse.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SocialPluse.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SocialPluse.Web.dll"]