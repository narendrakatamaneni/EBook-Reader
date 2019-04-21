using System;
using System.Collections.Generic;
using System.Text;
using EBook_Reader.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace EBook_Reader.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Comments> Comments { get; set; }
        
    }
}
