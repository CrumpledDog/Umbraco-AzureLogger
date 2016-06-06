namespace Our.Umbraco.AzureLogger.Core.Controllers
{
    using global::Umbraco.Core;
    using global::Umbraco.Core.Services;
    using global::Umbraco.Web.Models.Trees;
    using global::Umbraco.Web.Mvc;
    using global::Umbraco.Web.Trees;
    using log4net;
    using System.Linq;
    using System.Net.Http.Formatting;
    using umbraco.BusinessLogic.Actions;
    using UmbracoTreeController = global::Umbraco.Web.Trees.TreeController;

    [Tree("developer", "azureLoggerTree", "Azure Logger", "icon-folder", "icon-folder-open", true, 0)]
    [PluginController("AzureLogger")]
    public class TreeController : UmbracoTreeController
    {
        /// <summary>
        /// Called by Umbraco to get the nodes for the tree (the 'Reload nodes' menu option will trigger this)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            TreeNodeCollection treeNodeCollection = new TreeNodeCollection();

            if (this.IsRoot(id))
            {
                // get all table appenders
                foreach(TableAppender tableAppender in  LogManager
                                                            .GetLogger(typeof(TableAppender))
                                                            .Logger
                                                            .Repository
                                                            .GetAppenders()
                                                            .Where(x => x is TableAppender)
                                                            .Cast<TableAppender>()
                                                            .OrderBy(x => x.Name))
                {
                    if (tableAppender.IsConnected())
                    {
                        treeNodeCollection.Add(this.CreateTreeNode(
                                                        "appender|" + tableAppender.Name, // id - appender name should be distinct
                                                        "-1", // parentId
                                                        queryStrings,
                                                        tableAppender.TreeName ?? tableAppender.Name,  // get the best friendly name
                                                        !string.IsNullOrWhiteSpace(tableAppender.IconName) ? tableAppender.IconName : "icon-list",
                                                        false, // has children
                                                        this.BuildRoute("ViewLog", tableAppender.Name)));
                    }
                    else
                    {
                        // no connection
                        treeNodeCollection.Add(this.CreateTreeNode(
                                                        "noConnection|" + tableAppender.Name,
                                                        "-1",
                                                        queryStrings,
                                                        tableAppender.TreeName ?? tableAppender.Name,  // get the best friendly name
                                                        "icon-alert-alt red",
                                                        false,
                                                        "/developer/"));
                    }
                }
            }
            //else if (id.StartsWith("appender"))
            //{
            //    treeNodeCollection.Add(this.CreateTreeNode(
            //                                    "filter|",
            //                                    id,
            //                                    queryStrings,
            //                                    "example filter node",
            //                                    "icon-filter",
            //                                    false,
            //                                    this.BuildRoute("ViewLog", string.Empty)));
            //}

            return treeNodeCollection;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            MenuItemCollection menuItemCollection = new MenuItemCollection();

            ILocalizedTextService localizedTextService = ApplicationContext.Services.TextService;

            if (this.IsRoot(id))
            {
                menuItemCollection.Items.Add<ActionRefresh>(localizedTextService.Localize(ActionRefresh.Instance.Alias), false);
            }
            else if (id.StartsWith("appender"))
            {
                menuItemCollection.Items.Add(new MenuItem("WipeLog", "Wipe Log") { Icon = "alert" }); // red class doesn't work here
            }
            else if (id.StartsWith("noConnection"))
            {
                //below refreshes child nodes only !
                //menuItemCollection.Items.Add<ActionRefresh>(localizedTextService.Localize(ActionRefresh.Instance.Alias), true);
            }

            return menuItemCollection;
        }

        private bool IsRoot(string id)
        {
            return id == Constants.System.Root.ToInvariantString();
        }

        /// <summary>
        /// Helper to specify the html file to use
        /// http://umbraco.github.io/Belle/#/tutorials/Creating-Editors-Trees
        /// https://our.umbraco.org/forum/umbraco-7/developing-umbraco-7-packages/46710-Optional-view-as-opposed-to-edithtml-for-tree-action
        /// </summary>
        /// <param name="htmlView">the html file</param>
        /// <param name="id">-1 indicates no id</param>
        /// <returns></returns>
        private string BuildRoute(string htmlView, string id = "-1")
        {
            if (string.IsNullOrWhiteSpace(htmlView)) { htmlView = "ViewLog"; }
            if (string.IsNullOrWhiteSpace(id)) { id = "-1"; } // id required in url, else fails to auto route

            // section/tree/method/id
            return string.Format("/developer/azureLoggerTree/{0}/{1}", htmlView, id);
        }

        //private static FormDataCollection Add(FormDataCollection querystring, string key, string value)
        //{
        //    return querystring;
        //    //queryStrings.ReadAsNameValueCollection().Add("filter", "level");
        //}
    }
}
