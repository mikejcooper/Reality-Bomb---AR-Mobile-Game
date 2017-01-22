using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[Serializable]                             // Allows object to be shown in Inspector.
public class ShootingSettings {

	// This class is used to contain a configuration of the Tanks launch values

	public float m_MinLaunchForce;      // Minimum launch force
	public float m_MaxLaunchForce;      // Maximum launch force
	public float m_MaxChargeTime;       // Maximum charge time of the tank
	public int m_ShellType;             // Type of shell

	public ShootingSettings(float MinForce, float MaxForce, float ChargeTime, int ShellType) {
		m_MinLaunchForce = MinForce;
		m_MaxLaunchForce = MaxForce;
		m_MaxChargeTime = ChargeTime;
		m_ShellType = ShellType;
	}

}

public class TankShooting : MonoBehaviour
{
	public int m_PlayerNumber = 1;              // Used to identify the different players.
	public Rigidbody m_Shell;                   // Prefab of the shell.
	public Rigidbody m_Shell1;                  // Prefab of shell 1.
	public Transform m_FireTransform;           // A child of the tank where the shells are spawned.
	public Slider m_AimSlider;                  // A child of the tank that displays the current launch force.
	public AudioSource m_ShootingAudio;         // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
	public AudioClip m_ChargingClip;            // Audio that plays when each shot is charging up.
	public AudioClip m_FireClip;                // Audio that plays when each shot is fired.
	public ShootingSettings[] m_ShootingSettings;    // Array containing different configurations for the shooting behaviour of the tank
	private int m_ShellType;                     // Determines what type of shell you will be firing

	private float m_MinLaunchForce;             // The force given to the shell if the fire button is not held.
	private float m_MaxLaunchForce;             // The force given to the shell if the fire button is held for the max charge time.
	private float m_MaxChargeTime;              // How long the shell can charge for before it is fired at max force.
	private float m_LaunchForceDifference;      // Difference between the max launch force and the min launch force
	private float m_SliderDifference;           // Difference between the max slider value and min slider value

	private float m_CurrentLaunchForce;         // The force that will be given to the shell when the fire button is released.
	private float m_CurrentForceProportion;     // The proportion of the current launch force out of the max launch force
	private float m_ChargeSpeed;                // How fast the launch force increases, based on the max charge time.
	private bool m_Fired;                       // Whether or not the shell has been launched with this button press.
	public float m_TimeBetweenShots;			// The delay before another shot can be taken
	private float m_TimeStamp;					// The next time that the tank is able to fire

	private Button m_FireButtonShort;			// Button used to fire short shots
	private Button m_FireButtonLong;			// Button used to fire long shots
	private bool m_FireButtonShortDown;			// Used to check if the short fire button is currently pressed down
	private bool m_FireButtonLongDown;			// Used to check if the long fire button is currently pressed down
	private bool m_Charging;					// Used to check if a shot is charging

	private void OnEnable()
	{
		// When the tank is turned on, reset the launch force and the UI
		m_CurrentLaunchForce = m_MinLaunchForce;
		m_AimSlider.value = m_AimSlider.minValue;
	}

	private void Start ()
	{

		//get references to the firing buttons and add event triggers
		SetupFireButtons ();

		//Set fired to true so the tank doesn't fire immediately as it spawns
		m_Fired = true;

		// Calculate the max slider difference
		m_SliderDifference = m_AimSlider.maxValue - m_AimSlider.minValue;

		// Set the min launch force, max launch force and max charge time
		SetupShell(0);

		// The rate that the launch force charges up is the range of possible forces by the max charge time.
		m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
	}

	private void Update ()
	{
		//Only update all of the fire values if the firing delay is over
		if (Time.time >= m_TimeStamp)
		{
			Update_Fire ();
		}
	}

	//Get the firing button reference and create event triggers
	private void SetupFireButton(Button FireButton, int num) 
	{
		//Get a reference to the button
		String buttonName = "FireButton" + Convert.ToString (num);
		FireButton = GameObject.Find (buttonName).gameObject.GetComponent<Button> ();

		//Adding trigger for when the fire button is pressed down
		EventTrigger trigger = FireButton.gameObject.AddComponent<EventTrigger>();
		EventTrigger.Entry pointerDown = new EventTrigger.Entry();
		pointerDown.eventID = EventTriggerType.PointerDown;
		pointerDown.callback.AddListener (delegate {FireButtonDown(num);});
		trigger.triggers.Add (pointerDown);

		//Adding trigger for when the fire button is released
		EventTrigger.Entry pointerUp = new EventTrigger.Entry();
		pointerUp.eventID = EventTriggerType.PointerUp;
		pointerUp.callback.AddListener (delegate {FireButtonUp(num);});
		trigger.triggers.Add (pointerUp);
	}

