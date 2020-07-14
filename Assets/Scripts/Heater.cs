/**
 * Heater.cs
 *
 * A heater and a mass that takes time to reach temperature.
 * Also an attached sensor that reports the temperature.
 *
 * This is a fairly high-level abstraction of a heater unit,
 * since it can be set instantly to any temperature.
 * However, as an object in a simulation environment it can
 * closely mimic the behavior of a real heater by taking time
 * to reach a target, wavering around the target, etc.
 *
 */

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Heater : MonoBehaviour {

	public float celsius = 20, ambient = 20;
	public float hysteresis = 3; // within this range
	public float rate = 1; // speed of heating compared to an "average" heater

	const float heat_dps = 2, cool_dps = 1.5f; // degrees per second for the simulation

	bool instant = true;
	bool heater_on = false;
	bool did_reach_target = false;

	private float _target = 0;
	public float target {
		get { return _target; }
		set { _target = value; did_reach_target = false; }
	}

	void Awake() {
		ambient = celsius = 19 + 2 * Random.value;
		target = 0;
		rate = 1;
		instant = true;
	}

	void Start() {
	}

	// Frequently, maybe more than 60fps
	void FixedUpdate() {

		if (heater_on) {
			if (celsius >= target) {
				celsius = target;		// (Just to keep it from oscillating continuously.)
				heater_on = false; // Turn off heater if temperature was reached
				did_reach_target = true;

				// heater_reached_target(); // Stop the thermal watchdog
			}
		}
		else if (celsius < target) {
			if (celsius < target - hysteresis) {
				// Enable Heater if below the target temperature minus hysteresis
				// In a real printer "hysteresis" represents the allowed range of
				// inaccuracy before the printer starts the countdown for an alarm.
				heater_on = true;
				did_reach_target = false;
			}

			// watch_heater_for_heating(); // If the heater is too slow this will trigger an alarm
		}

		// Note: A real mass takes some time before starting to rise
		// and turning off the heater leaves behind some residual heat
		// that hasn't reached the sensor yet.
		// A more accurate simulation would account for these issues
		// by calculating actual watts spent on the heating resistor
		// and the watts gained, retained, and lost by the thermal mass.
		// When new mass is added in the form of cold filament, that
		// could be accounted for in terms of watts also.
		if (heater_on) {
			celsius += heat_dps * Time.deltaTime;
			// thermal runaway?
		}
		else if (celsius > ambient) {
			// can't cool below ambient temperature (20)
			celsius -= cool_dps * Time.deltaTime;
		}
	}

	// Once per frame
	void Update () {
	}

	public void SetInstant(bool bInstant=true) { instant = bInstant; }

}

