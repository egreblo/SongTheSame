using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SongTheSame.Domain.Leagues;

namespace SongTheSame.Infrastructure.Persistence.Configurations;

internal sealed class LeagueConfiguration : IEntityTypeConfiguration<League>
{
    public void Configure(EntityTypeBuilder<League> builder)
    {
        builder.ToTable("leagues");
        builder.HasKey(league => league.Id);
        builder.Property(league => league.Name).HasMaxLength(League.MaxNameLength).IsRequired();
        builder.Property(league => league.InviteCode).HasMaxLength(16).IsRequired();
        builder.HasIndex(league => league.InviteCode).IsUnique();

        builder.OwnsMany(league => league.Members, members =>
        {
            members.ToTable("league_members");
            members.WithOwner().HasForeignKey("LeagueId");
            members.HasKey("LeagueId", nameof(LeagueMember.UserId));
            members.HasIndex(member => member.UserId);
        });
        builder.Navigation(league => league.Members).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
