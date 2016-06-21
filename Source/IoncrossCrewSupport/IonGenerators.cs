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

        public string generatorName = "";

        public string generatorGUIName = "";
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

        public float outputLevel = 1f;
        public float outputLevelStep = 0.1f;
        public float outputLevelMin = 0.0f;
        public float outputLevelMax = 1.0f;

        public double inputModifier;
        public double outputModifier;

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

            if (!hideActivateControls && IonLifeSupportScenario.Instance.IsLifeSupportEnabled)
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
            }

            return strInfo;
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
        }

        /************************************************************************\
         * IonModuleGenerator class                                             *
         * InitializeValues function                                             *
         *                                                                      *
        \************************************************************************/
        
        public override void InitializeValues()
        {
            base.InitializeValues();
#if DEBUG
            Debug.Log("IonModuleGenerator.InitializeValues() " + this.part.name + " " + generatorName);
#endif
            //Assign default values
            generatorName = "";
            generatorGUIName = "";

            hideStatus = false;
            hideStatusL2 = false;
            hideEfficency = false;
            hideOutputControls = false;
            hideActivateControls = false;

            //isActive = false;
            startOn = false;
            //alwaysOn = false;

            outputLevel = 1.0f;
            outputLevelStep = 0.1f;
            outputLevelMin = 0.0f;
            outputLevelMax = 1.0f;
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
            //Read variables from node
            if (node.HasValue("generatorName"))
                generatorName = node.GetValue("generatorName");
            if (node.HasValue("generatorGUIName"))
                generatorGUIName = node.GetValue("generatorGUIName");
            
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

            if (node.HasValue("isActive"))
                //isActive = "True" == node.GetValue("isActive") || "true" == node.GetValue("isActive")  || "TRUE" == node.GetValue("isActive");
                bool.TryParse(node.GetValue("isActive"), out isActive);
            else if (node.HasValue("startOn"))
                //isActive = startOn = "True" == node.GetValue("startOn") || "true" == node.GetValue("startOn") || "TRUE" == node.GetValue("startOn");
                bool.TryParse(node.GetValue("startOn"), out isActive);

            if (node.HasValue("alwaysOn"))
                alwaysOn = "True" == node.GetValue("alwaysOn") || "true" == node.GetValue("alwaysOn") || "TRUE" == node.GetValue("alwaysOn");

            if (node.HasValue("outputLevel"))
                outputLevel = Convert.ToSingle(node.GetValue("outputLevel"));
            if (node.HasValue("outputLevelStep"))
                outputLevelStep = Convert.ToSingle(node.GetValue("outputLevelStep"));
            if (node.HasValue("outputLevelMin"))
                outputLevelMin = Convert.ToSingle(node.GetValue("outputLevelMin"));
            if (node.HasValue("outputLevelMax"))
                outputLevelMax = Convert.ToSingle(node.GetValue("outputLevelMax"));
            

            //Read and process nodes
            foreach (ConfigNode subNode in node.nodes)
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
            listResourceNodes = new List<ConfigNode>();
            listInputs = new List<IonResourceData>();
            listOutputs = new List<IonResourceData>();

            InitializeValues();
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
#if DEBUG
            Debug.Log("IonModuleGenerator.OnSave() " + this.part.name + " " + generatorName);
#endif
            //Save variables
            
            node.AddValue("generatorName", generatorName);
            node.AddValue("generatorGUIName", generatorGUIName);
            
            node.AddValue("isActive", isActive);

            node.AddValue("alwaysOn", alwaysOn);
            node.AddValue("outputLevel", outputLevel);
            node.AddValue("outputLevelStep",outputLevelStep);
            node.AddValue("outputLevelMin",outputLevelMin);
            node.AddValue("outputLevelMax",outputLevelMax);

            node.AddValue("hideStatus",hideStatus);
            node.AddValue("hideStatusL2",hideStatusL2);
            node.AddValue("hideEfficency",hideEfficency);
            node.AddValue("hideOutputControls",hideOutputControls);
            node.AddValue("hideActivateControls",hideActivateControls);
            
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
                listInputs = new List<IonResourceData>();
                listOutputs = new List<IonResourceData>();
                ProcessNodestoList(listResourceNodes);
            }
            listResourceNodes = null;

            base.OnStart(state);
#if DEBUG
            Debug.Log("IonModuleGenerator.OnStart() " + this.part.name + " " + generatorName);
            Debug.Log("IonModuleGenerator.OnStart(): state " + state.ToString());
