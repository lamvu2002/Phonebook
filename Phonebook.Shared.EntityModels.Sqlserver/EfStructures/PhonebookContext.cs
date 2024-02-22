using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Phonebook.Shared;

public partial class PhonebookContext : DbContext
{
    public PhonebookContext(DbContextOptions<PhonebookContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<Subcategory> Subcategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2B9A1CDE29");

            entity.Property(e => e.CategoryId).ValueGeneratedNever();
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.ContactId).HasName("PK__Contact__5C6625BBD1F07750");

            entity.Property(e => e.ContactId).ValueGeneratedNever();

            entity.HasOne(d => d.Category).WithMany(p => p.Contacts).HasConstraintName("FK__Contact__Categor__3C69FB99");

            entity.HasOne(d => d.Subcategory).WithMany(p => p.Contacts).HasConstraintName("FK__Contact__Subcate__3D5E1FD2");
        });

        modelBuilder.Entity<Subcategory>(entity =>
        {
            entity.HasKey(e => e.SubcategoryId).HasName("PK__Subcateg__9C4E707D0F5E1A41");

            entity.Property(e => e.SubcategoryId).ValueGeneratedNever();

            entity.HasOne(d => d.Category).WithMany(p => p.Subcategories).HasConstraintName("FK__Subcatego__Categ__398D8EEE");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
