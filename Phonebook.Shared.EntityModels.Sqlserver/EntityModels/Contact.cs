using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Phonebook.Shared;

[Table("Contact")]
public partial class Contact
{
    [Key]
    [Column("ContactID")]
    public int ContactId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Name { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? PhoneNumber { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? EmailAddress { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Address { get; set; }

    [Column("CategoryID")]
    public int? CategoryId { get; set; }

    [Column("SubcategoryID")]
    public int? SubcategoryId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ImageLink { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Contacts")]
    public virtual Category? Category { get; set; }

    [ForeignKey("SubcategoryId")]
    [InverseProperty("Contacts")]
    public virtual Subcategory? Subcategory { get; set; }
}
