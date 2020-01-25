# Message Board

A small RESTful webapi message-board service written in C# / .NET Core 3.1.

# Development setup

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

Note: Running the project for the first time requires dependencies to be downloaded. This might take a while.

## Running the tests
Run unit & integration tests from project-root by:
```
dotnet test
```
Note: Running the tests for the first time requires dependencies to be downloaded. This might take a while.

# Deployment using Docker

This requires docker to be installed. Create a docker image from project-root by:
```
sudo docker build -t messageboardservice .
```

This will download both the .NET Core 3.1 SDK and ASP.NET Core 3.1 run-time, unless these images already exist, which might take a while.

After the image has been created it can be run as a container (include -d to detach):
```
sudo docker run --rm -p 5000:5000 messageboardservice
```
