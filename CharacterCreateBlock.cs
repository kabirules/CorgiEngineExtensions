using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine // you might want to use your own namespace here
{
	/// <summary>
	/// Add this class to a character and will be able to create a block in front of him
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Create Block")]
	public class CharacterCreateBlock : CharacterAbility
	{
		/// This method is only used to display a helpbox text
		/// at the beginning of the ability's inspector
		public override string HelpBoxText() { return "This component handles block creation. Just define the prefab for the block with 'Breakable' tag. Optionally, define two particles, one for block creation effect and another for block destruction effect."; }

		[Header("Block creation")]
		/// declare your parameters here
		public GameObject block; // IMPORTANT! -> MUST HAVE "Breakable" TAG!
		public GameObject particlesCreate;
		public GameObject particlesDestroy;
		private Character character;
		private SpriteRenderer spriteR;
		private Vector3 blockSize;

		/// <summary>
		/// Here you should initialize our parameters
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			character = GetComponent<Character>();
			spriteR = block.GetComponent<SpriteRenderer>();
			blockSize = spriteR.bounds.size; // Get block size so we can calculate the position to create it
		}

		/// <summary>
		/// Every frame, we check if we're crouched and if we still should be
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
		}

		/// <summary>
		/// Called at the start of the ability's cycle, this is where you'll check for input
		/// </summary>
		protected override void HandleInput()
		{			
			// here as an example we check if we're pressing down
			// on our main stick/direction pad/keyboard
			/*
			if (_inputManager.PrimaryMovement.y < -_inputManager.Threshold.y) 				
			{
				DoSomething ();
			}
			*/
			// We are going to use the same input for dash
			// TODO -> Use a different input?
			if (_inputManager.DashButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				createBlock();
			}
		}

		/// <summary>
		/// If we're pressing dash, we check for a few conditions to see if we can perform our action
		/// </summary>
		protected virtual void createBlock()
		{
			// We want to be able to create a block when falling or jumping
			if (_movement.CurrentState != CharacterStates.MovementStates.Jumping
				&& _movement.CurrentState != CharacterStates.MovementStates.Falling) {
				// if the ability is not permitted
				if ( !AbilityPermitted
					// or if we're not in our normal stance
					|| (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
					// or if we're grounded TODO -> if grounded, create to block at our feet
					|| (!_controller.State.IsGrounded)
					// or if we're gripping
					|| (_movement.CurrentState == CharacterStates.MovementStates.Gripping) )
				{
					// we do nothing and exit
					return;
				}
			}

			// Step 1. Check the direction the character is looking
			// Step 2. Check if the space to create the block is empty:
			// Step 2.1. YES -> Create the block
			// Step 2.2. NO -> Check if the block is breakable
			// Step 2.2.1. YES -> Remove the block
			// Step 2.2.2. NO -> Do nothing
			bool facingRight = character.IsFacingRight;
			float widthGapFiller = blockSize.x + (blockSize.x * 0.05f); // we leave a minimal gap between blocks
			float heightGapFiller = blockSize.y + + (blockSize.y * 0.05f); // we leave a minimal gap between blocks
			float xPosAux = transform.position.x / widthGapFiller;
			float yPosAux = transform.position.y / heightGapFiller;
			// Calculate x and y positions to create/destroy the block
			float xPos = facingRight ? 
							((float)System.Math.Ceiling(xPosAux) - 0.5f) * widthGapFiller + widthGapFiller:
							((float)System.Math.Floor(xPosAux) + 0.5f) * widthGapFiller - widthGapFiller;
			float yPos = (float)System.Math.Ceiling(yPosAux) * heightGapFiller - (heightGapFiller/2);
			// Vector 3 with the place we'll try to create/destroy the block
			Vector3 placeToCreate = new Vector3(xPos, yPos, 0);
			// Two Vectors3 to check if there is already a gameobject in the position we want to create/destroy the block
			Vector3 placeToCheckBegind = facingRight ? new Vector3(xPos - widthGapFiller/2, yPos, 0) : new Vector3(xPos + widthGapFiller/2, yPos, 0);
			Vector3 placeToCheckEnd = facingRight ? new Vector3(xPos + widthGapFiller/2, yPos, 0) : new Vector3(xPos - widthGapFiller/2, yPos, 0);

			// gameobject (collider2D) ckecking
			Collider2D collider = Physics2D.OverlapBox(new Vector2(xPos, yPos), new Vector2(blockSize.x-0.5f, blockSize.y-0.5f) , 0f);
			if (collider != null) 
			{
				Debug.Log("There is something here so... ");
				if (collider.gameObject.tag == "Breakable")
				{
					Debug.Log("Is a breakable block!");
					Debug.Log("Destroy it!");
					if (particlesDestroy != null)
					{
						Instantiate(particlesDestroy, placeToCreate, Quaternion.identity);	
					}
					Destroy(collider.gameObject);
				} else {
					Debug.Log("Is not a breakable block!");
					Debug.Log("Do not destroy it!");
				}
			} else {
				Debug.Log("There is nothing here, so create a breakable block");
				if (particlesCreate != null)
				{
					Instantiate(particlesCreate, placeToCreate, Quaternion.identity);	
				}				
				Instantiate(block, placeToCreate, Quaternion.identity);
			}
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			// TODO -> no animation by now
			// RegisterAnimatorParameter ("TODO_ANIMATOR_PARAMETER_NAME", AnimatorControllerParameterType.Bool);
		}

		/// <summary>
		/// At the end of the ability's cycle,
		/// we send our current crouching and crawling states to the animator
		/// </summary>
		public override void UpdateAnimator()
		{
			// TODO -> no animations by now
			// MMAnimator.UpdateAnimatorBool(_animator,"TODO_ANIMATOR_PARAMETER_NAME",(_movement.CurrentState == CharacterStates.MovementStates.Crouching), _character._animatorParameters);			
		}
	}
}