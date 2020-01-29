using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

using MessageBoardService.Controllers;
using MessageBoardService.Models;
using MessageBoardService.Tests.Utilities;

namespace MessageBoardService.Tests
{
    // Integration tests.
    public class MessagesControllerIntTest
        : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private readonly TestWebApplicationFactory<Startup> _factory;
        private readonly HttpClient _client = null;

        public MessagesControllerIntTest(TestWebApplicationFactory<Startup> factory)
        {
            _factory = factory;

            _client = factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services
                            .AddAuthentication("Test")
                            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                                "Test", options => {});

                        services.AddAuthorization(options =>
                        {
                            options.AddPolicy("TestPolicy", builder =>
                            {
                                builder.AuthenticationSchemes.Add("Test");
                                builder.RequireAuthenticatedUser();
                            });
                            options.DefaultPolicy = options.GetPolicy("TestPolicy");
                        });
                    });
                })
                .CreateClient(new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false,
                });

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Test");
        }

        [Fact]
        public async void Get_Messages_Returns_Success()
        {
            // Perform
            var result = await _client.GetAsync("/api/Messages");

            // Verify
            Assert.IsType<HttpResponseMessage>(result);
            var responseMessage = result as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);

            string responseBody = await responseMessage.Content.ReadAsStringAsync();
            var messages = JsonConvert.DeserializeObject<JArray>(responseBody).ToObject<List<JObject>>();

            Assert.Equal(2, messages.Count);
        }

        [Fact]
        public async void Get_MessageById_Returns_Success()
        {
            var messageId = 2;

            // Perform
            var uri = String.Format("/api/Messages/{0}", messageId);
            var result = await _client.GetAsync(uri);

            // Verify
            Assert.IsType<HttpResponseMessage>(result);
            var responseMessage = result as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);

            string responseBody = await responseMessage.Content.ReadAsStringAsync();
            var message = JsonConvert.DeserializeObject<JObject>(responseBody);

            var dbMessage = TestDbData.Messages.Find(e => e.Id == messageId);
            Assert.NotNull(dbMessage);

            Assert.Equal(dbMessage.Id, message["id"]);
            Assert.Equal(dbMessage.CreationDateTime, message["creationDateTime"]);
            Assert.Equal(dbMessage.UserId, message["userId"]);
            Assert.Equal(dbMessage.Title, message["title"]);
            Assert.Equal(dbMessage.Text, message["text"]);
        }

        [Fact]
        public async void Get_MessageById_Returns_NotFound()
        {
            var messageId = 7;

            // Perform
            var uri = String.Format("/api/Messages/{0}", messageId);
            var result = await _client.GetAsync(uri);

            // Verify
            Assert.IsType<HttpResponseMessage>(result);
            var responseMessage = result as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Fact]
        public async void Add_Valid_Message_Returns_Success()
        {
            var messageToAdd = new MessageModel() {
                Title = "Added message title",
                Text = "Added message text"
            };
            var content = new StringContent(JsonConvert.SerializeObject(messageToAdd), Encoding.UTF8, "application/json");

            // Perform
            var postResult = await _client.PostAsync("/api/Messages", content);

            // Verify result.
            Assert.IsType<HttpResponseMessage>(postResult);
            var postResponseMessage = postResult as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.Created, postResponseMessage.StatusCode);

            // Verify that reading the message provides the updated message.
            var uri = String.Format("/api/Messages/{0}", 3);
            var getResult = await _client.GetAsync(uri);

            Assert.IsType<HttpResponseMessage>(getResult);
            var getResponseMessage = getResult as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.OK, getResponseMessage.StatusCode);

            string getResponseBody = await getResponseMessage.Content.ReadAsStringAsync();
            var message = JsonConvert.DeserializeObject<JObject>(getResponseBody);

            Assert.Equal(3, message["id"]);
            Assert.Equal(1, message["userId"]);
            Assert.Equal(messageToAdd.Title, message["title"]);
            Assert.Equal(messageToAdd.Text, message["text"]);
        }

        [Fact]
        public async void Add_Valid_Message_Returns_Conflict()
        {
            var message = new MessageModel() {
                Id = 1,
                Title = "Added message title",
                Text = "Added message text"
            };
            var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

            // Perform
            var postResult = await _client.PostAsync("/api/Messages", content);

            // Verify
            Assert.IsType<HttpResponseMessage>(postResult);
            var postResponseMessage = postResult as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.Conflict, postResponseMessage.StatusCode);
        }

        [Fact]
        public async void Add_Message_With_Invalid_Title_Returns_BadRequest()
        {
            // Setup
            var tooLongTitle = new string('x', 51);
            var message = new MessageModel() {
                Title = tooLongTitle,
                Text = "Test text"
            };
            var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

            // Perform
            var result = await _client.PostAsync("/api/Messages", content);

            // Verify
            Assert.IsType<HttpResponseMessage>(result);
            var responseMessage = result as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.BadRequest, responseMessage.StatusCode);
        }

        [Fact]
        public async void Add_Message_With_Invalid_Text_Returns_BadRequest()
        {
            // Setup
            var tooLongText = new string('x', 501);
            var message = new MessageModel() {
                Title = "Test title",
                Text = tooLongText
            };
            var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

            // Perform
            var result = await _client.PostAsync("/api/Messages", content);

            // Verify
            Assert.IsType<HttpResponseMessage>(result);
            var responseMessage = result as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.BadRequest, responseMessage.StatusCode);
        }

        [Fact]
        public async void Update_Valid_Message_Returns_NoContent()
        {
            var messageId = 1;
            var updateMessage = new MessageModel() {
                Id = messageId,
                Title = "Updated message title",
                Text = "Updated message text"
            };
            var content = new StringContent(JsonConvert.SerializeObject(updateMessage), Encoding.UTF8, "application/json");
            var uri = String.Format("/api/Messages/{0}", messageId);

            // Perform
            var postResult = await _client.PutAsync(uri, content);

            // Verify result.
            Assert.IsType<HttpResponseMessage>(postResult);
            var postResponseMessage = postResult as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.NoContent, postResponseMessage.StatusCode);

            // Verify that reading the message provides the updated message.
            var getResult = await _client.GetAsync(uri);
            Assert.IsType<HttpResponseMessage>(getResult);
            var getResponseMessage = getResult as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.OK, getResponseMessage.StatusCode);

            string getResponseBody = await getResponseMessage.Content.ReadAsStringAsync();
            var message = JsonConvert.DeserializeObject<JObject>(getResponseBody);

            Assert.Equal(updateMessage.Id, message["id"]);
            Assert.Equal(1, message["userId"]);
            Assert.Equal(updateMessage.Title, message["title"]);
            Assert.Equal(updateMessage.Text, message["text"]);
        }

        [Fact]
        public async void Update_Valid_Message_Returns_NotFound()
        {
            var messageId = 12;
            var message = new MessageModel() {
                Id = messageId,
                Title = "Updated message title",
                Text = "Updated message text"
            };
            var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

            // Perform
            var uri = String.Format("/api/Messages/{0}", messageId);
            var result = await _client.PutAsync(uri, content);

            // Verify
            Assert.IsType<HttpResponseMessage>(result);
            var responseMessage = result as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Fact]
        public async void Update_Valid_Message_Returns_BadRequest()
        {
            var messageId = 1;
            var message = new MessageModel() {
                Id = messageId,
                Title = "Updated message title",
                Text = "Updated message text"
            };
            var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

            // Perform
            var result = await _client.PutAsync("/api/Messages/5", content);

            // Verify
            Assert.IsType<HttpResponseMessage>(result);
            var responseMessage = result as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.BadRequest, responseMessage.StatusCode);
        }

        [Fact]
        public async void Update_Valid_Message_Returns_Unauthorized()
        {
            var messageId = 2;
            var message = new MessageModel() {
                Id = messageId,
                Title = "Updated message title",
                Text = "Updated message text"
            };
            var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

            // Perform
            var uri = String.Format("/api/Messages/{0}", messageId);
            var result = await _client.PutAsync(uri, content);

            // Verify
            Assert.IsType<HttpResponseMessage>(result);
            var responseMessage = result as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.Unauthorized, responseMessage.StatusCode);
        }

        [Fact]
        public async void Update_Message_With_Invalid_Model_Data_Returns_BadRequest()
        {
            // Setup
            var messageId = 1;
            var tooLongTitle = new string('x', 51);
            var message = new MessageModel() {
                Id = messageId,
                Title = tooLongTitle,
                Text = "Test text"
            };
            var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

            // Perform
            var uri = String.Format("/api/Messages/{0}", messageId);
            var result = await _client.PutAsync(uri, content);

            // Verify
            Assert.IsType<HttpResponseMessage>(result);
            var responseMessage = result as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.BadRequest, responseMessage.StatusCode);
        }

        [Fact]
        public async void Update_Message_With_Invalid_Text_Returns_BadRequest()
        {
            // Setup
            var messageId = 1;
            var tooLongText = new string('x', 501);
            var message = new MessageModel() {
                Id = messageId,
                Title = "Test title",
                Text = tooLongText
            };
            var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

            // Perform
            var uri = String.Format("/api/Messages/{0}", messageId);
            var result = await _client.PutAsync(uri, content);

            // Verify
            Assert.IsType<HttpResponseMessage>(result);
            var responseMessage = result as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.BadRequest, responseMessage.StatusCode);
        }

        [Fact]
        public async void Delete_Message_Returns_Success()
        {
            // Setup
            var result = await _client.DeleteAsync("/api/Messages/1");

            // Verify
            Assert.IsType<HttpResponseMessage>(result);
            var responseMessage = result as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
        }

        [Fact]
        public async void Delete_Message_Returns_NotFound()
        {
            // Setup
            var result = await _client.DeleteAsync("/api/Messages/22");

            // Verify
            Assert.IsType<HttpResponseMessage>(result);
            var responseMessage = result as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Fact]
        public async void Delete_Message_Returns_Unauthorized()
        {
            // Setup
            var result = await _client.DeleteAsync("/api/Messages/2");

            // Verify
            Assert.IsType<HttpResponseMessage>(result);
            var responseMessage = result as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.Unauthorized, responseMessage.StatusCode);
        }

    }
}
