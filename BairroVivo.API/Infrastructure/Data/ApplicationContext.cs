using BairroVivo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BairroVivo.Infrastructure.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        public DbSet<MunicipioEntity> Municipios { get; set; }
        public DbSet<UsuarioEntity> Usuarios { get; set; }
        public DbSet<CategoriaEntity> Categorias { get; set; }
        public DbSet<OcorrenciaEntity> Ocorrencias { get; set; }
        public DbSet<AtualizacaoOcorrenciaEntity> Atualizacoes { get; set; }
        public DbSet<MidiaOcorrenciaEntity> Midias { get; set; }
        public DbSet<VotoSituacaoEntity> Votos { get; set; }
        public DbSet<HistoricoStatusOcorrenciaEntity> HistoricoStatus { get; set; }
        public DbSet<RequisicaoIdempotenteEntity> RequisicoesIdempotentes { get; set; }
        public DbSet<RelatorioOcorrenciaEntity> RelatoriosOcorrencia { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // TB_MUNICIPIO
            modelBuilder.Entity<MunicipioEntity>(entity =>
            {
                entity.ToTable("TB_MUNICIPIO");
                entity.HasKey(m => m.IdMunicipio).HasName("PK_MUNICIPIO");

                entity.Property(m => m.IdMunicipio)
                    .HasColumnName("ID_MUNICIPIO")
                    .HasColumnType("NUMBER(19)")
                    .ValueGeneratedOnAdd();

                entity.Property(m => m.CodigoIbge)
                    .HasColumnName("CODIGO_IBGE")
                    .HasColumnType("NUMBER(7)");

                entity.Property(m => m.Nome)
                    .HasColumnName("NOME")
                    .HasMaxLength(120)
                    .IsRequired();

                entity.Property(m => m.Uf)
                    .HasColumnName("UF")
                    .HasMaxLength(2)
                    .IsFixedLength()
                    .IsRequired();
            });

            // TB_USUARIO
            modelBuilder.Entity<UsuarioEntity>(entity =>
            {
                entity.ToTable("TB_USUARIO");
                entity.HasKey(u => u.IdUsuario).HasName("PK_USUARIO");

                entity.Property(u => u.IdUsuario)
                    .HasColumnName("ID_USUARIO")
                    .HasColumnType("NUMBER(19)")
                    .ValueGeneratedOnAdd();

                entity.Property(u => u.Nome)
                    .HasColumnName("NOME")
                    .HasMaxLength(120)
                    .IsRequired();

                entity.Property(u => u.Email)
                    .HasColumnName("EMAIL")
                    .HasMaxLength(180)
                    .IsRequired();

                entity.Property(u => u.SenhaHash)
                    .HasColumnName("SENHA_HASH")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(u => u.Papel)
                    .HasColumnName("PAPEL")
                    .HasMaxLength(12)
                    .HasDefaultValue("CIDADAO")
                    .IsRequired();

                entity.Property(u => u.FlAtivo)
                    .HasColumnName("FL_ATIVO")
                    .HasMaxLength(1)
                    .IsFixedLength()
                    .HasDefaultValue("S")
                    .IsRequired();

                entity.Property(u => u.DtCriacao)
                    .HasColumnName("DT_CRIACAO")
                    .IsRequired();

                entity.Property(u => u.DtAtualizacao)
                    .HasColumnName("DT_ATUALIZACAO")
                    .IsRequired();
            });

            // TB_CATEGORIA
            modelBuilder.Entity<CategoriaEntity>(entity =>
            {
                entity.ToTable("TB_CATEGORIA");
                entity.HasKey(c => c.IdCategoria).HasName("PK_CATEGORIA");

                entity.Property(c => c.IdCategoria)
                    .HasColumnName("ID_CATEGORIA")
                    .HasColumnType("NUMBER(19)")
                    .ValueGeneratedOnAdd();

                entity.Property(c => c.Nome)
                    .HasColumnName("NOME")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(c => c.Descricao)
                    .HasColumnName("DESCRICAO")
                    .HasMaxLength(400);

                entity.Property(c => c.Icone)
                    .HasColumnName("ICONE")
                    .HasMaxLength(40);

                entity.Property(c => c.FlAtiva)
                    .HasColumnName("FL_ATIVA")
                    .HasMaxLength(1)
                    .IsFixedLength()
                    .HasDefaultValue("S")
                    .IsRequired();
            });

            // TB_OCORRENCIA
            modelBuilder.Entity<OcorrenciaEntity>(entity =>
            {
                entity.ToTable("TB_OCORRENCIA");
                entity.HasKey(o => o.IdOcorrencia).HasName("PK_OCORRENCIA");

                entity.Property(o => o.IdOcorrencia)
                    .HasColumnName("ID_OCORRENCIA")
                    .HasColumnType("NUMBER(19)")
                    .ValueGeneratedOnAdd();

                entity.Property(o => o.IdUsuario)
                    .HasColumnName("ID_USUARIO")
                    .HasColumnType("NUMBER(19)")
                    .IsRequired();

                entity.Property(o => o.IdCategoria)
                    .HasColumnName("ID_CATEGORIA")
                    .HasColumnType("NUMBER(19)")
                    .IsRequired();

                entity.Property(o => o.IdMunicipio)
                    .HasColumnName("ID_MUNICIPIO")
                    .HasColumnType("NUMBER(19)")
                    .IsRequired();

                entity.Property(o => o.Titulo)
                    .HasColumnName("TITULO")
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(o => o.Descricao)
                    .HasColumnName("DESCRICAO")
                    .HasColumnType("CLOB")
                    .IsRequired();

                entity.Property(o => o.Latitude)
                    .HasColumnName("LATITUDE")
                    .HasColumnType("NUMBER(10,7)")
                    .IsRequired();

                entity.Property(o => o.Longitude)
                    .HasColumnName("LONGITUDE")
                    .HasColumnType("NUMBER(10,7)")
                    .IsRequired();

                entity.Property(o => o.EnderecoReferencia)
                    .HasColumnName("ENDERECO_REFERENCIA")
                    .HasMaxLength(255);

                entity.Property(o => o.FlAnonimo)
                    .HasColumnName("FL_ANONIMO")
                    .HasMaxLength(1)
                    .IsFixedLength()
                    .HasDefaultValue("N")
                    .IsRequired();

                entity.Property(o => o.Status)
                    .HasColumnName("STATUS")
                    .HasMaxLength(12)
                    .HasDefaultValue("ABERTA")
                    .IsRequired();

                entity.Property(o => o.NumCiclo)
                    .HasColumnName("NUM_CICLO")
                    .HasColumnType("NUMBER(10)")
                    .HasDefaultValue(1)
                    .IsRequired();

                entity.Property(o => o.FlExcluida)
                    .HasColumnName("FL_EXCLUIDA")
                    .HasMaxLength(1)
                    .IsFixedLength()
                    .HasDefaultValue("N")
                    .IsRequired();

                entity.Property(o => o.IdAutorTransicao)
                    .HasColumnName("ID_AUTOR_TRANSICAO")
                    .HasColumnType("NUMBER(19)");

                entity.Property(o => o.DtCriacao)
                    .HasColumnName("DT_CRIACAO")
                    .IsRequired();

                entity.Property(o => o.DtAtualizacao)
                    .HasColumnName("DT_ATUALIZACAO")
                    .IsRequired();

                entity.HasOne(o => o.Usuario)
                    .WithMany(u => u.Ocorrencias)
                    .HasForeignKey(o => o.IdUsuario)
                    .HasConstraintName("FK_OCORRENCIA_USUARIO")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Categoria)
                    .WithMany(c => c.Ocorrencias)
                    .HasForeignKey(o => o.IdCategoria)
                    .HasConstraintName("FK_OCORRENCIA_CATEG")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Municipio)
                    .WithMany(m => m.Ocorrencias)
                    .HasForeignKey(o => o.IdMunicipio)
                    .HasConstraintName("FK_OCORRENCIA_MUNIC")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.AutorTransicao)
                    .WithMany()
                    .HasForeignKey(o => o.IdAutorTransicao)
                    .HasConstraintName("FK_OCORRENCIA_AUTOR_ST")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TB_ATUALIZACAO
            modelBuilder.Entity<AtualizacaoOcorrenciaEntity>(entity =>
            {
                entity.ToTable("TB_ATUALIZACAO");
                entity.HasKey(a => a.IdAtualizacao).HasName("PK_ATUALIZACAO");

                entity.Property(a => a.IdAtualizacao)
                    .HasColumnName("ID_ATUALIZACAO")
                    .HasColumnType("NUMBER(19)")
                    .ValueGeneratedOnAdd();

                entity.Property(a => a.IdOcorrencia)
                    .HasColumnName("ID_OCORRENCIA")
                    .HasColumnType("NUMBER(19)")
                    .IsRequired();

                entity.Property(a => a.IdUsuario)
                    .HasColumnName("ID_USUARIO")
                    .HasColumnType("NUMBER(19)")
                    .IsRequired();

                entity.Property(a => a.Tipo)
                    .HasColumnName("TIPO")
                    .HasMaxLength(12)
                    .IsRequired();

                entity.Property(a => a.Texto)
                    .HasColumnName("TEXTO")
                    .HasColumnType("CLOB");

                entity.Property(a => a.FlAnonimo)
                    .HasColumnName("FL_ANONIMO")
                    .HasMaxLength(1)
                    .IsFixedLength()
                    .HasDefaultValue("N")
                    .IsRequired();

                entity.Property(a => a.DtCriacao)
                    .HasColumnName("DT_CRIACAO")
                    .IsRequired();

                entity.HasOne(a => a.Ocorrencia)
                    .WithMany(o => o.Atualizacoes)
                    .HasForeignKey(a => a.IdOcorrencia)
                    .HasConstraintName("FK_ATUALIZACAO_OCORR")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Usuario)
                    .WithMany(u => u.Atualizacoes)
                    .HasForeignKey(a => a.IdUsuario)
                    .HasConstraintName("FK_ATUALIZACAO_AUTOR")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TB_MIDIA
            modelBuilder.Entity<MidiaOcorrenciaEntity>(entity =>
            {
                entity.ToTable("TB_MIDIA");
                entity.HasKey(m => m.IdMidia).HasName("PK_MIDIA");

                entity.Property(m => m.IdMidia)
                    .HasColumnName("ID_MIDIA")
                    .HasColumnType("NUMBER(19)")
                    .ValueGeneratedOnAdd();

                entity.Property(m => m.IdOcorrencia)
                    .HasColumnName("ID_OCORRENCIA")
                    .HasColumnType("NUMBER(19)")
                    .IsRequired();

                entity.Property(m => m.IdAtualizacao)
                    .HasColumnName("ID_ATUALIZACAO")
                    .HasColumnType("NUMBER(19)");

                entity.Property(m => m.IdUsuario)
                    .HasColumnName("ID_USUARIO")
                    .HasColumnType("NUMBER(19)")
                    .IsRequired();

                entity.Property(m => m.Provedor)
                    .HasColumnName("PROVEDOR")
                    .HasMaxLength(40)
                    .IsRequired();

                entity.Property(m => m.ChaveStorage)
                    .HasColumnName("CHAVE_STORAGE")
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(m => m.UrlExibicao)
                    .HasColumnName("URL_EXIBICAO")
                    .HasMaxLength(1000);

                entity.Property(m => m.MimeType)
                    .HasColumnName("MIME_TYPE")
                    .HasMaxLength(60)
                    .IsRequired();

                entity.Property(m => m.TamanhoBytes)
                    .HasColumnName("TAMANHO_BYTES")
                    .HasColumnType("NUMBER(12)")
                    .IsRequired();

                entity.Property(m => m.DtCriacao)
                    .HasColumnName("DT_CRIACAO")
                    .IsRequired();

                entity.HasOne(m => m.Ocorrencia)
                    .WithMany(o => o.Midias)
                    .HasForeignKey(m => m.IdOcorrencia)
                    .HasConstraintName("FK_MIDIA_OCORRENCIA")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.Atualizacao)
                    .WithMany(a => a.Midias)
                    .HasForeignKey(m => m.IdAtualizacao)
                    .HasConstraintName("FK_MIDIA_ATUALIZACAO")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.Usuario)
                    .WithMany()
                    .HasForeignKey(m => m.IdUsuario)
                    .HasConstraintName("FK_MIDIA_USUARIO")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TB_VOTO_SITUACAO
            modelBuilder.Entity<VotoSituacaoEntity>(entity =>
            {
                entity.ToTable("TB_VOTO_SITUACAO");
                entity.HasKey(v => v.IdVoto).HasName("PK_VOTO_SITUACAO");

                entity.Property(v => v.IdVoto)
                    .HasColumnName("ID_VOTO")
                    .HasColumnType("NUMBER(19)")
                    .ValueGeneratedOnAdd();

                entity.Property(v => v.IdOcorrencia)
                    .HasColumnName("ID_OCORRENCIA")
                    .HasColumnType("NUMBER(19)")
                    .IsRequired();

                entity.Property(v => v.IdAtualizacao)
                    .HasColumnName("ID_ATUALIZACAO")
                    .HasColumnType("NUMBER(19)")
                    .IsRequired();

                entity.Property(v => v.IdUsuario)
                    .HasColumnName("ID_USUARIO")
                    .HasColumnType("NUMBER(19)")
                    .IsRequired();

                entity.Property(v => v.NumCiclo)
                    .HasColumnName("NUM_CICLO")
                    .HasColumnType("NUMBER(10)")
                    .IsRequired();

                entity.Property(v => v.TipoVoto)
                    .HasColumnName("TIPO_VOTO")
                    .HasMaxLength(12)
                    .IsRequired();

                entity.Property(v => v.DtVoto)
                    .HasColumnName("DT_VOTO")
                    .IsRequired();

                entity.HasOne(v => v.Ocorrencia)
                    .WithMany(o => o.Votos)
                    .HasForeignKey(v => v.IdOcorrencia)
                    .HasConstraintName("FK_VOTO_OCORRENCIA")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(v => v.Atualizacao)
                    .WithOne(a => a.Voto)
                    .HasForeignKey<VotoSituacaoEntity>(v => v.IdAtualizacao)
                    .HasConstraintName("FK_VOTO_ATUALIZACAO")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(v => v.Usuario)
                    .WithMany()
                    .HasForeignKey(v => v.IdUsuario)
                    .HasConstraintName("FK_VOTO_USUARIO")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TB_HISTORICO_STATUS
            modelBuilder.Entity<HistoricoStatusOcorrenciaEntity>(entity =>
            {
                entity.ToTable("TB_HISTORICO_STATUS");
                entity.HasKey(h => h.IdHistorico).HasName("PK_HIST_STATUS");

                entity.Property(h => h.IdHistorico)
                    .HasColumnName("ID_HISTORICO")
                    .HasColumnType("NUMBER(19)")
                    .ValueGeneratedOnAdd();

                entity.Property(h => h.IdOcorrencia)
                    .HasColumnName("ID_OCORRENCIA")
                    .HasColumnType("NUMBER(19)")
                    .IsRequired();

                entity.Property(h => h.IdUsuario)
                    .HasColumnName("ID_USUARIO")
                    .HasColumnType("NUMBER(19)");

                entity.Property(h => h.Operacao)
                    .HasColumnName("OPERACAO")
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(h => h.StatusAnterior)
                    .HasColumnName("STATUS_ANTERIOR")
                    .HasMaxLength(12);

                entity.Property(h => h.StatusNovo)
                    .HasColumnName("STATUS_NOVO")
                    .HasMaxLength(12)
                    .IsRequired();

                entity.Property(h => h.CicloAnterior)
                    .HasColumnName("CICLO_ANTERIOR")
                    .HasColumnType("NUMBER(10)");

                entity.Property(h => h.CicloNovo)
                    .HasColumnName("CICLO_NOVO")
                    .HasColumnType("NUMBER(10)")
                    .IsRequired();

                entity.Property(h => h.DtEvento)
                    .HasColumnName("DT_EVENTO")
                    .IsRequired();

                entity.HasOne(h => h.Ocorrencia)
                    .WithMany(o => o.HistoricoStatus)
                    .HasForeignKey(h => h.IdOcorrencia)
                    .HasConstraintName("FK_HIST_OCORR")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(h => h.Usuario)
                    .WithMany()
                    .HasForeignKey(h => h.IdUsuario)
                    .HasConstraintName("FK_HIST_USUARIO")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TB_REQUISICAO_IDEMPOTENTE
            modelBuilder.Entity<RequisicaoIdempotenteEntity>(entity =>
            {
                entity.ToTable("TB_REQUISICAO_IDEMPOTENTE");
                entity.HasKey(r => r.IdRequisicao).HasName("PK_REQ_IDEMPOTENTE");

                entity.Property(r => r.IdRequisicao)
                    .HasColumnName("ID_REQUISICAO")
                    .HasColumnType("NUMBER(19)")
                    .ValueGeneratedOnAdd();

                entity.Property(r => r.IdUsuario)
                    .HasColumnName("ID_USUARIO")
                    .HasColumnType("NUMBER(19)")
                    .IsRequired();

                entity.Property(r => r.Chave)
                    .HasColumnName("CHAVE")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(r => r.Rota)
                    .HasColumnName("ROTA")
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(r => r.MetodoHttp)
                    .HasColumnName("METODO_HTTP")
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(r => r.HashCorpo)
                    .HasColumnName("HASH_CORPO")
                    .HasMaxLength(64)
                    .IsRequired();

                entity.Property(r => r.IdRecurso)
                    .HasColumnName("ID_RECURSO")
                    .HasColumnType("NUMBER(19)");

                entity.Property(r => r.DtCriacao)
                    .HasColumnName("DT_CRIACAO")
                    .IsRequired();

                entity.HasOne(r => r.Usuario)
                    .WithMany()
                    .HasForeignKey(r => r.IdUsuario)
                    .HasConstraintName("FK_REQ_USUARIO")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(r => new { r.IdUsuario, r.Chave, r.Rota })
                    .IsUnique()
                    .HasDatabaseName("UX_REQ_IDEMPOTENTE");
            });

            // VW_RELATORIO_OCORRENCIAS
            modelBuilder.Entity<RelatorioOcorrenciaEntity>(entity =>
            {
                entity.ToView("VW_RELATORIO_OCORRENCIAS");
                entity.HasKey(r => new { r.IdMunicipio, r.IdCategoria, r.Status });

                entity.Property(r => r.IdMunicipio).HasColumnName("ID_MUNICIPIO");
                entity.Property(r => r.IdCategoria).HasColumnName("ID_CATEGORIA");
                entity.Property(r => r.Status).HasColumnName("STATUS");
                entity.Property(r => r.TotalOcorrencias).HasColumnName("TOTAL_OCORRENCIAS");

                entity.HasOne(r => r.Municipio)
                    .WithMany()
                    .HasForeignKey(r => r.IdMunicipio);

                entity.HasOne(r => r.Categoria)
                    .WithMany()
                    .HasForeignKey(r => r.IdCategoria);
            });
        }
    }
}
