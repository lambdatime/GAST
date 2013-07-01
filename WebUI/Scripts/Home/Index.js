$(function () {
    //---- View Models

    //SolrCore View Model
    function solrCoreViewModel(id, title, baseUrl, sortOrder, ownerViewModel) {
        this.Id = id;
        this.Title = ko.observable(title);
        this.BaseUrl = ko.observable(baseUrl);
        this.SortOrder = ko.observable(sortOrder);
        this.remove = function () { ownerViewModel.removeSolrCore(this.Id); };
        this.reindex = function () { ownerViewModel.reindexSolrCore(this.Id); };
        this.abort = function () { ownerViewModel.abortIndex(this.Id); };
        this.query = function () { ownerViewModel.redirectQuery(this.Id); };
        this.notification = function (b) { notify = b; };
        this.Status = ko.observable(new solrCoreStatusIdledViewModel(this, "Not Available Yet", "0:0:0.00", ownerViewModel));
        this.StatusTooltipCssClass = "corestatustip";

        this.DisableIndexing = ko.computed(function () {
            return this.Status().StatusName() == "Busy";
        }, this);
        this.DisableAbort = ko.computed(function () {
            return this.Status().StatusName() != "Busy";
        }, this);

        this.IsError = ko.computed(function () {
            return this.Status().IsError;
        }, this);
        this.IsBusy = ko.computed(function () {
            return this.Status().StatusName() == "Busy";
        }, this);
        this.IsIdle = ko.computed(function () {
            return this.Status().StatusName() == "Idle";
        }, this);
        this.IsAborted = ko.computed(function () {
            return this.Status().StatusName() == "Aborted";
        }, this);

        this.StatusRowClass = ko.computed(function () {
            if (this.IsIdle()) return "success";
            if (this.IsBusy()) return "warning";
            if (this.IsAborted()) return "error";

            return "error";
        }, this);

        var self = this;

        this.Title.subscribe(function (newValue) {
            ownerViewModel.updateSolrCore(ko.toJS(self));
        });

        this.BaseUrl.subscribe(function (newValue) {
            ownerViewModel.updateSolrCore(ko.toJS(self));
        });
    }

    function solrCoreStatusBusyViewModel(solrCore, name, timeElapsed, totalRowsFetched, totalDocumentsProcessed, ownerViewModel) {
        this.StatusName = ko.observable(name);
        this.StatusAlt = ko.observable("Time elapsed: " + timeElapsed + "<br /> Rows Fetched: " + totalRowsFetched + "<br /> Documents Processed: " + totalDocumentsProcessed);
        this.TimeElapsed = ko.observable(timeElapsed);
        this.TotalRowsFetched = ko.observable(totalRowsFetched);
        this.TotalDocumentsProcessed = ko.observable(totalDocumentsProcessed);
        this.IsError = false;
    }

    function solrCoreStatusIdledViewModel(solrCore, name, timeTaken, ownerViewModel) {
        this.StatusName = ko.observable(name);
        if (timeTaken != "0:0:0.00")
            this.StatusAlt = ko.observable("Reindex completed after " + timeTaken + ".");
        else {
            this.StatusAlt = ko.observable("");
        }
        this.IsError = false;
        this.TimeTaken = timeTaken;
    }

    function solrCoreStatusAbortedViewModel(solrCore, name, timeTaken, ownerViewModel) {
        this.StatusName = ko.observable(name);
        this.StatusAlt = ko.observable("Reindex aborted after " + timeTaken + ".");
        this.IsError = false;
        this.TimeTaken = timeTaken;
    }

    function solrCoreErrorStatusViewModel(solrCore, name, errorMessage, ownerViewModel) {
        this.StatusName = ko.observable(name);
        this.StatusAlt = ko.observable(errorMessage);
        this.IsError = true;
        this.ErrorMessage = errorMessage;
    }

    //SolrCore List View Model
    function solrCoreListViewModel() {
        var self = this;
        //Handlers for our Hub callbacks

        self.hub = $.connection.solrCores;
        self.solrCores = ko.observableArray([]);
        self.newSolrCoreText = ko.observable();
        self.newSolrCoreBaseUrl = ko.observable();
        self.listCssClass = "corelist";

        var solrCores = self.solrCores;
        
        var notify = true;

        self.solrCores.subscribe(function (newValue) {
        });

        //Initializes the view model
        self.init = function () {
            self.hub.server.getAll();
        };

        //Handlers for our Hub callbacks
        //Invoked from our SolrCoreHub.cs

        self.hub.client.solrCoreAll = function (allCores) {

            var mappedCores = $.map(allCores, function (item) {
                return new solrCoreViewModel(item.Id, item.Title,
                    item.BaseUrl, item.SortOrder, self);
            });

            solrCores(mappedCores);

            //fix sort orders (if necessary)
            self.fixSolrCoreSortOrders();

            $("#coreTable tbody").sortable({
                helper: fixHelper,
                update: function (event, ui) {
                    var id = ui.item.first().attr('data-ref');
                    var solrCore = ko.utils.arrayFilter(solrCores(), function (value) { return value.Id == id; })[0];
                    var prevOrder = solrCore.SortOrder();
                    var newIndex = ui.item.index();


                    if (newIndex == prevOrder) return;
                    var coresInSortRange = null;
                    var increment = -1;
                    if (newIndex > prevOrder) {
                        coresInSortRange = ko.utils.arrayFilter(solrCores(), function (value) { return value.SortOrder() > prevOrder && value.SortOrder() <= newIndex; });
                    } else {
                        coresInSortRange = ko.utils.arrayFilter(solrCores(), function (value) { return value.SortOrder() < prevOrder && value.SortOrder() >= newIndex; });
                        increment = 1;
                    }
                    coresInSortRange.forEach(function (item) {
                        var newSortOrder = item.SortOrder() + increment;
                        item.SortOrder(newSortOrder);
                    });
                    solrCore.SortOrder(newIndex);
                    var coreCopy = coresInSortRange.slice();
                    coreCopy.push(solrCore);
                    self.updateMassSortOrders(ko.toJS(coreCopy));
                }
            }).disableSelection();
        };

        self.hub.client.solrCoreUpdated = function (sc) {
            var solrCore = ko.utils.arrayFilter(solrCores(), function (value) { return value.Id == sc.Id; })[0];
            notify = false;
            solrCore.Title(sc.Title);
            solrCore.BaseUrl(sc.BaseUrl);
            solrCore.SortOrder(sc.SortOrder);
            notify = true;
        };

        self.hub.client.solrCoreReindexing = function (sc) {
        };

        self.hub.client.solrCoreAborting = function (sc) {
        };

        self.hub.client.solrCoreStatusUpdated = function (scid, status) {
            var solrCore = ko.utils.arrayFilter(solrCores(), function (value) { return value.Id == scid; })[0];
            notify = false;
            switch (status.StatusName) {
                case "Busy":
                    solrCore.Status(new solrCoreStatusBusyViewModel(solrCore, status.StatusName, status.TimeElapsed, status.TotalRowsFetched, status.TotalDocumentsProcessed, self));
                    break;
                case "Error":
                    solrCore.Status(new solrCoreErrorStatusViewModel(solrCore, status.StatusName, status.Message, self));
                    break;
                case "Idle":
                    solrCore.Status(new solrCoreStatusIdledViewModel(solrCore, status.StatusName, status.TimeTaken, self));
                    break;
                case "Aborted":
                    solrCore.Status(new solrCoreStatusAbortedViewModel(solrCore, status.StatusName, status.TimeTaken, self));
                    break;
                default:
                    solrCore.Status(new solrCoreErrorStatusViewModel(solrCore, status.StatusName, "Not enough information", self));
                    break;
            }
            notify = true;

            $('.' + self.listCssClass + ' tr[data-ref=' + solrCore.Id + ']  .' + solrCore.StatusTooltipCssClass).tooltip();
        };

        self.hub.client.reportError = function(error) {
            $("#error").text(error);
            $("#error").fadeIn(1000, function() {
                $("#error").fadeOut(3000);
            });
        };

        self.hub.client.solrCoreAdded = function (sc) {
            solrCores.push(new solrCoreViewModel(sc.Id, sc.Title, sc.BaseUrl, sc.SortOrder, self));
        };

        self.hub.client.solrCoreRemoved = function (id) {
            var solrCore = ko.utils.arrayFilter(solrCores(), function (value) { return value.Id == id; })[0];
            solrCores.remove(solrCore);
        };

        self.hub.client.sortOrderUpdated = function (sc) {
            var solrCore = ko.utils.arrayFilter(solrCores(), function (value) { return value.Id == sc.Id; })[0];
            notify = false;
            solrCore.SortOrder(sc.SortOrder);
            notify = true;
        };

        self.hub.client.sortOrdersUpdated = function (changedCores) {
            notify = false;
            var newCores = solrCores().slice();
            changedCores.forEach(function (sc) {
                var solrCore = ko.utils.arrayFilter(newCores, function (value) { return value.Id == sc.Id; })[0];
                solrCore.SortOrder(sc.SortOrder);
            });

            var mappedCores = $.map(newCores.sort(function (a, b) {
                return a.SortOrder() > b.SortOrder() ? 1 : -1;
            }), function (item) {
                return new solrCoreViewModel(item.Id, item.Title(),
                                item.BaseUrl(), item.SortOrder(), self);
            });

            solrCores(mappedCores);
            notify = true;
        };

        //View Model 'Commands'

        //To create a solr core
        self.addSolrCore = function () {
            var sc = { "Title": self.newSolrCoreText(), "BaseUrl": self.newSolrCoreBaseUrl() };
            self.hub.server.add(sc).done(function () {
                if (!window.console)
                    console = {
                        log: function () {
                        }
                    };
                console.log('Success!');
            }).fail(function (e) {
                window.console && console.warn(e);
            });

            self.newSolrCoreText("");
            self.newSolrCoreBaseUrl("");
        };

        //To remove a solrcore
        self.removeSolrCore = function (id) {
            self.hub.server.remove(id);
        };

        //To update this solrcore
        self.updateSolrCore = function (solrCore) {
            if (notify)
                self.hub.server.update(solrCore);
        };

        self.reindexSolrCore = function (id) {
            self.hub.server.reindex(id);
        };

        self.abortIndex = function (id) {
            self.hub.server.abort(id);
        };

        self.updateSortOrder = function (solrCore) {
            self.hub.server.updateSortOrder(solrCore);
        };

        self.updateMassSortOrders = function (solrCoresToUpdate) {
            self.hub.server.updateMassSortOrders(solrCoresToUpdate);
        };

        self.redirectQuery = function (id) {
            window.location = '/Query/' + id;
        };

        self.fixSolrCoreSortOrders = function () {
            var sortedCores = solrCores().sort(function (a, b) { return a.SortOrder() > b.SortOrder() ? 1 : -1; });
            var prevCore = null;
            sortedCores.forEach(function (item) {
                var currentCore = item;
                if (prevCore != null && prevCore.SortOrder() >= currentCore.SortOrder()) {
                    currentCore.SortOrder(prevCore.SortOrder() + 1);
                    self.updateSortOrder(ko.toJS(item));
                }
                prevCore = item;
            });
        };
    }

    ko.bindingHandlers.disableClick = {
        init: function (element, valueAccessor) {
            $(element).click(function (evt) {
                if (valueAccessor())
                    evt.preventDefault();
            });

        },

        update: function (element, valueAccessor) {
            var value = ko.utils.unwrapObservable(valueAccessor());
            ko.bindingHandlers.css.update(element, function () { return { disabled: value }; });
        }
    };

    var vm = new solrCoreListViewModel();

    ko.applyBindings(vm);
    // Start the connection
    $.connection.hub.start().done(function () { vm.init(); });

    if (!console) { console = {}; console.log = function () { }; }

    // Return a helper with preserved width of cells
    var fixHelper = function (e, ui) {
        ui.children().each(function () {
            $(this).width($(this).width());
        });
        return ui;
    };
});