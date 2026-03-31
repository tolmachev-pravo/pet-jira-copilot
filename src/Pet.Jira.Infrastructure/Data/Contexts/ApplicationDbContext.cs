using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Pet.Jira.Domain.Entities.Blog;
using Pet.Jira.Domain.Entities.Integrations;
using Pet.Jira.Domain.Entities.Notifications;
using Pet.Jira.Domain.Entities.Users;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Data.Contexts
{
	public class ApplicationDbContext : DbContext
	{
		private IDbContextTransaction _currentTransaction;

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			  : base(options)
		{
		}

		public DbSet<User> Users { get; set; }
		public DbSet<UserCalendarConnection> UserCalendarConnections { get; set; }
		public DbSet<UserNotification> UserNotifications { get; set; }
		public DbSet<Article> Articles { get; set; }

		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
		{
			return base.SaveChangesAsync(cancellationToken);
		}

		public async Task BeginTransactionAsync()
		{
			if (_currentTransaction != null)
			{
				return;
			}

			_currentTransaction = await base.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted).ConfigureAwait(false);
		}

		public async Task CommitTransactionAsync()
		{
			try
			{
				await SaveChangesAsync().ConfigureAwait(false);

				_currentTransaction?.Commit();
			}
			catch
			{
				RollbackTransaction();
				throw;
			}
			finally
			{
				if (_currentTransaction != null)
				{
					_currentTransaction.Dispose();
					_currentTransaction = null;
				}
			}
		}

		public void RollbackTransaction()
		{
			try
			{
				_currentTransaction?.Rollback();
			}
			finally
			{
				if (_currentTransaction != null)
				{
					_currentTransaction.Dispose();
					_currentTransaction = null;
				}
			}
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<User>(entity =>
			{
				entity.HasIndex(item => item.Username)
					.IsUnique();
			});

			builder.Entity<UserCalendarConnection>(entity =>
			{
				entity.HasIndex(item => new { item.UserId, item.Provider })
					.IsUnique();

				entity.HasOne(item => item.User)
					.WithMany(user => user.CalendarConnections)
					.HasForeignKey(item => item.UserId)
					.OnDelete(DeleteBehavior.Cascade);
			});

			builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

			base.OnModelCreating(builder);
		}
	}
}
