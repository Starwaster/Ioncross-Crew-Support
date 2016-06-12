//#define DEBUG
//#define DEBUG_UPDATES

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using KSP;


namespace IoncrossKerbal
{
    /*------------------------------------------------------*\
     * IonResourceData Class                                *
     * This classes stores information related to resources *
    \*------------------------------------------------------*/
    [Serializable]
    public class IonResourceData : IConfigNode, IEquatable<IonResourceData>, IEquatable<string>
    {
        private string name;
        public string Name { get { return name; } }
        private int id;
        public int ID { get { return id; } }
        private string guiName;
        public string GUIName { get { return guiName; } }

        protected double rateBase;
        public double RateBase { get { return rateBase; } }
        protected double ratePerKerbal;
        public double RatePerKerbal { get { return ratePerKerbal; } }
        protected double ratePerCapacity;
        public double RatePerCapacity { get { return ratePerCapacity; } }

        protected double rateBaseMod;
        public double RateBaseMod { get { return rateBaseMod; } }
        protected double ratePerKerbalMod;
        public double RatePerKerbalMod { get { return ratePerKerbalMod; } }
        protected double ratePerCapacityMod;
        public double RatePerCapacityMod { get { return ratePerCapacityMod; } }

        private bool low;
        public bool Low { get { return low; } set { low = value; } }
        private bool depleated;
        public bool Depleated { get { return depleated; } set { depleated = value; } }

        private IonModuleDisplay displayModule;
        public IonModuleDisplay DisplayModule { get { return displayModule; } set { displayModule = value; } }

        /************************************************************************\
         * IonResourceData class                                                *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonResourceData()
        {
#if DEBUG
            Debug.Log("IonResourceData.Constructor()");
#endif
            Initialize();
        }

        /************************************************************************\
         * IonResourceData class                                                *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonResourceData(ConfigNode node)
        {
#if DEBUG
            Debug.Log("IonResourceData.Constructor(ConfigNode)");
#endif
            Initialize();
            Load(node);
        }

        /************************************************************************\
         * IonResourceData class                                                *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonResourceData(IonResourceData other)
        {
#if DEBUG
            Debug.Log("IonResourceData.Constructor(IonResourceData)");
#endif
            name = other.name;
            id = other.id;
            guiName = other.guiName;

            rateBase = other.rateBase;
            ratePerKerbal = other.ratePerKerbal;
            ratePerCapacity = other.ratePerCapacity;

            rateBaseMod = other.rateBaseMod;
            ratePerKerbalMod = other.ratePerKerbalMod;
            ratePerCapacityMod = other.ratePerCapacityMod;

            low = other.low;
            depleated = other.depleated;
        }

        /************************************************************************\
         * IonResourceData class                                                *
         * Initialize function                                                   *
         *                                                                      *
         * Assigns default values to variables.                                 *
        \************************************************************************/
        protected virtual void Initialize()
        {
            name = "";
            guiName = "";

            rateBase = 0;
            ratePerKerbal = 0;
            ratePerCapacity = 0;

            rateBaseMod = 1;
            ratePerKerbalMod = 1;
            ratePerCapacityMod = 1;

            low = false;
            depleated = false;
        }

        /************************************************************************\
         * IonResourceData class                                                *
         * Equals function                                                      *
         *                                                                      *
         * Compares this object with another.  Returns true if they both have   *
         * the same name.                                                       *
        \************************************************************************/
        public bool Equals(IonResourceData other)
        {
            return Equals(other.Name);
        }

        /************************************************************************\
         * IonResourceData class                                                *
         * Equals function                                                      *
         *                                                                      *
         * Compares this object with another.  Returns true if resourceName     *
         * matches the name of this resource.                                   *
        \************************************************************************/
        public bool Equals(string resourceName)
        {
            return Name == resourceName;
        }


        /************************************************************************\
         * IonResourceData class                                                *
         * Load function                                                        *
         *                                                                      *
        \************************************************************************/
        public virtual void Load(ConfigNode node)
        {
#if DEBUG
            Debug.Log("IonResourceData.Load()");
            Debug.Log("IonResourceData.Load(): node\n" + node.ToString());
#endif
            name = node.GetValue("name");
            id = name.GetHashCode();

            if (node.HasValue("guiName"))
                guiName = node.GetValue("guiName");

            if (node.HasValue("rateBase"))
                rateBase = Convert.ToDouble(node.GetValue("rateBase"));
            else if (node.HasValue("rate"))
                rateBase = Convert.ToDouble(node.GetValue("rate"));
            if (node.HasValue("ratePerKerbal"))
                ratePerKerbal = Convert.ToDouble(node.GetValue("ratePerKerbal"));
            if (node.HasValue("ratePerCapacity"))
                ratePerCapacity = Convert.ToDouble(node.GetValue("ratePerCapacity"));

            if (node.HasValue("rateBaseMod"))
                rateBaseMod = Convert.ToDouble(node.GetValue("rateBaseMod"));
            else if (node.HasValue("rateMod"))
                rateBaseMod = Convert.ToDouble(node.GetValue("rateMod"));
            if (node.HasValue("ratePerKerbalMod"))
                ratePerKerbalMod = Convert.ToDouble(node.GetValue("ratePerKerbalMod"));
            if (node.HasValue("ratePerCapacityMod"))
                ratePerCapacityMod = Convert.ToDouble(node.GetValue("ratePerCapacityMod"));


            if (node.HasValue("low"))
                low = "True" == node.GetValue("low") || "true" == node.GetValue("low") || "TRUE" == node.GetValue("low");
            else if (node.HasValue("resourceLow"))
                low = "True" == node.GetValue("resourceLow") || "true" == node.GetValue("resourceLow") || "TRUE" == node.GetValue("resourceLow");

            if (node.HasValue("depleated"))
                depleated = "True" == node.GetValue("depleated") || "true" == node.GetValue("depleated") || "TRUE" == node.GetValue("depleated");
            else if (node.HasValue("resourceDepleated"))
                depleated = "True" == node.GetValue("resourceDepleated") || "true" == node.GetValue("resourceDepleated") || "TRUE" == node.GetValue("resourceDepleated");
        }
        /************************************************************************\
         * IonResourceData class                                                *
         * LoadLocal function                                                   *
         *                                                                      *
        \************************************************************************/
        public virtual void LoadLocal(ConfigNode node)
        {
#if DEBUG
            Debug.Log("IonResourceData.LoadLocal()");
            Debug.Log("IonResourceData.LoadLocal(): node\n" + node.ToString());
#endif
            name = node.GetValue("name");
            id = name.GetHashCode();

            if (node.HasValue("guiName"))
                guiName = node.GetValue("guiName");

            if (node.HasValue("rateBaseMod"))
                rateBaseMod = Convert.ToDouble(node.GetValue("rateBaseMod"));
            else if (node.HasValue("rateMod"))
                rateBaseMod = Convert.ToDouble(node.GetValue("rateMod"));
            if (node.HasValue("ratePerKerbalMod"))
                ratePerKerbalMod = Convert.ToDouble(node.GetValue("ratePerKerbalMod"));
            if (node.HasValue("ratePerCapacityMod"))
                ratePerCapacityMod = Convert.ToDouble(node.GetValue("ratePerCapacityMod"));
        }

