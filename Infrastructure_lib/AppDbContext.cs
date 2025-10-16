using Domain_lib.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure_lib;

public partial class AppDBContext : DbContext
{
    public AppDBContext()
    {
    }

    public AppDBContext(DbContextOptions<AppDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TdCard> TdCards { get; set; }

    public virtual DbSet<TdColumn> TdColumns { get; set; }

    public virtual DbSet<TdComment> TdComments { get; set; }

    public virtual DbSet<TdFile> TdFiles { get; set; }

    public virtual DbSet<TdGroup> TdGroups { get; set; }

    public virtual DbSet<TdHistory> TdHistories { get; set; }

    public virtual DbSet<TdProject> TdProjects { get; set; }

    public virtual DbSet<TdUser> TdUsers { get; set; }

    public virtual DbSet<TrPriority> TrPriorities { get; set; }

    public virtual DbSet<TrStatus> TrStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TdCard>(entity =>
        {
            entity.HasKey(e => e.Keyid).HasName("td_cards_pk");

            entity.ToTable("td_cards", "bureautasker");

            entity.Property(e => e.Keyid).HasColumnName("keyid");
            entity.Property(e => e.AssignedId).HasColumnName("assigned_id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.CardName).HasColumnName("card_name");
            entity.Property(e => e.ClosedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("closed_at");
            entity.Property(e => e.ClosedBy).HasColumnName("closed_by");
            entity.Property(e => e.ColumnId).HasColumnName("column_id");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Duedate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("duedate");
            entity.Property(e => e.GitId).HasColumnName("git_id");
            entity.Property(e => e.GitIid).HasColumnName("git_iid");
            entity.Property(e => e.OrderNum).HasColumnName("order_num");
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Assigned).WithMany(p => p.TdCardAssigneds)
                .HasForeignKey(d => d.AssignedId)
                .HasConstraintName("td_cards_td_users_fk");

            entity.HasOne(d => d.Author).WithMany(p => p.TdCardAuthors)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("td_cards_author_td_users_fk");

            entity.HasOne(d => d.ClosedByNavigation).WithMany(p => p.TdCardClosedByNavigations)
                .HasForeignKey(d => d.ClosedBy)
                .HasConstraintName("td_cards_closed_by_td_users_fk");

            entity.HasOne(d => d.Column).WithMany(p => p.TdCards)
                .HasForeignKey(d => d.ColumnId)
                .HasConstraintName("td_cards_td_columns_fk");

            entity.HasOne(d => d.PriorityNavigation).WithMany(p => p.TdCards)
                .HasForeignKey(d => d.Priority)
                .HasConstraintName("td_cards_tr_priority_fk");
        });

        modelBuilder.Entity<TdColumn>(entity =>
        {
            entity.HasKey(e => e.Keyid).HasName("td_columns_pk");

            entity.ToTable("td_columns", "bureautasker");

            entity.Property(e => e.Keyid).HasColumnName("keyid");
            entity.Property(e => e.ColumnName).HasColumnName("column_name");
            entity.Property(e => e.ColumnOrder).HasColumnName("column_order");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.StatusId)
                .HasDefaultValue(1)
                .HasColumnName("status_id");

            entity.HasOne(d => d.Project).WithMany(p => p.TdColumns)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("td_columns_td_projects_fk");

            entity.HasOne(d => d.Status).WithMany(p => p.TdColumns)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("td_columns_tr_status_fk");
        });

        modelBuilder.Entity<TdComment>(entity =>
        {
            entity.HasKey(e => e.Keyid).HasName("td_comments_pk");

            entity.ToTable("td_comments", "bureautasker");

            entity.Property(e => e.Keyid).HasColumnName("keyid");
            entity.Property(e => e.Card).HasColumnName("card");
            entity.Property(e => e.CommentText).HasColumnName("comment_text");
            entity.Property(e => e.Context).HasColumnName("context");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created");
            entity.Property(e => e.Deleted)
                .HasDefaultValue(false)
                .HasColumnName("deleted");
            entity.Property(e => e.GitId).HasColumnName("git_id");
            entity.Property(e => e.IsSystem)
                .HasDefaultValue(false)
                .HasColumnName("is_system");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.CardNavigation).WithMany(p => p.TdComments)
                .HasForeignKey(d => d.Card)
                .HasConstraintName("td_comments_td_cards_fk");

            entity.HasOne(d => d.User).WithMany(p => p.TdComments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("td_comments_td_users_fk");
        });

        modelBuilder.Entity<TdFile>(entity =>
        {
            entity.HasKey(e => e.Keyid).HasName("td_files_pk");

            entity.ToTable("td_files", "bureautasker");

            entity.Property(e => e.Keyid).HasColumnName("keyid");
            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created");
            entity.Property(e => e.FileExtension)
                .HasMaxLength(10)
                .HasColumnName("file_extension");
            entity.Property(e => e.FileName).HasColumnName("file_name");
            entity.Property(e => e.FilePath).HasColumnName("file_path");
            entity.Property(e => e.InnerPath).HasColumnName("inner_path");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.StatusId)
                .HasDefaultValue(1)
                .HasColumnName("status_id");
            entity.Property(e => e.Url).HasColumnName("url");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Comment).WithMany(p => p.TdFiles)
                .HasForeignKey(d => d.CommentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("td_files_td_comments_fk");

            entity.HasOne(d => d.Status).WithMany(p => p.TdFiles)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("td_files_tr_status_fk");

            entity.HasOne(d => d.User).WithMany(p => p.TdFiles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("td_files_td_users_fk");
        });

        modelBuilder.Entity<TdGroup>(entity =>
        {
            entity.HasKey(e => e.Keyid).HasName("td_group_pk");

            entity.ToTable("td_group", "bureautasker");

            entity.Property(e => e.Keyid).HasColumnName("keyid");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created");
            entity.Property(e => e.GitId).HasColumnName("git_id");
            entity.Property(e => e.GroupName).HasColumnName("group_name");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");

            entity.HasOne(d => d.Owner).WithMany(p => p.TdGroups)
                .HasForeignKey(d => d.OwnerId)
                .HasConstraintName("td_group_td_users_fk");
        });

        modelBuilder.Entity<TdHistory>(entity =>
        {
            entity.HasKey(e => e.Keyid).HasName("td_history_pk");

            entity.ToTable("td_history", "bureautasker");

            entity.Property(e => e.Keyid).HasColumnName("keyid");
            entity.Property(e => e.Action).HasColumnName("action");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityType).HasColumnName("entity_type");
            entity.Property(e => e.IsSystem)
                .HasDefaultValue(false)
                .HasColumnName("is_system");
            entity.Property(e => e.OldState)
                .HasColumnType("character varying")
                .HasColumnName("old_state");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("timestamp");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.TdHistories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("td_history_td_users_fk");
        });

