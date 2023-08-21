using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.ApplicationServices;


//Manual Namespaces @Jacobs.DD
//Folder: AbstractObjects 
using Jacobs.DD.AbstractObjects;
using System.Text.RegularExpressions;

namespace Objects
{
    #region Surface properties

    public class SurfaceObject : BaseObjects
    {
        //Result of SurfaceName Validity.
        private string _IsValidSurfaceName = "❌";
        public string IsValidSurfaceName
        {
            get { return _IsValidSurfaceName; }
            set { _IsValidSurfaceName = value; }
        }
    }

    #endregion

    #region Surface Methods

    public class SurfaceObjectCollection : System.Collections.ObjectModel.ObservableCollection<SurfaceObject>
    {
        //Generates the list of Surfaces available in Active document.
        public void GetFromSurfaceDatabase()
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            //Lock document
            using (DocumentLock DocLock = Doc.LockDocument())
            {
                Database DrawingDatabase = HostApplicationServices.WorkingDatabase;
                //Start transaction
                using (Transaction trans = DrawingDatabase.TransactionManager.StartTransaction())
                {
                    CivilDocument CivDoc = CivilApplication.ActiveDocument; //Active document selected.
                    //GetSurfaceIds() method is available so directly accessing it with ObjectIdCollection.
                    ObjectIdCollection SurIdColl = CivDoc.GetSurfaceIds();
                    SurfaceObject surfaceObj;
                    try
                    {
                        foreach (ObjectId objId in SurIdColl)
                        {
                            //Surface is available in both AutoCAD and Civil3D. This causes ambiguity.
                            var surface = (Autodesk.Civil.DatabaseServices.Surface)trans.GetObject(objId, OpenMode.ForRead);
                            if (surface.Name.Contains("|") == true) continue;

                            surfaceObj = new SurfaceObject();
                            surfaceObj.Name = surface.Name;
                            surfaceObj.BaseId = objId;
                            this.Add(surfaceObj);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Application.ShowAlertDialog("Unable to access Surface Table.\nError: " + ex.Message);
                    }
                    trans.Commit();
                }//End Transaction.
            }//Document unlocked.
        }

        //Updates the changes in Surfaces.
        public void UpdateSurface(ObjectId surfaceId, string newSurfaceName)
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            //Lock document
            using (DocumentLock DocLock = Doc.LockDocument())
            {
                try
                {
                    using (Transaction trans = surfaceId.Database.TransactionManager.StartTransaction())
                    {
                        //Surface is ambigious between AutoCAD and Civil3D so explicitly identify Civil3D Surface while accessing.
                        Autodesk.Civil.DatabaseServices.Surface surface = (Autodesk.Civil.DatabaseServices.Surface)trans.GetObject(surfaceId, OpenMode.ForWrite);
                        //Change the Civil3D Surface name.
                        surface.Name = newSurfaceName;
                        trans.Commit();
                    }
                }
                catch (System.Exception ex)
                {
                    Application.ShowAlertDialog("Surface name should be unique.\nError: " + ex.Message);
                }
            }//Document unlocked. 
        }

        //Delete the surface
        public void DeleteSurface(ObjectId surfaceObjId)
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            //Lock document
            using (DocumentLock DocLock = Doc.LockDocument())
            {
                try
                {
                    using (Transaction trans = surfaceObjId.Database.TransactionManager.StartTransaction())
                    {
                        //Open up the Surfaces table
                        var surface1 = (Autodesk.Civil.DatabaseServices.Surface)trans.GetObject(surfaceObjId, OpenMode.ForWrite);
                        //Delete the surface.
                        surface1.Erase(true);
                        trans.Commit();
                    }
                }
                catch (System.Exception ex)
                {
                    Application.ShowAlertDialog("The selected Surface can't be deleted.\nError: " + ex.Message);
                }
            }//Document unlocked.
        }

        //Refresh Surface list
        public SurfaceObjectCollection Refresh(SurfaceObjectCollection newSurfaceList)
        {
            //Get latest Surface list, copy to incoming collection and return it.
            newSurfaceList.GetFromSurfaceDatabase();
            return newSurfaceList;
        }

        //Checking whether given Surface complies with BIM Standard.
        public void CheckSurfaceName(SurfaceObject surfaceObject1, string SelectedBIMStandard)
        {
            //BASIC Check
            //Run RegEx to validate names.
            var pattern = "";
            switch (SelectedBIMStandard)
            {
                case "GG184":
                    pattern = @"^(?<C3DComponent>[S]{1})_(?<Type>((?<NonLinkedSurfaceType>[A-Za-z0-9]+)|(?<ParentCorridor>(?<Corridor>[C]{1})_(?<ModelName>[A-Za-z0-9]*?)_(?<Direction>EB|WB|Both|East|West|North|South)_(?<RoadType>[A-Za-z]*?)(_?(?<CorridorUniqueId>[0-9]{4}))?)))_(?<Description>[A-Za-z0-9]{1,100})(_?(?<SurfaceUniqueId>[0-9]{4}))?$";
                    break;
                case "IAN184":
                    pattern = @"";
                    break;
                default:
                    break;
            }
            Match match = Regex.Match(surfaceObject1.Name, pattern);
            surfaceObject1.IsValidSurfaceName = match.Success ? "✔" : "❌";
        }
    }

    #endregion
}
