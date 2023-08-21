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
using Autodesk.Civil.Runtime;
using Autodesk.Aec.Geometry;
using Autodesk.Civil.Settings;

//Manual Namespaces @Jacobs.DD
//Folder: AbstractObjects 
using Jacobs.DD.AbstractObjects;
using System.Text.RegularExpressions;

namespace Objects
{
    #region Corridor Properties

    public class CorridorsObject : BaseObjects
    {
        #region Corridor specific properties

        //Result: Is corridor name valid?
        private string _IsValidCorridorName = "❌";
        public string IsValidCorridorName
        {
            get { return _IsValidCorridorName; }
            set { _IsValidCorridorName = value; }
        }

        #endregion

        #region Baseline properties associated with the Corridor.

        //BaselineName: Name of baseline associated with Corridor
        private List<string> _AssociatedBaselineObjectName = new List<string>();
        public List<string> AssociatedBaselineObjectName
        {
            get { return _AssociatedBaselineObjectName; }
            set { _AssociatedBaselineObjectName = value; }
        }

        //Unused
        //BaselineId: ObjectId of baseline associated with corridor.
        private List<ObjectId> _AssociatedBaselineObjectId = new List<ObjectId>();
        public List<ObjectId> AssociatedBaselineObjectId
        {
            get { return _AssociatedBaselineObjectId; }
            set { _AssociatedBaselineObjectId = value; }
        }


        //Result: Is corridor associated baseline names valid?
        private List<bool> _IsValidAssociatedBaselineName = new List<bool>();
        public List<bool> IsValidAssociatedBaselineName
        {
            get { return _IsValidAssociatedBaselineName; }
            set { _IsValidAssociatedBaselineName = value; }
        }

        #endregion

        #region Layer Properties associated with the corridor.

        //LayerName: Name of layer associated with the corridor
        private string _AssociatedLayerObjectName = "";
        public string AssociatedLayerObjectName
        {
            get { return _AssociatedLayerObjectName; }
            set
            {
                _AssociatedLayerObjectName = value;
                OnPropertyChanged();
            }
        }

        //LayerId: ObjectId of layer associated with corridor.
        private ObjectId _AssociatedLayerObjectId = ObjectId.Null;
        public ObjectId AssociatedLayerObjectId
        {
            get { return _AssociatedLayerObjectId; }
            set { _AssociatedLayerObjectId = value; }
        }


        //Result: Is corridor associated layer name valid?
        private string _IsValidAssociatedLayerName = "❌";
        public string IsValidAssociatedLayerName
        {
            get { return _IsValidAssociatedLayerName; }
            set { _IsValidAssociatedLayerName = value; }
        }

        #endregion

    }

    #endregion

    #region Corridor Methods

    public class CorridorsObjectCollection : System.Collections.ObjectModel.ObservableCollection<CorridorsObject>
    {
        //Generate list of Corridors available in Civil3D Document.
        public void GetFromCorridorsDatabase()
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            Database corridorDatabase = Doc.Database;
            //Lock document
            using (DocumentLock DocLock = Doc.LockDocument())
            {
                using (Transaction trans = corridorDatabase.TransactionManager.StartTransaction())
                {
                    try
                    {
                        //Corridor is available in Civil3D but not in AutoCAD.
                        //Directly using document variable considers AutoCAD Document. Careful!
                        var civilDoc = CivilApplication.ActiveDocument;
                        var corridorColl = civilDoc.CorridorCollection;
                        CorridorsObject corridorObj; //Corridors Placeholder.

                        foreach (ObjectId objectId in corridorColl)
                        {
                            //Accessing Corridors.
                            var corridor = (Corridor)trans.GetObject(objectId, OpenMode.ForRead);
                            if (corridor.Name.Contains("|") == true) continue;

                            //Adding each corridors to the observable collection.
                            corridorObj = new CorridorsObject();
                            corridorObj.Name = corridor.Name;
                            corridorObj.BaseId = objectId;
                            //Associated Layer Info:
                            corridorObj.AssociatedLayerObjectName = corridor.Layer;
                            corridorObj.AssociatedLayerObjectId = corridor.LayerId;
                            //Associated Baseline Info: (There can be multiple baselines attached to one corridor.)
                            foreach (Baseline baseline in corridor.Baselines)
                            {
                                corridorObj.AssociatedBaselineObjectName.Add(baseline.Name);
                            }
                            //Adding this corridorObj to the collection.
                            this.Add(corridorObj);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Application.ShowAlertDialog("Unable to access Corridors.\nError: " + ex.Message);
                    }
                    trans.Commit();
                }
            }//Document unlocked.
        }

