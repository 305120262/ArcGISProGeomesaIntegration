using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.PluginDatastore;
using ArcGIS.Core.Geometry;

namespace ProPlugin1
{
    public class ProPluginDatasourceTemplate : PluginDatasourceTemplate
    {
        public RpcClient client = new RpcClient();
        private Dictionary<string, PluginTableTemplate> _tables;
        public override void Open(Uri connectionPath)
        {
            //TODO Initialize your plugin instance. Individual instances
            //of your plugin may be initialized on different threads

            string result = client.CallAsync("open").GetAwaiter().GetResult();
            if(result!="opened")
            {
                throw new System.IO.DirectoryNotFoundException(connectionPath.LocalPath);
            }
            _tables = new Dictionary<string, PluginTableTemplate>();
        }

        public override void Close()
        {
            //TODO Cleanup required to close the plugin 
            //data source instance
            string result = client.CallAsync("close").GetAwaiter().GetResult();
            _tables.Clear();
        }

        public override PluginTableTemplate OpenTable(string name)
        {
            //TODO Open the given table/object in the plugin
            //data source
            if (!_tables.Keys.Contains(name))
            {
                var table = new ProPluginTableTemplate(name, client);
                _tables.Add(name, table);
            }
            return _tables[name];
        }

        public override IReadOnlyList<string> GetTableNames()
        {
            var tableNames = new List<string>();
            
            tableNames.Add("gdelt-quickstart");

            //TODO Return the names of all tables in the plugin
            //data source
            return tableNames;
        }

        public override bool IsQueryLanguageSupported()
        {
            //default is false
            return base.IsQueryLanguageSupported();
        }
    }
}
