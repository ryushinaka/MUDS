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
        static Dictionary<string, System.Tuple<string, string>> _manifest;

        static string workingPath;

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
                if (store is ObjectWarehouse_Raw)
                {
                    _manifest.Add(tname, new Tuple<string, string>(Guid.NewGuid().ToString(), "memory"));
                }
                else if (store is ObjectWarehouse_DataSet)
                {
                    _manifest.Add(tname, new Tuple<string, string>(Guid.NewGuid().ToString(), "dataset"));
                }

#if DEBUG_MUDS
                Debug.Log("Created object warehouse of type {" + tname + "}");
#endif
            }

            //create the manifest of our data stores
            Configuration cfg = new Configuration();

            foreach (KeyValuePair<string, System.Tuple<string, string>> kv in _manifest)
            {
                cfg.Add(kv.Key);
                cfg[kv.Key].Add("Guid", kv.Value.Item1);
                cfg[kv.Key].Add("Type", kv.Value.Item2);
            }
            //write the manifest to file
            cfg.SaveToFile(workingPath + "manifest.txt");
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
                File.Delete(workingPath + _manifest[tname].Item1 + ".xml");
                _manifest.Remove(tname);

#if DEBUG_MUDS
                Debug.Log("Removed object warehouse of type {" + tname + "}");
