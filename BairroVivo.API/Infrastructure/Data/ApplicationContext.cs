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
        public DbSet<MidiaOcorrenciaEntity> MidiasOcorrencia { get; set; }
        public DbSet<AtualizacaoOcorrenciaEntity> AtualizacoesOcorrencia { get; set; }
        public DbSet<HistoricoStatusOcorrenciaEntity> HistoricoStatusOcorrencias { get; set; }
        public DbSet<AuditoriaOcorrenciaEntity> AuditoriasOcorrencia { get; set; }
        public DbSet<RequisicaoIdempotenteEntity> RequisicoesIdempotentes { get; set; }
        public DbSet<RelatorioBairroEntity> RelatoriosBairro { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MunicipioEntity>(entity =>
            {
                entity.ToTable("TB_MUNICIPIO");
                entity.HasKey(m => m.IdMunicipio).HasName("PK_TB_MUNICIPIO");

                entity.Property(m => m.IdMunicipio)
                    .HasColumnName("ID_MUNICIPIO")
                    .HasColumnType("NUMBER(10)")
                    .ValueGeneratedOnAdd();

                entity.Property(m => m.CodigoIbge)
                    .HasColumnName("CODIGO_IBGE")
                    .HasMaxLength(7)
                    .IsRequired();

                entity.Property(m => m.Nome)
                    .HasColumnName("NOME")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(m => m.Uf)
                    .HasColumnName("UF")
                    .HasMaxLength(2)
                    .IsRequired();

                entity.Property(m => m.Ativo)
                    .HasColumnName("ATIVO")
                    .HasMaxLength(1)
                    .IsRequired();
            });

            modelBuilder.Entity<UsuarioEntity>(entity =>
            {
                entity.ToTable("TB_USUARIO");
                entity.HasKey(u => u.IdUsuario).HasName("PK_TB_USUARIO");

                entity.Property(u => u.IdUsuario)
                    .HasColumnName("ID_USUARIO")
                    .HasColumnType("NUMBER(10)")
                    .ValueGeneratedOnAdd();

                entity.Property(u => u.Nome)
                    .HasColumnName("NOME")
                    .HasMaxLength(120)
                    .IsRequired();

                entity.Property(u => u.Email)
                    .HasColumnName("EMAIL")
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(u => u.SenhaHash)
                    .HasColumnName("SENHA_HASH")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(u => u.CpfHash)
                    .HasColumnName("CPF_HASH")
                    .HasMaxLength(64);

                entity.Property(u => u.Telefone)
                    .HasColumnName("TELEFONE")
                    .HasMaxLength(20);

                entity.Property(u => u.Bairro)
                    .HasColumnName("BAIRRO")
                    .HasMaxLength(100);

                entity.Property(u => u.IdMunicipio)
                    .HasColumnName("ID_MUNICIPIO")
                    .HasColumnType("NUMBER(10)");

                entity.Property(u => u.Perfil)
                    .HasColumnName("PERFIL")
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(u => u.Pontos)
                    .HasColumnName("PONTOS")
                    .HasColumnType("NUMBER(10)");

                entity.Property(u => u.Ativo)
                    .HasColumnName("ATIVO")
                    .HasMaxLength(1)
                    .IsRequired();

                entity.Property(u => u.CriadoEm)
                    .HasColumnName("CRIADO_EM")
                    .IsRequired();

                entity.Property(u => u.AtualizadoEm)
                    .HasColumnName("ATUALIZADO_EM");

                entity.HasOne(u => u.Municipio)
                    .WithMany(m => m.Usuarios)
                    .HasForeignKey(u => u.IdMunicipio)
                    .HasConstraintName("FK_USUARIO_MUNICIPIO")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CategoriaEntity>(entity =>
            {
                entity.ToTable("TB_CATEGORIA");
                entity.HasKey(c => c.IdCategoria).HasName("PK_TB_CATEGORIA");

                entity.Property(c => c.IdCategoria)
                    .HasColumnName("ID_CATEGORIA")
                    .HasColumnType("NUMBER(10)")
                    .ValueGeneratedOnAdd();

                entity.Property(c => c.Nome)
                    .HasColumnName("NOME")
                    .HasMaxLength(60)
                    .IsRequired();

                entity.Property(c => c.Descricao)
                    .HasColumnName("DESCRICAO")
                    .HasMaxLength(255);

                entity.Property(c => c.Icone)
                    .HasColumnName("ICONE")
                    .HasMaxLength(40);

                entity.Property(c => c.CorHex)
                    .HasColumnName("COR_HEX")
                    .HasMaxLength(7);

                entity.Property(c => c.SlaDiasPadrao)
                    .HasColumnName("SLA_DIAS_PADRAO")
                    .HasColumnType("NUMBER(3)")
                    .IsRequired();

                entity.Property(c => c.Ativo)
                    .HasColumnName("ATIVO")
                    .HasMaxLength(1)
                    .IsRequired();
            });

            modelBuilder.Entity<OcorrenciaEntity>(entity =>
            {
                entity.ToTable("TB_OCORRENCIA");
                entity.HasKey(o => o.IdOcorrencia).HasName("PK_TB_OCORRENCIA");

                entity.Property(o => o.IdOcorrencia)
                    .HasColumnName("ID_OCORRENCIA")
                    .HasColumnType("NUMBER(10)")
                    .ValueGeneratedOnAdd();

                entity.Property(o => o.Protocolo)
                    .HasColumnName("PROTOCOLO")
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(o => o.IdUsuario)
                    .HasColumnName("ID_USUARIO")
                    .HasColumnType("NUMBER(10)")
                    .IsRequired();

                entity.Property(o => o.IdCategoria)
                    .HasColumnName("ID_CATEGORIA")
                    .HasColumnType("NUMBER(10)")
                    .IsRequired();

                entity.Property(o => o.IdMunicipio)
                    .HasColumnName("ID_MUNICIPIO")
                    .HasColumnType("NUMBER(10)")
                    .IsRequired();

                entity.Property(o => o.Titulo)
                    .HasColumnName("TITULO")
                    .HasMaxLength(140)
                    .IsRequired();

                entity.Property(o => o.Descricao)
                    .HasColumnName("DESCRICAO")
                    .HasMaxLength(2000)
                    .IsRequired();

                entity.Property(o => o.Status)
                    .HasColumnName("STATUS")
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(o => o.Urgencia)
                    .HasColumnName("URGENCIA")
                    .HasMaxLength(15)
                    .IsRequired();

                entity.Property(o => o.Latitude)
                    .HasColumnName("LATITUDE")
                    .HasColumnType("NUMBER(10,7)")
                    .IsRequired();

                entity.Property(o => o.Longitude)
                    .HasColumnName("LONGITUDE")
                    .HasColumnType("NUMBER(10,7)")
                    .IsRequired();

                entity.Property(o => o.Logradouro)
                    .HasColumnName("LOGRADOURO")
                    .HasMaxLength(200);

                entity.Property(o => o.Bairro)
                    .HasColumnName("BAIRRO")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(o => o.Cep)
                    .HasColumnName("CEP")
                    .HasMaxLength(9);

                entity.Property(o => o.Confirmacoes)
                    .HasColumnName("CONFIRMACOES")
                    .HasColumnType("NUMBER(10)")
                    .IsRequired();

                entity.Property(o => o.ValidadaComunidade)
                    .HasColumnName("VALIDADA_COMUNIDADE")
                    .HasMaxLength(1)
                    .IsRequired();

                entity.Property(o => o.CategoriaSugeridaIa)
                    .HasColumnName("CATEGORIA_SUGERIDA_IA")
                    .HasMaxLength(60);

                entity.Property(o => o.ConfiancaIa)
                    .HasColumnName("CONFIANCA_IA")
                    .HasColumnType("NUMBER(4,3)");

                entity.Property(o => o.CriadaEm)
                    .HasColumnName("CRIADA_EM")
                    .IsRequired();

                entity.Property(o => o.AtualizadaEm)
                    .HasColumnName("ATUALIZADA_EM");

                entity.Property(o => o.ResolvidaEm)
                    .HasColumnName("RESOLVIDA_EM");

                entity.Property(o => o.SlaLimite)
                    .HasColumnName("SLA_LIMITE");

                entity.HasOne(o => o.Usuario)
                    .WithMany(u => u.Ocorrencias)
                    .HasForeignKey(o => o.IdUsuario)
                    .HasConstraintName("FK_OCORRENCIA_USUARIO")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Categoria)
                    .WithMany(c => c.Ocorrencias)
                    .HasForeignKey(o => o.IdCategoria)
                    .HasConstraintName("FK_OCORRENCIA_CATEGORIA")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Municipio)
                    .WithMany(m => m.Ocorrencias)
                    .HasForeignKey(o => o.IdMunicipio)
                    .HasConstraintName("FK_OCORRENCIA_MUNICIPIO")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MidiaOcorrenciaEntity>(entity =>
            {
                entity.ToTable("TB_MIDIA_OCORRENCIA");
                entity.HasKey(m => m.IdMidia).HasName("PK_TB_MIDIA_OCORRENCIA");

                entity.Property(m => m.IdMidia)
                    .HasColumnName("ID_MIDIA")
                    .HasColumnType("NUMBER(10)")
                    .ValueGeneratedOnAdd();

                entity.Property(m => m.IdOcorrencia)
                    .HasColumnName("ID_OCORRENCIA")
                    .HasColumnType("NUMBER(10)")
                    .IsRequired();

                entity.Property(m => m.UrlMidia)
                    .HasColumnName("URL_MIDIA")
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(m => m.TipoMidia)
                    .HasColumnName("TIPO_MIDIA")
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(m => m.Principal)
                    .HasColumnName("PRINCIPAL")
                    .HasMaxLength(1)
                    .IsRequired();

                entity.Property(m => m.CriadaEm)
                    .HasColumnName("CRIADA_EM")
                    .IsRequired();

                entity.HasOne(m => m.Ocorrencia)
                    .WithMany(o => o.Midias)
                    .HasForeignKey(m => m.IdOcorrencia)
                    .HasConstraintName("FK_MIDIA_OCORRENCIA")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AtualizacaoOcorrenciaEntity>(entity =>
            {
                entity.ToTable("TB_ATUALIZACAO_OCORRENCIA");
                entity.HasKey(a => a.IdAtualizacao).HasName("PK_TB_ATUALIZACAO_OCORRENCIA");

                entity.Property(a => a.IdAtualizacao)
                    .HasColumnName("ID_ATUALIZACAO")
                    .HasColumnType("NUMBER(10)")
                    .ValueGeneratedOnAdd();

                entity.Property(a => a.IdOcorrencia)
                    .HasColumnName("ID_OCORRENCIA")
                    .HasColumnType("NUMBER(10)")
                    .IsRequired();

                entity.Property(a => a.IdUsuario)
                    .HasColumnName("ID_USUARIO")
                    .HasColumnType("NUMBER(10)")
                    .IsRequired();

                entity.Property(a => a.Tipo)
                    .HasColumnName("TIPO")
                    .HasMaxLength(25)
                    .IsRequired();

                entity.Property(a => a.Comentario)
                    .HasColumnName("COMENTARIO")
                    .HasMaxLength(1000);

                entity.Property(a => a.UrlFotoEvidencia)
                    .HasColumnName("URL_FOTO_EVIDENCIA")
                    .HasMaxLength(500);

                entity.Property(a => a.Latitude)
                    .HasColumnName("LATITUDE")
                    .HasColumnType("NUMBER(10,7)");

                entity.Property(a => a.Longitude)
                    .HasColumnName("LONGITUDE")
                    .HasColumnType("NUMBER(10,7)");

                entity.Property(a => a.CriadaEm)
                    .HasColumnName("CRIADA_EM")
                    .IsRequired();

                entity.HasOne(a => a.Ocorrencia)
                    .WithMany(o => o.Atualizacoes)
                    .HasForeignKey(a => a.IdOcorrencia)
                    .HasConstraintName("FK_ATUALIZACAO_OCORRENCIA")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Usuario)
                    .WithMany(u => u.Atualizacoes)
                    .HasForeignKey(a => a.IdUsuario)
                    .HasConstraintName("FK_ATUALIZACAO_USUARIO")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<HistoricoStatusOcorrenciaEntity>(entity =>
            {
                entity.ToTable("TB_HISTORICO_STATUS_OCORRENCIA");
                entity.HasKey(h => h.IdHistorico).HasName("PK_TB_HISTORICO_STATUS");

                entity.Property(h => h.IdHistorico)
                    .HasColumnName("ID_HISTORICO")
                    .HasColumnType("NUMBER(10)")
                    .ValueGeneratedOnAdd();

                entity.Property(h => h.IdOcorrencia)
                    .HasColumnName("ID_OCORRENCIA")
                    .HasColumnType("NUMBER(10)")
                    .IsRequired();

                entity.Property(h => h.IdUsuarioResponsavel)
                    .HasColumnName("ID_USUARIO_RESPONSAVEL")
                    .HasColumnType("NUMBER(10)");

                entity.Property(h => h.StatusAnterior)
                    .HasColumnName("STATUS_ANTERIOR")
                    .HasMaxLength(20);

                entity.Property(h => h.StatusNovo)
                    .HasColumnName("STATUS_NOVO")
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(h => h.Observacao)
                    .HasColumnName("OBSERVACAO")
                    .HasMaxLength(500);

                entity.Property(h => h.AlteradoEm)
                    .HasColumnName("ALTERADO_EM")
                    .IsRequired();

                entity.HasOne(h => h.Ocorrencia)
                    .WithMany(o => o.HistoricoStatus)
                    .HasForeignKey(h => h.IdOcorrencia)
                    .HasConstraintName("FK_HIST_STATUS_OCORRENCIA")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(h => h.UsuarioResponsavel)
                    .WithMany()
                    .HasForeignKey(h => h.IdUsuarioResponsavel)
                    .HasConstraintName("FK_HIST_STATUS_USUARIO")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AuditoriaOcorrenciaEntity>(entity =>
            {
                entity.ToTable("TB_AUDITORIA_OCORRENCIA");
                entity.HasKey(a => a.IdAuditoria).HasName("PK_TB_AUDITORIA_OCORRENCIA");

                entity.Property(a => a.IdAuditoria)
                    .HasColumnName("ID_AUDITORIA")
                    .HasColumnType("NUMBER(10)")
                    .ValueGeneratedOnAdd();

                entity.Property(a => a.IdOcorrencia)
                    .HasColumnName("ID_OCORRENCIA")
                    .HasColumnType("NUMBER(10)")
                    .IsRequired();

                entity.Property(a => a.Operacao)
                    .HasColumnName("OPERACAO")
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(a => a.UsuarioBanco)
                    .HasColumnName("USUARIO_BANCO")
                    .HasMaxLength(60);

                entity.Property(a => a.StatusAnterior)
                    .HasColumnName("STATUS_ANTERIOR")
                    .HasMaxLength(20);

                entity.Property(a => a.StatusNovo)
                    .HasColumnName("STATUS_NOVO")
                    .HasMaxLength(20);

                entity.Property(a => a.DadosAnteriores)
                    .HasColumnName("DADOS_ANTERIORES");

                entity.Property(a => a.DadosNovos)
                    .HasColumnName("DADOS_NOVOS");

                entity.Property(a => a.DataOperacao)
                    .HasColumnName("DATA_OPERACAO")
                    .IsRequired();
            });

            modelBuilder.Entity<RequisicaoIdempotenteEntity>(entity =>
            {
                entity.ToTable("TB_REQUISICAO_IDEMPOTENTE");
                entity.HasKey(r => r.IdRequisicao).HasName("PK_TB_REQ_IDEMPOTENTE");

                entity.Property(r => r.IdRequisicao)
                    .HasColumnName("ID_REQUISICAO")
                    .HasColumnType("NUMBER(10)")
                    .ValueGeneratedOnAdd();

                entity.Property(r => r.ChaveIdempotencia)
                    .HasColumnName("CHAVE_IDEMPOTENCIA")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(r => r.IdUsuario)
                    .HasColumnName("ID_USUARIO")
                    .HasColumnType("NUMBER(10)");

                entity.Property(r => r.Endpoint)
                    .HasColumnName("ENDPOINT")
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(r => r.MetodoHttp)
                    .HasColumnName("METODO_HTTP")
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(r => r.StatusHttpResposta)
                    .HasColumnName("STATUS_HTTP_RESPOSTA")
                    .HasColumnType("NUMBER(3)")
                    .IsRequired();

                entity.Property(r => r.CorpoResposta)
                    .HasColumnName("CORPO_RESPOSTA");

                entity.Property(r => r.CriadaEm)
                    .HasColumnName("CRIADA_EM")
                    .IsRequired();

                entity.Property(r => r.ExpiraEm)
                    .HasColumnName("EXPIRA_EM")
                    .IsRequired();
            });

            modelBuilder.Entity<RelatorioBairroEntity>(entity =>
            {
                entity.ToTable("TB_RELATORIO_BAIRRO");
                entity.HasKey(r => r.IdRelatorio).HasName("PK_TB_RELATORIO_BAIRRO");

                entity.Property(r => r.IdRelatorio)
                    .HasColumnName("ID_RELATORIO")
                    .HasColumnType("NUMBER(10)")
                    .ValueGeneratedOnAdd();

                entity.Property(r => r.IdMunicipio)
                    .HasColumnName("ID_MUNICIPIO")
                    .HasColumnType("NUMBER(10)")
                    .IsRequired();

                entity.Property(r => r.Bairro)
                    .HasColumnName("BAIRRO")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(r => r.PeriodoRef)
                    .HasColumnName("PERIODO_REF")
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(r => r.TotalOcorrencias)
                    .HasColumnName("TOTAL_OCORRENCIAS")
                    .HasColumnType("NUMBER(10)");

                entity.Property(r => r.TotalResolvidas)
                    .HasColumnName("TOTAL_RESOLVIDAS")
                    .HasColumnType("NUMBER(10)");

                entity.Property(r => r.ResumoIa)
                    .HasColumnName("RESUMO_IA")
                    .IsRequired();

                entity.Property(r => r.ModeloIa)
                    .HasColumnName("MODELO_IA")
                    .HasMaxLength(60);

                entity.Property(r => r.GeradoEm)
                    .HasColumnName("GERADO_EM")
                    .IsRequired();

                entity.HasOne(r => r.Municipio)
                    .WithMany(m => m.Relatorios)
                    .HasForeignKey(r => r.IdMunicipio)
                    .HasConstraintName("FK_RELATORIO_MUNICIPIO")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            if (!Database.IsRelational())
            {
                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    var chavePrimaria = entityType.FindPrimaryKey();
                    if (chavePrimaria is { Properties.Count: 1 } &&
                        chavePrimaria.Properties[0].ClrType == typeof(decimal))
                    {
                        chavePrimaria.Properties[0]
                            .SetValueGeneratorFactory((_, _) => new DecimalInMemoryValueGenerator());
                    }
                }
            }
        }

        private sealed class DecimalInMemoryValueGenerator
            : Microsoft.EntityFrameworkCore.ValueGeneration.ValueGenerator<decimal>
        {
            private static long _sequencial = 100;
            public override bool GeneratesTemporaryValues => false;
            public override decimal Next(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry) =>
                Interlocked.Increment(ref _sequencial);
        }
    }
}
