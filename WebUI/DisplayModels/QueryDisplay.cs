namespace GreatAmericanSolrTracker.Web.DisplayModels
{
    public class QueryDisplay
    {
        public QueryDisplay()
        {
        }

        public QueryDisplay(SolrCoreDisplay solrCore, string query, string results)
        {
            SolrCore = solrCore;
            Query = query;
            QueryResults = results;
        }

        public SolrCoreDisplay SolrCore { get; set; }
        public string Query { get; set; }
        public string QueryResults { get; set; }
    }
}