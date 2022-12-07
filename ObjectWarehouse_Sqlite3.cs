using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Miniscript;
using Miniscript.SQLite;

namespace Miniscript.Unity3DDataSystem
{
    public class ObjectWarehouse_Sqlite3 : IObjectWarehouse
    {
        //Miniscript.SQLite.s
        int IObjectWarehouse.InstanceCount => throw new NotImplementedException();

        bool IObjectWarehouse.AddAttribute(ValString aname, Value value)
        {
            throw new NotImplementedException();
        }

        ValMap IObjectWarehouse.CreateInstance()
        {
            throw new NotImplementedException();
        }

        ValList IObjectWarehouse.CreateInstances(ValNumber quantity)
        {
            throw new NotImplementedException();
        }

        void IObjectWarehouse.DestroyInstance(ValString id)
        {
            throw new NotImplementedException();
        }

        void IObjectWarehouse.DestroyInstance(Guid id)
        {
            throw new NotImplementedException();
        }

        ValMap IObjectWarehouse.GetInstance(ValString id)
        {
            throw new NotImplementedException();
        }

        ValMap IObjectWarehouse.GetInstance(Guid id)
        {
            throw new NotImplementedException();
        }

        ValList IObjectWarehouse.GetInstances()
        {
            throw new NotImplementedException();
        }

        ValMap IObjectWarehouse.GetMSType()
        {
            throw new NotImplementedException();
        }

        ValMap IObjectWarehouse.GetRandomInstance()
        {
            throw new NotImplementedException();
        }

        ValList IObjectWarehouse.GetRandomInstances(ValNumber quantity, ValNumber unique)
        {
            throw new NotImplementedException();
        }

        bool IObjectWarehouse.HasAttribute(ValString name)
        {
            throw new NotImplementedException();
        }

        void IObjectWarehouse.Initialize(ValString name, ValMap map)
        {
            throw new NotImplementedException();
        }

        void IObjectWarehouse.ReadFromFile(string path)
        {
            throw new NotImplementedException();
        }

        bool IObjectWarehouse.RemoveAttribute(ValString aname)
        {
            throw new NotImplementedException();
        }

        ValList IObjectWarehouse.Select(ValString aname, ValString value)
        {
            throw new NotImplementedException();
        }

        ValList IObjectWarehouse.Select(ValString aname, ValNumber value)
        {
            throw new NotImplementedException();
        }

        ValList IObjectWarehouse.Select(ValString aname, ValNumber lower, ValNumber upper)
        {
            throw new NotImplementedException();
        }

        ValList IObjectWarehouse.SelectRegx(ValString aname, ValString pattern)
        {
            throw new NotImplementedException();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        bool IObjectWarehouse.ValueAssignChecker(Value key, Value value)
        {
            throw new NotImplementedException();
        }

        bool IObjectWarehouse.ValueAssignChecker2(ValMap parent, Value key, Value value)
        {
            throw new NotImplementedException();
        }

        void IObjectWarehouse.WriteToFile(string path)
        {
            throw new NotImplementedException();
        }
    }
}


