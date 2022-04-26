﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DB.Models.EnumTypes;
using Microsoft.EntityFrameworkCore;

namespace DB.Models
{
    [Table("playlist")]
    public class Playlist
    {
        public Playlist()
        {
            Songs = new HashSet<Song>();
            LikedPlaylist = new HashSet<LikedPlaylist>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("title")]
        [StringLength(255)]
        public string Title { get; set; } = null!;
        
        //айдишник именно создателя!
        //(не юзера, который просто лайкнул, для этого есть индекс таблица Liked_Playlist)
        [Column("user_id")]
        public string UserId { get; set; } = null!;
        
        [Column("playlist_type")] 
        public PlaylistType PlaylistType { get; set; }

        [Column("img_src")]
        [StringLength(255)]
        public string? ImgSrc { get; set; }
        
        [Column("verified")]
        public bool? Verified { get; set; }
        
        
        
        /*[ForeignKey("UserId")]
        [InverseProperty("CreatedPlaylists")]
        public UserInfo? User { get; set; } = null!;*/
        
        //это для индекс таблицы liled_playlist (связь многие ко многим)
        [ForeignKey("PlaylistId")]
        [InverseProperty("Playlists")]
        public ICollection<Song> Songs { get; set; }
        
        //это тоже для индекс таблицы playlist_song (связь многие ко многим)
        /*[ForeignKey("PlaylistId")]
        [InverseProperty("Playlists")]*/
        public ICollection<LikedPlaylist> LikedPlaylist { get; set; }
        
    }
}