        /************************************************************************\
         * IonResourceData class                                                *
         * Save function                                                        *
         *                                                                      *
        \************************************************************************/
        public virtual void Save(ConfigNode node)
        {
#if DEBUG
            Debug.Log("IonResourceData.Save()");
#endif
            node.AddValue("name", name);
            if (null != guiName)
                node.AddValue("guiName", guiName);

            node.AddValue("rateBase", rateBase);
            node.AddValue("ratePerKerbal", ratePerKerbal);
            node.AddValue("ratePerCapacity", ratePerCapacity);

            node.AddValue("rateBaseMod", rateBaseMod);
            node.AddValue("ratePerKerbalMod", ratePerKerbalMod);
            node.AddValue("ratePerCapacityMod", ratePerCapacityMod);

            node.AddValue("resourceLow", low);
            node.AddValue("resourceDepleated", depleated);
#if DEBUG
            Debug.Log("IonResourceData.Save(): node\n" + node.ToString());
#endif
        }
        /************************************************************************\
         * IonResourceData class                                                *
         * SaveLocal function                                                   *
         *                                                                      *
        \************************************************************************/
        public virtual void SaveLocal(ConfigNode node)
        {
#if DEBUG
            Debug.Log("IonResourceData.SaveLocal()");
#endif
            node.AddValue("name", name);
            if (null != guiName)
                node.AddValue("guiName", guiName);

            node.AddValue("rateBaseMod", rateBaseMod);
            node.AddValue("ratePerKerbalMod", ratePerKerbalMod);
            node.AddValue("ratePerCapacityMod", ratePerCapacityMod);
#if DEBUG
            Debug.Log("IonResourceData.SaveLocal(): node\n" + node.ToString());
#endif
        }


        /************************************************************************\
         * IonResourceData class                                                *
         * FindResource function                                                *
         *                                                                      *
         * Searches list for an entry matching resourceName.  If found it       *
         * returns the entry.  If not it retruns null.                          *
        \************************************************************************/
        public static IonResourceData FindResource(List<IonResourceData> list, string resourceName)
        {
            IonResourceData rtnResource = null;
            foreach (IonResourceData resourceData in list)
            {
                if(resourceName == resourceData.Name)
                {
                    rtnResource = resourceData;
                    break;
                }
            }

            return rtnResource;
        }


        /************************************************************************\
         * IonSuportResourceData class                                          *
         * AddDisplayRate function                                              *
         *                                                                      *
        \************************************************************************/
        public virtual void AddDisplayRate(float rate)
        {
#if DEBUG_UPDATES
            Debug.Log("IonResourceData.AddDisplayRate()");
            Debug.Log("IonResourceData.AddDisplayRate(): display module is " + (null == displayModule ? "NULL" : "not null"));
#endif
            if (null != displayModule)
                displayModule.AddRate(rate);
        }


        /************************************************************************\
         * IonSuportResourceData class                                          *
         * ToString function                                                    *
         *                                                                      *
        \************************************************************************/
        public override string ToString()
        {
            string rtnStr = "";

            ConfigNode node = new ConfigNode("IonResourceData");
            Save(node);

            rtnStr = node.ToString();

            return rtnStr;
        }
    }
    //==========================================================================================================
    // END of IonResourceData Class
    //==========================================================================================================

    /*------------------------------------------------------*\
     * IonSuportResourceData Class                          *
     * This classes stores global information related to a  *
     * resource used by kerbals or their pods in the crew   *
     * support system.                                      *
    \*------------------------------------------------------*/
    [Serializable]
    public class IonSupportResourceDataGlobal : IonResourceData
    {
        private double evaAmount;
        public double EVAamount { get { return evaAmount; } }
        private double evaMaxAmount;
        public double EVAMaxAmount { get { return evaMaxAmount; } }

        //Effect upon being unable to fulfill request for the resource
        //Note: causeLock will only apply to parts with a ModuleCommand attached
        private bool causeLock;
        public bool CauseLock { get { return causeLock; } }
        private bool causeDeath;
        public bool CauseDeath { get { return causeDeath; } }

        //If causeDeath is true then every timeKillRollInterval each kerbal onboard has killChance to be killed (provided at least the minimum framesWithoutResource has passed)
        private double killRollInterval;
        public double KillRollInterval { get { return killRollInterval; } }
        private float killChance;
        public float KillChance { get { return killChance; } }
		private float killChanceDeltaPenalty;
		public float KillChanceDeltaPenalty { get { return killChanceDeltaPenalty; } }


        /************************************************************************\
         * IonSupportResourceDataGlobal class                                   *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonSupportResourceDataGlobal() : base()
        {
#if DEBUG
            Debug.Log("IonSupportResourceDataGlobal.Constructor()");
#endif
        }

        /************************************************************************\
         * IonSupportResourceDataGlobal class                                   *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonSupportResourceDataGlobal(ConfigNode node) : base(node)
        {
#if DEBUG
            Debug.Log("IonSupportResourceDataGlobal.Constructor(ConfigNode)");
#endif
        }

        /************************************************************************\
         * IonSupportResourceDataGlobal class                                   *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonSupportResourceDataGlobal(IonSupportResourceDataGlobal other) : base(other)
        {
#if DEBUG
            Debug.Log("IonSupportResourceDataGlobal.Constructor(IonSupportResourceDataGlobal)");
#endif
            evaAmount = other.evaAmount;

            causeLock = other.causeLock;
            causeLock = other.causeLock;

            killRollInterval = other.killRollInterval;
            killChance = other.killChance;
        }

        /************************************************************************\
         * IonSupportResourceDataGlobal class                                   *
         * Initialize function                                                   *
         *                                                                      *
         * Assigns default values to variables.                                 *
        \************************************************************************/
        protected override void Initialize()
        {
            base.Initialize();

            evaAmount = -1;
            causeLock = false;
            causeDeath = false;
            killRollInterval = 3600;
            killChance = 0.1f;
        }


