using Microsoft.EntityFrameworkCore;

namespace TravelWeb_API.Models
{
    public class GlobalSearchContext: DbContext
    {
        public GlobalSearchContext(DbContextOptions<GlobalSearchContext> options) : base(options) { }

        public DbSet<ViewGlobalSearch> ViewGlobalSearches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ViewGlobalSearch>(entity => {
                entity.HasNoKey();
                entity.ToView("View_GlobalSearch");
            });
        }
    }
}
