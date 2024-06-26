using System;
using System.Collections.Generic;
using System.Reflection;
using OatmealDome.BinaryData.Core;

namespace OatmealDome.BinaryData
{
    /// <summary>
    /// Represents reflected type configuration required for reading and writing it as binary data.
    /// </summary>
    internal class TypeData
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private static readonly Dictionary<Type, TypeData> _cache = new Dictionary<Type, TypeData>();
        
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        private TypeData(Type type)
        {
            Type = type;

            // Get the type configuration.
            Attribute = Type.GetCustomAttribute<BinaryObjectAttribute>() ?? new BinaryObjectAttribute();

            // Get the member configurations, and collect a parameterless constructor on the way.
            OrderedMembers = new SortedDictionary<int, MemberData>();
            UnorderedMembers = new SortedList<string, MemberData>(StringComparer.Ordinal);
            foreach (MemberInfo member in Type.GetMembers(
                BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                switch (member)
                {
                    case ConstructorInfo constructorInfo:
                        if (constructorInfo.GetParameters().Length == 0)
                        {
                            Constructor = constructorInfo;
                        }
                        break;
                    case FieldInfo field:
                        ValidateFieldInfo(field);
                        break;
                    case PropertyInfo property:
                        ValidatePropertyInfo(property);
                        break;
                }
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the <see cref="Type"/> to which informations are stored.
        /// </summary>
        internal Type Type { get; }
        
        /// <summary>
        /// Gets the <see cref="BinaryObjectAttribute"/> configuring how the object is read and written.
        /// </summary>
        internal BinaryObjectAttribute Attribute { get; }

        /// <summary>
        /// Gets a parameterless <see cref="ConstructorInfo"/> to instantiate the class.
        /// </summary>
        internal ConstructorInfo Constructor { get; }

        /// <summary>
        /// Gets the dictionary of <see cref="MemberData"/> for members with the
        /// <see cref="BinaryMemberAttribute.Order"/> property set.
        /// </summary>
        internal SortedDictionary<int, MemberData> OrderedMembers { get; }

        /// <summary>
        /// Gets the list of <see cref="MemberData"/> for members missing the <see cref="BinaryMemberAttribute.Order"/>
        /// property.
        /// </summary>
        internal SortedList<string, MemberData> UnorderedMembers { get; }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------
        
        /// <summary>
        /// Gets the <see cref="TypeData"/> instance for the given <paramref name="type"/> and caches the information on
        /// it if necessary.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to query information about.</param>
        /// <returns>The <see cref="TypeData"/> instance holding information about the type.</returns>
        internal static TypeData GetTypeData(Type type)
        {
            if (!_cache.TryGetValue(type, out TypeData typeData))
            {
                typeData = new TypeData(type);
                _cache.Add(type, typeData);
            }
            return typeData;
        }

        /// <summary>
        /// Invokes the parameterless constructor on the object.
        /// </summary>
        /// <returns>A new instance of the object.</returns>
        internal object GetInstance()
        {
            // Invoke the automatic default constructor for structs.
            if (Type.IsValueType)
            {
                return Activator.CreateInstance(Type);
            }

            // Invoke an explicit parameterless constructor for classes.
            if (Constructor == null)
            {
                throw new MissingMethodException($"No parameterless constructor found for {Type}.");
            }
            return Constructor.Invoke(null);
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private void ValidateFieldInfo(FieldInfo field)
        {
            // Get a possible binary member configuration or use the default one.
            BinaryMemberAttribute attrib = field.GetCustomAttribute<BinaryMemberAttribute>();
            bool hasAttrib = attrib != null;
            attrib = attrib ?? new BinaryMemberAttribute();

            // Field must be decorated or public.
            if (hasAttrib || (!Attribute.Explicit && field.IsPublic))
            {
                // For fields of enumerable type ElementCount must be specified.
                if (field.FieldType.IsEnumerable() && attrib.Length <= 0)
                {
                    throw new InvalidOperationException(
                        $"Field {field} requires an element count specified with a {nameof(BinaryMemberAttribute)}.");
                }

                // Store member in a deterministic order.
                MemberData memberData = new MemberData(field, field.FieldType, attrib);
                if (attrib.Order == Int32.MinValue)
                {
                    UnorderedMembers.Add(field.Name, memberData);
                }
                else
                {
                    OrderedMembers.Add(attrib.Order, memberData);
                }
            }
        }

        private void ValidatePropertyInfo(PropertyInfo prop)
        {
            // Get a possible binary member configuration or use the default one.
            BinaryMemberAttribute attrib = prop.GetCustomAttribute<BinaryMemberAttribute>();
            bool hasAttrib = attrib != null;
            attrib = attrib ?? new BinaryMemberAttribute();

            // Property must have getter and setter - if not, throw an exception if it is explicitly decorated.
            if (hasAttrib && (prop.GetMethod == null || prop.SetMethod == null))
            {
                throw new InvalidOperationException($"Getter and setter on property {prop} not found.");
            }
            // Property must be decorated or getter and setter public.
            if (hasAttrib
                || (!Attribute.Explicit && prop.GetMethod?.IsPublic == true && prop.SetMethod?.IsPublic == true))
            {
                // For properties of enumerable type ElementCount must be specified.
                if (prop.PropertyType.IsEnumerable() && attrib.Length <= 0)
                {
                    throw new InvalidOperationException(
                        $"Property {prop} requires an element count specified with a {nameof(BinaryMemberAttribute)}.");
                }

                // Store member in a deterministic order.
                MemberData memberData = new MemberData(prop, prop.PropertyType, attrib);
                if (attrib.Order == Int32.MinValue)
                {
                    UnorderedMembers.Add(prop.Name, memberData);
                }
                else
                {
                    OrderedMembers.Add(attrib.Order, memberData);
                }
            }
        }
    }
}
