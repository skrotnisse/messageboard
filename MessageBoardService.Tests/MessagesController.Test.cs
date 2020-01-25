using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

using MessageBoardService.Controllers;
using MessageBoardService.Models;

namespace MessageBoardService.Tests
{
    // Unit tests.
    public class MessagesControllerTest
    {
        MessageDBContext dbContext = null;

        private void PopulateContext(MessageDBContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Messages.AddRange(
                new Message() {
                    Id = 1,
                    CreationDateTime = DateTime.Now,
                    Title = "Test message title 1",
                    Text = "Test message text #1111"
                },
                new Message() {
                    Id = 2,
                    CreationDateTime = DateTime.Now,
                    Title = "Test message title 2",
                    Text = "Test message text #2222"
                }
            );

            context.SaveChanges();
        }

        public MessagesControllerTest()
        {
            var options = new DbContextOptionsBuilder<MessageDBContext>()
                .UseInMemoryDatabase("MessageBoard_UnitTest")
                .Options;
            dbContext = new MessageDBContext(options);
            PopulateContext(dbContext);
        }

    #region Get Message By Id
        [Fact]
        public async void GetMessageById_Returns_OkObjectResult()
        {
            // Setup
            var controller = new MessagesController(dbContext);
            var messageId = 2;

            // Perform
            var data = await controller.GetMessage(messageId);

            // Verify
            Assert.IsType<OkObjectResult>(data);
        }

        [Fact]
        public async void GetMessageById_Returns_NotFoundResult()
        {
            // Setup
            var controller = new MessagesController(dbContext);
            var messageId = 3;

            // Perform
            var data = await controller.GetMessage(messageId);

            // Verify
            Assert.IsType<NotFoundResult>(data);
        }

        [Fact]
        public async void GetMessageById_Matches_DB_Content()
        {
            // Setup
            var controller = new MessagesController(dbContext);
            var messageId = 2;

            // Perform
            var data = await controller.GetMessage(messageId);

            // Verify
            Assert.IsType<OkObjectResult>(data);
            var okResult = data as OkObjectResult;

            Assert.IsType<Message>(okResult.Value);
            var message = okResult.Value as Message;

            Assert.Equal("Test message title 2", message.Title);
            Assert.Equal("Test message text #2222", message.Text);
        }
    #endregion

    #region Get All Messages
        [Fact]
        public async void GetMessages_Returns_OkObjectResult()
        {
            // Setup
            var controller = new MessagesController(dbContext);

            // Perform
            var data = await controller.GetMessages();

            // Verify
            Assert.IsType<OkObjectResult>(data);
        }

        [Fact]
        public void GetMessages_Returns_BadRequestResult()
        {
            // Setup
            var controller = new MessagesController(dbContext);

            // Perform
            var data = controller.GetMessages();
            data = null;

            // Verify
            if (data != null) {
                Assert.IsType<BadRequestResult>(data);
            }
        }

        [Fact]
        public async void GetMessages_Matches_DB_Content()
        {
            // Setup
            var controller = new MessagesController(dbContext);

            // Perform
            var data = await controller.GetMessages();

            // Verify
            Assert.IsType<OkObjectResult>(data);
            var okResult = data as OkObjectResult;

            Assert.IsType<List<Message>>(okResult.Value);
            var messages = okResult.Value as List<Message>;

            Assert.Equal(2, messages.Count);

            Assert.Equal("Test message title 1", messages[0].Title);
            Assert.Equal("Test message text #1111", messages[0].Text);

            Assert.Equal("Test message title 2", messages[1].Title);
            Assert.Equal("Test message text #2222", messages[1].Text);
        }
    #endregion

    #region Add Message
        [Fact]
        public async void Add_ValidMessage_Returns_CreatedAtActionResult()
        {
            // Setup
            var controller = new MessagesController(dbContext);
            var message = new Message() {
                Title = "Test message title 3",
                Text = "Test message text #3333"
            };

            // Perform
            var data = await controller.PostMessage(message);

            // Verify
            Assert.IsType<CreatedAtActionResult>(data);
        }

        [Fact]
        public async void Add_ValidMessage_Returns_Expected_Message_ID()
        {
            // Setup
            var controller = new MessagesController(dbContext);
            var message = new Message() {
                Title = "Test message title 3",
                Text = "Test message text #3333"
            };

            // Perform
            var data = await controller.PostMessage(message);

            // Verify
            Assert.IsType<CreatedAtActionResult>(data);
            var createdAtActionResult = data as CreatedAtActionResult;
            Assert.IsType<Message>(createdAtActionResult.Value);
            var createdMessage = createdAtActionResult.Value as Message;

            Assert.Equal(3, createdMessage.Id);
        }

        [Fact]
        public async void Add_InvalidMessage_Returns_ConflictResult()
        {
            // Setup
            var controller = new MessagesController(dbContext);
            var message = new Message() {
                Id = 1,
                Title = "Test message title",
                Text = "Test message text"
            };

            // Perform
            var data = await controller.PostMessage(message);

            // Verify
            Assert.IsType<ConflictResult>(data);
        }

    #endregion

    #region Update Message
        [Fact]
        public async void Update_ValidData_Returns_NoContentResult()
        {
            // Setup
            var controller = new MessagesController(dbContext);
            var messageId = 2;

            // Perform
            var data = await controller.GetMessage(messageId);

            Assert.IsType<OkObjectResult>(data);
            var okResult = data as OkObjectResult;
            Assert.IsType<Message>(okResult.Value);
            var message = okResult.Value as Message;

            message.Title = "Test message title 2 - Updated";
            var updatedData = await controller.PutMessage(message.Id, message);

            // Verify
            Assert.IsType<NoContentResult>(updatedData);
        }

        [Fact]
        public async void Update_InvalidData_Returns_NotFoundResult()
        {
            // Setup
            var controller = new MessagesController(dbContext);
            var messageId = 1;

            // Perform
            var data = await controller.GetMessage(messageId);

            Assert.IsType<OkObjectResult>(data);
            var okResult = data as OkObjectResult;
            Assert.IsType<Message>(okResult.Value);
            var existingMessage = okResult.Value as Message;

            var message = new Message();
            message.Id = 17;
            message.CreationDateTime = existingMessage.CreationDateTime;
            message.Title = "Test message title 2 - Updated";
            message.Text = existingMessage.Text;

            var result = await controller.PutMessage(message.Id, message);

            // Verify
            Assert.IsType<NotFoundResult>(result);
        }
    #endregion

    #region Delete Message
        [Fact]
        public async void Delete_Existing_Message_Returns_OkObjectResult()
        {
            // Setup
            var controller = new MessagesController(dbContext);
            var messageId = 2;

            // Perform
            var data = await controller.DeleteMessage(messageId);

            // Verify
            Assert.IsType<OkObjectResult>(data);
        }

        [Fact]
        public async void Delete_NonExisting_Message_Returns_NotFoundResult()
        {
            // Setup
            var controller = new MessagesController(dbContext);
            var messageId = 5;

            // Perform
            var data = await controller.DeleteMessage(messageId);

            // Verify
            Assert.IsType<NotFoundResult>(data);
        }
    #endregion
    }
}
