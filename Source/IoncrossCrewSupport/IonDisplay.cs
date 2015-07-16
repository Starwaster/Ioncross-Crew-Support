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
     * IonModuleDisplay Class                               *
     * classes to handles displaying resource useage        *
     * rates for other modules.                             *
    \*------------------------------------------------------*/
    public class IonModuleDisplay : PartModule
    {
        [KSPField(guiActive = false, isPersistant = true)]
        public string resourceName;

        [KSPField(guiActive = false, isPersistant = true)]
        public string resourceGUIName;

        [KSPField(guiActive = true, guiName = "ResourceName Rate", guiUnits = "/h", guiFormat = "F3", isPersistant = false)]
        public float displayRate;

        [KSPField(guiActive = false, isPersistant = true)]
        public bool isRate = true;

        //Private members for storing the rate of usage
        public int ratesSize = 20;
        public int RatesSize { set { ratesSize = value; rates = new double[ratesSize]; } }

        public double curRate;  //the sum of all rates sent to this module in an update frame
        public double[] rates;  //an array of rates for pass update frams, used to calculate an average
        public int curIndex;    //index of the curent element in rates
        public double curSum;   //current sum of all rates in rates



        public IonModuleDisplay()
        {
            ratesSize = 20;
            RatesSize = ratesSize;
            curIndex = 0;
            curSum = 0;
        }

        /************************************************************************\
         * IonModuleDisplay class                                               *
         * OnLoad function override                                             *
         *                                                                      *
        \************************************************************************/
        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
#if DEBUG
            Debug.Log("IonModuleDisplay.OnLoad() " + this.part.name);
            Debug.Log("IonModuleDisplay.OnLoad(): node\n" + node.ToString());
#endif
            
            if (node.HasValue("RatesSize"))
                ratesSize = Convert.ToInt32(node.GetValue("RatesSize"));
        }



        /************************************************************************\
         * IonModuleDisplay class                                               *
         * OnSave function override                                             *
         *                                                                      *
        \************************************************************************/
        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
#if DEBUG
            Debug.Log("IonModuleDisplay.OnSave() " + this.part.name);
#endif
            node.AddValue("RatesSize", ratesSize);
#if DEBUG
            Debug.Log("IonModuleDisplay.OnSave(): node\n" + node.ToString());
