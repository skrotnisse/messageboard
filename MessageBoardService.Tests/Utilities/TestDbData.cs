using System;
using System.Collections.Generic;
using MessageBoardService.Models;

namespace MessageBoardService.Tests.Utilities
{
    class TestDbData
    {
        private static List<Message> _messages = new List<Message>()
        {
            new Message() {
                Id = 1,
                UserId = 1,
                CreationDateTime = DateTime.Now,
                Title = "Test message title #1",
                Text = "Test message text #1"
            },
            new Message() {
                Id = 2,
                UserId = 2,
                CreationDateTime = DateTime.Now,
                Title = "Test message title #2",
                Text = "Test message text #2"
            }
        };

        public static List<Message> Messages
        {
            get { return _messages; }
        }
    }

}