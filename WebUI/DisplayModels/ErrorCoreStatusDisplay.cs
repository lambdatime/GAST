using GreatAmericanSolrTracker.Web.Models;

namespace GreatAmericanSolrTracker.Web.DisplayModels
{
    public class ErrorCoreStatusDisplay : CoreStatusDisplay
    {
        public ErrorCoreStatusDisplay() : base(new ErrorCoreStatusInfo())
        {

        }

        public ErrorCoreStatusDisplay(ErrorCoreStatusInfo coreStatusInfo)
            : base(coreStatusInfo)
        {
            Message = coreStatusInfo.Message;
        }

        public string Message { get; set; }
    }
}