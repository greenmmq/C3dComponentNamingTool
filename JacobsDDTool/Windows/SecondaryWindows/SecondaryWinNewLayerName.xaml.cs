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
using System.Windows.Shapes;

namespace Jacobs.DD.Windows.SecondaryWindows
{
    /// <summary>
    /// Interaction logic for SecondaryWinNewLayerName.xaml
    /// </summary>
    public partial class SecondaryWinNewLayerName : Window
    {
        public SecondaryWinNewLayerName()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            //Starting a new LayerObjectCollection
            var layerObjColl1 = new Objects.LayerObjectCollection();
            //Try to create a new layer.
            try
            {
                //Confirm that TextBox was not empty.
                if (textBoxNewLayerName.Text != null)
                {
                    //Pass TextBox content as New Layer Name and create new layer.
                    layerObjColl1.CreateNewLayer(textBoxNewLayerName.Text);
                    Close();
                }
                else throw new Exception("Layer name can't be blank.");
            }
            catch (Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Invalid Operation.\n"+ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
