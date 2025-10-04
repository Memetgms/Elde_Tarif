﻿using Microsoft.AspNetCore.Identity;

namespace Elde_Tarif.Models
{
    public class AppUser : IdentityUser
    {
        public string? AdSoyad { get; set; }
        // İlişkiler
        public ICollection<Yorum> Yorumlar { get; set; } = new List<Yorum>();
        public ICollection<Favori> Favoriler { get; set; } = new List<Favori>();
    }
}
