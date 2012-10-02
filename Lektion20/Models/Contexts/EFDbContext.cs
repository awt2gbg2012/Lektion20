using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Lektion20.Models.Entities;

namespace Lektion20.Models.Contexts
{
    public class EFDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
    }
}