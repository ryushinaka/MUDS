using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Miniscript;
using System;
using System.Data;

namespace Miniscript.Unity3DDataSystem
{
    public class ObjectWarehouse_Raw : IObjectWarehouse
    {
        internal string _name = string.Empty;
        internal ValMap _type = null;
        internal List<ValMap> _instanced = new List<ValMap>();
        internal object _locker = new object();

        ValMap IObjectWarehouse.CreateInstance()
        {
            ValMap newTmp = _type.Clone();
            newTmp.map.Add(new ValString("__ID__"), new ValString(Guid.NewGuid().ToString()));
            newTmp.assignOverride = new ValMap.AssignOverrideFunc(((IObjectWarehouse)this).ValueAssignChecker);
            lock (_locker)
            {
                _instanced.Add(newTmp);
            }
            return newTmp;
        }

        ValList IObjectWarehouse.CreateInstances(ValNumber quantity)
        {
            return null;
        }

        void IObjectWarehouse.DestroyInstance(ValString id)
        {
            ((IObjectWarehouse)this).DestroyInstance(Guid.Parse(id.value));
        }

        void IObjectWarehouse.DestroyInstance(Guid id)
        {
            Guid tmp;
            lock (_locker)
            {
                for (int i = 0; i < _instanced.Count; i++)
                {
                    tmp = Guid.Parse(_instanced[i]["__ID__"].ToString());
                    if (tmp.Equals(id))
                    {
                        _instanced.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        ValMap IObjectWarehouse.GetInstance(ValString id)
        {
            return ((IObjectWarehouse)this).GetInstance(Guid.Parse(id.value));
        }

        ValMap IObjectWarehouse.GetInstance(Guid id)
        {
            Guid tmp;
            ValMap rst = null;
            lock (_locker)
            {
                for (int i = 0; i < _instanced.Count; i++)
                {
                    tmp = Guid.Parse(_instanced[i]["__ID__"].ToString());
                    if (tmp.Equals(id))
                    {
                        rst = _instanced[i].Clone();
                        break;
                    }
                }
            }

            return rst;
        }

        ValMap IObjectWarehouse.GetRandomInstance()
        {
            System.Random rnd = new System.Random();
            var x = rnd.Next(0, _instanced.Count + 1);
            return _instanced[x];
        }

        ValList IObjectWarehouse.GetInstances()
        {
            ValList tmp = new ValList();
            lock (_locker)
            {
                tmp.values.AddRange(_instanced);
            }
            return tmp;
        }

        ValList IObjectWarehouse.GetRandomInstances(ValNumber quantity, ValNumber unique)
        {
            if (unique.BoolValue() == true)
            {
                ValList lst = new ValList();
                List<Guid> ids = new List<Guid>();
                int counter = quantity.IntValue();
                while (counter > 0)
                {
                    var tmp = ((IObjectWarehouse)this).GetRandomInstance();
                    Guid b = Guid.Parse(tmp["__ID__"].ToString());
                    if (!ids.Contains(b))
                    {
                        ids.Add(b);
                        lst.values.Add(tmp);
                    }

                    counter--;
                }
                return lst;
            }
            else
            {
                ValList lst = new ValList();
                int counter = quantity.IntValue();
                while (counter > 0)
                {
                    lst.values.Add(((IObjectWarehouse)this).GetRandomInstance());
                    counter--;
                }
                return lst;
            }
        }

        ValMap IObjectWarehouse.GetMSType()
        {
            return _type.Clone();
        }

        void IObjectWarehouse.Initialize(ValString name, ValMap map)
        {
            if (_type == null)
            {
                if (map.ContainsKey("__ID__"))
                {
                    MiniScriptSingleton.LogError("ObjectWarehouse.Initialize: ValMap given already contains '__ID__'");
                    return;
                }
                if (string.IsNullOrEmpty(name.value) || string.IsNullOrWhiteSpace(name.value))
                {
                    MiniScriptSingleton.LogError("ObjectWarehouse.Initialize: 'name' attribute given is null or empty/whitespace.");
                    return;
                }
                //add the unique identifier for this object 'type'
                map.map.Add(new ValString("__ID__"), new ValString(Guid.NewGuid().ToString()));
                _name = name.value;
                _type = map;
            }
            else
            {
                MiniScriptSingleton.LogError("Attempt to assign a new Type to an ObjectWarehouse after its Type has already been assigned." +
             System.Environment.NewLine + "assigned:(" + _name + ") vs new:(" + name + ")");
            }
        }

        int IObjectWarehouse.InstanceCount { get { return _instanced.Count; } }

        bool IObjectWarehouse.hasAttribute(ValString name)
        {
            if (_type.ContainsKey(name.value)) return true;
            return false;
        }

        bool IObjectWarehouse.AddAttribute(ValString aname, Value value)
        {
            if (aname.value == "__ID__")
            {
                MiniScriptSingleton.LogError("The attribute '__ID__' is a reserved keyword, use a different name!");
                return false;
            }
            if (string.IsNullOrEmpty(aname.value) || string.IsNullOrWhiteSpace(aname.value))
            {
                MiniScriptSingleton.LogError("The attribute name value is empty, a valid name must be given.");
                return false;
            }
            if (_type.ContainsKey(aname))
            {
                MiniScriptSingleton.LogError("The attribute('" + aname + "') already exists for the type(" + _name + ")");
                return false;
            }

            if (!_type.ContainsKey(aname))
            {
                _type.map.Add(new ValString(aname.value), value);
                //modify any existing instances of the 'type'
                if (_instanced.Count > 0)
                {
                    lock (_locker)
                    {
                        foreach (ValMap map in _instanced)
                        {
                            map.map.Add(aname, value);
                        }
                    }
                }

                return true;
            }

            return false;
        }

        bool IObjectWarehouse.RemoveAttribute(ValString aname)
        {
            if (aname.value == "__ID__")
            {
                MiniScriptSingleton.LogError("The attribute '__ID__' is a reserved keyword and can not be removed!");
                return false;
            }
            if (string.IsNullOrEmpty(aname.value) || string.IsNullOrWhiteSpace(aname.value))
            {
                MiniScriptSingleton.LogError("The attribute name value is empty, a valid name must be given.");
                return false;
            }
            if (!_type.ContainsKey(aname))
            {
                MiniScriptSingleton.LogError("The attribute('" + aname + "') does not exist for the type(" + _name + ")");
                return false;
            }

            if (_type.ContainsKey(aname))
            {
                _type.map.Remove(aname);
                //modify any existing instances of the 'type'
                if (_instanced.Count > 0)
                {
                    lock (_locker)
                    {
                        foreach (ValMap map in _instanced)
                        {
                            map.map.Remove(aname);
                        }
                    }
                }

                return true;
            }

            return false;
        }

        bool IObjectWarehouse.ValueAssignChecker(Value key, Value value)
        {
            //internally, we can validate our data graph in whatever manner we choose
            //But for my purposes, this will also call out to a MiniScript script
            //MiniScriptSingleton.ValueCheck(_name, key, value); 
            //this executes the interpreter running the script for validation of this particular 'type'
            return true;
        }

        bool IObjectWarehouse.ValueAssignChecker2(ValMap parent, Value key, Value value)
        {
            //internally, we can validate our data graph in whatever manner we choose
            //But for my purposes, this will also call out to a MiniScript script
            //MiniScriptSingleton.ValueCheck(_name, key, value); 
            //this executes the interpreter running the script for validation of this particular 'type'
            return true;
        }

        ValList IObjectWarehouse.Select(ValString aname, ValString value)
        {
            if (_type == null) { return null; }
            if (_instanced.Count == 0) { return null; }
            if (!_type.ContainsKey(aname))
            {
                MiniScriptSingleton.LogError("ObjectWarehouse.Select: The type('" + _name + "') does not contain the attribute '" + aname + "'");
                return null;
            }
            if (string.IsNullOrEmpty(value.value) || string.IsNullOrWhiteSpace(value.value))
            {
                MiniScriptSingleton.LogError("ObjectWarehouse.Select: The 'value' parameter is invalid.");
                return null;
            }

            ValList rst = new ValList();
            for (int i = 0; i < _instanced.Count; i++)
            {
                if (_instanced[i][aname.value].ToString().Equals(value))
                {
                    rst.values.Add(_instanced[i].Clone());
                }
            }
            return rst;
        }
        ValList IObjectWarehouse.SelectRegx(ValString aname, ValString pattern)
        {
            if (_type == null) { return null; }
            if (_instanced.Count == 0) { return null; }

            ValList rst = new ValList();
            for (int i = 0; i < _instanced.Count; i++)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch
                    (_instanced[i][aname.value].ToString(), pattern.value))
                {
                    rst.values.Add(_instanced[i].Clone());
                }
            }
            return rst;
        }
        ValList IObjectWarehouse.Select(ValString aname, ValNumber value)
        {
            if (_type == null) { return null; }
            if (_instanced.Count == 0) { return null; }

            if (!_type.ContainsKey(aname))
            {
                MiniScriptSingleton.LogError("ObjectWarehouse.Select: The type('" + _name + "') does not contain the attribute '" + aname + "'");
                return null;
            }
            if (!(_type[aname.value] is ValNumber))
            {
                MiniScriptSingleton.LogError("ObjectWarehouse.Select: The attribute '" + aname + "' is not a ValNumber.");
                return null;
            }

            ValList rst = new ValList();

            for (int i = 0; i < _instanced.Count; i++)
            {
                if (_instanced[i][aname.value] == value) { rst.values.Add(_instanced[i].Clone()); }
            }

            return rst;
        }
        ValList IObjectWarehouse.Select(ValString aname, ValNumber lower, ValNumber upper)
        {
            if (_type == null) { return null; }
            if (_instanced.Count == 0) { return null; }

            if (!_type.ContainsKey(aname))
            {
                MiniScriptSingleton.LogError("ObjectWarehouse.Select: The type('" + _name + "') does not contain the attribute '" + aname + "'");
                return null;
            }
            if (!(_type[aname.value] is ValNumber))
            {
                MiniScriptSingleton.LogError("ObjectWarehouse.Select: The attribute '" + aname + "' is not a ValNumber.");
                return null;
            }

            ValList rst = new ValList();
            ValNumber tmp = null;
            for (int i = 0; i < _instanced.Count; i++)
            {
                tmp = (ValNumber)_instanced[i][aname.value];
                if (tmp.value >= lower.value && tmp.value <= upper.value) { rst.values.Add(_instanced[i].Clone()); }
            }

            return rst;
        }

        void IObjectWarehouse.WriteToFile(string path)
        {
            var set = new DataSet(_name);
            var dt = new DataTable(_name);
            foreach(KeyValuePair<Value,Value>kv in _type.map)
            {
                var dc = new DataColumn(kv.Key.ToString());
                if (kv.Value is ValString)
                {
                    dc.DataType = typeof(string);
                }
                else if (kv.Value is ValNumber)
                {
                    dc.DataType = typeof(double);
                }
                dt.Columns.Add(dc);
            }
            dt.AcceptChanges(); set.Tables.Add(dt); set.AcceptChanges();
            foreach (Value v in _instanced)
            {
                ValMap row = (ValMap)v;
                var dtrow = dt.NewRow();
                foreach (KeyValuePair<Value, Value> rowkv in row.map)
                {
                    dtrow[rowkv.Key.ToString()] = rowkv.Value;
                }
                dt.Rows.Add(dtrow);
            }
            set.WriteXml(path, XmlWriteMode.WriteSchema);
        }
        void IObjectWarehouse.ReadFromFile(string path)
        {
            var set = new DataSet();
            set.ReadXml(path, XmlReadMode.ReadSchema);
            var dt = set.Tables[0];
            foreach(DataColumn dc in dt.Columns)
            {
                if (dc.DataType == typeof(string))
                {
                    _type.map.Add(new ValString(dc.ColumnName), new ValString(string.Empty));
                }
                else if (dc.DataType == typeof(double))
                {
                    _type.map.Add(new ValString(dc.ColumnName), new ValNumber(0));
                }
            }
            foreach(DataRow dr in dt.Rows)
            {
                var mrow = _type.Clone();
                foreach(DataColumn dc in dt.Columns)
                {
                    if (dc.DataType == typeof(string))
                    {
                        mrow[dc.ColumnName] = new ValString(dr[dc.ColumnName].ToString());
                    }
                    else if (dc.DataType == typeof(double))
                    {
                        mrow[dc.ColumnName] = new ValNumber((double)dr[dc.ColumnName]);
                    }
                }
                _instanced.Add(mrow);
            }

        }

        internal ValMap ValMapFromDataTable(ref DataTable table)
        {
            ValMap rst = new ValMap();
            foreach (DataColumn dc in table.Columns)
            {
                if (dc.DataType == typeof(string))
                {
                    rst.map.Add(new ValString(dc.ColumnName), new ValString((string)dc.DefaultValue));
                }
                else if (dc.DataType == typeof(double))
                {
                    rst.map.Add(new ValString(dc.ColumnName), new ValNumber((double)dc.DefaultValue));
                }
            }

            return rst;
        }
    }
}

