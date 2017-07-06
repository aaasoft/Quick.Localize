using Quick.Localize;
using System;
using System.Text;

namespace Test
{
    [TextResource]
    public enum TextFromCode
    {
        [Text("Hello World!", Language = "en-US")]
        [Text("你好世界!", Language = "zh-CN")]
        HelloWorld
    }

    [TextResource]
    public enum TextFromEmbedResource
    {
        HelloCSharp
    }

    [TextResource]
    public enum TextFromExternalFile
    {
        HelloDotNet
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("--English 英文--");
            var textManager = TextManager.GetInstance("en-US");
            Console.WriteLine(textManager.GetText(TextFromCode.HelloWorld));
            Console.WriteLine(textManager.GetText(TextFromEmbedResource.HelloCSharp));
            Console.WriteLine(textManager.GetText(TextFromExternalFile.HelloDotNet));

            Console.WriteLine();
            Console.WriteLine("--Chinese 中文--");
            textManager = TextManager.GetInstance("zh-CN");
            Console.WriteLine(textManager.GetText(TextFromCode.HelloWorld));
            Console.WriteLine(textManager.GetText(TextFromEmbedResource.HelloCSharp));
            Console.WriteLine(textManager.GetText(TextFromExternalFile.HelloDotNet));

            Console.ReadLine();
        }
    }
}