﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Prowl.Runtime.Serialization
{
    public static class TagSerializer
    {
        public class SerializationContext
        {
            public Dictionary<object, int> objectToId = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);
            public Dictionary<int, object> idToObject = new Dictionary<int, object>();
            public int nextId = 1;
            public List<Guid> dependencies = new List<Guid>();

            public SerializationContext()
            {
                objectToId.Clear();
                objectToId.Add(new NullKey(), 0);
                idToObject.Clear();
                idToObject.Add(0, new NullKey());
                nextId = 1;
                dependencies.Clear();
            }

        }

        private class NullKey { }

        private static bool IsPrimitive(Type t) => t.IsPrimitive || t.IsAssignableTo(typeof(string)) || t.IsAssignableTo(typeof(Guid)) || t.IsAssignableTo(typeof(DateTime)) || t.IsEnum || t.IsAssignableTo(typeof(byte[]));


        #region Serialize

        public static Tag Serialize(object? value, string tagName = "") => Serialize(value, tagName, new());
        public static Tag Serialize(object? value, string tagName, SerializationContext ctx)
        {
            if (value == null)
                return new NullTag(tagName);

            if (value is Tag t)
            {
                var clone = t.Clone();
                clone.Name = tagName;
                return clone;
            }

            var type = value.GetType();
            if (IsPrimitive(type))
                return PrimitiveToTag(tagName, value);

            if (type.IsArray && value is Array array)
                return ArrayToListTag(tagName, array, ctx);

            if (value is IDictionary dict)
                return DictionaryToTag(tagName, dict, ctx);

            return SerializeObject(value, tagName, ctx);
        }

        private static Tag PrimitiveToTag(string tagName, object p)
        {
            if (p is byte b)             return new ByteTag(tagName, b);
            else if (p is sbyte sb)      return new ByteTag(tagName, (byte)sb);
            else if (p is byte[] bArr)   return new ByteArrayTag(tagName, bArr);
            else if (p is bool bo)       return new ByteTag(tagName, (byte)(bo ? 1 : 0));
            else if (p is float f)       return new FloatTag(tagName, f);
            else if (p is double d)      return new DoubleTag(tagName, d);
            else if (p is int i)         return new IntTag(tagName, i);
            else if (p is uint ui)       return new IntTag(tagName, (int)ui);
            else if (p is long l)        return new LongTag(tagName, l);
            else if (p is ulong ul)      return new LongTag(tagName, (long)ul);
            else if (p is short s)       return new ShortTag(tagName, s);
            else if (p is ushort us)     return new ShortTag(tagName, (short)us);
            else if (p is string str)    return new StringTag(tagName, str);
            else if (p is DateTime date) return new LongTag(tagName, date.ToBinary());
            else if (p is Guid g)        return new StringTag(tagName, g.ToString());
            else if (p.GetType().IsEnum) return new IntTag(tagName, (int)p); // Serialize enums as integers
            else throw new NotSupportedException("The type '" + p.GetType() + "' is not a supported primitive.");
        }

        private static ListTag ArrayToListTag(string tagName, Array array, SerializationContext ctx)
        {
            var elementType = array.GetType().GetElementType();
            var listType = TagType.Compound;
            if (elementType == typeof(byte))
                listType = TagType.Byte;
            else if (elementType == typeof(bool))
                listType = TagType.Byte;
            else if (elementType == typeof(double))
                listType = TagType.Double;
            else if (elementType == typeof(float))
                listType = TagType.Float;
            else if (elementType == typeof(int))
                listType = TagType.Int;
            else if (elementType == typeof(long))
                listType = TagType.Long;
            else if (elementType == typeof(short))
                listType = TagType.Short;
            else if (elementType == typeof(string))
                listType = TagType.String;
            else if (elementType == typeof(byte[]))
                listType = TagType.ByteArray;
            List<Tag> tags = new();
            for (int i = 0; i < array.Length; i++)
                tags.Add(Serialize(array.GetValue(i), "", ctx));
            return new ListTag(tagName, tags, listType);
        }

        private static CompoundTag DictionaryToTag(string tagName, IDictionary dict, SerializationContext ctx)
        {
            CompoundTag tag = new(tagName);
            foreach (DictionaryEntry kvp in dict)
            {
                if (kvp.Key is string str)
                    tag.Add(Serialize(kvp.Value, str, ctx));
                else
                    throw new InvalidCastException("Dictionary keys must be strings!");
            }
            return tag;
        }

        private static Tag SerializeObject(object? value, string tagName, SerializationContext ctx)
        {
            if (value == null) return new NullTag(tagName); // ID defaults to 0 which is null or an Empty Compound

            var type = value.GetType();

            var nameAttribute = Attribute.GetCustomAttribute(type, typeof(SerializeAsAttribute)) as SerializeAsAttribute;
            var compound = new CompoundTag(nameAttribute?.Name ?? tagName);

            if (ctx.objectToId.TryGetValue(value, out int id))
            {
                compound.SerializedID = id;
                // Dont need to write compound data, its already been serialized at some point earlier
                return compound;
            }

            id = ctx.nextId++;
            ctx.objectToId[value] = id;
            ctx.idToObject[id] = value;

            if (value is ISerializeCallbacks callback)
                callback.PreSerialize();

            if (value is ISerializable serializable)
            {
                // Manual Serialization
                compound = serializable.Serialize(tagName, ctx);
            }
            else
            {
                // Automatic Serializer
                var properties = GetAllFields(type).Where(field => (field.IsPublic || field.GetCustomAttribute<SerializeAsAttribute>() != null) &&
                                                       field.GetCustomAttribute<SerializeIgnoreAttribute>() == null);

                foreach (var field in properties)
                {
                    var nameAs = field.GetCustomAttribute<SerializeAsAttribute>();
                    string name = nameAs != null ? nameAs.Name : field.Name;

                    var propValue = field.GetValue(value);
                    if (propValue == null)
                    {
                        if (Attribute.GetCustomAttribute(field, typeof(IgnoreOnNullAttribute)) != null) continue;
                        compound.Add(new NullTag(name));
                    }
                    else
                    {
                        Tag tag = Serialize(propValue, name, ctx);
                        if (string.IsNullOrEmpty(tag.Name)) throw new NullReferenceException("Data Tag Name is missing! Cannot finish Serialization!");
                        compound.Add(tag);
                    }
                }
            }

            compound.SerializedID = id;
            compound.SerializedType = type.AssemblyQualifiedName;

            if (value is ISerializeCallbacks callback2)
                callback2.PostSerialize();

            return compound;
        }

        #endregion

        #region Deserialize

        public static T? Deserialize<T>(Tag value) => (T?)Deserialize(value, typeof(T));

        public static object? Deserialize(Tag value, Type type) => Deserialize(value, type, new SerializationContext());

        public static T? Deserialize<T>(Tag value, SerializationContext ctx) => (T?)Deserialize(value, typeof(T), ctx);
        public static object? Deserialize(Tag value, Type targetType, SerializationContext ctx)
        {
            if (value is NullTag) return null;

            if (IsPrimitive(targetType))
            {
                if (value is ByteTag b) return b.Value;
                else if (value is DoubleTag dou) return dou.Value;
                else if (value is FloatTag flo) return flo.Value;
                else if (value is IntTag i)
                {
                    if (targetType.IsEnum)
                        return Enum.ToObject(targetType, i.Value);
                    return i.Value;
                }
                else if (value is LongTag lo)
                {
                    if (targetType == typeof(DateTime))
                        return DateTime.FromBinary(lo.Value);
                    return lo.Value;
                }
                else if (value is ShortTag sh) return sh.Value;
                else if (value is StringTag str)
                {
                    if (targetType == typeof(Guid))
                        return Guid.Parse(str.Value);
                    return str.Value;
                }
                else if (value is ByteArrayTag barr) return barr.Value;
                else throw new NotSupportedException("The Tag type '" + value.GetType() + "' is not supported.");
            }

            if (value is ListTag list)
            {
                Type type;
                if (list.ListType == TagType.Byte) type = typeof(byte);
                else if (list.ListType == TagType.Double) type = typeof(double);
                else if (list.ListType == TagType.Float) type = typeof(float);
                else if (list.ListType == TagType.Int) type = typeof(int);
                else if (list.ListType == TagType.Long) type = typeof(long);
                else if (list.ListType == TagType.Short) type = typeof(short);
                else if (list.ListType == TagType.String) type = typeof(string);
                else if (list.ListType == TagType.ByteArray) type = typeof(byte[]);
                else if (list.ListType == TagType.Compound)
                {
                    if (targetType.IsArray) type = targetType.GetElementType();
                    else type = typeof(object); // !!!! The Compounds store what type they are! We should use that to deserialize them!
                }
                else throw new NotSupportedException("The Tag List type '" + list.ListType + "' is not supported.");

                var array = Array.CreateInstance(type, list.Count);
                for (int idx = 0; idx < array.Length; idx++)
                    array.SetValue(Deserialize(list[idx], type, ctx), idx);
                return array;
            }

            if (value is CompoundTag compound)
            {
                if (targetType.IsAssignableTo(typeof(IDictionary)))
                {
                    // tag is dictionary
                    var dict = (IDictionary)Activator.CreateInstance(targetType);
                    var valueType = targetType.GetGenericArguments()[1];
                    foreach (var tag in compound.AllTags)
                    {
                        var key = tag.Name;
                        var val = Deserialize(tag, valueType, ctx);
                        dict.Add(key, val);
                    }
                    return dict;
                }

                return DeserializeObject(compound, ctx);
            }

            throw new NotSupportedException("The node type '" + value.GetType() + "' is not supported.");
        }

        private static object? DeserializeObject(CompoundTag compound, SerializationContext ctx)
        {
            if (ctx.idToObject.TryGetValue(compound.SerializedID, out object? existingObj))
                return existingObj;

            if (string.IsNullOrWhiteSpace(compound.SerializedType))
                return null;

            Type oType = Type.GetType(compound.SerializedType);
            if (oType == null)
                throw new Exception("Cannot deserialize type '" + compound.SerializedType + "', It does not exist.");

            object resultObject = CreateInstance(oType);

            ctx.idToObject[compound.SerializedID] = resultObject;

            if (resultObject is ISerializable serializable)
            {
                serializable.Deserialize(compound, ctx);
                resultObject = serializable;
            }
            else
            {
                FieldInfo[] fields = GetAllFields(oType).ToArray();

                var properties = fields.Where(field => (field.IsPublic || field.GetCustomAttribute<SerializeAsAttribute>() != null) && field.GetCustomAttribute<SerializeIgnoreAttribute>() == null);
                foreach (var field in properties)
                {
                    string name = field.Name;

                    var nameAttributes = Attribute.GetCustomAttributes(field, typeof(SerializeAsAttribute));
                    if (nameAttributes.Length != 0)
                        name = ((SerializeAsAttribute)nameAttributes[0]).Name;

                    if (!compound.TryGet<Tag>(name, out var node))
                    {
                        // Before we completely give up, a field can have FormerlySerializedAs Attributes
                        // This allows backwards compatibility
                        var formerNames = Attribute.GetCustomAttributes(field, typeof(FormerlySerializedAsAttribute));
                        foreach (SerializeAsAttribute formerName in formerNames)
                        {
                            //if (compound.Tags.Any(a => a.Name == formerName.Name))
                            if (compound.TryGet<Tag>(formerName.Name, out node))
                            {
                                name = formerName.Name;
                                break;
                            }
                        }
                        if (node == null) // Continue onto the next field
                            continue;
                    }

                    object data = Deserialize(node, field.FieldType, ctx);

                    // Some manual casting for edge cases
                    if (data is byte @byte)
                    {
                        if (field.FieldType == typeof(bool))
                            data = @byte != 0;
                        if (field.FieldType == typeof(sbyte))
                            data = (sbyte)@byte;
                    }

                    field.SetValue(resultObject, data);
                }
            }

            return resultObject;
        }


        static object CreateInstance(Type type)
        {
            object data = Activator.CreateInstance(type);

            if (data is ISerializeCallbacks callback2)
                callback2.PostCreation();

            return data;
        }

        static IEnumerable<FieldInfo> GetAllFields(Type t)
        {
            if (t == null)
                return Enumerable.Empty<FieldInfo>();

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                 BindingFlags.Instance | BindingFlags.DeclaredOnly;

            return t.GetFields(flags).Concat(GetAllFields(t.BaseType));
        }

        #endregion
    }
}
