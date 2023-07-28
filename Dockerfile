FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

## restore
COPY Pet.Jira.sln ./Pet.Jira.sln
COPY src/Pet.Jira.Application/Pet.Jira.Application.csproj ./src/Pet.Jira.Application/Pet.Jira.Application.csproj
COPY src/Pet.Jira.Domain/Pet.Jira.Domain.csproj ./src/Pet.Jira.Domain/Pet.Jira.Domain.csproj
COPY src/Pet.Jira.Infrastructure/Pet.Jira.Infrastructure.csproj ./src/Pet.Jira.Infrastructure/Pet.Jira.Infrastructure.csproj
COPY src/Pet.Jira.Web/Pet.Jira.Web.csproj ./src/Pet.Jira.Web/Pet.Jira.Web.csproj
COPY tests/Pet.Jira.UnitTests/Pet.Jira.UnitTests.csproj ./tests/Pet.Jira.UnitTests/Pet.Jira.UnitTests.csproj
RUN dotnet restore ./Pet.Jira.sln

## publish
COPY src/. ./src/
RUN dotnet publish ./src/Pet.Jira.Web/Pet.Jira.Web.csproj --configuration Realease --no-restore --output /app


# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app ./
EXPOSE 80
ENV ASPNETCORE_URLS="http://+"
ENTRYPOINT ["dotnet", "Pet.Jira.Web.dll"]


