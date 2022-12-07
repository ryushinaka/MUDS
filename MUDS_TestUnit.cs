using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Miniscript.Unity3DDataSystem
{
    public class MUDS_TestUnit : MonoBehaviour
    {
        
        void Start()
        {
            //register all the intrinsic methods for MUDS
            MUDSIntrinsics.Initialize();
            //set the working directory for the data store functionality, this happens to be my local path
            //assign this string the value it should have for your task.
            ObjectFactorySingleton.LoadMod(@"C:/Mods/caf4efb5-2661-4c90-9250-b318e2188422/data/");

            //now lets create a tutorial script in Miniscript syntax to show the Intrinsic API
            string source = "CreateDataStore(\"NPCs\", \"memory\")";
            source += System.Environment.NewLine + "CreateDataStore(\"Characters\", \"memory\") ";
            //add 2 properties to the object (NPCs) in the datastore
            source += System.Environment.NewLine + "AddAttribute(\"NPCs\", \"attributeName\", \"string\")";
            source += System.Environment.NewLine + "AddAttribute(\"NPCs\", \"attributeAge\", \"number\")";
            //add 2 properties to the object (Characters) in the datastore
            source += System.Environment.NewLine + "AddAttribute(\"Characters\", \"attributeName\", \"string\")";
            source += System.Environment.NewLine + "AddAttribute(\"Characters\", \"attributeAge\", \"number\")";
            //remove the Name property from the 'NPCs' data store, this has an immediate and permanent change
            source += System.Environment.NewLine + "RemoveAttribute(\"NPCs\", \"attributeAge\")";            
            //get the ValList of the data stores in memory currently
            source += System.Environment.NewLine + "GetTypeStoreList";
            //save a data store to the file system
            source += System.Environment.NewLine + "SaveDataStore(\"NPCs\")";
            //removes the data store from memory, this does *not* implicitly save the data store before its removal
            //if the data store is not saved prior to using this Intrinsic, it will be forever lost to null memory.
            source += System.Environment.NewLine + "UnloadDataStore(\"NPCs\")";            
            //load the data store from the file system
            source += System.Environment.NewLine + "LoadDataStore(\"NPCs\")";            
            //saves all data stores into a "save state" with the label 'NPCs'
            //You can choose to think of "save state" as a saved game, assuming all of your data is stored within
            source += System.Environment.NewLine + "SaveState(\"NPCs\")";
            //loads all data stores from a "save state" with the label 'NPCs'
            source += System.Environment.NewLine + "LoadState(\"NPCs\")";
            //remove the 'NPCs' data store from the file system
            source += System.Environment.NewLine + "RemoveDataStore(\"NPCs\")";
            //Gets the list of "states" that have been previously saved.
            source += System.Environment.NewLine + "GetStates()";
            //functionally the same as Save/LoadState, but there is only ever one Autosave
            //this is useful for a QuickLoad/QuickSave feature implementation
            source += System.Environment.NewLine + "CreateAutosave()";
            //creates a single variable/record of the data type defined by the 'Characters' data store
            //The instance created is already part of the collection before it is returned by the Intrinsic result.
            source += System.Environment.NewLine + "CreateInstance(\"Characters\")";
            //creates 10 instances of the data type 'Characters', returned as a ValList containing ValMap's
            //all 10 instances in this example will persist until the Intrinsic RemoveInstance() is called.
            source += System.Environment.NewLine + "CreateInstances(\"Characters\", 10)";
            //returns a ValNumber containing the quantity of instances that exist in the specified Data Store (Characters)
            source += System.Environment.NewLine + "InstanceQuantity(\"Characters\")";
            //returns true/false if a data store(Characters) has the specified property(attributeName)
            source += System.Environment.NewLine + "HasAttribute(\"Characters\", \"attributeName\")";
            //this returns a random instance from the Characters data store
            source += System.Environment.NewLine + "GetRandomInstance(\"Characters\")";
            //this returns 2 random instances from the Characters data store as a ValList, and allows duplicates
            source += System.Environment.NewLine + "GetRandomInstances(\"Characters\", 2, false)";
            //this returns 2 random instances from the Characters data store as a ValList, and does not allow duplicates
            source += System.Environment.NewLine + "GetRandomInstances(\"Characters\", 2, true)";



            Interpreter p = new Interpreter(source);            
            p.standardOutput = new TextOutputMethod(StdOutput);
            p.errorOutput = new TextOutputMethod(ErrOutput);
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            p.RunUntilDone();
            watch.Stop();

            Debug.Log("Time to execute script with Intrinsics = " + watch.ElapsedTicks + " ticks / " + watch.ElapsedMilliseconds + " milliseconds.");
        }

        
        void Update()
        {

        }

        void StdOutput(string msg)
        {
            Debug.Log("StdOutput: " + msg);
        }
        void ErrOutput(string msg)
        {
            Debug.Log("ErrOutput: " + msg);
        }
    }
}