#endif
        }

        /************************************************************************\
         * IonModuleDisplay class                                               *
         * OnStart function override                                            *
         *                                                                      *
        \************************************************************************/
        public override void OnStart(PartModule.StartState state)
        {
			Fields["displayRate"].guiActive = IonLifeSupportScenario.Instance.IsLifeSupportEnabled;

			base.OnStart(state);
#if DEBUG
            Debug.Log("IonModuleDisplay.OnStart() " + this.part.name);
            Debug.Log("IonModuleDisplay.OnStart(): state " + state.ToString());
#endif
            RatesSize = ratesSize;
            curIndex = 0;
            curSum = 0;

            if (null == resourceGUIName || 0 == resourceGUIName.Length)
            {
                resourceGUIName = resourceName;
            }

            if (isRate)
                Fields["displayRate"].guiName = resourceGUIName + " Rate";
            else
                Fields["displayRate"].guiName = resourceGUIName;
        }

        
        /************************************************************************\
         * IonModuleDisplay class                                               *
         * FixedUpdate function override                                           *
         *                                                                      *
        \************************************************************************/
        public void FixedUpdate()
        {
			if(IonLifeSupportScenario.Instance.IsLifeSupportEnabled)
			{
	            //base.FixedUpdate();
	#if DEBUG_UPDATES
	            Debug.Log("IonModuleDisplay.OnStart() " + this.part.name);
	#endif
	            if (isRate)
	            {
	                //subtract the oldest value from the sum
	                //add the new value to the sum
	                curSum -= rates[curIndex];
	                if (TimeWarp.fixedDeltaTime != 0)
	                    rates[curIndex] = curRate / TimeWarp.fixedDeltaTime;
	                else
	                    rates[curIndex] = 0;
	                curSum += rates[curIndex];

	                //increment and ensure curIndex stays in bounds
	                curIndex = (curIndex + 1) % ratesSize;

	                displayRate = (float)(curSum / ratesSize);
	                Fields["displayRate"].guiUnits = "/s";

	                //adjust the units so the value is above 0.5
	                string units = "";
	                setRateUnits(ref displayRate, ref units, 0.5f);
	                Fields["displayRate"].guiUnits = "/" + units;
	                
	                //Reset curRate for next update
	                curRate = 0.0f;
				}
            }
			else
			{

			}
        }

        /************************************************************************\
         * IonModuleDisplay class                                               *
         * Set functions                                                        *
         * Uses to add to the curRate field.                                    *
         * curRate stores the sum of all the rates provided this update frame   *
        \************************************************************************/
        public void AddRate(float rate)
        {
#if DEBUG_UPDATES
            Debug.Log("IonModuleDisplay.AddRate(" + rate + ") " + this.part.name);
#endif
            curRate += rate;
        }

        /************************************************************************\
         * IonModuleDisplay class                                               *
         * SetGUIName functions                                                 *
         * Uses to change the guiName for the display.                          *
        \************************************************************************/
        public void SetGUIName(string name)
        {
#if DEBUG
            Debug.Log("IonModuleDisplay.SetGUIName(" + name + ") " + this.part.name);
#endif
            Fields["displayRate"].guiName = name;
        }

        /************************************************************************\
         * IonModuleDisplay class                                               *
         * SetFormat functions                                                  *
         * Uses to change the format for the display.                           *
        \************************************************************************/
        public void SetFormat(string format)
        {
#if DEBUG
            Debug.Log("IonModuleDisplay.SetFormat(" + format + ") " + this.part.name);
#endif
            Fields["displayRate"].guiFormat = format;
        }

        /************************************************************************\
         * IonModuleDisplay class                                               *
         * SetUnits functions                                                   *
         * Uses to change the units for the display.                            *
        \************************************************************************/
        public void SetUnits(string units)
        {
#if DEBUG
            Debug.Log("IonModuleDisplay.SetUnits(" + units + ") " + this.part.name);
#endif
            Fields["displayRate"].guiUnits = units;
        }


        /************************************************************************\
         * IonModuleDisplay class                                               *
         * setRateUnits functions                                               *
         *                                                                      *
         * Adjusts the units of rate so the value will be greater than minValue.*
        \************************************************************************/
        public static void setRateUnits(ref float rate, ref string units, float minValue)
        {
            units = "sec.";

            //adjust rate and units so the value stays above minValue
            if (Math.Abs(rate) < minValue)
            {
                rate *= 60.0f;
                units = "min.";

                if (Math.Abs(rate) < minValue)
                {
                    rate *= 60.0f;
                    units = "hour";

					if (Math.Abs(rate) < minValue)
					{
						rate *= (float)(KSPUtil.Day/3600);
						units = "day";
					}
                }
            }
        }
        

        /************************************************************************\
         * IonModuleDisplay class                                               *
         * findDisplayModule functions                                          *
         * Finds an IonModuleDisplay for resourceName on part and returns it.   *
         * If one does not exist it return null.                               *
        \************************************************************************/
        public static IonModuleDisplay findDisplayModule(Part part, IonResourceData resource)
        {
#if DEBUG
            Debug.Log("IonModuleDisplay.findDisplayModule() " + part.name + ": " + resource);
#endif
            IonModuleDisplay displayModule = null;

            if (null != part.Modules)
            {
                foreach (PartModule module in part.Modules)
                {
                    if (module is IonModuleDisplay && ((IonModuleDisplay)module).resourceName == resource.Name)
                    {
#if DEBUG
                        Debug.Log("IonModuleDisplay.findDisplayModule(): " + resource.Name + " module found");
#endif
                        displayModule = (IonModuleDisplay)module;
                        break;
                    }
                }
            }

            return displayModule;
        }

        /************************************************************************\
         * IonModuleDisplay class                                               *
         * findAndCreateGeneratorModule functions                               *
         * Finds an IonModuleDisplay for resourceName on part and returns it.   *
         * If one does not exist it creates one and returns it.                *
        \************************************************************************/
        public static IonModuleDisplay findAndCreateDisplayModule(Part part, IonResourceData resource)
        {
#if DEBUG
			Debug.Log("IonModuleDisplay.findDisplayModule() " + part.name + ": " + resource);
#endif
            IonModuleDisplay displayModule = findDisplayModule(part, resource);

            if (null == displayModule)
            {
#if DEBUG
                Debug.Log("IonModuleDisplay.findAndCreateDisplayModule(): " + resource.Name + " module not found, creating new");
#endif
                try { displayModule = (IonModuleDisplay)part.AddModule("IonModuleDisplay"); }
                catch (NullReferenceException)
                {
#if DEBUG
                    Debug.Log("IonModuleDisplay.findAndCreateDisplayModule(): NULL REFERENCE EXCEPTION CAUGHT! part.Modules was probablly null");
#endif
                    return null;
                }
                displayModule.resourceName = resource.Name;
                displayModule.resourceGUIName = resource.Name;
#if DEBUG
                Debug.Log("IonModuleDisplay.findAndCreateDisplayModule(): " + displayModule.resourceName + " module added");
#endif
            }

            return displayModule;
        }
    }
    //==========================================================================================================
    // END of IonModuleDisplay Class
    //==========================================================================================================

}