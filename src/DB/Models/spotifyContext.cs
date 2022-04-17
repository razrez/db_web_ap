﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DB.Models
{
    public partial class SpotifyContext : DbContext
    {
        public SpotifyContext()
        {
        }

        public SpotifyContext(DbContextOptions<SpotifyContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Genre> Genres { get; set; } = null!;
        public virtual DbSet<Playlist> Playlists { get; set; } = null!;
        public virtual DbSet<Premium> Premia { get; set; } = null!;
        public virtual DbSet<Profile> Profiles { get; set; } = null!;
        public virtual DbSet<Song> Songs { get; set; } = null!;
        public virtual DbSet<UserInfo> UserInfos { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=spotify;Username=postgres;Password=3369");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum("country", new[] { "russia", "ukraine", "usa", "greece" })
                .HasPostgresEnum("genre_type", new[] { "rock", "jazz", "techno", "electro", "country", "pop" })
                .HasPostgresEnum("playlist_type", new[] { "album", "single", "ep", "user", "liked_songs" })
                .HasPostgresEnum("premium_type", new[] { "individual", "student", "duo", "family", "basic" })
                .HasPostgresEnum("user_type", new[] { "user", "artist", "admin" });

            modelBuilder.Entity<Genre>(entity =>
            {
                entity.HasOne(d => d.Playlist)
                    .WithMany()
                    .HasForeignKey(d => d.PlaylistId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_genre");
            });

            modelBuilder.Entity<Playlist>(entity =>
            {
                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.PlaylistsNavigation)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("fk_playlist");

                entity.HasMany(d => d.Songs)
                    .WithMany(p => p.Playlists)
                    .UsingEntity<Dictionary<string, object>>(
                        "PlaylistSong",
                        l => l.HasOne<Song>().WithMany().HasForeignKey("SongId").HasConstraintName("fk_playlist_song_song_id"),
                        r => r.HasOne<Playlist>().WithMany().HasForeignKey("PlaylistId").HasConstraintName("fk_playlist_song_playlist_id"),
                        j =>
                        {
                            j.HasKey("PlaylistId", "SongId").HasName("playlist_song_pkey");

                            j.ToTable("playlist_song");

                            j.IndexerProperty<int>("PlaylistId").HasColumnName("playlist_id");

                            j.IndexerProperty<int>("SongId").HasColumnName("song_id");
                        });
            });

            modelBuilder.Entity<Premium>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("premium_pkey");

                entity.Property(e => e.UserId).ValueGeneratedNever();

                entity.HasOne(d => d.User)
                    .WithOne(p => p.Premium)
                    .HasForeignKey<Premium>(d => d.UserId)
                    .HasConstraintName("fk_premium");
            });

            modelBuilder.Entity<Profile>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("profile_pkey");

                entity.Property(e => e.UserId).ValueGeneratedNever();

                entity.HasOne(d => d.User)
                    .WithOne(p => p.Profile)
                    .HasForeignKey<Profile>(d => d.UserId)
                    .HasConstraintName("fk_profile");
            });

            modelBuilder.Entity<Song>(entity =>
            {
                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Songs)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("fk_song");
            });

            modelBuilder.Entity<UserInfo>(entity =>
            {
                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.HasMany(d => d.Playlists)
                    .WithMany(p => p.Users)
                    .UsingEntity<Dictionary<string, object>>(
                        "LikedPlaylist",
                        l => l.HasOne<Playlist>().WithMany().HasForeignKey("PlaylistId").HasConstraintName("fk_liked_playlist_playlist_id"),
                        r => r.HasOne<UserInfo>().WithMany().HasForeignKey("UserId").HasConstraintName("fk_liked_playlist_user_id"),
                        j =>
                        {
                            j.HasKey("UserId", "PlaylistId").HasName("liked_playlist_pkey");

                            j.ToTable("liked_playlist");

                            j.IndexerProperty<int>("UserId").HasColumnName("user_id");

                            j.IndexerProperty<int>("PlaylistId").HasColumnName("playlist_id");
                        });
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
