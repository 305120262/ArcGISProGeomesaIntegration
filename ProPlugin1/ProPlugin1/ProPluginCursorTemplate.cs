using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.PluginDatastore;
using ArcGIS.Core.Geometry;
using System.Text.RegularExpressions;

namespace ProPlugin1
{
    public class ProPluginCursorTemplate : PluginCursorTemplate
    {
        private RpcClient client;
        private Queue<string> _oids;
        private List<string> _columnFilters;
        private string _current = "";
        private static readonly object _lock = new object();
        private ProPluginTableTemplate _table;
        private int _count;
        

        public ProPluginCursorTemplate(RpcClient client, IEnumerable<string> oids, List<string> columnFilters,ProPluginTableTemplate table)
        {
            this.client = client;
            this._oids = new Queue<string>(oids);
            _columnFilters = columnFilters;
            _table = table;
            _count = this._oids.Count;
        }

        public override PluginRow GetCurrentRow()
        {
            string id = "";
            //The lock shouldn't be necessary if your cursor is a per thread instance
            //(like the sample is)
            lock (_lock)
            {
                id = _current;
            }
            var listOfRowValues = new List<object>();
            var parameters = string.Format("{0},{1}", _table.GetName(), id);
            string result = client.CallAsync("findRow|"+ parameters).GetAwaiter().GetResult();

            var rawValues = result.Split('|').ToArray();

            int index = 0;
            foreach (var field in _table.GetFields())
            {
                if (_columnFilters.Contains(field.Name.ToUpper()))
                {
                    if (field.Name == "OBJECTID")
                    {
                        listOfRowValues.Add(this._count-this._oids.Count()-1);
                    }
                    else if(field.Name== "geom")
                    {
                        Regex rgx = new Regex(@"[-+]?[0-9]*\.?[0-9]+", RegexOptions.Compiled);
                        var matches = rgx.Matches(rawValues.GetValue(index).ToString());
                        double x=0 , y=0;
                        int i = 0;
                        foreach (Match m in matches)
                        {
                            if(i==0)
                            {
                                x = double.Parse(m.Value);
                            }
                            else if (i == 1)
                            {
                                y = double.Parse(m.Value);
                            }
                            i++;
                        }
                        
                        var mappoint = MapPointBuilder.CreateMapPoint(x,y, SpatialReferenceBuilder.CreateSpatialReference(4326));
                        listOfRowValues.Add(mappoint);
                    }
                    else
                    {
                        listOfRowValues.Add(rawValues.GetValue(index));
                        index++;
                    }
                }
                else
                {
                    listOfRowValues.Add(System.DBNull.Value);
                    index++;
                }
                
            }

            return new PluginRow(listOfRowValues);
        }

        public override bool MoveNext()
        {
            if (_oids.Count == 0)
                return false;

            //The lock shouldn't be necessary if your cursor is a per thread instance
            //(like the sample is)
            lock (_lock)
            {
                _current = _oids.Dequeue();
            }
            return true;
        }
    }
}
