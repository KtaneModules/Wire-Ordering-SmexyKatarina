using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rand = UnityEngine.Random;

public class WireOrdering : MonoBehaviour {

	// Module Components
	public KMBombInfo bomb;
	public KMAudio bAudio;

	public KMSelectable[] wires;
	public MeshRenderer[] wireRenders;
	public Transform[] wireHLTransforms;
	public MeshRenderer[] cutWiresRenders;
	public MeshRenderer[] buttonRenders;
	public MeshRenderer[] connectorRenders;
	public TextMesh[] buttonTexts;

	public Material[] colorMats;

	// Specifically for Logging
	static int modIDCount = 1;
	int modID;
	private bool modSolved = false;

	// Module Vars
	string[] wireNames = new string[] { "Wire1", "Wire2", "Wire3", "Wire4" };
	bool[] wireCuts;

	int moduleRule;
	int[] wireColors, buttonColors, connectorsActive, buttonNumbers, cutOrder;
	bool[] buttonsUsed, colorsUsed, connectorsUsed, numbersUsed;
	int statModuleCuts = 0;
	int moduleCuts = 0;

	void Awake() {
		modID = modIDCount++;

		foreach (KMSelectable wire in wires) {
			if (wire.name.EqualsAny("Wire1", "Wire2", "Wire3", "Wire4") && modSolved == false) {
				wire.OnInteract += delegate () { WireCut(wire); return false; };
			}
		}
	}

	void FixedUpdate() {
		if (!modSolved) {
			CheckCuts();
		}
	}

	void Start() {
		wireColors = new int[4];
		buttonColors = new int[4];
		connectorsActive = new int[4];
		buttonNumbers = new int[4];
		buttonsUsed = new bool[] { false, false, false, false };
		colorsUsed = new bool[] { false, false, false, false };
		connectorsUsed = new bool[] { false, false, false, false };
		numbersUsed = new bool[] { false, false, false, false }; 
		wireCuts = new bool[] { false, false, false, false, false };
		GenerateModule();
	}

	void WireCut(KMSelectable wire) {
		int index = Array.IndexOf(wireNames, wire.name);
		if (wireCuts[index] == true) { return; }
		CheckCutWire(index);
	}

