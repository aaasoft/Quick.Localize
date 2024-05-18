using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using GetText;
using Quick.Localize.Resource;

namespace Quick.Localize
{
    public class GettextResourceManager
    {
        public const string MO_FILE_EXTENSION = "mo";
        public const string LOCALE_PATH = "locale";

        public static event EventHandler<CultureInfo> CurrentCultureChanged;
        public static CultureInfo CurrentCulture { get; private set; } = CultureInfo.CurrentCulture;

        public static void ChangeCurrentCulture(CultureInfo culture)
        {
            lock (resourceManagerDict)
            {
                resourceManagerDict.Clear();
                CurrentCulture = culture;
                CurrentCultureChanged.Invoke(typeof(GettextResourceManager), culture);
            }
        }

        private static Dictionary<Assembly, GettextResourceManager> resourceManagerDict = new Dictionary<Assembly, GettextResourceManager>();
        public static GettextResourceManager GetResourceManager(Assembly baseAssembly)
        {
            lock (resourceManagerDict)
            {
                if (!resourceManagerDict.TryGetValue(baseAssembly, out var manager))
                    resourceManagerDict[baseAssembly] = manager = new GettextResourceManager(baseAssembly);
                return manager;
            }
        }

        private Assembly baseAssembly;
        private string baseAssemblyName;
        private Dictionary<CultureInfo, Catalog> catalogDict = new Dictionary<CultureInfo, Catalog>();

        public GettextResourceManager(Assembly baseAssembly)
        {
            this.baseAssembly = baseAssembly;
            baseAssemblyName = baseAssembly.GetName().Name;
        }

        private Stream getResourceStream(CultureInfo culture)
        {
            var localeTags = new[] { culture.IetfLanguageTag.Replace("-", "_"), culture.TwoLetterISOLanguageName };
            foreach (var localeTag in localeTags)
            {
                var stream = ResourceUtils.GetResourceFromFile(Path.Combine(LOCALE_PATH, localeTag, $"{baseAssemblyName}.{MO_FILE_EXTENSION}"));
                if (stream != null)
                    return stream;
                stream = ResourceUtils.GetResourceFromAssembly(baseAssembly, baseAssemblyName, LOCALE_PATH, localeTag, MO_FILE_EXTENSION);
                if (stream != null)
                    return stream;
            }
            return null;
        }

        public ICatalog GetCatalog()
        {
            return GetCatalog(CurrentCulture);
        }

        public ICatalog GetCatalog(CultureInfo culture)
        {
            lock (catalogDict)
            {
                if (!catalogDict.TryGetValue(culture, out var catalog))
                {
                    var resourceStream = getResourceStream(culture);
                    if (resourceStream != null)
                        using (resourceStream)
                            catalog = new Catalog(resourceStream, culture);
                    if (catalog == null)
                        catalog = new Catalog();
                    catalogDict[culture] = catalog;
                }
                return catalog;
            }
        }
    }
}