using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Data;
using Miniscript;

namespace Miniscript.Unity3DDataSystem
{
    public class ObjectWarehouse_DataSet : IObjectWarehouse
    {
        internal object _locker = new object();
        internal DataSet _set = new DataSet();

        ValMap IObjectWarehouse.CreateInstance()
        {
            ValMap newTmp = new ValMap();

            DataRow dr = _set.Tables[0].NewRow();
            dr["__ID__"] = Guid.NewGuid().ToString();

            lock (_locker)
            {
                _set.Tables[0].Rows.Add(dr);
            }

            foreach (DataColumn dc in _set.Tables[0].Columns)
            {
                if (dc.DataType == typeof(string))
                {
                    newTmp.map.Add(new ValString(dc.ColumnName), new ValString(dc.DefaultValue.ToString()));
                }
                else if (dc.DataType == typeof(double))
                {
                    newTmp.map.Add(new ValString(dc.ColumnName), new ValNumber((double)dc.DefaultValue));
                }
            }

            newTmp.assignOverride2 = new ValMap.AssignOverrideFunc2(((IObjectWarehouse)this).ValueAssignChecker2);
            newTmp["__ID__"] = new ValString(dr["__ID__"].ToString());

            return newTmp;
        }

        ValList IObjectWarehouse.CreateInstances(ValNumber quantity)
        {
            ValList rst = new ValList();
            int i = 0;
            while (i < quantity.IntValue())
            {
                rst.values.Add(((IObjectWarehouse)this).CreateInstance());
                i++;
            }
            return rst;
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
                for (int i = 0; i < _set.Tables[0].Rows.Count; i++)
                {
                    tmp = Guid.Parse(_set.Tables[0].Rows[i]["__ID__"].ToString());
                    if (tmp.Equals(id))
                    {
                        _set.Tables[0].Rows.RemoveAt(i);
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
                for (int i = 0; i < _set.Tables[0].Rows.Count; i++)
                {
                    tmp = Guid.Parse(_set.Tables[0].Rows[i]["__ID__"].ToString());
                    if (tmp.Equals(id))
                    {
                        //rst.map.Add(new ValString("__ID__"), new ValString(_set.Tables[0].Rows[i]["__ID__"].ToString()));
                        foreach (DataColumn dc in _set.Tables[0].Columns)
                        {
                            if (dc.DataType == typeof(string))
                            {
                                rst.map.Add(new ValString(dc.ColumnName),
                                    new ValString(_set.Tables[0].Rows[i][dc].ToString()));
                            }
                            else if (dc.DataType == typeof(double))
                            {
                                rst.map.Add(new ValString(dc.ColumnName),
                                    new ValNumber(double.Parse(_set.Tables[0].Rows[i][dc].ToString())));
                            }
                        }

                        break;
                    }
                }
            }

            return rst;
        }

        ValMap IObjectWarehouse.GetRandomInstance()
        {
            System.Random rnd = new System.Random();
            var x = rnd.Next(0, _set.Tables[0].Rows.Count + 1);
            return DataRowToValMap(_set.Tables[0].Rows[x]);
        }

        ValList IObjectWarehouse.GetInstances()
        {
            ValList tmp = new ValList();
            lock (_locker)
            {
                foreach (DataRow dr in _set.Tables[0].Rows)
                {
                    ValMap row = new ValMap();
                    foreach (DataColumn dc in _set.Tables[0].Columns)
                    {
                        if (dc.DataType == typeof(string))
                        {
                            row.map.Add(new ValString(dc.ColumnName),
                                new ValString(dr[dc].ToString()));
                        }
                        else if (dc.DataType == typeof(double))
                        {
                            row.map.Add(new ValString(dc.ColumnName),
                                new ValNumber(double.Parse(dr[dc].ToString())));
                        }
                    }
                    tmp.values.Add(row);
                }
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
            ValMap rst = new ValMap();
            foreach (DataColumn dc in _set.Tables[0].Columns)
            {
                if (dc.DataType == typeof(string))
                {
                    rst.map.Add(new ValString(dc.ColumnName), new ValString(string.Empty));
                }
                else if (dc.DataType == typeof(double))
                {
                    rst.map.Add(new ValString(dc.ColumnName), new ValNumber(0));
                }
            }
            return rst;
        }

        void IObjectWarehouse.Initialize(ValString name, ValMap map)
        {
            if (_set.DataSetName == "NewDataSet")
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

                _set.DataSetName = name.value;
                _set.Tables.Clear(); _set.AcceptChanges();

                //add the unique identifier for this object 'type'
                var dt = _set.Tables.Add(name.value);
                _set.AcceptChanges();
                _set.Tables[0].Columns.Add("__ID__", typeof(string));

                foreach (KeyValuePair<Value, Value> kv in map.map)
                {
                    if (kv.Value is ValString)
                    {
                        _set.Tables[0].Columns.Add(kv.Key.ToString(), typeof(string));
                    }
                    else if (kv.Value is ValNumber)
                    {
                        _set.Tables[0].Columns.Add(kv.Key.ToString(), typeof(double));
                    }
                }
            }
            else
            {
                MiniScriptSingleton.LogError("Attempt to assign a new Type to an ObjectWarehouse after its Type has already been assigned." +
             System.Environment.NewLine + "assigned:(" + _set.DataSetName + ") vs new:(" + name + ")");
            }
        }

        int IObjectWarehouse.InstanceCount
        {
            get
            {
                if (_set.Tables.Count == 0) return 0;
                return _set.Tables[0].Rows.Count;
            }
        }

        bool IObjectWarehouse.hasAttribute(ValString name)
        {
            if (((IObjectWarehouse)this).GetMSType().ContainsKey(name.value)) return true;
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
            if (((IObjectWarehouse)this).GetMSType().ContainsKey(aname))
            {
                MiniScriptSingleton.LogError("The attribute('" + aname + "') already exists for the type(" + _set.DataSetName + ")");
                return false;
            }

            if (!((IObjectWarehouse)this).GetMSType().ContainsKey(aname))
            {
                var d = new DataColumn(aname.value);
                //assign the default value to the column
                if (value is ValString) { d.DefaultValue = value.ToString(); }
                else if (value is ValNumber) { d.DefaultValue = double.Parse(value.ToString()); }
                //commit change to the dataset
                _set.Tables[0].Columns.Add(d);
                _set.Tables[0].AcceptChanges();

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
            if (!_set.Tables[0].Columns.Contains(aname.value))
            {
                MiniScriptSingleton.LogError("The attribute('" + aname + "') does not exist for the type(" + _set.DataSetName + ")");
                return false;
            }

            if (_set.Tables[0].Columns.Contains(aname.value))
            {
                _set.Tables[0].Columns.Remove(aname.value);
                _set.Tables[0].AcceptChanges();
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
            //MiniScriptSingleton.ValueCheck(parent, key, value); 
            //this executes the interpreter running the script for validation of this particular 'type'
            return true;
        }

        ValList IObjectWarehouse.Select(ValString aname, ValString value)
        {
            if (!_set.Tables[0].Columns.Contains(aname.value))
            {
                MiniScriptSingleton.LogError("ObjectWarehouse.Select: The type('" + _set.DataSetName + "') does not contain the attribute '" + aname + "'");
                return null;
            }
            if (string.IsNullOrEmpty(value.value) || string.IsNullOrWhiteSpace(value.value))
            {
                MiniScriptSingleton.LogError("ObjectWarehouse.Select: The 'value' parameter is invalid.");
                return null;
            }

            ValList rst = new ValList();
            var dr = _set.Tables[0].Select(aname.value + " = '" + value + "'");
            foreach (DataRow sr in dr)
            {
                rst.values.Add(DataRowToValMap(sr));
            }

            return rst;
        }
        ValList IObjectWarehouse.SelectRegx(ValString aname, ValString pattern)
        {
            if (!_set.Tables[0].Columns.Contains(aname.value))
            {
                MiniScriptSingleton.LogError("ObjectWarehouse.Select: The type('" + _set.DataSetName + "') does not contain the attribute '" + aname + "'");
                return null;
            }
            if (string.IsNullOrEmpty(pattern.value) || string.IsNullOrWhiteSpace(pattern.value))
            {
                MiniScriptSingleton.LogError("ObjectWarehouse.Select: The 'value' parameter is invalid.");
                return null;
            }

            ValList rst = new ValList();
            foreach (DataRow dr in _set.Tables[0].Rows)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch
                    (dr[aname.value].ToString(), pattern.value))
                {
                    rst.values.Add(DataRowToValMap(dr));
                }
            }
            return rst;
        }
        ValList IObjectWarehouse.Select(ValString aname, ValNumber value)
        {
            if (!_set.Tables[0].Columns.Contains(aname.value))
            {
                MiniScriptSingleton.LogError("ObjectWarehouse.Select: The type('" + _set.DataSetName + "') does not contain the attribute '" + aname + "'");
                return null;
            }
            if (_set.Tables[0].Columns[aname.value].DataType != typeof(double))
            {
                MiniScriptSingleton.LogError("ObjectWarehouse.Select: The attribute '" + aname + "' is not a ValNumber.");
                return null;
            }

            ValList rst = new ValList();

            foreach (DataRow dr in _set.Tables[0].Rows)
            {
                if (dr[aname.value] == value) { rst.values.Add(DataRowToValMap(dr)); }
            }

            return rst;
        }
        ValList IObjectWarehouse.Select(ValString aname, ValNumber lower, ValNumber upper)
        {
            if (!_set.Tables[0].Columns.Contains(aname.value))
            {
                MiniScriptSingleton.LogError("ObjectWarehouse.Select: The type('" + _set.DataSetName + "') does not contain the attribute '" + aname + "'");
                return null;
            }
            if (_set.Tables[0].Columns[aname.value].DataType != typeof(double))
            {
                MiniScriptSingleton.LogError("ObjectWarehouse.Select: The attribute '" + aname + "' is not a ValNumber.");
                return null;
            }

            ValList rst = new ValList();
            ValNumber tmp = null;
            foreach (DataRow dr in _set.Tables[0].Rows)
            {
                tmp = (ValNumber)dr[aname.value];
                if (tmp.value >= lower.value && tmp.value <= upper.value)
                {
                    rst.values.Add(DataRowToValMap(dr));
                }
            }

            return rst;
        }

        ValMap DataRowToValMap(DataRow dr)
        {
            ValMap rst = new ValMap();
            foreach (DataColumn dc in _set.Tables[0].Columns)
            {
                if (dc.DataType == typeof(string))
                {
                    rst.map.Add(new ValString(dc.ColumnName), new ValString(dr[dc].ToString()));
                }
                else if (dc.DataType == typeof(double))
                {
                    rst.map.Add(new ValString(dc.ColumnName), new ValNumber(double.Parse(dr[dc].ToString())));
                }
            }
            return rst;
        }

        void IObjectWarehouse.WriteToFile(string path)
        {
            _set.WriteXml(path, XmlWriteMode.WriteSchema);
        }
        void IObjectWarehouse.ReadFromFile(string path)
        {
            _set.ReadXml(path, XmlReadMode.ReadSchema);
        }
    }
}

