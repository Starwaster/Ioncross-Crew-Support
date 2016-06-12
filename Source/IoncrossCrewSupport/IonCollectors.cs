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
     * IonModuleCollectorLanded Class                       *
     *                                                      *
     * Subclass of IonModuleGenerator to handle resource    *
     * collection when landed or splashed on a planet       *
    \*======================================================*/
    public class IonModuleCollectorLanded : IonModuleGenerator
    {

        [KSPField(isPersistant = false)]
        public bool requiresLand = false;

        [KSPField(isPersistant = false)]
        public bool requiresWater = false;
        

        /******************\
         * Display Fields *
        \******************/

        /*****************************\
         * Activate/Deactive Actions *
        \*****************************/

        /*****************************\
         * Activate/Deactive Buttons *
        \*****************************/


        /********************************************\
         * Set Generator Status and Level Functions *
         * Used by above actions and buttons.       *
        \********************************************/


        /************************************************************************\
         * IonModuleCollectorLanded Class                                       *
         * OnAwake function override                                            *
         *                                                                      *
        \************************************************************************/
        public override void OnAwake()
        {
            base.OnAwake();
#if DEBUG
            Debug.Log("IonModuleCollectorBase.OnAwake() " + this.part.name + " " + generatorName);
#endif
        }


        /************************************************************************\
         * IonModuleCollectorLanded Class                                       *
         * OnLoad function override                                             *
         *                                                                      *
        \************************************************************************/
        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
#if DEBUG
            Debug.Log("IonModuleCollectorBase.OnLoad() " + this.part.name + " " + generatorName);
            Debug.Log("IonModuleCollectorBase.OnLoad(): node\n" + node.ToString());
#endif            
        }

        /************************************************************************\
         * IonModuleCollectorLanded Class                                       *
         * OnSave function override                                             *
         *                                                                      *
        \************************************************************************/
        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
#if DEBUG
            Debug.Log("IonModuleCollectorBase.OnSave() " + this.part.name + " " + generatorName);
            Debug.Log("IonModuleCollectorBase.OnSave(): node\n" + node.ToString());
#endif
            
        }

        /************************************************************************\
         * IonModuleCollectorLanded Class                                       *
         * OnStart function override                                            *
         *                                                                      *
        \************************************************************************/
        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
#if DEBUG
            Debug.Log("IonModuleCollectorBase.OnStart() " + this.part.name + " " + generatorName);
            Debug.Log("IonModuleCollectorBase.OnStart(): state " + state.ToString());