        /************************************************************************\
         * IonSuportResourceData class                                          *
         * Load function                                                        *
         *                                                                      *
        \************************************************************************/
        public override void Load(ConfigNode node)
        {
            base.Load(node);
#if DEBUG
            Debug.Log("IonSuportResourceData.Load()");
            Debug.Log("IonSuportResourceData.Load(): node\n" + node.ToString());
#endif
            if (node.HasValue("evaAmount"))
                evaAmount = Convert.ToDouble(node.GetValue("evaAmount"));
            if (node.HasValue("evaMaxAmount"))
                evaMaxAmount = Convert.ToDouble(node.GetValue("evaMaxAmount"));

            if (node.HasValue("causeLock"))
                causeLock = "True" == node.GetValue("causeLock") || "true" == node.GetValue("causeLock") || "TRUE" == node.GetValue("causeLock");
            else if (node.HasValue("boolCauseLock"))
                causeLock = "True" == node.GetValue("boolCauseLock") || "true" == node.GetValue("boolCauseLock") || "TRUE" == node.GetValue("boolCauseLock");

            if (node.HasValue("causeDeath"))
                causeDeath = "True" == node.GetValue("causeDeath") || "true" == node.GetValue("causeDeath") || "TRUE" == node.GetValue("causeDeath");
            else if (node.HasValue("boolCauseDeath"))
                causeDeath = "True" == node.GetValue("boolCauseDeath") || "true" == node.GetValue("boolCauseDeath") || "TRUE" == node.GetValue("boolCauseDeath");

            if (causeDeath)
            {
                if (node.HasValue("killRollInterval"))
                    killRollInterval = Convert.ToDouble(node.GetValue("killRollInterval"));
                if (node.HasValue("killChance"))
                    killChance = Convert.ToSingle(node.GetValue("killChance"));
				if (node.HasValue("killChanceDeltaPenalty"))
					killChanceDeltaPenalty = Convert.ToSingle(node.GetValue("killChanceDeltaPenalty"));
			}
        }

        /************************************************************************\
         * IonSuportResourceData class                                          *
         * Save function                                                        *
         *                                                                      *
        \************************************************************************/
        public override void Save(ConfigNode node)
        {
            base.Save(node);
#if DEBUG
            Debug.Log("IonSuportResourceData.Save()");
#endif
            node.AddValue("evaAmount", evaAmount);
            node.AddValue("evaMaxAmount", evaMaxAmount);

            node.AddValue("causeLock", causeLock);
            node.AddValue("causeDeath", causeDeath);

            if (causeDeath)
            {
                node.AddValue("killRollInterval", killRollInterval);
                node.AddValue("killChance", killChance);
            }
#if DEBUG
            Debug.Log("IonSuportResourceData.Save(): node\n" + node.ToString());
#endif
        }
    }
    //==========================================================================================================
    // END of IonSuportResourceDataGlobal Class
    //==========================================================================================================

    /*------------------------------------------------------*\
     * IonGeneratorResourceData Class                       *
     * This classes stores information related to a         *
     * generator resource for a part.                       *
    \*------------------------------------------------------*/
    [Serializable]
    public class IonGeneratorResourceData : IonResourceData
    {
        private double curAvailable;
        public double CurAvailable { get { return curAvailable; } set { curAvailable = value; }  }
        private double curFreeAmount;
        public double CurFreeAmount { get { return curFreeAmount; } set { curFreeAmount = value; } }
        private double curRequest;
        public double CurRequest { get { return curRequest; } set { curRequest = value; } }

        private float effectOnEfficency;
        public float EffectOnEfficency { get { return effectOnEfficency; } }
        private float cutoffMargin;
        public float CutoffMargin { get { return cutoffMargin; } }
        

        /************************************************************************\
         * IonGeneratorResourceData class                                       *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonGeneratorResourceData() : base()
        {
#if DEBUG
            Debug.Log("IonGeneratorResourceData.Constructor()");
#endif
        }

        /************************************************************************\
         * IonGeneratorResourceData class                                       *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonGeneratorResourceData(ConfigNode node) : base(node)
        {
#if DEBUG
            Debug.Log("IonGeneratorResourceData.Constructor(ConfigNode)");
#endif
        }

        /************************************************************************\
         * IonGeneratorResourceData class                                       *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonGeneratorResourceData(IonGeneratorResourceData other) : base(other)
        {
#if DEBUG
            Debug.Log("IonGeneratorResourceData.Constructor(IonGeneratorResourceData)");
#endif
            curAvailable = other.curAvailable;
            curFreeAmount = other.curFreeAmount;
            curRequest = other.curRequest;

            effectOnEfficency = other.effectOnEfficency;
            cutoffMargin = other.cutoffMargin;
        }

        /************************************************************************\
         * IonGeneratorResourceData class                                       *
         * Initialize function                                                   *
         *                                                                      *
         * Assigns default values to variables.                                 *
        \************************************************************************/
        protected override void Initialize()
        {
            base.Initialize();

            curAvailable = 0;
            curFreeAmount = 0;
            curRequest = 0;
            effectOnEfficency = 1;
            cutoffMargin = 0;
        }


        /************************************************************************\
         * IonGeneratorResourceData class                                       *
         * Load function                                                        *
         *                                                                      *
        \************************************************************************/
        public override void Load(ConfigNode node)
        {
            base.Load(node);
#if DEBUG
            Debug.Log("IonGeneratorResourceData.Load()");
            Debug.Log("IonGeneratorResourceData.Load(): node\n" + node.ToString());
#endif
            if (node.HasValue("effectOnEfficency"))
            {
                effectOnEfficency = Convert.ToSingle(node.GetValue("effectOnEfficency"));
                if (effectOnEfficency > 1)
                    effectOnEfficency = 1;
                else if (effectOnEfficency < 0)
                    effectOnEfficency = 0;
            }

            if (node.HasValue("cutoffMargin"))
            {
                cutoffMargin = Convert.ToSingle(node.GetValue("cutoffMargin"));
                if (cutoffMargin > 1)
                    cutoffMargin = 1;
                else if (cutoffMargin < 0)
                    cutoffMargin = 0;
            }
        }

