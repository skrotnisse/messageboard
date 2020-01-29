using Microsoft.EntityFrameworkCore;

namespace MessageBoardService.Models
{
    public class MessageDBContext : DbContext
    {
        public MessageDBContext(DbContextOptions<MessageDBContext> options)
            : base(options)
        {
        }

        public DbSet<MessageModel> Messages { get; set; }
    }
}
