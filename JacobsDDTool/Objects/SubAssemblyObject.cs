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
    #region SubAssembly properties

    public class SubAssemblyObject : BaseObjects
    {
        //Result of SubassemblyName Validity.
        private string _IsValidSubAssemblyName = "❌";
        public string IsValidSubAssemblyName
        {
            get { return _IsValidSubAssemblyName; }
            set { _IsValidSubAssemblyName = value; }
        }
    }

    #endregion

    #region SubAssembly Methods

    public class SubAssemblyObjectCollection : System.Collections.ObjectModel.ObservableCollection<SubAssemblyObject>
    {
        //Generates collection of SubAssemblies in active document.
        public void GetFromSubAssemblyDatabase()
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            Database DrawingDatabase = HostApplicationServices.WorkingDatabase;
            //Lock document
            using (DocumentLock DocLock = Doc.LockDocument())
            {
                //Transaction Started.
                using (Transaction trans = DrawingDatabase.TransactionManager.StartTransaction())
                {
                    CivilDocument CivDoc = CivilApplication.ActiveDocument; //Active document selected.
                    SubassemblyCollection subAssemblyColl = CivDoc.SubassemblyCollection;
                    SubAssemblyObject subAssemblyObj;
                    try
                    {
                        foreach (ObjectId objId in subAssemblyColl)
                        {
                            Subassembly subAssembly = trans.GetObject(objId, OpenMode.ForRead) as Subassembly;
                            if (subAssembly.Name.Contains("|") == true) continue;

                            subAssemblyObj = new SubAssemblyObject();
                            subAssemblyObj.Name = subAssembly.Name;
                            subAssemblyObj.BaseId = objId;
                            this.Add(subAssemblyObj);
                        }
                    }
                    catch { }
                    trans.Commit(); //Transaction Closed.
                }
            }//Document unlocked.
        }

        //Updates changes in SubAssembly
        public void UpdateSubAssembly(ObjectId subAssemblyId, string newSubAssemblyName)
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            //Lock Document
            using (DocumentLock DocLock = Doc.LockDocument())
            {
                try
                {
                    using (Transaction trans = subAssemblyId.Database.TransactionManager.StartTransaction())
                    {
                        //Access Subassemblies.
                        var subAssembly = (Subassembly)trans.GetObject(subAssemblyId, OpenMode.ForWrite);
                        //Changing the Subassembly Name
                        subAssembly.Name = newSubAssemblyName;
                        trans.Commit();
                    }//End Transaction.
                }
                catch (System.Exception ex)
                {
                    Application.ShowAlertDialog("Subassembly names must be unique.\nError: " + ex.Message);
                }
            }//Document unlocked.
        }

        //Delete the SubAssembly
        public void DeleteSubAssembly(ObjectId subAssemblyObjId)
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            //Lock document
            using (DocumentLock DocLock = Doc.LockDocument())
            {
                try
                {
                    using (Transaction trans = subAssemblyObjId.Database.TransactionManager.StartTransaction())
                    {
                        //Open up the SubAssembly table
                        var subAssembly1 = (Subassembly)trans.GetObject(subAssemblyObjId, OpenMode.ForWrite);
                        //Delete the SubAssembly
                        subAssembly1.Erase(true);
                        trans.Commit();
                    }//End transaction.
                }
                catch (System.Exception ex)
                {
                    Application.ShowAlertDialog("The selected Subassembly can't be deleted.\nError: " + ex.Message);
                }
            }//Document unlocked.
        }

        //Refresh SubAssembly List
        public SubAssemblyObjectCollection Refresh(SubAssemblyObjectCollection newSubAssemblyList)
        {
            //Get latest SubAssembly list, copy to incoming collection and return it.
            newSubAssemblyList.GetFromSubAssemblyDatabase();
            return newSubAssemblyList;
        }

        //Checking whether given Subassembly complies with BIM Standard.
        public void CheckSubAssemblyName(SubAssemblyObject subAssemblyObject1, string SelectedBIMStandard)
        {
            //BASIC Check
            //Run RegEx to validate names.
            var pattern = "";
            switch (SelectedBIMStandard)
            {
                case "GG184":
                    pattern = @"^(?<C3DDefaultName>JENG_GG184)_(?<Description>[A-Za-z0-9]{1,100})_?(?<UniqueID>[0-9]{4})?$";
                    break;
                case "IAN184":
                    pattern = @"";
                    break;
                default:
                    break;
            }
            Match match = Regex.Match(subAssemblyObject1.Name, pattern);
            subAssemblyObject1.IsValidSubAssemblyName = match.Success ? "✔" : "❌";
        }
    }

    #endregion
}
