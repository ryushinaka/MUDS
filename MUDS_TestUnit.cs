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

            string source = "CreateDataStore(\"NPCs\", \"memory\")";
            source += System.Environment.NewLine + "CreateDataStore(\"Characters\", \"memory\") ";
            //add 2 properties to the "object" (Characters) in the datastore
            source += System.Environment.NewLine + "AddAttribute(\"NPCs\", \"attributeName\", \"string\")";
            source += System.Environment.NewLine + "AddAttribute(\"NPCs\", \"attributeAge\", \"string\")";
            //now remove the Name property
            source += System.Environment.NewLine + "RemoveAttribute(\"NPCs\", \"attributeName\", \"string\")";


            Interpreter p = new Interpreter(source);
            p.standardOutput = new TextOutputMethod(StdOutput);
            p.errorOutput = new TextOutputMethod(ErrOutput);
            p.RunUntilDone();
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

