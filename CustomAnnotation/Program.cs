namespace CustomAnnotation
{
	using System;
	using System.Linq;
	using System.Reflection;

	class Program
	{
		static void Main(string[] args)
		{
			var x = "123.45";
			var y = (x.Substring(x.Length - 3)).ToArray()[0];
			var m = new Demo { Name = "Rahul", Quantity = "125", Amount = "123.45" };
			var validation = m.Validate();
			Console.ReadKey();
		}
	}

	class Demo
	{
		[StringLength(5,10,"character limit 5 to 10")]
		public string Name { get; set; }

		[Numeric("not a valid number")]
		public string Quantity { get; set; }

		[Currency("not a valid amount")]
		public string Amount { get; set; }

		[NotAllow(",.?", "character not allowed")]
		public string Test { get; set; } = "Rahul Karmakar";

		[EqualOf("Test", "must be same")]
		public string Best { get; set; } = "Rahul Karmakar";
	}

	static class Validator
	{
		public static string Validate(this object model)
		{
			var returnval = string.Empty;
			try
			{
				foreach (var prop in model.GetType().GetProperties().ToList())
				{
					if(Attributes.HasAttribute<StringLengthAttribute>(prop))
					{
						var attr = Attributes.GetAttribute<StringLengthAttribute>(prop);
						var val = prop.GetValue(model, null).ToString();
						if (val.Length < attr.Min || val.Length > attr.Max)
						{
							returnval = attr.ErrorMessage;
							break;
						}
					}
					else if (Attributes.HasAttribute<NumericAttribute>(prop))
					{
						var attr = Attributes.GetAttribute<NumericAttribute>(prop);
						var val = prop.GetValue(model, null).ToString();
						if (!val.All(char.IsDigit))
						{
							returnval = attr.ErrorMessage;
							break;
						}
					}
					else if (Attributes.HasAttribute<CurrencyAttribute>(prop))
					{
						var isbreak = false;
						var attr = Attributes.GetAttribute<CurrencyAttribute>(prop);
						var val = prop.GetValue(model, null).ToString();
						if ((val.Substring(val.Length - 3)).ToArray()[0] != '.')
						{
							returnval = attr.ErrorMessage;
							break;
						}
						else
						{
							foreach (var c in val.ToArray())
							{
								if (!char.IsDigit(c) && c != '.')
								{
									returnval = attr.ErrorMessage;
									isbreak = true;
									break;
								}
							}
						}
						if (isbreak)
							break;
					}
					else if (Attributes.HasAttribute<NotAllowAttribute>(prop))
					{
						var isbreak = false;
						var attr = Attributes.GetAttribute<NotAllowAttribute>(prop);
						var val = prop.GetValue(model, null).ToString();
						foreach (var c in val.ToArray())
						{
							if(attr.Chars.Any(o => o == c))
							{
								returnval = attr.ErrorMessage;
								isbreak = true;
								break;
							}
						}
						if (isbreak)
							break;
					}
					else if (Attributes.HasAttribute<EqualOfAttribute>(prop))
					{
						var attr = Attributes.GetAttribute<EqualOfAttribute>(prop);
						var val = prop.GetValue(model, null).ToString();
						var compwith = model.GetType().GetProperty(attr.CompareTo).GetValue(model, null).ToString();
						if (val != compwith)
						{
							returnval = attr.ErrorMessage;
							break;
						}
					}
				}
			}
			catch(Exception e)
			{
				returnval = e.GetBaseException().Message;
			}
			return returnval;
		}
	}

	sealed class StringLengthAttribute : Attribute
	{
		public StringLengthAttribute(int min, int max, string errorMessage)
		{
			Min = min;
			Max = max;
			ErrorMessage = errorMessage;
		}

		public int Min { get; }
		public int Max { get; }
		public string ErrorMessage { get; }
	}

	sealed class NumericAttribute : Attribute
	{
		public NumericAttribute(string errorMessage)
		{
			ErrorMessage = errorMessage;
		}
		public string ErrorMessage { get; }
	}

	sealed class CurrencyAttribute : Attribute
	{
		public CurrencyAttribute(string errorMessage)
		{
			ErrorMessage = errorMessage;
		}
		public string ErrorMessage { get; }
	}

	sealed class NotAllowAttribute : Attribute
	{
		public NotAllowAttribute(string chars, string errorMessage)
		{
			ErrorMessage = errorMessage;
			Chars = chars.ToCharArray();
		}

		public char[] Chars { get; }
		public string ErrorMessage { get; }
	}

	sealed class EqualOfAttribute : Attribute
	{
		public EqualOfAttribute(string compareTo, string errorMessage)
		{
			ErrorMessage = errorMessage;
			CompareTo = compareTo;
		}

		public string CompareTo { get; }
		public string ErrorMessage { get; }
	}

	static class Attributes
	{
		public static T GetAttribute<T>(Type fromType) where T : Attribute
		{
			var attributes = fromType.GetCustomAttributes(typeof(T), false);
			return GetAttribute<T>(attributes);
		}

		public static T GetAttribute<T>(PropertyInfo propertyInfo) where T : Attribute
		{
			var attributes = propertyInfo.GetCustomAttributes(typeof(T), false);
			return GetAttribute<T>(attributes);
		}

		public static bool HasAttribute<T>(this PropertyInfo propertyInfo) where T : Attribute
		{
			var attr = GetAttribute<T>(propertyInfo);
			return attr != null;
		}

		private static T GetAttribute<T>(object[] attributes) where T : Attribute
		{
			if (!attributes.Any())
				return null;
			var attribute = (T)attributes.First();
			return attribute;
		}
	}
}
