using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessageBoardService.Models
{
    public class MessageModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }

        public DateTime CreationDateTime { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 5)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Text { get; set; }
    }
}