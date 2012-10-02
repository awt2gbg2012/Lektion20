using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using FacebookPrototype.Models.Entities;

namespace FacebookPrototype.Models.Contexts
{
    public class EFDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
    }
}