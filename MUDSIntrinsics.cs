using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Miniscript;
using SharpConfig;

namespace Miniscript.Unity3DDataSystem
{
    public static class MUDSIntrinsics
    {
        /// <summary>
        /// Static constructor so the Intrinsics are registered first, before anything else happens
        /// </summary>
        public static void Initialize()
        {
            var a = Intrinsic.Create("CreateDataStore");
            #region
            a.AddParam("typename", "SomeName");
            a.AddParam("storetype", "memory");
            a.code = (context, partialResult) =>
            {

#if DEBUG_MUDS
                Debug.Log("MUDSIntrinsics.CreateDataStore: " + context.ToString());
#endif
                if (ObjectFactorySingleton.Contains(context.GetLocalString("typename")))
                {   //type is already 'registered' and has an ObjectWarehouse allocated
                    MiniScriptSingleton.LogError("Intrinsic CreateDataStore() was given a 'typename' argument matching one already allocated.");
                }
                else
                {
                    switch (context.GetLocalString("storetype"))
                    {
                        case "memory":
                            ObjectFactorySingleton.Create(context.GetLocalString("typename"), new ObjectWarehouse_Raw());
                            break;
                        case "dataset":
                            ObjectFactorySingleton.Create(context.GetLocalString("typename"), new ObjectWarehouse_DataSet());
                            break;
                        default:
                            MiniScriptSingleton.LogError("Intrinsic CreateDataStore() 'storetype' argument is unsupported.");
                            break;
                    }
                }

                context.interpreter.standardOutput.Invoke("");
                return new Intrinsic.Result(null, true);
            };
            #endregion

            a = Intrinsic.Create("RemoveDataStore");
            #region
            a.AddParam("typename");
            a.code = (context, partialResult) =>
            {
#if DEBUG_MUDS
                Debug.Log("MUDSIntrinsics.RemoveDataStore: " + context.ToString());
#endif
                ObjectFactorySingleton.Remove(context.GetLocalString("typename"));
                return new Intrinsic.Result(null, true);
            };
            #endregion

            a = Intrinsic.Create("GetTypeStoreList");
            a.code = (context, partialResult) =>
            {
#if DEBUG_MUDS
                Debug.Log("MUDSIntrinsics.GetTypeStoreList: " + context.ToString());
#endif
                return new Intrinsic.Result(ObjectFactorySingleton.TypeList(), true);
            };

            a = Intrinsic.Create("Select");
            #region
            a.AddParam("type");
            a.AddParam("property");
            a.AddParam("value");
            a.code = (context, partialResult) =>
            {
#if DEBUG_MUDS
                Debug.Log("MUDSIntrinsics.Select: " + context.ToString());
#endif
                var rf = ObjectFactorySingleton.Get(context.GetLocalString("type"));
                if (rf != null)
                {
                    ValList rst = new ValList();
                    if (context.GetLocal("value") is ValString)
                    {
                        rst = rf.Select(new ValString(context.GetLocal("property").ToString()), new ValString(context.GetLocal("value").ToString()));
                    }
                    else if (context.GetLocal("value") is ValNumber)
                    {
                        rst = rf.Select(new ValString(context.GetLocal("property").ToString()), new ValNumber(double.Parse(context.GetLocal("value").ToString())));
                    }

                    return new Intrinsic.Result(rst, true);
                }
                return new Intrinsic.Result(null);
            };
            #endregion

            a = Intrinsic.Create("SelectRegx");
            #region
            a.AddParam("type");
            a.AddParam("property");
            a.AddParam("pattern");
            a.code = (context, partialResult) =>
            {
#if DEBUG_MUDS
                Debug.Log("MUDSIntrinsics.SelectRegx: " + context.ToString());
#endif
                var rf = ObjectFactorySingleton.Get(context.GetLocalString("type"));
                if (rf != null)
                {
                    ValList rst = new ValList();
                    rst = rf.SelectRegx(new ValString(context.GetLocalString("property")),
                        new ValString(context.GetLocalString("pattern")));

                    return new Intrinsic.Result(rst);
                }
                return new Intrinsic.Result(null);
            };
            #endregion

            a = Intrinsic.Create("Select");
            #region
            a.AddParam("type");
            a.AddParam("property");
            a.AddParam("lower");
            a.AddParam("upper");
            a.code = (context, partialResult) =>
            {
#if DEBUG_MUDS
                Debug.Log("MUDSIntrinsics.Select: " + context.ToString());
#endif
                var rf = ObjectFactorySingleton.Get(context.GetLocalString("type"));
                if (rf != null)
                {
                    ValList rst = new ValList();
                    ValNumber z, v;
                    z = new ValNumber(double.Parse(context.GetLocalString("lower")));
                    v = new ValNumber(double.Parse(context.GetLocalString("upper")));

                    rst = rf.Select(new ValString(context.GetLocalString("property")), z, v);

                    return new Intrinsic.Result(rst);
                }

                return new Intrinsic.Result(null);
            };
            #endregion

            a = Intrinsic.Create("GetInstance");
            #region
            a.AddParam("type");
            a.AddParam("ID");
            a.code = (context, partialResult) =>
            {
#if DEBUG_MUDS
                Debug.Log("MUDSIntrinsics.GetInstance: " + context.ToString());
#endif
                var rf = ObjectFactorySingleton.Get(context.GetLocalString("type"));
                if (rf != null)
                {
                    return new Intrinsic.Result(rf.GetInstance(System.Guid.Parse(context.GetLocalString("ID"))));
                }
                return new Intrinsic.Result(null);
            };
            #endregion

            a = Intrinsic.Create("GetRandomInstance");
            #region
            a.AddParam("type");
            a.code = (context, partialResult) =>
            {
#if DEBUG_MUDS
                Debug.Log("MUDSInstrinsics.GetRandomInstance: " + context.ToString());
#endif
                var rf = ObjectFactorySingleton.Get(context.GetLocalString("type"));
                if (rf != null)
                {
                    return new Intrinsic.Result(rf.GetRandomInstance());
                }
                return new Intrinsic.Result(null);
            };
            #endregion

            a = Intrinsic.Create("GetRandomInstances");
            #region
            a.AddParam("type");
            a.AddParam("quantity");
            a.AddParam("unique");
            a.code = (context, partialResult) =>
            {
#if DEBUG_MUDS
                Debug.Log("MUDSIntrinsics.GetRandomInstances: " + context.ToString());
#endif
                var rf = ObjectFactorySingleton.Get(context.GetLocalString("type"));
                if (rf != null)
                {
                    ValNumber za = (ValNumber)context.GetLocal("quantity");
                    ValNumber zb = (ValNumber)context.GetLocal("unique");
                    return new Intrinsic.Result(rf.GetRandomInstances(za, zb));
                }
                return new Intrinsic.Result(null);
            };
            #endregion

            a = Intrinsic.Create("GetInstances");
            #region
            a.AddParam("type");
            a.code = (context, partialResult) =>
            {
#if DEBUG_MUDS
                Debug.Log("MUDSIntrinsics.GetInstances: " + context.ToString());
#endif
                var rf = ObjectFactorySingleton.Get(context.GetLocalString("type"));
                if (rf != null)
                {
                    return new Intrinsic.Result(rf.GetInstances());
                }

                return new Intrinsic.Result(null);
            };
            #endregion

            a = Intrinsic.Create("RemoveInstance");
            #region
            a.AddParam("type");
            a.AddParam("ID");
            a.code = (context, partialResult) =>
            {
#if DEBUG_MUDS
                Debug.Log("MUDSIntrinsics.RemoveInstance: " + context.ToString());
#endif
                var rf = ObjectFactorySingleton.Get(context.GetLocalString("type"));
                if (rf != null)
                {
                    rf.DestroyInstance(System.Guid.Parse(context.GetLocalString("ID")));
                }

                return new Intrinsic.Result(null);
            };
            #endregion

            a = Intrinsic.Create("RemoveInstances");
            #region 
            a.AddParam("type");
            a.AddParam("list");
            a.code = (context, partialResult) =>
            {
#if DEBUG_MUDS
                Debug.Log("MUDSIntrinsics.RemoveInstances: " + context.ToString());
#endif
                var rf = ObjectFactorySingleton.Get(context.GetLocalString("type"));
                if (rf != null)
                {
                    var lst = context.GetLocal("list") as ValList;
                    foreach (Value v in lst.values)
                    {
                        if (v is ValMap)
                        {
                            rf.DestroyInstance(System.Guid.Parse(((ValMap)v)["__ID__"].ToString()));
                        }
                    }
                    return new Intrinsic.Result(ValNumber.Truth(true));
                }

                return new Intrinsic.Result(ValNumber.Truth(false));
            };
            #endregion

            a = Intrinsic.Create("CreateInstance");
            #region
            a.AddParam("type");
            a.code = (context, partialResult) =>
            {
#if DEBUG_MUDS
                Debug.Log("MUDSIntrinsics.CreateInstance: " + context.ToString());
#endif
                var rf = ObjectFactorySingleton.Get(context.GetLocalString("type"));
                if (rf != null)
                {
                    var rst = rf.CreateInstance();
                    return new Intrinsic.Result(rst);
                }

                return new Intrinsic.Result(null);
            };
            #endregion

            a = Intrinsic.Create("CreateInstances");
            #region
            a.AddParam("type");
            a.AddParam("quantity");
            a.code = (context, partialResult) =>
            {
#if DEBUG_MUDS
                Debug.Log("MUDSIntrinsics.CreateInstances: " + context.ToString());
#endif
                var rf = ObjectFactorySingleton.Get(context.GetLocalString("type"));
                if (rf != null)
                {
                    var rst = rf.CreateInstances(new ValNumber(context.GetLocalInt("quantity")));
                    return new Intrinsic.Result(rst);
                }

                return new Intrinsic.Result(null);
            };
            #endregion

            a = Intrinsic.Create("InstanceQuantity");
            #region
            a.AddParam("type");
            a.code = (context, partialResult) =>
            {
#if DEBUG_MUDS
                Debug.Log("MUDSIntrinsics.InstanceQuantity: " + context.ToString());
#endif
                var rf = ObjectFactorySingleton.Get(context.GetLocalString("type"));
                if (rf != null)
                {
                    return new Intrinsic.Result(new ValNumber(rf.InstanceCount));
                }
                return new Intrinsic.Result(0);
            };
            #endregion

            a = Intrinsic.Create("HasAttribute");
            #region
            a.AddParam("type");
            a.AddParam("propertyname");
            a.code = (context, partialResult) =>
            {
#if DEBUG_MUDS
                Debug.Log("MUDSIntrinsics.HasAttribute: " + context.ToString());
#endif
                var rf = ObjectFactorySingleton.Get(context.GetLocalString("type"));
                if (rf != null)
                {
                    if (rf.GetMSType().ContainsKey("propertyname"))
                    {
                        return new Intrinsic.Result(ValNumber.Truth(true));
                    }
                }
                return new Intrinsic.Result(ValNumber.Truth(false));
            };
            #endregion

            a = Intrinsic.Create("AddAttribute");
            #region
            a.AddParam("type");
            a.AddParam("propertyname");
            a.AddParam("propertytype");
            a.code = (context, partialResult) =>
            {
#if DEBUG_MUDS
                Debug.Log("MUDSIntrinsics.AddAttribute: " + context.ToString());
#endif
                var rf = ObjectFactorySingleton.Get(context.GetLocalString("type"));
                if (rf != null)
                {
                    if (rf.AddAttribute(new ValString(context.GetLocalString("propertyname")),
                        context.GetLocal("propertytype")))
                    {
                        return new Intrinsic.Result(ValNumber.Truth(true));
                    }
                }
                return new Intrinsic.Result(ValNumber.Truth(false));
            };
            #endregion

            a = Intrinsic.Create("RemoveAttribute");
            #region
            a.AddParam("type");
            a.AddParam("propertyname");
            a.code = (context, partialResult) =>
            {
#if DEBUG_MUDS
                Debug.Log("MUDSIntrinsics.RemoveAttribute: " + context.ToString());
#endif
                var rf = ObjectFactorySingleton.Get(context.GetLocalString("type"));
                if (rf != null)
                {
                    if (rf.RemoveAttribute(new ValString(context.GetLocalString("propertyname"))))
                    {
                        return new Intrinsic.Result(ValNumber.Truth(true));
                    }
                }
                return new Intrinsic.Result(ValNumber.Truth(false));
            };
            #endregion

            a = Intrinsic.Create("SaveDataStore");
            #region
            a.AddParam("type");
            a.code = (context, partialResult) =>
            {
#if DEBUG_MUDS
                Debug.Log("MUDSIntrinsics.SaveDataStore: " + context.ToString());
#endif
                ObjectFactorySingleton.SaveDataStore(context.GetLocalString("type"));
                return new Intrinsic.Result(null);
            };
            #endregion

            a = Intrinsic.Create("LoadDataStore");
            #region
            a.AddParam("type");
            a.code = (context, partialResult) =>
            {
#if DEBUG_MUDS
                Debug.Log("MUDSIntrinsics.LoadDataStore: " + context.ToString());
#endif
                ObjectFactorySingleton.LoadDataStore(context.GetLocalString("type"));
                return new Intrinsic.Result(null);
            };
            #endregion

            //can be used to save the game
            a = Intrinsic.Create("SaveState");
            #region
            a.AddParam("savename");
            a.code = (context, partialResult) =>
            {

                return new Intrinsic.Result(null);
            };
            #endregion

            //creates a 'saved game' as a single file, unnamed, useful for multiple reasons
            a = Intrinsic.Create("SaveAutosave");
            #region
            a.code = (context, partialResult) =>
            {

                return new Intrinsic.Result(null);
            };
            #endregion

            a = Intrinsic.Create("LoadState");
            #region
            a.AddParam("savename");
            a.code = (context, partialResult) =>
            {

                return new Intrinsic.Result(null);
            };
            #endregion

            a = Intrinsic.Create("LoadAutosave");
            #region        
            a.code = (context, partialResult) =>
            {

                return new Intrinsic.Result(null);
            };
            #endregion


            a = Intrinsic.Create("GetStates");
            #region        
            a.code = (context, partialResult) =>
            {

                return new Intrinsic.Result(null);
            };
            #endregion
        }
    }
}


