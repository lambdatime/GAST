using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml.Linq;
using GreatAmericanSolrTracker.Web.DisplayModels;
using GreatAmericanSolrTracker.Web.Models;
using Microsoft.AspNet.SignalR;

namespace GreatAmericanSolrTracker.Web.Hubs
{
    public class SolrCores : Hub
    {
        public SolrCores()
        {
            _timer = new Timer(UpdateAllStatuses, null, _updateInterval, _updateInterval);
        }
        private static Timer _timer;

        private readonly int _updateInterval = 1000; //ms

        public bool Add(SolrCoreDisplay newSolrCore)
        {
            try
            {
                using (var context = new SolrCoreContext())
                {
                    var highestSortOrder = context.SolrCores.Max(x => x.SortOrder);

                    var solrCore = context.SolrCores.Create();
                    solrCore.Title = newSolrCore.Title;
                    solrCore.BaseUrl = newSolrCore.BaseUrl;
                    solrCore.ModifiedOn = DateTime.Now;
                    solrCore.SortOrder = highestSortOrder + 1;
                    context.SolrCores.Add(solrCore);
                    context.SaveChanges();
                    var resultSolrCore = new SolrCoreDisplay(solrCore);
                    Clients.All().solrCoreAdded(resultSolrCore);
                    return true;
                }
            }
            catch (Exception)
            {
                Clients.Caller.reportError("Unable to update core.");
                return false;
            }
        }

        public bool Update(SolrCoreDisplay updatedSolrCore)
        {
            using (var context = new SolrCoreContext())
            {
                var oldSolrCore = context.SolrCores.FirstOrDefault(c => c.SolrCoreId == updatedSolrCore.Id);
                try
                {
                    if (oldSolrCore == null)
                        return false;
                    else
                    {
                        oldSolrCore.Title = updatedSolrCore.Title;
                        oldSolrCore.BaseUrl = updatedSolrCore.BaseUrl;
                        oldSolrCore.ModifiedOn = DateTime.Now;
                        oldSolrCore.SortOrder = updatedSolrCore.SortOrder;
                        context.SaveChanges();
                        Clients.All().solrCoreUpdated(updatedSolrCore);
                        return true;
                    }
                }
                catch (Exception)
                {
                    Clients.Caller.reportError("Unable to update core.");
                    return false;
                }
            }
        }

        public bool UpdateSortOrder(SolrCoreDisplay updatedSolrCore)
        {
            using (var context = new SolrCoreContext())
            {
                var oldSolrCore = context.SolrCores.FirstOrDefault(c => c.SolrCoreId == updatedSolrCore.Id);
                try
                {
                    if (oldSolrCore == null)
                        return false;
                    else
                    {
                        oldSolrCore.ModifiedOn = DateTime.Now;
                        oldSolrCore.SortOrder = updatedSolrCore.SortOrder;
                        context.SaveChanges();
                        Clients.All().sortOrderUpdated(updatedSolrCore);
                        return true;
                    }
                }
                catch (Exception)
                {
                    Clients.Caller.reportError("Unable to update core sort order.");
                    return false;
                }
            }
        }

        public bool UpdateMassSortOrders(SolrCoreDisplay[] updatedSolrCores)
        {
            var updatedSolrCoreIds = updatedSolrCores.Select(x => x.Id);
            using (var context = new SolrCoreContext())
            {
                try
                {
                    var oldSolrCores = context.SolrCores.Where(x => updatedSolrCoreIds.Contains(x.SolrCoreId)).ToList();
                    oldSolrCores.ForEach(oldSolrCore =>
                                             {
                                                 var updatedSolrCore = updatedSolrCores.Single(x => x.Id == oldSolrCore.SolrCoreId);
                                                 oldSolrCore.ModifiedOn = DateTime.Now;
                                                 oldSolrCore.SortOrder = updatedSolrCore.SortOrder;
                                             });
                    context.SaveChanges();
                    Clients.All().sortOrdersUpdated(oldSolrCores.Select(x => new SolrCoreDisplay(x)));
                    return true;
                }
                catch (Exception)
                {
                    Clients.Caller.reportError("Unable to update core sort orders.");
                    return false;
                }
            }
        }

