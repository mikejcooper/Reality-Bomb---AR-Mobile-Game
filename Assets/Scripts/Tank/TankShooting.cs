using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[Serializable]                             // Allows object to be shown in Inspector.
public class ShootingSettings {

	// This class is used to contain a configuration of the Tanks shooting settings

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
	public int m_LaunchType;                     // Determines the settings used when firing a shell
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
	private float m_TimeStamp;					

	private Button m_FireButton;				// The button used to fire the shells
	private bool m_FireButtonDown;				// Used to check if the the firing button is currently being pressed down

	private void OnEnable()
	{
		// When the tank is turned on, reset the launch force and the UI
		m_CurrentLaunchForce = m_MinLaunchForce;
		m_AimSlider.value = m_AimSlider.minValue;
	}

	private void Start ()
	{

		//Getting reference to the fire button
		m_FireButton = GameObject.Find ("FireButton").gameObject.GetComponent<Button> ();

		//Adding trigger for when the fire button is pressed down
		EventTrigger trigger = m_FireButton.gameObject.AddComponent<EventTrigger>();
		EventTrigger.Entry pointerDown = new EventTrigger.Entry();
		pointerDown.eventID = EventTriggerType.PointerDown;
		pointerDown.callback.AddListener (delegate {
			Down ();
		});
		trigger.triggers.Add (pointerDown);

		//Adding trigger for when the fire button is released
		EventTrigger.Entry pointerUp = new EventTrigger.Entry();
		pointerUp.eventID = EventTriggerType.PointerUp;
		pointerUp.callback.AddListener (delegate {
			Up ();
		});
		trigger.triggers.Add (pointerUp);

		//Set fired to true so the tank doesn't fire immediately as it spawns
		m_Fired = true;

		// Calculate the max slider difference
		m_SliderDifference = m_AimSlider.maxValue - m_AimSlider.minValue;

		// Set the min launch force, max launch force and max charge time
		SetupShell();

		// The rate that the launch force charges up is the range of possible forces by the max charge time.
		m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
	}

	private void Update ()
	{
		//Update all of the fire variables
		if (Time.time >= m_TimeStamp) {
			Update_Fire ();
		}


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
		// Otherwise, if the fire button is being held and the shell hasn't been launched yet...
		else if (m_FireButtonDown == true && !m_Fired)
		{
			// Increment the launch force and update the slider.
			m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

			// Calculate the proportion of the current force
			m_CurrentForceProportion = (m_CurrentLaunchForce - m_MinLaunchForce) / m_LaunchForceDifference;

			// Set the aimslider value to be proportional to the current force
			m_AimSlider.value = m_AimSlider.minValue + m_CurrentForceProportion * m_SliderDifference;
		}
		// Otherwise, if the fire button has just started being pressed...
		else if (m_FireButtonDown == true)
		{
			// ... reset the fired flag and reset the launch force.
			m_Fired = false;
			m_CurrentLaunchForce = m_MinLaunchForce;

			// Change the clip to the charging clip and start it playing.
			m_ShootingAudio.clip = m_ChargingClip;
			m_ShootingAudio.Play ();
		}
		// Otherwise, if the fire button is released and the shell hasn't been launched yet...
		else if (m_FireButtonDown == false && !m_Fired)
		{
			// ... launch the shell.
			Fire ();
		}
	}

	public void Down() 
	{
		m_FireButtonDown = true;
	}

	public void Up() 
	{
		m_FireButtonDown = false;
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

		m_TimeStamp = Time.time + m_TimeBetweenShots;
	}

	private void SetupShell()
	{

		// Give the tank a shooting configuration based on its launch type
		m_MinLaunchForce = m_ShootingSettings[m_LaunchType].m_MinLaunchForce;
		m_MaxLaunchForce = m_ShootingSettings[m_LaunchType].m_MaxLaunchForce;
		m_MaxChargeTime = m_ShootingSettings[m_LaunchType].m_MaxChargeTime;
		m_ShellType = m_ShootingSettings[m_LaunchType].m_ShellType;

		// Calculate the max difference in the launch force
		m_LaunchForceDifference = m_MaxLaunchForce - m_MinLaunchForce;
	}

}
