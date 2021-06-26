//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Makes the hand act as an input module for Unity's event system
//
//=============================================================================

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CustomVRInputModule : BaseInputModule
	{
		private GameObject submitObject;

		//-------------------------------------------------
		private static CustomVRInputModule _instance;
		public static CustomVRInputModule instance
		{
			get
			{
				if ( _instance == null )
					_instance = GameObject.FindObjectOfType<CustomVRInputModule>();

				return _instance;
			}
		}


		//-------------------------------------------------
		public override bool ShouldActivateModule()
		{
			if ( !base.ShouldActivateModule() )
				return false;

			return submitObject != null;
		}


		//-------------------------------------------------
		public void HoverBegin( GameObject gameObject )
		{
			// Debug.Log($"HoverBegin {gameObject} {LayerMask.LayerToName(gameObject.layer)}");

			PointerEventData pointerEventData = new PointerEventData( eventSystem );
			ExecuteEvents.Execute( gameObject, pointerEventData, ExecuteEvents.pointerEnterHandler );
		}


		//-------------------------------------------------
		public void HoverEnd( GameObject gameObject )
		{
			// Debug.Log($"HoverEnd {gameObject} {LayerMask.LayerToName(gameObject.layer)}");

			PointerEventData pointerEventData = new PointerEventData( eventSystem );
			pointerEventData.selectedObject = null;
			ExecuteEvents.Execute( gameObject, pointerEventData, ExecuteEvents.pointerExitHandler );
		}

		public void Click( GameObject gameObject )
		{
			// Debug.Log($"Click {gameObject} {LayerMask.LayerToName(gameObject.layer)}");

			PointerEventData pointerEventData = new PointerEventData( eventSystem );
			ExecuteEvents.Execute( gameObject, pointerEventData, ExecuteEvents.pointerClickHandler );
		}


		//-------------------------------------------------
		public void Submit( GameObject gameObject )
		{
			// Debug.Log($"Submit {gameObject} {LayerMask.LayerToName(gameObject.layer)}");

			submitObject = gameObject;
		}


		//-------------------------------------------------
		public override void Process()
		{
			// Debug.Log("Process");
			
			if ( submitObject )
			{
				BaseEventData data = GetBaseEventData();
				data.selectedObject = submitObject;
				ExecuteEvents.Execute( submitObject, data, ExecuteEvents.submitHandler );

				submitObject = null;
			}
		}
	}
