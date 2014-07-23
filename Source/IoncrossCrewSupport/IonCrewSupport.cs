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
    /*======================================================*\
     * IonModuleCrewSupport Class                           *
     * When attached to a crewed part this module handles   *
     * resource consumption and wates of both the kerbals   *
     * in the part and the part itselft.                    *
    \*======================================================*/
    public class IonModuleCrewSupport : IonModuleBase
    {
        public List<ConfigNode> listResourceNodes;
        public List<ConfigNode> listPodGenNodes;
        public List<IonResourceData> listSupportResources;
        public List<IonGeneratorData> listPodGenerators;
        //public List<IonCollectorData> listPodCollectors;

        public ModuleCommand moduleCommand;

        
        /******************\
         * Display Fields *
        \******************/
        [KSPField(guiActive = true, guiName = "Life Support Status", isPersistant = false)]
        string lifeSupportStatus;

        [KSPField(guiActive = true, guiName = " ", isPersistant = false)]
        string lifeSupportStatusL2;


        /************************************************************************\
         * IonModuleCrewSupport class                                           *
         * GetInfo function override                                            *
         *                                                                      *
        \************************************************************************/
        public override string GetInfo()
        {
#if DEBUG
            Debug.Log("IonModuleCrewSupport.GetInfo() " + this.part.name);
            Debug.Log("IonModuleCrewSupport.GetInfo(): listSupportResources " + (null == listSupportResources ? "null" : listSupportResources.Count.ToString()));
#endif
            string strInfo = "";
            float ratePart, rateKerbal;
            string unitPart, unitKerbal;

            strInfo += "Ioncross Crew Support Systems Installed\n";
            strInfo += "  - crew capacity " + this.part.CrewCapacity + "\n";
            strInfo += "  Resource Usage\n";

            foreach(IonSupportResourceDataLocal supportResource in listSupportResources)
            {
                ratePart = (float)(supportResource.RateBase * supportResource.RateBaseMod + supportResource.RatePerCapacity * supportResource.RatePerCapacityMod * this.part.CrewCapacity);
                rateKerbal = (float)(supportResource.RatePerKerbal * supportResource.RatePerKerbalMod);
                unitPart = "sec.";
                unitKerbal = "sec.";

                //adjust displayRate units so the value stays above 0.5
                IonModuleDisplay.setRateUnits(ref ratePart, ref unitPart, 0.5f);
                IonModuleDisplay.setRateUnits(ref rateKerbal, ref unitKerbal, 0.5f);

                if (0 != ratePart)
                    strInfo += "    - " + supportResource.Name + " (" + Math.Round(ratePart, 1) + "/" + unitPart + ")\n";

                if (0 != rateKerbal)
                    strInfo += "    - " + supportResource.Name + " (" + Math.Round(rateKerbal, 1) + "/" + unitKerbal + ") per kerbal\n";
#if DEBUG
                Debug.Log("IonModuleCrewSupport.GetInfo(): setting up info for " + supportResource.Name + " \"" + strInfo + "\"");
#endif
            }

            foreach(IonGeneratorData generator in listPodGenerators)
            {
                strInfo += "\n";
                strInfo += generator.GetInfo(this.part.CrewCapacity);
            }

            //foreach (IonCollectorData collector in listPodCollectors)
            //{
            //    strInfo += "\n";
            //    strInfo += collector.GetInfo(this.part.CrewCapacity);
            //}

            return strInfo;
        }

        /************************************************************************\
         * IonModuleCrewSupport class                                           *
         * OnAwake function override                                            *
         *                                                                      *
        \************************************************************************/
        public override void OnAwake()
        {
            base.OnAwake();
#if DEBUG
            Debug.Log("IonModuleCrewSupport.OnAwake() " + this.part.name);
#endif
        }

        /************************************************************************\
         * IonModuleCrewSupport class                                           *
         * InitializeValues function                                             *
         *                                                                      *
        \************************************************************************/
        
        public override void InitializeValues()
        {
            base.InitializeValues();
#if DEBUG
            Debug.Log("IonModuleCrewSupport.InitializeValues() " + this.part.name);
#endif
        }
        

        /************************************************************************\
         * IonModuleCrewSupport class                                           *
         * OnLoad function override                                             *
         *                                                                      *
        \************************************************************************/
        public override void OnLoad(ConfigNode node)
        {
            //Create lists
            if (null == listResourceNodes)
            {
                Debug.Log("IonModuleCrewSupport.OnLoad(): listResourceNodes is null, creating new");
                listResourceNodes = new List<ConfigNode>();
            }
            if (null == listPodGenNodes)
            {
                Debug.Log("IonModuleCrewSupport.OnLoad(): listPodGenNodes is null, creating new");
                listPodGenNodes = new List<ConfigNode>();
            }
            if (null == listSupportResources)
            {
                Debug.Log("IonModuleCrewSupport.OnLoad(): listSupportResources is null, creating new");
                listSupportResources = new List<IonResourceData>();
            }
            if (null == listPodGenerators)
            {
                Debug.Log("IonModuleCrewSupport.OnLoad(): listPodGenerators is null, creating new");
                listPodGenerators = new List<IonGeneratorData>();
            }

            base.OnLoad(node);
#if DEBUG
            Debug.Log("IonModuleCrewSupport.OnLoad() " + this.part.name);
            Debug.Log("IonModuleCrewSupport.OnLoad(): node\n" + node.ToString());
#endif
            //Copy data from global lists into local lists
            CopyGlobalLists();
            
            //Read and process nodes adding modifications to local lists
            foreach (ConfigNode subNode in node.nodes)
            {
#if DEBUG
                Debug.Log("IonModuleCrewSupport.OnLoad(): processing subNode " + subNode.name);
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

                else if ("ION_POD_GENERATOR" == subNode.name || "ION_POD_COLLECTOR" == subNode.name)
                {
#if DEBUG
                    Debug.Log("IonModuleCrewSupport.OnLoad(): reading modified pod generator node\n" + subNode.ToString());
#endif
                    IonGeneratorData generatorDataL;
                    generatorDataL = listPodGenerators.Find(delegate(IonGeneratorData data) { return data.generatorName == subNode.GetValue("generatorName"); });
                    if (null != generatorDataL)
                    {
                        generatorDataL.Load(subNode);
                    }
                    else
                    {
                        Debug.Log("IonModuleCrewSupport.OnLoad(): ERROR - modified " + subNode.name + " entry in IonModuleCrewSupport does not exist to be modified.");
                    }
                }
            }// end of process nodes
        }


        /************************************************************************\
         * IonModuleCrewSupport class                                           *
         * OnSave function override                                             *
         *                                                                      *
        \************************************************************************/
        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
#if DEBUG
            Debug.Log("IonModuleCrewSupport.OnSave() " + this.part.name);
#endif
            //Save contents of listResources
            if (null != listSupportResources)
            {
                foreach (IonSupportResourceDataLocal supportResource in listSupportResources)
                {
                    ConfigNode subNode = new ConfigNode("ION_SUPPORT_RESOURCE");
                    supportResource.SaveLocal(subNode);
                    node.AddNode(subNode);
                }
            }
#if DEBUG
            Debug.Log("IonModuleCrewSupport.OnSave(): node\n" + node.ToString());
#endif
        }


        /************************************************************************\
         * IonModuleCrewSupport class                                           *
         * OnStart function override                                            *
         *                                                                      *
        \************************************************************************/
        public override void OnStart(PartModule.StartState state)
        {
            //Reprocess and clear listResourceNodes, if necessary
            if (null == listSupportResources || null == listPodGenerators)
            {
                listSupportResources = new List<IonResourceData>();
                listPodGenerators = new List<IonGeneratorData>();

                CopyGlobalLists();

                ProcessNodestoList(listResourceNodes);
            }
            listResourceNodes = null;

            base.OnStart(state);
#if DEBUG
            Debug.Log("IonModuleCrewSupport.OnStart() " + this.part.name);
            Debug.Log("IonModuleCrewSupport.OnStart(): state " + state.ToString());
#endif
            //Add modified pod generators from listPodGenerators
            foreach (IonGeneratorData generatorData in listPodGenerators)
            {
#if DEBUG
                Debug.Log("IonModuleCrewSupport.OnStart(): processing " + generatorData.generatorName);
#endif
                IonModuleGenerator generatorModule = (IonModuleGenerator)IonModuleGenerator.findGeneratorModule(this.part, generatorData);
                if (null != generatorModule)
                {
                    generatorModule.Load(generatorData);
                    generatorModule.lastLoaded = this.lastLoaded;
                }
                else
                {
                    Debug.Log("IonModuleCrewSupport.OnStart(): ERROR - " + generatorData.generatorName + " generator not found on this part.");
                }
            }
            //delete generators and collector lists because they are no longer needed
            listPodGenerators = null;


            //Attach display modules
            foreach (IonResourceData resouces in listSupportResources)
                resouces.DisplayModule = IonModuleDisplay.findDisplayModule(this.part, resouces);
        }


        /************************************************************************\
         * IonModuleCrewSupport class                                           *
         * OnUpdate function override                                           *
         *                                                                      *
        \************************************************************************/
        public override void OnUpdate()
        {
            base.OnUpdate();
#if DEBUG_UPDATES
            Debug.Log("IonModuleCrewSupport.OnUpdate() " + this.part.name);
#endif
            bool allResourcesMet = true;

            if (part.protoModuleCrew.Count > 0)
            {
                lifeSupportStatus = "Active";
                lifeSupportStatusL2 = "";
                allResourcesMet = ConsumeResources(TimeWarp.deltaTime);
            }
            else
            {
                lifeSupportStatus = "Inactive";
                lifeSupportStatusL2 = "";
            }
        }


        /************************************************************************\
         * IonModuleCrewSupport class                                           *
         * Initialize function                                                   *
         *                                                                      *
        \************************************************************************/
        protected override void FirstUpdateInitialize()
        {
            base.FirstUpdateInitialize();
#if DEBUG
            Debug.Log("IonModuleCrewSupport.FirstUpdateInitialize() " + this.part.name);
#endif
            //listResourceNodes = null;
        }


        /************************************************************************\
         * IonModuleCrewSupport class                                           *
         * ConsumeResources function                                            *
         *                                                                      *
         * Runs the module's resource use simulation over deltaTime.            *
        \************************************************************************/
        public override bool ConsumeResources(double deltaTime)
        {
#if DEBUG_UPDATES
            Debug.Log("IonModuleCrewSupport.ConsumeResources() " + this.part.name);
#endif
            bool allResourcesMet = true;

            int crew = part.protoModuleCrew.Count;
            int crewCapacity = part.CrewCapacity;

            double resourceRequest;
            double resourceReturn;

            foreach (IonSupportResourceDataLocal supportResource in listSupportResources)
            {
                resourceRequest = (supportResource.RateBase * supportResource.RateBaseMod + supportResource.RatePerKerbal * supportResource.RatePerKerbalMod * crew + supportResource.RatePerCapacity * supportResource.RatePerCapacityMod * crewCapacity) * deltaTime;
                resourceReturn = RequestResource(supportResource.ID, resourceRequest);

                supportResource.AddDisplayRate((float)resourceReturn);
#if DEBUG_UPDATES
                Debug.Log("IonModuleCrewSupport.ConsumeResources(): requesting " + resourceRequest + " of " + supportResource.Name);
                Debug.Log("IonModuleCrewSupport.ConsumeResources(): returning " + resourceReturn + " of " + supportResource.Name);
#endif

                //Check if request was met and if its been long enough without them to trigger any effects
                if (Math.Abs(resourceRequest - resourceReturn) * 1000 > Math.Abs(resourceRequest))
                {
#if DEBUG
                    Debug.Log("IonModuleCrewSupport.ConsumeResources(): return did not meet request for " + supportResource.Name + " | resourceReturn / resourceRequest " + resourceReturn + "/" + resourceRequest);
#endif
                    allResourcesMet = false;
                    lifeSupportStatus = "Warning!";
                    lifeSupportStatusL2 = supportResource.Name + " Levels!";

                    if (supportResource.CauseDeath)
                    {
                        supportResource.FramesWithoutResource++;
                        supportResource.TimeSinceLastKillRoll += deltaTime;

                        //End timewarp if resources are low
                        //if (supportResource.FramesWithoutResource > IoncrossController.Instance.Settings.KillResources_MinFramesWarning && TimeWarp.CurrentRateIndex > 0)
                        //{
                        //    supportResource.Low = true;
                        //    TimeWarp.SetRate(TimeWarp.CurrentRateIndex - 1, false);
                        //}
#if DEBUG
                        Debug.Log("IonModuleCrewSupport.ConsumeResources(): framesWithoutResource " + supportResource.FramesWithoutResource + " | timeSinceLastKillRoll " + supportResource.TimeSinceLastKillRoll);
#endif
                        if (supportResource.TimeSinceLastKillRoll > supportResource.KillRollInterval && supportResource.FramesWithoutResource > IoncrossController.Instance.Settings.KillResources_MinFramesKill)
                        {
#if DEBUG_UPDATES
                            Debug.Log("IonModuleCrewSupport.ConsumeResources(): kill crew roll for low " + supportResource.Name + " levels!");
#endif
                            KillCrewRoll(supportResource.KillChance);
                            supportResource.TimeSinceLastKillRoll = 0;
                            supportResource.FramesWithoutResource = 0;
                        }
                    }

                    if (supportResource.CauseLock && null != moduleCommand)
                    {
                            //Find way to disable the command pod
                    }
                }

                //if the resource request was met
                else
                {
                    //Once a way to disable the command pod is found, re-enable it here
                    supportResource.FramesWithoutResource = 0;
                    supportResource.Low = false;
                }
            }

            return allResourcesMet;
        }


        /************************************************************************\
         * IonModuleCrewSupport class                                           *
         * ConsumeResourcesQuick function                                       *
         *                                                                      *
         * Runs a quicker aproximation of the module's resource use simulation  *
         * over deltaTime.                                                      *
        \************************************************************************/
        public override void ConsumeResourcesQuick(double deltaTime)
        {
#if DEBUG_UPDATES
            Debug.Log("IonModuleCrewSupport.ConsumeResourceQuick() " + this.part.name);
#endif
            int crew = part.protoModuleCrew.Count;
            int crewCapacity = part.CrewCapacity;

            double resourceRequest;
            double resourceReturn;

            foreach (IonSupportResourceDataLocal supportResource in listSupportResources)
            {
                resourceRequest = (supportResource.RateBase * supportResource.RateBaseMod + supportResource.RatePerKerbal * supportResource.RatePerKerbalMod * crew + supportResource.RatePerCapacity * supportResource.RatePerCapacityMod * crewCapacity) * deltaTime;
                resourceReturn = RequestResource(supportResource.ID, resourceRequest);
#if DEBUG_UPDATES
                Debug.Log("IonModuleCrewSupport.ConsumeResourceQuick(): requesting " + resourceRequest + " of " + supportResource.Name);
                Debug.Log("IonModuleCrewSupport.ConsumeResourceQuick(): returning " + resourceReturn + " of " + supportResource.Name);
#endif
                //Skip check if the request was met
            }
        }


        /************************************************************************\
         * IonModuleCrewSupport class                                           *
         * KillCrewRoll function                                                *
         *                                                                      *
        \************************************************************************/
        protected void KillCrewRoll(float fDeathChance)
        {
#if DEBUG
            Debug.Log("IonModuleCrewSupport.KillCrewRoll() " + this.part.name);
#endif
            float rand;
            for (int i = 0; i < this.part.protoModuleCrew.Count; i++)
            {
                rand = UnityEngine.Random.Range(0.0f, 100.0f);
#if DEBUG
                Debug.Log("IonModuleCrewSupport.KillCrewRoll(): i = " + i + " | rand = " + rand + " | kill chance = " + fDeathChance);      
#endif
                if (fDeathChance * 100 > rand)
                {
                    RemoveCrew(part.protoModuleCrew[i--]);
                }
            }
        }

        /************************************************************************\
         * IonModuleCrewSupport class                                           *
         * RemoveCrew function                                                  *
         *                                                                      *
        \************************************************************************/
        private void RemoveCrew(ProtoCrewMember crew)
        {
#if DEBUG
            Debug.Log("IonModuleCrewSupport.RemoveCrew() " + this.part.name);
            Debug.Log("IonModuleCrewSupport.RemoveCrew(): killing crew " + crew.ToString());
#endif
			this.part.RemoveCrewmember(crew);
			crew.Die();
        }

        /************************************************************************\
         * IonModuleCrewSupport class                                           *
         * GetResourceUsage function                                            *
         *                                                                      *
         * Calculates the module's resource usage over deltaTime and returns a  *
         * list of all resources used and their amounts.                        *
        \************************************************************************/
        public override List<IonResourceData> GetResources()
        {
#if DEBUG
            Debug.Log("IonModuleCrewSupport.GetResources() " + this.part.name);
#endif
            List<IonResourceData> listResources = new List<IonResourceData>();

            foreach (IonSupportResourceDataLocal supportResource in listSupportResources)
            {
                //IonResourceData resourceData = new IonResourceData(supportResource);
                //listResources.Add(resourceData);
                listResources.Add(supportResource);
#if DEBUG
                Debug.Log("IonModuleEVASupport.GetResources(): adding " + supportResource.Name);
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
                return listSupportResources;

            return null;
        }
        public override IonResourceData CreateResourceEntry(ConfigNode node)
        {
            return new IonSupportResourceDataLocal(node);
        }


        /************************************************************************\
         * IonModuleCrewSupport class                                           *
         * CopyGlobalLists function                                             *
         *                                                                      *
         * Copies data from global support resource, pod generators, and pod    *
         * collector lists to the local lists.                                  *
        \************************************************************************/
        public void CopyGlobalLists()
        {
            IonSupportResourceDataLocal supportResourceL;
            IonGeneratorData generatorDataL;

            foreach (IonSupportResourceDataGlobal supportResourceG in IoncrossController.Instance.Settings.ListSupportResources)
            {
#if DEBUG
                Debug.Log("IonModuleCrewSupport.CopyGlobalLists(): creating local resource for " + supportResourceG.Name);
#endif

                supportResourceL = (IonSupportResourceDataLocal)listSupportResources.Find(delegate(IonResourceData resource) { return resource.Name == supportResourceG.Name; });
                if (null != supportResourceL)
                {
                    supportResourceL.Data = supportResourceG;
                }
                else
                {
                    supportResourceL = new IonSupportResourceDataLocal(supportResourceG);
                    listSupportResources.Add(supportResourceL);
                }
            }

            foreach (IonGeneratorData generatorDataG in IoncrossController.Instance.Settings.ListPodGenerators)
            {
#if DEBUG
                Debug.Log("IonModuleCrewSupport.CopyGlobalLists(): creating local entry for " + generatorDataG.generatorName);
#endif
                generatorDataL = listPodGenerators.Find(delegate(IonGeneratorData generator) { return generator.generatorName == generatorDataG.generatorName; });
                if (null != generatorDataL)
                {
                    generatorDataL = generatorDataG;
                }
                else
                {
                    generatorDataL = new IonGeneratorData(generatorDataG);
                    listPodGenerators.Add(generatorDataL);
                }
            }

            foreach (IonCollectorData collectorDataG in IoncrossController.Instance.Settings.ListPodCollectors)
            {
#if DEBUG
                Debug.Log("IonModuleCrewSupport.CopyGlobalLists(): creating local entry for " + collectorDataG.generatorName);
#endif
                generatorDataL = listPodGenerators.Find(delegate(IonGeneratorData generator) { return generator.generatorName == collectorDataG.generatorName; });
                if (null != generatorDataL)
                {
                    generatorDataL = collectorDataG;
                }
                else
                {
                    generatorDataL = new IonCollectorData(collectorDataG);
                    listPodGenerators.Add(generatorDataL);
                }
            }
        }
    }
    //==========================================================================================================
    // END of IonModuleCrewSupport Class
    //==========================================================================================================
}