using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Billing.Data.Models
{
    public partial class IAMContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.Property(e => e.SavedAt).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.TenantId)
                .HasComputedColumnSql("(CONVERT([varchar](50),concat(CONVERT([varchar](10),left(lower([RealmName]),(10))),CONVERT([varchar](1),'-'),CONVERT([varchar](3),right(lower(CONVERT([varchar](50),[TUId])),(3))),CONVERT([varchar](2),right('00'+CONVERT([varchar],[SNo]),(2))))))", true)
                .ValueGeneratedOnAdd();
            });
        }
    }
}
