using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SharpConfig;

namespace Miniscript.Unity3DDataSystem
{
    public class ObjectWarehouseManifest
    {
        /// <summary>
        /// Dictionary key value is the 'name' attribute of the data store, the Tuple contains the 
        /// Guid and 'type' of the data store (in that order)
        /// </summary>
        public Dictionary<string, System.Tuple<string, string>> values;

        public bool Contains(string name)
        {
            return values.ContainsKey(name);
        }

        public void Add(string name, string type)
        {
            if (values.ContainsKey(name)) return;

            values.Add(name, new System.Tuple<string, string>(System.Guid.NewGuid().ToString(), type));
            string path = new ApplicationConfig().Path + "/" + ModManagementSingleton.Instance.CurrentMod.ModHashID
                + "/data/" + values[name].Item1;
            if (type == "memory") { path += ".txt"; }
            else if (type == "dataset") { path += ".xml"; }

            File.WriteAllBytes(values[name].Item1, new byte[] { });
        }

        public void Remove(string name)
        {
            if (!values.ContainsKey(name)) return;

            values.Remove(name);
            string path = new ApplicationConfig().Path + "/" + ModManagementSingleton.Instance.CurrentMod.ModHashID
                + "/data/" + values[name].Item1;
            if (values[name].Item2 == "memory") { path += ".txt"; }
            else if (values[name].Item2 == "dataset") { path += ".xml"; }
            File.Delete(path);
        }

        public string GetPath(string name)
        {
            if (values.ContainsKey(name))
            {
                string path = new ApplicationConfig().Path + "/" + ModManagementSingleton.Instance.CurrentMod.ModHashID + "/data/";
                path += values[name].Item1;
                if (values[name].Item2 == "memory") { path += ".txt"; }
                else if (values[name].Item2 == "dataset") { path += ".xml"; }
                return path;
            }

            return string.Empty;
        }

        public void ReadManifest()
        {
            if (ModManagementSingleton.Instance.CurrentMod == null) { return; }

            string path = new ApplicationConfig().Path + "/" + ModManagementSingleton.Instance.CurrentMod.ModHashID + "/data/manifest.txt";
            if (File.Exists(path))
            {
                var c = Configuration.LoadFromFile(path);
                string path2 = new ApplicationConfig().Path + "/" + ModManagementSingleton.Instance.CurrentMod.ModHashID + "/data/";
                foreach (Section s in c)
                {
                    if (s["Type"].StringValue == "memory")
                    {
                        if (File.Exists(path2 + s["Guid"].StringValue + ".txt"))
                        {
                            values.Add(s.Name, new System.Tuple<string, string>(s["Guid"].StringValue, s["Type"].StringValue));
                        }
                    }
                    else if (s["Type"].StringValue == "dataset")
                    {
                        if (File.Exists(path2 + s["Guid"].StringValue + ".xml"))
                        {
                            values.Add(s.Name, new System.Tuple<string, string>(s["Guid"].StringValue, s["Type"].StringValue));
                        }
                    }
                }
            }
        }

        public void WriteManifest()
        {
            if (ModManagementSingleton.Instance.CurrentMod == null) { return; }

            Configuration cfg = new Configuration();

            foreach (KeyValuePair<string, System.Tuple<string, string>> kv in values)
            {
                cfg.Add(kv.Key);
                cfg[kv.Key].Add("Guid", kv.Value.Item1);
                cfg[kv.Key].Add("Type", kv.Value.Item2);
            }

            string path = new ApplicationConfig().Path + "/" + ModManagementSingleton.Instance.CurrentMod.ModHashID + "/data/manifest.txt";
            cfg.SaveToFile(path);
        }

        public ObjectWarehouseManifest()
        {
            values = new Dictionary<string, System.Tuple<string, string>>();
        }
    }
}

