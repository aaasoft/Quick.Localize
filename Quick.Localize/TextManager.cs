﻿using Quick.Localize.Resource;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Quick.Localize
{
    public class TextManager
    {
        public static String LanguageFileExtension { get; set; } = ".lang";
        public static String LanguageFolder { get; set; } = "Language";
        public static String LanguagePathInAssembly { get; set; } = "Language";

        private static Dictionary<String, TextManager> textManagerDict = new Dictionary<string, TextManager>();

        /// <summary>
        /// 获取默认的文本管理器实例
        /// </summary>
        public static TextManager DefaultInstance { get { return GetInstance(DefaultLanguage); } }
        /// <summary>
        /// 默认语言
        /// </summary>
        public static String DefaultLanguage { get; set; } = "zh-CN";

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
        public static Dictionary<String, String> GetLanguageResourceDictionary(String languageContent)
        {
            Dictionary<String, String> languageDict = new Dictionary<String, string>();

            //(?'key'.+)\s*=(?'value'.+)
            Regex regex = new Regex(@"(?'key'.+)\s*=(?'value'.+)");
            MatchCollection languageMatchCollection = regex.Matches(languageContent);
            foreach (Match match in languageMatchCollection)
            {
                var indexGroup = match.Groups["key"];
                var valueGroup = match.Groups["value"];

                if (!indexGroup.Success || !valueGroup.Success)
                    continue;
                String key = indexGroup.Value;
                String value = valueGroup.Value;
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
            Collection<String> collection = new Collection<string>();

            //搜索语言目录
            DirectoryInfo languageFolderDi = new DirectoryInfo(LanguageFolder);
            if (languageFolderDi.Exists)
            {
                foreach (String languageName in languageFolderDi.GetDirectories().Select(t => t.Name))
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
        private Dictionary<String, Dictionary<String, String>> typeLanguageResourceDict = new Dictionary<String, Dictionary<String, string>>();
        /// <summary>
        /// 当前TextManager的语言
        /// </summary>
        public String Language { get; private set; }

        private TextManager(String language)
        {
            this.Language = language;
        }

        /// <summary>
        /// 获取语言文字
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public String GetText(Enum key, params Object[] args)
        {
            var text = GetText(key.ToString(), key.GetType());
            if (args == null || args.Length == 0)
                return text;
            return String.Format(text, args);
        }

        /// <summary>
        /// 获取语言文字(带尾巴，即语言枚举的完整类名与枚举名)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public String GetTextWithTail(Enum key, params Object[] args)
        {
            return $"{GetText(key, args)}(LanguageKey: {key.GetType().FullName}.{key.ToString()})";
        }

        public String GetText(String key, Type type)
        {
            Dictionary<String, String> languageResourceDict = getLanguageResourceDict(type);
            if (languageResourceDict == null
                || !languageResourceDict.ContainsKey(key))
                return $"Language Resource[Type:{type.FullName}, Key:{key}] not found!";
            return languageResourceDict[key];
        }

        public String GetText(String key, Assembly assembly, String resourcePath)
        {
            var languageResourceDict = getLanguageResourceDict(assembly, resourcePath);
            if (languageResourceDict == null
                || !languageResourceDict.ContainsKey(key))
                return null;
            return languageResourceDict[key];
        }

        private Dictionary<String, String> getLanguageResourceDict(Type type)
        {
            var typeInfo = type.GetTypeInfo();

            String key = String.Format("{0};{1}", typeInfo.Assembly.GetName().Name, type.FullName);
            lock (typeLanguageResourceDict)
            {
                if (typeLanguageResourceDict.ContainsKey(key))
                    return typeLanguageResourceDict[key];

                Dictionary<String, String> languageResourceDict = new Dictionary<String, string>();
                typeLanguageResourceDict.Add(key, languageResourceDict);

                if (typeInfo.IsEnum && typeInfo.GetCustomAttributes(typeof(TextResourceAttribute), false).Count() > 0)
                {
                    foreach (Enum item in Enum.GetValues(type))
                    {
                        MemberInfo itemMemberInfo = typeInfo.GetMember(item.ToString())[0];
                        var textAttributes = itemMemberInfo.GetCustomAttributes(typeof(TextAttribute), false).Cast<TextAttribute>();
                        TextAttribute textAttribute = textAttributes.Where(attr => attr.Language == Language).FirstOrDefault();
                        if (textAttribute == null)
                            textAttribute = textAttributes.Where(attr => attr.Language == TextAttribute.DEFAULT_LANGUAGE).FirstOrDefault();
                        if (textAttribute == null)
                            textAttribute = textAttributes.FirstOrDefault();
                        if (textAttribute == null)
                            continue;
                        languageResourceDict.Add(item.ToString(), textAttribute.Value);
                    }
                }
                fillLanguageResourceDict(typeInfo.Assembly, type.FullName, languageResourceDict);
                return languageResourceDict;
            }
        }

        private Dictionary<String, String> getLanguageResourceDict(Assembly assembly, String resourcePath)
        {
            String key = String.Format("{0};{1}", assembly.GetName().Name, resourcePath);
            lock (typeLanguageResourceDict)
            {
                if (typeLanguageResourceDict.ContainsKey(key))
                    return typeLanguageResourceDict[key];
                Dictionary<String, String> languageResourceDict = new Dictionary<String, String>();
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