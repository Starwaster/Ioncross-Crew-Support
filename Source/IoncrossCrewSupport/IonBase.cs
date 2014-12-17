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

    public abstract class IonModuleBase : PartModule
    {
        public double lastLoaded = -1f;
        public IonModuleBase masterBase = null;

        public bool initialized = false;
        public bool firstUpdateRun = false;


        /************************************************************************\
         * IonModuleBase class                                                  *
         * OnAwake function override                                            *
         *                                                                      *
        \************************************************************************/
        public override void OnAwake()
        {
            base.OnAwake();
#if DEBUG
            Debug.Log("IonModuleBase.OnAwake() " + this.part.name);
#endif
            if(!initialized)
                InitializeValues();
        }

        /************************************************************************\
         * IonModuleBase class                                                  *
         * InitializeValues function                                             *
         *                                                                      *
        \************************************************************************/
        public virtual void InitializeValues()
        {
#if DEBUG
            Debug.Log("IonModuleBase.InitializeValues() " + this.part.name);
#endif
            initialized = true;
            lastLoaded = -1;
        }

        /************************************************************************\
         * IonModuleBase class                                                  *
         * OnLoad function override                                             *
         *                                                                      *
        \************************************************************************/
        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
#if DEBUG
            Debug.Log("IonModuleBase.OnLoad() " + this.part.name);
            Debug.Log("IonModuleBase.OnLoad(): node\n" + node.ToString());
#endif
            if (node.HasValue("lastLoaded"))
                lastLoaded = Convert.ToDouble(node.GetValue("lastLoaded"));
        }


        /************************************************************************\
         * IonModuleBase class                                                  *
         * OnSave function override                                             *
         *                                                                      *
        \************************************************************************/
        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
#if DEBUG
            Debug.Log("IonModuleBase.OnSave() " + this.part.name);
#endif
            if (HighLogic.LoadedSceneIsFlight)
                node.AddValue("lastLoaded", lastLoaded);
            else
                node.AddValue("lastLoaded", -1);

#if DEBUG
            Debug.Log("IonModuleBase.OnSave(): node\n" + node.ToString());
#endif
        }


        /************************************************************************\
         * IonModuleBase class                                                  *
         * OnStart function override                                            *
         *                                                                      *
        \************************************************************************/
        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
#if DEBUG
            Debug.Log("IonModuleBase.OnStart() " + this.part.name);
            Debug.Log("IonModuleBase.OnStart(): state " + state.ToString());
