using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.PluginDatastore;
using ArcGIS.Core.Geometry;
using Newtonsoft.Json;

namespace ProPlugin1
{
    public class ProPluginTableTemplate : PluginTableTemplate
    {

        private readonly String name;
        private readonly RpcClient client;
        private List<PluginField> pluginFields;

        public ProPluginTableTemplate(String name,RpcClient client)
        {
            this.name = name;
            this.client = client;
        }
        
        public override IReadOnlyList<PluginField> GetFields()
        {
            if (pluginFields == null)
            {
                pluginFields = new List<PluginField>();
                //TODO Get the list of PluginFields for this currently opened 
                //plugin table/object
                string result = client.CallAsync("getFields|" + this.name).GetAwaiter().GetResult();
                var fields = JsonConvert.DeserializeObject<List<string>>(result);

                pluginFields.Add(new PluginField("OBJECTID", "OBJECTID", FieldType.OID));

                foreach (var f in fields)
                {
                    if (f == "geom")
                    {
                        pluginFields.Add(new PluginField(f, f, FieldType.Geometry));
                    }
                    else
                    {
                        pluginFields.Add(new PluginField(f, f, FieldType.String));
                    }
                }
            }
            return pluginFields;
        }

        public override string GetName()
        {
            //TODO Get the name of this currently opened plugin table/object
            return name;
        }

        public override PluginCursorTemplate Search(QueryFilter queryFilter)
        {
            //TODO Perform a non-spatial search on this currently opened 
            //plugin table/object
            //Where clause will always be empty if 
            //PluginDatasourceTemplate.IsQueryLanguageSupported = false.

            string result = client.CallAsync("queryRows|" + this.name).GetAwaiter().GetResult();
            var oids = JsonConvert.DeserializeObject<List<string>>(result);
            var columns = this.GetQuerySubFields(queryFilter);
            var cur = new ProPluginCursorTemplate(client,oids, columns, this);
            return cur;
        }

        public override PluginCursorTemplate Search(SpatialQueryFilter spatialQueryFilter)
        {
            //TODO Perform a spatial search on this currently opened 
            //plugin table/object
            //Where clause will always be empty if 
            //PluginDatasourceTemplate.IsQueryLanguageSupported = false.
            Envelope ext = spatialQueryFilter.FilterGeometry.Extent;
            var parameters = String.Format("{0},{1},{2},{3},{4}", this.name, ext.XMin, ext.YMin, ext.XMax, ext.YMax);
            string result = client.CallAsync("squeryRows|" + parameters).GetAwaiter().GetResult();
            var oids = JsonConvert.DeserializeObject<List<string>>(result);
            var columns = this.GetQuerySubFields(spatialQueryFilter);
            var cur = new ProPluginCursorTemplate(client, oids, columns, this);
            
            return cur;
        }

        public override GeometryType GetShapeType()
        {
            //TODO return the correct GeometryType if the plugin table
            //is a feature class
            return GeometryType.Point;
        }

        public override Envelope GetExtent()
        {
            Envelope env = EnvelopeBuilder.CreateEnvelope(-180,-90,180,90, SpatialReferenceBuilder.CreateSpatialReference(4326));
            return env;
        }

        private List<string> GetQuerySubFields(QueryFilter qf)
        {
            //Honor Subfields in Query Filter
            string columns = qf.SubFields ?? "*";
            List<string> subFields;
            if (columns == "*")
            {
                subFields = this.GetFields().Select(col => col.Name.ToUpper()).ToList();
            }
            else
            {
                var names = columns.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                subFields = names.Select(n => n.ToUpper()).ToList();
            }

            return subFields;
        }
    }
}
