namespace BGLineUnwrapper
{
	using System;
	using System.IO;

	internal static partial class Program
	{
		#region Public Methods
		public static void Main(string[] args)
		{
			var option = args.Length > 0 ? args[0] : "BG2";
			BGDom dom = option switch
			{
				"BG1" => BG1Dom.FromFile(@"C:\Users\rmorl\Documents\Games\Baldur's Gate\BG Walkthrough.txt"),
				"BG2" => BG2Dom.FromFile(@"C:\Users\rmorl\Documents\Games\Baldur's Gate 2\Walkthrough.txt"),
				_ => throw new InvalidOperationException()
			};

			var doWord = true;
			using var writer = File.CreateText(@"D:\Output." + (doWord ? "xml" : "htm"));
			Saver saver = doWord
				? new SaveAsWordML(writer)
				: new SaveAsHtml(writer);
			saver.Save(dom);
		}
		#endregion
	}
}
