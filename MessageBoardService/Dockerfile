FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /service

# Copy everything and restore/build for release
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /service
ENV ASPNETCORE_URLS http://+:5000
COPY --from=build-env /service/out .
ENTRYPOINT ["dotnet", "MessageBoardService.dll"]

