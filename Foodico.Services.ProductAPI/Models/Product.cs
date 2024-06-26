﻿using System.ComponentModel.DataAnnotations;

namespace Foodico.Services.ProductAPI.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        [Required]
        public string Name { get; set; }
        [Range(1, 5000)]
        public double Price { get; set; }
        public string Description { get; set; }

        [Required]
        public string CategoryName { get; set; }
        public string ImageUrl { get; set; }
    }
}
