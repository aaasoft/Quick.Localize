using Quick.Localize.Resource;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Quick.Localize
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    public class TextManager
    {
        public static string LanguageFileExtension { get; set; } = ".lang";
        public static string LanguageFolder { get; set; } = "Language";
        public static string LanguagePathInAssembly { get; set; } = "Language";

        private static Dictionary<string, TextManager> textManagerDict = new Dictionary<string, TextManager>();

        /// <summary>
        /// 获取默认的文本管理器实例
        /// </summary>
        public static TextManager DefaultInstance { get { return GetInstance(DefaultLanguage); } }
        /// <summary>
        /// 默认语言
        /// </summary>
        public static string DefaultLanguage { get; set; } = "zh-CN";

        /// <summary>
        /// 获取文本管理器实例
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static TextManager GetInstance(String language)
        {
            lock (textManagerDict)
            {
                if (textManagerDict.ContainsKey(language))
                    return textManagerDict[language];
                var newTextManager = new TextManager(language);
                textManagerDict[language] = newTextManager;
                return newTextManager;
            }
        }

        /// <summary>
        /// 获取语言资源字典
        /// </summary>
        /// <param name="languageContent"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetLanguageResourceDictionary(string languageContent)
        {
            Dictionary<string, string> languageDict = new Dictionary<String, string>();

            //(?'key'.+)\s*=(?'value'.+)
            Regex regex = new Regex(@"(?'key'.+)\s*=(?'value'.+)");
            MatchCollection languageMatchCollection = regex.Matches(languageContent);
            foreach (Match match in languageMatchCollection)
            {
                var indexGroup = match.Groups["key"];
                var valueGroup = match.Groups["value"];

                if (!indexGroup.Success || !valueGroup.Success)
                    continue;
                var key = indexGroup.Value;
                var value = valueGroup.Value;
                if (value.EndsWith("\r"))
                    value = value.Substring(0, value.Length - 1);
                if (languageDict.ContainsKey(key))
                    languageDict.Remove(key);
                languageDict.Add(key, value);
            }
            return languageDict;
        }

        /// <summary>
        /// 获取所有可用的语言资源
        /// </summary>
        /// <returns></returns>
        public static string[] GetLanguages()
        {
            Collection<string> collection = new Collection<string>();

            //搜索语言目录
            DirectoryInfo languageFolderDi = new DirectoryInfo(LanguageFolder);
            if (languageFolderDi.Exists)
            {
                foreach (var languageName in languageFolderDi.GetDirectories().Select(t => t.Name))
                {
                    if (!collection.Contains(languageName))
                        collection.Add(languageName);
                }
            }

            if (!collection.Contains(DefaultLanguage))
                collection.Insert(0, DefaultLanguage);
            return collection.ToArray();
        }

        //类的语言资源字典
        private Dictionary<string, Dictionary<string, string>> typeLanguageResourceDict = new Dictionary<string, Dictionary<String, string>>();
        /// <summary>
        /// 当前TextManager的语言
        /// </summary>
        public string Language { get; private set; }

        private TextManager(string language)
        {
            this.Language = language;
        }

        /// <summary>
        /// 获取语言文字
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetText<T>(T key, params object[] args)
            where T : struct, Enum
        {
            var text = GetText<T>(key.ToString());
            if (args == null || args.Length == 0)
                return text;
            return string.Format(text, args);
        }

        /// <summary>
        /// 获取语言文字(带尾巴，即语言枚举的完整类名与枚举名)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetTextWithTail<T>(T key, params object[] args)
            where T : struct, Enum
        {
            return $"{GetText(key, args)}(LanguageKey: {key.GetType().FullName}.{key})";
        }

        public string GetText<T>(string key)
            where T:struct,Enum
        {
            var type = typeof(T);
            var languageResourceDict = getLanguageResourceDict<T>();
            if (languageResourceDict == null
                || !languageResourceDict.ContainsKey(key))
                return $"Language Resource[Type:{type.FullName}, Key:{key}] not found!";
            return languageResourceDict[key];
        }

        public string GetText(string key, Assembly assembly, string resourcePath)
        {
            var languageResourceDict = getLanguageResourceDict(assembly, resourcePath);
            if (languageResourceDict == null
                || !languageResourceDict.ContainsKey(key))
                return null;
            return languageResourceDict[key];
        }

        private Dictionary<string, string> getLanguageResourceDict<T>()
            where T : struct, Enum
        {
            var type = typeof(T);
            var key = string.Format("{0};{1}", type.Assembly.GetName().Name, type.FullName);
            lock (typeLanguageResourceDict)
            {
                if (typeLanguageResourceDict.ContainsKey(key))
                    return typeLanguageResourceDict[key];

                var languageResourceDict = new Dictionary<string, string>();
                typeLanguageResourceDict.Add(key, languageResourceDict);

                if (type.GetCustomAttributes(typeof(TextResourceAttribute), false).Count() > 0)
                {
                    foreach (var itemMemberInfo in type.GetMembers())
                    {
                        var textAttributes = itemMemberInfo.GetCustomAttributes<TextAttribute>();
                        var textAttribute = textAttributes.FirstOrDefault(attr => attr.Language == Language);
                        if (textAttribute == null)
                            textAttribute = textAttributes.Where(attr => attr.Language == TextAttribute.DEFAULT_LANGUAGE).FirstOrDefault();
                        if (textAttribute == null)
                            textAttribute = textAttributes.FirstOrDefault();
                        if (textAttribute == null)
                            continue;
                        languageResourceDict.Add(itemMemberInfo.Name, textAttribute.Value);
                    }
                }
                fillLanguageResourceDict(type.Assembly, type.FullName, languageResourceDict);
                return languageResourceDict;
            }
        }

        private Dictionary<string, string> getLanguageResourceDict(Assembly assembly, string resourcePath)
        {
            var key = string.Format("{0};{1}", assembly.GetName().Name, resourcePath);
            lock (typeLanguageResourceDict)
            {
                if (typeLanguageResourceDict.ContainsKey(key))
                    return typeLanguageResourceDict[key];
                Dictionary<string, string> languageResourceDict = new Dictionary<string, string>();
                typeLanguageResourceDict.Add(key, languageResourceDict);
                fillLanguageResourceDict(assembly, resourcePath, languageResourceDict);
                return languageResourceDict;
            }
        }

        private void fillLanguageResourceDict(Assembly assembly, String resourcePath, Dictionary<String, String> languageResourceDict)
        {
            //==========================
            //然后尝试从资源文件中读取
            //==========================
            //视图模型接口类所在的程序集名称
            String assemblyName = assembly.GetName().Name;

            //==========================
            //最后搜索语言目录和主题目录下的文件
            //==========================
            //要搜索的可能的语言文件名称
            List<String> languageFileNameList = new List<string>();

            if (resourcePath.StartsWith(assemblyName + "."))
            {
                String shortName = resourcePath.Substring((assemblyName + ".").Length);
                languageFileNameList.Add(shortName + LanguageFileExtension);
            }
            languageFileNameList.Add(resourcePath + LanguageFileExtension);

            //语言目录下的语言文件内容
            String languageBaseFolder = Path.Combine(LanguageFolder, this.Language, assemblyName);
            String languageContent = ResourceUtils.GetResourceText(
                    languageFileNameList,
                    assembly,
                    languageBaseFolder,
                    //路径
                    assemblyName, LanguagePathInAssembly, this.Language, "[fileName]"
                );
            if (languageContent != null)
            {
                var tmpDict = GetLanguageResourceDictionary(languageContent);
                foreach (String key in tmpDict.Keys)
                {
                    if (languageResourceDict.ContainsKey(key))
                        languageResourceDict.Remove(key);
                    languageResourceDict.Add(key, tmpDict[key]);
                }
            }
        }
    }
}