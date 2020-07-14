/**
 * Motor.cs
 *
 * A stepper motor. (Stepper is the stepper manager.)
 * Names may be changed later.
 *
 * Modeling components:
 *
 * A "real axis" is best described as distance_per_revolution, thus everything,
 * including the pulley, which is not part of the stepper motor is part of the
 * "real axis" assembly.
 *
 * A Motor can then "plug in" to this axis with its steps_per_revolution.
 * So, you could say: "rotate Z 1000 steps" and see how far it goes.
 *
 * The feedback between Axis and Motor goes both ways. Change an Axis and it
 * changes one or more Motors. Change a Motor and it moves at least one Axis.
 *
 * All in all the Motor is "dumb." Its position can be set directly for high-
 * level movement, or its step-logic can be invoked for full simulation.
 * Note that these Motors don't move on their own. They are "set" in three ways:
 *
 * 1. The position value is initialized (no stepping or mechanical feedback).
 * 2. A single microstep is applied in a certain direction (from Stepper.cs).
 * 3. A whole step is applied for fastest movement. (Is this an option?)
 * 4. The position is re-initialized based on the "real axis."
 * 5. The Stepper code may set the position directly instead of stepping.
 * 6. In a certain mode the position (steps) may always be calculated from XYZ.
 * 7. The Motor can act as a utility for converting mm to steps and vice-versa,
 *    but the caller needs to provide the distance-per-revolution value.
 *
 */

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Motor : MonoBehaviour {

	public int position = 0; // In total microsteps. Sometimes the same as the axis position, but not guaranteed.

	public float fraction {
		get {
			float f = (float)position;
			f -= Mathf.Floor(f/microsteps_per_revolution) * microsteps_per_revolution;
			if (f < 0) f += microsteps_per_revolution;
			return f / microsteps_per_revolution;
		}
	}
	public float angle { get { return fraction * 360; } }
	public float radians { get { return fraction * 2 * Mathf.PI; } }

	public int full_steps_per_revolution = 200;

	//
	// The Stepper Driver is built into the Motor class
	// so microsteps and direction bits are handled here also
	//
	public int microsteps = 16;
	public int microsteps_per_revolution { get { return full_steps_per_revolution * microsteps; } }

	int step_add = 1;
	bool _direction_bit = false;
	public bool direction_bit {
		get { return _direction_bit; }
		set { _direction_bit = value; step_add = value ? -1 : 1; }
	}

	public bool redundant_motor = false; // The second Z motor shouldn't do anything, for example

	// Conversion utilities
	public float steps_per_mm(float mm_per_revolution) { return mm_per_revolution / full_steps_per_revolution; }
	public float microsteps_per_mm(float mm_per_revolution) { return mm_per_revolution / microsteps_per_revolution; }

	void Awake() {
	}

	void Start() {
	}

	// Frequently, maybe more than 60fps
	void FixedUpdate() {
	}

	// Once per frame
	void Update () {
	}

	//
	// Pulse the motor to microstep once.
	// This is usually done at very high frequency. For the realtime sim this can
	// be bypassed, setting the stepper positions directly along with the final
	// output positions to the Printer axes.
	//
	void microstep() { position += step_add; }

}

