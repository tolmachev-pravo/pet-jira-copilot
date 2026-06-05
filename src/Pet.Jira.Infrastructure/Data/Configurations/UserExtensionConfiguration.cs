using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pet.Jira.Domain.Entities.Extensions;

namespace Pet.Jira.Infrastructure.Data.Configurations
{
    public class UserExtensionConfiguration : IEntityTypeConfiguration<UserExtension>
    {
        public void Configure(EntityTypeBuilder<UserExtension> builder)
        {
            builder
                .HasIndex(e => new { e.Username, e.Type })
                .IsUnique();
        }
    }
}
