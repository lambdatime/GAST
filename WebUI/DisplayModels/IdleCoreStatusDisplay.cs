using System;
using GreatAmericanSolrTracker.Web.Models;

namespace GreatAmericanSolrTracker.Web.DisplayModels
{
    public class IdleCoreStatusDisplay : CoreStatusDisplay
    {
        public IdleCoreStatusDisplay() : this(new IdleCoreStatusInfo())
        {
            
        }

        public IdleCoreStatusDisplay(IdleCoreStatusInfo coreStatusInfo)
            : base(coreStatusInfo)
        {
            TimeTaken = coreStatusInfo.TimeTaken;
        }

        public TimeSpan TimeTaken { get; set; }
    }

    public class AbortedCoreStatusDisplay : IdleCoreStatusDisplay
    {
        public AbortedCoreStatusDisplay() : this(new AbortedCoreStatusInfo())
        {
            
        }

        public AbortedCoreStatusDisplay(AbortedCoreStatusInfo coreStatusInfo) : base(coreStatusInfo)
        {
            TimeTaken = coreStatusInfo.TimeTaken;
        }

        
    }
}