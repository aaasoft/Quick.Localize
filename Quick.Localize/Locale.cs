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

        public static string GetString(string text) => GetCatalog(Assembly.GetCallingAssembly()).GetString(text);
        public static string GetString(string text, params object[] args) => GetCatalog(Assembly.GetCallingAssembly()).GetString(text, args);
        public static string GetPluralString(string text, string pluralText, long n) => GetCatalog(Assembly.GetCallingAssembly()).GetPluralString(text, pluralText, n);
        public static string GetPluralString(string text, string pluralText, long n, params object[] args) => GetCatalog(Assembly.GetCallingAssembly()).GetPluralString(text, pluralText, n, args);
        public static string GetParticularString(string context, string text) => GetCatalog(Assembly.GetCallingAssembly()).GetParticularString(context, text);
        public static string GetParticularString(string context, string text, params object[] args) => GetCatalog(Assembly.GetCallingAssembly()).GetParticularString(context, text, args);
        public static string GetParticularPluralString(string context, string text, string pluralText, long n) => GetCatalog(Assembly.GetCallingAssembly()).GetParticularPluralString(context, text, pluralText, n);
        public static string GetParticularPluralString(string context, string text, string pluralText, long n, params object[] args) => GetCatalog(Assembly.GetCallingAssembly()).GetParticularPluralString(context, text, pluralText, n, args);
    }
}