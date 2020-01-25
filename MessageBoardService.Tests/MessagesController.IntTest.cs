using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

using MessageBoardService.Controllers;
using MessageBoardService.Models;

namespace MessageBoardService.IntTests
{
    // Integration tests.
    public class MessagesControllerIntTest
    {
        TestServer server = null;
        HttpClient client = null;

        public MessagesControllerIntTest()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<Startup>();

            server = new TestServer(builder);
            client = server.CreateClient();
        }

    #region Add New Message
        [Fact]
        public async void Add_InvalidTitleMessage_Returns_BadRequest()
        {
            // Setup
            var tooLongTitle = new string('x', 51);
            var message = new Message() {
                Title = tooLongTitle,
                Text = "Test text"
            };
            var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

            // Perform
            var result = await client.PostAsync("/api/Messages", content);

            // Verify
            Assert.IsType<HttpResponseMessage>(result);
            var responseMessage = result as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.BadRequest, responseMessage.StatusCode);
        }

        [Fact]
        public async void Add_InvalidTextMessage_Returns_BadRequest()
        {
            // Setup
            var tooLongText = new string('x', 501);
            var message = new Message() {
                Title = "Test title",
                Text = tooLongText
            };
            var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

            // Perform
            var result = await client.PostAsync("/api/Messages", content);

            // Verify
            Assert.IsType<HttpResponseMessage>(result);
            var responseMessage = result as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.BadRequest, responseMessage.StatusCode);
        }

    #endregion

        [Fact]
        public async void Update_InvalidTitleMessage_Returns_BadRequest()
        {
            // Setup
            var message = new Message() {
                Id = 1,
                Title = "First message title",
                Text = "First message text"
            };
            var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");
            await client.PostAsync("/api/Messages", content);

            var tooLongTitle = new string('x', 51);
            var updateMessage = new Message() {
                Title = tooLongTitle,
                Text = "Test text"
            };
            var updateContent = new StringContent(JsonConvert.SerializeObject(updateMessage), Encoding.UTF8, "application/json");

            // Perform
            var result = await client.PostAsync("/api/Messages", updateContent);

            // Verify
            Assert.IsType<HttpResponseMessage>(result);
            var responseMessage = result as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.BadRequest, responseMessage.StatusCode);
        }

        [Fact]
        public async void Update_InvalidTextMessage_Returns_BadRequest()
        {
            // Setup
            var message = new Message() {
                Id = 1,
                Title = "First message title",
                Text = "First message text"
            };
            var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");
            await client.PostAsync("/api/Messages", content);

            var tooLongText = new string('x', 501);
            var updateMessage = new Message() {
                Title = "Test title",
                Text = tooLongText
            };
            var updateContent = new StringContent(JsonConvert.SerializeObject(updateMessage), Encoding.UTF8, "application/json");

            // Perform
            var result = await client.PostAsync("/api/Messages", updateContent);

            // Verify
            Assert.IsType<HttpResponseMessage>(result);
            var responseMessage = result as HttpResponseMessage;
            Assert.Equal(HttpStatusCode.BadRequest, responseMessage.StatusCode);
        }
    }
}
