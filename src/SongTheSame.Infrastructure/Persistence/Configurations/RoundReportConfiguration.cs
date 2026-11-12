using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SongTheSame.Domain.Reports;

namespace SongTheSame.Infrastructure.Persistence.Configurations;

internal sealed class RoundReportConfiguration : IEntityTypeConfiguration<RoundReport>
{
    public void Configure(EntityTypeBuilder<RoundReport> builder)
    {
        builder.ToTable("round_reports");
        builder.HasKey(report => report.Id);
        builder.HasIndex(report => new { report.SessionId, report.RoundIndex }).IsUnique();
        builder.HasIndex(report => report.Status);
        builder.Property(report => report.Note).HasMaxLength(500);
        builder.Property(report => report.AdminNote).HasMaxLength(500);
        builder.Property(report => report.Status).HasConversion<string>().HasMaxLength(20);
    }
}
