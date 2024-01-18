using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CheckConnectionTool
{
    public class CheckConnectionToolCommand : Command
    {
        public CheckConnectionToolCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static CheckConnectionToolCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "CheckConnectionToolCommand";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var filter = new ObjectEnumeratorSettings();
            filter.HiddenObjects = true;
            var allObjects = doc.Objects.GetObjectList(filter);
            foreach (var tempObject in allObjects)
            {
                var tempAttributes = tempObject.Attributes;
                tempAttributes.Visible = true;
                tempObject.CommitChanges();
            }
            doc.Views.Redraw();
            // Select an object
            ObjRef objRef;

            var rc = RhinoGet.GetOneObject("Select object", false, ObjectType.AnyObject, out objRef);
            if (rc != Result.Success) return rc;

            var selectedRhinoObject = objRef.Object();
            if (selectedRhinoObject == null) return Result.Failure;

            foreach (var rhinoObject in doc.Objects)
            {
                string temp = selectedRhinoObject.Attributes.GetUserString("ConnectedLines");
                if (temp != null)
                {
                    List<string> connectedLines = temp.Split(',').ToList();
                    if (rhinoObject.Id != selectedRhinoObject.Id && !(connectedLines.Contains(rhinoObject.Name)))
                    {
                        var attributes = rhinoObject.Attributes;
                        attributes.Visible = false;
                        rhinoObject.CommitChanges();
                    }
                }
                else if (rhinoObject.Id != selectedRhinoObject.Id)
                {
                    var attributes = rhinoObject.Attributes;
                    attributes.Visible = false;
                    rhinoObject.CommitChanges();
                }

            }
            doc.Views.Redraw();

            // ---
            return Result.Success;
        }
    }
}
