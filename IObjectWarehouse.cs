using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Miniscript;
using System;

namespace Miniscript.Unity3DDataSystem
{
    public interface IObjectWarehouse
    {
        void Initialize(ValString name, ValMap map);

        ValMap GetMSType();
        ValMap GetInstance(ValString id);
        ValMap GetInstance(Guid id);
        ValMap GetRandomInstance();
        void DestroyInstance(ValString id);
        void DestroyInstance(Guid id);

        ValList GetInstances();
        ValList GetRandomInstances(ValNumber quantity, ValNumber unique);

        ValMap CreateInstance();
        ValList CreateInstances(ValNumber quantity);

        int InstanceCount { get; }

        bool hasAttribute(ValString name);
        bool AddAttribute(ValString aname, Value value);
        bool RemoveAttribute(ValString aname);

        bool ValueAssignChecker(Value key, Value value);
        bool ValueAssignChecker2(ValMap parent, Value key, Value value);

        ValList Select(ValString aname, ValString value);
        ValList SelectRegx(ValString aname, ValString pattern);
        ValList Select(ValString aname, ValNumber value);
        ValList Select(ValString aname, ValNumber lower, ValNumber upper);

        void WriteToFile(string path);
        void ReadFromFile(string path);
    }
}

