﻿using System;
using DB.Models.EnumTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace DB.Models
{
    public sealed class SpotifyContext : IdentityDbContext<UserInfo>
    {
        public SpotifyContext()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        public SpotifyContext(DbContextOptions<SpotifyContext> options, DbSet<Genre> genres, DbSet<Playlist> playlists, DbSet<Premium> premia, DbSet<Profile> profiles, DbSet<Song> songs, DbSet<UserInfo> userInfos)
            : base(options)
        {
        }

        public DbSet<Playlist> Playlists { get; set; } = null!;
        public DbSet<Premium> Premia { get; set; } = null!;
        public DbSet<Profile> Profiles { get; set; } = null!;
        public DbSet<Song> Songs { get; set; } = null!;
        public DbSet<UserInfo> UserInfos { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                
                optionsBuilder.UseNpgsql(connectionString);
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasPostgresEnum<Country>()
                .HasPostgresEnum<GenreType>()
                .HasPostgresEnum<PlaylistType>()
                .HasPostgresEnum<PremiumType>()
                .HasPostgresEnum<UserType>();

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
                //entity.HasKey(k => new { k.UserId, k.Id });
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
                //entity.Property(e => e.Id).UseIdentityAlwaysColumn();

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
                            
                            //warning bro
                            j.IndexerProperty<string>("UserId").HasColumnName("user_id");

                            j.IndexerProperty<int>("PlaylistId").HasColumnName("playlist_id");
                        });
            });

        }
    }
}
