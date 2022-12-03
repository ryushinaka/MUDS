using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USE_ODIN
using Sirenix.OdinInspector;
#endif
using Miniscript;
using Miniscript.Unity3DDataSystem;

public class ObjectFactoryObserver : MonoBehaviour
{
    public Dictionary<string, int> warehouses;

    void Start()
    {
        warehouses = new Dictionary<string, int>();
        //ObjectFactorySingleton.OnNewWarehouse += new ObjectFactorySingleton.NewWarehouse(AddWarehouse);
    }

    float interval = 0f;
    void Update()
    {
        if(interval >= Time.time)
        {
            if(warehouses != null)
            {
                warehouses.Clear();
                foreach (Value v in ObjectFactorySingleton.TypeList().values)
                {
                    warehouses.Add(v.ToString(), ObjectFactorySingleton.Get(v.ToString()).InstanceCount);
                }
                interval += 5f;
            }            
        }
    }
}
