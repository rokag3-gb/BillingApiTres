using Microsoft.EntityFrameworkCore;

namespace Billing.Data.Models.Bill
{
    public partial class BillContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NcpMaster>(entity =>
            {
                entity.Property(e => e.KeyId)
                .HasComputedColumnSql("(CONVERT([varchar](500),concat([demandMonth],'-',[zone],'-',[account],'-',[memberNo])))", true)
                .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<NcpDetail>(entity =>
            {
                entity.HasOne(d => d.BillItem)
                      .WithMany(p => p.NcpDetails)
                      .HasForeignKey(d => d.KeyId)
                      .HasPrincipalKey(p => p.KeyId);
            });
        }
    }
}
