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
     * IonModuleGenerator class                             *
     *                                                      *
     * Class for handling generators functionality.         *
    \*======================================================*/
    public class IonModuleGenerator : IonModuleBase
    {
        public List<ConfigNode> listResourceNodes;
        public List<IonResourceData> listInputs;
        public List<IonResourceData> listOutputs;

		protected double inputModifier;
		protected double outputModifier;

        public virtual string GeneratorGUIName
        {
            get { return generatorGUIName; }
            set
            {
                generatorGUIName = value;
                Fields["generatorStatus"].guiName = generatorGUIName + " Status";
                Fields["outputLevelDisplay"].guiName = generatorGUIName + " Output";
                Fields["efficency"].guiName =       generatorGUIName + " Efficency";
                Events["ActivateButton"].guiName = "Activate " + generatorGUIName;
                Events["ShutdownButton"].guiName = "Deactivate " + generatorGUIName;
                Events["IncreaseButton"].guiName = "Increase " + generatorGUIName + " Output";
                Events["DecreaseButton"].guiName = "Decrease " + generatorGUIName + " Output";

                Actions["ActivateAction"].guiName = "Activate " + generatorGUIName;
                Actions["ShutdownAction"].guiName = "Deactivate " + generatorGUIName;
                Actions["ToggleAction"].guiName = "Toggle " + generatorGUIName;
                Actions["IncreaseAction"].guiName = "Increase " + generatorGUIName + " Output";
                Actions["DecreaseAction"].guiName = "Decrease " + generatorGUIName + " Output";
            }
        }

		[KSPField(isPersistant = true)]
		public string generatorName = "";

		[KSPField(isPersistant = false)]
		public string generatorGUIName = "";

		[KSPField(isPersistant = true)]
		public float outputLevel = 1f;

		[KSPField(isPersistant = false)]
		public float outputLevelStep = 0.1f;

		[KSPField(isPersistant = false)]
		public float outputLevelMin = 0.0f;

		[KSPField(isPersistant = false)]
		public float outputLevelMax = 1.0f;

        [KSPField(isPersistant = true)]
        public bool isActive = false;

        [KSPField(isPersistant = false)]
        public bool startOn = false;

        [KSPField(isPersistant = false)]
        public bool alwaysOn = false;

        [KSPField(isPersistant = false)]
        public bool hideStatus = false;

        [KSPField(isPersistant = false)]
        public bool hideStatusL2 = false;

        [KSPField(isPersistant = false)]
        public bool hideEfficency = false;

        [KSPField(isPersistant = false)]
        public bool hideOutputControls = false;

        [KSPField(isPersistant = false)]
        public bool hideActivateControls = false;

        /******************\
         * Display Fields *
        \******************/
        [KSPField(guiActive = true, guiName = "Generator Status", isPersistant = false)]
        public string generatorStatus = "standby";

        [KSPField(guiActive = true, guiName = " ", isPersistant = false)]
        public string generatorStatusL2;

        [KSPField(guiActive = true, guiName = "Output", guiUnits = "%", guiFormat = "F2", isPersistant = false)]
        public float outputLevelDisplay = 100.0f;

        [KSPField(guiActive = true, guiName = "Efficency", guiFormat = "F2", isPersistant = false)]
        public double efficency = 1.0f;


        /*****************************\
         * Activate/Deactive Buttons *
        \*****************************/
        [KSPEvent(guiActive = true, guiName = "Activate Generator")]
        public void ActivateButton()
        {
            SetGeneratorState(true);
        }

        [KSPEvent(guiActive = true, guiName = "Shutdown Generator")]
        public void ShutdownButton()
        {
            SetGeneratorState(false);
        }

        [KSPEvent(guiActive = true, guiName = "Increase Output")]
        public void IncreaseButton()
        {
            ChangeGeneratorLevel(outputLevelStep, outputLevelMin, outputLevelMax);
        }

        [KSPEvent(guiActive = true, guiName = "Decrease Output")]
        public void DecreaseButton()
        {
            ChangeGeneratorLevel(-outputLevelStep, outputLevelMin, outputLevelMax);
        }


        /*****************************\
         * Activate/Deactive Actions *
        \*****************************/
        [KSPAction("Activate Generator")]
        public void ActivateAction(KSPActionParam param)
        {
            SetGeneratorState(true);
        }

        [KSPAction("Shutdown Generator")]
        public void ShutdownAction(KSPActionParam param)
        {
            SetGeneratorState(false);
        }

        [KSPAction("Toggle Generator")]
        public void ToggleAction(KSPActionParam param)
        {
            SetGeneratorState(!isActive);
        }

        [KSPAction("Increase Output")]
        public void IncreaseAction(KSPActionParam param)
        {
            ChangeGeneratorLevel(outputLevelStep, outputLevelMin, outputLevelMax);
        }

        [KSPAction("Decrease Output")]
        public void DecreaseAction(KSPActionParam param)
        {
            ChangeGeneratorLevel(-outputLevelStep, outputLevelMin, outputLevelMax);
        }


        /********************************************\
         * Set Generator Status and Level Functions *
         * Used by above actions and buttons.       *
        \********************************************/
        protected virtual void SetGeneratorState(bool generatorState)
        {
            if (alwaysOn)
            {
                generatorState = true;
            }
            isActive = generatorState;
            generatorStatus = isActive ? "Active" : "Inactive";

            if (!hideActivateControls && IonLifeSupportScenario.Instance.isLifeSupportEnabled)
            {
                Events["ActivateButton"].active = !isActive;
                Events["ShutdownButton"].active = isActive;
            }
        }

        protected virtual void ChangeGeneratorLevel(float outputIncrement, float min = 0.0f, float max = 1.0f)
        {
            SetGeneratorLevel(outputLevel + outputIncrement, min, max);
        }

        protected virtual void SetGeneratorLevel(float newLevel, float min = 0.0f, float max = 1.0f)
        {
            outputLevel = newLevel;

            if (outputLevel > max)
                outputLevel = max;
            else if (outputLevel < min)
                outputLevel = min;

            outputLevelDisplay = outputLevel * 100.0f;
        }


        /************************************************************************\
         * IonModuleGenerator class                                             *
         * GetInfo function override                                            *
         *                                                                      *
        \************************************************************************/
        public override string GetInfo()
        {
#if DEBUG
            Debug.Log("IonModuleGenerator.GetInfo() " + this.part.name + " " + generatorName);
#endif
            string strInfo = "";

            strInfo += generatorGUIName + " Information:\n";
            strInfo += GetInfoBasic();
            strInfo += GetInfoLists();

            return strInfo;
        }


        /************************************************************************\
         * IonModuleGenerator class                                             *
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
         * IonModuleGenerator class                                             *
         * GetInfoLists function                                                *
         *                                                                      *
         * Helper function for getInfo.                                         *
        \************************************************************************/
        protected virtual string GetInfoLists()
        {
            string strInfo = "";

            if (listInputs.Count > 0)
            {
                strInfo += "  Input Resources\n";
                strInfo += getInfoList(listInputs, 4);
            }

            if (listOutputs.Count > 0)
            {
                strInfo += "  Output Resources\n";
                strInfo += getInfoList(listOutputs, 4);
            }

            return strInfo;
        }


        /************************************************************************\
         * IonModuleGenerator class                                             *
         * getInfoList function                                                 *
         *                                                                      *
         * Helper function for getInfo.                                         *
        \************************************************************************/
        protected string getInfoList(List<IonResourceData> list, int indentLevel = 0)
        {
            string strInfo = "";
            float rate;
            string unit;

            foreach (IonGeneratorResourceData resource in list)
            {
                rate = (float)(resource.RateBase + resource.RatePerCapacity * this.part.CrewCapacity);
                unit = "sec.";

                //adjust displayRate units so the value stays above 0.5
                IonModuleDisplay.setRateUnits(ref rate, ref unit, 0.5f);

                //Indent line
                for (int i = 0; i < indentLevel; i++)
                    strInfo += " ";

                //add info to string
                strInfo += "- " + resource.Name + " (" + Math.Round(rate, 1) + "/" + unit + ")" + (1 == resource.EffectOnEfficency ? " [Required]\n" : "\n");
				//strInfo += "Flow = " + GetFlowModeDescription(resource.FlowMode);
            }

            return strInfo;
        }

		public string GetFlowModeDescription(ResourceFlowMode fMode)
		{
			string text = string.Empty;
			switch (fMode)
			{
				case ResourceFlowMode.NO_FLOW:
					text += "<color=orange>Cannot drain from other parts.</color>\n";
					break;
				case ResourceFlowMode.ALL_VESSEL:
				case ResourceFlowMode.ALL_VESSEL_BALANCE:
					text = text + "<color=" + XKCDColors.HexFormat.KSPUnnamedCyan + ">Drains evenly across vessel.</color>\n";
					break;
				case ResourceFlowMode.STAGE_PRIORITY_FLOW:
				case ResourceFlowMode.STAGE_PRIORITY_FLOW_BALANCE:
					text = text + "<color=" + XKCDColors.HexFormat.KSPBadassGreen + ">Drains evenly across vessel, per priority.</color>\n";
					break;
				case ResourceFlowMode.STACK_PRIORITY_SEARCH:
				case ResourceFlowMode.STAGE_STACK_FLOW:
				case ResourceFlowMode.STAGE_STACK_FLOW_BALANCE:
					text = text + "<color=" + XKCDColors.HexFormat.YellowishOrange + ">Drains evenly respecting crossfeed, per priority.</color>\n";
					break;
			}
			return text;
		}


        /************************************************************************\
         * IonModuleGenerator class                                             *
         * OnAwake function override                                            *
         *                                                                      *
        \************************************************************************/
        public override void  OnAwake()
        {
            base.OnAwake();
#if DEBUG
            Debug.Log("IonModuleGenerator.OnAwake() " + this.part.name + " " + generatorName);
#endif
			listResourceNodes = new List<ConfigNode>();
			listInputs = new List<IonResourceData>();
			listOutputs = new List<IonResourceData>();
        }


        /************************************************************************\
         * IonModuleGenerator class                                             *
         * OnLoad function override                                             *
         *                                                                      *
        \************************************************************************/
        public override void OnLoad(ConfigNode node)
        {
            //Create lists
            if (null == listResourceNodes)
            {
                Debug.Log("IonModuleGenerator.OnLoad(): listResourceNodes is null, creating new");
                listResourceNodes = new List<ConfigNode>();
            }
            if (null == listInputs)
            {
                Debug.Log("IonModuleGenerator.OnLoad(): listInputs is null, creating new");
                listInputs = new List<IonResourceData>();
            }
            if (null == listOutputs)
            {
                Debug.Log("IonModuleGenerator.OnLoad(): listOutputs is null, creating new");
                listOutputs = new List<IonResourceData>();
            }

            base.OnLoad(node);
#if DEBUG
            Debug.Log("IonModuleGenerator.OnLoad() " + this.part.name + " " + generatorName);
            Debug.Log("IonModuleGenerator.OnLoad(): node\n " + node.ToString());
#endif

			//Read and process nodes
			foreach (ConfigNode subNode in node.GetNodes("INPUT_RESOURCE"))
            {
#if DEBUG
                Debug.Log("IonModuleGenerator.OnLoad(): processing subNode " + subNode.name);
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

			foreach (ConfigNode subNode in node.GetNodes("OUTPUT_RESOURCE"))
			{
#if DEBUG
				Debug.Log("IonModuleGenerator.OnLoad(): processing subNode " + subNode.name);
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

			// experiment: Let's try NOT reading these in. They're all KSPField and some are persistent so they should not need this extra handling.
			return;
			//Read variables from node
			if (node.HasValue("generatorName"))
				generatorName = node.GetValue("generatorName");
			if (node.HasValue("generatorGUIName"))
				generatorGUIName = node.GetValue("generatorGUIName");

			if (node.HasValue("hideStatus"))
				bool.TryParse(node.GetValue("hideStatus"), out hideStatus);
			if (node.HasValue("hideStatusL2"))
				bool.TryParse(node.GetValue("hideStatusL2"), out hideStatusL2);
			if (node.HasValue("hideEfficency"))
				bool.TryParse(node.GetValue("hideEfficency"), out hideEfficency);
			if (node.HasValue("hideOutputControls"))
				bool.TryParse(node.GetValue("hideOutputControls"), out hideOutputControls);
			if (node.HasValue("hideActivateControls"))
				bool.TryParse(node.GetValue("hideActivateControls"), out hideActivateControls);

			if (node.HasValue("isActive"))
				bool.TryParse(node.GetValue("isActive"), out isActive);
			else if (node.HasValue("startOn"))
				bool.TryParse(node.GetValue("startOn"), out isActive); // TODO Does this really need to be done like this?

			if (node.HasValue("alwaysOn"))
				bool.TryParse(node.GetValue("alwaysOn"), out alwaysOn);

			if (node.HasValue("outputLevel"))
				outputLevel = Convert.ToSingle(node.GetValue("outputLevel"));
			if (node.HasValue("outputLevelStep"))
				outputLevelStep = Convert.ToSingle(node.GetValue("outputLevelStep"));
			if (node.HasValue("outputLevelMin"))
				outputLevelMin = Convert.ToSingle(node.GetValue("outputLevelMin"));
			if (node.HasValue("outputLevelMax"))
				outputLevelMax = Convert.ToSingle(node.GetValue("outputLevelMax"));
		}

        /************************************************************************\
         * IonModuleGenerator class                                             *
         * Load function                                                        *
         *                                                                      *
         * Used to load generator data from a supportGenerator object.          *
        \************************************************************************/
        public virtual void Load(IonGeneratorData supportGenerator)
        {
            //Create lists
			if (listResourceNodes == null)
				listResourceNodes = new List<ConfigNode>();
			if (listInputs == null)
				listInputs = new List<IonResourceData>();
			if (listOutputs == null)
				listOutputs = new List<IonResourceData>();

            //InitializeValues();
#if DEBUG
            Debug.Log("IonModuleGenerator.Load() " + this.part.name + " " + generatorName);
#endif
            //Read variables from supportGenerator
            generatorName = supportGenerator.generatorName;
            generatorGUIName = supportGenerator.generatorGUIName;
            startOn = supportGenerator.startOn;
            alwaysOn = supportGenerator.alwaysOn;

            outputLevelStep = supportGenerator.outputLevelStep;
            outputLevelMin = supportGenerator.outputLevelMin;
            outputLevelMax = supportGenerator.outputLevelMax;

            hideStatus = supportGenerator.hideStatus;
            hideStatusL2 = supportGenerator.hideStatusL2;
            hideEfficency = supportGenerator.hideEfficency;
            hideOutputControls = supportGenerator.hideOutputControls;
            hideActivateControls = supportGenerator.hideActivateControls;

            //Process data in the supportGenerator lists
            foreach (IonGeneratorResourceData inputResource in supportGenerator.listInputs)
            {
#if DEBUG
                Debug.Log("IonModuleGenerator.Load(): loading resource " + inputResource.Name + " from supportGenerator.listInputs");
#endif
                IonGeneratorResourceData genResource = new IonGeneratorResourceData(inputResource);
                listInputs.Add(genResource);
            }

            foreach (IonGeneratorResourceData outputResource in supportGenerator.listOutputs)
            {
#if DEBUG
                Debug.Log("IonModuleGenerator.Load(): loading resource " + outputResource.Name + " from supportGenerator.listOutputs");
#endif
                IonGeneratorResourceData genResource = new IonGeneratorResourceData(outputResource);
                listOutputs.Add(genResource);
            }
		}


        /************************************************************************\
         * IonModuleGenerator class                                             *
         * OnSave function override                                             *
         *                                                                      *
        \************************************************************************/
		
        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
			return;
#if DEBUG
            Debug.Log("IonModuleGenerator.OnSave() " + this.part.name + " " + generatorName);
#endif
			//Save inputs
            if (null != listInputs)
            {
                foreach (IonGeneratorResourceData resource in listInputs)
                {
#if DEBUG
                    Debug.Log("IonModuleGenerator.OnSave(): saving resouce " + resource.Name + " from listInputs");
#endif
                    ConfigNode resourceNode = new ConfigNode("INPUT_RESOURCE");
                    resource.Save(resourceNode);
                    node.AddNode(resourceNode);
                }
            }

            //Save outputs
            if (null != listOutputs)
            {
                foreach (IonGeneratorResourceData resource in listOutputs)
                {
#if DEBUG
                    Debug.Log("IonModuleGenerator.OnSave(): saving resouce " + resource.Name + " from listOutputs");
#endif
                    ConfigNode resourceNode = new ConfigNode("OUTPUT_RESOURCE");
                    resource.Save(resourceNode);
                    node.AddNode(resourceNode);
                }
            }
#if DEBUG
            Debug.Log("IonModuleGenerator.OnSave(): node\n" + node.ToString());
#endif
			// Don't save these. They shouldn't be persisted. They aren't player configurable and they can't change during gameplay.  (except the first three which are already handled via isPersistant = true)
			//Save variables
			node.AddValue("generatorName", generatorName);
			node.AddValue("isActive", isActive);
			node.AddValue("outputLevel", outputLevel);
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
		}


        /************************************************************************\
         * IonModuleGenerator class                                             *
         * OnStart function override                                            *
         *                                                                      *
        \************************************************************************/
        public override void OnStart(PartModule.StartState state)
        {
            //Reprocess and clear listResourceNodes, if necessary
            if (null == listInputs || null == listOutputs)
            {
				Debug.Log("IonModuleGenerator.OnStart() - Null listInputs or listOutputs. We probably shouldn't be here unless this is during the game initialization when parts are being compiled");
                listInputs = new List<IonResourceData>();
                listOutputs = new List<IonResourceData>();
                //ProcessNodestoList(listResourceNodes);
            }

			IonModuleGenerator generator = part.partInfo.partPrefab.FindModulesImplementing<IonModuleGenerator>().FirstOrDefault(gen => gen.generatorName == generatorName);

			if (listInputs.Count == 0 && part.partInfo != null)
			{
				//listInputs = ((IonModuleGenerator)part.partInfo.partPrefab.Modules["IonModuleGenerator"]).listInputs;
				listInputs = generator.listInputs;
				Debug.Log("IonModuleGenerator " + generatorName + " loaded " + listInputs.Count.ToString() +" listInputs");
			}
			else
				Debug.Log("DEBUG - IonModuleGenerator.OnStart(): listInputs = " + listInputs.Count.ToString() + ", partInfo = " + part.partInfo == null ? "YES" : "NO");

			if (listOutputs.Count == 0 && part.partInfo != null)
			{
				listOutputs = generator.listOutputs;
				Debug.Log("IonModuleGenerator " + generatorName + " loaded " + listOutputs.Count.ToString() + " listOutputs");
			}
			else
				Debug.Log("DEBUG - IonModuleGenerator.OnStart(): listOutputs = " + listOutputs.Count.ToString() + ", partInfo null? = " + part.partInfo == null ? "YES" : "NO");

#if DEBUG
            Debug.Log("IonModuleGenerator.OnStart() " + this.part.name + " " + generatorName);
            Debug.Log("IonModuleGenerator.OnStart(): state " + state.ToString());
#endif


			//Attach display modules
			foreach (IonResourceData resource in GetResources())
            {
                resource.DisplayModule = IonModuleDisplay.findDisplayModule(this.part, resource);
            }

			Debug.Log("Finished setting display module for IonGenerator " + GeneratorGUIName);

            //Hide unwanted displays, buttons, and actions
			Fields["generatorStatus"].guiActive = !hideStatus && IonLifeSupportScenario.Instance.isLifeSupportEnabled;
			Fields["generatorStatusL2"].guiActive = !hideStatusL2 && IonLifeSupportScenario.Instance.isLifeSupportEnabled;
			Fields["efficency"].guiActive = !hideEfficency && IonLifeSupportScenario.Instance.isLifeSupportEnabled;

            if(hideOutputControls)
            {
                Fields["outputLevelDisplay"].guiActive = false;

                Events["IncreaseButton"].active = false;
                Events["DecreaseButton"].active = false;

                Actions["IncreaseAction"].active = false;
                Actions["DecreaseAction"].active = false;
            }

            if(hideActivateControls || alwaysOn || !IonLifeSupportScenario.Instance.isLifeSupportEnabled)
            {
                Events["ActivateButton"].active = false;
                Events["ShutdownButton"].active = false;

                Actions["ActivateAction"].active = false;
                Actions["ShutdownAction"].active = false;
                Actions["ToggleAction"].active = false;
            }
			else if(IonLifeSupportScenario.Instance.isLifeSupportEnabled)
			{
				Events["ActivateButton"].active = !isActive;
				Events["ShutdownButton"].active = isActive;
				
				Actions["ActivateAction"].active = true;
				Actions["ShutdownAction"].active = true;
				Actions["ToggleAction"].active = true;
			}

            //Set GeneratorGUIName
            if (null != generatorGUIName && generatorGUIName.Length > 0)
            {
                GeneratorGUIName = generatorGUIName;
            }
            else
            {
                GeneratorGUIName = generatorName;
            }

            //Set generator state
            //lastLoaded is used to detrimine if this part has flown yet
			if (alwaysOn || (startOn && (lastLoaded < 0 || state == StartState.PreLaunch)))
            {
                isActive = true;
            }
            SetGeneratorState(isActive);
			base.OnStart(state);
		}


        /************************************************************************\
         * IonModuleGenerator class                                             *
         * FixedUpdate function override                                           *
         *                                                                      *
        \************************************************************************/
        public override void FixedUpdate()
        {
			if(IonLifeSupportScenario.Instance.isLifeSupportEnabled && HighLogic.LoadedSceneIsFlight)
			{
	            base.FixedUpdate();
#if DEBUG_UPDATES
    	        Debug.Log("IonModuleGenerator.FixedUpdate() " + this.part.name + " " + generatorName);
#endif
	            bool allResourcesMet = true;

	            UpdateSetup();

	            if (isActive && isAble())
	            {
	                generatorStatusL2 = "";
	                CalculateModifiers(Planetarium.GetUniversalTime() - this.lastLoaded);
	                allResourcesMet = ConsumeResources(Planetarium.GetUniversalTime() - this.lastLoaded);
	            }

	            if (!isActive)
	                generatorStatusL2 = "";
			}
        }

        /************************************************************************\
         * IonModuleGenerator class                                             *
         * Initialize function                                                   *
         *                                                                      *
        \************************************************************************/
        protected override void FirstUpdateInitialize()
        {
            base.FirstUpdateInitialize();
#if DEBUG
            Debug.Log("IonModuleGenerator.FirstUpdateInitialize() " + this.part.name + " " + generatorName);
#endif
        }


        /************************************************************************\
         * IonModuleGenerator class                                             *
         * FixedUpdate function override                                           *
         *                                                                      *
        \************************************************************************/
        public virtual void UpdateSetup()
        {
#if DEBUG_UPDATES
            Debug.Log("IonModuleGenerator.UpdateSetup() " + this.part.name + " " + generatorName);
#endif
            //Reset modifiers
            outputModifier = 1;
            inputModifier = 1;
        }

		/************************************************************************\
         * IonModuleGenerator class                                             *
         * CalculateModifiers function                                          *
         *                                                                      *
         * Calculates what limits should be put on input and output modifiers   *
         * based on the avalability of resources or free space as applicable.   *
         * outputModifier may start out and end up > 1, in which case the       *
         * generator will produce more output resource for the normal amount of *
         * input.                                                               *
         * inputModifier should not be > 1.                                     *
        \************************************************************************/
		public virtual void CalculateModifiers(double deltaTime)
		{
#if DEBUG_UPDATES
            Debug.Log("IonModuleGenerator.CalculateModifiers() " + this.part.name + " " + generatorName);
            Debug.Log("IonModuleGenerator.CalculateModifiers(): inputModifier " + inputModifier + " | outputModifier " + outputModifier);
#endif
			int crew = part.protoModuleCrew.Count;
			int crewCapacity = part.CrewCapacity;

			double limitFactor = 1;
			double resourceRequest;

			//Cycle Through inputs and determine which, if any, have limits on space or amount
			foreach (IonGeneratorResourceData input in listInputs)
			{
				//calculate amount and free space
				double amount;
				double maxAmount;
				this.part.GetConnectedResourceTotals(input.ID, PartResourceLibrary.GetDefaultFlowMode(input.ID), out amount, out maxAmount, true);

				input.CurAvailable = 0;
				input.CurFreeAmount = 0;

				input.CurAvailable = amount;
				input.CurFreeAmount = maxAmount - amount;

				//calculate how much will be requested
				resourceRequest = (input.RateBase + (input.RatePerKerbal * crew) + (input.RatePerCapacity * crewCapacity)) * deltaTime * outputLevel;

				//calculate limitFactor
				if (!resourceRequest.Equals(0d))
				{
					limitFactor = (resourceRequest > 0 ? input.CurAvailable : -input.CurFreeAmount) / resourceRequest; //if resourceRequest > 0 use curAvalable, else use -curFreeAmount (- to keep limit factor +)

					//if this is a required resouce and it is limited
					if (input.EffectOnEfficency.Equals(1f) && limitFactor < 1d)
					{
						input.Low = true;
						inputModifier = limitFactor < inputModifier ? limitFactor : inputModifier; //inputEfficency = Math.Min(limitFactor, inputEfficency);

						if (limitFactor < 0.001)
						{
							generatorStatusL2 = input.Name + " Depleted";
							input.Depleated = true;
						}
					}
				}
#if DEBUG_UPDATES
                Debug.Log("IonModuleGenerator.CalculateModifiers(): Looking at Input " + input.Name + " | request will be for " + resourceRequest + " | " + (resourceRequest > 0 ? "curAvalable" : "curFreeAmount") + " = " + (resourceRequest > 0 ? input.CurAvailable : input.CurFreeAmount) + (resourceRequest != 0 ? (" | limitFactor " + limitFactor + " | efficency set to " + efficency) : ""));
#endif
			}


			//Cycle Through outputs and determine which, if any, have limits on space or amount
			foreach (IonGeneratorResourceData output in listOutputs)
			{
				double amount;
				double maxAmount;
				//calculate amount and free space
				this.part.GetConnectedResourceTotals(output.ID, PartResourceLibrary.GetDefaultFlowMode(output.ID), out amount, out maxAmount, true);

				output.CurAvailable = amount;
				output.CurFreeAmount = maxAmount - amount;

				output.CurAvailable *= 1.0f - output.CutoffMargin;
				output.CurFreeAmount *= 1.0f - output.CutoffMargin;

				//calculate how much will be requested
				resourceRequest = -(output.RateBase + (output.RatePerKerbal * crew) + (output.RatePerCapacity * crewCapacity)) * deltaTime * outputLevel;

				//calculate limitFactor
				if (!resourceRequest.Equals(0d))
				{
					limitFactor = Math.Min((resourceRequest > 0 ? output.CurAvailable : -output.CurFreeAmount) / resourceRequest, 1); //if resourceRequest > 0 use curAvalable, else use -curFreeAmount (- to keep limit factor +)

					if (limitFactor < outputModifier && output.EffectOnEfficency >= 1)
						outputModifier = limitFactor;
				}
#if DEBUG_UPDATES
                Debug.Log("IonModuleGenerator.CalculateModifiers(): Looking at Output " + output.Name + " | request will be for " + resourceRequest + " | " + (resourceRequest > 0 ? "curAvalable" : "curFreeAmount") + " = " + (resourceRequest > 0 ? output.CurAvailable : output.CurFreeAmount) + (resourceRequest != 0 ? (" | limitFactor " + limitFactor + " | outputEfficency set to " + outputModifier) : ""));
#endif
			}

			inputModifier = outputModifier = Math.Min(inputModifier, outputModifier);

			//inputModifier *= outputModifier;
			//if (inputModifier > 1)
			//	inputModifier = 1;
			//if (inputModifier < 1)
			//	outputModifier *= inputModifier;

#if DEBUG_UPDATES
			Debug.Log ("inputEffectOnEfficiency: " + inputEffectOnEfficiency);
			Debug.Log("IonModuleGenerator.CalculateModifiers(): inputModifier " + inputModifier + " | outputModifier " + outputModifier);
#endif
		}


		/************************************************************************\
         * IonModuleGenerator class                                             *
         * CalculateModifiersQuick function                                     *
         *                                                                      *
         * Runs a quick calculatation of what limits should be put on input and *
         * output modifiers based on the avalability of resources or free space *
         * as applicable. outputModifier may start out and end up > 1, in which *
         * case the generator will produce more output resource for the normal  *
         * amount of input.                                                     *
         * inputModifier should not be > 1.                                     *
        \************************************************************************/
		public virtual void CalculateModifiersQuick(double deltaTime)
		{
#if DEBUG_UPDATES
            Debug.Log("IonModuleGenerator.CalculateModifiersQuick() " + this.part.name + " " + generatorName);
            Debug.Log("IonModuleGenerator.CalculateModifiersQuick(): inputModifier " + inputModifier + " | outputModifier " + outputModifier);
#endif
			int crew = part.protoModuleCrew.Count;
			int crewCapacity = part.CrewCapacity;

			double limitFactor = 1;
			double resourceRequest;
			double startingOutputMod = outputModifier;

			//Cycle Through inputs and determine which, if any, have limits on space or amount
			foreach (IonGeneratorResourceData input in listInputs)
			{
				//calculate amount and free space
				double amount;
				double maxAmount;
				this.part.GetConnectedResourceTotals(input.ID, PartResourceLibrary.GetDefaultFlowMode(input.ID), out amount, out maxAmount, true);

				input.CurAvailable = 0;
				input.CurFreeAmount = 0;

				input.CurAvailable = amount;
				input.CurFreeAmount = maxAmount - amount;

				//calculate how much will be requested
				resourceRequest = (input.RateBase + input.RatePerKerbal * crew + input.RatePerCapacity * crewCapacity) * deltaTime * outputLevel;

				//calculate limitFactor
				if (!resourceRequest.Equals(0d))
				{
					limitFactor = (resourceRequest > 0 ? input.CurAvailable : -input.CurFreeAmount) / resourceRequest; //if resourceRequest > 0 use curAvalable, else use -curFreeAmount (- to keep limit factor +)

					//if this is a required resouce and it is limited
					//Electric charge requirment is ignored for the quick version
					if (1f == input.EffectOnEfficency && limitFactor < 1 && input.Name != "ElectricCharge")
					{
						input.Low = true;
						inputModifier = limitFactor < inputModifier ? limitFactor : inputModifier; //inputEfficency = Math.Min(limitFactor, inputEfficency);
					}
				}
#if DEBUG_UPDATES
                Debug.Log("IonModuleGenerator.CalculateModifiersQuick(): Looking at Input " + input.Name + " | request will be for " + resourceRequest + " | " + (resourceRequest > 0 ? "curAvalable" : "curFreeAmount") + " = " + (resourceRequest > 0 ? input.CurAvailable : input.CurFreeAmount) + (resourceRequest != 0 ? (" | limitFactor " + limitFactor + " | efficency set to " + efficency) : ""));
#endif
			}


			//Cycle Through outputs and determine which, if any, have limits on space or amount
			foreach (IonGeneratorResourceData output in listOutputs)
			{
				//calculate amount and free space
				double amount;
				double maxAmount;
				this.part.GetConnectedResourceTotals(output.ID, PartResourceLibrary.GetDefaultFlowMode(output.ID), out amount, out maxAmount, true);

				output.CurAvailable = 0;
				output.CurFreeAmount = 0;

				output.CurAvailable = amount;
				output.CurFreeAmount = maxAmount - amount;

				output.CurAvailable *= 1.0f - output.CutoffMargin;
				output.CurFreeAmount *= 1.0f - output.CutoffMargin;

				//calculate how much will be requested
				resourceRequest = -(output.RateBase + output.RatePerKerbal * crew + output.RatePerCapacity * crewCapacity) * deltaTime * outputLevel;

				//calculate limitFactor
				if (!(resourceRequest > 0d || resourceRequest < 0d))
				{
					limitFactor = (resourceRequest > 0 ? output.CurAvailable : -output.CurFreeAmount) / resourceRequest; //if resourceRequest > 0 use curAvalable, else use -curFreeAmount (- to keep limit factor +)

					//if nessarry, adjust outputEfficency (outputEfficency can be > 1)
					if (limitFactor < outputModifier && output.EffectOnEfficency >= 1)
						outputModifier = limitFactor;
				}
#if DEBUG_UPDATES
                Debug.Log("IonModuleGenerator.CalculateModifiersQuick(): Looking at Output " + output.Name + " | request will be for " + resourceRequest + " | " + (resourceRequest > 0 ? "curAvalable" : "curFreeAmount") + " = " + (resourceRequest > 0 ? output.CurAvailable : output.CurFreeAmount) + (resourceRequest != 0 ? (" | limitFactor " + limitFactor + " | outputEfficency set to " + outputModifier) : ""));
#endif
			}

			inputModifier = outputModifier = Math.Min(inputModifier, outputModifier);

#if DEBUG_UPDATES
            Debug.Log("IonModuleGenerator.CalculateModifiersQuick(): inputModifier " + inputModifier + " | outputModifier " + outputModifier);
#endif
		}


		/************************************************************************\
         * IonModuleGenerator class                                             *
         * ConsumeResources function                                            *
         *                                                                      *
         * Runs the module's resource use simulation over deltaTime.            *
        \************************************************************************/
		public override bool ConsumeResources(double deltaTime)
        {
#if DEBUG_UPDATES
            Debug.Log("IonModuleGenerator.ConsumeResources() " + this.part.name + " " + generatorName);
            Debug.Log("IonModuleGenerator.ConsumeResources(): inputModifier " + inputModifier + " | outputModifier " + outputModifier);
#endif
            bool allResourcesMet = true;

            int crew = part.protoModuleCrew.Count;
            int crewCapacity = part.CrewCapacity;

            double limitFactor;
            double resourceRequest;
            double resourceReturn;

#if DEBUG_UPDATES
			if (listInputs.Count == 0)
			{
				Debug.Log("IonModuleGenerator.ConsumeResources(): listInputs empty!");
			}
#endif

			//Process Inputs
			foreach (IonGeneratorResourceData input in listInputs)
            {
                resourceRequest = (input.RateBase + input.RatePerKerbal * crew + input.RatePerCapacity * crewCapacity) * deltaTime * outputLevel * inputModifier;
#if DEBUG_UPDATES
				Debug.Log("Input request for " + input.Name + " = " + resourceRequest.ToString());
#endif


                //Check if the resource is limited
				if (!resourceRequest.Equals(0d))
                {
                    limitFactor = (resourceRequest > 0 ? input.CurAvailable : input.CurFreeAmount) / resourceRequest; //if resourceRequest > 0 use curAvalable, else use curFreeAmount

                    if (limitFactor < 1)
                    {
                        input.Low = true;

                        if (input.EffectOnEfficency <= 1.0f)
                        {
                            outputModifier *= (1.0f - input.EffectOnEfficency * (1.0 - limitFactor));
#if DEBUG_UPDATES
                            Debug.Log("IonModuleGenerator.ConsumeResources(): Insufficent " + input.Name + " to fill request, outputEfficency lowered to " + outputModifier);
#endif
                        }
                        else //(THIS SHOULD NOT HAPPEN with the checks in CalculateModifiers) 
                        {
							// Starwaster... well of COURSE it should happen. We ONLY get here if the input was explicitly configured to be LESS than 1. 
							//Modify efficency by the change in inputEfficency
							// changed check above to <= 1 so we only get here if effectOnEfficiency > 1 which would be bad
							outputModifier *= limitFactor;
                            inputModifier *= limitFactor;
#if DEBUG_UPDATES
                            Debug.Log("IonModuleGenerator.ConsumeResources(): ERROR: INSUFFICENT REQUIRED RESOURCE " + input.Name + " to fill request, inputEfficency lowered to " + inputModifier + ", outputEfficency lowered to " + outputModifier);
#endif
                        }

                        //Modify the resourceRequest by the limitFactor
                        resourceRequest *= limitFactor;
                    }
                }

                //Check if the resource is low enough to be considered depleated
                if (((input.RateBase + input.RatePerKerbal * crew + input.RatePerCapacity * crewCapacity) > 0 ? input.CurAvailable : input.CurFreeAmount) < 0.001)
                {
                    generatorStatusL2 = input.Name + " depleted";
                    input.Depleated = true;
                }

                //Request resource and add the rate to the display module
				resourceReturn = RequestResource(input.Name, resourceRequest, input.FlowMode);
                input.AddDisplayRate((float)resourceReturn);
#if DEBUG_UPDATES
                Debug.Log("IonModuleGenerator.ConsumeResources(): requesting " + resourceRequest + " of " + input.Name);
                Debug.Log("IonModuleGenerator.ConsumeResources(): returning  " + resourceReturn + " of " + input.Name);
#endif

                //Check if resource was met (THIS SHOULD NOT HAPPEN with the limitFactor checks)
                if (Math.Abs(resourceRequest - resourceReturn) * 1000 > Math.Abs(resourceRequest))
                {
                    allResourcesMet = false;
#if DEBUG_UPDATES
                    Debug.Log("IonModuleGenerator.ConsumeResources(): ERROR: RESOURCE REQUEST NOT MET FOR " + input.Name);
#endif
                }
            }

#if DEBUG_UPDATES
			if (listOutputs.Count == 0)
			{
				Debug.Log("IonModuleGenerator.ConsumeResources(): listOutputs empty!");
			}
#endif

			//Process Outputs
			foreach (IonGeneratorResourceData output in listOutputs)
            {
                resourceRequest = -(output.RateBase + output.RatePerKerbal * crew + output.RatePerCapacity * crewCapacity) * deltaTime * outputLevel * outputModifier;

#if DEBUG_UPDATES
				Debug.Log("Output request for " + output.Name + " = " + resourceRequest.ToString());
#endif

				resourceReturn = RequestResource(output.Name, resourceRequest, output.FlowMode);

                output.AddDisplayRate((float)resourceReturn);
#if DEBUG_UPDATES
                Debug.Log("IonModuleGenerator.ConsumeResources(): requesting " + resourceRequest + " of " + output.Name);
                Debug.Log("IonModuleGenerator.ConsumeResources(): returning  " + resourceReturn + " of " + output.Name);
#endif
            }

            //set efficency
            efficency = outputModifier / inputModifier;

            return allResourcesMet;
        }



        /************************************************************************\
         * IonModuleGenerator class                                             *
         * ConsumeResourcesQuick function                                       *
         *                                                                      *
         * Runs a quicker aproximation of the module's resource use simulation  *
         * over deltaTime.                                                      *
        \************************************************************************/
        public override void ConsumeResourcesQuick(double deltaTime)
        {
#if DEBUG_UPDATES
            Debug.Log("IonModuleGenerator.ConsumeResourcesQuick() " + this.part.name + " " + generatorName);
            Debug.Log("IonModuleGenerator.ConsumeResourcesQuick(): inputModifier " + inputModifier + " | outputModifier " + outputModifier);
#endif
            int crew = part.protoModuleCrew.Count;
            int crewCapacity = part.CrewCapacity;

            double resourceRequest;
            double resourceReturn;


            //Process Inputs
            foreach (IonGeneratorResourceData input in listInputs)
            {
                resourceRequest = (input.RateBase + input.RatePerKerbal * crew + input.RatePerCapacity * crewCapacity) * deltaTime * outputLevel * inputModifier;
				resourceReturn = RequestResource(input.Name, resourceRequest);
#if DEBUG_UPDATES
                Debug.Log("IonModuleGenerator.ConsumeResourcesQuick(): requesting " + resourceRequest + " of " + input.Name);
                Debug.Log("IonModuleGenerator.ConsumeResourcesQuick(): returning  " + resourceReturn + " of " + input.Name);
#endif
            }

            //Process Outputs
            foreach (IonGeneratorResourceData output in listOutputs)
            {
                resourceRequest = -(output.RateBase + output.RatePerKerbal * crew + output.RatePerCapacity * crewCapacity) * deltaTime * outputLevel * outputModifier;
				resourceReturn = RequestResource(output.Name, resourceRequest);
#if DEBUG_UPDATES
                Debug.Log("IonModuleGenerator.ConsumeResourcesQuick(): requesting " + resourceRequest + " of " + output.Name);
                Debug.Log("IonModuleGenerator.ConsumeResourcesQuick(): returning  " + resourceReturn + " of " + output.Name);
#endif
            }

            //set efficency
            efficency = outputModifier / inputModifier;
        }


        /************************************************************************\
         * IonModuleGenerator class                                             *
         * GetResourceUsage function                                            *
         *                                                                      *
         * Calculates the module's resource usage over deltaTime and returns a  *
         * list of all resources used and their amounts.                        *
        \************************************************************************/
        public override List<IonResourceData> GetResources()
        {
#if DEBUG
            Debug.Log("IonModuleGenerator.GetResources() " + this.part.name + " " + generatorName);
#endif
            List<IonResourceData> listResources = new List<IonResourceData>();

            foreach (IonGeneratorResourceData genResource in listInputs)
            {
                //IonResourceData resourceData = new IonResourceData(genResource);
                //listResources.Add(resourceData);
                listResources.Add(genResource);
#if DEBUG
                Debug.Log("IonModuleGenerator.GetResources(): adding " + genResource.Name);
#endif
            }

            foreach (IonGeneratorResourceData genResource in listOutputs)
            {
                //IonResourceData resourceData = new IonResourceData(genResource);
                //listResources.Add(resourceData);
                listResources.Add(genResource);
#if DEBUG
                Debug.Log("IonModuleGenerator.GetResources(): adding " + genResource.Name);
#endif
            }
            return listResources;
        }


        /************************************************************************\
         * IonModuleGenerator class                                             *
         * isAble function                                                      *
         *                                                                      *
         * Tests whether the generator is able to operate under current         *
         * conditions.                                                          *
        \************************************************************************/
        public virtual bool isAble()
        {
            return true;
        }


        /************************************************************************\
         * IonModuleGenerator class                                             *
         * isListSubNode function                                               *
         *                                                                      *
         * Tests whether nodeName corresponds to an input/output list.          *
        \************************************************************************/
        public virtual bool isListSubNode(string nodeName)
        {
            return "INPUT_RESOURCE" == nodeName || "OUTPUT_RESOURCE" == nodeName;
        }


        /************************************************************************\
         * IonModuleGenerator class                                             *
         * GetCorrespondingList function                                        *
         *                                                                      *
         * Returns the input/output list that corresponds to nodeName.          *
         * Returns null if there is no corresponding list.                      *
        \************************************************************************/
        public override List<IonResourceData> GetCorrespondingList(string nodeName)
        {
            if ("INPUT_RESOURCE" == nodeName)
                return listInputs;
            if ("OUTPUT_RESOURCE" == nodeName)
                return listOutputs;

            return null;
        }

        public override IonResourceData CreateResourceEntry(ConfigNode node)
        {
            return new IonGeneratorResourceData(node);
        }


        /************************************************************************\
         * IonModuleGenerator class                                             *
         * FindGeneratorConfigNode function                                     *
         *                                                                      *
         * Returns a IonSupportGenerator object containing data loaded from the *
         * ConfigNode of ION_POD_GENERATOR with a generatorName value matching  *
         * this generator.                                                      *
        \************************************************************************/
        public virtual IonGeneratorData FindGeneratorConfigNode()
        {
#if DEBUG
            Debug.Log("IonModuleGenerator.FindGeneratorConfigNode() " + this.part.name + " " + generatorName);
#endif
            IonGeneratorData supportGenerator = null;

            //Pull default values from config nodes
            //Loops through all ION_POD_GENERATOR configNodes in the GameDatabase
            foreach (ConfigNode generatorNode in GameDatabase.Instance.GetConfigNodes("ION_POD_GENERATOR"))
            {
#if DEBUG
                Debug.Log("IonModuleGenerator.FindGeneratorConfigNode(): ION_POD_GENERATOR node found\n" + generatorNode.ToString());
#endif
                if (generatorNode.HasValue("generatorName") && generatorNode.GetValue("generatorName") == generatorName)
                {
                    supportGenerator = new IonGeneratorData();
                    supportGenerator.Load(generatorNode);
                    break;
                }
            }

            return supportGenerator;
        }


        /************************************************************************\
         * IonModuleGenerator class                                             *
         * findGeneratorModule functions                                        *
         *                                                                      *
         * Finds an IonModuleCollector for resourceName on part and returns it. *
         * If one does not exisit it returns null.                              *
        \************************************************************************/
        public static IonModuleGenerator findGeneratorModule(Part part, IonGeneratorData supportGenerator)
        {
#if DEBUG
            Debug.Log("IonModuleGenerator.findGeneratorModule() " + part.name + " " + supportGenerator.generatorName);
#endif
            IonModuleGenerator generatorModule = null;


            if (null != part.Modules)
            {
                foreach (PartModule module in part.Modules)
                {
                    if (module is IonModuleGenerator && ((IonModuleGenerator)module).generatorName == supportGenerator.generatorName)
                    {
#if DEBUG
                        Debug.Log("IonModuleGenerator.findGeneratorModule(): " + supportGenerator.generatorName + " module found");
#endif
                        generatorModule = (IonModuleGenerator)module;
                        break;
                    }
                }
            }

            return generatorModule;
        }


        /************************************************************************\
         * IonModuleGenerator class                                             *
         * findGeneratorModule functions                                        *
         *                                                                      *
         * Finds an IonModuleCollector for resourceName on part and returns it. *
         * If one does not exisit it creates one and returns it.                *
        \************************************************************************/
        public static IonModuleGenerator findAndCreateGeneratorModule(Part part, IonGeneratorData supportGenerator, string ModuleClass)
        {
#if DEBUG
            Debug.Log("IonModuleGenerator.findAndCreateGeneratorModule() " + part.name + " " + supportGenerator.generatorName);
#endif
            IonModuleGenerator generatorModule = IonModuleGenerator.findGeneratorModule(part, supportGenerator);

            if (null == generatorModule)
            {
#if DEBUG
                Debug.Log("IonModuleGenerator.findAndCreateGeneratorModule(): " + supportGenerator.generatorName + " module not found, creating new");
#endif
                try { generatorModule = (IonModuleGenerator)part.AddModule(ModuleClass); }
                catch (NullReferenceException)
                {
#if DEBUG
                    Debug.Log("IonModuleGenerator.findAndCreateGeneratorModule(): NULL REFERENCE EXCEPTION CAUGHT! part.Modules was probablly null");
#endif
                    return null;
                }

                generatorModule.generatorName = supportGenerator.generatorName;
                if (null != supportGenerator.generatorGUIName && supportGenerator.generatorGUIName.Length > 0)
                    generatorModule.generatorGUIName = supportGenerator.generatorGUIName;
                else
                    generatorModule.generatorGUIName = supportGenerator.generatorName;
            }

            return generatorModule;
        }
    }
    //==========================================================================================================
    // END of IonModuleGeneratorBase Class
    //==========================================================================================================
}