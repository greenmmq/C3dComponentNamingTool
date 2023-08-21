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
    #region Assembly Properties

    public class AssemblyObject : BaseObjects
    {
        //Result of AssemblyName Validity.
        private string _IsValidAssemblyName = "❌";
        public string IsValidAssemblyName
        {
            get { return _IsValidAssemblyName; }
            set { _IsValidAssemblyName = value; }
        }
    }

    #endregion

    #region Assembly Methods

    public class AssemblyObjectCollection : System.Collections.ObjectModel.ObservableCollection<AssemblyObject>
    {
        //Generates list of all assemblies
        public void GetFromAssemblyDatabase()
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            CivilDocument CivDoc = CivilApplication.ActiveDocument; //Active document selected.
            Database DrawingDatabase = HostApplicationServices.WorkingDatabase;
            //Lock document
            using (DocumentLock DocLock = Doc.LockDocument())
            {
                //Transaction started.
                using (Transaction trans = DrawingDatabase.TransactionManager.StartTransaction())
                {
                    AssemblyCollection assemblyColl = CivDoc.AssemblyCollection;
                    AssemblyObject assemblyObj;

                    try
                    {
                        foreach (ObjectId objId in assemblyColl)
                        {
                            Assembly assembly = trans.GetObject(objId, OpenMode.ForRead) as Assembly;
                            if (assembly.Name.Contains("|") == true) continue;

                            assemblyObj = new AssemblyObject();
                            assemblyObj.Name = assembly.Name;
                            assemblyObj.BaseId = objId;
                            this.Add(assemblyObj);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Application.ShowAlertDialog("Unable to access Assemblies.\n" + ex.Message);
                    }
                    trans.Commit();
                }//Transaction Closed.
            }//Document unlocked.
        }

        //Updates changes in Assembly.
        public void UpdateAssembly(ObjectId assemblyId, string newAssemblyName)
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            try
            {
                //Lock Document
                using (DocumentLock DocLock = Doc.LockDocument())
                {
                    using (Transaction trans = assemblyId.Database.TransactionManager.StartTransaction())
                    {
                        //Access Assembly collection
                        var assembly = (Assembly)trans.GetObject(assemblyId, OpenMode.ForWrite);
                        //Change the assembly name.
                        assembly.Name = newAssemblyName;
                        trans.Commit();
                    }
                }//Document unlocked.
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog("Assembly name should be unique.\nError: " + ex.Message);
            }
        }

        //Delete Assembly
        public void DeleteAssembly(ObjectId assemblyObjId)
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            try
            {
                //Lock document.
                using (DocumentLock DocLock = Doc.LockDocument())
                {
                    using (Transaction trans = assemblyObjId.Database.TransactionManager.StartTransaction())
                    {
                        //Open up Assembly Table for write.
                        var assembly1 = (Assembly)trans.GetObject(assemblyObjId, OpenMode.ForWrite);
                        //Delete the assembly.
                        assembly1.Erase(true);
                        trans.Commit();
                    }
                }//Document unlocked.
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog("The selected assembly can't be deleted.\nError: " + ex.Message);
            }
        }

        //Refresh Assembly list.
        public AssemblyObjectCollection Refresh(AssemblyObjectCollection newAssemblyList)
        {
            //Get latest Assembly list, copy to incoming collection and return it.
            newAssemblyList.GetFromAssemblyDatabase();
            return newAssemblyList;
        }

        //Checking whether given Assembly complies with BIM Standard.
        public void CheckAssemblyName(AssemblyObject assemblyObject1, string SelectedBIMStandard)
        {
            //BASIC Check
            //Run RegEx to validate names.
            var pattern = "";
            switch (SelectedBIMStandard)
            {
                case "GG184":
                    pattern = @"^(?<C3DComponent>[A])_(?<ModelName>[A-Za-z0-9]+?)_(?<Direction>EB|WB|Both|East|West|North|South)_(?<Description>[A-Za-z0-9]{1,100})_?(?<UniqueID>[0-9]{4})?$";
                    break;
                case "IAN184":
                    pattern = @"";
                    break;
                default:
                    break;
            }
            Match match = Regex.Match(assemblyObject1.Name, pattern);
            assemblyObject1.IsValidAssemblyName = match.Success ? "✔" : "❌";
        }
    }

    #endregion
}
