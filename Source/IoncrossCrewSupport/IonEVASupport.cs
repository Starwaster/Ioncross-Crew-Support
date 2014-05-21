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

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class IoncrossEVAController : UnityEngine.MonoBehaviour
    {
        private Vessel curVessel;
        private Vessel oldVessel;

        private bool vesselisEVA;

        public void Awake()
        {
#if DEBUG
            Debug.Log("IoncrossEVAController.Awake()");
#endif
        }


        public void Update()
        {
#if DEBUG_UPDATES
            Debug.Log("IoncrossEVAController.Update()");
#endif

            curVessel = FlightGlobals.ActiveVessel;

            //if this is a new vessel
            if(null != curVessel && null != oldVessel && oldVessel != curVessel)
            {
#if DEBUG
                Debug.Log("IoncrossEVAController.Update(): Vessel switched from " + oldVessel.vesselName + " to " + curVessel.vesselName + " (" + curVessel.id + ")");
#endif
                if (curVessel.isEVA)
                {
#if DEBUG
                    Debug.Log("IoncrossEVAController.Update(): This is an EVA vessel");
#endif
                    vesselisEVA = true;

                    IonModuleEVASupport evaModule = null;
                    foreach (PartModule module in curVessel.rootPart.Modules)
                    {
                        if (module is IonModuleEVASupport)
                        {
                            evaModule = (IonModuleEVASupport)module;
                            break;
                        }
                    }

                    if (null == evaModule)
                    {
                        evaModule = CreateEVA(curVessel.rootPart);
                    }

                    if (!evaModule.evaIntilized)
                    {
                        InitilizeEVA(evaModule);
                    }
                }
            }

            //if this is a new vessel and the old vessel was EVA but is now null
            if (null != curVessel && null == oldVessel && !curVessel.isEVA && vesselisEVA)
            {
#if DEBUG
                Debug.Log("IoncrossEVAController.Update(): Old Vessel was EVA");
                Debug.Log("IoncrossEVAController.Update(): Vessel switched from " + "null" + " to " + curVessel.vesselName + " (" + curVessel.id + ")");
#endif
            }

            if (null != curVessel && curVessel.isEVA)
            {
                if (curVessel.isEVA)
                {
                    //save current eva resources
                }
                else
                {
                    vesselisEVA = false;
                }
            }

            oldVessel = curVessel;
        }



        public void OnGUI()
        {
#if DEBUG_UPDATES
            Debug.Log("IoncrossEVAController.OnGUI()");
#endif
        }

        private IonModuleEVASupport CreateEVA(Part evaPart)
        {
#if DEBUG
            Debug.Log("IoncrossEVAController.CreateEVA()");
#endif
            IonModuleEVASupport evaModule;
            evaModule = (IonModuleEVASupport)(evaPart.AddModule("IonModuleEVASupport"));
            evaModule.lastLoaded = Planetarium.GetUniversalTime();
            evaModule.evaIntilized = false;

            return evaModule;
        }

        private void InitilizeEVA(IonModuleEVASupport evaModule)
        {
#if DEBUG
            Debug.Log("IoncrossEVAController.InitilizeEVA()");
#endif
            evaModule.evaStartTime = Planetarium.GetUniversalTime();

            foreach (IonSupportResourceDataGlobal supportResource in IoncrossController.Instance.Settings.ListSupportResources)
            {
#if DEBUG
                Debug.Log("IoncrossEVAController.InitilizeEVA(): processing  ION_SUPPORT_RESOURCE node\n" + supportResource.Name);
#endif
                if (supportResource.EVAamount > 0)
                {
#if DEBUG
                    Debug.Log("IoncrossEVAController.InitilizeEVA(): adding " + supportResource.EVAamount + " of " + supportResource.Name);
#endif
                    IonEVAResourceDataLocal EVAResource = new IonEVAResourceDataLocal(supportResource);

                    EVAResource.MaxAmount = supportResource.EVAamount;
                    EVAResource.Amount = Math.Min(CollectResource(supportResource.ID, supportResource.EVAamount, oldVessel), supportResource.EVAamount);

                    evaModule.AddResource(EVAResource);
                }
            }

            evaModule.evaIntilized = true;
        }


        private double CollectResource(int resourceID, double amount, Vessel oldVessel)
        {
#if DEBUG
            Debug.Log("IoncrossEVAController.CollectResource()");
#endif
            double collectedAmount = 0;

            foreach (Part part in oldVessel.Parts)
            {
                collectedAmount += part.RequestResource(resourceID, amount - collectedAmount);
                if (collectedAmount >= amount)
                    break;
            }

            return collectedAmount;
        }
    }



    /*======================================================*\
     * IonModuleEVA Class                                   *
     * When attached to the Kerbal EVA part this module     *
     * handles resource consumption and wates of the kerbal *
    \*======================================================*/
    public class IonModuleEVASupport : IonModuleBase
    {
        public bool evaIntilized = false;
        public double evaStartTime = -1;
        public List<ConfigNode> listResourceNodes;
        public List<IonResourceData> listEVAResources;

        /************************************************************************\
         * IonModuleEVASupport class                                            *
         * OnAwake function override                                            *
         *                                                                      *
        \************************************************************************/
        public override void OnAwake()
        {
            base.OnAwake();
#if DEBUG
            Debug.Log("IonModuleEVASupport.OnAwake() " + this.part.name);
#endif
        }


        /************************************************************************\
         * IonModuleEVASupport class                                            *
         * OnLoad function override                                             *
         *                                                                      *
        \************************************************************************/
        public override void OnLoad(ConfigNode node)
        {
            //Create lists
            if (null == listResourceNodes)
            {
                Debug.Log("IonModuleEVASupport.OnLoad(): listResourceNodes is null, creating new");
                listResourceNodes = new List<ConfigNode>();
            }
            if (null == listEVAResources)
            {
                Debug.Log("IonModuleEVASupport.OnLoad(): listEVAResources is null, creating new");
                listEVAResources = new List<IonResourceData>();
            }

            base.OnLoad(node);
#if DEBUG
            Debug.Log("IonModuleEVASupport.OnLoad() " + this.part.name);
            Debug.Log("IonModuleEVASupport.OnLoad(): node\n" + node.ToString());
#endif
            if(node.HasValue("evaIntilized"))
                evaIntilized = "True" == node.GetValue("evaIntilized") || "true" == node.GetValue("evaIntilized") || "TRUE" == node.GetValue("evaIntilized");

            if (node.HasValue("evaStartTime"))
                evaStartTime = Convert.ToDouble(node.GetValue("evaStartTime"));

            IonEVAResourceDataLocal evaResourceL;

            //Copy data from global lists into local lists for modifications
            foreach (IonSupportResourceDataGlobal supportResourceG in IoncrossController.Instance.Settings.ListSupportResources)
            {
                if (supportResourceG.EVAamount >= 0)
                {
#if DEBUG
                    Debug.Log("IonModuleEVASupport.OnLoad(): creating local resource for " + supportResourceG.Name);
#endif
                    evaResourceL = (IonEVAResourceDataLocal)listEVAResources.Find(delegate(IonResourceData resource) { return resource.Name == supportResourceG.Name; });
                    if (null != evaResourceL)
                    {
                        evaResourceL.Data = supportResourceG;
                    }
                    else
                    {
                        evaResourceL = new IonEVAResourceDataLocal(supportResourceG);
                        listEVAResources.Add(evaResourceL);
                    }

                }
            }

            //Read and process nodes adding modifications to local lists
            foreach (ConfigNode subNode in node.nodes)
            {
#if DEBUG
                Debug.Log("IonModuleEVASupport.OnLoad(): processing subNode " + subNode.name);
#endif
                //Check if the subNode corresponds to a list
                List<IonResourceData> curList = GetCorrespondingList(subNode.name);
                if (null != curList)
                {
                    //Add node to listResourceNodes (for later processing)
                    listResourceNodes.Add(subNode);

                    //Process node to add data to correct input/output list
                    ProcessNodetoList(subNode);
                }
            }
        }


        /************************************************************************\
         * IonModuleEVASupport class                                            *
         * OnSave function override                                             *
         *                                                                      *
        \************************************************************************/
        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
#if DEBUG
            Debug.Log("IonModuleEVASupport.OnSave() " + this.part.name);
#endif
            node.AddValue("evaIntilized", evaIntilized);
            node.AddValue("evaStartTime", evaStartTime);

            //Save contents of listEVAResources
            if (null != listEVAResources)
            {
                foreach (IonEVAResourceDataLocal evaResource in listEVAResources)
                {
#if DEBUG
                    Debug.Log("IonModuleEVASupport.OnSave(): saving resource " + evaResource.Name);
#endif
                    ConfigNode subNode = new ConfigNode("ION_SUPPORT_RESOURCE");
                    evaResource.SaveLocal(subNode);
                    node.AddNode(subNode);
                }
            }
#if DEBUG
            Debug.Log("IonModuleEVASupport.OnSave(): node\n" + node.ToString());
#endif
        }


        /************************************************************************\
         * IonModuleEVASupport class                                            *
         * OnStart function override                                            *
         *                                                                      *
        \************************************************************************/
        public override void OnStart(PartModule.StartState state)
        {
            //Reprocess and clear listResourceNodes, if necessary
            if (null == listEVAResources)
            {
                listEVAResources = new List<IonResourceData>();
                ProcessNodestoList(listResourceNodes);
            }
            listResourceNodes = null;

            base.OnStart(state);
#if DEBUG
            Debug.Log("IonModuleEVASupport.OnStart() " + this.part.name);
            Debug.Log("IonModuleEVASupport.OnStart(): state " + state.ToString());
#endif


            //Attach display modules
            foreach (IonResourceData resource in listEVAResources)
            {
                resource.DisplayModule = IonModuleDisplay.findDisplayModule(this.part, resource);
                resource.DisplayModule.SetGUIName(resource.Name);
                resource.DisplayModule.isRate = false;
                resource.DisplayModule.SetUnits("%");
                resource.DisplayModule.SetFormat("F1");

            }
        }


        /************************************************************************\
         * IonModuleEVASupport class                                            *
         * OnUpdate function override                                           *
         *                                                                      *
        \************************************************************************/
        public override void OnUpdate()
        {
            base.OnUpdate();
#if DEBUG_UPDATES
            Debug.Log("IonModuleEVASupport.OnUpdate() " + this.part.name);
#endif
            bool allResourcesMet = true;
            allResourcesMet = ConsumeResources(TimeWarp.deltaTime);
        }


        /************************************************************************\
         * IonModuleEVASupport class                                            *
         * Initilize function                                                   *
         *                                                                      *
        \************************************************************************/
        protected override void FirstUpdateInitilize()
        {
            base.FirstUpdateInitilize();
#if DEBUG
            Debug.Log("IonModuleEVASupport.FirstUpdateInitilize() " + this.part.name);
#endif
        }


        /************************************************************************\
         * IonModuleEVASupport class                                            *
         * ConsumeResources function                                            *
         *                                                                      *
         * Runs the module's resource use simulation over deltaTime.            *
        \************************************************************************/
        public override bool ConsumeResources(double deltaTime)
        {
#if DEBUG_UPDATES
            Debug.Log("IonModuleEVASupport.ConsumeResources() " + this.part.name);
#endif
            bool allResourcesMet = true;

            double resourceRequest;
            double resourceReturn;

            foreach (IonEVAResourceDataLocal evaResource in listEVAResources)
            {
                resourceRequest = (evaResource.RatePerKerbal) * deltaTime;
                resourceReturn = RequestResource(evaResource.ID, resourceRequest);

                evaResource.DisplayModule.displayRate = (float)(evaResource.Amount / evaResource.MaxAmount) * 100.0f;

#if DEBUG_UPDATES
                Debug.Log("IonModuleEVASupport.ConsumeResources(): requesting " + resourceRequest + " of " + evaResource.Name);
                Debug.Log("IonModuleEVASupport.ConsumeResources(): returning " + resourceReturn + " of " + evaResource.Name);
#endif

                //Check if all resources were met and if its been long enough without them to trigger any effects
                if (Math.Abs(resourceRequest - resourceReturn) * 1000 > Math.Abs(resourceRequest))
                {
#if DEBUG_UPDATES
                    Debug.Log("IonModuleEVASupport.ConsumeResources(): return did not meet request for " + evaResource.Name);
#endif
                    allResourcesMet = false;

                    if (evaResource.CauseDeath)
                    {
                        evaResource.FramesWithoutResource++;
                        evaResource.TimeSinceLastKillRoll += deltaTime;

                        //End timewarp if resources are low
                        //if (evaResource.FramesWithoutResource > IoncrossController.Instance.Settings.KillResources_MinFramesWarning && TimeWarp.CurrentRateIndex > 0)
                        //{
                        //    evaResource.Low = true;
                        //    TimeWarp.SetRate(TimeWarp.CurrentRateIndex - 1, false);
                        //}
#if DEBUG
                        Debug.Log("IonModuleEVASupport.ConsumeResources(): framesWithoutResource " + evaResource.FramesWithoutResource + " | timeSinceLastKillRoll " + evaResource.TimeSinceLastKillRoll);
#endif

                        if (evaResource.TimeSinceLastKillRoll > evaResource.Data.KillRollInterval && evaResource.FramesWithoutResource > IoncrossController.Instance.Settings.KillResources_MinFramesKill)
                        {
#if DEBUG_UPDATES
                            Debug.Log("IonModuleEVASupport.ConsumeResources(): kill crew roll for low " + evaResource.Name + " levels!");
#endif
                            KillCrewRoll(evaResource.KillChance);
                            evaResource.TimeSinceLastKillRoll = 0;
                            evaResource.FramesWithoutResource = 0;
                        }
                    }
                }

                else
                {
                    evaResource.FramesWithoutResource = 0;
                    evaResource.Low = false;
                }
            }

            return allResourcesMet;
        }


        /************************************************************************\
         * IonModuleEVASupport class                                            *
         * ConsumeResourcesQuick function                                       *
         *                                                                      *
         * Runs a quicker aproximation of the module's resource use simulation  *
         * over deltaTime.                                                      *
        \************************************************************************/
        public override void ConsumeResourcesQuick(double deltaTime)
        {
#if DEBUG_UPDATES
            Debug.Log("IonModuleEVASupport.ConsumeResourcesQuick() " + this.part.name);
#endif
            double resourceRequest;
            double resourceReturn;

            foreach (IonEVAResourceDataLocal evaResource in listEVAResources)
            {
                resourceRequest = (evaResource.RatePerKerbal) * deltaTime;
                resourceReturn = RequestResource(evaResource.ID, resourceRequest);
#if DEBUG_UPDATES
                Debug.Log("IonModuleEVASupport.ConsumeResourceQuick(): requesting " + resourceRequest + " of " + evaResource.Name);
                Debug.Log("IonModuleEVASupport.ConsumeResourceQuick(): returning " + resourceReturn + " of " + evaResource.Name);
#endif
                //Skip check if the request was met
            }
        }


        /************************************************************************\
         * IonModuleEVASupport class                                            *
         * KillCrewRoll function                                                *
         *                                                                      *
        \************************************************************************/
        protected void KillCrewRoll(float fDeathChance)
        {
#if DEBUG
            Debug.Log("IonModuleEVASupport.KillCrewRoll() " + this.part.name);
#endif
            float rand;
            for (int i = 0; i < this.part.protoModuleCrew.Count; i++)
            {
                rand = UnityEngine.Random.Range(0.0f, 100.0f);
#if DEBUG
                Debug.Log("IonModuleEVASupport.KillCrewRoll(): i = " + i + " | rand = " + rand + " | kill chance = " + fDeathChance);
#endif
                if (fDeathChance * 100 > rand)
                {
                    RemoveCrew(part.protoModuleCrew[i--]);
                }
            }
        }

        /************************************************************************\
         * IonModuleEVASupport class                                            *
         * RemoveCrew function                                                  *
         *                                                                      *
        \************************************************************************/
        private void RemoveCrew(ProtoCrewMember crew)
        {
#if DEBUG
            Debug.Log("IonModuleEVASupport.RemoveCrew() " + this.part.name);
            Debug.Log("IonModuleEVASupport.RemoveCrew(): killing crew " + crew.ToString());
#endif
            crew.Die();
            this.part.protoModuleCrew.Remove(crew);
            if (this.part.protoModuleCrew.Count == 0)
            {
                this.part.explode();
            }
        }


        /************************************************************************\
         * IonModuleEVASupport class                                            *
         * GetResourceUsage function                                            *
         *                                                                      *
         * Calculates the modules resource usage over deltaTime and returns a   *
         * list of all resources used and their amounts.                        *
        \************************************************************************/
        public override List<IonResourceData> GetResources()
        {
#if DEBUG
            Debug.Log("IonModuleEVASupport.GetResources() " + this.part.name);
#endif
            List<IonResourceData> listResources = new List<IonResourceData>();

            foreach (IonEVAResourceDataLocal evaResource in listEVAResources)
            {
                //IonResourceData resourceData = new IonResourceData(evaResource);
                //listResources.Add(resourceData);
                listResources.Add(evaResource);
#if DEBUG
                Debug.Log("IonModuleEVASupport.GetResources(): adding " + evaResource.Name);
#endif
            }
            return listResources;
        }


        /************************************************************************\
         * IonModuleCrewSupport class                                           *
         * GetCorrespondingList function                                        *
         *                                                                      *
         * Returns the list that corresponds to nodeName.                       *
         * Returns null if there is no corresponding list.                      *
        \************************************************************************/
        public override List<IonResourceData> GetCorrespondingList(string nodeName)
        {
            if ("SUPPORT_RESOURCE" == nodeName)
                return listEVAResources;

            return null;
        }
        public override IonResourceData CreateResourceEntry(ConfigNode node)
        {
            return new IonEVAResourceDataLocal(node);
        }


        /************************************************************************\
         * IonModuleEVA class                                                   *
         * AddResource function                                                 *
         *                                                                      *
        \************************************************************************/
        public void AddResource(IonEVAResourceDataLocal newResource)
        {
#if DEBUG
            Debug.Log("IonModuleEVASupport.AddResource() " + this.part.name);
            Debug.Log("IonModuleEVASupport.AddResource(): resource\n" + newResource.ToString());
#endif
            IonEVAResourceDataLocal evaResource = new IonEVAResourceDataLocal(newResource);

            //Attach display module
            evaResource.DisplayModule = IonModuleDisplay.findDisplayModule(this.part, newResource);
            evaResource.DisplayModule.SetGUIName(evaResource.Name);
            evaResource.DisplayModule.isRate = false;
            evaResource.DisplayModule.SetUnits("%");
            evaResource.DisplayModule.SetFormat("F1");

            listEVAResources.Add(evaResource);
        }


        /************************************************************************\
         * IonModuleEVA class                                                   *
         * RequestResource function                                             *
         *                                                                      *
        \************************************************************************/
        public override double RequestResource(string resourceName, double resourceAmount)
        {
            return RequestResource(resourceName.GetHashCode(), resourceAmount);
        }

        /************************************************************************\
         * IonModuleEVA class                                                   *
         * RequestResource function                                             *
         *                                                                      *
        \************************************************************************/
        public override double RequestResource(int resourceID, double resourceAmount)
        {
#if DEBUG_UPDATES
            Debug.Log("IonModuleEVASupport.RequestResource() " + this.part.name);
            Debug.Log("IonModuleEVASupport.RequestResource(): request for " + resourceAmount + " units of " + resourceID);
#endif
            double amount = 0;

            foreach (IonEVAResourceDataLocal evaResource in listEVAResources)
            {
                if (evaResource.ID == resourceID)
                {
                    if(resourceAmount > 0)
                    {
                        amount = Math.Min(resourceAmount, evaResource.Amount);
#if DEBUG_UPDATES
                        Debug.Log("IonModuleEVASupport.RequestResource(): resourceAmount " + resourceAmount + " | evaResource.amount " + evaResource.Amount);
                        Debug.Log("IonModuleEVASupport.RequestResource(): amount " + amount);
#endif
                    }
                    else
                    {
                        amount = Math.Max(resourceAmount, evaResource.Amount - evaResource.MaxAmount);
#if DEBUG_UPDATES
                        Debug.Log("IonModuleEVASupport.RequestResource(): resourceAmount " + resourceAmount + " | evaResource.maxAmount - evaResource.amount " + (evaResource.MaxAmount - evaResource.Amount));
                        Debug.Log("IonModuleEVASupport.RequestResource(): amount " + amount);
#endif
                    }

                    evaResource.Amount -= amount;
                }
            }

#if DEBUG_UPDATES
            Debug.Log("IonModuleEVASupport.RequestResource(): returning " + amount);
#endif
            return amount;
        }
    }
}