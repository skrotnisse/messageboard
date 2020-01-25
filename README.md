# Message Board

A small RESTful webapi message-board service written in C# / .NET Core 3.1.

# Local deployment

This section describes how to setup a local development environment to run the service and test cases.

## Setup on Ubuntu 18.04

Install the .NET 3.1 SDK using snap:
```
sudo snap install --classic dotnet-sdk
sudo snap alias dotnet-sdk.dotnet dotnet
```

Verify that the dotnet SDK was properly installed, e.g.:
```
user@host:~$ dotnet --version
3.1.101
user@host:~$
```

## Running the service

Run the service locally from project-root by:
```
dotnet run --project MessageBoardService
```

## Running the tests
Run unit & integration tests from project-root by:
```
dotnet test
```
