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
using Autodesk.AutoCAD.ApplicationServices;


namespace Jacobs.DD.Windows
{
    public partial class winWelcomeScreen : Window
    {
        //Initializes Welcome screen.
        public winWelcomeScreen()
        {
            InitializeComponent();
        }

        //Available CommandMethods in Initialization Class
        enum RunCommandMethod { JDDLayersList, JDDAlignmentList, JDDCorridorsList, JDDSurfaceList, JDDAssemblyList, JDDSubAssemblyList };

        //Switch tool to run various Commands from Initialization Class.
        private void SwitchC3DComponents(RunCommandMethod selectedC3DCommand)
        {
            //Create a new variable to access CommandMethods available in Initialization Class.
            var initializationObj = new Initialization();
            //Run CommandMethods from Initialization Class.
            switch (selectedC3DCommand)
            {
                case RunCommandMethod.JDDLayersList: initializationObj.cmdAcadLayerList(); break;
                case RunCommandMethod.JDDAlignmentList: initializationObj.cmdAcadAlignmentList(); break;
                case RunCommandMethod.JDDCorridorsList: initializationObj.cmdAcadCorridorsList(); break;
                case RunCommandMethod.JDDSurfaceList: initializationObj.cmdAcadSurfaceList(); break;
                case RunCommandMethod.JDDAssemblyList: initializationObj.cmdAcadAssemblyList(); break;
                case RunCommandMethod.JDDSubAssemblyList: initializationObj.cmdAcadSubAssemblyList(); break;
                default: break;
            }
        }

        //Following button turns SwitchC3DComponents On/Off.
        #region Buttons on WPF Window.

        private void btnLayersList_Click(object sender, RoutedEventArgs e)
        {
            SwitchC3DComponents(RunCommandMethod.JDDLayersList);
        }

        private void btnAlignmentList_Click(object sender, RoutedEventArgs e)
        {
            SwitchC3DComponents(RunCommandMethod.JDDAlignmentList);
        }

        private void btnCorridorsList_Click(object sender, RoutedEventArgs e)
        {
            SwitchC3DComponents(RunCommandMethod.JDDCorridorsList);
        }

        private void btnSurfacesList_Click(object sender, RoutedEventArgs e)
        {
            SwitchC3DComponents(RunCommandMethod.JDDSurfaceList);
        }

        private void btnAssembliesList_Click(object sender, RoutedEventArgs e)
        {
            SwitchC3DComponents(RunCommandMethod.JDDAssemblyList);
        }

        private void btnSubAssembliesList_Click(object sender, RoutedEventArgs e)
        {
            SwitchC3DComponents(RunCommandMethod.JDDSubAssemblyList);
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}