        //Updating Corridor Names.
        public void UpdateCorridors(ObjectId corridorId, string newCorridorName)
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            try
            {
                //Lock document
                using (DocumentLock DocLock = Doc.LockDocument())
                {
                    using (Transaction trans = corridorId.Database.TransactionManager.StartTransaction())
                    {
                        //Accessing Corridor Collection
                        var corridor = (Corridor)trans.GetObject(corridorId, OpenMode.ForWrite);
                        //Change the Corridor name
                        corridor.Name = newCorridorName;
                        trans.Commit();
                    }
                }//Document unlocked.
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog("Corridor names should be unique.\nError: " + ex.Message);
            }
        }

        //Delete Corridor
        public void DeleteCorridor(ObjectId corridorObjId)
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            try
            {
                //Lock document
                using (DocumentLock DocLock = Doc.LockDocument())
                {
                    using (Transaction trans = corridorObjId.Database.TransactionManager.StartTransaction())
                    {
                        //Open up Corridors List for write.
                        var corridor1 = (Corridor)trans.GetObject(corridorObjId, OpenMode.ForWrite);
                        //Delete the corridor.
                        corridor1.Erase(true);
                        trans.Commit();
                    }
                }//Document unlocked.
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog("The selected corridor cann't be deleted.\nError: " + ex.Message);
            }
        }

        //Refresh Corridors list
        public CorridorsObjectCollection Refresh(CorridorsObjectCollection newCorridorsList)
        {
            //Get latest Corridors list, copy to incoming collection and return it.
            newCorridorsList.GetFromCorridorsDatabase();
            return newCorridorsList;
        }

        //Checking whether given Corridor complies with BIM Standard.
        public void CheckCorridorName(CorridorsObject corridorObject1, string SelectedBIMStandard)
        {
            //BASIC Check
            //Run RegEx to validate names.
            var pattern = "";
            switch (SelectedBIMStandard)
            {
                case "GG184":
                    pattern = @"^(?<C3DComponent>[C]{1})_(?<ModelName>[A-Za-z0-9]*?)_(?<Direction>EB|WB|Both|East|West|North|South)_(?<RoadType>[A-Za-z]*?)(_?(?<UniqueId>[0-9]{4}))?$";
                    break;
                case "IAN184":
                    pattern = @"";
                    break;
                default:
                    break;
            }
            Match match = Regex.Match(corridorObject1.Name, pattern);
            corridorObject1.IsValidCorridorName = match.Success ? "✔" : "❌";
        }

        //Update the layer name associated with Alignments.
        public void UpdateAssociatedLayer(CorridorsObject corridorObj, string newAssociatedLayerName)
        {
            try
            {
                var layerObjColl1 = new Objects.LayerObjectCollection();
                layerObjColl1.UpdateLayer(corridorObj.AssociatedLayerObjectId, newAssociatedLayerName);
                corridorObj.AssociatedLayerObjectName = newAssociatedLayerName;
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog("Unable to update associated layer name.\nError: " + ex.Message);
            }
        }

        //Checking whether given associated Layer complies with BIM Standard.
        public void CheckAssociatedLayerObjectName(CorridorsObject corridorObj2, string SelectedBIMStandard)
        {
            //Run RegEx to validate names.
            var pattern = "";
            switch (SelectedBIMStandard)
            {
                case "GG184":
                    //<Prefix: C-Zz3510-M-AlignHoriz_>-<Parent Alignment Name>
                    pattern = @"^(?<AssociatedLayerPrefix>C-SL8035-M-Corridor)_" + corridorObj2.Name + "$";
                    break;
                case "IAN184":
                    pattern = @"^[A-Z][A-Za-z]{0,1}-[A-Z][A-Za-z](_[0-9]{2}){1,4}( [A-Z]{3,4}){0,1}-[D|M|T|H|P](-[A-Z][a-z]){0,1}_[A-Za-z0-9_ ]{0,250}$";
                    break;
                default:
                    break;
            }
            Match match = Regex.Match(corridorObj2.AssociatedLayerObjectName, pattern);
            //Both parent alignment name and Associated layer names must be valid together.
            corridorObj2.IsValidAssociatedLayerName = (match.Success && corridorObj2.IsValidCorridorName == "✔") ? "✔" : "❌";
        }

        //Autocorrect Associated Layer names
        public void AutocorrectLayerName(CorridorsObject corridorObj3, String SelectedBIMStandard)
        {
            var newAssociatedLayerObjName = corridorObj3.AssociatedLayerObjectName;

            switch (SelectedBIMStandard)
            {
                case "GG184":
                    newAssociatedLayerObjName = "C-SL8035-M-Corridor_" + corridorObj3.Name;
                    break;
                case "IAN184":
                    //Add autocorrect functionality for IAN184
                    break;
                default:
                    break;
            }
            UpdateAssociatedLayer(corridorObj3, newAssociatedLayerObjName);
            CheckAssociatedLayerObjectName(corridorObj3, SelectedBIMStandard);
        }
    }

    #endregion
}
