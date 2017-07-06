﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;

namespace Quick.Localize.Resource
{
    public class ResourceUtils
    {
        //搜索文件
        internal static String findFilePath(String baseFolder, String fileName)
        {
            while (fileName.StartsWith("/"))
                fileName = fileName.Substring(1);
            fileName = fileName.Replace("/", ".");

            String[] fileNames = new String[2];
            fileNames[0] = Path.Combine(baseFolder, fileName);
            fileNames[1] = Path.Combine(baseFolder, "." + fileName);

            foreach (var fullFileName in fileNames)
                if (File.Exists(fullFileName))
                    return new FileInfo(fullFileName).FullName;

            String[] nameArray = fileName.Split(new Char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < nameArray.Length; i++)
            {
                String folderName = String.Join(".", nameArray, 0, i);
                String fullFolderPath = Path.Combine(baseFolder, folderName);
                if (!Directory.Exists(fullFolderPath))
                    continue;
                String subFileName = String.Join(".", nameArray, i, nameArray.Length - i);

                var fullFileName = findFilePath(fullFolderPath, subFileName);
                if (fullFileName != null)
                    return fullFileName;
            }
            return null;
        }

        /// <summary>
        /// 获取资源
        /// </summary>
        /// <param name="fileNameList"></param>
        /// <param name="assembly"></param>
        /// <param name="baseFolder"></param>
        /// <param name="pathParts"></param>
        /// <returns></returns>
        public static Stream GetResource(List<String> fileNameList, Assembly assembly, String baseFolder, params Object[] pathParts)
        {
            //文件是否存在
            Boolean isFileExists = false;
            String filePath = null;
            foreach (String fileName in fileNameList)
            {
                //先判断全名文件是否存在
                filePath = findFilePath(baseFolder, fileName);
                isFileExists = filePath != null;
                if (isFileExists)
                    break;
            }

            //先尝试从目录加载
            if (isFileExists)
            {
                return File.OpenRead(filePath);
            }
            //然后尝试从程序集资源中加载
            else
            {
                String assemblyName = assembly.GetName().Name;

                //先寻找嵌入的资源
                foreach (String fileName in fileNameList)
                {
                    String resourceName = String.Join(".", pathParts);
                    resourceName = resourceName.Replace("[fileName]", fileName);
                    resourceName = resourceName.Replace("-", "_");
                    if (IsEmbedResourceExist(assembly, resourceName))
                        return assembly.GetManifestResourceStream(resourceName);
                }
            }
            return null;
        }

        //程序集的嵌入的资源字典
        private static Dictionary<Assembly, String[]> assemblyEmbedResourceDict = new Dictionary<Assembly, string[]>();
        //程序集的Resource编译的资源字典
        private static Dictionary<Assembly, Dictionary<String, String>> assemblyResourceResourceDict = new Dictionary<Assembly, Dictionary<String, String>>();

        //确保程序集的资源元数据信息都已加载
        private static void makeSureLoadAssemblyResourceMetaData(Assembly assembly)
        {
            String assemblyName = assembly.GetName().Name;
            lock (typeof(ResourceUtils))
            {
                //嵌入的资源
                if (!assemblyEmbedResourceDict.ContainsKey(assembly))
                {
                    assemblyEmbedResourceDict[assembly] = assembly.GetManifestResourceNames();
                }
            }
        }

        public static Boolean IsEmbedResourceExist(Assembly assembly, String resourceName)
        {
            makeSureLoadAssemblyResourceMetaData(assembly);
            return assemblyEmbedResourceDict[assembly].Contains(resourceName);
        }

        private static String takePathPart(String resourcePath, String separator, Int32 startIndex = -1, Int32 count = 0)
        {
            var resourcePathParts = resourcePath.Split(new String[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            if (startIndex >= resourcePathParts.Length)
                startIndex = resourcePathParts.Length - 1;
            if (count > resourcePathParts.Length)
                count = resourcePathParts.Length;
            if (startIndex < 0)
                startIndex = resourcePathParts.Length - count - 1;
            if (count <= 0)
                count = resourcePathParts.Length - startIndex;

            if (startIndex < 0 && count <= 0)
                return resourcePath;

            return String.Join(separator, resourcePathParts, startIndex, count);
        }

        public static String GetResourceText(List<String> fileNameList, Assembly assembly, String baseFolder, params Object[] pathParts)
        {
            Stream resourceStream = GetResource(fileNameList, assembly, baseFolder, pathParts);
            if (resourceStream == null)
                return null;

            using (resourceStream)
            {
                String fileContent = null;
                StreamReader streamReader = new StreamReader(resourceStream);
                fileContent = streamReader.ReadToEnd();
                streamReader.Dispose();
                resourceStream.Dispose();
                return fileContent;
            }
        }
    }
}