        public bool Remove(int Id)
        {
            try
            {
                using (var context = new SolrCoreContext())
                {
                    var solrCore = context.SolrCores.FirstOrDefault(c => c.SolrCoreId == Id);
                    context.SolrCores.Remove(solrCore);
                    context.SaveChanges();
                    Clients.All().solrCoreRemoved(Id);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.reportError("Error : " + ex.Message);
                return false;
            }
        }

        public bool Reindex(int Id)
        {
            try
            {
                using (var context = new SolrCoreContext())
                {
                    var solrCore = context.SolrCores.FirstOrDefault(c => c.SolrCoreId == Id);
                    ReindexCore(solrCore.BaseUrl);
                    Clients.All().solrCoreReindexing(Id);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.reportError("Error : " + ex.Message);
                return false;
            }
        }

        public bool Abort(int Id)
        {
            try
            {
                using (var context = new SolrCoreContext())
                {
                    var solrCore = context.SolrCores.FirstOrDefault(c => c.SolrCoreId == Id);
                    AbortIndex(solrCore.BaseUrl);
                    Clients.All().solrCoreAborting(Id);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.reportError("Error : " + ex.Message);
                return false;
            }
        }

        public void GetAll()
        {
            using (var context = new SolrCoreContext())
            {
                var res = context.SolrCores.ToList().Select(x => new SolrCoreDisplay(x)).OrderBy(x => x.SortOrder);
                Clients.Caller.solrCoreAll(res.ToArray());
            }
        }

        private void UpdateAllStatuses(object state)
        {
            using (var context = new SolrCoreContext())
            {
                var res = context.SolrCores.ToList().Select(x => new SolrCoreDisplay(x)).ToList();
                res.ForEach(UpdateStatus);
            }
        }

        private void UpdateStatus(SolrCoreDisplay updatedSolrCore)
        {
            var status = GetCoreStatusInfo(updatedSolrCore.BaseUrl);
            Clients.All.solrCoreStatusUpdated(updatedSolrCore.Id, status);
        }

        private CoreStatusDisplay GetCoreStatusInfo(string coreUrl)
        {
            var responseElement = GetCoreStatusResponse(coreUrl);
            if (responseElement != null)
            {
                var statusElement =
                    responseElement.Elements("str").FirstOrDefault(x => x.Attribute("name").Value == "status");
                switch (statusElement.Value)
                {
                    case "idle":
                        var statusMessagesElement = responseElement.Elements("lst").FirstOrDefault(x => x.Attribute("name").Value == "statusMessages");
                        if (statusMessagesElement != null)
                        {
                            var timeTakenElement = statusMessagesElement.Elements("str").FirstOrDefault(x => x.Attribute("name").Value == "Time taken ");
                            TimeSpan? timeTaken = null;
                            if (timeTakenElement != null)
                            {
                                timeTaken = TimeSpan.Parse(timeTakenElement.Value);
                            }
                            var abortedElement = statusMessagesElement.Elements("str").FirstOrDefault(x => x.Attribute("name").Value == "Aborted");
                            CoreStatusDisplay resultStatusDisplay = null;
                            if (timeTaken.HasValue)
                            {
                                resultStatusDisplay = new IdleCoreStatusDisplay() { TimeTaken = timeTaken.Value };
                                if (abortedElement != null)
                                {
                                    resultStatusDisplay = new AbortedCoreStatusDisplay() {TimeTaken = timeTaken.Value};
                                }
                                return resultStatusDisplay;
                            }
                        }
                        return new IdleCoreStatusDisplay();
                    case "busy":
                        statusMessagesElement = responseElement.Elements("lst").FirstOrDefault(x => x.Attribute("name").Value == "statusMessages");
                        if (statusMessagesElement != null)
                        {
                            var timeElapsedElement = statusMessagesElement.Elements("str").FirstOrDefault(x => x.Attribute("name").Value == "Time Elapsed");
                            
                            var totalRowsFetchedElement = statusMessagesElement.Elements("str").FirstOrDefault(x => x.Attribute("name").Value == "Total Rows Fetched");
                            var totalDocumentsProcessedElement = statusMessagesElement.Elements("str").FirstOrDefault(x => x.Attribute("name").Value == "Total Documents Processed");
                            if (timeElapsedElement != null && totalRowsFetchedElement != null && totalDocumentsProcessedElement != null)
                            {
                                TimeSpan? timeElapsed = null;
                                if (timeElapsedElement != null)
                                {
                                    timeElapsed = TimeSpan.Parse(timeElapsedElement.Value);
                                }
                                var totalRowsFetched = int.Parse(totalRowsFetchedElement.Value);
                                var totalDocumentsProcessed = int.Parse(totalDocumentsProcessedElement.Value);
                                return new IndexingCoreStatusDisplay()
                                {
                                    TimeElapsed = timeElapsed.Value,
                                    TotalDocumentsProcessed = totalDocumentsProcessed,
                                    TotalRowsFetched = totalRowsFetched
                                };
                            }
                            
                        }
                        return new IndexingCoreStatusDisplay()
                        {
                            TimeElapsed = new TimeSpan(0, 0, 0, 0),
                            TotalDocumentsProcessed = 0,
                            TotalRowsFetched = 0
                        };
                    default:
                        return new ErrorCoreStatusDisplay() { Message = string.Format("Error: Unable to get status for core at '{0}'.", coreUrl) };
                }
            }
            else
            {
                return new ErrorCoreStatusDisplay() { Message = string.Format("Error: Unable to get status for core at '{0}'.", coreUrl) };
            }
        }

        private static XElement GetCoreStatusResponse(string coreUrl)
        {
            const string queryString = "qt=/dataimport&command=status";
            var url = string.Format("{0}/select?{1}", coreUrl, Uri.EscapeUriString(queryString));

            using (var wc = new WebClient())
            {
                try
                {
                wc.Encoding = System.Text.Encoding.UTF8;
                wc.Headers["User-Agent"] = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; .NET4.0C; .NET4.0E)";
                var responseString = wc.DownloadString(url);

                
                    var xmlDoc = XDocument.Parse(responseString);
                    var responseElement = xmlDoc.Element("response");
                    return responseElement;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        private static void ReindexCore(string coreUrl)
        {
            CommitCommand(coreUrl, "full-import", true);
        }
        private static void AbortIndex(string coreUrl)
        {
            CommitCommand(coreUrl, "abort");
        }

        private static void CommitCommand(string coreUrl, string command, bool clean = false)
        {
            var queryString = string.Format("qt=/dataimport&verbose=true&commit=true&command={0}{1}", command, clean ? "&clean = true" : "");
            var url = string.Format("{0}/select?{1}", coreUrl, Uri.EscapeUriString(queryString));

            using (var wc = new WebClient())
            {
                try
                {
                    wc.Encoding = System.Text.Encoding.UTF8;
                    wc.Headers["User-Agent"] = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; .NET4.0C; .NET4.0E)";
                    wc.DownloadString(url);
                }
                catch (Exception)
                {

                }
                return;
            }
        }
    }
}