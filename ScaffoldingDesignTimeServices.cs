using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore.Design;

namespace robot_controller_api
{
	public class ScaffoldingDesignTimeServices : IDesignTimeServices
	{
		public void ConfigureDesignTimeServices(IServiceCollection services)
		{
			services.AddHandlebarsScaffolding();

			var myHelper = (helperName: "text-to-camelCase", helperFunction: (Action<EncodedTextWriter, Context, Arguments>) ToCamelCaseHelper);

			services.AddHandlebarsHelpers(myHelper);
		}
		void ToCamelCaseHelper(EncodedTextWriter writer, Context context, Arguments parameters)
		{
			if (parameters.Length > 0)
			{
				var text = parameters[0]?.ToString();
				if (!string.IsNullOrEmpty(text))
				{
					var camelCaseText = ToCamelCase(text);
					writer.Write(camelCaseText);
				}
			}
		}

		private static string ToCamelCase(string text)
		{
			if(string.IsNullOrEmpty(text) || text.Length < 2)
			{
				return text;
			}

			return Char.ToLowerInvariant(text[0]) + text.Substring(1);
		}
	}

}
