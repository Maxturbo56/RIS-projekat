using AstralNexus.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AstralNexus.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<BoardCard> BoardCards => Set<BoardCard>();
    public DbSet<UserCard> UserCards => Set<UserCard>();
    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<DeckCard> DeckCards => Set<DeckCard>();
    public DbSet<MatchRecord> Matches => Set<MatchRecord>();
    public DbSet<PackOpening> PackOpenings => Set<PackOpening>();
    public DbSet<PackOpeningCard> PackOpeningCards => Set<PackOpeningCard>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(x => x.UserName)
            .IsUnique();

        modelBuilder.Entity<Card>()
            .HasIndex(x => x.ExternalKey)
            .IsUnique();

        modelBuilder.Entity<BoardCard>()
            .HasIndex(x => x.ExternalKey)
            .IsUnique();

        modelBuilder.Entity<Deck>()
            .HasIndex(x => new { x.UserId, x.Name })
            .IsUnique();

        modelBuilder.Entity<UserCard>()
            .HasKey(x => new { x.UserId, x.CardId });

        modelBuilder.Entity<DeckCard>()
            .HasKey(x => new { x.DeckId, x.CardId });

        modelBuilder.Entity<PackOpeningCard>()
            .HasKey(x => new { x.PackOpeningId, x.Position });

        modelBuilder.Entity<UserCard>()
            .HasOne(x => x.User)
            .WithMany(x => x.Collection)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserCard>()
            .HasOne(x => x.Card)
            .WithMany(x => x.Owners)
            .HasForeignKey(x => x.CardId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Deck>()
            .HasOne(x => x.User)
            .WithMany(x => x.Decks)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DeckCard>()
            .HasOne(x => x.Deck)
            .WithMany(x => x.Cards)
            .HasForeignKey(x => x.DeckId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DeckCard>()
            .HasOne(x => x.Card)
            .WithMany(x => x.DeckCards)
            .HasForeignKey(x => x.CardId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MatchRecord>()
            .HasOne(x => x.User)
            .WithMany(x => x.Matches)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MatchRecord>()
            .HasOne(x => x.Deck)
            .WithMany(x => x.Matches)
            .HasForeignKey(x => x.DeckId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<PackOpening>()
            .HasOne(x => x.User)
            .WithMany(x => x.PackOpenings)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PackOpeningCard>()
            .HasOne(x => x.PackOpening)
            .WithMany(x => x.Cards)
            .HasForeignKey(x => x.PackOpeningId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PackOpeningCard>()
            .HasOne(x => x.Card)
            .WithMany(x => x.PackOpeningCards)
            .HasForeignKey(x => x.CardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