#endif           
        }

        /************************************************************************\
         * IonModuleCollectorLanded Class                                       *
         * FixedUpdate function override                                           *
         *                                                                      *
        \************************************************************************/
        public override void FixedUpdate()
        {
			if(IonLifeSupportScenario.Instance.IsLifeSupportEnabled)
			{
	            base.FixedUpdate();
#if DEBUG_UPDATES
	            Debug.Log("IonModuleCollectorBase.FixedUpdate() " + this.part.name + " " + generatorName);
#endif
			}
        }


        /************************************************************************\
         * IonModuleCollectorLanded class                                       *
         * isAble function                                                      *
         * Tests whether the generator is able to operate under current         *
         * conditions.                                                          *
        \************************************************************************/
        public override bool isAble()
        {
            return requiresLand && this.part.vessel.Landed || requiresWater && !this.part.vessel.Splashed || !requiresWater && !requiresLand && this.part.vessel.LandedOrSplashed;
        }
    }
    //==========================================================================================================
    // END of IonModuleCollectorBase Class
    //==========================================================================================================



    /*======================================================*\
     * IonModuleCollector Class                   *
     *                                                      *
     * Subclass of IonModuleGenerator to handle resource    *
     * collection from the atmosphere.                      *
    \*======================================================*/
    public class IonModuleCollector : IonModuleGenerator
    {
        public List<IonResourceData> listOutputs_oxygen;
        public List<IonResourceData> listOutputs_noOxygen;

        [KSPField(isPersistant = false)]
        public float minAtmosphere = 0.001f;

        [KSPField(isPersistant = false)]
        public bool isAutomaticOxygen = false;

        [KSPField(isPersistant = false)]
        public bool isAutomaticNoOxygen = false;

        [KSPField(isPersistant = false)]
        public bool hideAtmoContents = false;
        /******************\
         * Display Fields *
        \******************/
        [KSPField(guiActive = true, guiName = "Atmo Contents", isPersistant = false)]
        public string atmosphereContents;


        /********************************************\
         * Set Generator Status and Level Functions *
         * Used by above actions and buttons.       *
        \********************************************/
        protected override void SetGeneratorState(bool generatorState)
        {
            base.SetGeneratorState(generatorState);
			if (isAutomaticOxygen || isAutomaticNoOxygen || !IonLifeSupportScenario.Instance.IsLifeSupportEnabled)
            {
                Events["ActivateButton"].active = false;
                Events["ShutdownButton"].active = false;
            }
        }


        /************************************************************************\
         * IonModuleCollector class                                   *
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
         * IonModuleCollector class                                   *
         * GetInfoLists function                                                *
         *                                                                      *
         * Helper function for getInfo.                                         *
        \************************************************************************/
        protected override string GetInfoLists()
        {
            string strInfo = base.GetInfoLists();

            if (listOutputs_oxygen.Count > 0)
            {
                strInfo += "  Output Resources (Oxygen)\n";
                strInfo += getInfoList(listOutputs_oxygen, 4);
            }

            if (listOutputs_noOxygen.Count > 0)
            {
                strInfo += "  Output Resources (Non-Oxygen)\n";
                strInfo += getInfoList(listOutputs_noOxygen, 4);
            }

            return strInfo;
        }


        /************************************************************************\
         * IonModuleCollector class                                   *
         * OnAwake function override                                            *
         *                                                                      *
        \************************************************************************/
        public override void  OnAwake()
        {
            base.OnAwake();
#if DEBUG
            Debug.Log("IonModuleCollector.OnAwake() " + this.part.name + " " + generatorName);
#endif
        }

        /************************************************************************\
         * IonModuleCollector class                                   *
         * InitializeValues function                                             *
         *                                                                      *
        \************************************************************************/
        
        public override void InitializeValues()
        {
            base.InitializeValues();
#if DEBUG
            Debug.Log("IonModuleCollector.InitializeValues() " + this.part.name + " " + generatorName);
#endif
            //Assign default values
            minAtmosphere = 0;
            hideAtmoContents = false;
        }
        


        /************************************************************************\
         * IonModuleCollector class                                   *
         * OnLoad function override                                             *
         *                                                                      *
        \************************************************************************/
        public override void OnLoad(ConfigNode node)
        {
            //Create lists
            if (null == listOutputs_oxygen)
            {
                Debug.Log("IonModuleCollector.OnLoad(): listOutputs_oxygen is null, creating new");
                listOutputs_oxygen = new List<IonResourceData>();
            }
            if (null == listOutputs_noOxygen)
            {
                Debug.Log("IonModuleCollector.OnLoad(): listOutputs_noOxygen is null, creating new");
                listOutputs_noOxygen = new List<IonResourceData>();
            }

            base.OnLoad(node);
#if DEBUG
            Debug.Log("IonModuleCollector.OnLoad() " + this.part.name + " " + generatorName);
            Debug.Log("IonModuleCollector.OnLoad(): node\n" + node.ToString());
#endif
            
            if (node.HasValue("minAtmosphere"))
                minAtmosphere = Convert.ToSingle(node.GetValue("minAtmosphere"));
            if (node.HasValue("isAutomaticOxygen"))
                isAutomaticOxygen = "True" == node.GetValue("isAutomaticOxygen") || "true" == node.GetValue("isAutomaticOxygen") || "TRUE" == node.GetValue("isAutomaticOxygen");
            if (node.HasValue("isAutomaticNoOxygen"))
                isAutomaticNoOxygen = "True" == node.GetValue("isAutomaticNoOxygen") || "true" == node.GetValue("isAutomaticNoOxygen") || "TRUE" == node.GetValue("isAutomaticNoOxygen");
            if (node.HasValue("hideAtmoContents"))
                hideAtmoContents = "True" == node.GetValue("hideAtmoContents") || "true" == node.GetValue("hideAtmoContents") || "TRUE" == node.GetValue("hideAtmoContents");
             
        }


        /************************************************************************\
         * IonModuleCollector class                                   *
         * Load function                                                        *
         *                                                                      *
         * Used to load generator data from a supportCollector object.          *
        \************************************************************************/
        public override void Load(IonGeneratorData supportGenerator)
        {
            //Create lists
            listOutputs_oxygen = new List<IonResourceData>();
            listOutputs_noOxygen = new List<IonResourceData>();

            base.Load(supportGenerator);
#if DEBUG
            Debug.Log("IonModuleCollector.Load() " + this.part.name + " " + generatorName);
#endif
            IonCollectorData supportCollector = supportGenerator as IonCollectorData;

            //Read variables from supportGenerator
            minAtmosphere = supportCollector.minAtmosphere;
            isAutomaticOxygen = supportCollector.isAutomaticOxygen;
            isAutomaticNoOxygen = supportCollector.isAutomaticNoOxygen;
            hideAtmoContents = supportCollector.hideAtmoContents;

            //Process data in the supportGenerator lists
            foreach (IonGeneratorResourceData outputResource in supportCollector.listOutputs_oxygen)
            {
#if DEBUG
                Debug.Log("IonModuleGenerator.Load(): loading resource " + outputResource.Name + " from supportCollector.listOutputs_oxygen");
#endif
                IonGeneratorResourceData genResource = new IonGeneratorResourceData(outputResource);
                listOutputs_oxygen.Add(genResource);
            }

            foreach (IonGeneratorResourceData outputResource in supportCollector.listOutputs_noOxygen)
            {
#if DEBUG
                Debug.Log("IonModuleGenerator.Load(): loading resource " + outputResource.Name + " from supportCollector.listOutputs_noOxygen");
#endif
                IonGeneratorResourceData genResource = new IonGeneratorResourceData(outputResource);
                listOutputs_noOxygen.Add(genResource);
            }
        }


        /************************************************************************\
         * IonModuleCollector class                                   *
         * OnLoad function override                                             *
         *                                                                      *
        \************************************************************************/
        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
#if DEBUG
            Debug.Log("IonModuleCollector.OnSave() " + this.part.name + " " + generatorName);
#endif
            //Save variables
            node.AddValue("minAtmosphere", minAtmosphere);
            node.AddValue("isAutomaticOxygen", isAutomaticOxygen);
            node.AddValue("isAutomaticNoOxygen", isAutomaticNoOxygen);
            node.AddValue("hideAtmoContents", hideAtmoContents);

            //Save oxygen outputs
            if (null != listOutputs_oxygen)
            {
                foreach (IonGeneratorResourceData resource in listOutputs_oxygen)
                {
#if DEBUG
                    Debug.Log("IonModuleCollector.OnSave(): adding resouce " + resource.Name + " from listOutputs_oxygen");
#endif
                    ConfigNode resourceNode = new ConfigNode("OUTPUT_RESOURCE_OXYGEN");
                    resource.Save(resourceNode);
                    node.AddNode(resourceNode);
                }
            }

            //Save no oxygen outputs
            if (null != listOutputs_noOxygen)
            {
                foreach (IonGeneratorResourceData resource in listOutputs_noOxygen)
                {
#if DEBUG
                    Debug.Log("IonModuleCollector.OnSave(): adding resouce " + resource.Name + " from listOutputs_noOxygen");
#endif
                    ConfigNode resourceNode = new ConfigNode("OUTPUT_RESOURCE_NO_OXYGEN");
                    resource.Save(resourceNode);
                    node.AddNode(resourceNode);
                }
            }
#if DEBUG
            Debug.Log("IonModuleCollector.OnSave(): node\n" + node.ToString());
#endif
        }


        /************************************************************************\
         * IonModuleCollector class                                   *
         * OnStart function override                                            *
         *                                                                      *
        \************************************************************************/
        public override void OnStart(StartState state)
        {
            //Reprocess and clear listResourceNodes, if necessary
            if (null == listOutputs_oxygen || null == listOutputs_noOxygen)
            {
                listOutputs_oxygen = new List<IonResourceData>();
                listOutputs_noOxygen = new List<IonResourceData>();
            }

            base.OnStart(state);
#if DEBUG
            Debug.Log("IonModuleCollector.OnStart() " + this.part.name + " " + generatorName);
            Debug.Log("IonModuleCollector.OnStart(): state " + state.ToString());
#endif
            //Hide unwanted feilds and buttons
            if (hideAtmoContents || !IonLifeSupportScenario.Instance.IsLifeSupportEnabled)
                Fields["atmosphereContents"].guiActive = false;
			else
				Fields["atmosphereContents"].guiActive = true;

			if ((isAutomaticOxygen || isAutomaticNoOxygen) || !IonLifeSupportScenario.Instance.IsLifeSupportEnabled)
            {
                Events["ActivateButton"].guiActive = false;
                Events["ShutdownButton"].guiActive = false;

                Actions["ActivateAction"].active = false;
                Actions["ShutdownAction"].active = false;
                Actions["ToggleAction"].active = false;
            }
        }


        /************************************************************************\
         * IonModuleCollector class                                   *
         * FixedUpdate function override                                           *
         *                                                                      *
        \************************************************************************/
        public override void FixedUpdate()
        {
			if(IonLifeSupportScenario.Instance.IsLifeSupportEnabled)
			{
	            base.FixedUpdate();
#if DEBUG_UPDATES
    	        Debug.Log("IonModuleCollector.FixedUpdate() " + this.part.name + " " + generatorName);
#endif
			}
        }


        /************************************************************************\
         * IonModuleCollector class                                   *
         * FixedUpdate function override                                           *
         *                                                                      *
        \************************************************************************/
        public override void UpdateSetup()
        {
#if DEBUG_UPDATES
            Debug.Log("IonModuleCollector.UpdateSetup() " + this.part.name + " " + generatorName);
            Debug.Log("IonModuleCollector.UpdateSetup(): atmosphere density " + vessel.atmDensity + " | min density " + minAtmosphere);
#endif
            //Determing which output list to load and how efficent the intake will be
            if ((float)vessel.atmDensity < minAtmosphere)
            {
#if DEBUG_UPDATES
                Debug.Log("IonModuleCollector.UpdateSetup(): atmosphere too thin");
#endif
                atmosphereContents = "Insufficient Atmosphere";
                if (isAutomaticOxygen || isAutomaticNoOxygen)
                    SetGeneratorState(false);
            }

            else if (vessel.mainBody.atmosphereContainsOxygen)
            {
#if DEBUG_UPDATES
                Debug.Log("IonModuleCollector.UpdateSetup(): atmosphere has oxygen");
#endif
                atmosphereContents = "Oxygen";
                listOutputs = listOutputs_oxygen;
                if (isAutomaticOxygen)
                    SetGeneratorState(true);
            }

            else
            {
#if DEBUG_UPDATES
                Debug.Log("IonModuleCollector.UpdateSetup(): atmosphere has no oxygen");
#endif
                atmosphereContents = "Carbon Dioxide";
                listOutputs = listOutputs_noOxygen;
                if (isAutomaticNoOxygen)
                    SetGeneratorState(true);
            }


#if DEBUG_UPDATES
            Debug.Log("IonModuleCollector.UpdateSetup(): checking listOutputs");
            foreach (IonGeneratorResourceData genResource in listOutputs)
            {
                Debug.Log("IonModuleCollector.UpdateSetup(): resource " + genResource.Name);
            }
            Debug.Log("IonModuleCollector.UpdateSetup(): checking listOutputs_oxygen");
            foreach (IonGeneratorResourceData genResource in listOutputs_oxygen)
            {
                Debug.Log("IonModuleCollector.UpdateSetup(): resource " + genResource.Name);
            }
            Debug.Log("IonModuleCollector.UpdateSetup(): checking listOutputs_noOxygen");
            foreach (IonGeneratorResourceData genResource in listOutputs_noOxygen)
            {
                Debug.Log("IonModuleCollector.UpdateSetup(): resource " + genResource.Name);
            }
#endif

            base.UpdateSetup();
        }


        /************************************************************************\
         * IonModuleCollector class                                   *
         * CalculateModifiers function                                          *
         *                                                                      *
         * Calculates input and output modifiers based on atmosphere density    *
         * and then calls IonModuleGenerator version of CalculateModifiers.     *
        \************************************************************************/
        public override void CalculateModifiers(double deltaTime)
        {
#if DEBUG_UPDATES
            Debug.Log("IonModuleCollector.CalculateModifiers() " + this.part.name + " " + generatorName);
            Debug.Log("IonModuleCollector.CalculateModifiers(): inputModifier " + inputModifier + " | outputModifier " + outputModifier);
#endif
            outputModifier = vessel.atmDensity;
#if DEBUG_UPDATES
            Debug.Log("IonModuleCollector.CalculateModifiers(): inputModifier " + inputModifier + " | outputModifier " + outputModifier);
#endif
            base.CalculateModifiers(deltaTime);
        }


        /************************************************************************\
         * IonModuleCollector class                                   *
         * CalculateModifiersQuick function                                     *
         *                                                                      *
         * Runs a quick calculatation of  input and output modifiers based on   *
         * atmosphere density and then calls IonModuleGenerator version of      *
         * CalculateModifiers.                                                  *
        \************************************************************************/
        public override void CalculateModifiersQuick(double deltaTime)
        {
#if DEBUG_UPDATES
            Debug.Log("IonModuleCollector.CalculateModifiersQuick() " + this.part.name + " " + generatorName);
            Debug.Log("IonModuleCollector.CalculateModifiersQuick(): inputModifier " + inputModifier + " | outputModifier " + outputModifier);
#endif
            outputModifier = vessel.atmDensity;
#if DEBUG_UPDATES
            Debug.Log("IonModuleCollector.CalculateModifiersQuick(): inputModifier " + inputModifier + " | outputModifier " + outputModifier);
#endif
            base.CalculateModifiers(deltaTime);
        }


        /************************************************************************\
         * IonModuleCollector class                                   *
         * GetResourceUsage function                                            *
         *                                                                      *
         * Calculates the modules resource usage over deltaTime and returns a   *
         * list of all resources used and their amounts.                        *
        \************************************************************************/
        public override List<IonResourceData> GetResources()
        {
#if DEBUG
            Debug.Log("IonModuleCollector.GetResources() " + this.part.name + " " + generatorName);
#endif
            List<IonResourceData> listResources = base.GetResources();

            foreach (IonGeneratorResourceData genResource in listOutputs_oxygen)
            {
                //IonResourceData resourceData = new IonResourceData(genResource);
                //listResources.Add(resourceData);
                listResources.Add(genResource);
#if DEBUG
                Debug.Log("IonModuleCollector.GetResources(): adding " + genResource.Name);
#endif
            }

            foreach (IonGeneratorResourceData genResource in listOutputs_noOxygen)
            {
                //IonResourceData resourceData = new IonResourceData(genResource);
                //listResources.Add(resourceData);
                listResources.Add(genResource);
#if DEBUG
                Debug.Log("IonModuleCollector.GetResources(): adding " + genResource.Name);
#endif
            }
            return listResources;
        }


        /************************************************************************\
         * IonModuleCollector class                                   *
         * isAble function                                                      *
         *                                                                      *
         * Tests whether the generator is able to operate under current         *
         * conditions.                                                          *
        \************************************************************************/
        public override bool isAble()
        {
            return (float)vessel.atmDensity > minAtmosphere;
        }


        /************************************************************************\
         * IonModuleCollector class                                   *
         * isListSubNode function                                               *
         *                                                                      *
         * Tests whether nodeName corresponds to an input/output list.          *
        \************************************************************************/
        public override bool isListSubNode(string nodeName)
        {
            return "OUTPUT_RESOURCE_OXYGEN" == nodeName || "OUTPUT_RESOURCE_NO_OXYGEN" == nodeName || base.isListSubNode(nodeName);
        }


        /************************************************************************\
         * IonModuleCollector class                                   *
         * GetList function                                                     *
         *                                                                      *
         * Returns the list that matches the string nodeName.                   *
        \************************************************************************/
        public override List<IonResourceData> GetCorrespondingList(string nodeName)
        {
            if ("OUTPUT_RESOURCE_OXYGEN" == nodeName)
                return listOutputs_oxygen;
            if ("OUTPUT_RESOURCE_NO_OXYGEN" == nodeName)
                return listOutputs_noOxygen;

            return base.GetCorrespondingList(nodeName);
        }


        /************************************************************************\
         * IonModuleCollector class                                   *
         * FindGeneratorConfigNode function                                     *
         *                                                                      *
         * Returns a IonSupportGenerator object containing data loaded from the *
         * ConfigNode of ION_POD_COLLECTOR with a generatorName value matching  *
         * this generator.                                                      *
        \************************************************************************/
        public override IonGeneratorData FindGeneratorConfigNode()
        {
#if DEBUG
            Debug.Log("IonModuleCollector.FindGeneratorConfigNode() " + this.part.name + " " + generatorName);
#endif
            IonCollectorData supportCollector = null;

            //Pull default values from config nodes
            //Loops through all ION_POD_GENERATOR configNodes in the GameDatabase
            foreach (ConfigNode generatorNode in GameDatabase.Instance.GetConfigNodes("ION_POD_COLLECTOR"))
            {
#if DEBUG
                Debug.Log("IonModuleCollector.FindGeneratorConfigNode(): ION_POD_COLLECTOR node found\n" + generatorNode.ToString());
#endif
                if (generatorNode.HasValue("generatorName") && generatorNode.GetValue("generatorName") == generatorName)
                {
                    supportCollector = new IonCollectorData();
                    supportCollector.Load(generatorNode);
                    break;
                }
            }

            return (IonGeneratorData)supportCollector;
        }
    }
    //==========================================================================================================
    // END of IonModuleCollector Class
    //==========================================================================================================
}