using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Data;
using Miniscript;
using Miniscript.Unity3DDataSystem;
using SharpConfig;

namespace Miniscript.Unity3DDataSystem
{
    public static class ObjectFactorySingleton
    {
        /// <summary>
        /// The data types defined by scripts, associated with their names
        /// </summary>
        static Dictionary<string, IObjectWarehouse> _types;
        static ObjectWarehouseManifest _manifest;

        public static string folderPath;

        public static bool Contains(string typename)
        {
            if (_types.ContainsKey(typename)) return true;
            return false;
        }

        public static void Create(string tname, IObjectWarehouse store)
        {
            if (_types.ContainsKey(tname))
            {
                MiniScriptSingleton.LogError("Attempt to create duplicate Types from script (type='" + tname + "'");
                return;
            }
            else
            {
                _types.Add(tname, store);
                _types[tname].Initialize(new ValString(tname), new ValMap());
                if (store is ObjectWarehouse_Raw) { _manifest.Add(tname, "memory"); }
                else if (store is ObjectWarehouse_DataSet) { _manifest.Add(tname, "dataset"); }

#if DEBUG_MUDS
                Debug.Log("Created object warehouse of type {" + tname + "}");
#endif
            }
        }

        public static ValList TypeList()
        {
            ValList tmp = new ValList();
            foreach (KeyValuePair<string, IObjectWarehouse> kv in _types)
            {
                tmp.values.Add(new ValString(kv.Key));
            }

            return tmp;
        }

        public static void Remove(string tname)
        {
            if (_types.ContainsKey(tname))
            {
                _types.Remove(tname);
                _manifest.Remove(tname);
#if DEBUG_MUDS
                Debug.Log("Removed object warehouse of type {" + tname + "}");
#endif
            }
        }

        public static IObjectWarehouse Get(string tname)
        {
            if (_types.ContainsKey(tname))
            {
                return _types[tname];
            }

            return null;
        }

        public static bool LoadDataStore(string name)
        {
            if (!_manifest.Contains(name))
            {
                MiniScriptSingleton.LogError("LoadDataStore failed to find the specified store label {" + name + "}");
                return false;
            }

            _types[name].ReadFromFile(_manifest.GetPath(name));
            return true;
        }
        public static bool SaveDataStore(string name)
        {
            if (!_manifest.Contains(name))
            {
                MiniScriptSingleton.LogError("SaveDataStore failed to find the specified store label {" + name + "}");
                return false;
            }

            _types[name].WriteToFile(_manifest.GetPath(name));
            return true;
        }

        public static void LoadMod()
        {
            _manifest.ReadManifest();
        }

        public static void SaveAutosave()
        {
            if (_manifest.Contains("Autosave"))
            {
                string path = _manifest.GetPath("Autosave");
                DoSaveAction("Autosave", path);
            }
            else if (!_manifest.Contains("Autosave"))
            {
                _manifest.Add("Autosave", "state");
                DoSaveAction("Autosave", _manifest.GetPath("Autosave"));
            }
        }

        public static void SaveState(string label)
        {
            if (_manifest.Contains(label))
            {
                string path = _manifest.GetPath(label);
                DoSaveAction(label, path);
            }
            else if (!_manifest.Contains(label))
            {
                _manifest.Add(label, "state");
                DoSaveAction(label, _manifest.GetPath(label));
            }
        }

        public static void LoadAutosave()
        {
            if (_manifest.Contains("Autosave"))
            {
                string path = _manifest.GetPath("Autosave");
                DoLoadAction(path);
            }
        }

        public static void LoadState(string label)
        {
            if (_manifest.Contains(label))
            {
                string path = _manifest.GetPath(label);
                DoLoadAction(path);
            }
        }

