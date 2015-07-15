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
     * IoncrossSettings Class                               *
     * Holds all the settings for the mod                   *
    \*======================================================*/
    public class IoncrossSettings
    {
        private List<IonSupportResourceDataGlobal> listSupportResources;
        public List<IonSupportResourceDataGlobal> ListSupportResources { get { return listSupportResources; } }

        private List<IonGeneratorData> listPodGenerators;
        public List<IonGeneratorData> ListPodGenerators { get { return listPodGenerators; } }

        private List<IonCollectorData> listPodCollectors;
        public List<IonCollectorData> ListPodCollectors { get { return listPodCollectors; } }

        private double lockResources_commandPodRate;
        public double LockResources_CommandPodRate { get { return lockResources_commandPodRate; } }

        private int killResources_minFramesWarning;
        public double KillResources_MinFramesWarning { get { return killResources_minFramesWarning; } }
        private int killResources_minFramesKill;
        public double KillResources_MinFramesKill { get { return killResources_minFramesKill; } }

        private bool inactiveCalc_enabled;
        public bool InactiveCalc_Enabled { get { return inactiveCalc_enabled; } }

        private int inactiveCalc_maxSteps;
        public int InactiveCalc_MaxSteps { get { return inactiveCalc_maxSteps; } }
		private float minimumBreathableAtmoDensity;
		public float MinimumBreathableAtmoDensity { get { return minimumBreathableAtmoDensity; } }


        /************************************************************************\
         * IoncrossSettings class                                               *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public IoncrossSettings()
        {
#if DEBUG
            Debug.Log("IoncrossSettings.Constructor()");
#endif
            listSupportResources = new List<IonSupportResourceDataGlobal>();
            listPodGenerators = new List<IonGeneratorData>();
            listPodCollectors = new List<IonCollectorData>();

            ReadConfigurationFile();

            //Loops through all ION_SUPPORT_RESOURCE configNodes in the GameDatabase
            foreach (ConfigNode supportResourceNode in GameDatabase.Instance.GetConfigNodes("ION_SUPPORT_RESOURCE"))
            {
#if DEBUG
                Debug.Log("IoncrossSettings.Constructor(): found ION_SUPPORT_RESOURCE node\n" + supportResourceNode.ToString());
#endif
                IonSupportResourceDataGlobal supportResource = new IonSupportResourceDataGlobal();
                supportResource.Load(supportResourceNode);
                listSupportResources.Add(supportResource);
            }


            //Loops through all ION_POD_GENERATOR configNodes in the GameDatabase
            foreach (ConfigNode generatorNode in GameDatabase.Instance.GetConfigNodes("ION_POD_GENERATOR"))
            {
#if DEBUG
                Debug.Log("IoncrossSettings.Constructor(): found ION_POD_GENERATOR node\n" + generatorNode.ToString());
#endif
                IonGeneratorData generatorData = new IonGeneratorData(generatorNode);
                listPodGenerators.Add(generatorData);
            }


            //Loops through all ION_POD_COLLECTOR configNodes in the GameDatabase
            foreach (ConfigNode collectorNode in GameDatabase.Instance.GetConfigNodes("ION_POD_COLLECTOR"))
            {
#if DEBUG
                Debug.Log("IoncrossSettings.Constructor(): found ION_POD_COLLECTOR node\n" + collectorNode.ToString());
#endif
                IonCollectorData supportCollector = new IonCollectorData(collectorNode);
                listPodCollectors.Add(supportCollector);
            }
        }


        /************************************************************************\
         * IoncrossSettings class                                               *
         * ReadConfigurationFile function                                       *
         *                                                                      *
         * Reads in settings from the mod's configuration file.                 *
        \************************************************************************/
        private void ReadConfigurationFile()
        {
#if DEBUG
            Debug.Log("IoncrossSettings.ReadConfigurationFile()");
#endif
            KSP.IO.PluginConfiguration configFile = KSP.IO.PluginConfiguration.CreateForType<IonModuleCrewSupport>();
            configFile.load();

            lockResources_commandPodRate = configFile.GetValue<double>("lockResources_commandPodRate");
            killResources_minFramesWarning = configFile.GetValue<int>("killResources_minFramesWarning");
            killResources_minFramesKill = configFile.GetValue<int>("killResources_minFramesKill");

            inactiveCalc_enabled = configFile.GetValue<bool>("inactiveCalc_enabled");
            inactiveCalc_maxSteps = configFile.GetValue<int>("inactiveCalc_maxSteps");
			minimumBreathableAtmoDensity = configFile.GetValue<float>("minimumBreathableAtmoDensity");
        }


        /************************************************************************\
         * IoncrossSettings class                                               *
         * GetSupportResource function                                          *
         *                                                                      *
         * Finds and resturns the IonSupportResourceDataGlobal entry matching   *
         * name.                                                                *
        \************************************************************************/
        public IonSupportResourceDataGlobal GetSupportResource(string name)
        {
#if DEBUG
            Debug.Log("IoncrossSettings.GetSupportResource()");
            Debug.Log("IoncrossSettings.GetSupportResource(): name " + name);
#endif
            return GetSupportResource(name.GetHashCode());
        }

        /************************************************************************\
         * IoncrossSettings class                                               *
         * GetSupportResource function                                          *
         *                                                                      *
         * Finds and resturns the IonSupportResourceDataGlobal entry matching   *
         * id.                                                                *
        \************************************************************************/
        public IonSupportResourceDataGlobal GetSupportResource(int id)
        {
#if DEBUG
            Debug.Log("IoncrossSettings.GetSupportResource()");
            Debug.Log("IoncrossSettings.GetSupportResource(): id " + id);
#endif
            IonSupportResourceDataGlobal rtnResource = null;

            foreach (IonSupportResourceDataGlobal resource in listSupportResources)
            {
                if (resource.ID == id)
                {
                    rtnResource = resource;
                    break;
                }
            }

#if DEBUG
            Debug.Log("IoncrossSettings.GetSupportResource(): return resource " + rtnResource.Name);
#endif
            return rtnResource;
        }


    }
    //==========================================================================================================
    // END of IoncrossSettings Class
    //==========================================================================================================
    

    /*======================================================*\
     * IoncrossController Class                             *
     * Used to trigger the ProcessPartList function.        *
    \*======================================================*/
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class IonTrigger : UnityEngine.MonoBehaviour
    {
        public void Start()
        {
#if DEBUG
            Debug.Log("IonTrigger.Start()");
#endif
            IoncrossController.Instance.ProcessPartList();
        }
    }



    /*======================================================*\
     * IoncrossController Class                             *
     * Global object used to setup and control the mod's    *
     * PartModules.                                         *
    \*======================================================*/
    [KSPAddon(KSPAddon.Startup.EveryScene, true)]
    public class IoncrossController : UnityEngine.MonoBehaviour
    {
        public static IoncrossController Instance { get { return GameDatabase.FindObjectOfType(typeof(IoncrossController)) as IoncrossController; } }

        private IoncrossSettings settings;
        public IoncrossSettings Settings { get { return settings; } }

        private bool initialized;

        //string testData;
        //string prevTestData;


        /************************************************************************\
         * IoncrossController class                                             *
         * Awake function                                                       *
         *                                                                      *
        \************************************************************************/
        public void Awake()
        {
#if DEBUG
            Debug.Log("IoncrossController.Awake()");
#endif
            DontDestroyOnLoad(this);

            settings = new IoncrossSettings();
            initialized = false;

            //testData = GameInfo.GetSceneName(GameInfo.gameScene);
            //prevTestData = GameInfo.GetSceneName(GameInfo.gameScene);
        }

        /************************************************************************\
         * IoncrossController class                                             *
         * Start function                                                       *
         *                                                                      *
        \************************************************************************/
        public void Start()
        {
#if DEBUG
            Debug.Log("IoncrossController.Start()");
#endif
            //ProcessPartList();
        }

        /************************************************************************\
         * IoncrossController class                                             *
         * Update function                                                      *
         *                                                                      *
        \************************************************************************/
        public void FixedUpdate()
        {
#if DEBUG_UPDATES
			Debug.Log("IoncrossController.FixedUpdate()");
#endif

            //testData = GameInfo.GetSceneName(GameInfo.gameScene);

            //if (testData != prevTestData)
            //{
			//    Debug.Log("IoncrossController.FixedUpdate(): New gameScene " + testData);
            //}
            //prevTestData = testData;

            if (!initialized)
            {
                //ProcessPartList();
                initialized = true;
            }

        }

        /************************************************************************\
         * IoncrossController class                                             *
         * OnGUI function                                                       *
         *                                                                      *
        \************************************************************************/
        public void OnGUI()
        {
#if DEBUG_UPDATES
            Debug.Log("IoncrossController.OnGUI()");
#endif
        }


        /************************************************************************\
         * IoncrossController class                                             *
         * ProcessPartList function                                             *
         *                                                                      *
         * This function goes through all parts looking for ones with an        *
         * Ioncross module attached.  These parts passed off to ProcessPart.    *
        \************************************************************************/
        public void ProcessPartList()
        {
#if DEBUG
            Debug.Log("IoncrossController.ProcessPartList()");
#endif
            List<IonResourceData> listPartResources;

            //Traverse through all parts
            foreach (AvailablePart avalablePart in PartLoader.LoadedPartsList)
            {
                Part part = avalablePart.partPrefab;

                //Traverse through the part's modules looking for an Ioncross module
                if (null != part.Modules)
                {
                    for (int i = 0; i < part.Modules.Count; i++)
                    {
                        listPartResources = new List<IonResourceData>();

                        if (part.Modules[i] is IonModuleCrewSupport)
                            ProcessPartCrewSupport(part, (IonModuleCrewSupport)part.Modules[i], ref listPartResources);
                        if (part.Modules[i] is IonModuleGenerator)
                            ProcessPartGenerator(part, (IonModuleGenerator)part.Modules[i], ref listPartResources);

                        AddDisplayModules(part, listPartResources);
                    }
                }
            }

#if DEBUG
            Debug.Log("IoncrossController.ProcessPartList(): processing EVA");
#endif
            AvailablePart evaPart = PartLoader.getPartInfoByName("kerbalEVA");
            if (null != evaPart)
            {
#if DEBUG
                Debug.Log("IoncrossController.ProcessPartList(): found EVA part " + evaPart.name);
#endif
                ProcessPartEVA(evaPart.partPrefab);
            }
        }


        /************************************************************************\
         * IoncrossController class                                             *
         * ProcessPartCrewSupport function                                      *
         *                                                                      *
         * This function takes a part that has an IonModuleCrewSupport module   *
         * attached and adds the nessessary pod generators and adds the         *
         * resources used by the part to listPartResources.                     *
        \************************************************************************/
        private void ProcessPartCrewSupport(Part part, PartModule module, ref List<IonResourceData> listPartResources)
        {
#if DEBUG
            Debug.Log("IoncrossController.ProcessPartCrewSupport(" + part.name + ")");
#endif
            ModuleCommand commandModule = (ModuleCommand)part.Modules["ModuleCommand"];
            ConfigNode partNode;

            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("PART"))
            {
                if (node.GetValue("name") == part.name)
                {
#if DEBUG
                    Debug.Log("IoncrossController.ProcessPartCrewSupport(" + part.name + "): found partModule node\n" + node.ToString());
#endif
                    partNode = node;
                    break;
                }
            }

            //Add pod generators to the part
            foreach (IonGeneratorData generatorData in settings.ListPodGenerators)
            {
#if DEBUG
                Debug.Log("IoncrossController.ProcessPartCrewSupport(" + part.name + "): adding pod generator module " + generatorData.generatorName);
#endif
                IonModuleGenerator podGenerator = IonModuleGenerator.findAndCreateGeneratorModule(part, generatorData, generatorData.moduleClass);
                podGenerator.Load(generatorData);
            }

            //Add pod collectors to the part
            foreach (IonCollectorData collectorData in settings.ListPodCollectors)
            {
#if DEBUG
                Debug.Log("IoncrossController.ProcessPartCrewSupport(" + part.name + "): adding pod collector module " + collectorData.generatorName);
#endif
                IonModuleCollector podCollector = (IonModuleCollector)IonModuleGenerator.findAndCreateGeneratorModule(part, collectorData, collectorData.moduleClass);
                podCollector.Load(collectorData);
            }

            //Add support resources to listPartResources and add commandModule drain
            foreach (IonSupportResourceDataGlobal supportResource in settings.ListSupportResources)
            {
#if DEBUG
                Debug.Log("IoncrossController.ProcessPartCrewSupport(" + part.name + "): proccessing support resource " + supportResource.Name);
#endif
                listPartResources.Add((IonResourceData)supportResource);
                if (supportResource.CauseLock && null != commandModule)
                    AddCommandResourceDrain(commandModule, supportResource);
            }
        }


        /************************************************************************\
         * IoncrossController class                                             *
         * ProcessPartGenerator function                                        *
         *                                                                      *
         * This function takes a part that has an IonModuleGeneratorBase module *
         * attached and adds the resources used by the part to                  *
         * listPartResources.                                                   *
        \************************************************************************/
        private void ProcessPartGenerator(Part part, IonModuleGenerator module, ref List<IonResourceData> listPartResources)
        {
#if DEBUG
            Debug.Log("IoncrossController.ProcessPartGenerator(" + part.name + " " + module.generatorName + ")");
#endif
            //Add generator resources to listPartResources
            foreach (IonResourceData genResource in module.GetResources())
            {
                listPartResources.Add((IonResourceData)genResource);
            }
        }


        /************************************************************************\
         * IoncrossController class                                             *
         * ProcessPartEVA function                                              *
         *                                                                      *
         * This function takes the EVA part and adds the IonModuleEVASupport    *
         * module to it.                                                        *
        \************************************************************************/
        private void ProcessPartEVA(Part part)
        {
#if DEBUG
            Debug.Log("IoncrossController.ProcessPartEVA(" + part.name + ")");
#endif
            IonModuleEVASupport evaModule = part.gameObject.AddComponent<IonModuleEVASupport>();
            evaModule.moduleName = "IonModuleEVASupport";
            evaModule.evainitialized = false;

            //create display modules
            IonModuleDisplay displayModule;
            foreach (IonSupportResourceDataGlobal supportResource in settings.ListSupportResources)
            {
                if (supportResource.EVAamount >= 0)
                {
                    displayModule = part.gameObject.AddComponent<IonModuleDisplay>();
                    displayModule.moduleName = "IonModuleDisplay";
                    displayModule.resourceName = supportResource.Name;
                    displayModule.isRate = false;
                    displayModule.ratesSize = 1;
                    displayModule.resourceGUIName = supportResource.Name;
                }
            }
        }


        /************************************************************************\
         * IoncrossController class                                             *
         * AddCommandResourceDrain function                                     *
         *                                                                      *
         * This function adds a small resource requirement to the ModuleCommand *
         * so that control will lock upon resouce depletion.                    *
        \************************************************************************/
        private void AddCommandResourceDrain(ModuleCommand commandModule, IonSupportResourceDataGlobal supportResource)
        {
#if DEBUG
            Debug.Log("IoncrossController.AddCommandResourceDrain(" + supportResource.Name + ")");
#endif
            bool resourceExists = false;
            foreach (ModuleResource inputResource in commandModule.inputResources)
            {
                if (inputResource.name == supportResource.Name)
                {
                    resourceExists = true;
                    break;
                }
            }

            if (!resourceExists)
            {
                ModuleResource commandResource = new ModuleResource();
                commandResource.name = supportResource.Name;
                commandResource.id = supportResource.Name.GetHashCode();
                commandResource.rate = settings.LockResources_CommandPodRate;

                commandModule.inputResources.Add(commandResource);
            }
        }


        /************************************************************************\
         * IoncrossController class                                             *
         * AddDisplayModules function                                           *
         *                                                                      *
         * This function adds display modules for all the resources in          *
         * listResources to part.                                               *
        \************************************************************************/
        private void AddDisplayModules(Part part, List<IonResourceData> listResources)
        {
#if DEBUG
            Debug.Log("IoncrossController.AddDisplayModules(" + part.name + ")");
#endif
            foreach (IonResourceData resource in listResources)
            {
#if DEBUG
                Debug.Log("IoncrossController.AddDisplayModules(" + part.name + "): adding display module for Support resource " + resource.Name);
#endif
                IonModuleDisplay.findAndCreateDisplayModule(part, resource);
            }
        }
    }
}
