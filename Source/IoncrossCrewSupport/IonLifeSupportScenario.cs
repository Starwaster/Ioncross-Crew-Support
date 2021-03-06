// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------
using KSP;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IoncrossKerbal
{
	[KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[]{GameScenes.FLIGHT,GameScenes.SPACECENTER})]
	public class IonLifeSupportScenario : ScenarioModule
	{
		public IonLifeSupportScenario ()
		{
		}

		public static IonLifeSupportScenario Instance;

		private bool _isLifeSupportEnabled = true;

		public bool isLifeSupportEnabled
		{
			get
			{
				return this._isLifeSupportEnabled;
			}
			set
			{
				this._isLifeSupportEnabled = value;
			}
		}

		private bool _isThermalEnabled = false;

		public bool isThermalEnabled
		{
			get
			{
				return this._isThermalEnabled;
			}
			set
			{
				this._isThermalEnabled = value;
			}
		}

		public override void OnAwake ()
		{
			IonLifeSupportScenario.Instance = this;
		}

		public override void OnSave(ConfigNode node)
		{
			node.AddValue("isLifeSupportEnabled", isLifeSupportEnabled);
			node.AddValue("isThermalEnabled", isThermalEnabled);
		}

		public override void OnLoad(ConfigNode node)
		{
			if (node.HasValue("isLifeSupportEnabled"))
				isLifeSupportEnabled = bool.Parse (node.GetValue ("isLifeSupportEnabled"));
			if (node.HasValue("isThermalEnabled"))
				isThermalEnabled = bool.Parse(node.GetValue("isThermalEnabled"));
		}
	}
}

