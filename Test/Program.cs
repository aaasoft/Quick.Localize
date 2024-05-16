using Quick.Localize;
using System;
using System.Globalization;
using System.Text;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var textResourceManager = GettextResourceManager.GetResourceManager(typeof(Program).Assembly);

            Console.OutputEncoding = Encoding.UTF8;
            var cultures = new[] { null, CultureInfo.GetCultureInfo("en-US"), CultureInfo.GetCultureInfo("zh-CN") };
            foreach (var culture in cultures)
            {
                var cultureName = "Current 当前";
                if (culture != null)
                    cultureName = $"{culture.EnglishName} - {culture.DisplayName}";
                Console.WriteLine($"--{cultureName}--");
                var catalog = textResourceManager.GetCatalog(culture);
                Console.WriteLine(catalog.GetString("Hello world!"));
                Console.WriteLine(catalog.GetString("Hello C#!"));
                Console.WriteLine(catalog.GetString("Hello dotnet!"));
                Console.WriteLine();
            }
        }
    }
}