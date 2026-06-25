using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.RegistrationNumber).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.GPA).HasPrecision(3, 2);
        builder.Property<DateTime>("LastUpdated")
                .HasDefaultValueSql("Now()")
                .IsRequired();
        builder.Property(s => s.Version)
               .IsRowVersion();   
        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}