	//Setup all of the firing buttons
	private void SetupFireButtons() 
	{
		SetupFireButton (m_FireButtonShort, 0);
		SetupFireButton (m_FireButtonLong, 1);
	}
		
	private void Update_Fire()
	{
		// The slider should have a default value of the minimum launch force.
		m_AimSlider.value = m_AimSlider.minValue;

		// If the max force has been exceeded and the shell hasn't yet been launched...
		if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
		{
			// ... use the max force and launch the shell.
			m_CurrentLaunchForce = m_MaxLaunchForce;
			Fire ();
		}
		// Otherwise, if shot is still charging and the shell hasn't been launched yet...
		else if (m_Charging == true && !m_Fired)
		{
			// Increment the launch force and update the slider.
			m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

			// Calculate the proportion of the current force
			m_CurrentForceProportion = (m_CurrentLaunchForce - m_MinLaunchForce) / m_LaunchForceDifference;

			// Set the aimslider value to be proportional to the current force
			m_AimSlider.value = m_AimSlider.minValue + m_CurrentForceProportion * m_SliderDifference;
		}
		// Otherwise, if the shot has just started charging...
		else if (m_Charging == true)
		{
			// ... reset the fired flag and reset the launch force.
			m_Fired = false;
			m_CurrentLaunchForce = m_MinLaunchForce;

			// Change the clip to the charging clip and start it playing.
			m_ShootingAudio.clip = m_ChargingClip;
			m_ShootingAudio.Play ();
		}
		// Otherwise, if the shot has finished charging and the shell hasn't been launched yet...
		else if (m_Charging == false && !m_Fired)
		{
			// ... launch the shell.
			Fire ();
		}
	}

	//Called when a firing button in pressed down
	public void FireButtonDown(int fireButton) 
	{
		m_Charging = false;
		//FireButtonShort is pressed down
		if (fireButton == 0) 
		{
			m_FireButtonShortDown = true;
			//Only charge shot if FireButtonLong is not being pressed
			if (m_FireButtonLongDown == false)
			{
				m_Charging = true;
				SetupShell(0);
			}
		} 
		//FireButtonLong is pressed down
		else if (fireButton == 1) 
		{
			m_FireButtonLongDown = true;
			//Only charge shot if FireButtonShort is not being pressed
			if (m_FireButtonShortDown == false) 
			{
				m_Charging = true;
				SetupShell (1);
			}
		}
	}

	//Called when a firing button is released
	public void FireButtonUp(int fireButton) 
	{
		m_Charging = false;
		//FireButtonShort is released
		if (fireButton == 0) 
		{
			m_FireButtonShortDown = false;
			//If FireButtonLong is being pressed down then charge shot
			if (m_FireButtonLongDown == true) {
				m_Charging = true;
				SetupShell (1);
			}
		}
		//if FireButtonLong is released
		else if (fireButton == 1)
		{
			m_FireButtonLongDown = false;
			//If FireButtonShort is being pressed down then charge shot
			if (m_FireButtonShortDown == true) 
			{
				m_Charging = true;
				SetupShell (0);
			}
		}
	}
		
	public void Fire ()
	{

		// Set the fired flag so only Fire is only called once.
		m_Fired = true;

		// Create an instance of a shell depending on the value of ShellType and store a reference to it's rigidbody.
		Rigidbody shellInstance;
		if(m_ShellType == 0)
		{
			shellInstance = Instantiate (m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
		} 
		else 
		{
			shellInstance = Instantiate (m_Shell1, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
		}

		// Set the shell's velocity to the launch force in the fire position's forward direction.
		shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;

		// Change the clip to the firing clip and play it.
		m_ShootingAudio.clip = m_FireClip;
		m_ShootingAudio.Play ();

		// Reset the launch force.  This is a precaution in case of missing button events.
		m_CurrentLaunchForce = m_MinLaunchForce;

		// Calculate the next time after which we can shoot
		m_TimeStamp = Time.time + m_TimeBetweenShots;
	}

	//Set the tanks launch values to a configuration set in the editor
	private void SetupShell(int launchType)
	{

		// Give the tank a shooting configuration based on its launch type
		m_MinLaunchForce = m_ShootingSettings[launchType].m_MinLaunchForce;
		m_MaxLaunchForce = m_ShootingSettings[launchType].m_MaxLaunchForce;
		m_MaxChargeTime = m_ShootingSettings[launchType].m_MaxChargeTime;
		m_ShellType = m_ShootingSettings[launchType].m_ShellType;

		// Calculate the max difference in the launch force
		m_LaunchForceDifference = m_MaxLaunchForce - m_MinLaunchForce;
	}

}
