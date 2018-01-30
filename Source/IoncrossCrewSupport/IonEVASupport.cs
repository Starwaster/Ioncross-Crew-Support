//#define DEBUG
//#define DEBUG_UPDATES

using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using UnityEngine;
using System.Text;

using KSP;


namespace IoncrossKerbal
{

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class IoncrossEVAController : UnityEngine.MonoBehaviour
    {
        //private Vessel curVessel;
        //private Vessel oldVessel;
        //private bool vesselisEVA;

        public void Awake()
        {
#if DEBUG
            Debug.Log("IoncrossEVAController.Awake()");
#endif
        }

        public void Start()
        {
#if DEBUG
            Debug.Log("IoncrossEVAController.Start()");
#endif
            GameEvents.onCrewOnEva.Add(KerbalEVAListener);
        }

        private void KerbalEVAListener(GameEvents.FromToAction<Part, Part> eventData)
        {
            
#if DEBUG
            Debug.Log("IoncrossEVAController.KerbalEVAListener(): possible Kerbal on EVA: " + eventData.to.vessel.vesselName + " (" + eventData.to.vessel.id + ")");
#endif
            if (eventData.to.vessel.isEVA)
            {
#if DEBUG
                Debug.Log("IoncrossEVAController.Update(): Kerbal is on EVA");
#endif
                IonModuleEVASupport evaModule = null;
                if (eventData.to.vessel.rootPart.Modules.Contains("IonModuleEVASupport"))
                {
                    evaModule = (IonModuleEVASupport)eventData.to.vessel.rootPart.Modules["IonModuleEVASupport"];
                }

                if (null == evaModule)
                {
                    evaModule = CreateEVA(eventData.to.vessel.rootPart);
                }

                if (!evaModule.evainitialized)
                {
                    InitializeEVA(evaModule, eventData.from.vessel);
                }
            }
        }

        private IonModuleEVASupport CreateEVA(Part evaPart)
        {
#if DEBUG
            Debug.Log("IoncrossEVAController.CreateEVA()");
#endif
            IonModuleEVASupport evaModule;
            evaModule = (IonModuleEVASupport)(evaPart.AddModule("IonModuleEVASupport"));
            evaModule.lastLoaded = Planetarium.GetUniversalTime();
            evaModule.evainitialized = false;

            return evaModule;
        }

        private void InitializeEVA(IonModuleEVASupport evaModule, Vessel source)
        {
#if DEBUG
            Debug.Log("IoncrossEVAController.InitializeEVA()");
#endif

            evaModule.evaStartTime = Planetarium.GetUniversalTime();

            foreach (IonSupportResourceDataGlobal supportResource in IoncrossController.Instance.Settings.ListSupportResources)
            {
#if DEBUG
                Debug.Log("IoncrossEVAController.InitializeEVA(): processing  ION_SUPPORT_RESOURCE node\n" + supportResource.Name);
#endif
                if (supportResource.EVAamount > 0)
                {
#if DEBUG
                    Debug.Log("IoncrossEVAController.InitializeEVA(): adding " + supportResource.EVAamount + " of " + supportResource.Name);
#endif
                    IonEVAResourceDataLocal EVAResource = new IonEVAResourceDataLocal(supportResource);

                    EVAResource.MaxAmount = supportResource.EVAamount;
                    EVAResource.Amount = Math.Min(CollectResource(supportResource.ID, supportResource.EVAamount, source), supportResource.EVAamount);


                    evaModule.AddResource(EVAResource);
                }
            }

            evaModule.evainitialized = true;
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
        public bool evainitialized = false;
        public double evaStartTime = -1;
		public List<ConfigNode> listResourceNodes;
		public List<IonResourceData> listEVAResources = new List<IonResourceData>();

		IonModuleEVASupport()
		{
			//listEVAResources =
		}

        /************************************************************************\
         * IonModuleEVASupport class                                            *
         * OnAwake function override                                            *
         *                                                                      *
        \************************************************************************/
        public override void OnAwake()
        {
            base.OnAwake();
			listResourceNodes = new List<ConfigNode>();

#if DEBUG
            Debug.Log("IonModuleEVASupport.OnAwake() " + this.part.name);
#endif
			GameEvents.onCrewBoardVessel.Add (ReturnResources);
        }

		public void  ReturnResources(GameEvents.FromToAction<Part,Part> transfer)
		{
			Debug.Log ("IonModuleEVASupport.ReturnResources() called from onCrewBoardVessel."); 
			if (transfer.from == this.part) 
			{
				//foreach (PartResource resource in transfer.from.Resources)
				foreach (IonEVAResourceDataLocal resource in listEVAResources)
				{
					Debug.Log ("Found " + resource.Amount + " " + resource.Name + " on " + transfer.from.name);
					// Move the resource from Kerbal to vessel.
					//transfer.to.RequestResource (resource.resourceName, -resource.amount);
					transfer.to.RequestResource(resource.Name, -resource.Amount);
					resource.Amount = 0f;
					// Discard resource from Kerbal regardless of success/failure of resource transfer.
					//transfer.from.RequestResource (resource.resourceName, resource.amount);
				}
			}
		}

        /************************************************************************\
         * IonModuleEVASupport class                                            *
         * OnLoad function override                                             *
         *                                                                      *
        \************************************************************************/
        public override void OnLoad(ConfigNode node)
        {
#if DEBUG
			Debug.Log("IonModuleEVASupport.OnLoad() " + this.part.name);
#endif

			//Create lists
            if (null == listResourceNodes)
            {
                Debug.Log("IonModuleEVASupport.OnLoad(): listResourceNodes is null, creating new");
                listResourceNodes = new List<ConfigNode>();
            }
            if (null == (object)listEVAResources)
            {
                Debug.Log("IonModuleEVASupport.OnLoad(): listEVAResources is null, creating new");
                listEVAResources = new List<IonResourceData>();
            }

            base.OnLoad(node);
#if DEBUG
            Debug.Log("IonModuleEVASupport.OnLoad() " + this.part.name);
            Debug.Log("IonModuleEVASupport.OnLoad(): node " + node.name);
#endif
            if(node.HasValue("evainitialized"))
				bool.TryParse(node.GetValue("evainitialized"), out evainitialized);

			if (node.HasValue("evaStartTime"))
				double.TryParse(node.GetValue("evaStartTime"), out evaStartTime);

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
			foreach (ConfigNode subNode in node.GetNodes())
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
					if (subNode.name == "ION_SUPPORT_RESOURCE")
					{
						Debug.Log ("Resource = " + subNode.GetValue ("name"));
						Debug.Log ("amount = " + subNode.GetValue ("amount"));
						Debug.Log ("maxAmount = " + subNode.GetValue ("maxAmount"));
					}
				}
				else
				{
					Debug.Log ("-- ERROR: Unable to process subNode " + subNode.name);

					if (subNode.name == "ION_SUPPORT_RESOURCE")
					{
						Debug.Log ("Resource = " + subNode.GetValue ("name"));
						Debug.Log ("amount = " + subNode.GetValue ("amount"));
						Debug.Log ("maxAmount = " + subNode.GetValue ("maxAmount"));
					}
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
            node.AddValue("evainitialized", evainitialized);
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
					//subNode.AddValue ("amount", evaResource.Amount);
					//subNode.AddValue ("maxAmount", evaResource.MaxAmount);
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
            if (null == (object)listEVAResources)
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
         * FixedUpdate function override                                           *
         *                                                                      *
        \************************************************************************/
        public override void FixedUpdate()
        {
			if(IonLifeSupportScenario.Instance.isLifeSupportEnabled && HighLogic.LoadedSceneIsFlight)
			{
				base.FixedUpdate();
#if DEBUG_UPDATES
        	    Debug.Log("IonModuleEVASupport.FixedUpdate() " + this.part.name);
#endif
    	        bool allResourcesMet = true;
				allResourcesMet = ConsumeResources(Planetarium.GetUniversalTime() - this.lastLoaded);
			}
        }


        /************************************************************************\
         * IonModuleEVASupport class                                            *
         * Initialize function                                                   *
         *                                                                      *
        \************************************************************************/
        protected override void FirstUpdateInitialize()
        {
            base.FirstUpdateInitialize();
#if DEBUG
            Debug.Log("IonModuleEVASupport.FirstUpdateInitialize() " + this.part.name);
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
				evaResource.DisplayModule.displayRate = (float)(evaResource.Amount / evaResource.MaxAmount) * 100.0f;
				
				if (evaResource.Name == "Oxygen")
				{
					// Quick and dirty hack to make Kerbals not consume EVA oxygen while on Kerbin
					if (this.vessel.mainBody.atmosphereContainsOxygen && this.vessel.atmDensity >= IoncrossController.Instance.Settings.MinimumBreathableAtmoDensity)
						continue;
				}
                resourceRequest = (evaResource.RatePerKerbal) * deltaTime;
                resourceReturn = RequestResource(evaResource.ID, resourceRequest);
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
						evaResource.TotalTimeWithoutResource += deltaTime;

                        //End timewarp if resources are low
                        //if (evaResource.FramesWithoutResource > IoncrossController.Instance.Settings.KillResources_MinFramesWarning && TimeWarp.CurrentRateIndex > 0)
                        //{
                        //    evaResource.Low = true;
                        //    TimeWarp.SetRate(TimeWarp.CurrentRateIndex - 1, false);
                        //}
#if DEBUG_UPDATES
                        Debug.Log("IonModuleEVASupport.ConsumeResources(): framesWithoutResource " + evaResource.FramesWithoutResource + " | timeSinceLastKillRoll " + evaResource.TimeSinceLastKillRoll);
#endif

                        if (evaResource.TimeSinceLastKillRoll > evaResource.Data.KillRollInterval && evaResource.FramesWithoutResource > IoncrossController.Instance.Settings.KillResources_MinFramesKill)
                        {
#if DEBUG_UPDATES
                            Debug.Log("IonModuleEVASupport.ConsumeResources(): kill crew roll for low " + evaResource.Name + " levels!");
#endif
							float deltaPenalty = (float)(evaResource.KillChanceDeltaPenalty * evaResource.TotalTimeWithoutResource);
							KillCrewRoll(evaResource.KillChance + deltaPenalty);
                            evaResource.TimeSinceLastKillRoll = 0;
                            evaResource.FramesWithoutResource = 0;
                        }
                    }
                }

                else
                {
					evaResource.TimeSinceLastKillRoll = 0;
					evaResource.TotalTimeWithoutResource = 0;
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
            this.part.RemoveCrewmember(crew);
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
            if ("ION_SUPPORT_RESOURCE" == nodeName)
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
			if (newResource == null)
				print ("IonEVASupport.AddResource() was passed null reference for newResource!!!!");
#if DEBUG
            Debug.Log("IonModuleEVASupport.AddResource() " + this.part.name);
            Debug.Log("IonModuleEVASupport.AddResource(): resource\n" + newResource.ToString());
#endif
            IonEVAResourceDataLocal evaResource = new IonEVAResourceDataLocal(newResource);

			if ((object)evaResource == null)
				print ("IonEVASupport.AddResource(): Null reference creating evaResource!!!!");

            //Attach display module
			evaResource.DisplayModule = IonModuleDisplay.findAndCreateDisplayModule(this.part, newResource);
			if ((object)evaResource.DisplayModule == null)
				print ("IonEVASupport.AddResource(): Null Reference retrieving evaResource.DisplayModule!!!!");
			evaResource.DisplayModule.SetGUIName(evaResource.Name);
            evaResource.DisplayModule.isRate = false;
            evaResource.DisplayModule.SetUnits("%");
            evaResource.DisplayModule.SetFormat("F1");

			if ((object)listEVAResources == null)
				print ("IonEVASupport.AddResource(): Null Reference retrieving listEVAResources!!!!");
			listEVAResources.Add(evaResource);

			//ConfigNode node = new ConfigNode("RESOURCE");
			//node.AddValue ("name", evaResource.Name);
			//node.AddValue ("amount", evaResource.Amount);
			//node.AddValue ("maxAmount", evaResource.MaxAmount);
			//part.AddResource (node);
        }


        /************************************************************************\
         * IonModuleEVA class                                                   *
         * RequestResource function                                             *
         *                                                                      *
        \************************************************************************/
		//public override double RequestResource(string resourceName, double resourceAmount, ResourceFlowMode flowMode = ResourceFlowMode.NULL)
        //{
		//	if (flowMode == ResourceFlowMode.NULL)
		//		flowMode = PartResourceLibrary.GetDefaultFlowMode(resourceName);
		//	
		//	return part.RequestResource(resourceName.GetHashCode(), resourceAmount, flowMode);
        //}

        /************************************************************************\
         * IonModuleEVA class                                                   *
         * RequestResource function                                             *
         *                                                                      *
        \************************************************************************/
        public double RequestResource(int resourceID, double resourceAmount)
        {
#if DEBUG_UPDATES
            Debug.Log("IonModuleEVASupport.RequestResource() " + this.part.name);
            Debug.Log("IonModuleEVASupport.RequestResource(): request for " + resourceAmount + " units of " + resourceID);
#endif
            double amount = 0;

			double preRequest = part.RequestResource(resourceID, resourceAmount);

#if DEBUG_UPDATES
			Debug.Log("IonModuleEVASupport.RequestResource(): PreRequest found " + preRequest.ToString("F8") + " units.");
#endif

			if (preRequest == resourceAmount)
				return preRequest;
			else
				resourceAmount -= preRequest;				

            foreach (IonEVAResourceDataLocal evaResource in listEVAResources)
            {
                if (evaResource.ID == resourceID)
                {
                    if(resourceAmount > 0)
                    {
                        amount = Math.Min(resourceAmount, evaResource.Amount);
#if DEBUG_UPDATES
                        Debug.Log("IonModuleEVASupport.RequestResource(): resourceAmount " + resourceAmount.ToString("F8") + " | evaResource.amount " + evaResource.Amount.ToString("F8"));
						Debug.Log("IonModuleEVASupport.RequestResource(): amount " + amount.ToString("F8"));
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