#endif
        }


        /************************************************************************\
         * IonModuleBase class                                                  *
         * OnUpdate function override                                           *
         *                                                                      *
        \************************************************************************/
        public override void OnUpdate()
        {
			if(IonLifeSupportScenario.Instance.IsLifeSupportEnabled)
			{
	            base.OnUpdate();
	#if DEBUG_UPDATES
	            Debug.Log("IonModuleBase.OnUpdate() " + this.part.name);
	#endif
	            if (!firstUpdateRun)
	                FirstUpdateInitialize();


	            double deltaTime = Planetarium.GetUniversalTime() - lastLoaded;
	            
	            //If delatTime is more than 10 update cycles worth, if more than 5 minutes time, and this module is either the master, or there is no master
	            if (deltaTime > 10 * TimeWarp.deltaTime && deltaTime > 300 && (this == masterBase || null == masterBase || this.vessel != masterBase.vessel))
	            {
	#if DEBUG
	                Debug.Log("IonModuleBase.OnUpdate(): cur time " + Planetarium.GetUniversalTime() + " | time last active " + lastLoaded);
	                Debug.Log("IonModuleBase.OnUpdate(): This vessel has been inactive for " + deltaTime + " | TimeWarp.deltaTime " + TimeWarp.deltaTime);
	#endif
	                List<ModuleResource> listResourceUsage = new List<ModuleResource>();

	                masterBase = this;
	                foreach (Part vesselPart in this.part.vessel.Parts)
	                {
	                    foreach (PartModule module in vesselPart.Modules)
	                    {
	                        if (module is IonModuleBase)
	                        {
	                            ((IonModuleBase)module).masterBase = this;
	                            if (module is IonModuleCrewSupport)
	                            {
	                            }
	                            else if (module is IonModuleGenerator)
	                            {
	                            }
	                        }
	                    }
	                }

	                CalculateInactiveResourceUsage(deltaTime);
	            }
			}
			lastLoaded = Planetarium.GetUniversalTime();
		}


        /************************************************************************\
         * IonModuleBase class                                                  *
         * Initialize function                                                   *
         *                                                                      *
        \************************************************************************/
        protected virtual void FirstUpdateInitialize()
        {
#if DEBUG
            Debug.Log("IonModuleBase.FirstUpdateInitialize() " + this.part.name);
#endif
            if (lastLoaded < 0)
            {
                lastLoaded = Planetarium.GetUniversalTime();
            }

            firstUpdateRun = true;
        }


        /************************************************************************\
         * IonModuleBase class                                                  *
         * ConsumeResources function                                            *
         *                                                                      *
         * Runs the module's resource use simulation over deltaTime.            *
        \************************************************************************/
        public abstract bool ConsumeResources(double deltaTime);


        /************************************************************************\
         * IonModuleBase class                                                  *
         * ConsumeResourcesQuick function                                       *
         *                                                                      *
         * Runs a quicker aproximation of the module's resource use simulation  *
         * over deltaTime.                                                      *
        \************************************************************************/
        public abstract void ConsumeResourcesQuick(double deltaTime);


        /************************************************************************\
         * IonModuleBase class                                                  *
         * GetResourceUsage function                                            *
         *                                                                      *
         * Calculates the module's resource usage over deltaTime and returns a  *
         * list of all resources used and their amounts.                        *
        \************************************************************************/
        public abstract List<IonResourceData> GetResources();


        /************************************************************************\
         * IonModuleBase class                                                  *
         * GetCorrespondingList function                                        *
         *                                                                      *
         * Returns the resource list that corresponds to nodeName.              *
         * Returns null if there is no corresponding list.                      *
        \************************************************************************/
        public abstract List<IonResourceData> GetCorrespondingList(string nodeName);


        /************************************************************************\
         * IonModuleBase class                                                  *
         * ProcessNodestoList function                                          *
         *                                                                      *
         * Processes the nodes list and adds data from them to conesponding     *
         * lists.                                                               *
        \************************************************************************/
        public virtual void ProcessNodestoList(List<ConfigNode> nodes)
        {
#if DEBUG
            Debug.Log("IonModuleBase.ProcessNodestoList() " + this.part.name);
#endif
            //Read and process nodes
            foreach (ConfigNode node in nodes)
            {
                ProcessNodetoList(node);
            }
        }


        /************************************************************************\
         * IonModuleBase class                                                  *
         * ProcessNodestoList function                                          *
         *                                                                      *
         * Processes the node by adding the data to the coresponding list.      *
        \************************************************************************/
        public virtual bool ProcessNodetoList(ConfigNode node)
        {
#if DEBUG
            Debug.Log("IonModuleBase.ProcessNodetoList() " + this.part.name);
            Debug.Log("IonModuleBase.ProcessNodetoList(): processing node " + node.name);
#endif
            bool success = false;

            //Check if the subNode corresponds to a list
            List<IonResourceData> curList = GetCorrespondingList(node.name);
            if (null != curList)
            {
                //Add node to proper input/output list
                IonResourceData resource = curList.Find(delegate(IonResourceData data) { return data.Name == node.GetValue("name"); });
                if (null != resource)
                {
#if DEBUG
                    Debug.Log("IonModuleBase.ProcessNodetoList(): " + node.name + " " + node.GetValue("name") + " found, loading data into existing entry");
#endif
                    resource.Load(node);
                    success = true;
                }
                else
                {
#if DEBUG
                    Debug.Log("IonModuleBase.ProcessNodetoList(): " + node.name + " " + node.GetValue("name") + " not found, creating new entry");
#endif
                    resource = CreateResourceEntry(node);
                    curList.Add(resource);
                    success = true;
                }
            }

            return success;
        }

        public virtual IonResourceData CreateResourceEntry(ConfigNode node)
        {
            return new IonResourceData(node);
        }


        /************************************************************************\
         * IonModuleBase class                                                  *
         * CalculateInactiveResourceUsage function                              *
         *                                                                      *
        \************************************************************************/
        public void CalculateInactiveResourceUsage(double deltaTime)
        {
#if DEBUG
            Debug.Log("IonModuleBase.CalculateInactiveResourceUsage() " + this.part.name);
#endif
            List<IonModuleCrewSupport> listCrewSupportModules = new List<IonModuleCrewSupport>();
            List<IonModuleEVASupport> listEVASupportModules = new List<IonModuleEVASupport>();
            List<IonModuleGenerator> listGeneratorModules = new List<IonModuleGenerator>();

            double timeStep;
            int numSteps;

            /********************************************************************\
             * Traverse through all modules on all parts to find all the Ion    *
             * modules.  Add these modules to the modules lists.                *
            \********************************************************************/
            foreach (Part vesselPart in this.part.vessel.Parts)
            {
                foreach (PartModule module in vesselPart.Modules)
                {
                    if (module is IonModuleBase)
                    {
                        ((IonModuleBase)module).masterBase = this;
                        if (module is IonModuleCrewSupport)
                        {
                            listCrewSupportModules.Add(module as IonModuleCrewSupport);
                        }
                        else if (module is IonModuleEVASupport)
                        {
                            listEVASupportModules.Add(module as IonModuleEVASupport);
                        }
                        else if (module is IonModuleGenerator)
                        {
                            listGeneratorModules.Add(module as IonModuleGenerator);
                        }
                    }
                }
            }

            /********************************************************************\
             * Split the delta time into a number of small steps                *
            \********************************************************************/
            numSteps = 10000;
            timeStep = deltaTime / numSteps;
            
            while(timeStep < 60 && numSteps > 1)
            {
                numSteps -= 60;
                if (numSteps < 0)
                    numSteps = 1;
                timeStep = deltaTime / numSteps;
            }
#if DEBUG
            Debug.Log("IonModuleBase.CalculateInactiveResourceUsage(): numSteps " + numSteps + " | timeStep " + timeStep);
#endif

            /********************************************************************\
             * Run the simulation in small steps.                               *
            \********************************************************************/
            for(int i = 0; i < numSteps; i++)
            {
#if DEBUG_UPDATES
                Debug.Log("IonModuleBase.CalculateInactiveResourceUsage(): step " + i + " of " + numSteps);
#endif
                foreach (IonModuleCrewSupport crewModule in listCrewSupportModules)
                {
#if DEBUG_UPDATES
                    Debug.Log("IonModuleBase.CalculateInactiveResourceUsage(): crewModule " + crewModule.part.name);
#endif
                    if (crewModule.part.protoModuleCrew.Count > 0)
                        crewModule.ConsumeResourcesQuick(timeStep);
                }
                foreach (IonModuleEVASupport evaModule in listEVASupportModules)
                {
#if DEBUG_UPDATES
                    Debug.Log("IonModuleBase.CalculateInactiveResourceUsage(): evaModule " + evaModule.part.name);
#endif
                    evaModule.ConsumeResourcesQuick(timeStep);
                }
                foreach (IonModuleGenerator generatorModule in listGeneratorModules)
                {
#if DEBUG_UPDATES
                    Debug.Log("IonModuleBase.CalculateInactiveResourceUsage(): generatorModule " + generatorModule.part.name);
#endif
                    if (generatorModule.isAble() && generatorModule.isActive)
                    {
                        generatorModule.UpdateSetup();
                        generatorModule.CalculateModifiersQuick(timeStep);
                        generatorModule.ConsumeResourcesQuick(timeStep);
                    }
                }
            }
        }
        

        /************************************************************************\
         * IonModuleBase class                                                  *
         * RequestResource function                                             *
         *                                                                      *
        \************************************************************************/
        public virtual double RequestResource(string resourceName, double resourceAmount)
        {
            return RequestResource(resourceName.GetHashCode(), resourceAmount);
        }

        /************************************************************************\
         * IonModuleBase class                                                  *
         * RequestResource function                                             *
         *                                                                      *
        \************************************************************************/
        public virtual double RequestResource(int resourceID, double resourceAmount)
        {
#if DEBUG_UPDATES
            Debug.Log("IonModuleBase.RequestResource() " + this.part.name);
            Debug.Log("IonModuleBase.RequestResource(): request for " + resourceAmount + " units of " + resourceID);
#endif
            double amount = 0;
            double deltaAmount = 0;

            List<PartResource> connectedResources = new List<PartResource>();
			this.part.GetConnectedResources(resourceID, PartResourceLibrary.GetDefaultFlowMode(resourceID), connectedResources);

            foreach (PartResource pResource in connectedResources)
            {
                if (Math.Abs(amount) >= Math.Abs(resourceAmount))
                    break;

                deltaAmount = 0;

                if (resourceAmount > 0)
                {
                    deltaAmount = Math.Min(resourceAmount - amount, pResource.amount);
#if DEBUG_UPDATES
                    Debug.Log("IonModuleBase.RequestResource(): resourceAmount " + resourceAmount + " | amount " + amount + " | pResource.amount " + pResource.amount);
                    Debug.Log("IonModuleBase.RequestResource(): deltaAmount " + deltaAmount);
#endif
                    pResource.amount -= deltaAmount;
                }
                else
                {
                    deltaAmount = Math.Max(resourceAmount - amount, pResource.amount - pResource.maxAmount);
#if DEBUG_UPDATES
                    Debug.Log("IonModuleBase.RequestResource(): resourceAmount " + resourceAmount + " | amount " + amount + " | pResource.maxAmount - pResource.amount " + (pResource.maxAmount - pResource.amount));
                    Debug.Log("IonModuleBase.RequestResource(): deltaAmount " + deltaAmount);
#endif
                    pResource.amount -= deltaAmount;
                }

                amount += deltaAmount;
#if DEBUG_UPDATES
                Debug.Log("IonModuleBase.RequestResource(): amount " + amount);
#endif
            }

#if DEBUG_UPDATES
            Debug.Log("IonModuleBase.RequestResource(): returning " + amount);
#endif
            return amount;
        }
    }
}