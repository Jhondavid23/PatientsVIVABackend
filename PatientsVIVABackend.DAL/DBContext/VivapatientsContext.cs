using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PatientsVIVABackend.Model;

public partial class VivapatientsContext : DbContext
{
    public VivapatientsContext()
    {
    }

    public VivapatientsContext(DbContextOptions<VivapatientsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Patient> Patients { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PK__Patients__970EC3660794DD89");

            entity.HasIndex(e => e.DocumentNumber, "UQ__Patients__68993918652541AA").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.DocumentNumber).HasMaxLength(20);
            entity.Property(e => e.DocumentType)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Email).HasMaxLength(120);
            entity.Property(e => e.FirstName).HasMaxLength(80);
            entity.Property(e => e.LastName).HasMaxLength(80);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
