﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoETL
{
    public class ChoMetadataObjectCache
    {
        public static readonly ChoMetadataObjectCache Default = new ChoMetadataObjectCache();

        private readonly object _padLock = new object();
        private readonly Dictionary<Type, object> _objectCache = new Dictionary<Type, object>();

        public object GetMetadataObject(object @this)
        {
            if (@this == null)
                return @this;

            Type type = @this.GetType();
            if (_objectCache.ContainsKey(type))
                return _objectCache[type] != null ? _objectCache[type] : @this;

            MetadataTypeAttribute attr = type.GetCustomAttribute<MetadataTypeAttribute>();
            if (attr == null || attr.MetadataClassType == null)
                return @this;
            else
            {
                lock (_padLock)
                {
                    if (!_objectCache.ContainsKey(type))
                    {
                        object obj = null;

                        try
                        {
                            obj = ChoActivator.CreateInstance(attr.MetadataClassType);
                        }
                        catch { }

                        _objectCache.Add(type, obj);
                    }

                    return _objectCache[type] != null ? _objectCache[type] : @this;
                }
            }
        }

        public void Add<T>(object metadataObj)
            where T : class
        {
            Add(typeof(T), metadataObj);
        }

        public void Add(Type type, object metadataObj)
        {
            if (type == null || metadataObj == null)
                return;

            lock (_padLock)
            {
                if (_objectCache.ContainsKey(type))
                    _objectCache[type] = metadataObj;
                else
                    _objectCache.Add(type, metadataObj);
            }
        }

        public void Remove<T>()
            where T : class
        {
            Remove(typeof(T));
        }

        public void Remove(Type type)
        {
            if (type == null)
                return;

            lock (_padLock)
            {
                if (_objectCache.ContainsKey(type))
                    _objectCache.Remove(type);
            }
        }

        public static T CreateMetadataObject<T>(Type recordType)
            where T : class
        {
            T callbackRecord = default(T);

            try
            {
                MetadataTypeAttribute attr = recordType.GetCustomAttribute<MetadataTypeAttribute>();
                if (attr == null)
                {
                    if (typeof(T).IsAssignableFrom(recordType))
                        callbackRecord = Activator.CreateInstance(recordType) as T;
                }
                else
                {
                    if (attr.MetadataClassType != null && typeof(T).IsAssignableFrom(attr.MetadataClassType))
                        callbackRecord = Activator.CreateInstance(attr.MetadataClassType) as T;
                }
            }
            catch
            {

            }

            return callbackRecord;
        }
    }
}
