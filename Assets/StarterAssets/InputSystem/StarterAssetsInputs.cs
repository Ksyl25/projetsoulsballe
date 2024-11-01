using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool isAiming;
        public bool ishasguns = false;

        [Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
            //Debug.Log(jump);
            JumpInput(value.isPressed);
			/*Debug.Log("lavariable jump:");
			Debug.Log(jump);
            Debug.Log(ishasguns);*/
        }

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
           // Debug.Log("saut ok");
        }
        public void OnAiming(InputValue value)
        {
			//clique sur tirer 
            //isAiming=value.isPressed;
            isAimingf(value.isPressed);
            //Debug.Log("vise ok");
        }
        public void OnHasguns1(InputValue value)
        {
            //isAiming=value.isPressed;
            //Debug.Log(ishasguns);
            ishasgunsf(value.isPressed);
            /*Debug.Log("la variable hasguns");
            Debug.Log(ishasguns);*/
        }
#endif


        public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

        public void isAimingf(bool newAimingState)
        {
            isAiming = newAimingState;
        }

        public void ishasgunsf(bool newhasgunsState)
        {
            ishasguns = newhasgunsState;
        }

        private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}