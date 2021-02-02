﻿using OpenRPA.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpenRPA
{
    public class Config : AppSettings<Config>
    {
        public string wsurl = "";
        public string username = "";
        public byte[] jwt;
        public byte[] password;
        public byte[] entropy;
        public string cancelkey = "{ESCAPE}";
        public bool isagent = false;
        public bool showloadingscreen = true;
        public string culture = "en";
        public string ocrlanguage = "eng";
        public string[] openworkflows = new string[] { };
        public System.Drawing.Rectangle mainwindow_position = System.Drawing.Rectangle.Empty;
        public string designerlayout = "";
        public Dictionary<string, object> properties = new Dictionary<string, object>();
        public bool record_overlay = false;
        public int querypagesize = 50;
        public DateTime lastupdatecheck = DateTime.Now;
        public TimeSpan updatecheckinterval = TimeSpan.FromDays(1);
        public bool doupdatecheck = true;
        public bool autoupdateupdater = true;
        public bool log_to_file = true;
        public int log_file_level_minimum = NLog.LogLevel.Info.Ordinal;
        public int log_file_level_maximum = NLog.LogLevel.Fatal.Ordinal;
        public bool log_verbose = false;
        public bool log_activity = false;
        public bool log_debug = false;
        public bool log_selector = false;
        public bool log_selector_verbose = false;
        public bool log_information = true;
        public bool log_output = true;
        public bool log_warning = true;
        public bool log_error = true;
        public bool use_sendkeys = false;
        public bool use_virtual_click = true;
        public bool use_animate_mouse = false;
        public TimeSpan use_postwait = TimeSpan.Zero;
        public bool minimize = true;
        public bool recording_add_to_designer = true;
        public TimeSpan reloadinterval = TimeSpan.FromMinutes(5);
        public TimeSpan move_animation_run_time = TimeSpan.FromMilliseconds(500);
        public int move_animation_steps = 20;
        public bool remote_allow_multiple_running = false;
        public int remote_allow_multiple_running_max = 2;
        public string cef_useragent = "";
        public bool cef_allow_unsigned_certificates = false;
        public bool show_getting_started = true;
        // public bool notify_on_workflow_start = false;
        public bool notify_on_workflow_remote_start = true;
        public bool notify_on_workflow_end = true;
        public bool notify_on_workflow_remote_end = false;
        public bool log_busy_warning = true;
        private void loadEntropy()
        {
            if (entropy == null || entropy.Length == 0)
            {
                entropy = new byte[20];
                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(entropy);
                }
            }
        }
        public byte[] ProtectString(string data)
        {
            loadEntropy();
            // Data to protect.
            byte[] plaintext = Encoding.UTF8.GetBytes(data);

            // Generate additional entropy (will be used as the Initialization vector)
            byte[] ciphertext = ProtectedData.Protect(plaintext, entropy, DataProtectionScope.CurrentUser);
            return ciphertext;
        }
        public SecureString UnprotectString(byte[] data)
        {
            loadEntropy();
            // Data to protect.
            // byte[] plaintext = Encoding.UTF8.GetBytes(data);

            SecureString SecureData = new SecureString();
            byte[] ciphertext = ProtectedData.Unprotect(data, entropy, DataProtectionScope.CurrentUser);
            foreach (var c in Encoding.Default.GetString(ciphertext))
            {
                SecureData.AppendChar(c);
            }
            return SecureData;
        }
        private static Config _local = null;
        public static Config local
        {
            get
            {
                if (_local == null)
                {
                    string filename = "settings.json";
                    var fi = new System.IO.FileInfo(filename);
                    var _fileName = System.IO.Path.GetFileName(filename);
                    var di = fi.Directory;
                    if (System.IO.File.Exists(System.IO.Path.Combine(Extensions.ProjectsDirectory, "settings.json")))
                    {
                            filename = System.IO.Path.Combine(Extensions.ProjectsDirectory, "settings.json");
                    }
                    else if (System.IO.File.Exists(filename))
                    {
                    }
                    else if (System.IO.File.Exists(System.IO.Path.Combine(di.Parent.FullName, "settings.json")))
                    {
                        filename = System.IO.Path.Combine(di.Parent.FullName, "settings.json");
                    }
                    else
                    {
                        // Will create a new file in ProjectsDirectory
                        filename = System.IO.Path.Combine(Extensions.ProjectsDirectory, "settings.json");
                    }
                    _local = Load(filename);
                     
                }
                return _local;
            }
        }
        public static void Save()
        {
            try
            {
                local.Save(System.IO.Path.Combine(Extensions.ProjectsDirectory, "settings.json"));
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
        public static void Reload()
        {
            _local = null;
        }
        public T GetProperty<T>(string pluginname, T mydefault, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            try
            {
                if (propertyName == null)
                {
                    throw new ArgumentNullException(nameof(propertyName));
                }
                object value;
                if (properties.TryGetValue(pluginname + "_" + propertyName, out value))
                {
                    if (typeof(T) == typeof(int) && value is long) value = int.Parse(value.ToString());
                    if (typeof(T) == typeof(TimeSpan) && value != null)
                    {
                        TimeSpan ts = TimeSpan.Zero;
                        if (TimeSpan.TryParse(value.ToString(), out ts))
                        {
                            return (T)(object)ts;
                        }
                    }
                    if (typeof(T) == typeof(string[]) && value != null)
                    {
                        object o = null;
                        if (value.GetType() == typeof(string[])) o = value;
                        if (value.GetType() == typeof(Newtonsoft.Json.Linq.JArray)) o = ((Newtonsoft.Json.Linq.JArray)value).ToObject<string[]>();
                        return (T)o;
                    }
                    return (T)value;
                }
                SetProperty(pluginname, mydefault, propertyName);
                return mydefault;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }
        public bool SetProperty<T>(string pluginname, T newValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            try
            {
                if (propertyName == null)
                {
                    throw new ArgumentNullException(nameof(propertyName));
                }
                // if (IsEqual(GetProperty<T>(pluginname + "_" + propertyName, default(T)), newValue)) return false;
                properties[pluginname + "_" + propertyName] = newValue;
                Type typeParameterType = typeof(T);
                if (typeParameterType.Name.ToLower().Contains("readonly"))
                {
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }
        private bool IsEqual<T>(T field, T newValue)
        {
            // Alternative: EqualityComparer<T>.Default.Equals(field, newValue);
            return Equals(field, newValue);
        }
        private string GetNameFromExpression<T>(Expression<Func<T>> selectorExpression)
        {
            var body = (MemberExpression)selectorExpression.Body;
            var propertyName = body.Member.Name;
            return propertyName;
        }
    }
}

