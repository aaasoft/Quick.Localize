using System.Collections.Concurrent;
using System.Reflection;
using GetText;

namespace Quick.Localize
{
    internal class InnerResource
    {
        internal static ConcurrentDictionary<Assembly, GettextResourceManager> gettextResourceManagerDict = new ConcurrentDictionary<Assembly, GettextResourceManager>();
        internal static ConcurrentDictionary<Assembly, ICatalog> catalogDict = new ConcurrentDictionary<Assembly, ICatalog>();

        static InnerResource()
        {
            GettextResourceManager.CurrentCultureChanged += (sender, e) => ClearCache();
        }

        public static void ClearCache()
        {
            catalogDict.Clear();
            gettextResourceManagerDict.Clear();
        }
    }

    public class Locale<T>
    {
        private static ICatalog GetCatalog(Assembly assembly)
        {
            return InnerResource.catalogDict.GetOrAdd(assembly, key =>
            {
                var gettextResourceManager = InnerResource.gettextResourceManagerDict
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
                    _ResourceAssembly = typeof(T).Assembly;
                return _ResourceAssembly;
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