#endif
                //create the manifest of our data stores
                Configuration cfg = new Configuration();

                foreach (KeyValuePair<string, System.Tuple<string, string>> kv in _manifest)
                {
                    cfg.Add(kv.Key);
                    cfg[kv.Key].Add("Guid", kv.Value.Item1);
                    cfg[kv.Key].Add("Type", kv.Value.Item2);
                }
                //write the manifest to file
                cfg.SaveToFile(workingPath + "manifest.txt");
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
            if (!_manifest.ContainsKey(name))
            {
                MiniScriptSingleton.LogError("LoadDataStore failed to find the specified store label {" + name + "}");
                return false;
            }

            string path = workingPath + _manifest[name].Item1 + ".xml";
            if (_types.ContainsKey(name))
            {   //if it already exists, we load & replace the existing element in the dictionary
                //the ReadFromFile function achieves this by removing previous data.
                _types[name].ReadFromFile(path);
                return true;
            }
            else
            {   //the data store is not currently loaded in _types
                //so we have to instantiate the ObjectWarehouse variable before reading the data
                switch (_manifest[name].Item2)
                {
                    case "memory":
                        _types.Add(name, new ObjectWarehouse_Raw());
                        _types[name].ReadFromFile(path);
                        break;
                    case "dataset":
                        _types.Add(name, new ObjectWarehouse_DataSet());
                        _types[name].ReadFromFile(path);
                        break;
                }
                return true;
            }
        }

        public static bool SaveDataStore(string name)
        {
            if (!_manifest.ContainsKey(name))
            {
                MiniScriptSingleton.LogError("SaveDataStore failed to find the specified store label {" + name + "}");
                return false;
            }

            string path = workingPath + _manifest[name].Item1 + ".xml";
            _types[name].WriteToFile(path);
            return true;
        }

        public static bool UnloadDataStore(string name)
        {
            if (!_manifest.ContainsKey(name))
            {
                MiniScriptSingleton.LogError("UnloadDataStore failed to find the specified store label {" + name + "}");
                return false;
            }

            _types.Remove(name);         
            if(!File.Exists(workingPath + _manifest[name].Item1 + ".xml"))
            {
                _manifest.Remove(name);
            }
            //else a datastore of this Guid does already exist in a saved file format, and we cant remove the manifest entry
            //if the entry is removed than the saved data store is treated as not an element of the manifest
            return true;
        }

        public static void LoadMod(string pathe)
        {
            workingPath = pathe;
            //ensure that our file manifest and types collection are empty when loading from the 'mods' folder
            _manifest.Clear();
            _types.Clear();

            string path = workingPath + "manifest.txt";
            if (File.Exists(path))
            {
                var c = Configuration.LoadFromFile(path);                
                foreach (Section s in c)
                {
                    if (s["Type"].StringValue == "memory")
                    {
                        if (File.Exists(workingPath + s["Guid"].StringValue + ".txt"))
                        {
                            _manifest.Add(s.Name, new System.Tuple<string, string>(s["Guid"].StringValue, s["Type"].StringValue));
                        }
                    }
                    else if (s["Type"].StringValue == "dataset")
                    {
                        if (File.Exists(workingPath + s["Guid"].StringValue + ".xml"))
                        {
                            _manifest.Add(s.Name, new System.Tuple<string, string>(s["Guid"].StringValue, s["Type"].StringValue));
                        }
                    }
                }
            }
        }

        public static void CreateAutosave()
        {
            if (_manifest.ContainsKey("Autosave"))
            {
                string path = workingPath;
                path += _manifest["Autosave"].Item1; //append the Guid
                path += ".tmp";

                DoSaveAction("Autosave", path);

                //create the manifest of our data stores
                Configuration cfg = new Configuration();

                foreach (KeyValuePair<string, System.Tuple<string, string>> kv in _manifest)
                {
                    cfg.Add(kv.Key);
                    cfg[kv.Key].Add("Guid", kv.Value.Item1);
                    cfg[kv.Key].Add("Type", kv.Value.Item2);
                }
                //write the manifest to file
                cfg.SaveToFile(workingPath + "manifest.txt");
            }
            else if (!_manifest.ContainsKey("Autosave"))
            {
                _manifest.Add("Autosave", new Tuple<string, string>(Guid.NewGuid().ToString(), "state"));
                string path = workingPath + _manifest["Autosave"].Item1 + ".tmp";
                DoSaveAction("Autosave", path);

                //create the manifest of our data stores
                Configuration cfg = new Configuration();

                foreach (KeyValuePair<string, System.Tuple<string, string>> kv in _manifest)
                {
                    cfg.Add(kv.Key);
                    cfg[kv.Key].Add("Guid", kv.Value.Item1);
                    cfg[kv.Key].Add("Type", kv.Value.Item2);
                }
                //write the manifest to file
                cfg.SaveToFile(workingPath + "manifest.txt");
            }
        }

        public static void SaveState(string label)
        {
            if (_manifest.ContainsKey(label))
            {
                string path = workingPath + _manifest[label].Item1 + ".save";
                DoSaveAction(label, path);
            }
            else if (!_manifest.ContainsKey(label))
            {
                _manifest.Add(label, new Tuple<string, string>(Guid.NewGuid().ToString(), "state"));
                string path = workingPath + _manifest[label].Item1 + ".save";
                DoSaveAction(label, path);

                //create the manifest of our data stores
                Configuration cfg = new Configuration();

                foreach (KeyValuePair<string, System.Tuple<string, string>> kv in _manifest)
                {
                    cfg.Add(kv.Key);
                    cfg[kv.Key].Add("Guid", kv.Value.Item1);
                    cfg[kv.Key].Add("Type", kv.Value.Item2);
                }
                //write the manifest to file
                cfg.SaveToFile(workingPath + "manifest.txt");
            }
        }

        public static void LoadAutosave()
        {
            if (_manifest.ContainsKey("Autosave"))
            {
                string path = workingPath + _manifest["Autosave"].Item1 + ".tmp";
                DoLoadAction(path);
            }
        }

        public static void LoadState(string label)
        {
            if (_manifest.ContainsKey(label))
            {
                string path = workingPath + _manifest[label].Item1 + ".save";
                DoLoadAction(path);
            }
        }

        public static ValList GetStates()
        {
            ValList result = new ValList();            
            foreach(KeyValuePair<string, Tuple<string,string>>kv in _manifest)
            {
                //only add the states that are data stores, skip the Autosave
                if(kv.Value.Item2 != "Autosave")
                {
                    //label is Key, Guid is Item1, Type is Item2
                    ValMap map = new ValMap();
                    map.map.Add(new ValString("Label"), new ValString(kv.Key));
                    map.map.Add(new ValString("Guid"), new ValString(kv.Value.Item1));
                    map.map.Add(new ValString("Type"), new ValString(kv.Value.Item2));
                    result.values.Add(map);
                }
            }
            return result;
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
            //first we clear what is currently in memory
            _types.Clear();
            //allocate a blank DataSet to prepare to read the xml file
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
                            foreach (DataColumn dc in table.Columns)
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
            _manifest = new Dictionary<string, Tuple<string, string>>();
        }
    }
}

