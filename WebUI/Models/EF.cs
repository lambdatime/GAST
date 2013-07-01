using System.Data.Entity;

namespace GreatAmericanSolrTracker.Web.Models
{
    public class SolrCoreContext : DbContext
    {
        public SolrCoreContext()
            : base("ApplicationServices")
        {
            
        }
        public DbSet<SolrCore> SolrCores { get; set; }
    }

    
}