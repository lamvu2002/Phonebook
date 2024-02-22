using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Phonebook.Shared;

public partial class Subcategory
{
    [Key]
    [Column("SubcategoryID")]
    public int SubcategoryId { get; set; }

    [Column("CategoryID")]
    public int? CategoryId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? SubcategoryName { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Subcategories")]
    public virtual Category? Category { get; set; }

    [InverseProperty("Subcategory")]
    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}
