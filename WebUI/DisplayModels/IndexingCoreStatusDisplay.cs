using System;
using GreatAmericanSolrTracker.Web.Models;

namespace GreatAmericanSolrTracker.Web.DisplayModels
{
    public class IndexingCoreStatusDisplay : CoreStatusDisplay
    {
        public IndexingCoreStatusDisplay() : this(new IndexingCoreStatusInfo())
        {
            
        }

        public IndexingCoreStatusDisplay(IndexingCoreStatusInfo coreStatusInfo)
            : base(coreStatusInfo)
        {
            TimeElapsed = coreStatusInfo.TimeElapsed;
            TotalRowsFetched = coreStatusInfo.TotalRowsFetched;
            TotalDocumentsProcessed = coreStatusInfo.TotalDocumentsProcessed;
        }

        public TimeSpan TimeElapsed { get; set; }
        public int TotalRowsFetched { get; set; }
        public int TotalDocumentsProcessed { get; set; }
    }
}