        /************************************************************************\
         * IonGeneratorResourceData class                                       *
         * Save function                                                        *
         *                                                                      *
        \************************************************************************/
        public override void Save(ConfigNode node)
        {
            base.Save(node);
#if DEBUG
            Debug.Log("IonGeneratorResourceData.Save()");
#endif
            node.AddValue("effectOnEfficency", effectOnEfficency);
            node.AddValue("cutoffMargin", cutoffMargin);
#if DEBUG
            Debug.Log("IonGeneratorResourceData.Save(): node\n" + node.ToString());
#endif
        }
    }
    //==========================================================================================================
    // END of IonGeneratorResourceData Class
    //==========================================================================================================

    /*------------------------------------------------------*\
     * IonSupportResourceDataLocal Class                    *
     * This classes stores local information related to a   *
     * resource used by kerbals or their pods in the crew   *
     * support system.                                      *
    \*------------------------------------------------------*/
    [Serializable]
    public class IonSupportResourceDataLocal : IonResourceData
    {
        private IonSupportResourceDataGlobal data;
        public IonSupportResourceDataGlobal Data { get { return data; } set { data = value; } }

        public new double RateBase { get { return (null != data ? data.RateBase : RateBase); } }
        public new double RatePerKerbal { get { return (null != data ? data.RatePerKerbal : RatePerKerbal); } }
        public new double RatePerCapacity { get { return (null != data ? data.RatePerCapacity : RatePerCapacity); } }

        private double timeSinceLastKillRoll;
        public double TimeSinceLastKillRoll { get { return timeSinceLastKillRoll; } set { timeSinceLastKillRoll = value; } }
        private int framesWithoutResource;
        public int FramesWithoutResource { get { return framesWithoutResource; } set { framesWithoutResource = value; } }

		private double totalTimeWithoutResource;
		public double TotalTimeWithoutResource { get { return totalTimeWithoutResource; } set { totalTimeWithoutResource = value; } }

        public bool CauseLock { get { return (null != data ? data.CauseLock : false); } }
        public bool CauseDeath { get { return (null != data ? data.CauseDeath : false); } }

        public double KillRollInterval { get { return (null != data ? data.KillRollInterval : 0); } }
		public float KillChance { get { return (null != data ? data.KillChance : 0); } }
		public float KillChanceDeltaPenalty { get { return (null != data ? data.KillChanceDeltaPenalty : 0); } }

        public new bool Low { get { return (null != data ? data.Low : Low); } set { if (null != data) data.Low = value; else Low = value; } }
        public new bool Depleated { get { return (null != data ? data.Depleated : Depleated); } set { if (null != data) data.Depleated = value; else Depleated = value; } }


        /************************************************************************\
         * IonSupportResourceDataLocal class                                    *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonSupportResourceDataLocal() : base()
        {
#if DEBUG
            Debug.Log("IonSupportResourceDataLocal.Constructor()");
#endif
        }

        /************************************************************************\
         * IonSupportResourceDataLocal class                                    *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonSupportResourceDataLocal(ConfigNode node) : base(node)
        {
#if DEBUG
            Debug.Log("IonSupportResourceDataLocal.Constructor(ConfigNode)");
#endif
        }

        /************************************************************************\
         * IonSupportResourceDataLocal class                                    *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonSupportResourceDataLocal(IonSupportResourceDataGlobal globalData)
        {
#if DEBUG
            Debug.Log("IonSupportResourceDataLocal.Constructor(IonSupportResourceDataGlobal)");
#endif
            Initialize();

            ConfigNode node = new ConfigNode();
            globalData.Save(node);
            Load(node);
            rateBaseMod = 1;
            ratePerKerbalMod = 1;
            ratePerCapacityMod = 1;

            data = globalData;
        }

        /************************************************************************\
         * IonSupportResourceDataLocal class                                    *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonSupportResourceDataLocal(IonSupportResourceDataLocal other) : base(other)
        {
#if DEBUG
            Debug.Log("IonSupportResourceDataLocal.Constructor(IonSupportResourceDataLocal)");
#endif
            data = other.data;

            timeSinceLastKillRoll = other.timeSinceLastKillRoll;
            framesWithoutResource = other.framesWithoutResource;
			totalTimeWithoutResource = other.totalTimeWithoutResource;
        }

        /************************************************************************\
         * IonSupportResourceDataLocal class                                    *
         * Initialize function                                                   *
         *                                                                      *
         * Assigns default values to variables.                                 *
        \************************************************************************/
        protected override void Initialize()
        {
            base.Initialize();

            timeSinceLastKillRoll = 0;
            framesWithoutResource = 0;
			totalTimeWithoutResource = 0;
        }

        /************************************************************************\
         * IonSupportResourceDataLocal class                                    *
         * SetData function                                                     *
         *                                                                      *
         * Finds the matching resource entry in the global support resource     *
         * list and sets data to point to it.                                   *
        \************************************************************************/
        private void SetData()
        {
            if (null != IoncrossController.Instance && null != IoncrossController.Instance.Settings)
                data = IoncrossController.Instance.Settings.GetSupportResource(ID);
            else
                data = null;
        }


        /************************************************************************\
         * IonSupportResourceDataLocal class                                    *
         * Load function                                                        *
         *                                                                      *
        \************************************************************************/
        public override void Load(ConfigNode node)
        {
            base.Load(node);
#if DEBUG
            Debug.Log("IonSupportResourceDataLocal.Load()");
            Debug.Log("IonSupportResourceDataLocal.Load(): node\n" + node.ToString());
#endif
            if (node.HasValue("timeSinceLastKillRoll"))
                timeSinceLastKillRoll = Convert.ToDouble(node.GetValue("timeSinceLastKillRoll"));
            if (node.HasValue("framesWithoutResource"))
                framesWithoutResource = Convert.ToInt32(node.GetValue("framesWithoutResource"));
			if (node.HasValue("totalTimeWithoutResource"))
				totalTimeWithoutResource = Convert.ToDouble(node.GetValue("totalTimeWithoutResource"));

			//totalTimeWithoutResource

            SetData();
        }
        /************************************************************************\
         * IonSupportResourceDataLocal class                                    *
         * LoadLocal function                                                   *
         *                                                                      *
        \************************************************************************/
        public override void LoadLocal(ConfigNode node)
        {
            base.LoadLocal(node);
#if DEBUG
            Debug.Log("IonSupportResourceDataLocal.LoadLocal()");
            Debug.Log("IonSupportResourceDataLocal.LoadLocal(): node\n" + node.ToString());
#endif
            if (node.HasValue("timeSinceLastKillRoll"))
                timeSinceLastKillRoll = Convert.ToDouble(node.GetValue("timeSinceLastKillRoll"));
            if (node.HasValue("framesWithoutResource"))
                framesWithoutResource = Convert.ToInt32(node.GetValue("framesWithoutResource"));
			if (node.HasValue("totalTimeWithoutResource"))
				totalTimeWithoutResource = Convert.ToDouble(node.GetValue("totalTimeWithoutResource"));

			//totalTimeWithoutResource
            SetData();
        }

