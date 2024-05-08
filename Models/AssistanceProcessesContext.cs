using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AsistenciaProcess.Models;

public partial class AssistanceProcessesContext : DbContext
{
    public AssistanceProcessesContext()
    {
    }

    public AssistanceProcessesContext(DbContextOptions<AssistanceProcessesContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Asistencium> Asistencia { get; set; }

    public virtual DbSet<Curso> Cursos { get; set; }

    public virtual DbSet<Horario> Horarios { get; set; }

    public virtual DbSet<Materium> Materia { get; set; }

    public virtual DbSet<Matricula> Matriculas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySQL("Name=ConnectionStrings:MYSQLconn");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asistencium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("asistencia");

            entity.HasIndex(e => e.IdUser, "id_User");

            entity.HasIndex(e => e.IdHorario, "id_horario");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Dato)
                .HasMaxLength(1)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Fecha).HasColumnType("date");
            entity.Property(e => e.IdHorario).HasColumnName("id_horario");
            entity.Property(e => e.IdUser)
                .HasMaxLength(50)
                .HasColumnName("id_User");

        });

        modelBuilder.Entity<Curso>(entity =>
        {
            entity.HasKey(e => new { e.Nrc, e.Periodo }).HasName("PRIMARY");

            entity.ToTable("curso");

            entity.Property(e => e.Nrc).HasColumnName("nrc");
            entity.Property(e => e.Periodo)
                .HasMaxLength(6)
                .HasDefaultValueSql("'202020'")
                .HasColumnName("periodo");
            entity.Property(e => e.Campus)
                .HasMaxLength(3)
                .HasDefaultValueSql("'TE'")
                .HasColumnName("campus");
            entity.Property(e => e.Capacidad)
                .HasDefaultValueSql("'25'")
                .HasColumnName("capacidad");
            entity.Property(e => e.CodigoDocente)
                .HasMaxLength(12)
                .HasDefaultValueSql("''")
                .HasColumnName("codigoDocente");
            entity.Property(e => e.Curso1)
                .HasMaxLength(4)
                .HasDefaultValueSql("''")
                .HasColumnName("curso");
            entity.Property(e => e.FechaFin)
                .HasColumnType("date")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio)
                .HasColumnType("date")
                .HasColumnName("fecha_inicio");
            entity.Property(e => e.Materia)
                .HasMaxLength(6)
                .HasDefaultValueSql("''")
                .HasColumnName("materia");
            entity.Property(e => e.NombreCurso)
                .HasMaxLength(30)
                .HasDefaultValueSql("''")
                .HasColumnName("nombreCurso");
            entity.Property(e => e.Ocupados)
                .HasDefaultValueSql("'0'")
                .HasColumnName("ocupados");
            entity.Property(e => e.Seccion)
                .HasMaxLength(3)
                .HasDefaultValueSql("''")
                .HasColumnName("seccion");
        });

        modelBuilder.Entity<Horario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("horario");

            entity.HasIndex(e => new { e.Nrc, e.Periodo }, "NRC");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DayWeek)
                .HasMaxLength(1)
                .HasColumnName("dayWeek");
            entity.Property(e => e.Edf)
                .HasMaxLength(5)
                .HasDefaultValueSql("''");
            entity.Property(e => e.HoraFin)
                .HasColumnType("time")
                .HasColumnName("hora_fin");
            entity.Property(e => e.HoraInicio)
                .HasColumnType("time")
                .HasColumnName("hora_inicio");
            entity.Property(e => e.Nrc).HasColumnName("NRC");
            entity.Property(e => e.Periodo)
                .HasMaxLength(6)
                .HasColumnName("periodo");
            entity.Property(e => e.Salon)
                .HasMaxLength(3)
                .HasDefaultValueSql("''");

        });

        modelBuilder.Entity<Materium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("materia");

            entity.Property(e => e.Id)
                .HasMaxLength(5)
                .HasDefaultValueSql("''")
                .HasColumnName("id");
        });

        modelBuilder.Entity<Matricula>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("matriculas");

            entity.HasIndex(e => new { e.FkNrc, e.Periodo }, "fk_nrc");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FechaDeMatricula)
                .HasColumnType("datetime")
                .HasColumnName("fechaDeMatricula");
            entity.Property(e => e.FkNrc).HasColumnName("fk_nrc");
            entity.Property(e => e.Periodo)
                .HasMaxLength(6)
                .HasColumnName("periodo");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("user_id");

        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.EntraId }).HasName("PRIMARY");

            entity.ToTable("usuario");

            entity.HasIndex(e => e.EntraId, "EntraId").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(12)
                .HasDefaultValueSql("'T000'")
                .HasColumnName("id");
            entity.Property(e => e.EntraId)
                .HasMaxLength(50)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Apellido)
                .HasMaxLength(20)
                .HasDefaultValueSql("''")
                .HasColumnName("apellido");
            entity.Property(e => e.Email)
                .HasMaxLength(80)
                .HasDefaultValueSql("''")
                .HasColumnName("email");
            entity.Property(e => e.Nombre)
                .HasMaxLength(20)
                .HasDefaultValueSql("''")
                .HasColumnName("nombre");
            entity.Property(e => e.Rol)
                .HasMaxLength(10)
                .HasColumnName("rol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
