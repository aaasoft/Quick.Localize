# Quick.Localize [![NuGet Downloads](https://img.shields.io/nuget/dt/Quick.Localize.svg)](https://www.nuget.org/packages/Quick.Localize/)

* Quick.Localize is a i18n library.

Example
```
using Quick.Localize;
using System;
using System.Globalization;
using System.Reflection;
using System.Text;

var textResourceManager = GettextResourceManager.GetResourceManager(Assembly.GetExecutingAssembly());

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
```
