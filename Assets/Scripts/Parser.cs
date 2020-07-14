/**
 * Parser.cs
 *
 * A simple GCode Parser that reads GCode input and dispatches
 * commands to the various handlers.
 *
 * BasicPrinter only models the physical printer. A separate class
 * 
 *
 */

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class GCode : MonoBehaviour {

	Planner planner;

	void Awake() {
		planner = FindObjectOfType(typeof(Planner)) as Planner;
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
	 * Most Gcode lines go into a FIFO command queue.
	 *
	 * GCode can come from a serial input or from an SD file.
	 * Most commands are processed in the order received, but
	 * since a movement may take several seconds, some commands
	 * may get processed sooner than intended. When you want a
	 * command to wait for the movement queue to drain, use M400.
	 */
	void add_command_to_queue(string gcode) {

	}

	/**
	 * Most Gcode lines go into a FIFO command queue
	 */
	void process_command_queue() {

	}

	/**
	 * Process a single command. GCode moves will be added to
	 * the Planner Queue. Most other commands will be processed
	 * immediately.
	 */
	void process_command(string gcode) {
		gcode = gcode.Trim();
		char[] split = new char[1] { ' ' };
		string[] part = gcode.Split(split);
		char[] letter = part[0].Substring(0, 1).ToCharArray();
		switch (letter[0]) {
			case 'G':
				break;
			case 'M':
				break;
			case 'T':
				break;
			default:
				Debug.Log("Unknown command prefix " + letter[0]);
				break;
		}
	}

	/**
	 * Add a move to the planner
	 */
	void gcode_M0_M1(Vector4 xyzePos) {

	}

}

