using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using System.Linq;

public class ProfanityFilter {

	private List<string> _badList;
	private string _regexPattern;
	private string _regexReplacePattern;
	private string _regexWordSplitPattern;
	private string _placeholder;

	public ProfanityFilter () {
		_badList = ProfanityLists.langWords.ToList ();
		_badList.AddRange(ProfanityLists.badWords.ToList());

		_regexPattern = @"[^a-zA-z0-9|\$|\@]|\^";
		_regexReplacePattern = @"\w";
		_regexWordSplitPattern = @"\b";
		_placeholder = "*";
	}

	public string Clean (string input) {
		string[] words = Regex.Split (input, _regexWordSplitPattern);
		return String.Join("", words.Select (word => {
			return IsProfane(word) ? ReplaceWord(word) : word;
		}).ToArray ());
	}

	public bool IsClean (string input) {
		string[] words = Regex.Split (input, _regexWordSplitPattern);
		foreach (var word in words) {
			if (IsProfane (word)) {
				return false;
			}
		}
		return true;
	}

	private bool IsProfane (string input) {
		var words = input.Split (' ');
		for (int i=0; i<words.Length; i++) {
			var word = Regex.Replace(words[i].ToLower(), _regexPattern, "");
			foreach (var badword in _badList) {
				if (word.Contains (badword)) {
					return true;
				}
			}
		}
		return false;
	}

	private string ReplaceWord (string input) {
		return Regex.Replace (Regex.Replace (input, _regexPattern, ""), _regexReplacePattern, _placeholder);
	}
}
