using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class WireOrderingScript : MonoBehaviour {

	// Unity Vars
	public KMBombInfo _bomb;
	public KMAudio _audio;

	public KMSelectable[] _wires;

	public MeshRenderer[] _cutRenderers;
	public MeshRenderer[] _displayRenderers;
	public MeshRenderer[] _connectorRenderers1, _connectorRenderers2, _connectorRenderers3, _connectorRenderers4;

	public TextMesh[] _displayTexts;

	public Material[] _colorMaterials;

	// Logging
	static int _modIDCount = 1;
	int _modID;
	private bool _modSolved = false;

	// Mod Vars

	int[] _chosenColors = new int[4];
	int[] _chosenColorsDis = new int[4];
	int[] _chosenColorsWire = new int[4];
	int[] _chosenDisNum = new int[4];
	int[] _chosenConPos = new int[4];
	int[] _chosenCutOrder = new int[4];
	bool[] _cutWires = new bool[4];

	int _cutIndex = 0;
	int _statCutIndex = 0;

	void Awake() {
		_modID = _modIDCount++;

		foreach (KMSelectable wire in _wires) {
			wire.OnInteract += delegate () { CutWire(wire); return false; };
		}
	}

	void Start() {
		GenerateModule();
	}

	void FixedUpdate() {
		if (_modSolved) { return; }
		CheckCuts();
	}

	void GenerateModule() {
		GenerateColors();
		UpdateColors();
		GenerateTexts();
		GenerateConnectors();
		GenerateCuts();
	}

	void CutWire(KMSelectable wire) {
		int index = Array.IndexOf(_wires,wire);
		if (index != _chosenCutOrder[_statCutIndex]) {
			_cutIndex++;
			_wires[index].GetComponent<Renderer>().enabled = false;
			_wires[index].Highlight.gameObject.SetActive(false);
			_cutRenderers[index].enabled = true;
			_cutWires[index] = true;
			GetComponent<KMBombModule>().HandleStrike();
			Debug.LogFormat("[Wire Ordering #{0}]: Wire cut was: {1} and was incorrect.", _modID, index+1);
			return;
		} else {
			switch (index)
			{
				case 0:
					_cutIndex++;
					_statCutIndex++;
					_wires[0].GetComponent<Renderer>().enabled = false;
					_wires[0].Highlight.gameObject.SetActive(false);
					_cutRenderers[0].enabled = true;
					_cutWires[0] = true;
					Debug.LogFormat("[Wire Ordering #{0}]: 1st wire was cut.", _modID);
					break;
				case 1:
					_cutIndex++;
					_statCutIndex++;
					_wires[1].GetComponent<Renderer>().enabled = false;
					_wires[1].Highlight.gameObject.SetActive(false);
					_cutRenderers[1].enabled = true;
					_cutWires[1] = true;
					Debug.LogFormat("[Wire Ordering #{0}]: 2nd wire was cut.", _modID);
					break;
				case 2:
					_cutIndex++;
					_statCutIndex++;
					_wires[2].GetComponent<Renderer>().enabled = false;
					_wires[2].Highlight.gameObject.SetActive(false);
					_cutRenderers[2].enabled = true;
					_cutWires[2] = true;
					Debug.LogFormat("[Wire Ordering #{0}]: 3rd wire was cut.", _modID);
					break;
				case 3:
					_cutIndex++;
					_statCutIndex++;
					_wires[3].GetComponent<Renderer>().enabled = false;
					_wires[3].Highlight.gameObject.SetActive(false);
					_cutRenderers[3].enabled = true;
					_cutWires[3] = true;
					Debug.LogFormat("[Wire Ordering #{0}]: 4th wire was cut.", _modID);
					break;
				default:
					Debug.LogFormat("[Wire Ordering #{0}]: Unable to update wire.", _modID);
					break;
			}
			if (_cutIndex == 4 && _statCutIndex == 4)
			{
				StartCoroutine(SolveModule());
				return;
			}
		}
		return;
	}

	// Generate Colors
	void GenerateColors() {
		for (int i = 0; i <= 3; i++) {
			int color = rnd.Range(0, 8);
			while (_chosenColors.Contains(color)) {
				color = rnd.Range(0, 8);
			}
			_chosenColors[i] = color;
		}
	}

	// Updates Display and Wire colors.
	void UpdateColors() {
		bool[] dis = new bool[4];
		bool[] wire = new bool[4];
		bool[] usedColors = new bool[4];

		int[] disc = new int[4];
		int[] wirec = new int[4];

		for (int i = 0; i <= 3; i++) {
			int rand = rnd.Range(0, 4);	
			while (usedColors[rand] == true) {
				rand = rnd.Range(0, 4);
			}
			int color = _chosenColors[rand];
			usedColors[rand] = true;
			int xpos = rnd.Range(0, 4);
			int ypos = rnd.Range(0, 4);
			while (dis[xpos] == true) {
				xpos = rnd.Range(0, 4);
			}
			while (wire[ypos] == true) {
				ypos = rnd.Range(0, 4);
			}
			_displayRenderers[xpos].material = _colorMaterials[color];
			_wires[ypos].GetComponent<Renderer>().material = _colorMaterials[color];
			_cutRenderers[ypos].material = _colorMaterials[color];
			dis[xpos] = true;
			wire[ypos] = true;
			disc[xpos] = Array.IndexOf(_chosenColors, color);
			wirec[ypos] = Array.IndexOf(_chosenColors, color);
			_chosenColorsDis[xpos] = color;
			_chosenColorsWire[ypos] = color;
			
		}
		Debug.LogFormat("[Wire Ordering #{0}]: The display colors are: {1}, {2}, {3}, {4}.", _modID, _colorMaterials[_chosenColors[disc[0]]].name, _colorMaterials[_chosenColors[disc[1]]].name, _colorMaterials[_chosenColors[disc[2]]].name, _colorMaterials[_chosenColors[disc[3]]].name);
		Debug.LogFormat("[Wire Ordering #{0}]: The wire colors are: {1}, {2}, {3}, {4}.", _modID, _colorMaterials[_chosenColors[wirec[0]]].name, _colorMaterials[_chosenColors[wirec[1]]].name, _colorMaterials[_chosenColors[wirec[2]]].name, _colorMaterials[_chosenColors[wirec[3]]].name);
	}

	// Generates Display Texts
	void GenerateTexts() {
		bool[] numUsed = new bool[4];
		for (int i = 0; i <= 3; i++) {
			int num = rnd.Range(1, 5);
			while (numUsed[num - 1] == true)
			{
				num = rnd.Range(1, 5);
			}
			_chosenDisNum[i] = num;
			numUsed[num - 1] = true;
			_displayTexts[i].color = _chosenColorsDis[i] == 7 ? new Color(255, 255, 255, 255) : new Color(0, 0, 0, 255);
			_displayTexts[i].text = num.ToString();
		}
		Debug.LogFormat("[Wire Ordering #{0}]: The display numbers are: {1}, {2}, {3}, {4}.", _modID, _chosenDisNum[0], _chosenDisNum[1], _chosenDisNum[2], _chosenDisNum[3]);
	}

	void GenerateConnectors() {
		int conRand;
		int correct;
		bool[] connectorsUsed = new bool[4];
		for (int i = 0; i <= 3; i++) {
			switch (i) {
				case 0:
					conRand = rnd.Range(0, 4);
					correct = ConnectorPosition(conRand);
					_connectorRenderers1[correct].enabled = true;
					_chosenConPos[i] = correct;
					connectorsUsed[correct] = true;
					break;
				case 1:
					conRand = rnd.Range(4, 8);
					while (connectorsUsed[ConnectorPosition(conRand)] == true)
					{
						conRand = rnd.Range(4, 8);
					}
					correct = ConnectorPosition(conRand);
					_connectorRenderers2[correct].enabled = true;
					_chosenConPos[i] = correct;
					connectorsUsed[correct] = true;
					break;
				case 2:
					conRand = rnd.Range(8, 12);
					while (connectorsUsed[ConnectorPosition(conRand)] == true)
					{
						conRand = rnd.Range(8, 12);
					}
					correct = ConnectorPosition(conRand);
					_connectorRenderers3[correct].enabled = true;
					_chosenConPos[i] = correct;
					connectorsUsed[correct] = true;
					break;
				case 3:
					conRand = rnd.Range(12, 16);
					while (connectorsUsed[ConnectorPosition(conRand)] == true)
					{
						conRand = rnd.Range(12, 16);
					}
					correct = ConnectorPosition(conRand);
					_connectorRenderers4[correct].enabled = true;
					_chosenConPos[i] = correct;
					connectorsUsed[correct] = true;
					break;
			}
		}
		Debug.LogFormat("[Wire Ordering #{0}]: The connectors are: {1}, {2}, {3}, {4}.", _modID, _chosenConPos[0] + 1, _chosenConPos[1] + 1, _chosenConPos[2] + 1, _chosenConPos[3] + 1);
	}

	// Generates Order of Cuts
	void GenerateCuts() {
		int rule = ( ((_chosenColorsDis[0]+1) * _chosenDisNum[0]) + ( (_chosenColorsDis[1]+1) * _chosenDisNum[1]) + ( (_chosenColorsDis[2]+1) * _chosenDisNum[2]) + ( (_chosenColorsDis[3]+1) * _chosenDisNum[3]) ) % 10;
		Debug.LogFormat("[Wire Ordering #{0}]: The rule number is: {1}.", _modID, rule);
		switch (rule) {
			case 0:
				for (int i = 0; i <= 3; i++) 
				{
					_chosenCutOrder[i] = i;
				}
				break;
			case 1:
				for (int i = 3; i >= 0; i--)
				{
					_chosenCutOrder[i] = i;
				}
				break;
			case 2:
				for (int i = 0; i <= 3; i++) {
					_chosenCutOrder[i] = Array.IndexOf(_chosenDisNum, i + 1);
				}
				break;
			case 3:
				for (int i = 0; i <= 3; i++) {
					_chosenCutOrder[i] = _chosenConPos[i];
				}
				break;
			case 4:
				for (int i = 0; i <= 3; i++)
				{
					_chosenCutOrder[i] = Array.IndexOf(_chosenDisNum, i+1);
				}
				break;
			case 5:
				for (int i = 0; i <= 3; i++)
				{
					_chosenCutOrder[i] = Array.IndexOf(_chosenColorsWire, _chosenColorsDis[i]);
				}
				break;
			case 6:
				int[] colorAscending = new int[4];
				_chosenColors.CopyTo(colorAscending, 0);
				Array.Sort(colorAscending);
				for (int i = 0; i <= 3; i++) 
				{
					_chosenCutOrder[i] = Array.IndexOf(_chosenColorsWire, colorAscending[i]);
				}
				break;
			case 7:
				colorAscending = new int[4];
				_chosenColors.CopyTo(colorAscending, 0);
				for (int i = 0; i <= 3; i++) 
				{
					_chosenCutOrder[i] = Array.IndexOf(_chosenColorsDis, colorAscending[i]);
				}
				break;
			case 8:
				int[] colorDescending = new int[4];
				_chosenColors.CopyTo(colorDescending, 0);
				Array.Sort(colorDescending); Array.Reverse(colorDescending);
				for (int i = 0; i <= 3; i++) 
				{
					_chosenCutOrder[i] = Array.IndexOf(_chosenColorsWire, colorDescending[i]);
				}
				break;
			case 9:
				colorDescending = new int[4];
				_chosenColors.CopyTo(colorDescending, 0);
				Array.Sort(colorDescending); Array.Reverse(colorDescending);
				for (int i = 0; i <= 3; i++) 
				{
					_chosenCutOrder[i] = Array.IndexOf(_chosenColorsDis, colorDescending[i]);
				}
				break;
		}
		Debug.LogFormat("[Wire Ordering #{0}]: The correct cut order is: {1}, {2}, {3}, {4}", _modID, _chosenCutOrder[0]+1, _chosenCutOrder[1]+1, _chosenCutOrder[2]+1, _chosenCutOrder[3]+1);
	}

	void CheckCuts()
	{
		if (_modSolved)
		{
			return;
		}
		if (_statCutIndex == 4) {
			return;
		}
		if (_cutWires[_chosenCutOrder[_statCutIndex]] == true && _statCutIndex != _cutIndex)
		{
			_statCutIndex++;
		}
	}

	int ConnectorPosition(int conId)
	{
		switch (conId)
		{
			case 0:
			case 1:
			case 2:
			case 3:
				return conId;
			case 4:
			case 5:
			case 6:
			case 7:
				return conId - 4;
			case 8:
			case 9:
			case 10:
			case 11:
				return conId - 8;
			case 12:
			case 13:
			case 14:
			case 15:
				return conId - 12;
		}
		return -1;
	}

	IEnumerator SolveModule() {
		char[] solve = new char[4];
		switch (rnd.Range(0,8)) {
			case 0:
				solve = new char[] { 'C', 'O', 'O', 'L' };
				break;
			case 1:
				solve = new char[] { 'G', 'O', 'O', 'D' };
				break;
			case 2:
				solve = new char[] { 'D', 'O', 'N', 'E' };
				break;
			case 3:
				solve = new char[] { 'N', 'I', 'C', 'E' };
				break;
			case 4:
				solve = new char[] { 'D', 'O', 'P', 'E' };
				break;
			case 5:
				solve = new char[] { 'L', 'E', 'W', 'D' };
				break;
			case 6:
				solve = new char[] { 'E', 'P', 'I', 'C' };
				break;
			case 7:
				solve = new char[] { 'N', 'E', 'R', 'D' };
				break;
			case 8:
				solve = new char[] { 'K', 'A', 'T', 'A' };
				break;
			case 9:
				solve = new char[] { 'Y', 'M', 'C', 'A' };
				break;
		}
		for (int i = 0; i <= 3; i++) {
			_displayRenderers[i].material = _colorMaterials[6];
			_cutRenderers[i].material = _colorMaterials[6];
			_displayTexts[i].text = solve[i].ToString();
			_displayTexts[i].color = new Color(0, 0, 0, 255);
			yield return new WaitForSeconds(0.50f);
		}
		_modSolved = true;
		GetComponent<KMBombModule>().HandlePass();
		yield break;
	}
	// 

	#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} cut 1 2 3 4 [Cut's the wire at the position needed, chain by spaces]";
	#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string command) {
		string[] args = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (args.Length <= 1 || args.Length >= 6) {
			yield return "sendtochaterror That is an incorrect amount of arguments, please try again.";
		}
		if (!args[0].ToLower().Equals("cut")) {
			yield return "sendtochaterror That is an incorrect command, please try again.";
		}
		List<int> wirePos = new List<int>();
		foreach (string s in args) {
			int result;
			if (s.ToLower().Equals("cut")) { continue; }
			if (int.TryParse(s, out result)) {
				wirePos.Add(int.Parse(s));
			} else {
				yield return "sendtochaterror Incorrect string format, please try again.";
			}
		}
		foreach (int i in wirePos) {
			_wires[i-1].OnInteract();
			yield return new WaitForSeconds(0.1f);
		}
		yield break;
	}

	IEnumerator TwitchHandleForcedSolve() {
		yield return null;
		foreach (int i in _chosenCutOrder) {
			_wires[i].OnInteract();
		}
		yield break;
	}
}
