using System;
using System.ComponentModel.DataAnnotations;

namespace GreatAmericanSolrTracker.Web.Models
{
    public class SolrCore
    {
        [Key]
        public int SolrCoreId { get; set; }

        [Required]
        [MaxLength(140)]
        [MinLength(1)]
        public string Title { get; set; }

        [Required]
        [MaxLength(140)]
        [MinLength(1)]
        public string BaseUrl { get; set; }

        public DateTime ModifiedOn { get; set; }

        [Required]
        public int SortOrder { get; set; }
    }
}