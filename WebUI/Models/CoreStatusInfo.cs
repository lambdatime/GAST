using System;

namespace GreatAmericanSolrTracker.Web.Models
{
    public abstract class CoreStatusInfo
    {
        protected CoreStatusInfo(CoreStatusType statusType)
        {
            StatusType = statusType;
        }

        public CoreStatusType StatusType { get; set; }

    }

    public class IndexingCoreStatusInfo : CoreStatusInfo
    {
        public IndexingCoreStatusInfo()
            : base(CoreStatusType.Busy)
        {
        }

        public TimeSpan TimeElapsed { get; set; }
        public int TotalRowsFetched { get; set; }
        public int TotalDocumentsProcessed { get; set; }
    }

    public class IdleCoreStatusInfo : CoreStatusInfo
    {
        public IdleCoreStatusInfo()
            : this(CoreStatusType.Idle)
        {
        }

        protected IdleCoreStatusInfo(CoreStatusType coreStatusType) : base(coreStatusType)
        {
            
        }

        public TimeSpan TimeTaken { get; set; }
    }

    public class AbortedCoreStatusInfo : IdleCoreStatusInfo
    {
        public AbortedCoreStatusInfo()
            : base(CoreStatusType.Aborted)
        {
        }

        public DateTime Aborted { get; set; }
        public TimeSpan TimeTaken { get; set; }
    }

    public class ErrorCoreStatusInfo : CoreStatusInfo
    {
        public ErrorCoreStatusInfo()
            : base(CoreStatusType.Error)
        {
        }

        public string Message { get; set; }
    }
}