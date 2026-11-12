using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SongTheSame.Domain.Similarity;

namespace SongTheSame.Infrastructure.Persistence.Configurations;

internal sealed class SimilarityCalibrationConfiguration : IEntityTypeConfiguration<SimilarityCalibration>
{
    public void Configure(EntityTypeBuilder<SimilarityCalibration> builder)
    {
        builder.ToTable("similarity_calibrations");
        builder.HasKey(calibration => calibration.Id);
        builder.HasIndex(calibration => calibration.CreatedAt);
    }
}