        static void DoSaveAction(string label, string path)
        {
            var set = new DataSet();
            if (path.EndsWith(".tmp"))
            {   //its an autosave
                set.DataSetName = "Autosave";
            }
            else if (path.EndsWith(".save"))
            {   //its a regular saved game
                set.DataSetName = label;
            }

            DataTable dt = new DataTable("Meta");
            DataColumn dc = new DataColumn("Label");
            dt.Columns.Add(dc); dc.DataType = typeof(string);
            dc.DefaultValue = label; set.Tables.Add(dt); set.AcceptChanges();
            var z = set.Tables[0].NewRow();
            z["Label"] = label;
            set.Tables[0].Rows.Add(z);

            dt = new DataTable("WarehouseTypes");
            dc = new DataColumn("Label");
            dt.Columns.Add(dc); dc.DataType = typeof(string);
            dc = new DataColumn("Type");
            dt.Columns.Add(dc); dc.DataType = typeof(string);
            dc = new DataColumn("ID");
            dt.Columns.Add(dc); dc.DataType = typeof(string);

            set.Tables.Add(dt); set.AcceptChanges();

            foreach (KeyValuePair<string, IObjectWarehouse> kv in _types)
            {
                if (kv.Value is ObjectWarehouse_DataSet)
                {
                    z = set.Tables["WarehouseTypes"].NewRow();
                    z["Label"] = kv.Key;
                    z["Type"] = "dataset";
                    z["ID"] = Guid.NewGuid().ToString();
                    set.Tables["WarehouseTypes"].Rows.Add(z);
                }
                else if (kv.Value is ObjectWarehouse_Raw)
                {
                    z = set.Tables["WarehouseTypes"].NewRow();
                    z["Label"] = kv.Key;
                    z["Type"] = "memory";
                    z["ID"] = Guid.NewGuid().ToString();
                    set.Tables["WarehouseTypes"].Rows.Add(z);
                }

                var map = kv.Value.GetMSType();

                dt = new DataTable(kv.Key);
                foreach (KeyValuePair<Value, Value> mapkv in map.map)
                {
                    dc = new DataColumn(mapkv.Key.ToString());
                    if (mapkv.Value is ValString)
                    {
                        dc.DataType = typeof(string);
                    }
                    else if (mapkv.Value is ValNumber)
                    {
                        dc.DataType = typeof(double);
                    }
                    dt.Columns.Add(dc);
                }
                dt.AcceptChanges();
                set.Tables.Add(dt); set.AcceptChanges();

                var lst = kv.Value.GetInstances();
                foreach (Value v in lst.values)
                {
                    ValMap row = (ValMap)v;
                    var dtrow = dt.NewRow();
                    foreach (KeyValuePair<Value, Value> rowkv in row.map)
                    {
                        dtrow[rowkv.Key.ToString()] = rowkv.Value;
                    }
                    dt.Rows.Add(dtrow);
                }
            }

            set.WriteXml(path, XmlWriteMode.WriteSchema);
        }

        static void DoLoadAction(string path)
        {
            var set = new DataSet();
            set.ReadXml(path, XmlReadMode.ReadSchema);

            foreach (DataRow frow in set.Tables["WarehouseTypes"].Rows)
            {
                if (frow["Type"].ToString().Equals("memory"))
                {
                    #region
                    var wh1 = new ObjectWarehouse_Raw();
                    var table = set.Tables[frow["Label"].ToString()];
                    lock (wh1._locker)
                    {
                        wh1._name = frow["Label"].ToString();
                        wh1._type = wh1.ValMapFromDataTable(ref table);
                        foreach (DataRow dr in set.Tables[frow["Label"].ToString()].Rows)
                        {
                            var tmp = wh1._type.Clone();
                            foreach(DataColumn dc in table.Columns)
                            {
                                if (dc.DataType == typeof(string))
                                {
                                    tmp.map[new ValString(dc.ColumnName)] = new ValString(dr[dc.ColumnName].ToString());
                                }
                                else if (dc.DataType == typeof(double))
                                {
                                    tmp.map[new ValString(dc.ColumnName)] = new ValNumber((double)dr[dc.ColumnName]);
                                }
                            }
                            wh1._instanced.Add(tmp);
                        }
                    }
                    _types.Add(frow["Label"].ToString(), wh1);
                    #endregion
                }
                else if (frow["Type"].ToString().Equals("dataset"))
                {
                    #region
                    var wh2 = new ObjectWarehouse_DataSet();
                    lock (wh2._locker)
                    {
                        wh2._set.Tables.Add(set.Tables[frow["Label"].ToString()]);
                        wh2._set.AcceptChanges();
                    }
                    _types.Add(frow["Label"].ToString(), wh2);
                    #endregion
                }
            }
        }

        static ObjectFactorySingleton()
        {
            _types = null;
            _types = new Dictionary<string, IObjectWarehouse>();
            _manifest = new ObjectWarehouseManifest();
        }
    }
}

