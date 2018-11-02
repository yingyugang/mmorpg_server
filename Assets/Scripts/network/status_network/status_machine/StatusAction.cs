using UnityEngine;
using System.Collections;

namespace MMO
{
	[System.Serializable]
	public class StatusAction
	{
		public GameObject GO;
		public FinalStatusMachine statusMachine;
		bool mIsEnable = true;
		bool mIsExcute;

		public virtual void OnAwake ()
		{
			
		}

		public virtual void OnEnter ()
		{
			mIsExcute = true;
		}

		public virtual void OnUpdate ()
		{

		}

		public virtual void OnExit ()
		{
			mIsExcute = false;
		}

		public bool IsEnable{
			get{ 
				return mIsEnable;
			}
			set { 
				mIsEnable = value;
			}
		}

		public bool IsExcute{
			set{
				mIsExcute = value;
			}
			get{
				return mIsExcute;
			}
		}

	}
}
