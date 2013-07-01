using GreatAmericanSolrTracker.Web.Models;

namespace GreatAmericanSolrTracker.Web.DisplayModels
{
    public class SolrCoreDisplay
    {
        public SolrCoreDisplay()
        {
            
        }
        public SolrCoreDisplay(SolrCore solrCore)
        {
            Id = solrCore.SolrCoreId;
            Title = solrCore.Title;
            BaseUrl = solrCore.BaseUrl;
            SortOrder = solrCore.SortOrder;
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string BaseUrl { get; set; }
        public int SortOrder { get; set; }
    }
}