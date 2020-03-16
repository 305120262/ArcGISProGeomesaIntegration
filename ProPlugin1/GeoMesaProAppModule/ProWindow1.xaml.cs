using ArcGIS.Core.Data;
using ArcGIS.Core.Data.PluginDatastore;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GeoMesaProAppModule
{
    /// <summary>
    /// Interaction logic for ProWindow1.xaml
    /// </summary>
    public partial class ProWindow1 : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ProWindow1()
        {
            InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            String uri = String.Format("{0}:{1}|{2}", hostnameTbx.Text, portTbx.Text, catalogTbx.Text);
            QueuedTask.Run(()=> {
                using (PluginDatastore pluginws = new PluginDatastore(new PluginDatasourceConnectionPath("HbasePlugin1_Datasource", new Uri(uri, UriKind.Absolute))))
                {
                    using (var table = pluginws.OpenTable("gdelt-quickstart"))
                    {
                        //Add as a layer to the active map or scene
                        LayerFactory.Instance.CreateFeatureLayer((FeatureClass)table, MapView.Active.Map);
                    }
                }
            });
            this.Close();
        }

    }
}
