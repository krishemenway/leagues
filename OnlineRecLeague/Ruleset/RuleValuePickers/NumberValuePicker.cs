﻿using OnlineRecLeague.CommonDataTypes;

namespace OnlineRecLeague.Ruleset.RuleValuePickers
{
	public class NumberValuePicker : IRuleValuePicker
	{
		public NumberValuePicker(int defaultValue, Range<int> allowedRange = null)
		{
			DefaultValue = defaultValue.ToString();
			AllowedRange = allowedRange;
		}

		public string ValuePickerType => nameof(NumberValuePicker);

		public string DefaultValue { get; private set; }
		public Range<int> AllowedRange { get; private set; }

		public bool IsValid(string value)
		{
			throw new System.NotImplementedException();
		}
	}
}