        /************************************************************************\
         * IonSupportResourceDataLocal class                                    *
         * Save function                                                        *
         *                                                                      *
        \************************************************************************/
        public override void Save(ConfigNode node)
        {
            base.Save(node);
#if DEBUG
            Debug.Log("IonSupportResourceDataLocal.Save()");
#endif
            node.AddValue("timeSinceLastKillRoll", timeSinceLastKillRoll);
            node.AddValue("framesWithoutResource", framesWithoutResource);
			node.AddValue ("totalTimeWithoutResource", totalTimeWithoutResource);
#if DEBUG
            Debug.Log("IonSupportResourceDataLocal.Save(): node\n" + node.ToString());
#endif
        }
        /************************************************************************\
         * IonSupportResourceDataLocal class                                    *
         * SaveLocal function                                                   *
         *                                                                      *
        \************************************************************************/
        public override void SaveLocal(ConfigNode node)
        {
            base.SaveLocal(node);
#if DEBUG
            Debug.Log("IonSupportResourceDataLocal.SaveLocal()");
#endif
            node.AddValue("timeSinceLastKillRoll", timeSinceLastKillRoll);
            node.AddValue("framesWithoutResource", framesWithoutResource);
			node.AddValue ("totalTimeWithoutResource", totalTimeWithoutResource);
#if DEBUG
            Debug.Log("IonSupportResourceDataLocal.SaveLocal(): node\n" + node.ToString());
#endif
        }

    }
    //==========================================================================================================
    // END of IonSupportResourceDataLocal Class
    //==========================================================================================================

    /*------------------------------------------------------*\
     * IonEVAResourceDataLocal Class                        *
     * This classes stores local information related to an  *
     * EVA resources used by kerbals or their EVA packs in  *
     * the crew support system.                             *
    \*------------------------------------------------------*/
    [Serializable]
    public class IonEVAResourceDataLocal : IonSupportResourceDataLocal
    {
        private double amount;
        public double Amount { get { return amount; } set { amount = value; } }
        private double maxAmount;
        public double MaxAmount { get { return maxAmount; } set { maxAmount = value; } }
        
        
        /************************************************************************\
         * IonEVAResourceDataLocal class                                        *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonEVAResourceDataLocal() : base()
        {
#if DEBUG
            Debug.Log("IonEVAResourceDataLocal.Constructor()");
#endif
        }

        /************************************************************************\
         * IonEVAResourceDataLocal class                                        *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonEVAResourceDataLocal(ConfigNode node) : base(node)
        {
#if DEBUG
            Debug.Log("IonEVAResourceDataLocal.Constructor(ConfigNode)");
#endif
        }

        /************************************************************************\
         * IonEVAResourceDataLocal class                                        *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonEVAResourceDataLocal(IonSupportResourceDataGlobal globalData) : base(globalData)
        {
#if DEBUG
            Debug.Log("IonEVAResourceDataLocal.Constructor(IonSupportResourceDataGlobal)");
#endif
        }

        /************************************************************************\
         * IonEVAResourceDataLocal class                                        *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonEVAResourceDataLocal(IonEVAResourceDataLocal other) : base(other)
        {
#if DEBUG
            Debug.Log("IonEVAResourceDataLocal.Constructor(IonEVAResourceDataLocal)");
#endif
            amount = other.amount;
            maxAmount = other.maxAmount;
        }

        /************************************************************************\
         * IonEVAResourceDataLocal class                                        *
         * Initialize function                                                   *
         *                                                                      *
         * Assigns default values to variables.                                 *
        \************************************************************************/
        protected override void Initialize()
        {
            base.Initialize();

            amount = 0;
            maxAmount = 0;
        }


        /************************************************************************\
         * IonEVAResourceDataLocal class                                        *
         * Load function                                                        *
         *                                                                      *
        \************************************************************************/
        public override void Load(ConfigNode node)
        {
            base.Load(node);
#if DEBUG
            Debug.Log("IonEVAResourceDataLocal.Load()");
            Debug.Log("IonEVAResourceDataLocal.Load(): node " + node.name);
#endif
            if (node.HasValue("amount"))
                double.TryParse(node.GetValue("amount"), out amount);
            if (node.HasValue("maxAmount"))
                double.TryParse (node.GetValue("maxAmount"), out maxAmount);

			Debug.Log ("IonEVAResourceDataLocal.Load() - amount / maxAmount: " + amount.ToString () + " / " + maxAmount.ToString ());
        }
        /************************************************************************\
         * IonEVAResourceDataLocal class                                        *
         * LoadLocal function                                                   *
         *                                                                      *
        \************************************************************************/
        public override void LoadLocal(ConfigNode node)
        {
            base.LoadLocal(node);
#if DEBUG
            Debug.Log("IonEVAResourceDataLocal.LoadLocal()");
            Debug.Log("IonEVAResourceDataLocal.LoadLocal(): node " + node.name);
#endif
            if (node.HasValue("amount"))
                double.TryParse(node.GetValue("amount"), out amount);
            if (node.HasValue("maxAmount"))
                double.TryParse(node.GetValue("maxAmount"), out maxAmount);

			Debug.Log ("IonEVAResourceDataLocal.LoadLocal() - amount / maxAmount: " + amount.ToString () + " / " + maxAmount.ToString ());
		}


