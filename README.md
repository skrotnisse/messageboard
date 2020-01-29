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

This requires docker to be installed. Create a docker image from the /MessageBoardService directory by:
```
sudo docker build -t messageboardservice .
```

This will download both the .NET Core 3.1 SDK and ASP.NET Core 3.1 run-time, unless these images already exist, which might take a while.

After the image has been created it can be run as a container (include -d to detach):
```
sudo docker run --rm -p 5000:5000 messageboardservice
```

# Usage

The service is running a REST API accessible at http://localhost:5000/api. Note that this service with its current configuration does **NOT** use SSL/TLS (HTTPS), meaning the connection will be insecure.

## REST API endpoints

| URI            | Method        | Description                                                               |
| -------------- |:-------------:| ------------------------------------------------------------------------- |
| /Login         | POST          | User authentication required before managing messages (generates JWT).    |
| /Messages      | GET           | Returns a list of all messages.                                           |
| /Messages      | POST          | Adds a new message.                                                       |
| /Messages/{id} | PUT           | Updates an existing message by ID.                                        |
| /Messages/{id} | GET           | Returns a single message by ID.                                           |
| /Messages/{id} | DELETE        | Deletes a single message by ID.                                           |

## Logging in

There exist a couple of predefined users that have been hard-coded for the login-service:

| Id | First name | Last name | Username | Password |
| --:| ---------- | --------- | -------- | -------- |
|  1 | John       | Crichton  | john     | secret   |
|  2 | Aeryn      | Sun       | aeryn    | secret   |

Login using one of the predefined users, by POST'ing to `/Login` with body e.g:
```
{
  "username": "john",
  "password": "secret"
}
```

Upon successful login a JSON object with a 'token' key-value will be returned. This is the JWT created for this session.

Note that the service uses a dummy private key/secret (see appsettings.json) to sign the JWTs:
```
  "AppSettings": {
    "Secret": "USED TO SIGN AND VERIFY JWT - MUST BE KEPT A SECRET"
  },
```

## Managing messages

Messages on the board are managed through the `/Messages*` URIs. Each request needs to include a valid JWT in the Authorization HTTP header, using the Bearer schema (see logging in section above).

### Adding new messages

New messages are added by issuing a POST request to the `/Messages` URI. The body needs to include at least a `title` and `text`, e.g:
```
{
  "title": "Message title",
  "text": "Message text"
}
```
Both `title` and `text` fields are required. The `title` value is only allowed to be between 5 and 50 characters long, and the `text` value is not allowed to be longer than 500 characters.

### Reading messages

Retrieve a complete list of messages on the board by issuing a GET request to the `/Messages` URI. A specific message can be retrieved by issuing a GET request to the `/Messages/{id}` URI where `{id}` is the message identifier.

### Updating existing messages

Existing messages can be updated by issuing a PUT request to the `/Messages/{id}` URI. The `id` of the body needs to match the `{id}` of the URI. E.g. for a URI `/Messages/3`, a body could look like:
```
{
  "id": 3,
  "title": "Updated message title",
  "text": "Updated message text"
}
```

Similar constraints on the `title` and `text` values apply as for adding new messages (see section above). A user is only allowed to update their own messages.

### Deleting messages

Messages can be deleted by issuing a DELETE request to the `/Messages/{id}` URI. The message with identifier `{id}` will be deleted. A user is only allowed to delete their own messages.
