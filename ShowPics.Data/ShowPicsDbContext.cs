using Microsoft.EntityFrameworkCore;
using ShowPics.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Data
{
    public class ShowPicsDbContext : DbContext
    {
        public ShowPicsDbContext(DbContextOptions<ShowPicsDbContext> options) : base(options)
        {
        }

        public DbSet<Folder> Folders { get; set; }
        public DbSet<File> Files { get; set; }

    }
}
