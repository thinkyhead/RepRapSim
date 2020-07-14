/**
 * Planner.cs
 *
 * A lookahead planner that converts axis moves (mm) into stepper moves (steps).
 *
 * The planner also pre-scans the buffer to decide on acc/decelerate values.
 *
 * For simplicity I'll just transpose the planner from Marlin (C++) to Unity (C#).
 *
 *  1. Simplifies the composition of functions and variables.
 *  2. Provides a sandbox for planner as a class.
 *  3. Re-composing code here is easier than within Marlin.
 *  4. 
 *
 */

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Planner : MonoBehaviour {

	Stepper stepper = new Stepper();

	int block_buffer_head, block_buffer_tail;

	void Awake() {
		block_buffer_head = block_buffer_tail = 0;
	}

	void Start() {
	}

	// Frequently, maybe more than 60fps
	void FixedUpdate() {
	}

	// Once per frame
	void Update () {
	}

	/**
	 * The Planner manages moves, including acceleration, coordinating 4
	 * positional outputs (XYZE). Moves are given to the planner in
	 * Cartesian XYZ coordinates in the printer's coordinate space. Thus,
	 * for some 0,0 will be front-left, while others have 0,0 in the center.
	 *
	 * High-level details (where to move, how fast, splitting lines, etc.)
	 * are handled by the Printer object. The Planner decides the actual speeds
	 * and hands off the movement 
	 */
	void add_move(Vector4 xyzePos) {

	}

}