        /************************************************************************\
         * IonEVAResourceDataLocal class                                        *
         * Save function                                                        *
         *                                                                      *
        \************************************************************************/
        public override void Save(ConfigNode node)
        {
            base.Save(node);
#if DEBUG
            Debug.Log("IonEVAResourceDataLocal.Save()");
#endif
            node.AddValue("amount", amount.ToString());
            node.AddValue("maxAmount", maxAmount.ToString ());
#if DEBUG
            Debug.Log("IonEVAResourceDataLocal.Save(): node\n" + node.ToString());
#endif
        }
        /************************************************************************\
         * IonEVAResourceDataLocal class                                        *
         * SaveLocal function                                                   *
         *                                                                      *
        \************************************************************************/
        public override void SaveLocal(ConfigNode node)
        {
            base.SaveLocal(node);
#if DEBUG
            Debug.Log("IonEVAResourceDataLocal.SaveLocal()");
#endif
            node.AddValue("amount", amount.ToString ());
            node.AddValue("maxAmount", maxAmount.ToString ());
#if DEBUG
            Debug.Log("IonEVAResourceDataLocal.SaveLocal(): node\n" + node.ToString());
#endif
        }
    }
    //==========================================================================================================
    // END of IonEVAResourceDataLocal Class
    //==========================================================================================================

    /*------------------------------------------------------*\
     * IonSupportGenerator Class                            *
     * This classes stores the information related to       *
     * default generators placed in all parts with an       *
     * IonCrewSupportModule attached.                       *
    \*------------------------------------------------------*/
    [Serializable]
    public class IonGeneratorData : IConfigNode
    {
        public string moduleClass;
        public string generatorName;
        public string generatorGUIName;

        public bool startOn;
        public bool alwaysOn;

        public float outputLevelStep;
        public float outputLevelMin;
        public float outputLevelMax;

        public bool hideStatus;
        public bool hideStatusL2;
        public bool hideEfficency;
        public bool hideOutputControls;
        public bool hideActivateControls;

        public List<IonGeneratorResourceData> listInputs;
        public List<IonGeneratorResourceData> listOutputs;
        

        /************************************************************************\
         * IonGeneratorData class                                               *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonGeneratorData()
        {
#if DEBUG
            Debug.Log("IonGeneratorData.Constructor()");
#endif
            Initialize();
        }

        /************************************************************************\
         * IonGeneratorData class                                               *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonGeneratorData(ConfigNode node)
        {
#if DEBUG
            Debug.Log("IonGeneratorData.Constructor(ConfigNode)");
#endif
            Initialize();
            Load(node);
        }

        /************************************************************************\
         * IonGeneratorData class                                               *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonGeneratorData(IonGeneratorData other)
        {
#if DEBUG
            Debug.Log("IonGeneratorData.Constructor(IonGeneratorData)");
#endif
            generatorName = other.generatorName;
            generatorGUIName = other.generatorGUIName;

            startOn = other.startOn;
            alwaysOn = other.alwaysOn;

            outputLevelStep = other.outputLevelStep;
            outputLevelMin = other.outputLevelMin;
            outputLevelMax = other.outputLevelMax;

            hideStatus = other.hideStatus;
            hideStatusL2 = other.hideStatusL2;
            hideEfficency = other.hideEfficency;
            hideOutputControls = other.hideOutputControls;
            hideActivateControls = other.hideActivateControls;

            listInputs = new List<IonGeneratorResourceData>(other.listInputs);
            listOutputs = new List<IonGeneratorResourceData>(other.listOutputs);
        }

        /************************************************************************\
         * IonGeneratorData class                                               *
         * Initialize function                                                   *
         *                                                                      *
         * Assigns default values to variables.                                 *
        \************************************************************************/
        protected virtual void Initialize()
        {
            moduleClass = "IonModuleGenerator";
            generatorName = "";
            generatorGUIName = "";

            startOn = false;
            alwaysOn = false;

            outputLevelStep = 0.1f;
            outputLevelMin = 0.0f;
            outputLevelMax = 1.0f;

            hideStatus = false;
            hideStatusL2 = false;
            hideEfficency = false;
            hideOutputControls = false;
            hideActivateControls = false;
        }


        /************************************************************************\
         * IonGeneratorData class                                               *
         * GetInfo function                                                     *
         *                                                                      *
         * Returns a string containing information on this generator.           *
        \************************************************************************/
        public virtual string GetInfo(int crewCapacity)
        {
#if DEBUG
            Debug.Log("IonGeneratorData.GetInfo() " + generatorName);
#endif
            string strInfo = "";

            strInfo += generatorGUIName + " Information:\n";
            strInfo += GetInfoBasic();
            strInfo += GetInfoLists(crewCapacity);

            return strInfo;
        }

        /************************************************************************\
         * IonGeneratorData class                                               *
         * GetInfoBasic function                                                *
         *                                                                      *
         * Helper function for getInfo.                                         *
        \************************************************************************/
        protected virtual string GetInfoBasic()
        {
            string strInfo = "";

            strInfo += startOn || alwaysOn ? "  - Starts active\n" : "";
            strInfo += alwaysOn ? "  - Cannot be turned off\n" : "";

            if (!hideOutputControls)
            {
                strInfo += "  - Minimum output setting: " + Math.Round(outputLevelMin, 1) + "\n";
                strInfo += "  - Maximum output setting: " + Math.Round(outputLevelMax, 1) + "\n";
            }

            return strInfo;
        }

        /************************************************************************\
         * IonGeneratorData class                                               *
         * GetInfoLists function                                                *
         *                                                                      *
         * Helper function for getInfo.                                         *
        \************************************************************************/
        protected virtual string GetInfoLists(int crewCapacity)
        {
            string strInfo = "";

            if (listInputs.Count > 0)
            {
                strInfo += "  Input Resources\n";
                strInfo += getInfoList(crewCapacity, listInputs, 4);
            }

            if (listOutputs.Count > 0)
            {
                strInfo += "  Output Resources\n";
                strInfo += getInfoList(crewCapacity, listOutputs, 4);
            }

            return strInfo;
        }


        /************************************************************************\
         * IonGeneratorData class                                               *
         * getInfoList function                                                 *
         *                                                                      *
         * Helper function for getInfo.                                         *
        \************************************************************************/
        protected string getInfoList(int crewCapacity, List<IonGeneratorResourceData> list, int indentLevel = 0)
        {
            string strInfo = "";
            float rate;
            string unit;

            foreach (IonGeneratorResourceData resource in list)
            {
                rate = (float)(resource.RateBase + resource.RatePerCapacity * crewCapacity);
                unit = "sec.";

                //adjust displayRate units so the value stays above 0.5
                IonModuleDisplay.setRateUnits(ref rate, ref unit, 0.5f);

                //Indent line
                for (int i = 0; i < indentLevel; i++)
                    strInfo += " ";

                //add info to string
                strInfo += "- " + resource.Name + " (" + Math.Round(rate, 2) + "/" + unit + ")" + (1 == resource.EffectOnEfficency ? " [Required]\n" : "\n");
            }

            return strInfo;
        }


