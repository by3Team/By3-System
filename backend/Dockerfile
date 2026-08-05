FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["By3.Api/By3.Api.csproj", "By3.Api/"]
COPY ["By3.Service/By3.Service.csproj", "By3.Service/"]
COPY ["By3.Repository/By3.Repository.csproj", "By3.Repository/"]
RUN dotnet restore "By3.Api/By3.Api.csproj"
COPY . .
WORKDIR "/src/By3.Api"
RUN dotnet build "By3.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "By3.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 创建非 root 用户并以该用户运行，降低容器内权限
RUN useradd -m -s /bin/bash appuser
USER appuser
ENTRYPOINT ["dotnet", "By3.Api.dll"]
