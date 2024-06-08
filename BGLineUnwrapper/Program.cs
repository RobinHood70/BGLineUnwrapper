namespace BGLineUnwrapper
{
	using System;
	using System.IO;

	internal static class Program
	{
		#region Static Fields
		private static readonly bool DoWord = true;
		#endregion

		#region Public Methods
		public static void Main(string[] args)
		{
			var option = args.Length > 0 ? args[0] : "BG2";
			var fileName = option switch
			{
				"BG1" => @"C:\Users\rmorl\Documents\Games\Baldur's Gate\BG Walkthrough.txt",
				"BG2" => @"C:\Users\rmorl\Documents\Games\Baldur's Gate 2\Walkthrough.txt",
				_ => throw new InvalidOperationException()
			};

			BGDom dom = option switch
			{
				"BG1" => BG1Dom.FromFile(fileName),
				"BG2" => BG2Dom.FromFile(fileName),
				_ => throw new InvalidOperationException()
			};

			var directory = Path.GetDirectoryName(fileName) ?? throw new InvalidOperationException();
			fileName = Path.Combine(directory, @"Output." + (DoWord ? "xml" : "htm"));
			using var writer = File.CreateText(fileName);
			Saver saver = DoWord
				? new SaveAsWordML(writer)
				: new SaveAsHtml(writer);
			saver.Save(dom);
		}
		#endregion
	}
}
