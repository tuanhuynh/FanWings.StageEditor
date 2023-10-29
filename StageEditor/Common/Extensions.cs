using System;
using System.Globalization;
using System.Windows;
using System.Xml;

namespace StageEditor {

    public static class XmlExtension {

        public static double ParseToDouble(this string value, double defaultValue = 0) {
            if (!string.IsNullOrEmpty(value)) {
                try {
                    return Convert.ToDouble(value, new CultureInfo("en-US"));
                } catch (Exception) {

                }    
            }
            return defaultValue;
        }
        
        public static string ToFixedString(this double value) {
            return value.ToString(new CultureInfo("en-US")).Replace(",", ".");
        }

        public static void AddAttribute(this XmlNode node, XmlDocument xml, string attribute, string value) {
            XmlAttribute attr = xml.CreateAttribute(attribute);
            attr.Value = value;
            node.Attributes.Append(attr);
        }

        public static void AddAttribute(this XmlNode node, XmlDocument xml, string attribute, object value) {
            XmlAttribute attr = xml.CreateAttribute(attribute);
            attr.Value = value != null ? value.ToString() : "";
            node.Attributes.Append(attr);
        }

        public static XmlNode AddChild(this XmlNode parent, XmlDocument xml, string nodeName) {
            XmlNode node = xml.CreateElement(nodeName);
            parent.AppendChild(node);
            return node;
        }

        private static bool HasValue(this XmlNode node, string attribute) {
            return node != null && node.Attributes[attribute] != null
                && !string.IsNullOrEmpty(node.Attributes[attribute].Value);
        }

        public static int ReadInt(this XmlNode node, string attribute, int defaultValue = 0) {
            if (node.HasValue(attribute)) {
                int result;
                if (int.TryParse(node.Attributes[attribute].Value, out result)) {
                    return result;
                }
            }
            return defaultValue;
        }

        public static string ReadString(this XmlNode node, string attribute, string defaultValue = "") {
            if (node.HasValue(attribute)) {
                return node.Attributes[attribute].Value.Trim();
            }
            return defaultValue;
        }
        
        public static double ReadDouble(this XmlNode node, string attribute, double defaultValue = 0) {
            if (node.HasValue(attribute)) {
                return node.Attributes[attribute].Value.ParseToDouble(defaultValue);
            }
            return defaultValue;
        }

        public static double[] ReadDoubleArray(this XmlNode node, string attribute, char seperator = ',') {
            if (node.HasValue(attribute)) {
                string[] values = node.Attributes[attribute].Value.Split(seperator);
                double[] result = new double[values.Length];
                for (int i = 0; i < values.Length; i++) {
                    result[i] = double.Parse(values[i]);
                }
                return result;
            }
            return null;
        }
        public static double Get(this double[] values, int index, double defaultValue = 0) {
            return (values != null && values.Length > index && index >= 0) ? values[index] : defaultValue;
        }



        public static Point ReadVector(this XmlNode node, string attribute, char seperator = ',') {
            if (node.HasValue(attribute)
                && node.Attributes[attribute].Value.Contains(seperator.ToString())) {
                string[] strs = node.Attributes[attribute].Value.Split(seperator);
                if (strs!=null && strs.Length >= 2) {
                    return new Point(strs[0].ParseToDouble(), strs[1].ParseToDouble());
                }
            }
            return new Point(0, 0);
        }

        public static T ReadEnum<T>(this XmlNode node, string attribute, T defaultValue = default(T)) {
            if (node.HasValue(attribute)) {
                return (T)Enum.Parse(typeof(T), node.Attributes[attribute].Value);
            }
            return defaultValue;
        }
        
    }

}
