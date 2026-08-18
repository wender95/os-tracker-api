using Microsoft.EntityFrameworkCore;
using SistemaOrdemServico.Domain.Entities;

namespace SistemaOrdemServico.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<FluxoOS> FluxosOS { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<EventoMovimentacao> EventosMovimentacao { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Mapeamento FluxoOS
        modelBuilder.Entity<FluxoOS>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.NumeroOS).IsRequired().HasMaxLength(50);
            b.Property(x => x.IdentificadorFluxo).IsRequired().HasMaxLength(100);
            b.Property(x => x.SetorAtual).IsRequired();
            b.Property(x => x.Status).IsRequired();

            // Mapeamento da coleção privada _eventos
            b.HasMany(x => x.Eventos)
             .WithOne()
             .HasForeignKey(e => e.FluxoId)
             .OnDelete(DeleteBehavior.Cascade);

            b.Navigation(x => x.Eventos).Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        // Mapeamento EventoMovimentacao
        modelBuilder.Entity<EventoMovimentacao>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Setor).IsRequired();
            b.Property(x => x.TipoEvento).IsRequired();
            b.Property(x => x.Timestamp).IsRequired();
            b.Property(x => x.MotivoJustificativa).HasMaxLength(500);
        });

        // Mapeamento Usuario
        modelBuilder.Entity<Usuario>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Nome).IsRequired().HasMaxLength(150);
            b.Property(x => x.Email).IsRequired().HasMaxLength(150);
            b.Property(x => x.Perfil).IsRequired();
        });
    }
}