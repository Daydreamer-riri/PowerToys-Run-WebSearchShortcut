namespace Community.PowerToys.Run.Plugin.WebSearchShortcut.Properties
{
    using System;

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute(
        "System.Resources.Tools.StronglyTypedResourceBuilder",
        "17.0.0.0"
    )]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources
    {
        private static global::System.Resources.ResourceManager resourceMan;

        private static global::System.Globalization.CultureInfo resourceCulture;

        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode"
        )]
        internal Resources() { }

        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(
            global::System.ComponentModel.EditorBrowsableState.Advanced
        )]
        public static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    global::System.Resources.ResourceManager temp =
                        new global::System.Resources.ResourceManager(
                            "Community.PowerToys.Run.Plugin.WebSearchShortcut.Properties.Resources",
                            typeof(Resources).Assembly
                        );
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(
            global::System.ComponentModel.EditorBrowsableState.Advanced
        )]
        public static global::System.Globalization.CultureInfo Culture
        {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }

        /// <summary>
        ///   Looks up a localized string similar to Get search results from Everything.
        /// </summary>
        public static string plugin_description
        {
            get { return ResourceManager.GetString("plugin_description", resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Everything.
        /// </summary>
        public static string plugin_name
        {
            get { return ResourceManager.GetString("plugin_name", resourceCulture); }
        }

        public static string settings_storage_file_name
        {
            get { return ResourceManager.GetString("settings_storage_file_name", resourceCulture); }
        }

        /// <summary>
        ///   open
        /// </summary>
        public static string open
        {
            get { return ResourceManager.GetString("open", resourceCulture); }
        }

        public static string open_in_explorer
        {
            get { return ResourceManager.GetString("open_in_explorer", resourceCulture); }
        }

        public static string reload_title
        {
            get { return ResourceManager.GetString("reload_title", resourceCulture); }
        }

        public static string reload_sub_title
        {
            get { return ResourceManager.GetString("reload_sub_title", resourceCulture); }
        }

        public static string config_title
        {
            get { return ResourceManager.GetString("config_title", resourceCulture); }
        }

        public static string config_sub_title
        {
            get { return ResourceManager.GetString("config_sub_title", resourceCulture); }
        }

        public static string error_title
        {
            get { return ResourceManager.GetString("error_title", resourceCulture); }
        }

        public static string select_subtitle
        {
            get { return ResourceManager.GetString("select_subtitle", resourceCulture); }
        }

        public static string search_subtitle
        {
            get { return ResourceManager.GetString("search_subtitle", resourceCulture); }
        }

        public static string search_for
        {
            get { return ResourceManager.GetString("search_for", resourceCulture); }
        }
    }
}