        /************************************************************************\
         * IonGeneratorData class                                               *
         * Load function                                                        *
         *                                                                      *
        \************************************************************************/
        public virtual void Load(ConfigNode node)
        {
#if DEBUG
            Debug.Log("IonGeneratorData.Load()");
            Debug.Log("IonGeneratorData.Load(): node\n" + node.ToString());
#endif
            if (node.HasValue("moduleClass"))
                moduleClass = node.GetValue("moduleClass");

            //Read variables from node
            if (node.HasValue("generatorName"))
                generatorName = node.GetValue("generatorName");
            else if (node.HasValue("name"))
                generatorName = node.GetValue("name");

            if (node.HasValue("generatorGUIName"))
                generatorGUIName = node.GetValue("generatorGUIName");

            if (node.HasValue("startOn"))
                startOn = "True" == node.GetValue("startOn") || "true" == node.GetValue("startOn") || "TRUE" == node.GetValue("startOn");

            if (node.HasValue("alwaysOn"))
                alwaysOn = "True" == node.GetValue("alwaysOn") || "true" == node.GetValue("alwaysOn") || "TRUE" == node.GetValue("alwaysOn");

            //Read output level variables
            if (node.HasValue("outputLevelStep"))
            {
                outputLevelStep = Convert.ToSingle(node.GetValue("outputLevelStep"));
            outputLevelStep = Math.Min(outputLevelStep, 1.0f);
            outputLevelStep = Math.Max(outputLevelStep, 0.0f);
            }

            if (node.HasValue("outputLevelMin"))
            {
                outputLevelMin = Convert.ToSingle(node.GetValue("outputLevelMin"));
            outputLevelMin = Math.Min(outputLevelMin, 1.0f);
            outputLevelMin = Math.Max(outputLevelMin, 0.0f);
            }

            if (node.HasValue("outputLevelMax"))
            {
                outputLevelMax = Convert.ToSingle(node.GetValue("outputLevelMax"));
            outputLevelMax = Math.Min(outputLevelMax, 1.0f);
            outputLevelMax = Math.Max(outputLevelMax, 0.0f);
            }

            //Read hide variables
            if (node.HasValue("hideStatus"))
                hideStatus = "True" == node.GetValue("hideStatus") || "true" == node.GetValue("hideStatus") || "TRUE" == node.GetValue("hideStatus");
            if (node.HasValue("hideStatusL2"))
                hideStatusL2 = "True" == node.GetValue("hideStatusL2") || "true" == node.GetValue("hideStatusL2") || "TRUE" == node.GetValue("hideStatusL2");
            if (node.HasValue("hideEfficency"))
                hideEfficency = "True" == node.GetValue("hideEfficency") || "true" == node.GetValue("hideEfficency") || "TRUE" == node.GetValue("hideEfficency");
            if (node.HasValue("hideOutputControls"))
                hideOutputControls = "True" == node.GetValue("hideOutputControls") || "true" == node.GetValue("hideOutputControls") || "TRUE" == node.GetValue("hideOutputControls");
            if (node.HasValue("hideActivateControls"))
                hideActivateControls = "True" == node.GetValue("hideActivateControls") || "true" == node.GetValue("hideActivateControls") || "TRUE" == node.GetValue("hideActivateControls");


            //Create lists
            listInputs = new List<IonGeneratorResourceData>();
            listOutputs = new List<IonGeneratorResourceData>();

            //Traverse through subNodes and add to lists
            foreach (ConfigNode subNode in node.nodes)
            {
#if DEBUG
                Debug.Log("IonGeneratorData.Load(): subnode " + subNode.ToString());
#endif
                if ("INPUT_RESOURCE" == subNode.name)
                {
                    IonGeneratorResourceData inputResource = new IonGeneratorResourceData(subNode);
                    listInputs.Add(inputResource);
                }
                else if ("OUTPUT_RESOURCE" == subNode.name)
                {
                    IonGeneratorResourceData outputResource = new IonGeneratorResourceData(subNode);
                    listOutputs.Add(outputResource);
                }
            }
        }

        /************************************************************************\
         * IonGeneratorData class                                               *
         * Save function                                                        *
         *                                                                      *
        \************************************************************************/
        public virtual void Save(ConfigNode node)
        {
#if DEBUG
            Debug.Log("IonGeneratorData.Save()");
#endif
            node.AddValue("moduleClass", moduleClass);
            node.AddValue("generatorName", generatorName);
            node.AddValue("generatorGUIName", generatorGUIName);
            
            node.AddValue("startOn", startOn);
            node.AddValue("alwaysOn", alwaysOn);

            node.AddValue("outputLevelStep", outputLevelStep);
            node.AddValue("outputLevelMin", outputLevelMin);
            node.AddValue("outputLevelMax", outputLevelMax);

            node.AddValue("hideStatus", hideStatus);
            node.AddValue("hideStatusL2", hideStatusL2);
            node.AddValue("hideEfficency", hideEfficency);
            node.AddValue("hideOutputControls", hideOutputControls);
            node.AddValue("hideActivateControls", hideActivateControls);

            foreach (IonGeneratorResourceData inputResource in listInputs)
            {
                ConfigNode inputNode = new ConfigNode("INPUT_RESOURCE");
                inputResource.Save(inputNode);
            }

            foreach (IonGeneratorResourceData outputResource in listOutputs)
            {
                ConfigNode outputNode = new ConfigNode("OUTPUT_RESOURCE");
                outputResource.Save(outputNode);
            }
#if DEBUG
            Debug.Log("IonGeneratorData.Save(): node\n" + node.ToString());
#endif
        }
    }
    //==========================================================================================================
    // END of IonGeneratorData Class
    //==========================================================================================================



    /*------------------------------------------------------*\
     * IonSupportCollector Class                            *
     * This classes stores the information related to       *
     * default collectors placed in all parts with an       *
     * IonCrewSupportModule attached.                       *
    \*------------------------------------------------------*/
    [Serializable]
    public class IonCollectorData : IonGeneratorData
    {
        public float minAtmosphere;

        public bool isAutomaticOxygen;
        public bool isAutomaticNoOxygen;

        public bool hideAtmoContents;

        public List<IonGeneratorResourceData> listOutputs_oxygen;
        public List<IonGeneratorResourceData> listOutputs_noOxygen;
        