#endif


            //Attach display modules
            foreach (IonResourceData resource in GetResources())
            {
                resource.DisplayModule = IonModuleDisplay.findDisplayModule(this.part, resource);
            }

            //Hide unwanted displays, buttons, and actions
			Fields["generatorStatus"].guiActive = !hideStatus && IonLifeSupportScenario.Instance.IsLifeSupportEnabled;
			Fields["generatorStatusL2"].guiActive = !hideStatusL2 && IonLifeSupportScenario.Instance.IsLifeSupportEnabled;
			Fields["efficency"].guiActive = !hideEfficency && IonLifeSupportScenario.Instance.IsLifeSupportEnabled;

            if(hideOutputControls)
            {
                Fields["outputLevelDisplay"].guiActive = false;

                Events["IncreaseButton"].active = false;
                Events["DecreaseButton"].active = false;

                Actions["IncreaseAction"].active = false;
                Actions["DecreaseAction"].active = false;
            }

            if(hideActivateControls || alwaysOn || !IonLifeSupportScenario.Instance.IsLifeSupportEnabled)
            {
                Events["ActivateButton"].active = false;
                Events["ShutdownButton"].active = false;

                Actions["ActivateAction"].active = false;
                Actions["ShutdownAction"].active = false;
                Actions["ToggleAction"].active = false;
            }
			else if(IonLifeSupportScenario.Instance.IsLifeSupportEnabled)
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
            if (alwaysOn || (startOn && lastLoaded < 0))
            {
                isActive = true;
            }
            SetGeneratorState(isActive);
        }


        /************************************************************************\
         * IonModuleGenerator class                                             *
         * FixedUpdate function override                                           *
         *                                                                      *
        \************************************************************************/
        public override void FixedUpdate()
        {
			if(IonLifeSupportScenario.Instance.IsLifeSupportEnabled && HighLogic.LoadedSceneIsFlight)
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
	                CalculateModifiers(TimeWarp.fixedDeltaTime);
	                allResourcesMet = ConsumeResources(TimeWarp.fixedDeltaTime);
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

                        if (input.EffectOnEfficency < 1.0f)
                        {
                            outputModifier *= (1.0f - input.EffectOnEfficency * (1.0 - limitFactor));
#if DEBUG_UPDATES
                            Debug.Log("IonModuleGenerator.ConsumeResources(): Insufficent " + input.Name + " to fill request, outputEfficency lowered to " + outputModifier);
#endif
                        }
                        else //(THIS SHOULD NOT HAPPEN with the checks in CalculateEfficencies)
                        {
                            //Modify efficency by the change in inputEfficency
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
                resourceReturn = RequestResource(input.ID, resourceRequest);
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

            //Process Outputs
            foreach (IonGeneratorResourceData output in listOutputs)
            {
                resourceRequest = -(output.RateBase + output.RatePerKerbal * crew + output.RatePerCapacity * crewCapacity) * deltaTime * outputLevel * outputModifier;

#if DEBUG_UPDATES
				Debug.Log("Output request for " + output.Name + " = " + resourceRequest.ToString());
#endif

				resourceReturn = RequestResource(output.ID, resourceRequest);

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
            double startingOutputMod = outputModifier;
			double inputEffectOnEfficiency = 1.0;

            //Cycle Through inputs and determine which, if any, have limits on space or amount
            foreach (IonGeneratorResourceData input in listInputs)
            {
                //calculate amount and free space
                List<PartResource> connectedResources = new List<PartResource>();
				this.part.GetConnectedResources(input.ID, PartResourceLibrary.GetDefaultFlowMode(input.ID), connectedResources);

                input.CurAvailable = 0;
                input.CurFreeAmount = 0;
                foreach (PartResource pResource in connectedResources)
                {
                    input.CurAvailable += pResource.amount;
                    input.CurFreeAmount += pResource.maxAmount - pResource.amount;
                }

                //calculate how much will be requested
                resourceRequest = (input.RateBase + input.RatePerKerbal * crew + input.RatePerCapacity * crewCapacity) * deltaTime * outputLevel;

                //calculate limitFactor
				if (resourceRequest.Equals(0d))
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
				inputEffectOnEfficiency *= input.EffectOnEfficency;
            }


            //Cycle Through outputs and determine which, if any, have limits on space or amount
            foreach (IonGeneratorResourceData output in listOutputs)
            {
                //calculate amount and free space
                List<PartResource> connectedResources = new List<PartResource>();
				//this.part.GetConnectedResources(output.ID, ResourceFlowMode.ALL_VESSEL, connectedResources);
				this.part.GetConnectedResources(output.ID, PartResourceLibrary.GetDefaultFlowMode (output.ID), connectedResources);

                output.CurAvailable = 0;
                output.CurFreeAmount = 0;
                foreach (PartResource pResource in connectedResources)
                {
                    output.CurAvailable += pResource.amount;
                    output.CurFreeAmount += pResource.maxAmount - pResource.amount;
                }
                output.CurAvailable *= 1.0f - output.CutoffMargin;
                output.CurFreeAmount *= 1.0f - output.CutoffMargin;

                //calculate how much will be requested
                resourceRequest = -(output.RateBase + output.RatePerKerbal * crew + output.RatePerCapacity * crewCapacity) * deltaTime * outputLevel;

                //calculate limitFactor
				if (resourceRequest.Equals(0d))
                {
                    limitFactor = (resourceRequest > 0 ? output.CurAvailable : -output.CurFreeAmount) / resourceRequest; //if resourceRequest > 0 use curAvalable, else use -curFreeAmount (- to keep limit factor +)

                    //if nessarry, adjust outputModifier (outputModifier can be > 1)
                    if (limitFactor < outputModifier && 1f == output.EffectOnEfficency)
                        outputModifier = limitFactor;
                }
#if DEBUG_UPDATES
                Debug.Log("IonModuleGenerator.CalculateModifiers(): Looking at Output " + output.Name + " | request will be for " + resourceRequest + " | " + (resourceRequest > 0 ? "curAvalable" : "curFreeAmount") + " = " + (resourceRequest > 0 ? output.CurAvailable : output.CurFreeAmount) + (resourceRequest != 0 ? (" | limitFactor " + limitFactor + " | outputEfficency set to " + outputModifier) : ""));
#endif
            }

            inputModifier *= outputModifier / startingOutputMod;
            if (inputModifier > 1)
                    inputModifier = 1;
            if (inputModifier < 1)
                outputModifier *= Math.Max(inputModifier, (1.0 - inputEffectOnEfficiency));

#if DEBUG_UPDATES
			Debug.Log ("inputEffectOnEfficiency: " + inputEffectOnEfficiency);
			Debug.Log("IonModuleGenerator.CalculateModifiers(): inputModifier " + inputModifier + " | outputModifier " + outputModifier);
#endif
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
                resourceReturn = RequestResource(input.ID, resourceRequest);
#if DEBUG_UPDATES
                Debug.Log("IonModuleGenerator.ConsumeResourcesQuick(): requesting " + resourceRequest + " of " + input.Name);
                Debug.Log("IonModuleGenerator.ConsumeResourcesQuick(): returning  " + resourceReturn + " of " + input.Name);
#endif
            }

            //Process Outputs
            foreach (IonGeneratorResourceData output in listOutputs)
            {
                resourceRequest = -(output.RateBase + output.RatePerKerbal * crew + output.RatePerCapacity * crewCapacity) * deltaTime * outputLevel * outputModifier;
                resourceReturn = RequestResource(output.ID, resourceRequest);
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
                List<PartResource> connectedResources = new List<PartResource>();
				this.part.GetConnectedResources(input.ID, PartResourceLibrary.GetDefaultFlowMode(input.ID), connectedResources);

                input.CurAvailable = 0;
                input.CurFreeAmount = 0;
                foreach (PartResource pResource in connectedResources)
                {
                    input.CurAvailable += pResource.amount;
                    input.CurFreeAmount += pResource.maxAmount - pResource.amount;
                }

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
                List<PartResource> connectedResources = new List<PartResource>();
				this.part.GetConnectedResources(output.ID, PartResourceLibrary.GetDefaultFlowMode(output.ID), connectedResources);

                output.CurAvailable = 0;
                output.CurFreeAmount = 0;
                foreach (PartResource pResource in connectedResources)
                {
                    output.CurAvailable += pResource.amount;
                    output.CurFreeAmount += pResource.maxAmount - pResource.amount;
                }
                output.CurAvailable *= 1.0f - output.CutoffMargin;
                output.CurFreeAmount *= 1.0f - output.CutoffMargin;

                //calculate how much will be requested
                resourceRequest = -(output.RateBase + output.RatePerKerbal * crew + output.RatePerCapacity * crewCapacity) * deltaTime * outputLevel;

                //calculate limitFactor
				if (!(resourceRequest > 0d || resourceRequest < 0d))
                {
                    limitFactor = (resourceRequest > 0 ? output.CurAvailable : -output.CurFreeAmount) / resourceRequest; //if resourceRequest > 0 use curAvalable, else use -curFreeAmount (- to keep limit factor +)

                    //if nessarry, adjust outputEfficency (outputEfficency can be > 1)
                    if (limitFactor < outputModifier && 1 == output.EffectOnEfficency)
                        outputModifier = limitFactor;

                }
#if DEBUG_UPDATES
                Debug.Log("IonModuleGenerator.CalculateModifiersQuick(): Looking at Output " + output.Name + " | request will be for " + resourceRequest + " | " + (resourceRequest > 0 ? "curAvalable" : "curFreeAmount") + " = " + (resourceRequest > 0 ? output.CurAvailable : output.CurFreeAmount) + (resourceRequest != 0 ? (" | limitFactor " + limitFactor + " | outputEfficency set to " + outputModifier) : ""));
#endif
            }


            inputModifier *= outputModifier / startingOutputMod;
            if (inputModifier > 1)
                inputModifier = 1;
            if (inputModifier < 1)
                outputModifier *= inputModifier;

#if DEBUG_UPDATES
            Debug.Log("IonModuleGenerator.CalculateModifiersQuick(): inputModifier " + inputModifier + " | outputModifier " + outputModifier);
#endif
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