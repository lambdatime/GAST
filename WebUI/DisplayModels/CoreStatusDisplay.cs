using GreatAmericanSolrTracker.Web.Models;

namespace GreatAmericanSolrTracker.Web.DisplayModels
{
    public abstract class CoreStatusDisplay
    {
        public CoreStatusDisplay()
        {
        }

        protected CoreStatusDisplay(CoreStatusInfo coreStatusInfo)
        {
            StatusType = coreStatusInfo.StatusType;
        }

        public CoreStatusType StatusType { get; set; }

        public string StatusName
        {
            get
            {
                switch (StatusType)
                {
                    case CoreStatusType.Busy:
                        return "Busy";
                    case CoreStatusType.Error:
                        return "Error";
                    case CoreStatusType.Aborted:
                        return "Aborted";
                    default:
                        return "Idle";
                }
            }
        }

        public bool IsError
        {
            get { return StatusType == CoreStatusType.Error; }
        }
    }
}