        /************************************************************************\
         * IonCollectorData class                                               *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonCollectorData() :base()
        {
#if DEBUG
            Debug.Log("IonCollectorData.Constructor()");
#endif
        }

        /************************************************************************\
         * IonCollectorData class                                               *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonCollectorData(ConfigNode node) : base(node)
        {
#if DEBUG
            Debug.Log("IonCollectorData.Constructor(ConfigNode)");
#endif
            Load(node);
        }

        /************************************************************************\
         * IonCollectorData class                                               *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IonCollectorData(IonCollectorData other) : base(other)
        {
#if DEBUG
            Debug.Log("IonCollectorData.Constructor(IonCollectorData)");
#endif
            minAtmosphere = other.minAtmosphere;

            isAutomaticOxygen = other.isAutomaticOxygen;
            isAutomaticNoOxygen = other.isAutomaticNoOxygen;

            hideAtmoContents = other.hideAtmoContents;

            listOutputs_oxygen = new List<IonGeneratorResourceData>(other.listOutputs_oxygen);
            listOutputs_noOxygen = new List<IonGeneratorResourceData>(other.listOutputs_noOxygen);
        }

        /************************************************************************\
         * IonCollectorData class                                               *
         * Initialize function                                                   *
         *                                                                      *
         * Assigns default values to variables.                                 *
        \************************************************************************/
        protected override void Initialize()
        {
            base.Initialize();

            moduleClass = "IonModuleCollector";
            minAtmosphere = 0.0f;
            isAutomaticOxygen = false;
            isAutomaticNoOxygen = false;
            hideAtmoContents = false;
        }


        /************************************************************************\
         * IonSupportCollector class                                            *
         * GetInfoBasic function                                                *
         *                                                                      *
         * Helper function for getInfo.                                         *
        \************************************************************************/
        protected override string GetInfoBasic()
        {
            string strInfo = base.GetInfoBasic();

            strInfo += "  - Minimum atmosphere density: " + Math.Round(minAtmosphere, 2) + "\n";
            strInfo += isAutomaticOxygen ? "  - Automaticaly turns on in oxygen atmosphere\n" : "";
            strInfo += isAutomaticNoOxygen ? "  - Automaticaly turns on in non-oxygen atmosphere\n" : "";

            return strInfo;
        }

        /************************************************************************\
         * IonSupportCollector class                                            *
         * GetInfoLists function                                                *
         *                                                                      *
         * Helper function for getInfo.                                         *
        \************************************************************************/
        protected override string GetInfoLists(int crewCapacity)
        {
            string strInfo = base.GetInfoLists(crewCapacity);

            if (listOutputs_oxygen.Count > 0)
            {
                strInfo += "  Output Resources (Oxygen)\n";
                strInfo += getInfoList(crewCapacity, listOutputs_oxygen, 4);
            }

            if (listOutputs_noOxygen.Count > 0)
            {
                strInfo += "  Output Resources (Non-Oxygen)\n";
                strInfo += getInfoList(crewCapacity, listOutputs_noOxygen, 4);
            }

            return strInfo;
        }


        /************************************************************************\
         * IonSupportCollector class                                            *
         * Load function                                                        *
         *                                                                      *
        \************************************************************************/
        public override void Load(ConfigNode node)
        {
            base.Load(node);
#if DEBUG
            Debug.Log("IonSupportCollector.Load()");
            Debug.Log("IonSupportCollector.Load(): node\n" + node.ToString());
#endif
            //Read variables from node
            if (node.HasValue("minAtmosphere"))
                minAtmosphere = Convert.ToSingle(node.GetValue("minAtmosphere"));

            if (node.HasValue("isAutomaticOxygen"))
                isAutomaticOxygen = "True" == node.GetValue("isAutomaticOxygen") || "true" == node.GetValue("isAutomaticOxygen") || "TRUE" == node.GetValue("isAutomaticOxygen");

            if (node.HasValue("isAutomaticNoOxygen"))
                isAutomaticNoOxygen = "True" == node.GetValue("isAutomaticNoOxygen") || "true" == node.GetValue("isAutomaticNoOxygen") || "TRUE" == node.GetValue("isAutomaticNoOxygen");

            if (node.HasValue("hideAtmoContents"))
                hideAtmoContents = "True" == node.GetValue("hideAtmoContents") || "true" == node.GetValue("hideAtmoContents") || "TRUE" == node.GetValue("hideAtmoContents");


            //Create lists
            listOutputs_oxygen = new List<IonGeneratorResourceData>();
            listOutputs_noOxygen = new List<IonGeneratorResourceData>();

            //Traverse through subNodes and add to lists
            foreach (ConfigNode subNode in node.nodes)
            {
#if DEBUG
                Debug.Log("IonSupportCollector.Load(): subnode " + subNode.ToString());
#endif
                if ("OUTPUT_RESOURCE_OXYGEN" == subNode.name)
                {
                    IonGeneratorResourceData outputResource = new IonGeneratorResourceData(subNode);
                    listOutputs_oxygen.Add(outputResource);
                }
                else if ("OUTPUT_RESOURCE_NO_OXYGEN" == subNode.name)
                {
                    IonGeneratorResourceData outputResource = new IonGeneratorResourceData(subNode);
                    listOutputs_noOxygen.Add(outputResource);
                }
            }
        }

        /************************************************************************\
         * IonSupportGenerator class                                            *
         * Save function                                                        *
         *                                                                      *
        \************************************************************************/
        public override void Save(ConfigNode node)
        {
            base.Save(node);
#if DEBUG
            Debug.Log("IonSupportCollector.Save()");
#endif
            node.AddValue("minAtmosphere", minAtmosphere);
            node.AddValue("isAutomaticOxygen", isAutomaticOxygen);
            node.AddValue("isAutomaticNoOxygen", isAutomaticNoOxygen);
            node.AddValue("hideAtmoContents", hideAtmoContents);

            foreach (IonGeneratorResourceData outputResource in listOutputs_oxygen)
            {
                ConfigNode outputNode = new ConfigNode("OUTPUT_RESOURCE_OXYGEN");
                outputResource.Save(outputNode);
            }

            foreach (IonGeneratorResourceData outputResource in listOutputs_noOxygen)
            {
                ConfigNode outputNode = new ConfigNode("OUTPUT_RESOURCE_NO_OXYGEN");
                outputResource.Save(outputNode);
            }
#if DEBUG
            Debug.Log("IonSupportCollector.Save(): node\n" + node.ToString());
#endif
        }
    }
    //==========================================================================================================
    // END of IonSupportCollector Class
    //==========================================================================================================
}