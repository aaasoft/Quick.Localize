using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Quick.Localize.Resource
{
    internal class ResourceUtils
    {
        /// <summary>
        /// 从文件中获取资源
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Stream GetResourceFromFile(string filePath)
        {
            if (File.Exists(filePath))
                return File.OpenRead(filePath);
            return null;
        }

        /// <summary>
        /// 从程序集中获取资源
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="pathParts"></param>
        /// <returns></returns>
        public static Stream GetResourceFromAssembly(Assembly assembly, params string[] pathParts)
        {
            //寻找嵌入的资源
            var resourceName = String.Join(".", pathParts);
            resourceName = resourceName.Replace("-", "_");
            if (assembly.GetManifestResourceNames().Contains(resourceName))
                return assembly.GetManifestResourceStream(resourceName);
            return null;
        }
    }
}