        modelBuilder.Entity<TdProject>(entity =>
        {
            entity.HasKey(e => e.Keyid).HasName("td_projects_pk");

            entity.ToTable("td_projects", "bureautasker");

            entity.Property(e => e.Keyid).HasColumnName("keyid");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created");
            entity.Property(e => e.GitId).HasColumnName("git_id");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.ProjectName).HasColumnName("project_name");
            entity.Property(e => e.Updated)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated");

            entity.HasOne(d => d.Owner).WithMany(p => p.TdProjects)
                .HasForeignKey(d => d.OwnerId)
                .HasConstraintName("td_projects_td_users_fk");
        });

        modelBuilder.Entity<TdUser>(entity =>
        {
            entity.HasKey(e => e.Keyid).HasName("td_users_pk");

            entity.ToTable("td_users", "bureautasker");

            entity.HasIndex(e => e.Login, "td_users_unique").IsUnique();

            entity.Property(e => e.Keyid).HasColumnName("keyid");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.GitId).HasColumnName("git_id");
            entity.Property(e => e.IsSystem)
                .HasDefaultValue(false)
                .HasColumnName("is_system");
            entity.Property(e => e.Login).HasColumnName("login");
            entity.Property(e => e.StatusId)
                .HasDefaultValue(1)
                .HasColumnName("status_id");
            entity.Property(e => e.UserName).HasColumnName("user_name");

            entity.HasOne(d => d.Status).WithMany(p => p.TdUsers)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("td_users_tr_status_fk");
        });

        modelBuilder.Entity<TrPriority>(entity =>
        {
            entity.HasKey(e => e.Keyid).HasName("tr_priority_pk");

            entity.ToTable("tr_priority", "bureautasker");

            entity.Property(e => e.Keyid)
                .ValueGeneratedNever()
                .HasColumnName("keyid");
            entity.Property(e => e.PriorityName).HasColumnName("priority_name");
        });

        modelBuilder.Entity<TrStatus>(entity =>
        {
            entity.HasKey(e => e.Keyid).HasName("tr_status_pk");

            entity.ToTable("tr_status", "bureautasker");

            entity.Property(e => e.Keyid)
                .ValueGeneratedNever()
                .HasColumnName("keyid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.StatusName).HasColumnName("status_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