	void UpdateWire(int index) {
		MeshRenderer wireRend = wireRenders[index];
		MeshRenderer cWireRend = cutWiresRenders[index];
		Transform wireHLTF = wireHLTransforms[index];
		wireRend.enabled = false;
		cWireRend.enabled = true;
		wireHLTF.localScale = new Vector3(0, 0, 0);
		wireCuts[index] = true;
		bAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, transform);
	}

	void CheckCutWire(int index) {
		if (cutOrder[statModuleCuts] == index) {
			UpdateWire(index);
			statModuleCuts++;
			if (statModuleCuts == 4) {
				Debug.LogFormat("[Wire Ordering #{0}]: Wire cut was: {1}, and is correct. Module solved.", modID, index + 1);
				SolveModule();
				return;
			}
			Debug.LogFormat("[Wire Ordering #{0}]: Wire cut was: {1}, and is correct.", modID, index+1);
			moduleCuts++;
			return;
		}
		if (cutOrder[statModuleCuts] != index) {
			UpdateWire(index);
			StrikeModule(index);
			moduleCuts++;
			return;
		}
	}

	void GenerateModule() {
		// Generate Wire Colors
		for (int x = 0; x <= 3; x++) {
			int colorIndex = rand.Range(0, 8);
			while (wireColors.Contains(colorIndex+1)) {
				colorIndex = rand.Range(0, 8);
			}
			MeshRenderer wireMr = wireRenders[x];
			MeshRenderer cWireMr = cutWiresRenders[x];
			wireMr.material = colorMats[colorIndex];
			cWireMr.material = colorMats[colorIndex];
			wireColors[x] = colorIndex + 1;
		}
		Debug.LogFormat("[Wire Ordering #{0}]: Wire colors are: {1}, {2}, {3}, {4}.", modID, wireColors[0], wireColors[1], wireColors[2], wireColors[3]);
		// Generate Button Colors
		for (int x = 0; x <= 3; x++) {
			int buttonRandom = rand.Range(0, 4);
			int colorIndex = rand.Range(0, 4);
			while (buttonsUsed[buttonRandom] == true) {
				buttonRandom = rand.Range(0, 4);
			}
			while (colorsUsed[colorIndex] == true) {
				colorIndex = rand.Range(0, 4);
			}
			buttonRenders[buttonRandom].material = colorMats[wireColors[colorIndex] - 1];
			buttonsUsed[buttonRandom] = true;
			colorsUsed[colorIndex] = true;
			buttonColors[buttonRandom] = wireColors[colorIndex];
		}
		Debug.LogFormat("[Wire Ordering #{0}]: Display colors are: {1}, {2}, {3}, {4}.", modID, buttonColors[0], buttonColors[1], buttonColors[2], buttonColors[3]);
		// Generate Button Numbers
		buttonsUsed = new bool[] { false, false, false, false };
		for (int x = 0; x <= 3; x++) {
			int numberRand = rand.Range(0, 4);
			int buttonRand = rand.Range(0, 4);
			while (numbersUsed[numberRand] == true)
			{
				numberRand = rand.Range(0, 4);
			}
			while (buttonsUsed[buttonRand] == true)
			{
				buttonRand = rand.Range(0, 4);
			}
			numbersUsed[numberRand] = true;
			buttonsUsed[buttonRand] = true;
			buttonTexts[buttonRand].color = returnTextColor(buttonColors[buttonRand]);
			buttonTexts[buttonRand].text = "" + (numberRand + 1);
			buttonNumbers[buttonRand] = numberRand + 1;
		}
		Debug.LogFormat("[Wire Ordering #{0}]: Display numbers are: {1}, {2}, {3}, {4}.", modID, buttonTexts[0].text, buttonTexts[1].text, buttonTexts[2].text, buttonTexts[3].text);
		// Generate Button Connectors
		for (int x = 0; x <= 3; x++) {
			int conRand;
			switch (x) {
				case 0:
					conRand = rand.Range(0, 4);
					connectorRenders[conRand].enabled = true;
					connectorsActive[x] = connectorPosition(conRand)+1;
					connectorsUsed[connectorPosition(conRand)] = true;
					break;
				case 1:
					conRand = rand.Range(4, 8);
					while (connectorsUsed[connectorPosition(conRand)] == true) {
						conRand = rand.Range(4, 8);
					}
					connectorRenders[conRand].enabled = true;
					connectorsActive[x] = connectorPosition(conRand)+1;
					connectorsUsed[connectorPosition(conRand)] = true;
					break;
				case 2:
					conRand = rand.Range(8, 12);
					while (connectorsUsed[connectorPosition(conRand)] == true)
					{
						conRand = rand.Range(8, 12);
					}
					connectorRenders[conRand].enabled = true;
					connectorsActive[x] = connectorPosition(conRand)+1;
					connectorsUsed[connectorPosition(conRand)] = true;
					break;
				case 3:
					conRand = rand.Range(12, 16);
					while (connectorsUsed[connectorPosition(conRand)] == true)
					{
						conRand = rand.Range(12, 16);
					}
					connectorRenders[conRand].enabled = true;
					connectorsActive[x] = connectorPosition(conRand)+1;
					connectorsUsed[connectorPosition(conRand)] = true;
					break;
				default:
					Debug.LogFormat("[Wire Ordering #{0}]: Unable to generate the button connectors.", modID);
					break;
			}
		}
		Debug.LogFormat("[Wire Ordering #{0}]: Displays (from left to right) are connected to wires (from left to right): {1}, {2}, {3}, {4} respectively.", modID, connectorsActive[0], connectorsActive[1], connectorsActive[2], connectorsActive[3]);

		// Module Rule Setup
		moduleRule = calculateRuleNumber();
		Debug.LogFormat("[Wire Ordering #{0}]: The rule number is: {1}", modID, moduleRule);
		switch (moduleRule) {
			case 0:
				// Cut from left to right
				cutOrder = new int[] { 0, 1, 2, 3 };
				break;
			case 1:
				// Cut from right to left
				cutOrder = new int[] { 3, 2, 1, 0 };
				break;
			case 2:
				// Cut based upon the button displays
				cutOrder = new int[]
				{
					Array.IndexOf(buttonNumbers,1),
					Array.IndexOf(buttonNumbers,2),
					Array.IndexOf(buttonNumbers,3),
					Array.IndexOf(buttonNumbers,4)
				};
				break;
			case 3:
				// Cut based upon connectors
				cutOrder = new int[]
				{
					connectorsActive[0]-1,
					connectorsActive[1]-1,
					connectorsActive[2]-1,
					connectorsActive[3]-1
				};
				break;
			case 4:
				// Cut wire based upon the color of the button with the displays in order
				cutOrder = new int[]
				{
					// Index of wire that needs to be cut, but needs the color of the button, but needs the number in order
					// 
					Array.IndexOf(wireColors,buttonColors[Array.IndexOf(buttonNumbers,1)]),
					Array.IndexOf(wireColors,buttonColors[Array.IndexOf(buttonNumbers,2)]),
					Array.IndexOf(wireColors,buttonColors[Array.IndexOf(buttonNumbers,3)]),
					Array.IndexOf(wireColors,buttonColors[Array.IndexOf(buttonNumbers,4)])
				};
				break;
			case 5:
				// Cut the wire same color as buttons from left to right
				cutOrder = new int[]
				{
					Array.IndexOf(wireColors, buttonColors[0]),
					Array.IndexOf(wireColors, buttonColors[1]),
					Array.IndexOf(wireColors, buttonColors[2]),
					Array.IndexOf(wireColors, buttonColors[3])
				};
				break;
			case 6:
				// Cut the wires in order (ascending) of their colors in the table
				int[] ascending = new int[4];
				wireColors.CopyTo(ascending,0);
				Array.Sort(ascending);
				cutOrder = new int[]
				{
					Array.IndexOf(wireColors, ascending[0]),
					Array.IndexOf(wireColors, ascending[1]),
					Array.IndexOf(wireColors, ascending[2]),
					Array.IndexOf(wireColors, ascending[3])
				};
				break;
			case 7:
				ascending = new int[4];
				buttonColors.CopyTo(ascending, 0);
				Array.Sort(ascending);
				cutOrder = new int[]
				{
					Array.IndexOf(buttonColors, ascending[0]),
					Array.IndexOf(buttonColors, ascending[1]),
					Array.IndexOf(buttonColors, ascending[2]),
					Array.IndexOf(buttonColors, ascending[3])
				};
				break;
			case 8:
				int[] descending = new int[4];
				wireColors.CopyTo(descending, 0);
				Array.Sort(descending);
				Array.Reverse(descending);
				cutOrder = new int[]
				{
					Array.IndexOf(wireColors, descending[0]),
					Array.IndexOf(wireColors, descending[1]),
					Array.IndexOf(wireColors, descending[2]),
					Array.IndexOf(wireColors, descending[3])
				};
				break;
			case 9:
				descending = new int[4];
				buttonColors.CopyTo(descending, 0);
				Array.Sort(descending);
				Array.Reverse(descending);
				cutOrder = new int[]
				{
					Array.IndexOf(buttonColors, descending[0]),
					Array.IndexOf(buttonColors, descending[1]),
					Array.IndexOf(buttonColors, descending[2]),
					Array.IndexOf(buttonColors, descending[3])
				};
				break;
			default:
				Debug.LogFormat("[Wire Ordering #{0}]: Unable to generate rule/proper wire cuts.",modID);
				break;
		}
		Debug.LogFormat("[Wire Ordering #{0}]: The correct order for the cuts are: {1}, {2}, {3}, {4}", modID, cutOrder[0]+1, cutOrder[1]+1, cutOrder[2]+1, cutOrder[3]+1);
	}

	void SolveModule() {
		modSolved = true;
		bAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
		GetComponent<KMBombModule>().HandlePass();
	}

	void StrikeModule(int index) {
		GetComponent<KMBombModule>().HandleStrike();
		Debug.LogFormat("[Wire Ordering #{0}]: Wire cut was {1}, and that was incorrect. Strike has been issued.", modID, (index+1));
	}

	int connectorPosition(int conId) {
		int conPos = 0;
		switch (conId) {
			case 0:
			case 1:
			case 2:
			case 3:
				conPos = conId;
				break;
			case 4:
			case 5:
			case 6:
			case 7:
				conPos = conId - 4;
				break;
			case 8:
			case 9:
			case 10:
			case 11:
				conPos = conId - 8;
				break;
			case 12:
			case 13:
			case 14:
			case 15:
				conPos = conId - 12;
				break;
		}
		return conPos;
	}

	int calculateRuleNumber() {
		return ((buttonColors[0] * buttonNumbers[0]) + (buttonColors[1] * buttonNumbers[1]) + (buttonColors[2] * buttonNumbers[2]) + (buttonColors[3] * buttonNumbers[3])) % 10;
	}

	void CheckCuts() {
		if (modSolved) {
			return;
		}
		if (statModuleCuts == 4) {
			SolveModule();
			return;
		}
		if (wireCuts[cutOrder[statModuleCuts]] == true && statModuleCuts != moduleCuts) {
			statModuleCuts++;
		}
	}

	Color returnTextColor(int buttonColor) {
		if (buttonColor.EqualsAny(1, 2, 3, 4, 5, 6, 7)) {
			return new Color(0,0,0,255);
		}
		return new Color(255, 255, 255, 255);
	}

	bool isArgsValid(string[] args) {
		foreach (string arg in args) {
			Debug.Log(arg);
			string[] validPositions = new string[] { "1", "2", "3", "4" };
			if (arg.Equals("cut")) {
				continue;
			}
			if (!validPositions.Contains(arg)) {
				return false;
			}
		}
		return true;
	}

	#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} cut <number,number...> [Cuts the wires at the specified positions from left to right, can be chained by spaces in between ]";
	#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string command) {
		string[] args = command.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		if (args.Length >= 6 || args.Length <= 1) {
			yield return "sendtochaterror Too many or not enough positions were given, please try again.";
		}
		string[] sepArgs = new string[args.Length-1];
		for (int i = 0; i < args.Length - 1; i++) {
			sepArgs[i] = args[i+1];
		}
		if (!isArgsValid(sepArgs)) {
			yield return "sendtochaterror One or more positions is invalid, please try again.";
		}
		foreach (string s in sepArgs) {
			CheckCutWire(int.Parse(s)-1);
			yield return new WaitForSeconds(0.2f);
		}
	}

	IEnumerator TwitchHandleForcedSolve() {
		for (int i = 0; i <= 3; i++) {
			CheckCutWire(cutOrder[i]);
			yield return new WaitForSeconds(0.5f);
		}
	}

}
