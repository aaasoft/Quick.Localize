using System;
using System.Collections.Concurrent;
using System.Reflection;
using GetText;

namespace Quick.Localize
{
    public class Locale
    {
        private static ConcurrentDictionary<Assembly, GettextResourceManager> gettextResourceManagerDict = new ConcurrentDictionary<Assembly, GettextResourceManager>();
        private static ConcurrentDictionary<Assembly, ICatalog> catalogDict = new ConcurrentDictionary<Assembly, ICatalog>();

        static Locale()
        {
            GettextResourceManager.CurrentCultureChanged += (sender, e) => ClearCache();
        }

        public static void ClearCache()
        {
            catalogDict.Clear();
            gettextResourceManagerDict.Clear();
        }

        private static ICatalog GetCatalog(Assembly assembly)
        {
            return catalogDict.GetOrAdd(assembly, key =>
            {
                var gettextResourceManager = gettextResourceManagerDict
                    .GetOrAdd(key, key2 => GettextResourceManager.GetResourceManager(key2));
                return gettextResourceManager.GetCatalog();
            });
        }

        private static Assembly _ResourceAssembly;
        public static Assembly ResourceAssembly
        {
            get
            {
                if (_ResourceAssembly == null)
                    _ResourceAssembly = Assembly.GetEntryAssembly();
                return _ResourceAssembly;
            }
            set
            {
                _ResourceAssembly = value;
            }
        }

        public static string GetString(string text) => GetCatalog(ResourceAssembly).GetString(text);
        public static string GetString(string text, params object[] args) => GetCatalog(ResourceAssembly).GetString(text, args);
        public static string GetPluralString(string text, string pluralText, long n) => GetCatalog(ResourceAssembly).GetPluralString(text, pluralText, n);
        public static string GetPluralString(string text, string pluralText, long n, params object[] args) => GetCatalog(ResourceAssembly).GetPluralString(text, pluralText, n, args);
        public static string GetParticularString(string context, string text) => GetCatalog(ResourceAssembly).GetParticularString(context, text);
        public static string GetParticularString(string context, string text, params object[] args) => GetCatalog(ResourceAssembly).GetParticularString(context, text, args);
        public static string GetParticularPluralString(string context, string text, string pluralText, long n) => GetCatalog(ResourceAssembly).GetParticularPluralString(context, text, pluralText, n);
        public static string GetParticularPluralString(string context, string text, string pluralText, long n, params object[] args) => GetCatalog(ResourceAssembly).GetParticularPluralString(context, text, pluralText, n, args);
    }
}